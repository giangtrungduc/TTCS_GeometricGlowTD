using UnityEngine;
using TowerDefense.StatusEffects;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Trigger enrage khi boss tụt xuống dưới một ngưỡng HP xác định.
    /// </summary>
    public class EnrageAtHalfHpSkill : BossAbility
    {
        [Header("Enrage")]
        [Tooltip("Mốc HP chuẩn hóa để trigger. 0.5 = 50%.")]
        [SerializeField, Range(0f, 1f)] private float enrageThreshold = 0.5f;

        [Tooltip("Hệ số tăng tốc áp lên boss sau khi enrage.")]
        [SerializeField, Min(1f)] private float speedMultiplier = 1.5f;

        [Tooltip("Nếu true, boss sẽ miễn nhiễm debuff sau khi enrage.")]
        [SerializeField] private bool grantCrowdControlImmunity = true;

        private bool isEnraged;

        public override void Initialize(BossBase boss, BossSkillController skillController)
        {
            base.Initialize(boss, skillController);
            isEnraged = false;
        }

        public override void OnBossDamaged(float incomingDamage, float appliedDamage, float previousHp, float currentHp)
        {
            if (owner == null || owner.MaxHp <= 0f || isEnraged)
            {
                return;
            }

            float previousNormalized = previousHp / owner.MaxHp;
            float currentNormalized = currentHp / owner.MaxHp;
            bool crossedThreshold = previousNormalized > enrageThreshold && currentNormalized <= enrageThreshold;

            if (crossedThreshold)
            {
                controller?.TryActivateAbility(this);
            }
        }

        public override void Execute()
        {
            if (owner == null || isEnraged)
            {
                return;
            }

            owner.StatusHandler?.AddEffect(new SpeedBuffEffect(speedMultiplier, -1f));

            if (grantCrowdControlImmunity)
            {
                owner.SetCrowdControlImmune(true);
            }

            isEnraged = true;
            MarkTriggered();
        }

        public override bool CanTrigger()
        {
            if (!base.CanTrigger())
            {
                return false;
            }

            return !isEnraged;
        }

        public override void ResetRuntimeState()
        {
            base.ResetRuntimeState();
            isEnraged = false;
        }
    }
}
