using UnityEngine;
using TMPro;
using TowerDefense.Core;
using UnityEngine.UI;

namespace TowerDefense.UI
{
    public class HUDController : MonoBehaviour
    {
        // ============================
        // CẤU HÌNH
        // ============================

        [Header("HUD")]
        [Tooltip("Text hiển thị Gold")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Tooltip("Text hiển thị Lives")]
        [SerializeField] private TextMeshProUGUI livesText;

        [Tooltip("Text hiển thị Wave")]
        [SerializeField] private TextMeshProUGUI waveText;

        [Header("Wave Settings")]
        [Tooltip("Tổng số wave của level hiện tại (dùng để hiển thị X/Y)")]
        [SerializeField] private int totalWaves = 8;

        [Tooltip("Nút bắt đầu wave sớm để nhận thưởng")]
        [SerializeField] private Button startWaveButton;

        // ============================
        // STATE
        // ============================

        private int currentWaveIndex = 0;

        // ============================
        // UNITY LIFECYCLE
        // ============================

        private void OnEnable()
        {
            GameEvents.OnGoldChanged += UpdateGoldUI;
            GameEvents.OnLivesChanged += UpdateLivesUI;
            GameEvents.OnWaveStarted += UpdateWaveUI;
            GameEvents.OnWaveStarted += HandleWaveStarted;
            GameEvents.OnWaveCompleted += HandleWaveCompleted;
            GameEvents.OnWaveCountdownChanged += HandleWaveCountdownChanged;
            if(startWaveButton != null)
            {
                startWaveButton.onClick.AddListener(OnStartWaveButtonClicked);
            }
        }

        private void OnDisable()
        {
            GameEvents.OnGoldChanged -= UpdateGoldUI;
            GameEvents.OnLivesChanged -= UpdateLivesUI;
            GameEvents.OnWaveStarted -= UpdateWaveUI;
            GameEvents.OnWaveStarted -= HandleWaveStarted;
            GameEvents.OnWaveCompleted -= HandleWaveCompleted;
            GameEvents.OnWaveCountdownChanged -= HandleWaveCountdownChanged;
            if(startWaveButton != null)
            {
                startWaveButton.onClick.RemoveListener(OnStartWaveButtonClicked);
            }
        }

        private void Start()
        {
            if(WaveManager.Instance != null)
            {
                totalWaves = WaveManager.Instance.TotalWaves;
            }
            UpdateGoldUI(EconomyManager.Instance != null ? EconomyManager.Instance.CurrentGold : 100);
            UpdateLivesUI(EconomyManager.Instance != null ? EconomyManager.Instance.CurrentLives : 20);
            UpdateWaveUI(0);
            RefreshStartWaveButton();
        }

        // ============================
        // EVENT HANDLERS
        // ============================

        private void UpdateGoldUI(int currentGold)
        {
            if (goldText != null)
            {
                goldText.text = $"Gold: {currentGold.ToString()}";
            }
        }

        private void UpdateLivesUI(int currentLives)
        {
            if (livesText != null)
            {
                livesText.text = $"Lives: {currentLives.ToString()}";
            }
        }

        private void UpdateWaveUI(int waveIndex)
        {
            currentWaveIndex = waveIndex + 1;
            if (waveText != null)
            {
                waveText.text = $"Wave {currentWaveIndex}/{totalWaves}";
            }
        }
        private void HandleWaveStarted(int waveIndex)
        {
            RefreshStartWaveButton();
        }
        private void HandleWaveCompleted(int waveIndex)
        {
            RefreshStartWaveButton();
        }
        private void HandleWaveCountdownChanged(int waveIndex, float timeRemaining)
        {
            RefreshStartWaveButton();
        }
        private void RefreshStartWaveButton()
        {
            if(startWaveButton == null || WaveManager.Instance == null || GameManager.Instance == null) return;
            bool shouldShowButton = (WaveManager.Instance.State == WaveState.Prepare) && (GameManager.Instance.CurrentState != GameState.Paused);
            startWaveButton.gameObject.SetActive(shouldShowButton);
        }

        // ============================
        // PUBLIC 
        // ============================

        public void OnStartWaveButtonClicked()
        {
            if(WaveManager.Instance == null) return;
            WaveManager.Instance.StartNextWave();
        }

        /// <summary>
        /// Gọi thủ công khi muốn reset HUD
        /// </summary>
        public void RefreshAll()
        {
            if (EconomyManager.Instance != null)
            {
                UpdateGoldUI(EconomyManager.Instance.CurrentGold);
                UpdateLivesUI(EconomyManager.Instance.CurrentLives);
            }
            UpdateWaveUI(currentWaveIndex - 1);
            RefreshStartWaveButton();
        }
    }
}