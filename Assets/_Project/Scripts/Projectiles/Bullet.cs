using TowerDefense.Enemies;
using TowerDefense.Pooling;
using UnityEngine;

namespace TowerDefense.Projectiles
{
    /// <summary>
    /// Viên đạn dùng chung cho cả 4 loại Tháp.
    /// Nó tự bay theo mục tiêu (homing), gây sát thương và có thể làm chậm.
    /// </summary>
    public class Bullet : MonoBehaviour
    {
        [SerializeField] private float speed = 25f;

        private Transform target;
        private EnemyBase targetEnemy;
        private float damage;
        private bool isSlow;
        private float slowPercent;
        private float slowDuration;

        public void Initialize(Transform targetTransform, float dmg, bool applySlow = false, float slowPct = 0f, float slowDur = 0f)
        {
            target = targetTransform;
            if (target != null)
                targetEnemy = target.GetComponent<EnemyBase>();

            damage = dmg;
            isSlow = applySlow;
            slowPercent = slowPct;
            slowDuration = slowDur;
        }

        private void Update()
        {
            // Nếu mục tiêu bị tiêu diệt bởi đạn khác trước khi viên đạn này tới nơi
            if (target == null || !target.gameObject.activeInHierarchy || targetEnemy == null || targetEnemy.IsDead)
            {
                SimplePool.Instance.Return(gameObject);
                return;
            }

            Vector3 dir = target.position - transform.position;
            float distanceThisFrame = speed * Time.deltaTime;

            // Nếu đạn chạm mục tiêu trong frame này
            if (dir.sqrMagnitude <= distanceThisFrame * distanceThisFrame)
            {
                HitTarget();
                return;
            }

            // Di chuyển đạn và chĩa mũi đạn về phía mục tiêu
            transform.Translate(dir.normalized * distanceThisFrame, Space.World);
            transform.up = dir.normalized;
        }

        private void HitTarget()
        {
            if (targetEnemy != null)
            {
                targetEnemy.TakeDamage(damage);
                if (isSlow) targetEnemy.ApplySlow(slowPercent, slowDuration);
            }

            SimplePool.Instance.Return(gameObject);
        }
    }
}
