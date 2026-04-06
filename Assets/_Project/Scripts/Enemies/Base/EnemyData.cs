using UnityEngine;

namespace TowerDefense.Enemies
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "TD/Enemy Data", order = 2)]
    public class EnemyData : ScriptableObject
    {
        // ===========================
        // IDENTITY
        // ===========================

        [Header("Identity")]

        [Tooltip("Tên enemy hiển thị trong HUD và debug")]
        public string enemyName = "New Enemy";

        [Tooltip("Mô tả ngắn — chỉ dùng trong Editor, không hiện trong game")]
        [TextArea(2, 3)]
        public string description = "";

        [Tooltip("Sprite hiển thị của enemy")]
        public Sprite enemySprite;

        [Tooltip("Màu đặc trưng — dùng để tint sprite, phân biệt loại enemy")]
        public Color themeColor = Color.white;

        // ===========================
        // STATS
        // ===========================

        [Header("Stats")]

        [Tooltip("HP tối đa")]
        [Min(1f)]
        public float maxHp = 100f;

        [Tooltip("Tốc độ di chuyển (world units/giây)")]
        [Min(0.1f)]
        public float moveSpeed = 2f;

        // ===========================
        // ECONOMY
        // ===========================

        [Header("Economy")]

        [Tooltip("Gold thưởng khi tiêu diệt")]
        [Min(0)]
        public int goldReward = 10;

        [Tooltip("Số mạng người chơi mất khi enemy qua đích")]
        [Min(1)]
        public int livesCost = 1;

        // ===========================
        // VALIDATION
        // ===========================

        /// <summary>
        /// Kiểm tra tính hợp lệ của data.
        /// Gọi bởi EnemyBase.Awake() trong Editor, hoặc bởi build pipeline.
        /// </summary>
        public bool Validate(out string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(enemyName))
            {
                errorMessage = "enemyName trống";
                return false;
            }
            if (maxHp <= 0f)
            {
                errorMessage = $"maxHp phải > 0 (hiện tại: {maxHp})";
                return false;
            }
            if (moveSpeed <= 0f)
            {
                errorMessage = $"moveSpeed phải > 0 (hiện tại: {moveSpeed})";
                return false;
            }
            if (goldReward < 0)
            {
                errorMessage = $"goldReward không được âm (hiện tại: {goldReward})";
                return false;
            }
            if (livesCost < 1)
            {
                errorMessage = $"livesCost phải >= 1 (hiện tại: {livesCost})";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

#if UNITY_EDITOR
        /// <summary>Tự động validate khi thay đổi giá trị trong Inspector.</summary>
        private void OnValidate()
        {
            if (!Validate(out string err))
            {
                Debug.LogWarning($"[EnemyData] '{name}': {err}", this);
            }
        }
#endif

        public override string ToString() => $"[EnemyData] {enemyName} | HP:{maxHp} Spd:{moveSpeed} Gold:{goldReward} Lives:{livesCost}";
    }
}