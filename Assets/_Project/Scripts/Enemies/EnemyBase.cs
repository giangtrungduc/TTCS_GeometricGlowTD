using TowerDefense.Core;
using TowerDefense.Pooling;
using UnityEngine;

namespace TowerDefense.Enemies
{
    public class EnemyBase : MonoBehaviour
    {
        [SerializeField] private EnemyData data;
        [SerializeField] private SpriteRenderer spriteRenderer;

        private Transform[] waypoints;
        private int currentWaypointIndex;
        private float currentHp;
        private float laneOffset;
        private Vector3 currentTargetPos;

        // Trạng thái hiệu ứng Visual
        private float slowTimer;
        private float slowPercent;
        private float flashTimer;
        private float spawnGraceTimer;
        private float spawnScaleTimer;
        private const float FLASH_DURATION = 0.1f;
        private const float SPAWN_SCALE_DURATION = 0.3f;
        private Color originalColor;

        public bool IsDead { get; private set; }
        public bool IsTargetable => !IsDead && spawnGraceTimer <= 0f;
        public bool IsSlowed => slowTimer > 0f;
        public float HpPercent => data != null && data.maxHp > 0 ? currentHp / data.maxHp : 0;

        private void Awake()
        {
            if (spriteRenderer == null) spriteRenderer = GetComponentInChildren<SpriteRenderer>();
            if (spriteRenderer != null) originalColor = spriteRenderer.color;
        }

        public void Initialize(Transform[] pathWaypoints, float randomLaneOffset = 0f)
        {
            waypoints = pathWaypoints;
            laneOffset = randomLaneOffset;
            currentWaypointIndex = 0;
            
            currentHp = data.maxHp;
            IsDead = false;
            spawnGraceTimer = 0.05f;
            spawnScaleTimer = SPAWN_SCALE_DURATION;
            slowTimer = 0f;
            flashTimer = 0f;

            transform.localScale = Vector3.one * 0.3f;

            if (spriteRenderer != null) spriteRenderer.color = originalColor;

            if (waypoints != null && waypoints.Length > 0)
            {
                CalculateTargetPosition();
                transform.position = currentTargetPos; 
                currentWaypointIndex++;
                CalculateTargetPosition();
            }
        }

        private void Update()
        {
            if (IsDead || GameManager.Instance.IsGameOver || waypoints == null) return;

            if (spawnGraceTimer > 0f) spawnGraceTimer -= Time.deltaTime;

            if (spawnScaleTimer > 0f)
            {
                spawnScaleTimer -= Time.deltaTime;
                float t = 1f - Mathf.Clamp01(spawnScaleTimer / SPAWN_SCALE_DURATION);
                float scale = Mathf.Lerp(0.3f, 1f, t);
                transform.localScale = Vector3.one * scale;
            }

            MoveAlongPath();
            HandleVisualEffects();
        }

        private void MoveAlongPath()
        {
            if (currentWaypointIndex >= waypoints.Length) return;

            Vector3 dir = currentTargetPos - transform.position;
            
            float currentSpeed = data.moveSpeed * (slowTimer > 0 ? (1f - slowPercent) : 1f);
            float dist = currentSpeed * Time.deltaTime;

            if (dir.sqrMagnitude <= dist * dist)
            {
                transform.position = currentTargetPos;
                currentWaypointIndex++;

                if (currentWaypointIndex >= waypoints.Length)
                    ReachEnd();
                else
                    CalculateTargetPosition();
            }
            else
            {
                transform.Translate(dir.normalized * dist, Space.World);
                
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                if (spriteRenderer != null)
                {
                    spriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
                }
                else
                {
                    transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
                }
            }
        }

        private void CalculateTargetPosition()
        {
            if (currentWaypointIndex >= waypoints.Length) return;
            
            Vector3 center = waypoints[currentWaypointIndex].position;
            if (currentWaypointIndex > 0)
            {
                Vector3 dir = (waypoints[currentWaypointIndex].position - waypoints[currentWaypointIndex - 1].position).normalized;
                Vector3 normal = new Vector3(-dir.y, dir.x, 0); 
                currentTargetPos = center + normal * laneOffset;
            }
            else
            {
                currentTargetPos = center;
            }
        }

        private void HandleVisualEffects()
        {
            if (slowTimer > 0)
            {
                slowTimer -= Time.deltaTime;
                if (slowTimer <= 0) UpdateColor();
            }

            // Xử lý hiệu ứng chớp (Đổi Alpha)
            if (flashTimer > 0)
            {
                flashTimer -= Time.deltaTime;
                if (flashTimer <= 0) UpdateAlpha();
            }
        }

        private void UpdateColor()
        {
            if (spriteRenderer == null) return;
            
            Color targetColor = (slowTimer > 0) ? new Color(0.5f, 0.7f, 1f) : originalColor;
            targetColor.a = spriteRenderer.color.a;
            spriteRenderer.color = targetColor;
        }

        private void UpdateAlpha()
        {
            if (spriteRenderer == null) return;

            Color targetColor = spriteRenderer.color;
            targetColor.a = (flashTimer > 0) ? 0.3f : originalColor.a; 
            spriteRenderer.color = targetColor;
        }

        public void TakeDamage(float amount)
        {
            if (IsDead) return;

            currentHp -= amount;

            flashTimer = FLASH_DURATION;
            UpdateAlpha();

            if (currentHp <= 0) Die();
        }

        public void ApplySlow(float pct, float duration)
        {
            // Nếu đang bị slow mạnh hơn thì không ghi đè slow yếu
            if (slowTimer <= 0 || pct >= slowPercent) 
            {
                slowPercent = pct;
            }
            slowTimer = duration; 

            UpdateColor();   
        }

        private void Die()
        {
            IsDead = true;
            transform.localScale = Vector3.one;
            GameManager.Instance.AddGold(data.goldReward);

            if (data.deathVfxPrefab != null)
            {
                SimplePool.Instance.Get(data.deathVfxPrefab, transform.position, Quaternion.identity);
            }

            SimplePool.Instance.Return(gameObject);
        }

        private void ReachEnd()
        {
            IsDead = true;
            transform.localScale = Vector3.one;
            GameManager.Instance.LoseLife(data.livesCost);

            if (data.deathVfxPrefab != null)
            {
                SimplePool.Instance.Get(data.deathVfxPrefab, transform.position, Quaternion.identity);
            }

            SimplePool.Instance.Return(gameObject);
        }
    }
}
