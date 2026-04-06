// ============================================================
// File: LightTower.cs
// Vị trí: Assets/_Project/Scripts/Towers/LightTower.cs
// Mục đích: Tháp kiểm soát đám đông (AoE), bắn cầu năng lượng.
// Kỹ năng Cấp 3 (Phán Xét): Có xác suất bắn 2 hoặc 4 viên đạn cùng lúc.
// ============================================================

using UnityEngine;
using TowerDefense.Projectiles;
using TowerDefense.Utils;

namespace TowerDefense.Towers
{
    public class LightTower : TowerBase
    {
        // ============================
        // CẤU HÌNH
        // ============================

        [Header("Light Tower")]
        [SerializeField] private OrbProjectile orbPrefab;

        [Header("Phán Xét (Cấp 3)")]
        [Tooltip("Xác suất bắn 2 orb (0.0 - 1.0)")]
        [Range(0f, 1f)]
        [SerializeField] private float doubleOrbChance = 0.2f;

        [Tooltip("Xác suất bắn 4 orb (0.0 - 1.0). Phải < doubleOrbChance")]
        [Range(0f, 1f)]
        [SerializeField] private float quadOrbChance = 0.01f;

        // ============================
        // STATE
        // ============================

        private ObjectPool<OrbProjectile> orbPool;
        private int totalShots = 0;
        private int doubleProcs = 0;
        private int quadProcs = 0;

        // ============================
        // PROPERTIES
        // ============================

        public bool IsJudgmentUnlocked => CurrentLevel >= 2;
        public int TotalShots => totalShots;
        public int DoubleProcs => doubleProcs;
        public int QuadProcs => quadProcs;

        // ============================
        // INIT
        // ============================

        protected override void OnTowerAwake()
        {
            if (orbPrefab == null)
            {
                Debug.LogError($"[LightTower] '{gameObject.name}' thiếu orbPrefab!");
                return;
            }

            orbPool = new ObjectPool<OrbProjectile>(orbPrefab, transform, 5);
        }

        // ============================
        // ATTACK LOGIC
        // ============================

        protected override void OnAttack(GameObject target, TowerLevelData stats)
        {
            if (orbPool == null) return;

            totalShots++;
            int orbCount = 1;

            // Xử lý kỹ năng Phán Xét
            if (IsJudgmentUnlocked)
            {
                float roll = Random.value;

                if (roll < quadOrbChance) // 1% ra 4 viên
                {
                    orbCount = 4;
                    quadProcs++;
                    Debug.Log($"<color=yellow>[LightTower]</color> ⚡⚡⚡⚡ QUAD JUDGMENT! (Lần {quadProcs}/{totalShots} phát)");
                }
                else if (roll < doubleOrbChance) // 19% ra 2 viên
                {
                    orbCount = 2;
                    doubleProcs++;

                    if (doubleProcs % 5 == 1)
                    {
                        Debug.Log($"<color=yellow>[LightTower]</color> ⚡⚡ DOUBLE JUDGMENT! (Rate thực tế: {(float)doubleProcs / totalShots * 100:F1}%)");
                    }
                }
            }

            // Bắn số lượng Orb tương ứng
            for (int i = 0; i < orbCount; i++)
            {
                SpawnOrb(target, stats, i);
            }
        }

        private void SpawnOrb(GameObject target, TowerLevelData stats, int orbIndex)
        {
            Vector3 spawnPos = transform.position;

            // Tách quỹ đạo nếu bắn nhiều viên để không đè hình lên nhau
            if (orbIndex > 0)
            {
                float angle = orbIndex * 90f * Mathf.Deg2Rad;
                spawnPos += new Vector3(Mathf.Cos(angle) * 0.3f, Mathf.Sin(angle) * 0.3f, 0f);
            }

            OrbProjectile orb = orbPool.Get(spawnPos);
            if (orb == null) return;

            orb.SetBlastRadius(stats.blastRadius);
            orb.Launch(target, stats.damage);
        }

        // ============================
        // UPGRADE & GIZMOS
        // ============================

        protected override void OnUpgraded()
        {
            if (CurrentLevel == 2)
            {
                Debug.Log($"[LightTower] ⚡ PHÁN XÉT UNLOCKED! {doubleOrbChance * 100}% Double, {quadOrbChance * 100}% Quad!");
            }
        }

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

            if (Data != null)
            {
                // Lưu ý: blastRadius vẽ ở đây chỉ để preview độ to của vụ nổ, không phải tầm đánh.
                Gizmos.color = new Color(1f, 0.8f, 0f, 0.15f);
                Gizmos.DrawSphere(transform.position, CurrentStats.blastRadius);

#if UNITY_EDITOR
                string label = $"Blast: {CurrentStats.blastRadius:F1}";
                if (IsJudgmentUnlocked) label += " (+Phán Xét)";
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, label);
#endif
            }
        }
    }
}