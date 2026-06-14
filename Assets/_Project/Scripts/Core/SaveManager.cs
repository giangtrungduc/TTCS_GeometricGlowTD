using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Lưu trữ tiến trình của người chơi bằng PlayerPrefs siêu nhanh.
    /// Không cần file text phức tạp.
    /// </summary>
    public static class SaveManager
    {
        // Hàm lưu số sao đạt được của một level cụ thể
        public static void SaveLevelStars(string levelName, int stars)
        {
            // Lấy số sao cũ (mặc định là 0 nếu chưa chơi)
            int currentStars = PlayerPrefs.GetInt(levelName + "_Stars", 0);
            
            // Kỷ lục mới: Chỉ lưu nếu số sao mới lấy được cao hơn kỷ lục cũ
            if (stars > currentStars)
            {
                PlayerPrefs.SetInt(levelName + "_Stars", stars);
                PlayerPrefs.Save();
            }
        }

        // Lấy kỷ lục số sao hiện tại của một level (dùng cho Màn Hình Chọn Màn)
        public static int GetLevelStars(string levelName)
        {
            return PlayerPrefs.GetInt(levelName + "_Stars", 0);
        }

        // Kiểm tra xem màn chơi có được mở khóa chưa
        public static bool IsLevelUnlocked(string previousLevelName)
        {
            // Nếu không có yêu cầu màn trước (VD: Màn 1) thì luôn mở khóa
            if (string.IsNullOrEmpty(previousLevelName)) return true;

            // Nếu màn trước đó chưa chơi (0 sao) thì khóa. Đạt >= 1 sao thì mở.
            return GetLevelStars(previousLevelName) > 0;
        }
    }
}
