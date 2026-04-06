using UnityEngine;

namespace TowerDefense.Core
{
    public class GameManager : ManagerBase<GameManager>
    {
        // ============================
        // CẤU HÌNH LEVEL
        // ============================
        [Header("Level Settings")]
        [Tooltip("Tên level hiện tại, dùng để lưu PlayerPrefs")]
        [SerializeField] private string levelName = "Level1";

        [Tooltip("Số mạng bắt đầu")]
        [SerializeField] private int startingLives = 20;

        [Tooltip("Số gold bắt đầu")]
        [SerializeField] private int startingGold = 200;

        // Trạng thái hiện tại của Game
        public GameState CurrentState { get; private set; } = GameState.Prepare;

        // Số wave đã hoàn thành
        private int wavesCleared = 0;

        // ============================
        // INIT
        // ============================

        protected override void OnAwake()
        {
            base.OnAwake();
        }
        private void Start()
        {
            if(EconomyManager.Instance != null)
            {
                EconomyManager.Instance.Initialize(startingGold, startingLives);
            }

            ChangeState(GameState.Prepare);
        }

        // ============================
        // EVENT SUBSCRIBE
        // ============================
        private void OnEnable()
        {
            GameEvents.OnAllWavesCleared += HandleAllWavesCleared;
            GameEvents.OnLivesChanged += HandleLivesChanged;
            GameEvents.OnWaveCompleted += HandleWaveCompleted;
        }
        private void OnDisable()
        {
            GameEvents.OnAllWavesCleared -= HandleAllWavesCleared;
            GameEvents.OnLivesChanged -= HandleLivesChanged;
            GameEvents.OnWaveCompleted -= HandleWaveCompleted;
        }

        // ============================
        // PUBLIC METHODS
        // ============================
        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            GameState previousState = CurrentState;
            CurrentState = newState;

            switch (newState)
            {
                case GameState.Prepare:
                    Time.timeScale = 1f;
                    break;
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f; 
                    break;
                case GameState.Win:
                    Time.timeScale = 0f;
                    HandleWin();
                    break;
                case GameState.Lose:
                    Time.timeScale = 0f;
                    HandleLose();
                    break;
            }

            GameEvents.RaiseGameStateChanged(newState);
        }
        public void StartPlaying()
        {
            if(CurrentState == GameState.Prepare)
            {
                ChangeState(GameState.Playing);
            }
        }
        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
            }
            else if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }
        public void RestartLevel()
        {
            Time.timeScale = 1f;
            GameEvents.ClearAllEvents();
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }

        // ============================
        // EVENT HANDLERS
        // ============================

        private void HandleAllWavesCleared()
        {
            if (CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Win);
            }
        }
        private void HandleLivesChanged(int currentLives)
        {
            if (currentLives <= 0 && CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Lose);
            }
        }
        private void HandleWaveCompleted(int waveIndex)
        {
            wavesCleared = waveIndex + 1;
        }

        // ============================
        // WIN / LOSE LOGIC
        // ============================
        
        // Xử lý khi thắng
        private void HandleWin()
        {
            int currentLives = 0;
            if(EconomyManager.Instance != null)
            {
                currentLives = EconomyManager.Instance.CurrentLives;
            }

            LevelResult result = new LevelResult(levelName, currentLives, wavesCleared);

            SaveResult(result);

            GameEvents.RaiseLevelCompleted(result);
        }

        // Xử lý khi thua
        private void HandleLose()
        {
            LevelResult result = new LevelResult(levelName, 0, wavesCleared);

            GameEvents.RaiseLevelCompleted(result);
        }

        // Lưu số sao vào PlayerPrefs (chỉ ghi đè nếu tốt hơn).
        private void SaveResult(LevelResult result)
        {
            string key = $"{result.levelName}_Stars";
            int previousBest = PlayerPrefs.GetInt(key);

            if(result.starCount > previousBest)
            {
                PlayerPrefs.SetInt(key, result.starCount);
                PlayerPrefs.Save();
            }
        }

        // ============================
        // CLEANUP
        // ============================
        protected override void OnDestroy()
        {
            GameEvents.ClearAllEvents();
            base.OnDestroy();
        }
    }
}
