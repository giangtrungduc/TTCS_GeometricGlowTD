using TowerDefense.Core;
using TowerDefense.Enemies;
using TowerDefense.Pooling;
using TowerDefense.Projectiles;
using UnityEngine;

namespace TowerDefense.Towers
{
    public class TowerBase : MonoBehaviour
    {
        [SerializeField] private TowerData data;
        [Tooltip("Cục Transform vẽ hình nòng súng (Sẽ bị xoay)")]
        [SerializeField] private Transform turretHead; 
        [Tooltip("Cục Transform rỗng nằm ở đầu nòng súng, dùng làm điểm mốc đẻ đạn")]
        [SerializeField] private Transform firePoint;  
        [Tooltip("Cục Transform chứa hình tròn mờ hiển thị tầm bắn")]
        [SerializeField] private Transform rangeIndicator;

        [Header("Audio Settings (Gắn thẳng trên Prefab)")]
        [SerializeField] private AudioSource audioSource;
        [SerializeField] private float minPitch = 0.9f;
        [SerializeField] private float maxPitch = 1.1f;

        private int currentLevel = 0;
        private float fireTimer;
        private EnemyBase currentTarget;

        public TowerData Data => data;
        public int CurrentLevel => currentLevel;
        public bool IsMaxLevel => data == null || currentLevel >= data.levels.Length - 1;

        public float CooldownPercent 
        {
            get 
            {
                if (CurrentStats == null || CurrentStats.fireRate <= 0f) return 1f;
                float cooldownTime = 1f / CurrentStats.fireRate;
                return Mathf.Clamp01(1f - (fireTimer / cooldownTime));
            }
        }

        public int TotalGoldSpent
        {
            get
            {
                if (data == null || data.levels == null) return 0;
                int total = 0;
                for (int i = 0; i <= currentLevel; i++)
                {
                    total += data.levels[i].cost;
                }
                return total;
            }
        }

        public TowerUpgradeLevel CurrentStats 
        {
            get
            {
                if (data == null || data.levels == null || data.levels.Length == 0) return null;
                return data.levels[Mathf.Clamp(currentLevel, 0, data.levels.Length - 1)];
            }
        }

        public TowerUpgradeLevel NextStats
        {
            get
            {
                if (IsMaxLevel) return null;
                return data.levels[currentLevel + 1];
            }
        }

        private void Start()
        {
            SetRangeIndicatorActive(false); 
        }

        private void Update()
        {
            if (GameManager.Instance.IsGameOver || CurrentStats == null) return;

            fireTimer -= Time.deltaTime;

            FindTarget();

            if (currentTarget != null)
            {
                RotateTurret();
                if (fireTimer <= 0f)
                {
                    Shoot();
                }
            }
        }

        private void FindTarget()
        {
            if (currentTarget != null)
            {
                bool isOutOfRange = Vector2.Distance(transform.position, currentTarget.transform.position) > CurrentStats.range;

                if (currentTarget.IsDead || !currentTarget.gameObject.activeInHierarchy || isOutOfRange)
                {
                    currentTarget = null;
                }
                else if (data.isSlowTower && currentTarget.IsSlowed)
                {
                    currentTarget = null;
                }
                else 
                {
                    return;
                }
            }

            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, CurrentStats.range);
            float minDistance = float.MaxValue;
            EnemyBase bestTarget = null;

            // Slow tower: ưu tiên quái chưa bị slow
            float minNonSlowedDist = float.MaxValue;
            EnemyBase bestNonSlowed = null;

            foreach (var hit in hits)
            {
                EnemyBase enemy = hit.GetComponent<EnemyBase>();
                if (enemy != null && enemy.IsTargetable)
                {
                    float d = Vector2.Distance(transform.position, enemy.transform.position);
                    if (d < minDistance)
                    {
                        minDistance = d;
                        bestTarget = enemy;
                    }

                    if (data.isSlowTower && !enemy.IsSlowed && d < minNonSlowedDist)
                    {
                        minNonSlowedDist = d;
                        bestNonSlowed = enemy;
                    }
                }
            }

            currentTarget = (data.isSlowTower && bestNonSlowed != null) ? bestNonSlowed : bestTarget;
        }

        private void RotateTurret()
        {
            if (turretHead == null || currentTarget == null) return;
            Vector3 dir = currentTarget.transform.position - turretHead.position;
            float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
            
            turretHead.rotation = Quaternion.Euler(0f, 0f, angle - 90f); 
        }

        private void Shoot()
        {
            fireTimer = 1f / CurrentStats.fireRate;

            if (data.projectilePrefab != null && firePoint != null)
            {
                // Gọi đạn từ pool ra dùng
                GameObject projObj = SimplePool.Instance.Get(data.projectilePrefab, firePoint.position, turretHead.rotation);
                Bullet bullet = projObj.GetComponent<Bullet>();
                
                if (bullet != null)
                {
                    bullet.Initialize(
                        currentTarget.transform, 
                        CurrentStats.damage, 
                        data.isSlowTower, 
                        CurrentStats.slowPercent, 
                        CurrentStats.slowDuration
                    );
                }
            }

            if (data.shootSound != null)
            {
                if (audioSource != null)
                {
                    audioSource.pitch = Random.Range(minPitch, maxPitch);
                    
                    float masterVolume = AudioManager.Instance != null ? AudioManager.Instance.GetSFXVolume() : 1f;
                    audioSource.PlayOneShot(data.shootSound, masterVolume);
                }
                else if (AudioManager.Instance != null)
                {
                    AudioManager.Instance.PlaySFX(data.shootSound);
                }
            }
        }

        public void Upgrade()
        {
            if (!IsMaxLevel)
            {
                currentLevel++;
                // Cập nhật lại kích thước vòng tròn nếu đang được bật
                if (rangeIndicator != null && rangeIndicator.gameObject.activeSelf)
                {
                    SetRangeIndicatorActive(true);
                }
            }
        }

        public void SetRangeIndicatorActive(bool active)
        {
            if (rangeIndicator == null) return;
            rangeIndicator.gameObject.SetActive(active);

            if (active && CurrentStats != null)
            {
                float diameter = CurrentStats.range * 2f;
                rangeIndicator.localScale = new Vector3(diameter, diameter, 1f);
            }
        }

        private void OnDrawGizmosSelected()
        {
            if (CurrentStats != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(transform.position, CurrentStats.range);
            }
        }
    }
}
