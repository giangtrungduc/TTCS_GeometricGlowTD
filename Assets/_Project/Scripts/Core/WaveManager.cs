using System.Collections;
using TowerDefense.Enemies;
using TowerDefense.Pooling;
using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Chịu trách nhiệm đọc WaveData và gọi hàm đẻ quái từ SimplePool.
    /// Nó tự chờ đến khi quái trên màn hình chết hết mới gọi Wave tiếp theo.
    /// </summary>
    public class WaveManager : MonoBehaviour
    {
        [SerializeField] private WaveData[] waves;
        [Tooltip("Kéo object chứa Script Path vào đây để cung cấp đường đi cho quái")]
        [SerializeField] private Path path;
        [SerializeField] private float timeBetweenWaves = 3f;

        private int currentWaveIndex = 0;
        private bool isSpawning = false;
        
        private bool skipDelay = false;
        private float waveDelayTimer = 0f;
        public float WaveDelayTimer => waveDelayTimer; // Công khai để mốt UI làm thanh đếm lùi

        // Các biến công khai cho UI
        public int CurrentWaveIndex => currentWaveIndex;
        public int TotalWaves => waves != null ? waves.Length : 0;
        public bool IsWaitingForWave => waveDelayTimer > 0 && !isSpawning;
        public int CurrentBonusGold => IsWaitingForWave ? Mathf.FloorToInt(waveDelayTimer * 2f) : 0;

        private void Start()
        {
            if (waves == null || waves.Length == 0) return;
            StartCoroutine(WaveRoutine());
        }

        private IEnumerator WaveRoutine()
        {
            while (currentWaveIndex < waves.Length && !GameManager.Instance.IsGameOver)
            {
                // Chờ trước khi bắt đầu Wave mới (Có thể ngắt quãng nếu bấm nút)
                skipDelay = false;
                waveDelayTimer = timeBetweenWaves;

                while (waveDelayTimer > 0 && !skipDelay && !GameManager.Instance.IsGameOver)
                {
                    waveDelayTimer -= Time.deltaTime;
                    yield return null;
                }

                isSpawning = true;
                WaveData currentWave = waves[currentWaveIndex];

                // Duyệt qua từng nhóm quái
                foreach (var group in currentWave.spawnGroups)
                {
                    for (int i = 0; i < group.count; i++)
                    {
                        if (GameManager.Instance.IsGameOver) yield break;

                        SpawnEnemy(group.enemyPrefab);
                        yield return new WaitForSeconds(group.spawnInterval);
                    }
                    
                    yield return new WaitForSeconds(group.postGroupDelay);
                }

                isSpawning = false;

                // Chờ cho đến khi không còn bóng dáng Enemy nào trên bản đồ
                while (FindObjectsByType<EnemyBase>(FindObjectsSortMode.None).Length > 0 && !GameManager.Instance.IsGameOver)
                {
                    yield return new WaitForSeconds(1f);
                }

            // Cấp tiền thưởng và lên Wave
            GameManager.Instance.AddGold(currentWave.goldRewardEndWave);
            currentWaveIndex++;
        }

        // Hoàn thành tất cả các đợt mà vẫn chưa chết
        if (!GameManager.Instance.IsGameOver)
        {
            GameManager.Instance.Victory();
        }
    }

    /// <summary>
    /// Hàm này sẽ được gán vào sự kiện OnClick của nút "Next Wave" trên UI.
    /// Nó sẽ lập tức ép Wave đẻ ra và thưởng tiền cho độ liều lĩnh của người chơi.
    /// </summary>
    public void StartNextWaveEarly()
    {
        if (isSpawning || GameManager.Instance.IsGameOver) return;
        
        if (waveDelayTimer > 0)
        {
            // Thưởng tiền: Lấy từ biến tính sẵn
            int bonusGold = CurrentBonusGold;
            if (bonusGold > 0)
            {
                GameManager.Instance.AddGold(bonusGold);
                Debug.Log($"Bắt đầu sớm! Thưởng nóng {bonusGold} Gold.");
            }
            
            skipDelay = true;
            waveDelayTimer = 0;
        }
    }

    private void SpawnEnemy(GameObject prefab)
        {
            if (prefab == null || path == null || path.waypoints.Length == 0) return;

            // Đẻ quái tại tọa độ của điểm đầu tiên
            GameObject obj = SimplePool.Instance.Get(prefab, path.waypoints[0].position, Quaternion.identity);
            EnemyBase enemy = obj.GetComponent<EnemyBase>();

            if (enemy != null)
            {
                // Truyền mảng điểm đi và gán một lane offset ngẫu nhiên (-0.3f tới 0.3f) cho đội hình tản ra
                enemy.Initialize(path.waypoints, Random.Range(-0.3f, 0.3f));
            }
        }
    }
}
