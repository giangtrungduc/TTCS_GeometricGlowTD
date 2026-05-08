using UnityEngine;
using TowerDefense.Projectiles;
using TowerDefense.Utils;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Tháp thường:
    /// - Không có gì đặc biệt.
    /// - Upgrade chỉ thay đổi chỉ số.
    /// - Bắn đạn đơn.
    /// </summary>
    public class NormalTower : TowerBase
    {
        [Header("Normal Tower")]
        [SerializeField] private SingleProjectile projectilePrefab;

        private ObjectPool<SingleProjectile> projectilePool;

        protected override void OnTowerAwake()
        {
            if (projectilePrefab == null)
            {
                Debug.LogError($"[NormalTower] '{gameObject.name}' thiếu projectilePrefab!");
                return;
            }

            projectilePool = new ObjectPool<SingleProjectile>(projectilePrefab, transform, 5);
        }

        protected override void OnAttack(GameObject target, TowerLevelData stats)
        {
            if (projectilePool == null) return;

            SingleProjectile projectile = projectilePool.Get(FirePoint.position);
            if (projectile == null) return;

            projectile.Launch(target, stats.damage);
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

#if UNITY_EDITOR
            if (Data != null)
            {
                string label = $"DMG: {CurrentStats.damage:F0} | DPS: {CurrentStats.BaseDPS:F1}";
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, label);
            }
#endif
        }
    }
}
