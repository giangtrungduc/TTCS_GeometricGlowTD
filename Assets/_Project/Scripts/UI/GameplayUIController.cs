using TowerDefense.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

namespace TowerDefense.UI
{
    public class GameplayUIController : MonoBehaviour
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI coinText;
        [SerializeField] private TextMeshProUGUI liveText;
        [SerializeField] private TextMeshProUGUI waveText;
        
        [Header("Start Wave Early")]
        [SerializeField] private Button startWaveButton;
        [SerializeField] private TextMeshProUGUI bonusGoldText; // Chữ nằm cạnh nút StartWave

        [Header("Pause Controls")]
        [SerializeField] private Button pauseButton;
        [SerializeField] private GameObject pausePanel;
        
        [Header("Pause Menu Elements")]
        [SerializeField] private Slider bgmSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Button resumeButton;
        [SerializeField] private Button homeButton;
        [SerializeField] private Button replayButton;

        private WaveManager waveManager;

        private void Start()
        {
            waveManager = FindAnyObjectByType<WaveManager>();

            // Đăng ký sự kiện ở Start để chắc chắn GameManager.Instance đã tồn tại (sau Awake)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged += UpdateGold;
                GameManager.Instance.OnLivesChanged += UpdateLives;
                GameManager.Instance.OnPauseToggled += HandlePauseToggled;

                // Đồng bộ dữ liệu ban đầu
                UpdateGold(GameManager.Instance.Gold);
                UpdateLives(GameManager.Instance.Lives);
            }

            // Setup Pause UI
            if (pausePanel != null) pausePanel.SetActive(false);
            
            // Gán các sự kiện cho nút cơ bản
            if (pauseButton != null) pauseButton.onClick.AddListener(OnPauseClicked);
            if (resumeButton != null) resumeButton.onClick.AddListener(OnResumeClicked);
            if (homeButton != null) homeButton.onClick.AddListener(OnHomeClicked);
            if (replayButton != null) replayButton.onClick.AddListener(OnReplayClicked);

            if (startWaveButton != null)
            {
                startWaveButton.onClick.AddListener(OnStartWaveClicked);
            }

            // Setup Slider Âm Thanh y như Main Menu
            if (AudioManager.Instance != null)
            {
                if (bgmSlider != null)
                {
                    bgmSlider.value = AudioManager.Instance.GetBGMVolume();
                    bgmSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetBGMVolume(val));
                }
                if (sfxSlider != null)
                {
                    sfxSlider.value = AudioManager.Instance.GetSFXVolume();
                    sfxSlider.onValueChanged.AddListener(val => AudioManager.Instance.SetSFXVolume(val));
                }
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnGoldChanged -= UpdateGold;
                GameManager.Instance.OnLivesChanged -= UpdateLives;
                GameManager.Instance.OnPauseToggled -= HandlePauseToggled;
            }
        }

        private void Update()
        {
            if (waveManager == null) return;

            // Cập nhật Wave Text
            if (waveText != null)
            {
                // CurrentWaveIndex đếm từ 0, nên hiển thị +1. Nếu game over / win thì giữ nguyên số cuối.
                int displayWave = Mathf.Min(waveManager.CurrentWaveIndex + 1, waveManager.TotalWaves);
                waveText.text = $"{displayWave} / {waveManager.TotalWaves}";
            }

            // Xử lý logic Ẩn/Hiện nút Gọi Wave sớm
            if (waveManager.IsWaitingForWave)
            {
                if (startWaveButton != null && !startWaveButton.gameObject.activeSelf)
                    startWaveButton.gameObject.SetActive(true);

                if (bonusGoldText != null)
                {
                    if (!bonusGoldText.gameObject.activeSelf) bonusGoldText.gameObject.SetActive(true);
                    bonusGoldText.text = $"+{waveManager.CurrentBonusGold}";
                }
            }
            else
            {
                if (startWaveButton != null && startWaveButton.gameObject.activeSelf)
                    startWaveButton.gameObject.SetActive(false);

                if (bonusGoldText != null && bonusGoldText.gameObject.activeSelf)
                    bonusGoldText.gameObject.SetActive(false);
            }
        }

        private void UpdateGold(int amount)
        {
            if (coinText != null) coinText.text = amount.ToString();
        }

        private void UpdateLives(int amount)
        {
            if (liveText != null) liveText.text = amount.ToString();
        }

        private void HandlePauseToggled(bool isPaused)
        {
            if (pausePanel != null)
            {
                pausePanel.SetActive(isPaused);
            }

            // Tự động đóng bảng Xây Tháp nếu đang mở lúc Pause
            if (isPaused && TowerUIController.Instance != null)
            {
                TowerUIController.Instance.CloseMenu();
            }
        }

        // --- HÀM CHO CÁC NÚT BẤM ---

        private void OnStartWaveClicked()
        {
            if (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver) return;

            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            if (waveManager != null)
            {
                waveManager.StartNextWaveEarly();
            }
        }

        private void OnPauseClicked()
        {
            // TogglePause sẽ bắn event OnPauseToggled, từ đó HandlePauseToggled sẽ lo vụ mở Panel
            GameManager.Instance.TogglePause();
        }

        private void OnResumeClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            GameManager.Instance.TogglePause();
        }

        private void OnHomeClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            
            // Nếu đang pause (TimeScale = 0) thì phải xả băng trước khi load scene mới
            if (GameManager.Instance.IsPaused)
            {
                GameManager.Instance.TogglePause();
            }
            
            SceneManager.LoadScene("LevelSelect");
        }

        private void OnReplayClicked()
        {
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            
            if (GameManager.Instance.IsPaused)
            {
                GameManager.Instance.TogglePause();
            }

            // Load lại chính cái Scene hiện tại
            string currentSceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(currentSceneName);
        }
    }
}
