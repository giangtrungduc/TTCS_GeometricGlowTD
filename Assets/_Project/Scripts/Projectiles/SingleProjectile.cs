using UnityEngine;

namespace TowerDefense.Projectiles
{
    /// <summary>
    /// Đạn đơn: gây damage và truyền effect nếu có cho đúng 1 enemy.
    /// </summary>
    public class SingleProjectile : ProjectileBase
    {
        [Header("Single Target Visual Settings")]

        [Tooltip("Có thu nhỏ dần khi bay gần đến mục tiêu không?")]
        [SerializeField] private bool shrinkOnApproach = false;

        [Tooltip("Tỷ lệ scale lúc mới bắn")]
        [SerializeField] private float startScale = 1f;

        [Tooltip("Tỷ lệ scale khi chạm mục tiêu")]
        [SerializeField] private float endScale = 0.5f;

        private float initialDistance;
        private Vector3 originalScale;

        protected override void Awake()
        {
            base.Awake();
            originalScale = transform.localScale;
        }

        protected override void OnLaunched()
        {
            transform.localScale = originalScale * startScale;

            initialDistance = Vector2.Distance(transform.position, lastKnownTargetPos);

            if (initialDistance < 0.01f)
            {
                initialDistance = 1f;
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

        protected override void OnHit(GameObject hitTarget, float hitDamage)
        {
            ApplyDamageAndEffects(hitTarget, hitDamage);
        }
    }
}