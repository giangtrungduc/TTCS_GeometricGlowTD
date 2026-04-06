using UnityEngine;
using TMPro;
using TowerDefense.Core;

namespace TowerDefense.UI
{
    public class HUDController : MonoBehaviour
    {
        // ============================
        // CẤU HÌNH
        // ============================

        [Header("HUD Texts")]
        [Tooltip("Text hiển thị Gold")]
        [SerializeField] private TextMeshProUGUI goldText;

        [Tooltip("Text hiển thị Lives")]
        [SerializeField] private TextMeshProUGUI livesText;

        [Tooltip("Text hiển thị Wave")]
        [SerializeField] private TextMeshProUGUI waveText;

        [Header("Wave Settings")]
        [Tooltip("Tổng số wave của level hiện tại (dùng để hiển thị X/Y)")]
        [SerializeField] private int totalWaves = 8;

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
        }

        private void OnDisable()
        {
            GameEvents.OnGoldChanged -= UpdateGoldUI;
            GameEvents.OnLivesChanged -= UpdateLivesUI;
            GameEvents.OnWaveStarted -= UpdateWaveUI;
        }

        private void Start()
        {
            UpdateGoldUI(EconomyManager.Instance != null ? EconomyManager.Instance.CurrentGold : 100);
            UpdateLivesUI(EconomyManager.Instance != null ? EconomyManager.Instance.CurrentLives : 20);
            UpdateWaveUI(0);
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

        // ============================
        // PUBLIC 
        // ============================

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
        }
    }
}