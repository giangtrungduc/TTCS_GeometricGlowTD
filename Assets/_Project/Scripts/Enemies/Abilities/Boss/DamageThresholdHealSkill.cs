using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Heal lại một phần sát thương nếu boss nhận phải một hit quá lớn.
    /// </summary>
    public class DamageThresholdHealSkill : BossAbility
    {
        [Header("Damage Threshold Heal")]
        [Tooltip("Ngưỡng damage kích hoạt tính theo % Max HP. 0.15 = hit lớn hơn 15% MaxHP.")]
        [SerializeField, Range(0f, 1f)] private float triggerPercentOfMaxHp = 0.15f;

        [Tooltip("Tỉ lệ damage nhận vào sẽ được hồi lại. 0.5 = hồi 50% applied damage.")]
        [SerializeField, Range(0f, 1f)] private float healPercentOfAppliedDamage = 0.5f;

        [Tooltip("Cooldown nội bộ giữa các lần proc.")]
        [SerializeField, Min(0f)] private float internalCooldown = 0f;

        private float lastTriggerTime = float.MinValue;
        private float pendingHealAmount;

        public override void OnBossDamaged(float incomingDamage, float appliedDamage, float previousHp, float currentHp)
        {
            if (owner == null || owner.MaxHp <= 0f)
            {
                return;
            }

            float damageThreshold = owner.MaxHp * triggerPercentOfMaxHp;
            if (appliedDamage < damageThreshold || !CanTrigger())
            {
                return;
            }

            pendingHealAmount = Mathf.Max(0f, appliedDamage * healPercentOfAppliedDamage);
            controller?.TryActivateAbility(this);
        }

        public override bool CanTrigger()
        {
            if (!base.CanTrigger())
            {
                return false;
            }

            return Time.time >= lastTriggerTime + internalCooldown;
        }

        public override void Execute()
        {
            if (owner == null || pendingHealAmount <= 0f)
            {
                return;
            }

            owner.Heal(pendingHealAmount);

            pendingHealAmount = 0f;
            lastTriggerTime = Time.time;
            MarkTriggered();
        }

        public override void ResetRuntimeState()
        {
            base.ResetRuntimeState();
            pendingHealAmount = 0f;
            lastTriggerTime = float.MinValue;
        }
    }
}
