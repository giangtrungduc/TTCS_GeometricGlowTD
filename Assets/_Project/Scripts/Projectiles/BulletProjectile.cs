using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Projectiles
{
    public class BulletProjectile : ProjectileBase
    {
        // ============================
        // CẤU HÌNH
        // ============================

        [Header("Bullet Settings")]
        [Tooltip("Có thu nhỏ dần khi bay gần đến mục tiêu không?")]
        [SerializeField] private bool shrinkOnApproach = false;

        [Tooltip("Tỷ lệ scale lúc mới bắn")]
        [SerializeField] private float startScale = 1f;

        [Tooltip("Tỷ lệ scale khi chạm mục tiêu")]
        [SerializeField] private float endScale = 0.5f;

        // ============================
        // STATE
        // ============================

        private float initialDistance;
        private Vector3 originalScale; // Lưu scale gốc của Prefab để tránh bóp méo hình

        // ============================
        // LIFECYCLE OVERRIDES
        // ============================

        protected override void Awake()
        {
            base.Awake();
            originalScale = transform.localScale;
        }

        protected override void OnLaunched()
        {
            // Reset scale dựa trên scale gốc của prefab
            transform.localScale = originalScale * startScale;

            // Tính khoảng cách ban đầu bằng lastKnownTargetPos
            initialDistance = Vector2.Distance(transform.position, lastKnownTargetPos);

            if (initialDistance < 0.01f)
            {
                initialDistance = 1f; // Tránh lỗi chia cho 0
            }
        }

        protected override void OnProjectileUpdate()
        {
            if (!shrinkOnApproach) return;

            float currentDist = Vector2.Distance(transform.position, lastKnownTargetPos);
            float ratio = Mathf.Clamp01(currentDist / initialDistance);

            float currentScaleRatio = Mathf.Lerp(endScale, startScale, ratio);
            transform.localScale = originalScale * currentScaleRatio;
        }

        // ============================
        // XỬ LÝ SÁT THƯƠNG
        // ============================

        protected override void OnHit(GameObject hitTarget, float hitDamage)
        {
            IDamageable damageable = hitTarget.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(hitDamage);
            }
            else
            {
                Debug.LogWarning(
                    $"[BulletProjectile] Hit '{hitTarget.name}' but no IDebuffable found!"
                );
            }
        }
    }
}