using UnityEngine;
using UnityEngine.UI;
using TowerDefense.Towers;

namespace TowerDefense.UI
{
    public class TowerUIController : MonoBehaviour
    {
        public static TowerUIController Instance { get; private set; }

        [Header("Main UI Components")]
        [SerializeField] private GameObject shopPanel;
        [SerializeField] private Button closeButton;

        [Header("Sub UI Controllers")]
        [SerializeField] private TowerBuyUI buyUI;
        [SerializeField] private TowerUpgradeUI upgradeUI;

        public BuildSlot CurrentSlot { get; private set; }

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            CloseMenu();
            if (closeButton != null) closeButton.onClick.AddListener(CloseMenu);
        }

        public void OpenMenu(BuildSlot slot)
        {
            CurrentSlot = slot;
            if (shopPanel != null) shopPanel.SetActive(true);

            if (slot.currentTower == null)
            {
                // Ô Trống -> Bật chế độ Xây Mới
                buyUI.gameObject.SetActive(true);
                upgradeUI.gameObject.SetActive(false);
                buyUI.Setup(slot);
            }
            else
            {
                // Có Tháp -> Bật chế độ Nâng Cấp / Bán
                buyUI.gameObject.SetActive(false);
                upgradeUI.gameObject.SetActive(true);
                upgradeUI.Setup(slot);
            }
        }

        public void CloseMenu()
        {
            if (shopPanel != null) shopPanel.SetActive(false);
            if (CurrentSlot != null && CurrentSlot.currentTower != null)
            {
                CurrentSlot.currentTower.SetRangeIndicatorActive(false);
            }
            CurrentSlot = null;
            
            buyUI.gameObject.SetActive(false);
            upgradeUI.gameObject.SetActive(false);
        }
    }
}