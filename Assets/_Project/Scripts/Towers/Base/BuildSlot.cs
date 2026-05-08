using UnityEngine;
using TowerDefense.Core;
using TowerDefense.UI;

namespace TowerDefense.Towers
{
    [RequireComponent(typeof(Collider2D))]
    public class BuildSlot : MonoBehaviour
    {
        [Header("Ô xây tower")]
        [Tooltip("Prefab radial menu dùng để xây, nâng cấp hoặc bán tower.")]
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
            if (!CanOpenMenu()) return;

            if (currentMenu != null)
            {
                CloseMenu();
                return;
            }

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

            if (EconomyManager.Instance != null)
            {
                if (!EconomyManager.Instance.TrySpendGold(level0.cost))
                {
                    Debug.LogWarning($"[BuildSlot] Không đủ gold để mua {towerData.towerName}", this);
                    return;
                }
            }

            GameObject towerObject = Instantiate(towerData.towerPrefab, transform.position, Quaternion.identity);
            currentTower = towerObject.GetComponent<TowerBase>();

            if (currentTower == null)
            {
                Debug.LogError("[BuildSlot] Tower prefab không có TowerBase.", towerObject);
                Destroy(towerObject);
                return;
            }

            currentTower.Activate();
            currentTower.SetInvestment(level0.cost);

            GameEvents.RaiseTowerPlaced(towerObject);
            CloseMenu();
        }

        public void UpgradeTower()
        {
            if (!IsOccupied || currentTower == null || !currentTower.CanUpgrade) return;

            bool canUpgrade = false;

            if (EconomyManager.Instance != null)
            {
                canUpgrade = EconomyManager.Instance.TrySpendGold(currentTower.UpgradeCost);
            }
            else
            {
                canUpgrade = true;
            }

            if (!canUpgrade) return;

            if (currentTower.Upgrade())
            {
                CloseMenu();
            }
        }

        public void SellTower()
        {
            if (!IsOccupied || currentTower == null) return;

            if (EconomyManager.Instance != null)
            {
                EconomyManager.Instance.AddGold(currentTower.SellValue);
            }

            GameEvents.RaiseTowerSold(currentTower.gameObject);
            Destroy(currentTower.gameObject);

            currentTower = null;
            CloseMenu();
        }

        public void CloseMenu()
        {
            if (currentMenu == null) return;

            Destroy(currentMenu.gameObject);
            currentMenu = null;
        }

        private void OpenMenu()
        {
            if (radialMenuPrefab == null)
            {
                Debug.LogError("[BuildSlot] Chưa gán RadialMenuPrefab.", this);
                return;
            }

            currentMenu = Instantiate(radialMenuPrefab, transform.position, Quaternion.identity);
            currentMenu.Open(this);
        }

        private bool CanOpenMenu()
        {
            if (GameManager.Instance == null) return true;

            GameState state = GameManager.Instance.CurrentState;
            return state == GameState.Playing;
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
