using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Enemy đặc biệt có thêm state cast skill, phase và miễn khống.
    /// Boss vẫn kế thừa toàn bộ lifecycle của EnemyBase:
    /// Initialize -> di chuyển -> nhận damage/heal -> chết.
    /// Lớp này chỉ chịu trách nhiệm điều phối trạng thái riêng của boss
    /// và chuyển các tín hiệu gameplay sang BossSkillController.
    /// </summary>
    public class BossBase : EnemyBase
    {
        public enum BossState
        {
            Moving,
            Casting,
            Dead
        }

        [Header("Boss Config")]
        [Tooltip("Dữ liệu cấu hình riêng cho boss: cast time, phase, audio, tuning skill.")]
        [SerializeField] private BossData bossData;

        [Tooltip("Controller điều phối toàn bộ skill của boss.")]
        [SerializeField] private BossSkillController skillController;

        [Header("Boss Runtime Flags")]
        [Tooltip("Boss có đang miễn khống hay không.")]
        [SerializeField] private bool isCrowdControlImmune;

        [Tooltip("Nếu true, boss sẽ đứng yên trong lúc cast skill chủ động.")]
        [SerializeField] private bool stopMovementWhileCasting = true;

        private BossState currentState = BossState.Moving;

        public BossData BossData => bossData;
        public BossSkillController SkillController => skillController;
        public BossState CurrentState => currentState;
        public bool IsCasting => currentState == BossState.Casting;
        public bool IsCrowdControlImmune => isCrowdControlImmune;
        public bool StopMovementWhileCasting => stopMovementWhileCasting;

        protected override void OnEnemyAwake()
        {
            base.OnEnemyAwake();

            if (skillController == null)
            {
                skillController = GetComponent<BossSkillController>();
            }
        }

        protected override void OnSpawned()
        {
            base.OnSpawned();

            currentState = BossState.Moving;
            isCrowdControlImmune = false;
            stopMovementWhileCasting = bossData != null
                ? bossData.stopMovementWhileCasting
                : stopMovementWhileCasting;

            if (pathFollower != null)
            {
                pathFollower.enabled = true;
            }

            skillController?.Initialize(this, bossData);
            skillController?.NotifyBossSpawned();
        }

        protected override void OnDamaged(float incomingDamage, float appliedDamage, float previousHp, float currentHp)
        {
            base.OnDamaged(incomingDamage, appliedDamage, previousHp, currentHp);
            NotifyIncomingDamage(incomingDamage, appliedDamage, previousHp, currentHp);
            NotifyThresholdsCrossed(previousHp, currentHp);
        }

        /// <summary>
        /// Hook dự kiến cho pipeline damage của boss.
        /// Khi triển khai thật, đây là nơi phù hợp để:
        /// - lấy previous HP
        /// - cho controller biết hit vừa nhận
        /// - check mốc HP / phase
        /// </summary>
        public virtual void NotifyIncomingDamage(float incomingDamage, float appliedDamage, float previousHp, float currentHp)
        {
            skillController?.NotifyBossDamaged(incomingDamage, appliedDamage, previousHp, currentHp);
        }

        /// <summary>
        /// Bắt đầu cast một skill chủ động.
        /// Trách nhiệm:
        /// - đổi state sang Casting
        /// - khóa di chuyển nếu boss cấu hình đứng yên lúc cast
        /// - dành chỗ cho animation / VFX / audio cast
        /// </summary>
        public virtual void BeginCast(BossAbility source, float castDuration)
        {
            currentState = BossState.Casting;

            if (stopMovementWhileCasting && pathFollower != null)
            {
                pathFollower.enabled = false;
            }
        }

        /// <summary>
        /// Kết thúc cast và trả boss về trạng thái di chuyển bình thường.
        /// </summary>
        public virtual void EndCast(BossAbility source)
        {
            if (currentState == BossState.Dead)
            {
                return;
            }

            currentState = BossState.Moving;

            if (pathFollower != null)
            {
                pathFollower.enabled = true;
            }
        }

        /// <summary>
        /// Bật hoặc tắt cờ miễn khống.
        /// Khi tích hợp thật, StatusEffectHandler nên hỏi cờ này trước khi nhận debuff.
        /// </summary>
        public virtual void SetCrowdControlImmune(bool value)
        {
            if (value && bossData != null && !bossData.supportsCrowdControlImmunity)
            {
                return;
            }

            isCrowdControlImmune = value;

            if (value)
            {
                statusHandler?.ClearAllDebuffs();
            }
        }

        /// <summary>
        /// Điểm mở rộng cho việc từ chối debuff khi boss đang ở phase miễn khống.
        /// </summary>
        public virtual bool CanReceiveDebuff(string effectId)
        {
            return !isCrowdControlImmune;
        }

        protected override void OnDeath()
        {
            currentState = BossState.Dead;
            skillController?.InterruptAll();
            if (pathFollower != null)
            {
                pathFollower.enabled = false;
            }

            base.OnDeath();
        }

        private void NotifyThresholdsCrossed(float previousHp, float currentHp)
        {
            if (skillController == null || bossData == null || MaxHp <= 0f)
            {
                return;
            }

            float previousNormalized = previousHp / MaxHp;
            float currentNormalized = currentHp / MaxHp;
            float[] thresholds = bossData.healthThresholds;

            if (thresholds == null || thresholds.Length == 0)
            {
                return;
            }

            for (int i = 0; i < thresholds.Length; i++)
            {
                float threshold = Mathf.Clamp01(thresholds[i]);
                bool crossed = previousNormalized > threshold && currentNormalized <= threshold;
                if (crossed)
                {
                    skillController.NotifyHealthThresholdCrossed(threshold, previousHp, currentHp);
                }
            }
        }
    }
}
