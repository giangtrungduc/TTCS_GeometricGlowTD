using UnityEngine;
using UnityEngine.UI;
using TowerDefense.Core; // Gọi EconomyManager

namespace TowerDefense.Towers
{
    public class RadialMenu : MonoBehaviour
    {
        [Header("Layout")]
        [SerializeField][Range(0.8f, 3f)] private float radiusMultiplier = 1.5f;
        [SerializeField] private float startAngle = 90f;
        [SerializeField] private bool counterClockwise = false;

        [Header("Animation")]
        [SerializeField][Min(0f)] private float openDuration = 0.15f;

        [Header("References")]
        [SerializeField] private Canvas menuCanvas;
        [SerializeField] private RectTransform backgroundRect;

        [Header("Build Mode")]
        [SerializeField] private TowerData[] availableTowers;

        [Header("Options")]
        [SerializeField] private RadialMenuOption[] optionPool;

        private BuildSlot targetSlot;
        private float currentRadius;

        public void Open(BuildSlot slot)
        {
            targetSlot = slot;
            float slotMaxSide = Mathf.Max(slot.SlotSize.x, slot.SlotSize.y);
            currentRadius = slotMaxSide * radiusMultiplier;

            ScaleCanvasToSlot(slot.SlotSize);

            if (backgroundRect != null)
            {
                float diameter = slotMaxSide * radiusMultiplier * 2f * 1.2f;
                backgroundRect.sizeDelta = new Vector2(diameter, diameter) / GetCanvasScale();
            }

            SetupOptions();
            gameObject.SetActive(true);
        }

        public void Close()
        {
            targetSlot = null;
            gameObject.SetActive(false);
        }

        private void SetupOptions()
        {
            foreach (var opt in optionPool) opt.gameObject.SetActive(false);

            if (!targetSlot.IsOccupied) SetupBuildMode();
            else SetupManageMode();
        }

        private void SetupBuildMode()
        {
            int count = Mathf.Min(availableTowers.Length, optionPool.Length);
            var activeOptions = new RadialMenuOption[count];

            for (int i = 0; i < count; i++)
            {
                TowerData data = availableTowers[i];
                if (data == null) continue;

                optionPool[i].SetupBuild(data, targetSlot);
                activeOptions[i] = optionPool[i];
            }

            ArrangeInCircle(activeOptions, count);
        }

        private void SetupManageMode()
        {
            int count = 2;
            TowerBase currentTower = targetSlot.CurrentTower;

            bool canUpgrade = currentTower.CanUpgrade;
            int upgradeCost = canUpgrade ? currentTower.UpgradeCost : 0;

            // Kiểm tra xem có đủ tiền nâng cấp không
            bool canAffordUpgrade = true;
            if (EconomyManager.Instance != null && canUpgrade)
            {
                canAffordUpgrade = EconomyManager.Instance.CurrentGold >= upgradeCost;
            }

            // Nút Nâng cấp (Tự nhận biết MAX LEVEL)
            optionPool[0].SetupManage(
                "Nâng cấp",
                targetSlot.UpgradeTower,
                upgradeCost,
                interactable: canUpgrade && canAffordUpgrade, // Khóa nếu max cấp hoặc thiếu tiền
                isMaxLevel: !canUpgrade // Báo hiệu để hiện chữ MAX
            );

            // Nút Bán
            optionPool[1].SetupManage(
                "Bán",
                targetSlot.SellTower,
                -currentTower.SellValue,
                interactable: true
            );

            ArrangeInCircle(optionPool, count);
        }

        private void ArrangeInCircle(RadialMenuOption[] options, int count)
        {
            if (count <= 0) return;
            float angleStep = 360f / count;
            float direction = counterClockwise ? 1f : -1f;
            float localRadius = currentRadius / GetCanvasScale();

            for (int i = 0; i < count; i++)
            {
                if (options[i] == null) continue;
                float angleDeg = startAngle + direction * angleStep * i;
                float angleRad = angleDeg * Mathf.Deg2Rad;

                Vector2 localPos = new Vector2(Mathf.Cos(angleRad) * localRadius, Mathf.Sin(angleRad) * localRadius);
                RectTransform rt = options[i].GetComponent<RectTransform>();
                if (rt != null) rt.anchoredPosition = localPos;

                if (openDuration > 0f) AnimateOptionIn(options[i], i, count);
            }
        }

        private void ScaleCanvasToSlot(Vector2 slotSize)
        {
            if (menuCanvas == null) return;
            CanvasScaler scaler = menuCanvas.GetComponent<CanvasScaler>();
            float refSize = scaler != null ? scaler.referenceResolution.x : 1000f;
            float worldDiameter = Mathf.Max(slotSize.x, slotSize.y) * radiusMultiplier * 2f * 1.4f;
            float scale = worldDiameter / refSize;
            menuCanvas.transform.localScale = new Vector3(scale, scale, 1f);
        }

        private float GetCanvasScale() => menuCanvas != null ? menuCanvas.transform.localScale.x : 0.01f;

        private void AnimateOptionIn(RadialMenuOption option, int index, int total)
        {
            StartCoroutine(ScaleIn(option.transform, index * (openDuration / total)));
        }

        private System.Collections.IEnumerator ScaleIn(Transform t, float delay)
        {
            t.localScale = Vector3.zero;
            if (delay > 0f) yield return new WaitForSeconds(delay);

            float elapsed = 0f;
            while (elapsed < openDuration)
            {
                elapsed += Time.deltaTime;
                float progress = Mathf.SmoothStep(0f, 1f, elapsed / openDuration);
                t.localScale = Vector3.one * progress;
                yield return null;
            }
            t.localScale = Vector3.one;
        }
    }
}