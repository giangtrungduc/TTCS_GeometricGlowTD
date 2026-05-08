using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    [RequireComponent(typeof(TargetingSystem))]
    public abstract class TowerBase : MonoBehaviour
    {
        // ============================
        // CẤU HÌNH
        // ============================

        [Header("Tower Data")]
        [Tooltip("SO chứa thông số của tháp")]
        [SerializeField] private TowerData towerData;

        [Header("Visual & Fire Point")]
        [Tooltip("Transform visual của súng / nòng tháp, dùng để xoay về hướng enemy")]
        [SerializeField] private Transform gunVisual;

        [Tooltip("Vị trí viên đạn được bắn ra")]
        [SerializeField] private Transform firePoint;

        [Tooltip("Offset góc xoay nếu sprite súng không mặc định quay sang phải. Nếu sprite quay lên trên, thử -90.")]
        [SerializeField] private float gunRotationOffset = -90f;

        // ============================
        // CACHED REFERENCES & STATE
        // ============================

        private TargetingSystem targeting;
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

        protected Transform GunVisual => gunVisual;
        protected Transform FirePoint => firePoint != null ? firePoint : transform;

        // ============================
        // UNITY LIFECYCLE
        // ============================

        protected virtual void Awake()
        {
            targeting = GetComponent<TargetingSystem>();

            if (towerData == null)
            {
                Debug.LogError($"[TowerBase] '{gameObject.name}' missing TowerData!");
            }

            if (gunVisual == null)
            {
                Debug.LogWarning($"[TowerBase] '{gameObject.name}' missing Gun Visual. Tower will still attack, but gun will not rotate.");
            }

            if (firePoint == null)
            {
                Debug.LogWarning($"[TowerBase] '{gameObject.name}' missing Fire Point. Projectile will spawn at tower transform position.");
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

            GameObject target = targeting.GetBestTarget();

            // Luôn xoay gun về hướng enemy khi có enemy trong phạm vi
            if (target != null)
            {
                RotateGunTowards(target);
            }

            if (attackTimer > 0f)
            {
                OnTowerUpdate();
                return;
            }

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
        }

        /// <summary>Nâng cấp tháp.</summary>
        public bool Upgrade()
        {
            if (towerData == null || !towerData.CanUpgrade(currentLevel)) return false;

            currentLevel++;

            if (targeting != null)
            {
                targeting.SetRange(CurrentStats.range);
            }

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

            if (targeting != null)
            {
                targeting.ClearTarget();
            }
        }

        // ============================
        // UTILITY CHO CLASS CON
        // ============================

        protected void ResetAttackTimer() => attackTimer = 0f;
        protected void SetAttackCooldown(float cooldown) => attackTimer = cooldown;
        protected bool IsTargetStillValid() => targeting != null && targeting.IsCurrentTargetValid();

        /// <summary>
        /// Xoay gunVisual về phía target.
        /// Mặc định sprite gun nên quay sang phải theo trục X.
        /// Nếu sprite gun quay lên trên, set gunRotationOffset = -90 trong Inspector.
        /// </summary>
        protected virtual void RotateGunTowards(GameObject target)
        {
            if (gunVisual == null || target == null) return;

            Vector3 direction = target.transform.position - gunVisual.position;

            if (direction.sqrMagnitude <= 0.0001f) return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            gunVisual.rotation = Quaternion.Euler(0f, 0f, angle + gunRotationOffset);
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

            if (firePoint != null)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(firePoint.position, 0.1f);
                Gizmos.DrawLine(firePoint.position, firePoint.position + firePoint.right * 0.4f);
            }
        }
    }
}