using UnityEngine;
using UnityEngine.UI;
using TowerDefense.Core;
using UnityEngine.SceneManagement;

namespace TowerDefense.UI
{
    public class PausePanelUI : MonoBehaviour
    {
        [Tooltip("Panel hiển thị khi game đang tạm dừng.")]
        [SerializeField] private GameObject pausePanel;
        
        [SerializeField] private Slider musicVolumeSlider;
        [SerializeField] private Slider sfxVolumeSlider;
        [Tooltip("Nút dùng để tạm dừng game.")]
        [SerializeField] private Button pauseButton;

        [Tooltip("Nút dùng để tiếp tục game.")]
        [SerializeField] private Button resumeButton;

        [Tooltip("Nút dùng để chơi lại level hiện tại.")]
        [SerializeField] private Button restartButton;
        [Tooltip("Nút dùng để thoát về menu.")]
        [SerializeField] private Button quitButton;

        private void Awake()
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(false);
            }
        }

        private void Start()
        {
            if (GameManager.Instance != null)
            {
                RefreshUI(GameManager.Instance.CurrentState);
            }
            else
            {
                RefreshUI(GameState.Playing);
            }
            GameEvents.OnGameStateChanged += HandleGameStateChanged;

            if (pauseButton != null)
            {
                pauseButton.onClick.AddListener(OnPauseClicked);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.AddListener(OnResumeClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.AddListener(OnRestartClicked);
            }
            if (quitButton != null)
            {
                quitButton.onClick.AddListener(OnQuitClicked);
            }
            if(musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.AddListener(OnMusicChanged);
                musicVolumeSlider.value = SaveManager.LoadMusicVolume();
            }
            if(sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.AddListener(OnSFXChanged);
                sfxVolumeSlider.value = SaveManager.LoadSFXVolume();
            }
        }

        void OnDestroy()
        {
            GameEvents.OnGameStateChanged -= HandleGameStateChanged;

            if (pauseButton != null)
            {
                pauseButton.onClick.RemoveListener(OnPauseClicked);
            }

            if (resumeButton != null)
            {
                resumeButton.onClick.RemoveListener(OnResumeClicked);
            }

            if (restartButton != null)
            {
                restartButton.onClick.RemoveListener(OnRestartClicked);
            }
            if (quitButton != null)
            {
                quitButton.onClick.RemoveListener(OnQuitClicked);
            }
            if(musicVolumeSlider != null)
            {
                musicVolumeSlider.onValueChanged.RemoveListener(OnMusicChanged);
            }
            if(sfxVolumeSlider != null)
            {
                sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXChanged);
            }
        }

        private void OnPauseClicked()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.TogglePause();
        }

        private void OnResumeClicked()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.TogglePause();
        }

        private void OnRestartClicked()
        {
            if (GameManager.Instance == null) return;

            GameManager.Instance.RestartLevel();
        }
        private void OnQuitClicked()
        {
            if(GameManager.Instance == null) return;
            GameManager.Instance.QuitToLevelSelect();
        }
        private void OnMusicChanged(float value)
        {
            AudioManager.Instance.SetMusicVolume(value);
        }
        private void OnSFXChanged(float value)
        {
            AudioManager.Instance.SetSfxVolume(value);
        }
        private void HandleGameStateChanged(GameState state)
        {
            RefreshUI(state);
        }

        private void RefreshUI(GameState state)
        {
            bool isPaused = state == GameState.Paused;
            bool canPause = state == GameState.Playing;

            if (pausePanel != null)
            {
                pausePanel.SetActive(isPaused);
            }

            if (pauseButton != null)
            {
                pauseButton.interactable = canPause;
                pauseButton.gameObject.SetActive(state != GameState.Win && state != GameState.Lose);
            }

            if (resumeButton != null)
            {
                resumeButton.interactable = isPaused;
            }

            if (restartButton != null)
            {
                restartButton.interactable = isPaused;
            }
        }
    }
}
