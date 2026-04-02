// ============================================================
// File: FireTower.cs
// Vị trí: Assets/_Project/Scripts/Towers/FireTower.cs
// Mục đích: Tháp bắn tia laser liên tục, sát thương tăng dần theo thời gian.
// Kỹ năng (Cấp 3): Bùng nổ - x2 tốc độ bắn trong thời gian ngắn.
// ============================================================

using System.Collections;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Towers
{
    public class FireTower : TowerBase
    {
        // ============================
        // CẤU HÌNH VISUAL
        // ============================

        [Header("Beam Visual")]
        [SerializeField] private Material beamMaterial;
        [SerializeField] private float beamStartWidth = 0.15f;
        [SerializeField] private float beamMaxWidth = 0.35f;

        [SerializeField] private Color beamColorLv1 = new Color(1f, 0.5f, 0f);
        [SerializeField] private Color beamColorLv2 = new Color(1f, 0.3f, 0f);
        [SerializeField] private Color beamColorLv3 = new Color(1f, 0.1f, 0f);

        [Header("Bùng Nổ (Cấp 3)")]
        [SerializeField] private float explosionInterval = 10f;
        [SerializeField] private float explosionDuration = 1f;
        [SerializeField] private float explosionCooldownMult = 0.5f;

        // ============================
        // STATE
        // ============================

        private LineRenderer lineRenderer;
        private GameObject currentBeamTarget;
        private float currentRampDamage;
        private float timeOnTarget;
        private Coroutine explosionCoroutine;
        
        private float cooldownMultiplier = 1f;
        private bool isExploding = false;

        // ============================
        // PROPERTIES
        // ============================

        public float CurrentRampDamage => currentRampDamage;
        public bool IsExplosionUnlocked => CurrentLevel >= 2;
        public bool IsExploding => isExploding;

        // ============================
        // INIT
        // ============================

        protected override void OnTowerAwake()
        {
            SetupLineRenderer();
        }

        private void SetupLineRenderer()
        {
            if (!TryGetComponent(out lineRenderer))
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            lineRenderer.positionCount = 2;
            lineRenderer.enabled = false;
            lineRenderer.sortingOrder = 10;
            
            lineRenderer.material = beamMaterial != null ? beamMaterial : new Material(Shader.Find("Sprites/Default"));
            lineRenderer.startWidth = beamStartWidth;
            lineRenderer.endWidth = beamStartWidth;
            lineRenderer.startColor = beamColorLv1;
            lineRenderer.endColor = beamColorLv1;
        }

        // ============================
        // UPDATE LOOP
        // ============================

        protected override void OnTowerUpdate()
        {
            // Kiểm tra mất mục tiêu để reset Ramp ngay lập tức
            if (currentBeamTarget != null && !currentBeamTarget.activeInHierarchy)
            {
                currentBeamTarget = null;
                ResetRampDamage();
            }

            UpdateBeamVisual();
            UpdateRampDamage();
        }

        // ============================
        // ATTACK LOGIC
        // ============================

        protected override void OnAttack(GameObject target, TowerLevelData stats)
        {
            if (target != currentBeamTarget)
            {
                currentBeamTarget = target;
                ResetRampDamage();
            }

            ApplyDamage(target, currentRampDamage);

            float finalCooldown = stats.attackCooldown * cooldownMultiplier;
            SetAttackCooldown(finalCooldown);
        }

        // ============================
        // RAMP DAMAGE
        // ============================

        private void UpdateRampDamage()
        {
            if (currentBeamTarget == null || CurrentStats.rampAmount <= 0) return;

            timeOnTarget += Time.deltaTime;

            if (timeOnTarget >= CurrentStats.rampInterval)
            {
                currentRampDamage += CurrentStats.rampAmount;

                if (CurrentStats.maxDamage > 0)
                {
                    currentRampDamage = Mathf.Min(currentRampDamage, CurrentStats.maxDamage);
                }

                timeOnTarget -= CurrentStats.rampInterval;
            }
        }

        private void ResetRampDamage()
        {
            if (Data == null) return;
            
            currentRampDamage = CurrentStats.damage;
            timeOnTarget = 0f;
        }

        // ============================
        // BEAM VISUALS
        // ============================

        private void UpdateBeamVisual()
        {
            if (currentBeamTarget == null)
            {
                if (lineRenderer.enabled) lineRenderer.enabled = false;
                return;
            }

            if (!lineRenderer.enabled) lineRenderer.enabled = true;

            // Offset Z tránh z-fighting với map
            Vector3 towerPos = transform.position; towerPos.z = -0.1f;
            Vector3 targetPos = currentBeamTarget.transform.position; targetPos.z = -0.1f;

            lineRenderer.SetPosition(0, towerPos);
            lineRenderer.SetPosition(1, targetPos);

            UpdateBeamWidth();
            UpdateBeamColor();

            if (isExploding)
            {
                // Hiệu ứng nhấp nháy khi Bùng Nổ
                float flash = Mathf.PingPong(Time.time * 10f, 1f);
                float flashWidth = beamMaxWidth * (0.8f + flash * 0.4f);
                lineRenderer.startWidth = flashWidth;
                lineRenderer.endWidth = flashWidth;
            }
        }

        private void UpdateBeamWidth()
        {
            if (isExploding) return; 

            float rampRatio = 0f;
            if (CurrentStats.maxDamage > CurrentStats.damage)
            {
                rampRatio = (currentRampDamage - CurrentStats.damage) / (CurrentStats.maxDamage - CurrentStats.damage);
            }

            float width = Mathf.Lerp(beamStartWidth, beamMaxWidth, rampRatio);
            lineRenderer.startWidth = width;
            lineRenderer.endWidth = width;
        }

        private void UpdateBeamColor()
        {
            Color color = CurrentLevel switch
            {
                0 => beamColorLv1,
                1 => beamColorLv2,
                _ => beamColorLv3
            };

            if (isExploding) color *= 1.5f; // Sáng lên khi Burst

            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

        // ============================
        // DAMAGE
        // ============================

        private void ApplyDamage(GameObject target, float damage)
        {
            // TODO Ngày 15: Gọi EnemyBase.TakeDamage()
            Debug.Log($"<color=red>[FireTower]</color> Đốt '{target.name}' - {damage:F1} DMG (Ramp: +{((damage / CurrentStats.damage - 1f) * 100):F0}%)");
        }

        // ============================
        // UPGRADE & ABILITY
        // ============================

        protected override void OnUpgraded()
        {
            ResetRampDamage();

            if (CurrentLevel == 2) // Mở khóa ở Cấp 3
            {
                Debug.Log($"[FireTower] 🔥 BÙNG NỔ UNLOCKED! Tốc bắn x{1f/explosionCooldownMult:F0} mỗi {explosionInterval}s");
                
                if (explosionCoroutine != null) StopCoroutine(explosionCoroutine);
                explosionCoroutine = StartCoroutine(ExplosionLoop());
            }
        }

        private IEnumerator ExplosionLoop()
        {
            yield return new WaitForSeconds(1f);

            while (true)
            {
                yield return new WaitForSeconds(explosionInterval);

                isExploding = true;
                cooldownMultiplier = explosionCooldownMult;
                
                // Ép bắn ngay lập tức để tận dụng tối đa 1s Bùng Nổ
                SetAttackCooldown(0f); 

                yield return new WaitForSeconds(explosionDuration);

                isExploding = false;
                cooldownMultiplier = 1f;
            }
        }

        // ============================
        // CLEANUP
        // ============================

        private void OnDisable()
        {
            if (lineRenderer != null) lineRenderer.enabled = false;
            
            if (explosionCoroutine != null)
            {
                StopCoroutine(explosionCoroutine);
                explosionCoroutine = null;
            }

            currentBeamTarget = null;
            isExploding = false;
            cooldownMultiplier = 1f;
        }

        // ============================
        // GIZMOS
        // ============================

        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected();

#if UNITY_EDITOR
            if (Data != null)
            {
                string label = $"DMG: {currentRampDamage:F1}";
                if (CurrentLevel >= 2)
                {
                    label += IsExploding ? " (BURST!)" : $" (cap: {CurrentStats.maxDamage})";
                }
                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, label);
            }
#endif
        }
    }
}