// ============================================================
// File: IceTower.cs
// Vị trí: Assets/_Project/Scripts/Towers/IceTower.cs
// Mục đích: Tháp hỗ trợ — Làm chậm AoE, Gây sát thương vùng và Đóng Băng.
// Kiến trúc: Chuẩn OOP, thao tác hoàn toàn thông qua StatusEffect.
// ============================================================

using System.Collections;
using System.Collections.Generic;
using TowerDefense.Core;
using TowerDefense.Enemies;
using TowerDefense.StatusEffects;
using UnityEngine;

namespace TowerDefense.Towers
{
    public class IceTower : TowerBase
    {
        // ============================
        // CẤU HÌNH
        // ============================

        [Header("Ice Tower")]
        [SerializeField] private LayerMask enemyLayer;
        [SerializeField] private int maxDetect = 20;

        [Header("Đóng Băng (Cấp 3)")]
        [SerializeField] private float freezeCheckInterval = 2f;

        // ============================
        // STATE
        // ============================

        private Collider2D[] aoeBuffer;
        private ContactFilter2D enemyFilter;

        // QUAN TRỌNG: Dùng Dictionary để ánh xạ Quái vật -> Viên bùa SlowEffect của riêng tháp này
        private Dictionary<StatusEffectHandler, SlowEffect> slowedEnemies;
        private List<StatusEffectHandler> removeBuffer;
        private HashSet<StatusEffectHandler> currentFrameSet;

        private Coroutine freezeCoroutine;

        // ============================
        // PROPERTIES
        // ============================

        public int SlowedEnemyCount => slowedEnemies?.Count ?? 0;
        public bool IsFreezeUnlocked => CurrentLevel >= 2;

        // ============================
        // INIT
        // ============================

        protected override void OnTowerAwake()
        {
            aoeBuffer = new Collider2D[maxDetect];
            slowedEnemies = new Dictionary<StatusEffectHandler, SlowEffect>();
            removeBuffer = new List<StatusEffectHandler>();
            currentFrameSet = new HashSet<StatusEffectHandler>();

            enemyFilter = new ContactFilter2D();
            enemyFilter.useLayerMask = true;
            enemyFilter.SetLayerMask(enemyLayer);
            enemyFilter.useTriggers = true;
        }

        // ============================
        // ATTACK LOGIC
        // ============================

        protected override void OnAttack(GameObject target, TowerLevelData stats)
        {
            int hitCount = 0;

            // Bỏ qua target đơn lẻ, quét qua toàn bộ quái đang nằm trong vùng hào quang
            foreach (var kvp in slowedEnemies)
            {
                StatusEffectHandler handler = kvp.Key;

                if (handler != null && handler.gameObject.activeInHierarchy)
                {
                    IDamageable damageable = target.GetComponent<IDamageable>();

                    if (damageable != null)
                    {
                        damageable.TakeDamage(stats.damage);
                    }
                    hitCount++;
                }
            }

            if (hitCount > 0)
            {
                Debug.Log($"<color=cyan>[IceTower]</color> AoE Frost Wave trúng <b>{hitCount}</b> quái, gây {stats.damage} DMG/quái!");
            }
        }

        // ============================
        // AURA LÀM CHẬM (MỖI FRAME)
        // ============================

        protected override void OnTowerUpdate()
        {
            UpdateAoeSlow();
        }

        private void UpdateAoeSlow()
        {
            TowerLevelData stats = CurrentStats;
            if (stats.slowPercent <= 0f) return;

            int count = Physics2D.OverlapCircle(transform.position, stats.range, enemyFilter, aoeBuffer);
            currentFrameSet.Clear();

            // 1. Quét tìm và gắn bùa cho quái mới
            for (int i = 0; i < count; i++)
            {
                Collider2D col = aoeBuffer[i];
                if (col == null || !col.gameObject.activeInHierarchy) continue;
                if (!col.TryGetComponent(out StatusEffectHandler handler)) continue;

                currentFrameSet.Add(handler);

                // Nếu quái chưa có bùa của tháp này -> Tạo & Gắn bùa
                if (!slowedEnemies.ContainsKey(handler))
                {
                    // Thời gian -1f tức là Aura vĩnh viễn (đến khi ra khỏi vòng)
                    SlowEffect auraSlow = new SlowEffect(stats.slowPercent, -1f);
                    handler.AddEffect(auraSlow);

                    // Ghi nhớ lại instance bùa để thu hồi sau
                    slowedEnemies.Add(handler, auraSlow);
                }
            }

            // 2. Gỡ bùa nếu quái đã đi ra ngoài tầm quét (hoặc bị tiêu diệt)
            removeBuffer.Clear();
            foreach (var kvp in slowedEnemies)
            {
                StatusEffectHandler handler = kvp.Key;
                SlowEffect slowInstance = kvp.Value; // Lấy đúng viên bùa đã phát

                if (handler == null || !handler.gameObject.activeInHierarchy || !currentFrameSet.Contains(handler))
                {
                    if (handler != null)
                    {
                        handler.RemoveEffect(slowInstance);
                    }
                    removeBuffer.Add(handler);
                }
            }

            // Dọn dẹp danh sách
            for (int i = 0; i < removeBuffer.Count; i++)
            {
                slowedEnemies.Remove(removeBuffer[i]);
            }
        }

        // ============================
        // UPGRADE & ABILITY
        // ============================

        protected override void OnUpgraded()
        {
            RefreshAllSlows();

            if (CurrentLevel == 2)
            {
                if (freezeCoroutine != null) StopCoroutine(freezeCoroutine);
                freezeCoroutine = StartCoroutine(FreezeProc());
            }
        }

        private void RefreshAllSlows()
        {
            // Ép gỡ toàn bộ bùa cũ ra, ở Update tiếp theo nó sẽ tự Add bùa mới theo chỉ số nâng cấp
            foreach (var kvp in slowedEnemies)
            {
                if (kvp.Key != null && kvp.Key.gameObject.activeInHierarchy)
                {
                    kvp.Key.RemoveEffect(kvp.Value);
                }
            }
            slowedEnemies.Clear();
        }

        private IEnumerator FreezeProc()
        {
            yield return new WaitForSeconds(1f);

            while (true)
            {
                yield return new WaitForSeconds(freezeCheckInterval);

                TowerLevelData stats = CurrentStats;
                if (stats.freezeChance <= 0f) continue;

                if (Random.value < stats.freezeChance)
                {
                    int frozenCount = 0;

                    foreach (var kvp in slowedEnemies)
                    {
                        if (kvp.Key == null || !kvp.Key.gameObject.activeInHierarchy) continue;

                        // Add Freeze Effect
                        kvp.Key.AddEffect(new FreezeEffect(stats.freezeDuration));
                        frozenCount++;
                    }

                    if (frozenCount > 0)
                    {
                        Debug.Log($"<color=cyan>[IceTower]</color> ❄ FREEZE PROC! Đóng băng <b>{frozenCount}</b> quái trong {stats.freezeDuration}s!");
                    }
                }
            }
        }

        // ============================
        // CLEANUP
        // ============================

        protected override void OnDisable()
        {
            base.OnDisable();
            // Thu hồi toàn bộ bùa khi tháp bị bán/vô hiệu hóa
            if (slowedEnemies != null)
            {
                foreach (var kvp in slowedEnemies)
                {
                    if (kvp.Key != null && kvp.Key.gameObject.activeInHierarchy)
                    {
                        kvp.Key.RemoveEffect(kvp.Value);
                    }
                }
                slowedEnemies.Clear();
            }

            if (freezeCoroutine != null)
            {
                StopCoroutine(freezeCoroutine);
                freezeCoroutine = null;
            }
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
                string label = $"Slow: {CurrentStats.slowPercent * 100}%";
                if (IsFreezeUnlocked) label += $" | Freeze: {CurrentStats.freezeChance * 100}%";
                label += $"\nSlowed: {SlowedEnemyCount}";

                UnityEditor.Handles.Label(transform.position + Vector3.up * 0.8f, label);
            }
#endif
        }
    }
}