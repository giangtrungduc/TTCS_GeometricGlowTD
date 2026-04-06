using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    [RequireComponent(typeof(Collider2D))]
    public class BuildSlot : MonoBehaviour
    {
        [Header("Build Slot")]
        [SerializeField] private RadialMenu radialMenuPrefab;

        private TowerBase currentTower;
        private RadialMenu currentMenu;
        private Collider2D slotCollider;

        public bool IsOccupied => currentTower != null;
        public TowerBase CurrentTower => currentTower;

        public Vector2 SlotSize => slotCollider != null ? slotCollider.bounds.size : Vector2.one;

        private void Awake()
        {
            slotCollider = GetComponent<Collider2D>();
        }

        private void OnMouseDown()
        {
            if (currentMenu != null) { CloseMenu(); return; }
            OpenMenu();
        }

        private void OnDisable()
        {
            CloseMenu();
        }

        public void PlaceTower(TowerData towerData)
        {
            if (IsOccupied || towerData == null) return;

            TowerLevelData level0 = towerData.GetLevel(0);

            // Bỏ qua trừ tiền nếu đang test không có EconomyManager
            if (EconomyManager.Instance != null)
            {
                if (!EconomyManager.Instance.TrySpendGold(level0.cost))
                {
                    Debug.LogWarning($"[BuildSlot] Không đủ gold để mua {towerData.towerName}");
                    return;
                }
            }

            GameObject go = Instantiate(towerData.towerPrefab, transform.position, Quaternion.identity);
            currentTower = go.GetComponent<TowerBase>();
            currentTower.Activate();
            currentTower.SetInvestment(level0.cost);

            GameEvents.RaiseTowerPlaced(go);
            CloseMenu();
        }

        public void UpgradeTower()
        {
            if (!IsOccupied || !currentTower.CanUpgrade) return;

            bool canUpgrade = false;

            if (EconomyManager.Instance != null)
            {
                if (EconomyManager.Instance.TrySpendGold(currentTower.UpgradeCost))
                    canUpgrade = true;
            }
            else
            {
                canUpgrade = true; // Cho phép upgrade free để test
            }

            if (canUpgrade)
            {
                if (currentTower.Upgrade())
                {
                    GameEvents.RaiseTowerUpgraded(currentTower.gameObject);
                    CloseMenu();
                }
            }
        }

        public void SellTower()
        {
            if (!IsOccupied) return;

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddGold(currentTower.SellValue);
            }

            GameEvents.RaiseTowerSold(currentTower.gameObject);
            Destroy(currentTower.gameObject);
            currentTower = null;
            CloseMenu();
        }

        private void OpenMenu()
        {
            if (radialMenuPrefab == null)
            {
                Debug.LogError("[BuildSlot] Chưa gán RadialMenuPrefab!", this);
                return;
            }

            currentMenu = Instantiate(radialMenuPrefab, transform.position, Quaternion.identity);
            currentMenu.Open(this);
        }

        public void CloseMenu()
        {
            if (currentMenu == null) return;
            Destroy(currentMenu.gameObject);
            currentMenu = null;
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = IsOccupied
                ? new Color(0f, 1f, 0f, 0.35f)
                : new Color(1f, 1f, 0f, 0.2f);
            Gizmos.DrawWireCube(transform.position, SlotSize);
        }
#endif
    }
}