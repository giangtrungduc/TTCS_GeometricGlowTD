using TowerDefense.Core;
using TowerDefense.Towers;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace TowerDefense.UI
{
    public class TowerBuyUI : MonoBehaviour
    {
        [Header("Prefabs Tháp")]
        [SerializeField] private GameObject basicPrefab;
        [SerializeField] private GameObject rapidPrefab;
        [SerializeField] private GameObject sniperPrefab;
        [SerializeField] private GameObject slowPrefab;

        [Header("Buy Mode UI")]
        [SerializeField] private Button[] towerButtons; 
        [SerializeField] private TextMeshProUGUI[] buyPriceTexts;
        [SerializeField] private GameObject buyInformationPanel;
        [SerializeField] private TextMeshProUGUI buyDamageText;
        [SerializeField] private TextMeshProUGUI buyFireRateText;
        [SerializeField] private TextMeshProUGUI buyRangeText;
        [SerializeField] private TextMeshProUGUI buySlowEffectText;

        private BuildSlot currentSlot;
        private int selectedTowerIndex = -1;

        private void Start()
        {
            if (towerButtons != null && towerButtons.Length >= 4)
            {
                towerButtons[0].onClick.AddListener(() => OnBuyTowerClicked(0, basicPrefab));
                towerButtons[1].onClick.AddListener(() => OnBuyTowerClicked(1, rapidPrefab));
                towerButtons[2].onClick.AddListener(() => OnBuyTowerClicked(2, sniperPrefab));
                towerButtons[3].onClick.AddListener(() => OnBuyTowerClicked(3, slowPrefab));
            }
        }

        public void Setup(BuildSlot slot)
        {
            currentSlot = slot;
            selectedTowerIndex = -1; // Reset lựa chọn
            if (buyInformationPanel != null) buyInformationPanel.SetActive(false);
            UpdateBuyPrices();
        }

        private void UpdateBuyPrices()
        {
            SetBuyPriceText(0, basicPrefab);
            SetBuyPriceText(1, rapidPrefab);
            SetBuyPriceText(2, sniperPrefab);
            SetBuyPriceText(3, slowPrefab);
        }

        private void SetBuyPriceText(int index, GameObject prefab)
        {
            if (buyPriceTexts != null && index < buyPriceTexts.Length && buyPriceTexts[index] != null)
            {
                if (prefab != null)
                {
                    TowerBase tower = prefab.GetComponent<TowerBase>();
                    if (tower != null && tower.Data != null)
                    {
                        buyPriceTexts[index].text = tower.Data.levels[0].cost.ToString();
                    }
                }
            }
        }

        private void OnBuyTowerClicked(int index, GameObject prefab)
        {
            if (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver || currentSlot == null || prefab == null) return;
            if (AudioManager.Instance != null) AudioManager.Instance.PlayButtonClick();

            TowerBase tower = prefab.GetComponent<TowerBase>();
            if (tower == null || tower.Data == null) return;

            if (selectedTowerIndex == index)
            {
                // Xây tháp
                if (currentSlot.BuildTower(prefab))
                {
                    TowerUIController.Instance.CloseMenu();
                }
                else
                {
                    Debug.Log("Không đủ vàng để xây tháp này!");
                }
            }
            else
            {
                // Hiển thị thông tin
                selectedTowerIndex = index;
                if (buyInformationPanel != null) buyInformationPanel.SetActive(true);

                TowerUpgradeLevel stats = tower.Data.levels[0];
                if (buyDamageText != null) buyDamageText.text = $"{stats.damage}";
                if (buyFireRateText != null) buyFireRateText.text = $"{stats.fireRate}/s";
                if (buyRangeText != null) buyRangeText.text = $"{stats.range}";
                
                if (buySlowEffectText != null)
                {
                    buySlowEffectText.text = tower.Data.isSlowTower ? $"{stats.slowPercent * 100}%" : "0%";
                }
            }
        }
    }
}