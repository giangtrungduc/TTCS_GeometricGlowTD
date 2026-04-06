using System.Reflection;
using TowerDefense.Utils;
using UnityEngine;

namespace TowerDefense.Enemies.Abilities
{
    [RequireComponent(typeof(PathFollower))]
    public class SummonAbility : MonoBehaviour
    {
        // ===========================
        // CẤU HÌNH
        // ===========================

        [Header("Summon Settings")]

        [Tooltip("Prefab của quái vật được triệu hồi")]
        [SerializeField] private EnemyBase minionPrefab;

        [Tooltip("Số lượng quái sinh ra mỗi nhịp đẻ")]
        [SerializeField] private int summonCount = 3;

        [Tooltip("Tối đa số quái được đẻ trong cả đời (0 = Vô hạn, dùng cho Boss)")]
        [SerializeField] private int maxTotal = 6;

        [Tooltip("Thời gian chờ đẻ quái (giây)")]
        [SerializeField] private float interval = 6f;

        // ===========================
        // STATE & REFERENCES
        // ===========================

        private int currentSummonedCount = 0;
        private PathFollower myPathFollower;

        // ===========================
        // INIT
        // ===========================

        private void Awake()
        {
            myPathFollower = GetComponent<PathFollower>();
        }

        // ===========================
        // LIFECYCLE
        // ===========================

        private void OnEnable()
        {
            currentSummonedCount = 0;
            InvokeRepeating(nameof(Summon), interval, interval);
        }

        private void OnDisable()
        {
            CancelInvoke(nameof(Summon));
        }

        // ===========================
        // ABILITY LOGIC
        // ===========================

        private void Summon()
        {
            // Kiểm tra giới hạn (Nếu MaxTotal = 0 thì bỏ qua check này)
            if (maxTotal > 0 && currentSummonedCount >= maxTotal)
            {
                CancelInvoke(nameof(Summon));
                return;
            }

            if (PoolManager.Instance == null || minionPrefab == null) return;

            // TODO: Ngày 18/19 - Sẽ thay bằng WaveManager.Instance.SpawnAt()
            // Tạm thời dùng PoolManager trực tiếp và hack đọc Path bằng Reflection để tự test
            WaypointPath currentPath = GetPathHack();

            for (int i = 0; i < summonCount; i++)
            {
                if (maxTotal > 0 && currentSummonedCount >= maxTotal) break;

                // Tọa độ tỏa ra xung quanh Summoner
                Vector3 offset = (Vector3)Random.insideUnitCircle * 0.5f;
                Vector3 spawnPos = transform.position + offset;

                EnemyBase minion = PoolManager.Instance.GetEnemy<EnemyBase>(minionPrefab, spawnPos);

                if (minion != null && currentPath != null)
                {
                    minion.Initialize(currentPath);
                    currentSummonedCount++;
                }
            }

            Debug.Log($"<color=#FF00FF>[Summoner]</color> '{name}' đã đẻ {summonCount} đệ tớ! (Tổng: {currentSummonedCount}/{maxTotal})");
        }

        // ===========================
        // HELPER
        // ===========================

        /// <summary> Dùng Reflection để đọc biến path bị ẩn của PathFollower </summary>
        private WaypointPath GetPathHack()
        {
            if (myPathFollower == null) return null;
            FieldInfo field = typeof(PathFollower).GetField("currentPath", BindingFlags.NonPublic | BindingFlags.Instance);
            return field != null ? (WaypointPath)field.GetValue(myPathFollower) : null;
        }
    }
}