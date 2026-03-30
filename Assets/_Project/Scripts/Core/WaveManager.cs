using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Core
{
    public class WaveManager : ManagerBase<WaveManager>
    {
        // ============================
        // CẤU HÌNH
        // ============================
        [Header("Wave Settings")]

        [Tooltip("Tổng số wave trong level này")]
        [SerializeField] private int totalWaves = 8;

        [Tooltip("Thời gian nghỉ giữa các wave (giây)")]
        [SerializeField] private float timeBetweenWaves = 15f;

        // ============================
        // STATE
        // ============================
        public int CurrentWaveIndex { get; private set; } = -1;
        public int TotalWaves => totalWaves;
        public bool IsWaveActive {  get; private set; } = false;
        private HashSet<GameObject> activeEnemies = new HashSet<GameObject>();

        // ============================
        // EVENT SUBSCRIBE
        // ============================
        private void OnEnable()
        {
            GameEvents.OnEnemySpawned += HandleEnemySpawned;
            GameEvents.OnEnemyDied += HandleEnemyDied;
            GameEvents.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemySpawned -= HandleEnemySpawned;
            GameEvents.OnEnemyDied -= HandleEnemyDied;
            GameEvents.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        }

        // ============================
        // PUBLIC METHODS
        // ============================
        public void StartNextWave()
        {
            if (IsWaveActive)
            {
                return;
            }

            CurrentWaveIndex++;

            if(CurrentWaveIndex >= totalWaves)
            {
                return;
            }

            IsWaveActive = true;
            GameEvents.RaiseWaveStarted(CurrentWaveIndex);

            // TODO
        }
        public void ForceCompleteCurrentWave()
        {
            if (!IsWaveActive) return;

            IsWaveActive = false;
            activeEnemies.Clear();
            GameEvents.RaiseWaveCompleted(CurrentWaveIndex);

            // Kiểm tra đã hết tất cả wave chưa
            if (CurrentWaveIndex + 1 >= totalWaves)
            {
                GameEvents.RaiseAllWavesCleared();
            }
        }
        public int ActiveEnemyCount => activeEnemies.Count;

        // ============================
        // EVENT HANDLERS
        // ============================
        private void HandleEnemySpawned(GameObject enemy)
        {
            activeEnemies.Add(enemy);
            
        }
        private void HandleEnemyDied(GameObject enemy)
        {
            activeEnemies.Remove(enemy);
            CheckWaveComplete();
        }
        private void HandleEnemyReachedEnd(GameObject enemy)
        {
            activeEnemies.Remove(enemy);
            CheckWaveComplete();
        }

        // ============================
        // PRIVATE METHODS
        // ============================
        private void CheckWaveComplete()
        {
            if (!IsWaveActive) return;

            if (activeEnemies.Count > 0) return;

            IsWaveActive = false;
            GameEvents.RaiseWaveCompleted(CurrentWaveIndex);

            if(CurrentWaveIndex + 1 >= totalWaves)
            {
                GameEvents.RaiseAllWavesCleared();
            }
        }
    }
}
