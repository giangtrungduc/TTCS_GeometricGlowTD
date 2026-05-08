using UnityEngine;
using TowerDefense.Projectiles;
using TowerDefense.StatusEffects;
using TowerDefense.Utils;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Tháp băng:
    /// - Bắn đạn AoE.
    /// - Đạn nổ gây damage cho toàn bộ quái trong phạm vi nổ.
    /// - Toàn bộ quái trúng vụ nổ nhận SlowEffect.
    /// - slowPercent, slowDuration, blastRadius lấy từ TowerData theo từng cấp.
    /// </summary>
    public class IceTower : TowerBase
    {
        [Header("Ice Tower")]
        [SerializeField] private AreaProjectile projectilePrefab;

        private ObjectPool<AreaProjectile> projectilePool;

        protected override void OnTowerAwake()
        {
            if (projectilePrefab == null)
            {
                Debug.LogError($"[IceTower] '{gameObject.name}' thiếu projectilePrefab!");
                return;
            }

            projectilePool = new ObjectPool<AreaProjectile>(projectilePrefab, transform, 5);
        }

        protected override void OnAttack(GameObject target, TowerLevelData stats)
        {
            if (projectilePool == null) return;

            AreaProjectile projectile = projectilePool.Get(FirePoint.position);
            if (projectile == null) return;

            projectile.SetBlastRadius(stats.blastRadius);

            float slowPercent = stats.slowPercent;
            float slowDuration = stats.slowDuration;

            projectile.SetOnApplyEffect(enemy =>
            {
                if (enemy == null) return;
                if (!enemy.activeInHierarchy) return;
                if (slowPercent <= 0f) return;
                if (slowDuration <= 0f) return;

                StatusEffectHandler handler = enemy.GetComponent<StatusEffectHandler>();
                if (handler == null) return;

                handler.AddEffect(new SlowEffect(slowPercent, slowDuration));
            });

            projectile.Launch(target, stats.damage);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"<color=cyan>[IceTower]</color> Bắn AoE Ice: " +
                $"DMG={stats.damage:F1}, " +
                $"Slow={slowPercent * 100f:F0}%, " +
                $"Duration={slowDuration:F1}s, " +
                $"Radius={stats.blastRadius:F1}"
            );
#endif
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

#if UNITY_EDITOR
            if (Data != null)
            {
                string label =
                    $"Ice AoE: {CurrentStats.blastRadius:F1}\n" +
                    $"Slow: {CurrentStats.slowPercent * 100f:F0}% / {CurrentStats.slowDuration:F1}s";

                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, label);
            }
#endif
        }
    }
}
