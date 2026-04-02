namespace TowerDefense.StatusEffects
{
    /// <summary>
    /// Đóng băng enemy — tốc độ về 0 ngay lập tức.
    /// Không Stackable: apply lại chỉ Refresh thời gian (freeze dài hơn thắng).
    /// Khi hết hạn, PathFollower tự phục hồi tốc độ, kể cả khi vẫn còn SlowEffect đang active.
    /// </summary>
    public class FreezeEffect : StatusEffect
    {
        public override string EffectID => "Freeze";
        public override bool Stackable => false;
        public override bool IsDebuff => true;

        /// <param name="duration">Thời gian đóng băng (giây). -1f = vĩnh viễn.</param>
        public FreezeEffect(float duration) : base(duration) { }

        protected override void OnApply(StatusEffectHandler handler)
        {
            // Multiplier = 0f — PathFollower ưu tiên Freeze cao nhất, tốc độ = 0 ngay
            handler.PathFollower?.AddSpeedModifier(EffectID, UniqueId, 0f);
        }

        protected override void OnTick(StatusEffectHandler handler, float deltaTime)
        {
            // Passive — PathFollower tự giữ tốc độ = 0
        }

        protected override void OnRemove(StatusEffectHandler handler)
        {
            handler.PathFollower?.RemoveSpeedModifier(EffectID, UniqueId);
        }
    }
}