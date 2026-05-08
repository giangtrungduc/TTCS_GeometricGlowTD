using UnityEngine;

namespace TowerDefense.Projectiles
{
    /// <summary>
    /// Đạn AoE: gây damage và truyền effect nếu có cho enemy trong phạm vi nổ.
    /// </summary>
    public class AreaProjectile : ProjectileBase
    {
        [Header("Area Projectile Settings")]

        [Tooltip("Bán kính nổ mặc định.")]
        [SerializeField] private float defaultBlastRadius = 1.5f;

        [Tooltip("Layer của enemy.")]
        [SerializeField] private LayerMask enemyLayer;

        [Tooltip("Số enemy tối đa bị ảnh hưởng bởi 1 vụ nổ.")]
        [SerializeField][Range(1, 50)] private int maxAoeTargets = 15;

        private float blastRadius;
        private Collider2D[] aoeBuffer;
        private ContactFilter2D enemyFilter;
        private Vector3 originalScale;

        protected override void Awake()
        {
            base.Awake();

            originalScale = transform.localScale;
            aoeBuffer = new Collider2D[maxAoeTargets];

            enemyFilter = new ContactFilter2D
            {
                useLayerMask = true,
                useTriggers = true,
            };

            enemyFilter.SetLayerMask(enemyLayer);
            blastRadius = defaultBlastRadius;
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            blastRadius = defaultBlastRadius;
        }

        public void SetBlastRadius(float radius)
        {
            if (radius <= 0f)
            {
                Debug.LogWarning(
                    $"[AreaProjectile] SetBlastRadius nhận giá trị không hợp lệ: {radius}. Giữ nguyên {blastRadius}."
                );
                return;
            }

            blastRadius = radius;
        }

        protected override void OnLaunched()
        {
            transform.localScale = originalScale;
        }

        protected override void OnHit(GameObject hitTarget, float hitDamage)
        {
            Vector2 hitPos = transform.position;

            int count = Physics2D.OverlapCircle(
                hitPos,
                blastRadius,
                enemyFilter,
                aoeBuffer
            );

            int enemiesHit = 0;

            for (int i = 0; i < count; i++)
            {
                Collider2D col = aoeBuffer[i];
                if (col == null) continue;
                if (!col.gameObject.activeInHierarchy) continue;

                ApplyDamageAndEffects(col.gameObject, hitDamage);
                enemiesHit++;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"<color=yellow>[AreaProjectile]</color> AoE nổ tại {hitPos}, " +
                $"bán kính <b>{blastRadius:F1}</b>, trúng <b>{enemiesHit}</b> quái " +
                $"({hitDamage} DMG/quái)"
            );
#endif
        }

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            float radius = Application.isPlaying ? blastRadius : defaultBlastRadius;

            Gizmos.color = new Color(1f, 0.8f, 0f, 0.15f);
            Gizmos.DrawSphere(transform.position, radius);

            Gizmos.color = new Color(1f, 0.8f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}