using System.Collections.Generic;
using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.StatusEffects
{
    /// <summary>
    /// Quản lý toàn bộ vòng đời của các StatusEffect trên một enemy.
    /// Gắn cùng GameObject với <see cref="PathFollower"/>.
    /// An toàn với Object Pool: dọn dẹp đầy đủ trong OnEnable/OnDisable.
    /// </summary>
    public class StatusEffectHandler : MonoBehaviour
    {
        // ===========================
        // REFERENCES
        // ===========================

        public PathFollower PathFollower { get; private set; }
        public SpriteRenderer SpriteRenderer { get; private set; }

        // ===========================
        // STATE
        // ===========================

        private readonly List<StatusEffect> activeEffects = new List<StatusEffect>();

        // Buffer tránh InvalidOperationException khi xóa trong lúc iterate
        private readonly List<StatusEffect> pendingRemoval = new List<StatusEffect>();

        private Color originalColor = Color.white;

        // Dirty flag: chỉ update màu khi trạng thái effect thực sự thay đổi
        private bool colorDirty = false;

        // ===========================
        // PROPERTIES
        // ===========================

        public int ActiveEffectCount => activeEffects.Count;
        public bool IsSlowed => HasEffect("Slow");
        public bool IsFrozen => HasEffect("Freeze");
        public bool IsSpeedBuffed => HasEffect("SpeedBuff");

        // ===========================
        // UNITY LIFECYCLE
        // ===========================

        private void Awake()
        {
            PathFollower = GetComponent<PathFollower>();
            SpriteRenderer = GetComponent<SpriteRenderer>();

            if (PathFollower == null)
            {
                Debug.LogWarning($"[StatusEffectHandler] Không tìm thấy PathFollower trên {name}");
            }
        }

        private void OnEnable()
        {
            // Dọn dẹp trạng thái cũ khi lấy từ Object Pool
            RemoveAllEffects();

            if (SpriteRenderer != null)
            {
                originalColor = SpriteRenderer.color;
            }
        }

        private void OnDisable()
        {
            // Chống rò rỉ modifier trong PathFollower khi trả về Pool
            RemoveAllEffects();
        }

        private void Update()
        {
            if (activeEffects.Count == 0) return;

            pendingRemoval.Clear();

            for (int i = 0; i < activeEffects.Count; i++)
            {
                bool stillActive = activeEffects[i].Tick(this, Time.deltaTime);
                if (!stillActive)
                {
                    pendingRemoval.Add(activeEffects[i]);
                    colorDirty = true;
                }
            }

            if (pendingRemoval.Count > 0)
            {
                for (int i = 0; i < pendingRemoval.Count; i++)
                {
                    activeEffects.Remove(pendingRemoval[i]);
                }
            }

            // Chỉ gọi khi cần — tránh set SpriteRenderer.color vô nghĩa mỗi frame
            if (colorDirty)
            {
                UpdateVisualTint();
                colorDirty = false;
            }
        }

        // ===========================
        // PUBLIC API
        // ===========================

        /// <summary>
        /// Thêm effect mới. Nếu effect không Stackable và đã tồn tại → Refresh thời gian.
        /// </summary>
        public void AddEffect(StatusEffect effect)
        {
            if (effect == null) return;

            if (!effect.Stackable)
            {
                StatusEffect existing = GetEffect(effect.EffectID);
                if (existing != null)
                {
                    existing.Refresh(effect.Duration);
                    return;
                }
            }

            activeEffects.Add(effect);
            effect.Apply(this);
            colorDirty = true;
        }

        /// <summary>Xóa tất cả effect có EffectID tương ứng.</summary>
        public void RemoveEffect(string effectID)
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (activeEffects[i].EffectID != effectID) continue;

                activeEffects[i].Remove(this);
                activeEffects.RemoveAt(i);
                colorDirty = true;
            }
        }

        /// <summary>Xóa tất cả effect, bất kể loại.</summary>
        public void RemoveAllEffects()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                activeEffects[i].Remove(this);
            }

            activeEffects.Clear();
            colorDirty = true;
        }

        /// <summary>
        /// Xóa tất cả debuff (IsDebuff = true).
        /// Dùng cho skill Bảo Hộ của Boss, không hard-code string.
        /// </summary>
        public void ClearAllDebuffs()
        {
            for (int i = activeEffects.Count - 1; i >= 0; i--)
            {
                if (!activeEffects[i].IsDebuff) continue;

                activeEffects[i].Remove(this);
                activeEffects.RemoveAt(i);
                colorDirty = true;
            }
        }

        public bool HasEffect(string effectID)
        {
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].EffectID == effectID && activeEffects[i].IsActive) return true;
            }
            return false;
        }

        /// <summary>Trả về instance đầu tiên tìm thấy với EffectID này, hoặc null.</summary>
        public StatusEffect GetEffect(string effectID)
        {
            for (int i = 0; i < activeEffects.Count; i++)
            {
                if (activeEffects[i].EffectID == effectID)
                {
                    return activeEffects[i];
                }
            }
            return null;
        }

        // ===========================
        // VISUALS
        // ===========================

        /// <summary>
        /// Đổi màu tint theo độ ưu tiên: Freeze > Slow > SpeedBuff > Gốc.
        /// Chỉ gọi khi colorDirty = true để tiết kiệm CPU.
        /// </summary>
        private void UpdateVisualTint()
        {
            if (SpriteRenderer == null) return;

            if (IsFrozen)
            {
                SpriteRenderer.color = new Color(0.5f, 0.8f, 1f);    // Xanh băng
            }
            else if (IsSlowed)
            {
                SpriteRenderer.color = new Color(0.6f, 0.7f, 1f);    // Xanh nhạt
            }
            else if (IsSpeedBuffed)
            {
                SpriteRenderer.color = new Color(1f, 0.8f, 0.4f);    // Vàng cam
            }
            else
            {
                SpriteRenderer.color = originalColor;
            }
        }

        /// <summary>Cho phép override màu gốc từ bên ngoài (VD: enemy có màu đặc biệt).</summary>
        public void SetOriginalColor(Color color)
        {
            originalColor = color;

            // Refresh màu ngay nếu hiện không có effect nào
            if (activeEffects.Count == 0 && SpriteRenderer != null)
            {
                SpriteRenderer.color = originalColor;
            }
        }
    }
}