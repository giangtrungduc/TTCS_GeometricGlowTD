using UnityEngine;

namespace TowerDefense.StatusEffects
{
    /// <summary>
    /// Tăng tốc enemy theo hệ số nhân.
    /// Stackable: nhiều nguồn có thể buff cùng lúc.
    /// PathFollower chỉ lấy buff mạnh nhất (multiplier lớn nhất) để áp dụng.
    /// </summary>
    public class SpeedBuffEffect : StatusEffect
    {
        // ===========================
        // CONFIGURATION
        // ===========================

        private readonly float speedMultiplier;

        public override string EffectID => "SpeedBuff";
        public override bool Stackable => true;
        public override bool IsDebuff => false;

        // ===========================
        // CONSTRUCTOR
        // ===========================

        /// <param name="speedMultiplier">Hệ số tăng tốc. Phải > 1f. VD: 1.5f = nhanh hơn 50%.</param>
        /// <param name="duration">Thời gian hiệu lực (giây). -1f = vĩnh viễn.</param>
        public SpeedBuffEffect(float speedMultiplier, float duration) : base(duration)
        {
            // Clamp: multiplier phải >= 1f, tránh buff lại biến thành debuff
            this.speedMultiplier = Mathf.Max(1f, speedMultiplier);
        }

        // ===========================
        // LIFECYCLE
        // ===========================

        protected override void OnApply(StatusEffectHandler handler)
        {
            handler.PathFollower?.AddSpeedModifier(EffectID, UniqueId, speedMultiplier);
        }

        protected override void OnTick(StatusEffectHandler handler, float deltaTime)
        {
            // Passive — PathFollower tự giữ trạng thái, không cần update mỗi frame
        }

        protected override void OnRemove(StatusEffectHandler handler)
        {
            handler.PathFollower?.RemoveSpeedModifier(EffectID, UniqueId);
        }
    }
}