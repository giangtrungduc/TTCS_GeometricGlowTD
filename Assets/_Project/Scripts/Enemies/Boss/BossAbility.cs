using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Base class cho mọi skill của boss.
    /// Mỗi ability chỉ nên chịu trách nhiệm một cơ chế cụ thể:
    /// heal theo ngưỡng hit, summon theo mốc HP, enrage ở 50%, ...
    /// Không nên tự quản toàn bộ state boss.
    /// </summary>
    public abstract class BossAbility : MonoBehaviour
    {
        [Header("Ability Identity")]
        [SerializeField] protected string abilityId = "BossAbility";
        [SerializeField] protected string displayName = "Boss Ability";

        [Header("Casting")]
        [Tooltip("Nếu true, skill cần boss đứng yên để thi triển.")]
        [SerializeField] protected bool requiresCasting;

        [Tooltip("Thời gian đứng cast trước khi Execute.")]
        [SerializeField, Min(0f)] protected float castDuration;

        [Header("Behavior")]
        [Tooltip("Nếu true, skill chỉ được trigger đúng một lần mỗi trận.")]
        [SerializeField] protected bool triggerOnlyOnce;

        protected BossBase owner;
        protected BossSkillController controller;
        protected bool hasTriggered;

        public string AbilityId => abilityId;
        public string DisplayName => displayName;
        public bool RequiresCasting => requiresCasting;
        public float CastDuration => castDuration;
        public bool TriggerOnlyOnce => triggerOnlyOnce;
        public bool HasTriggered => hasTriggered;
        public BossBase Owner => owner;
        public BossSkillController Controller => controller;

        /// <summary>
        /// Inject context chung từ controller.
        /// </summary>
        public virtual void Initialize(BossBase boss, BossSkillController skillController)
        {
            owner = boss;
            controller = skillController;
            hasTriggered = false;
        }

        /// <summary>
        /// Hook khi boss vừa spawn.
        /// Skill có thể dùng để reset state hoặc tự trigger intro.
        /// </summary>
        public virtual void OnBossSpawned() { }

        /// <summary>
        /// Hook khi boss vừa nhận damage.
        /// incomingDamage: damage thô trước xử lý.
        /// appliedDamage: damage thực tế đã làm giảm HP.
        /// </summary>
        public virtual void OnBossDamaged(float incomingDamage, float appliedDamage, float previousHp, float currentHp) { }

        /// <summary>
        /// Hook khi boss đi qua một mốc HP chuẩn hóa, ví dụ 0.8 / 0.6 / 0.5.
        /// </summary>
        public virtual void OnHealthThresholdCrossed(float thresholdNormalized, float previousHp, float currentHp) { }

        /// <summary>
        /// Ability có được phép trigger ở thời điểm hiện tại hay không.
        /// </summary>
        public virtual bool CanTrigger()
        {
            if (triggerOnlyOnce && hasTriggered)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Effect chính của skill.
        /// Với skill cần cast, method này được gọi sau khi cast hoàn tất.
        /// </summary>
        public abstract void Execute();

        /// <summary>
        /// Đánh dấu skill đã được dùng.
        /// </summary>
        protected virtual void MarkTriggered()
        {
            hasTriggered = true;
        }

        /// <summary>
        /// Reset state nội bộ của skill khi boss respawn hoặc khi cần test.
        /// </summary>
        public virtual void ResetRuntimeState()
        {
            hasTriggered = false;
        }
    }
}
