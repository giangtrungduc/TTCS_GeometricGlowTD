using UnityEngine;
using UnityEngine.SceneManagement;

namespace TowerDefense.Core
{
    public class GameManager : ManagerBase<GameManager>
    {
        [Header("Cấu hình level")]
        [Tooltip("Tên level hiện tại, dùng để lưu PlayerPrefs.")]
        [SerializeField] private LevelData currentLevelData;

        [Tooltip("Số mạng bắt đầu.")]
        [SerializeField] private int startingLives = 20;

        [Tooltip("Số gold bắt đầu.")]
        [SerializeField] private int startingGold = 200;

        public GameState CurrentState { get; private set; } = GameState.Playing;

        private int wavesCleared;

        protected override void OnAwake()
        {
            base.OnAwake();
        }

        private void Start()
        {
            Time.timeScale = 1f;

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.Initialize(startingGold, startingLives);
            }

            wavesCleared = 0;
            ChangeState(GameState.Playing);
        }

        private void OnEnable()
        {
            GameEvents.OnWaveCompleted += HandleWaveCompleted;
            GameEvents.OnAllWavesCleared += HandleAllWavesCleared;
            GameEvents.OnLivesChanged += HandleLivesChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnWaveCompleted -= HandleWaveCompleted;
            GameEvents.OnAllWavesCleared -= HandleAllWavesCleared;
            GameEvents.OnLivesChanged -= HandleLivesChanged;
        }

        public void ChangeState(GameState newState)
        {
            if (CurrentState == newState) return;

            CurrentState = newState;

            switch (newState)
            {
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

        public void TogglePause()
        {
            if (CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Paused);
                return;
            }

            if (CurrentState == GameState.Paused)
            {
                ChangeState(GameState.Playing);
            }
        }

        public void RestartLevel()
        {
            Time.timeScale = 1f;
            GameEvents.ClearAllEvents();

            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }

        private void HandleWaveCompleted(int waveIndex)
        {
            if (waveIndex < 0) return;

            wavesCleared = waveIndex + 1;
        }

        private void HandleAllWavesCleared()
        {
            if (CurrentState != GameState.Playing) return;

            ChangeState(GameState.Win);
        }

        private void HandleLivesChanged(int currentLives)
        {
            if (currentLives <= 0 && CurrentState == GameState.Playing)
            {
                ChangeState(GameState.Lose);
            }
        }

        private void HandleWin()
        {
            int currentLives = 0;

            if (EconomyManager.Instance != null)
            {
                currentLives = EconomyManager.Instance.CurrentLives;
            }

            LevelResult result = new LevelResult(currentLevelData.levelName, currentLives, wavesCleared);

             SaveManager.SaveStars(currentLevelData.levelID, result.starCount);
            GameEvents.RaiseLevelCompleted(result);
        }

        private void HandleLose()
        {
            LevelResult result = new LevelResult(currentLevelData.levelName, 0, wavesCleared);

            GameEvents.RaiseLevelCompleted(result);
        }
        public void QuitToLevelSelect()
        {
            Time.timeScale = 1f;
            GameEvents.ClearAllEvents();

            SceneManager.LoadScene("LevelSelected");
        }
        protected override void OnDestroy()
        {
            GameEvents.ClearAllEvents();
            base.OnDestroy();
        }
    }
}
