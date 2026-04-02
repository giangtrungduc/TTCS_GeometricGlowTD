// ============================================================
// File: NormalTower.cs
// Vị trí: Assets/_Project/Scripts/Towers/NormalTower.cs
// Mục đích: Tháp cơ bản, DPS ổn định, mục tiêu đơn.
// Ability Cấp 3 (Thiên Xạ): Có tỷ lệ Crit x4 sát thương.
// ============================================================

using UnityEngine;
using TowerDefense.Projectiles;
using TowerDefense.Utils;

namespace TowerDefense.Towers
{
    public class NormalTower : TowerBase
    {
        // ============================
        // CẤU HÌNH
        // ============================

        [Header("Normal Tower")]
        [SerializeField] private BulletProjectile bulletPrefab;

        [Header("Thiên Xạ (Cấp 3)")]
        [Tooltip("Tỷ lệ Crit (0.0 - 1.0)")]
        [Range(0f, 1f)]
        [SerializeField] private float critChance = 0.2f;

        [Tooltip("Hệ số nhân sát thương khi Crit")]
        [Min(1f)]
        [SerializeField] private float critMultiplier = 4f;

        // ============================
        // STATE
        // ============================

        private ObjectPool<BulletProjectile> bulletPool;
        private int totalShots = 0;
        private int totalCrits = 0;

        public int TotalShots => totalShots;
        public int TotalCrits => totalCrits;
        public float ActualCritRate => totalShots > 0 ? (float)totalCrits / totalShots : 0f;

        // Cấp 3 tương đương index 2 (0-based)
        public bool IsCritUnlocked => CurrentLevel >= 2;

        // ============================
        // INIT
        // ============================

        protected override void OnTowerAwake()
        {
            if (bulletPrefab == null)
            {
                Debug.LogError($"[NormalTower] '{gameObject.name}' thiếu bulletPrefab!");
                return;
            }

            bulletPool = new ObjectPool<BulletProjectile>(bulletPrefab, transform, 5);
        }

        // ============================
        // ATTACK LOGIC
        // ============================

        protected override void OnAttack(GameObject target, TowerLevelData stats)
        {
            if (bulletPool == null) return;

            totalShots++;
            float finalDamage = stats.damage;
            bool isCrit = false;

            // Xử lý Crit nếu đã đạt Cấp 3
            if (IsCritUnlocked && Random.value < critChance)
            {
                finalDamage *= critMultiplier;
                isCrit = true;
                totalCrits++;
            }

            BulletProjectile bullet = bulletPool.Get(transform.position);
            if (bullet == null) return;

            if (bullet.TryGetComponent(out SpriteRenderer sr))
            {
                sr.color = Color.white;
            }

            bullet.Launch(target, finalDamage);

            if (isCrit)
            {
                OnCriticalHit(bullet, finalDamage);
            }
        }

        protected override void OnUpgraded()
        {
            if (CurrentLevel == 2)
            {
                Debug.Log($"[NormalTower] ⚔ THIÊN XẠ UNLOCKED! {critChance * 100}% cơ hội x{critMultiplier} DMG!");
            }
        }

        // ============================
        // VISUALS & GIZMOS
        // ============================

        private void OnCriticalHit(BulletProjectile bullet, float damage)
        {
            if (bullet.TryGetComponent(out SpriteRenderer sr))
            {
                sr.color = new Color(1f, 0.3f, 0.1f, 1f); // Đổi sang Đỏ cam
            }

            bullet.transform.localScale *= 1.5f;

            if (totalCrits % 10 == 1)
            {
                Debug.Log($"[NormalTower] ⚔ CRIT! DMG={damage:F0} (Tỷ lệ thực tế: {ActualCritRate * 100:F1}%)");
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

#if UNITY_EDITOR
            if (Data != null)
            {
                string label = IsCritUnlocked ? $"DPS: {CurrentStats.BaseDPS:F0} (+crit)" : $"DPS: {CurrentStats.BaseDPS:F0}";
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, label);
            }
#endif
        }
    }
}