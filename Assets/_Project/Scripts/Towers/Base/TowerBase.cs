using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    [RequireComponent(typeof(TargetingSystem))]
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class TowerBase : MonoBehaviour
    {
        // ============================
        // CẤU HÌNH
        // ============================

        [Header("Tower Data")]
        [Tooltip("SO chứa thông số của tháp")]
        [SerializeField] private TowerData towerData;

        // ============================
        // CACHED REFERENCES & STATE
        // ============================

        private TargetingSystem targeting;
        private SpriteRenderer spriteRenderer;

        private int currentLevel = 0; // 0-based (0 = Cấp 1)
        private float attackTimer = 0f;
        private int totalInvested = 0;
        private bool isActive = false;

        // ============================
        // PROPERTIES
        // ============================

        public TowerData Data => towerData;
        public int CurrentLevel => currentLevel;
        public TowerLevelData CurrentStats => towerData.GetLevel(currentLevel);
        public int TotalInvested => totalInvested;
        public int SellValue => Mathf.RoundToInt(totalInvested * 0.6f);
        public bool CanUpgrade => towerData != null && towerData.CanUpgrade(currentLevel);
        public int UpgradeCost => towerData != null ? towerData.GetUpgradeCost(currentLevel) : -1;

        protected TargetingSystem Targeting => targeting;
        protected GameObject CurrentTarget => targeting?.CurrentTarget;
        protected bool HasTarget => targeting != null && targeting.HasTarget;

        // ============================
        // UNITY LIFECYCLE
        // ============================

        protected virtual void Awake()
        {
            targeting = GetComponent<TargetingSystem>();
            spriteRenderer = GetComponent<SpriteRenderer>();

            if (towerData == null)
            {
                Debug.LogError($"[TowerBase] '{gameObject.name}' missing TowerData!");
            }

            OnTowerAwake();
        }

        protected virtual void Start()
        {
            Activate();
        }

        protected virtual void Update()
        {
            if (!isActive || towerData == null) return;

            attackTimer -= Time.deltaTime;

            if (attackTimer > 0f)
            {
                OnTowerUpdate();
                return;
            }

            GameObject target = targeting.GetBestTarget();

            if (target != null)
            {
                TowerLevelData stats = CurrentStats;
                OnAttack(target, stats);
                attackTimer = stats.attackCooldown;
            }

            OnTowerUpdate();
        }

        // ============================
        // ABSTRACT & VIRTUAL
        // ============================

        /// <summary>Logic tấn công riêng của từng tháp.</summary>
        protected abstract void OnAttack(GameObject target, TowerLevelData stats);

        /// <summary>Gọi cuối hàm Awake.</summary>
        protected virtual void OnTowerAwake() { }

        /// <summary>Gọi mỗi frame, sau logic tấn công.</summary>
        protected virtual void OnTowerUpdate() { }

        /// <summary>Gọi sau khi upgrade thành công.</summary>
        protected virtual void OnUpgraded() { }

        protected virtual void OnDisable() { }

        // ============================
        // PUBLIC METHODS
        // ============================

        /// <summary>Kích hoạt tháp, set range và visual.</summary>
        public void Activate()
        {
            isActive = true;
            if (targeting != null && towerData != null)
            {
                targeting.SetRange(CurrentStats.range);
            }
            UpdateVisual();
        }

        /// <summary>Nâng cấp tháp.</summary>
        public bool Upgrade()
        {
            if (towerData == null || !towerData.CanUpgrade(currentLevel)) return false;

            currentLevel++;

            if (targeting != null) targeting.SetRange(CurrentStats.range);

            UpdateVisual();
            totalInvested += CurrentStats.cost;

            OnUpgraded();
            GameEvents.RaiseTowerUpgraded(gameObject);

            return true;
        }

        public void SetInvestment(int amount) => totalInvested = amount;
        public void AddInvestment(int amount) => totalInvested += amount;

        /// <summary>Reset tháp khi bán hoặc trả về pool.</summary>
        public void Deactivate()
        {
            isActive = false;
            currentLevel = 0;
            totalInvested = 0;
            attackTimer = 0f;

            if (targeting != null) targeting.ClearTarget();
        }

        // ============================
        // UTILITY CHO CLASS CON
        // ============================

        protected void ResetAttackTimer() => attackTimer = 0f;
        protected void SetAttackCooldown(float cooldown) => attackTimer = cooldown;
        protected bool IsTargetStillValid() => targeting != null && targeting.IsCurrentTargetValid();

        // ============================
        // PRIVATE
        // ============================

        private void UpdateVisual()
        {
            if (spriteRenderer == null || towerData == null) return;

            Sprite levelSprite = CurrentStats.towerSprite;
            if (levelSprite != null)
            {
                spriteRenderer.sprite = levelSprite;
            }
        }

        // ============================
        // GIZMOS
        // ============================

        protected virtual void OnDrawGizmosSelected()
        {
            float range = 0f;

            if (Application.isPlaying && targeting != null)
            {
                range = targeting.Range;
            }
            else if (towerData != null && towerData.MaxLevel > 0)
            {
                range = towerData.GetLevel(currentLevel).range;
            }

            if (range <= 0f) return;

            Color rangeColor = towerData != null ? towerData.themeColor : Color.white;

            rangeColor.a = 0.25f;
            Gizmos.color = rangeColor;
            Gizmos.DrawWireSphere(transform.position, range);

            rangeColor.a = 0.05f;
            Gizmos.color = rangeColor;
            Gizmos.DrawSphere(transform.position, range);
        }
    }
}