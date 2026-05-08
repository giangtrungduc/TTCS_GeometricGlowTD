using UnityEngine;
using UnityEngine.UI;
using TowerDefense.Core;
using TowerDefense.Towers;

namespace TowerDefense.UI
{
    public class RadialMenu : MonoBehaviour
    {
        [Header("Bố cục")]
        [Tooltip("Khoảng cách menu so với kích thước ô xây.")]
        [SerializeField][Range(0.8f, 3f)] private float radiusMultiplier = 1.5f;

        [Tooltip("Góc bắt đầu sắp xếp option.")]
        [SerializeField] private float startAngle = 90f;

        [Tooltip("Bật để sắp xếp option ngược chiều kim đồng hồ.")]
        [SerializeField] private bool counterClockwise = false;

        [Header("Hiệu ứng")]
        [Tooltip("Thời gian mở option.")]
        [SerializeField][Min(0f)] private float openDuration = 0.15f;

        [Header("Tham chiếu")]
        [Tooltip("Canvas chứa radial menu.")]
        [SerializeField] private Canvas menuCanvas;

        [Tooltip("Nền của radial menu.")]
        [SerializeField] private RectTransform backgroundRect;

        [Header("Chế độ xây")]
        [Tooltip("Danh sách tower có thể xây.")]
        [SerializeField] private TowerData[] availableTowers;

        [Header("Option")]
        [Tooltip("Pool các option dùng cho menu.")]
        [SerializeField] private RadialMenuOption[] optionPool;

        private BuildSlot targetSlot;
        private float currentRadius;

        private void OnEnable()
        {
            GameEvents.OnGoldChanged += HandleGoldChanged;
        }

        private void OnDisable()
        {
            GameEvents.OnGoldChanged -= HandleGoldChanged;
        }

        public void Open(BuildSlot slot)
        {
            if (slot == null) return;

            targetSlot = slot;

            float slotMaxSide = Mathf.Max(slot.SlotSize.x, slot.SlotSize.y);
            currentRadius = slotMaxSide * radiusMultiplier;

            ScaleCanvasToSlot(slot.SlotSize);
            SetupBackground(slotMaxSide);
            SetupOptions(true);

            gameObject.SetActive(true);
        }

        public void Close()
        {
            targetSlot = null;
            gameObject.SetActive(false);
        }

        private void HandleGoldChanged(int currentGold)
        {
            if (!gameObject.activeInHierarchy) return;
            if (targetSlot == null) return;

            SetupOptions(false);
        }

        private void SetupOptions(bool animate)
        {
            if (targetSlot == null || optionPool == null) return;

            StopAllCoroutines();

            foreach (RadialMenuOption option in optionPool)
            {
                if (option != null)
                {
                    option.gameObject.SetActive(false);
                    option.transform.localScale = Vector3.one;
                }
            }

            if (!targetSlot.IsOccupied)
            {
                SetupBuildMode(animate);
            }
            else
            {
                SetupManageMode(animate);
            }
        }

        private void SetupBuildMode(bool animate)
        {
            if (availableTowers == null || optionPool == null) return;

            int count = Mathf.Min(availableTowers.Length, optionPool.Length);
            RadialMenuOption[] activeOptions = new RadialMenuOption[count];

            for (int i = 0; i < count; i++)
            {
                TowerData data = availableTowers[i];
                if (data == null || optionPool[i] == null) continue;

                optionPool[i].SetupBuild(data, targetSlot);
                activeOptions[i] = optionPool[i];
            }

            ArrangeInCircle(activeOptions, count, animate);
        }

        private void SetupManageMode(bool animate)
        {
            if (targetSlot == null || targetSlot.CurrentTower == null) return;
            if (optionPool == null || optionPool.Length < 2) return;

            TowerBase currentTower = targetSlot.CurrentTower;

            bool canUpgrade = currentTower.CanUpgrade;
            int upgradeCost = canUpgrade ? currentTower.UpgradeCost : 0;

            bool canAffordUpgrade = true;

            if (EconomyManager.Instance != null && canUpgrade)
            {
                canAffordUpgrade = EconomyManager.Instance.CurrentGold >= upgradeCost;
            }

            optionPool[0].SetupManage(
                "Nâng cấp",
                targetSlot.UpgradeTower,
                upgradeCost,
                canUpgrade && canAffordUpgrade,
                !canUpgrade
            );

            optionPool[1].SetupManage(
                "Bán",
                targetSlot.SellTower,
                -currentTower.SellValue,
                true
            );

            ArrangeInCircle(optionPool, 2, animate);
        }

        private void ArrangeInCircle(RadialMenuOption[] options, int count, bool animate)
        {
            if (options == null || count <= 0) return;

            float angleStep = 360f / count;
            float direction = counterClockwise ? 1f : -1f;
            float localRadius = currentRadius / GetCanvasScale();

            for (int i = 0; i < count; i++)
            {
                RadialMenuOption option = options[i];
                if (option == null) continue;

                float angleDeg = startAngle + direction * angleStep * i;
                float angleRad = angleDeg * Mathf.Deg2Rad;

                RectTransform rectTransform = option.GetComponent<RectTransform>();
                if (rectTransform != null)
                {
                    rectTransform.anchoredPosition = new Vector2(
                        Mathf.Cos(angleRad) * localRadius,
                        Mathf.Sin(angleRad) * localRadius
                    );
                }

                if (animate && openDuration > 0f)
                {
                    AnimateOptionIn(option, i, count);
                }
                else
                {
                    option.transform.localScale = Vector3.one;
                }
            }
        }

        private void SetupBackground(float slotMaxSide)
        {
            if (backgroundRect == null) return;

            float diameter = slotMaxSide * radiusMultiplier * 2f * 1.2f;
            backgroundRect.sizeDelta = new Vector2(diameter, diameter) / GetCanvasScale();
        }

        private void ScaleCanvasToSlot(Vector2 slotSize)
        {
            if (menuCanvas == null) return;

            CanvasScaler scaler = menuCanvas.GetComponent<CanvasScaler>();
            float referenceSize = scaler != null ? scaler.referenceResolution.x : 1000f;
            float worldDiameter = Mathf.Max(slotSize.x, slotSize.y) * radiusMultiplier * 2f * 1.4f;
            float scale = worldDiameter / referenceSize;

            menuCanvas.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private float GetCanvasScale()
        {
            return menuCanvas != null ? menuCanvas.transform.localScale.x : 0.01f;
        }

        private void AnimateOptionIn(RadialMenuOption option, int index, int total)
        {
            StartCoroutine(ScaleIn(option.transform, index * (openDuration / total)));
        }

        private System.Collections.IEnumerator ScaleIn(Transform target, float delay)
        {
            target.localScale = Vector3.zero;

            if (delay > 0f)
            {
                yield return new WaitForSeconds(delay);
            }

            float elapsed = 0f;

            while (elapsed < openDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
                target.localScale = Vector3.one * progress;
                yield return null;
            }

            target.localScale = Vector3.one;
        }
    }
}