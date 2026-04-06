using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TowerDefense.Core; // Thêm dòng này để gọi EconomyManager

namespace TowerDefense.Towers
{
    public class RadialMenuOption : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private TextMeshProUGUI nameText;
        [SerializeField] private TextMeshProUGUI priceText;

        public void SetupBuild(TowerData data, BuildSlot slot)
        {
            if (data == null) return;

            TowerLevelData level0 = data.GetLevel(0);

            if (iconImage != null && data.icon != null)
            {
                iconImage.sprite = data.icon;
                iconImage.enabled = true;
            }

            if (nameText != null) nameText.text = data.towerName;
            if (priceText != null) priceText.text = $"{level0.cost}g";

            // Tự động bỏ qua check gold nếu chưa gắn EconomyManager (để test cho dễ)
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

        // ĐÃ NÂNG CẤP: Thêm tham số isMaxLevel
        public void SetupManage(string label, System.Action action, int goldValue, bool interactable = true, bool isMaxLevel = false)
        {
            if (iconImage != null) iconImage.enabled = false;
            if (nameText != null) nameText.text = label;

            if (priceText != null)
            {
                if (isMaxLevel)
                {
                    // NẾU LÀ MAX LEVEL: Ghi chữ MAX và đổi màu xám
                    priceText.text = "MAX";
                    priceText.color = Color.gray;
                    if (nameText != null) nameText.text = "Tối đa";
                }
                else if (goldValue > 0)
                {
                    priceText.text = $"-{goldValue}g";
                    priceText.color = new Color(1f, 0.4f, 0.4f); // đỏ nhạt
                }
                else
                {
                    priceText.text = $"+{Mathf.Abs(goldValue)}g";
                    priceText.color = new Color(0.4f, 1f, 0.5f); // xanh lá
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

            CanvasGroup cg = GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = interactable ? 1f : 0.45f;
        }
    }
}