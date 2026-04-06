using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Projectiles
{
    public class OrbProjectile : ProjectileBase
    {
        // ===========================
        // CẤU HÌNH
        // ===========================

        [Header("Orb AoE Settings")]

        [Tooltip("Bán kính nổ mặc định. LightTower sẽ override qua SetBlastRadius().")]
        [SerializeField] private float defaultBlastRadius = 1.5f;

        [Tooltip("Layer của enemy. Dùng để lọc Physics2D — chỉ quét đúng đối tượng cần thiết.")]
        [SerializeField] private LayerMask enemyLayer;

        [Tooltip("Số enemy tối đa bị ảnh hưởng bởi 1 vụ nổ. Tăng nếu mật độ enemy cao.")]
        [SerializeField][Range(1, 50)] private int maxAoeTargets = 15;

        // ===========================
        // STATE
        // ===========================

        /// <summary>Bán kính nổ thực tế. Set bởi LightTower trước khi Launch().</summary>
        private float blastRadius;

        private Collider2D[] aoeBuffer;

        private ContactFilter2D enemyFilter;

        /// <summary>Scale gốc của Prefab — khôi phục mỗi lần lấy từ Pool, tránh bóp méo.</summary>
        private Vector3 originalScale;

        // ===========================
        // INIT
        // ===========================

        protected override void Awake()
        {
            base.Awake();

            originalScale = transform.localScale;

            // Cấp phát buffer 1 lần duy nhất — không bao giờ alloc lại trong runtime
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

        // ===========================
        // PUBLIC API
        // ===========================

        /// <summary>
        /// Override bán kính nổ. Gọi trước Launch().
        /// </summary>
        /// <param name="radius">Bán kính (world units). Giá trị âm bị bỏ qua.</param>
        public void SetBlastRadius(float radius)
        {
            if (radius <= 0f)
            {
                Debug.LogWarning($"[OrbProjectile] SetBlastRadius nhận giá trị không hợp lệ: {radius}. Giữ nguyên {blastRadius}.");
                return;
            }
            blastRadius = radius;
        }

        // ===========================
        // LIFECYCLE
        // ===========================

        protected override void OnLaunched()
        {
            transform.localScale = originalScale;
        }

        protected override void OnHit(GameObject hitTarget, float hitDamage)
        {
            Vector2 hitPos = transform.position;

            int count = Physics2D.OverlapCircle(hitPos, blastRadius, enemyFilter, aoeBuffer);

            int enemiesHit = 0;

            for (int i = 0; i < count; i++)
            {
                Collider2D col = aoeBuffer[i];
                if (col == null) continue;
                if (!col.gameObject.activeInHierarchy) continue;

                IDamageable damageable = col.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    damageable.TakeDamage(hitDamage);
                    enemiesHit++;
                }
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"<color=yellow>[OrbProjectile]</color> AoE nổ tại {hitPos}, " +
                $"bán kính <b>{blastRadius:F1}</b>, trúng <b>{enemiesHit}</b> quái " +
                $"({hitDamage} DMG/quái)"
            );
#endif
        }

        // ===========================
        // GIZMOS
        // ===========================
#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            // Khi chưa Play: blastRadius chưa được Awake khởi tạo → dùng defaultBlastRadius
            // Khi đang Play: dùng blastRadius thực tế (có thể đã được LightTower override)
            float radius = Application.isPlaying ? blastRadius : defaultBlastRadius;

            Gizmos.color = new Color(1f, 0.8f, 0f, 0.15f);
            Gizmos.DrawSphere(transform.position, radius);

            Gizmos.color = new Color(1f, 0.8f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}