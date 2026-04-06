// ============================================================
// File: EnemySpawnerTest.cs
// Mục đích: Test sinh NGẪU NHIÊN nhiều loại quái vật chạy trên đường sử dụng PoolManager
// ============================================================

using System.Collections;
using UnityEngine;
using TowerDefense.Enemies;
using TowerDefense.Utils;

public class EnemySpawnerTest : MonoBehaviour
{
    [Header("Cấu hình Spawner")]
    [Tooltip("Danh sách các Prefab của Enemy (Normal, Fast, Tank, Healer, Summoner...)")]
    [SerializeField] private EnemyBase[] enemyPrefabs;

    [Tooltip("Đường đi cho Enemy")]
    [SerializeField] private WaypointPath testPath;

    [Tooltip("Thời gian chờ giữa 2 lần sinh quái (giây)")]
    [SerializeField] private float spawnInterval = 2f;

    [Tooltip("Tổng số quái sẽ sinh ra (để test Pool tái sử dụng)")]
    [SerializeField] private int maxEnemiesToSpawn = 20;

    private int spawnCount = 0;

    private void Start()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0 || testPath == null)
        {
            Debug.LogError("❌ Spawner thất bại: BẠN CHƯA GÁN EnemyPrefabs HOẶC WaypointPath!");
            return;
        }

        // Đảm bảo PoolManager đang tồn tại trong Scene
        if (PoolManager.Instance == null)
        {
            Debug.LogError("❌ KHÔNG TÌM THẤY PoolManager TRONG SCENE! Hãy tạo GameObject rỗng và gắn PoolManager vào.");
            return;
        }

        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        Debug.Log($"<color=green>BẮT ĐẦU SPAWN {maxEnemiesToSpawn} QUÁI VẬT TỪ {enemyPrefabs.Length} LOẠI, MỖI {spawnInterval} GIÂY...</color>");

        while (spawnCount < maxEnemiesToSpawn)
        {
            SpawnEnemy();
            yield return new WaitForSeconds(spawnInterval);
        }

        Debug.Log("<color=yellow>ĐÃ HOÀN THÀNH ĐỢT SPAWN TEST!</color>");
    }

    private void SpawnEnemy()
    {
        spawnCount++;
        Vector3 spawnPos = testPath.GetSpawnPoint(); // Lấy điểm đầu tiên của đường đi

        // Chọn ngẫu nhiên 1 loại quái trong danh sách
        int randomIndex = Random.Range(0, enemyPrefabs.Length);
        EnemyBase selectedPrefab = enemyPrefabs[randomIndex];

        // 1. LẤY QUÁI TỪ POOL (Thay vì dùng Instantiate)
        EnemyBase enemy = PoolManager.Instance.GetEnemy<EnemyBase>(selectedPrefab, spawnPos);

        if (enemy != null)
        {
            // 2. KHỞI TẠO ĐƯỜNG ĐI CHO QUÁI
            enemy.Initialize(testPath);
            Debug.Log($"[Spawner] Sinh quái thứ {spawnCount}: {enemy.name} (Hệ: {selectedPrefab.name})");
        }
    }
}