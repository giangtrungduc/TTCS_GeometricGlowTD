using UnityEngine;
using TowerDefense.Projectiles;
using TowerDefense.Utils;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Tháp lửa:
    /// - Bắn đạn đơn.
    /// - Damage tăng theo thời gian khi giữ cùng một mục tiêu.
    /// - Reset damage ramp khi đổi mục tiêu.
    /// </summary>
    public class FireTower : TowerBase
    {
        [Header("Fire Tower")]
        [SerializeField] private SingleProjectile projectilePrefab;

        private ObjectPool<SingleProjectile> projectilePool;

        private GameObject currentFireTarget;
        private float currentRampDamage;
        private float timeOnTarget;

        public float CurrentRampDamage => currentRampDamage;

        protected override void OnTowerAwake()
        {
            if (projectilePrefab == null)
            {
                Debug.LogError($"[FireTower] '{gameObject.name}' thiếu projectilePrefab!");
                return;
            }

            projectilePool = new ObjectPool<SingleProjectile>(projectilePrefab, transform, 5);
            ResetRampDamage();
        }

        protected override void OnTowerUpdate()
        {
            if (!IsRampTargetValid())
            {
                currentFireTarget = null;
                ResetRampDamage();
                return;
            }

            UpdateRampDamage();
        }

        protected override void OnAttack(GameObject target, TowerLevelData stats)
        {
            if (projectilePool == null) return;

            if (target != currentFireTarget)
            {
                currentFireTarget = target;
                ResetRampDamage();
            }

            SingleProjectile projectile = projectilePool.Get(FirePoint.position);
            if (projectile == null) return;

            projectile.Launch(target, currentRampDamage);

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log(
                $"<color=red>[FireTower]</color> Bắn '{target.name}' - {currentRampDamage:F1} DMG"
            );
#endif
        }

        private void UpdateRampDamage()
        {
            TowerLevelData stats = CurrentStats;

            if (stats.rampAmount <= 0f) return;
            if (stats.rampInterval <= 0f) return;

            timeOnTarget += Time.deltaTime;

            while (timeOnTarget >= stats.rampInterval)
            {
                currentRampDamage += stats.rampAmount;

                if (stats.maxDamage > 0f)
                {
                    currentRampDamage = Mathf.Min(currentRampDamage, stats.maxDamage);
                }

                timeOnTarget -= stats.rampInterval;
            }
        }

        private void ResetRampDamage()
        {
            if (Data == null)
            {
                currentRampDamage = 0f;
                timeOnTarget = 0f;
                return;
            }

            currentRampDamage = CurrentStats.damage;
            timeOnTarget = 0f;
        }

        private bool IsRampTargetValid()
        {
            if (currentFireTarget == null) return false;
            if (!currentFireTarget.activeInHierarchy) return false;

            float sqrDistance = (currentFireTarget.transform.position - transform.position).sqrMagnitude;
            float range = CurrentStats.range;

            return sqrDistance <= range * range;
        }

        protected override void OnUpgraded()
        {
            ResetRampDamage();
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            currentFireTarget = null;
            currentRampDamage = 0f;
            timeOnTarget = 0f;
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

#if UNITY_EDITOR
            if (Data != null)
            {
                string label = $"Fire DMG: {currentRampDamage:F1}";

                if (CurrentStats.maxDamage > 0f)
                {
                    label += $" / {CurrentStats.maxDamage:F1}";
                }

                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, label);
            }
#endif
        }
    }
}
