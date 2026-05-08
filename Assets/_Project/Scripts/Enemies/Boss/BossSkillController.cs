using UnityEngine;
using System.Collections;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Điều phối toàn bộ ability của boss.
    /// Controller này là nơi nhận tín hiệu gameplay từ BossBase rồi phân phối
    /// cho từng BossAbility:
    /// - boss spawn
    /// - boss nhận damage
    /// - boss vượt mốc HP
    /// - boss bắt đầu/kết thúc cast
    /// </summary>
    public class BossSkillController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BossBase owner;
        [SerializeField] private BossData bossData;

        [Header("Abilities")]
        [Tooltip("Danh sách ability gắn trên boss. Có thể auto-cache bằng GetComponents.")]
        [SerializeField] private BossAbility[] abilities;

        [Header("Runtime")]
        [Tooltip("Skill đang cast. Chỉ dùng cho skill chủ động cần lock movement.")]
        [SerializeField] private BossAbility activeCastingAbility;

        [SerializeField] private bool hasInitialized;

        private Coroutine activeCastRoutine;

        public BossBase Owner => owner;
        public BossData BossData => bossData;
        public BossAbility[] Abilities => abilities;
        public BossAbility ActiveCastingAbility => activeCastingAbility;
        public bool HasInitialized => hasInitialized;
        public bool HasRunningCast => activeCastingAbility != null;

        /// <summary>
        /// Khởi tạo controller sau khi boss được spawn.
        /// Trách nhiệm:
        /// - cache owner/data
        /// - thu thập danh sách ability
        /// - truyền context chung cho từng ability
        /// </summary>
        public virtual void Initialize(BossBase boss, BossData data)
        {
            InterruptAll();

            owner = boss;
            bossData = data;

            CacheAbilities();
            InitializeAbilities();

            hasInitialized = true;
        }

        /// <summary>
        /// Auto-cache toàn bộ BossAbility trên boss.
        /// </summary>
        public virtual void CacheAbilities()
        {
            abilities = GetComponents<BossAbility>();
        }

        /// <summary>
        /// Truyền reference chung cho từng ability.
        /// </summary>
        public virtual void InitializeAbilities()
        {
            if (abilities == null)
            {
                return;
            }

            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] == null) continue;
                abilities[i].Initialize(owner, this);
            }
        }

        /// <summary>
        /// Gọi hook OnBossSpawned của từng ability.
        /// Dùng cho intro skill, phase mở đầu hoặc audio/UI warning.
        /// </summary>
        public virtual void NotifyBossSpawned()
        {
            if (abilities == null)
            {
                return;
            }

            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] == null) continue;
                abilities[i].OnBossSpawned();
            }
        }

        /// <summary>
        /// Broadcast thông tin hit vừa nhận cho toàn bộ reactive ability.
        /// </summary>
        public virtual void NotifyBossDamaged(float incomingDamage, float appliedDamage, float previousHp, float currentHp)
        {
            if (abilities == null)
            {
                return;
            }

            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] == null) continue;
                abilities[i].OnBossDamaged(incomingDamage, appliedDamage, previousHp, currentHp);
            }
        }

        /// <summary>
        /// Cho controller thông báo các skill khi boss vừa đi qua một mốc HP.
        /// </summary>
        public virtual void NotifyHealthThresholdCrossed(float thresholdNormalized, float previousHp, float currentHp)
        {
            if (abilities == null)
            {
                return;
            }

            for (int i = 0; i < abilities.Length; i++)
            {
                if (abilities[i] == null) continue;
                abilities[i].OnHealthThresholdCrossed(thresholdNormalized, previousHp, currentHp);
            }
        }

        /// <summary>
        /// Kiểm tra một ability có thể bắt đầu cast hay không.
        /// Bản khung hiện tại chỉ chặn khi đang có skill khác cast.
        /// </summary>
        public virtual bool CanStartAbility(BossAbility ability)
        {
            if (ability == null)
            {
                return false;
            }

            if (!hasInitialized)
            {
                return false;
            }

            if (HasRunningCast)
            {
                return false;
            }

            return ability.CanTrigger();
        }

        /// <summary>
        /// Bắt đầu cast hoặc execute ngay tùy ability.
        /// </summary>
        public virtual bool TryActivateAbility(BossAbility ability)
        {
            if (!CanStartAbility(ability))
            {
                return false;
            }

            if (ability.RequiresCasting)
            {
                activeCastingAbility = ability;
                float castDuration = bossData != null
                    ? bossData.GetCastDuration(ability)
                    : ability.CastDuration;

                owner?.BeginCast(ability, castDuration);
                activeCastRoutine = StartCoroutine(CompleteCastAfterDelay(castDuration));
            }
            else
            {
                ability.Execute();
            }

            return true;
        }

        /// <summary>
        /// Kết thúc cast cho skill hiện tại và thực thi effect chính.
        /// Khi làm logic thật, method này sẽ được gọi sau cast timer/animation event.
        /// </summary>
        public virtual void CompleteActiveCast()
        {
            if (activeCastingAbility == null)
            {
                return;
            }

            if (activeCastRoutine != null)
            {
                StopCoroutine(activeCastRoutine);
                activeCastRoutine = null;
            }

            BossAbility ability = activeCastingAbility;
            activeCastingAbility = null;

            ability.Execute();
            owner?.EndCast(ability);
        }

        /// <summary>
        /// Hủy mọi cast đang diễn ra. Dùng khi boss chết, despawn hoặc scene đổi.
        /// </summary>
        public virtual void InterruptAll()
        {
            if (activeCastRoutine != null)
            {
                StopCoroutine(activeCastRoutine);
                activeCastRoutine = null;
            }

            if (activeCastingAbility != null)
            {
                owner?.EndCast(activeCastingAbility);
                activeCastingAbility = null;
            }
        }

        private IEnumerator CompleteCastAfterDelay(float castDuration)
        {
            if (castDuration > 0f)
            {
                yield return new WaitForSeconds(castDuration);
            }
            else
            {
                yield return null;
            }

            activeCastRoutine = null;
            CompleteActiveCast();
        }
    }
}
