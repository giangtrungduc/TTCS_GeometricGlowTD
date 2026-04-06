using UnityEngine;

namespace TowerDefense.Enemies.Abilities
{
    [RequireComponent(typeof(EnemyBase))]
    public class AuraAbility : MonoBehaviour
    {
        // ===========================
        // CẤU HÌNH
        // ===========================

        [Header("Aura Settings (Heal)")]

        [Tooltip("Bán kính vùng hồi máu")]
        [SerializeField] private float radius = 3f;

        [Tooltip("Lượng máu hồi phục mỗi nhịp")]
        [SerializeField] private float healAmount = 20f;

        [Tooltip("Thời gian giữa mỗi lần hồi máu (giây)")]
        [SerializeField] private float interval = 2f;

        [Tooltip("Layer của đồng minh (Enemy) để quét")]
        [SerializeField] private LayerMask enemyLayer;

        [Tooltip("Số lượng đồng minh tối đa hồi máu cùng lúc")]
        [SerializeField] private int maxTargets = 20;

        // ===========================
        // STATE & REFERENCES
        // ===========================

        private Collider2D[] aoeBuffer;
        private ContactFilter2D allyFilter;

        // ===========================
        // INIT
        // ===========================

        private void Awake()
        {
            aoeBuffer = new Collider2D[maxTargets];

            allyFilter = new ContactFilter2D();
            allyFilter.useLayerMask = true;
            allyFilter.SetLayerMask(enemyLayer);
            allyFilter.useTriggers = true;
        }

        // ===========================
        // LIFECYCLE
        // ===========================

        private void OnEnable()
        {
            InvokeRepeating(nameof(HealNearby), interval, interval);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(HealNearby));
        }

        // ===========================
        // ABILITY LOGIC
        // ===========================

        private void HealNearby()
        {
            int count = Physics2D.OverlapCircle(transform.position, radius, allyFilter, aoeBuffer);
            int healedCount = 0;

            for (int i = 0; i < count; i++)
            {
                Collider2D col = aoeBuffer[i];
                if (col == null || !col.gameObject.activeInHierarchy) continue;

                if (col.gameObject == gameObject) continue; 

                // Kiểm tra interface IDamageable và gọi Heal
                if (col.TryGetComponent(out IDamageable damageable))
                {
                    // Tránh gọi Heal nếu máu đã đầy (tối ưu hiệu năng)
                    if (damageable.CurrentHp < damageable.MaxHp)
                    {
                        damageable.Heal(healAmount);
                        healedCount++;
                    }
                }
            }

            if (healedCount > 0)
            {
                Debug.Log($"<color=#00FF00>[Healer]</color> '{name}' đã hồi {healAmount} HP cho {healedCount} đồng minh!");
            }
        }

        // ===========================
        // GIZMOS
        // ===========================

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(0f, 1f, 0f, 0.2f);
            Gizmos.DrawSphere(transform.position, radius);
            Gizmos.color = new Color(0f, 1f, 0f, 0.6f);
            Gizmos.DrawWireSphere(transform.position, radius);
        }
#endif
    }
}