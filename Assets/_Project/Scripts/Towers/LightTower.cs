using UnityEngine;
using TowerDefense.Projectiles;
using TowerDefense.Utils;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Tháp ánh sáng:
    /// - Bắn đạn AoE.
    /// - Mỗi cấp có xác suất riêng để đạn gây x3 damage.
    /// - tripleDamageChance lấy từ TowerData.
    /// </summary>
    public class LightTower : TowerBase
    {
        [Header("Light Tower")]
        [SerializeField] private AreaProjectile projectilePrefab;

        [Header("Triple Damage")]
        [Tooltip("Hệ số sát thương khi proc. Theo thiết kế là x3.")]
        [SerializeField] private float tripleDamageMultiplier = 3f;

        private int totalShots;
        private int tripleShots;

        public int TotalShots => totalShots;
        public int TripleShots => tripleShots;
        public float ActualTripleRate => totalShots > 0 ? (float)tripleShots / totalShots : 0f;

        protected override void OnTowerAwake()
        {
            if (projectilePrefab == null)
            {
                Debug.LogError($"[LightTower] '{gameObject.name}' thiếu projectilePrefab!");
            }
        }

        protected override void OnAttack(GameObject target, TowerLevelData stats)
        {
            if (projectilePrefab == null || PoolManager.Instance == null) return;

            totalShots++;

            float finalDamage = stats.damage;
            bool isTripleDamage = Random.value < stats.tripleDamageChance;

            if (isTripleDamage)
            {
                finalDamage *= tripleDamageMultiplier;
                tripleShots++;
            }

            AreaProjectile projectile = PoolManager.Instance.GetProjectile(projectilePrefab, FirePoint.position) as AreaProjectile;
            if (projectile == null) return;

            projectile.SetBlastRadius(stats.blastRadius);
            projectile.Launch(target, finalDamage);

            if (isTripleDamage)
            {
                ApplyTripleVisual(projectile);
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"<color=yellow>[LightTower]</color> Bắn AoE Light: " +
                $"DMG={finalDamage:F1}, " +
                $"Radius={stats.blastRadius:F1}, " +
                $"x3={isTripleDamage}, " +
                $"Chance={stats.tripleDamageChance * 100f:F0}%"
            );
#endif
        }

        private void ApplyTripleVisual(AreaProjectile projectile)
        {
            if (projectile == null) return;

            projectile.transform.localScale *= 1.35f;

            if (projectile.TryGetComponent(out SpriteRenderer sr))
            {
                sr.color = new Color(1f, 0.95f, 0.35f, 1f);
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

#if UNITY_EDITOR
            if (Data != null)
            {
                string label =
                    $"Light AoE: {CurrentStats.blastRadius:F1}\n" +
                    $"x3 Chance: {CurrentStats.tripleDamageChance * 100f:F0}%";

                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, label);
            }
#endif
        }
    }
}
