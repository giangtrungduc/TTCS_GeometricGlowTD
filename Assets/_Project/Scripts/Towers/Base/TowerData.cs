using UnityEngine;

namespace TowerDefense.Towers
{
    [System.Serializable]
    public struct TowerLevelData
    {
        // ============================
        // COMBAT — Mọi tower đều dùng
        // ============================

        [Header("- Combat -")]

        [Tooltip("Sát thương mỗi lần tấn công (hoặc mỗi tick cho Fire)")]
        [Min(0f)]
        public float damage;

        [Tooltip("Thời gian giữa mỗi lần tấn công (giây). Nhỏ hơn = bắn nhanh hơn")]
        [Min(0.05f)]
        public float attackCooldown;

        [Tooltip("Tầm bắn (world units). Dùng cho Physics2D.OverlapCircle")]
        [Min(0.5f)]
        public float range;

        // ============================
        // ECONOMY — Chi phí
        // ============================

        [Tooltip("Chi phí mua (Cấp 1) hoặc nâng cấp (Cấp 2, 3)")]
        [Min(0)]
        public int cost;

        // ============================
        // RAMP — Chỉ Fire Tower dùng
        // ============================

        [Header("— Ramp (Fire Tower) —")]

        [Tooltip("Damage tăng thêm mỗi lần ramp. 0 = không ramp")]
        [Min(0f)]
        public float rampAmount;

        [Tooltip("Thời gian giữa mỗi lần ramp (giây)")]
        [Min(0.1f)]
        public float rampInterval;

        [Tooltip("Damage tối đa (cap). Ramp không vượt quá giá trị này")]
        [Min(0f)]
        public float maxDamage;

        // ============================
        // SLOW — Chỉ Ice Tower dùng
        // ============================

        [Header("— Slow (Ice Tower) —")]

        [Tooltip("Phần trăm giảm tốc (0.0 - 1.0). VD: 0.3 = chậm 30%")]
        [Range(0f, 1f)]
        public float slowPercent;

        [Tooltip("Phần trăm đóng băng mỗi 2 giây (Cấp 3). 0 = không freeze")]
        [Range(0f, 1f)]
        public float freezeChance;

        [Tooltip("Thời gian đóng băng (giây)")]
        [Min(0f)]
        public float freezeDuration;

        // ============================
        // AOE — Chỉ Light Tower dùng
        // ============================

        [Header("— AoE (Light Tower) —")]

        [Tooltip("Bán kính nổ khi đạn trúng (world units). 0 = single target")]
        [Min(0f)]
        public float blastRadius;

        // ============================
        // VISUAL
        // ============================

        [Header("— Visual —")]

        [Tooltip("Sprite của tháp ở cấp này. Đổi khi upgrade")]
        public Sprite towerSprite;

        // ============================
        // COMPUTED PROPERTIES
        // ============================

        /// <summary>
        /// DPS cơ bản (không tính ramp, crit, AoE).
        /// Dùng để hiển thị trên UI tooltip.
        /// </summary>
        public float BaseDPS => attackCooldown > 0 ? damage / attackCooldown : 0f;
    }

    [CreateAssetMenu(fileName = "NewTowerData", menuName = "TD/Tower Data", order = 1)]
    public class TowerData : ScriptableObject
    {
        // ============================
        // TOWER IDENTITY
        // ============================

        [Header("═══ Tower Identity ═══")]

        [Tooltip("Tên hiển thị trên UI (VD: 'Tháp Lửa')")]
        public string towerName = "New Tower";

        [Tooltip("Mô tả ngắn cho tooltip")]
        [TextArea(2, 4)]
        public string description = "Tower description here.";

        [Tooltip("Icon hiển thị trên RadialMenu khi chọn build")]
        public Sprite icon;

        [Tooltip("Màu đại diện (dùng cho range indicator, UI highlight)")]
        public Color themeColor = Color.white;

        // ============================
        // TOWER PREFAB
        // ============================

        [Header("═══ Prefab ═══")]

        [Tooltip("Prefab của tower (chứa TowerBase component)")]
        public GameObject towerPrefab;

        // ============================
        // LEVEL DATA
        // ============================

        [Header("═══ Level Stats (3 Cấp) ═══")]

        [Tooltip("Stats cho 3 cấp. Index 0 = Cấp 1, Index 1 = Cấp 2, Index 2 = Cấp 3")]
        public TowerLevelData[] levels = new TowerLevelData[3];

        // ============================
        // ABILITY (Cấp 3)
        // ============================

        [Header("═══ Ability Cấp 3 ═══")]

        [Tooltip("Tên ability đặc biệt khi đạt Cấp 3")]
        public string abilityName = "None";

        [Tooltip("Mô tả ability cho UI tooltip")]
        [TextArea(2, 3)]
        public string abilityDescription = "";

        // ============================
        // PUBLIC METHODS
        // ============================

        public TowerLevelData GetLevel(int levelIndex)
        {
            if(levels == null || levels.Length == 0)
            {
                return new TowerLevelData();
            }

            int clampedIndex = Mathf.Clamp(levelIndex, 0, levels.Length - 1);
            return levels[clampedIndex];
        }

        public int MaxLevel => levels != null ? levels.Length : 0;

        public int GetTotalCost(int upToLevel)
        {
            if (levels == null) return 0;

            int total = 0;
            int maxIndex = Mathf.Min(upToLevel, levels.Length - 1);

            for(int i = 0; i <= maxIndex; i++)
            {
                total += levels[i].cost;
            }

            return total;
        }

        public int GetUpgradeCost(int currentLevel)
        {
            int nextLevel = currentLevel + 1;

            if(nextLevel >= MaxLevel)
            {
                return -1;
            }

            return levels[nextLevel].cost;
        }

        public bool CanUpgrade(int currentLevel)
        {
            return currentLevel + 1 < MaxLevel;
        }

        // ============================
        // VALIDATION (Editor only)
        // ============================

        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrEmpty(towerName))
            {
                errorMessage = "Tower name is empty";
                return false;
            }

            if (levels == null || levels.Length != 3)
            {
                errorMessage = $"Levels array must have exactly 3 elements, has {levels?.Length ?? 0}";
                return false;
            }

            for (int i = 0; i < levels.Length; i++)
            {
                if (levels[i].cost <= 0)
                {
                    errorMessage = $"Level {i + 1} cost must be > 0";
                    return false;
                }

                if (levels[i].attackCooldown <= 0)
                {
                    errorMessage = $"Level {i + 1} attackCooldown must be > 0";
                    return false;
                }

                if (levels[i].range <= 0)
                {
                    errorMessage = $"Level {i + 1} range must be > 0";
                    return false;
                }
            }

            // Kiểm tra cost tăng dần
            for (int i = 1; i < levels.Length; i++)
            {
                if (levels[i].cost < levels[i - 1].cost)
                {
                    errorMessage = $"Level {i + 1} cost ({levels[i].cost}) should be >= Level {i} cost ({levels[i - 1].cost})";
                    return false;
                }
            }

            errorMessage = "";
            return true;
        }

        public override string ToString()
        {
            if (levels == null || levels.Length == 0)
            {
                return $"[TowerData] {towerName} (no levels)";
            }

            string result = $"[TowerData] {towerName}\n";
            for (int i = 0; i < levels.Length; i++)
            {
                var lv = levels[i];
                result += $"  Lv{i + 1}: DMG={lv.damage} CD={lv.attackCooldown}s " +
                          $"RNG={lv.range} Cost={lv.cost} " +
                          $"DPS={lv.BaseDPS:F1}\n";
            }

            return result;
        }
    }
}
