using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefense.Core;
using TowerDefense.Towers;

namespace TowerDefense.UI
{
    public class RadialMenuOption : MonoBehaviour
    {
        [Header("Tham chiếu UI")]
        [Tooltip("Button chính của option.")]
        [SerializeField] private Button button;

        [Tooltip("Icon hiển thị cho tower khi ở chế độ xây.")]
        [SerializeField] private Image iconImage;

        [Tooltip("Tên option hoặc tên tower.")]
        [SerializeField] private TextMeshProUGUI nameText;

        [Tooltip("Giá xây, giá nâng cấp hoặc giá trị bán.")]
        [SerializeField] private TextMeshProUGUI priceText;

        private Color defaultPriceColor = Color.white;

        private void Awake()
        {
            if (priceText != null)
            {
                defaultPriceColor = priceText.color;
            }
        }

        public void SetupBuild(TowerData data, BuildSlot slot)
        {
            if (data == null || slot == null || button == null) return;

            TowerLevelData level0 = data.GetLevel(0);

            if (iconImage != null)
            {
                iconImage.sprite = data.icon;
                iconImage.enabled = data.icon != null;
            }

            if (nameText != null)
            {
                nameText.text = data.towerName;
            }

            if (priceText != null)
            {
                priceText.text = $"{level0.cost}g";
                priceText.color = defaultPriceColor;
            }

            bool canAfford = true;

            if (EconomyManager.Instance != null)
            {
                canAfford = EconomyManager.Instance.CurrentGold >= level0.cost;
            }

            SetInteractable(canAfford);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => slot.PlaceTower(data));

            gameObject.SetActive(true);
        }

        public void SetupManage(
            string label,
            System.Action action,
            int goldValue,
            bool interactable = true,
            bool isMaxLevel = false
        )
        {
            if (button == null) return;

            if (iconImage != null)
            {
                iconImage.enabled = false;
                iconImage.sprite = null;
            }

            if (nameText != null)
            {
                nameText.text = isMaxLevel ? "Tối đa" : label;
            }

            if (priceText != null)
            {
                if (isMaxLevel)
                {
                    priceText.text = "MAX";
                    priceText.color = Color.gray;
                }
                else if (goldValue > 0)
                {
                    priceText.text = $"-{goldValue}g";
                    priceText.color = new Color(1f, 0.4f, 0.4f);
                }
                else
                {
                    priceText.text = $"+{Mathf.Abs(goldValue)}g";
                    priceText.color = new Color(0.4f, 1f, 0.5f);
                }
            }

            SetInteractable(interactable);

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(() => action?.Invoke());

            gameObject.SetActive(true);
        }

        private void SetInteractable(bool interactable)
        {
            if (button == null) return;

            button.interactable = interactable;

            CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = interactable ? 1f : 0.45f;
            }
        }
    }
}