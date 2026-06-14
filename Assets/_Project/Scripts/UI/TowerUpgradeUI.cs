using TowerDefense.Core;
using TowerDefense.Towers;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerDefense.UI
{
    public class TowerUpgradeUI : MonoBehaviour
    {
        [Header("Upgrade Mode UI")]
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI levelText;
        [SerializeField] private Image cooldownFillImage;
        [SerializeField] private GameObject upgradeInformationPanel;
        [SerializeField] private TextMeshProUGUI upgDamageText;
        [SerializeField] private TextMeshProUGUI upgFireRateText;
        [SerializeField] private TextMeshProUGUI upgRangeText;
        [SerializeField] private TextMeshProUGUI upgSlowEffectText;
        
        [Header("Buttons")]
        [SerializeField] private Button upgradeButton;
        [SerializeField] private TextMeshProUGUI upgradePriceText;
        [SerializeField] private Button sellButton;
        [SerializeField] private TextMeshProUGUI sellPriceText;

        private BuildSlot currentSlot;

        private void Start()
        {
            if (upgradeButton != null) upgradeButton.onClick.AddListener(OnUpgradeClicked);
            if (sellButton != null) sellButton.onClick.AddListener(OnSellClicked);
        }

        private void Update()
        {
            if (currentSlot == null || currentSlot.currentTower == null) return;

            if (cooldownFillImage != null)
            {
                cooldownFillImage.fillAmount = currentSlot.currentTower.CooldownPercent;
            }

            if (!currentSlot.currentTower.IsMaxLevel && upgradeButton != null)
            {
                upgradeButton.interactable = (GameManager.Instance.Gold >= currentSlot.currentTower.NextStats.cost);
            }
        }

        public void Setup(BuildSlot slot)
        {
            currentSlot = slot;
            if (upgradeInformationPanel != null) upgradeInformationPanel.SetActive(true);
            RefreshUpgradePanel();
        }

        private void RefreshUpgradePanel()
        {
            if (currentSlot == null || currentSlot.currentTower == null) return;

            TowerBase tower = currentSlot.currentTower;
            TowerUpgradeLevel stats = tower.CurrentStats;

            if (nameText != null) nameText.text = tower.Data.towerName;
            if (levelText != null) levelText.text = $"Lv.{tower.CurrentLevel + 1}";

            if (upgDamageText != null) upgDamageText.text = $"{stats.damage}";
            if (upgFireRateText != null) upgFireRateText.text = $"{stats.fireRate}/s";
            if (upgRangeText != null) upgRangeText.text = $"{stats.range}";
            
            if (upgSlowEffectText != null)
                upgSlowEffectText.text = tower.Data.isSlowTower ? $"{stats.slowPercent * 100}%" : "0%";

            if (tower.IsMaxLevel)
            {
                if (upgradePriceText != null) upgradePriceText.text = "MAX";
                if (upgradeButton != null) upgradeButton.interactable = false;
            }
            else
            {
                int upgCost = tower.NextStats.cost;
                if (upgradePriceText != null) upgradePriceText.text = upgCost.ToString();
                if (upgradeButton != null) upgradeButton.interactable = (GameManager.Instance.Gold >= upgCost);
            }

            int refundAmount = tower.TotalGoldSpent / 2;
            if (sellPriceText != null) sellPriceText.text = refundAmount.ToString();
        }

        private void OnUpgradeClicked()
        {
            if (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver || currentSlot == null) return;

            if (currentSlot.UpgradeTower())
            {
                if(AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayButtonClick();
                }
                RefreshUpgradePanel();
            }
            else
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            }
        }

        private void OnSellClicked()
        {
            if (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver || currentSlot == null) return;

            if (currentSlot.SellTower())
            {
                if(AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlayButtonClick();
                }
                TowerUIController.Instance.CloseMenu();
            }
            else
            {
                if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();
            }
        }
    }
}