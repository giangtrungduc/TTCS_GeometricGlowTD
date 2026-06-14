using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Towers
{
    /// <summary>
    /// Gắn trên các ô đất. Cho phép click để đặt, nâng cấp hoặc bán trụ.
    /// </summary>
    public class BuildSlot : MonoBehaviour
    {
        public TowerBase currentTower { get; private set; }
        
        private SpriteRenderer spriteRenderer;
        private Color originalColor;
        
        // Quản lý ô đất đang được click toàn cục
        private static BuildSlot selectedSlot;

        private void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        public bool BuildTower(GameObject towerPrefab)
        {
            if (currentTower != null || towerPrefab == null) return false;

            TowerBase temp = towerPrefab.GetComponent<TowerBase>();
            if (temp == null || temp.Data == null || temp.Data.levels.Length == 0) return false;

            int cost = temp.Data.levels[0].cost;
            if (GameManager.Instance.SpendGold(cost))
            {
                GameObject obj = Instantiate(towerPrefab, transform.position, Quaternion.identity, transform);
                currentTower = obj.GetComponent<TowerBase>();
                return true;
            }
            return false;
        }

        public bool UpgradeTower()
        {
            if (currentTower == null || currentTower.IsMaxLevel) return false;

            int cost = currentTower.NextStats.cost;
            if (GameManager.Instance.SpendGold(cost))
            {
                currentTower.Upgrade();
                return true;
            }
            return false;
        }

        public bool SellTower()
        {
            if (currentTower == null) return false;

            // Bán tháp nhận lại 50% TỔNG số tiền đã bỏ ra xây và nâng cấp
            int refund = currentTower.TotalGoldSpent / 2;
            GameManager.Instance.AddGold(refund);

            Destroy(currentTower.gameObject);
            currentTower = null;
            return true;
        }

        // --- Visual Feedback khi di chuột ---
        private void OnMouseEnter()
        {
            if (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver) return;

            if (spriteRenderer != null && currentTower == null) 
                spriteRenderer.color = Color.yellow;
        }

        private void OnMouseExit()
        {
            if (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver) return;

            if (spriteRenderer != null) 
                spriteRenderer.color = originalColor;
        }

        private void OnMouseDown()
        {
            if (GameManager.Instance.IsPaused || GameManager.Instance.IsGameOver) return;

            // 1. Tắt hiển thị tầm bắn của tháp cũ (nếu có)
            if (selectedSlot != null && selectedSlot != this && selectedSlot.currentTower != null)
            {
                selectedSlot.currentTower.SetRangeIndicatorActive(false);
            }

            // 2. Ghi nhớ ô đất hiện tại đang được chọn
            selectedSlot = this;

            // 3. Hiển thị vòng tròn tầm bắn
            if (currentTower != null)
            {
                currentTower.SetRangeIndicatorActive(true);
            }

            // 4. Gọi UI Controller mở Menu
            if (UI.TowerUIController.Instance != null)
            {
                UI.TowerUIController.Instance.OpenMenu(this);
            }
        }
    }
}
