using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Towers
{
    public class TargetingSystem : MonoBehaviour
    {
        // ============================
        // CẤU HÌNH
        // ============================

        [Header("Targeting Settings")]

        [Tooltip("Layer chứa enemy. Chỉ detect object trên layer này")]
        [SerializeField] private LayerMask enemyLayer;

        [Tooltip("Số enemy tối đa detect cùng lúc. Tăng nếu cần")]
        [SerializeField] private int maxDetectCount = 20;

        // ============================
        // STATE
        // ============================

        private float currentRange;

        private Collider2D[] hitBuffer;

        private ContactFilter2D enemyFilter;

        private GameObject currentTarget;

        // ============================
        // PROPERTIES
        // ============================

        public GameObject CurrentTarget => currentTarget;

        public bool HasTarget => currentTarget != null
                              && currentTarget.activeInHierarchy;

        public float Range => currentRange;

        // ============================
        // INIT
        // ============================

        private void Awake()
        {
            // Pre-allocate buffer → không GC khi detect
            hitBuffer = new Collider2D[maxDetectCount];

            // Setup ContactFilter2D (Cách mới của Unity 6)
            enemyFilter = new ContactFilter2D();
            enemyFilter.useLayerMask = true;
            enemyFilter.SetLayerMask(enemyLayer);
            enemyFilter.useTriggers = true; // Bật true nếu enemy của bạn dùng trigger collider
        }

        public void SetRange(float range)
        {
            currentRange = range;
        }

        // ============================
        // PUBLIC: Tìm target tốt nhất
        // ============================

        public GameObject GetBestTarget()
        {
            // Truyền vào: vị trí, bán kính, bộ lọc layer, và mảng buffer
            int count = Physics2D.OverlapCircle(transform.position, currentRange, enemyFilter, hitBuffer);

            // Không có enemy nào trong tầm
            if (count == 0)
            {
                currentTarget = null;
                return null;
            }

            // Tìm enemy gần exit nhất
            GameObject bestTarget = null;
            float bestProgress = -1f;

            for (int i = 0; i < count; i++)
            {
                Collider2D col = hitBuffer[i];

                if (col == null) continue;
                if (!col.gameObject.activeInHierarchy) continue;

                PathFollower pathFollower = col.GetComponent<PathFollower>();

                if (pathFollower == null) continue;
                if (pathFollower.HasReachedEnd) continue;

                float progress = pathFollower.ProgressRatio;

                if (progress > bestProgress)
                {
                    bestProgress = progress;
                    bestTarget = col.gameObject;
                }
            }

            currentTarget = bestTarget;
            return bestTarget;
        }

        public bool IsCurrentTargetValid()
        {
            if (currentTarget == null) return false;
            if (!currentTarget.activeInHierarchy) return false;

            float distance = Vector2.Distance(transform.position, currentTarget.transform.position);
            if (distance > currentRange) return false;

            PathFollower pf = currentTarget.GetComponent<PathFollower>();
            if (pf != null && pf.HasReachedEnd) return false;

            return true;
        }

        public void ClearTarget()
        {
            currentTarget = null;
        }

        // ============================
        // GIZMOS
        // ============================

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = HasTarget
                ? new Color(0f, 1f, 0f, 0.3f)   // Xanh lá nhạt
                : new Color(1f, 1f, 1f, 0.2f);   // Trắng nhạt

            Gizmos.DrawWireSphere(transform.position, currentRange);

            if (HasTarget)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(
                    transform.position,
                    currentTarget.transform.position
                );
            }
        }
    }
}