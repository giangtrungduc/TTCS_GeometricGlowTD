using System;
using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Bộ não của game: Quản lý Tiền (Gold), Máu (Lives), và Trạng thái Thắng/Thua.
    /// Dùng Singleton để bất kỳ script nào cũng có thể gọi: GameManager.Instance...
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("Starting Stats")]
        [SerializeField] private int startingGold = 300;
        [SerializeField] private int startingLives = 20;

        public int Gold { get; private set; }
        public int Lives { get; private set; }
        public int MaxLives { get; private set; } // Dùng để tính % máu khi tính Sao
        public bool IsGameOver { get; private set; }
        public bool IsPaused { get; private set; } // Biến trạng thái Pause

        // Các Event cơ bản để UI lắng nghe và cập nhật chữ
        public event Action<int> OnGoldChanged;
        public event Action<int> OnLivesChanged;
        public event Action OnGameOver;
        public event Action<int> OnVictory; // Trả về số sao đạt được
        public event Action<bool> OnPauseToggled; // Kích hoạt để UI hiện/ẩn bảng Tạm dừng

        [Header("Level Progression")]
        [Tooltip("Tên của Scene tiếp theo (Ví dụ: Level2). Bỏ trống nếu đây là màn cuối.")]
        [SerializeField] private string nextLevelName;
        public string NextLevelName => nextLevelName;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            Gold = startingGold;
            Lives = startingLives;
            MaxLives = startingLives;
            IsGameOver = false;
            IsPaused = false;
            Time.timeScale = 1f; // Chống kẹt freeze khi load lại Scene từ trạng thái Pause

            OnGoldChanged?.Invoke(Gold);
            OnLivesChanged?.Invoke(Lives);
        }

        public void AddGold(int amount)
        {
            if (IsGameOver || amount <= 0) return;
            Gold += amount;
            OnGoldChanged?.Invoke(Gold);
        }

        public bool SpendGold(int amount)
        {
            if (IsGameOver || Gold < amount) return false;
            Gold -= amount;
            OnGoldChanged?.Invoke(Gold);
            return true;
        }

        public void LoseLife(int amount)
        {
            if (IsGameOver) return;
            Lives -= amount;
            OnLivesChanged?.Invoke(Lives);

            if (Lives <= 0)
            {
                Lives = 0;
                GameOver();
            }
        }

        private void GameOver()
        {
            IsGameOver = true;
            OnGameOver?.Invoke();
            Debug.Log("--- GAME OVER ---");
        }

        public void Victory()
        {
            if (IsGameOver) return;
            IsGameOver = true;

            // --- TÍNH VÀ LƯU SAO ---
            int stars = 1;
            if (Lives >= MaxLives) stars = 3; // Hoàn hảo không xước xát
            else if (Lives >= MaxLives / 2) stars = 2; // Còn trên 50% máu

            // Lấy tên Scene hiện tại làm tên Level để lưu
            string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
            SaveManager.SaveLevelStars(currentSceneName, stars);

            OnVictory?.Invoke(stars);
            Debug.Log($"--- VICTORY --- Đạt được {stars} Sao!");
        }

        public void TogglePause()
        {
            // Thắng hoặc thua rồi thì cấm Pause
            if (IsGameOver) return; 

            IsPaused = !IsPaused;
            
            // Đóng băng hoặc Mở băng thời gian
            Time.timeScale = IsPaused ? 0f : 1f;

            // Bắn tín hiệu để UI mở/đóng bảng Pause
            OnPauseToggled?.Invoke(IsPaused);
            
            Debug.Log(IsPaused ? "Game Paused" : "Game Resumed");
        }
    }
}
