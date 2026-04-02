using System.Threading;
using UnityEngine;

namespace TowerDefense.StatusEffects
{
    /// <summary>
    /// Base class cho mọi status effect (Slow, Freeze, SpeedBuff, ...).
    /// Lifecycle: Apply → Tick (mỗi frame) → Remove.
    /// Được quản lý hoàn toàn bởi <see cref="StatusEffectHandler"/>.
    /// </summary>
    public abstract class StatusEffect
    {
        // ===========================
        // UNIQUE ID
        // ===========================

        // Counter toàn cục, thread-safe — đảm bảo mỗi instance có ID duy nhất
        private static int _idCounter = 0;

        /// <summary>ID duy nhất của instance này, dùng làm sourceId trong PathFollower.</summary>
        public int UniqueId { get; } = Interlocked.Increment(ref _idCounter);

        // ===========================
        // CONFIGURATION
        // ===========================

        /// <summary>
        /// Thời gian hiệu lực (giây). Dùng -1f cho hiệu ứng vĩnh viễn (không tự hết).
        /// </summary>
        public float Duration { get; private set; }

        /// <summary>
        /// Định danh loại effect. Các effect cùng EffectID được Handler coi là cùng loại.
        /// </summary>
        public abstract string EffectID { get; }

        /// <summary>
        /// true  → nhiều nguồn có thể áp cùng lúc (VD: 3 tower cùng slow 1 enemy).
        /// false → chỉ giữ 1 instance, apply lại sẽ Refresh thời gian.
        /// </summary>
        public virtual bool Stackable => false;

        /// <summary>true nếu đây là debuff (dùng cho ClearDebuffs).</summary>
        public virtual bool IsDebuff => false;

        // ===========================
        // STATE
        // ===========================

        public float TimeRemaining { get; private set; }
        public bool IsActive { get; private set; }

        /// <summary>
        /// true khi thời gian đã hết.
        /// Effect vĩnh viễn (Duration == -1f) không bao giờ expired.
        /// </summary>
        public bool IsExpired => Duration > 0f && TimeRemaining <= 0f;

        // ===========================
        // CONSTRUCTOR
        // ===========================

        protected StatusEffect(float duration)
        {
            Duration = duration;
            TimeRemaining = duration;
            IsActive = false;
        }

        // ===========================
        // LIFECYCLE (internal — chỉ Handler gọi)
        // ===========================

        internal void Apply(StatusEffectHandler handler)
        {
            IsActive = true;
            TimeRemaining = Duration;
            OnApply(handler);
        }

        /// <returns>false nếu effect đã hết hạn và cần bị xóa.</returns>
        internal bool Tick(StatusEffectHandler handler, float deltaTime)
        {
            if (!IsActive) return false;

            if (Duration > 0f)
            {
                TimeRemaining -= deltaTime;
                if (TimeRemaining <= 0f)
                {
                    Remove(handler);
                    return false;
                }
            }

            OnTick(handler, deltaTime);
            return true;
        }

        internal void Remove(StatusEffectHandler handler)
        {
            if (!IsActive) return;

            IsActive = false;
            OnRemove(handler);
        }

        // ===========================
        // REFRESH
        // ===========================

        /// <summary>Làm mới thời gian về đúng Duration gốc.</summary>
        internal void Refresh()
        {
            TimeRemaining = Duration;
        }

        /// <summary>
        /// Làm mới thời gian, chỉ ghi đè Duration nếu duration mới dài hơn.
        /// Effect đang permanent (-1f) không bị thay đổi.
        /// </summary>
        internal void Refresh(float newDuration)
        {
            if (Duration < 0f) return; // Đang permanent, giữ nguyên

            if (newDuration < 0f || newDuration > Duration)
            {
                Duration = newDuration;
            }

            TimeRemaining = Duration;
        }

        // ===========================
        // ABSTRACT
        // ===========================

        protected abstract void OnApply(StatusEffectHandler handler);
        protected abstract void OnTick(StatusEffectHandler handler, float deltaTime);
        protected abstract void OnRemove(StatusEffectHandler handler);
    }
}