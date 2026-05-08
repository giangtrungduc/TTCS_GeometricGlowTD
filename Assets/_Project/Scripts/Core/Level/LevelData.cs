using UnityEngine;

namespace TowerDefense.Core
{
    [CreateAssetMenu(fileName = "LevelData", menuName = "TD/LevelData", order = 5)]
    public class LevelData : ScriptableObject
    {
        [Tooltip("Mã số của level, dùng để lưu trữ và xác định level trong hệ thống lưu trữ")]
        public int levelID;
        [Tooltip("Tên hiển thị của level, có thể dùng để hiển thị trong UI")]
        public string levelName;
        [Tooltip("Biểu tượng của level")]
        public Sprite iconLevel;
        [Tooltip("Hình nền của level")]
        public Sprite backgroundLevel;
    }
}
