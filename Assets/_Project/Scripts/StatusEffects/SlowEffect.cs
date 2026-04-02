using UnityEngine;

namespace TowerDefense.StatusEffects
{
    /// <summary>
    /// Làm chậm enemy theo tỉ lệ phần trăm.
    /// Stackable: nhiều nguồn (tower khác nhau) có thể áp cùng lúc.
    /// PathFollower chỉ lấy slow mạnh nhất (multiplier nhỏ nhất) để áp dụng.
    /// </summary>
    public class SlowEffect : StatusEffect
    {
        // ===========================
        // CONFIGURATION
        // ===========================

        private readonly float slowMultiplier; // Hệ số tốc độ sau khi slow, VD: slow 40% → multiplier = 0.6f

        public override string EffectID => "Slow";
        public override bool Stackable => true;
        public override bool IsDebuff => true;

        // ===========================
        // CONSTRUCTOR
        // ===========================

        /// <param name="slowPercent">Tỉ lệ làm chậm [0, 1]. VD: 0.4f = làm chậm 40%.</param>
        /// <param name="duration">Thời gian hiệu lực (giây). -1f = vĩnh viễn.</param>
        public SlowEffect(float slowPercent, float duration) : base(duration)
        {
            slowMultiplier = 1f - Mathf.Clamp01(slowPercent);
        }

        // ===========================
        // LIFECYCLE
        // ===========================

        protected override void OnApply(StatusEffectHandler handler)
        {
            handler.PathFollower?.AddSpeedModifier(EffectID, UniqueId, slowMultiplier);
        }

        protected override void OnTick(StatusEffectHandler handler, float deltaTime)
        {
            // Passive — PathFollower tự giữ trạng thái, không cần update mỗi frame
        }

        protected override void OnRemove(StatusEffectHandler handler)
        {
            // Dùng UniqueId (không phải GetHashCode) đảm bảo xóa đúng modifier
            handler.PathFollower?.RemoveSpeedModifier(EffectID, UniqueId);
        }
    }
}