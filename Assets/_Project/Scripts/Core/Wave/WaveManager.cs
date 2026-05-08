using System.Collections;
using System.Collections.Generic;
using TowerDefense.Enemies;
using TowerDefense.Utils;
using UnityEngine;

namespace TowerDefense.Core
{
    public enum WaveState
    {
        Prepare,
        Spawning,
        WaitingForClear,
        Completed
    }

    public class WaveManager : ManagerBase<WaveManager>
    {
        // ============================
        // CONFIG
        // ============================

        [Header("Wave Data")]

        [Tooltip("Data cấu hình toàn bộ wave cho màn hiện tại.")]
        [SerializeField] private WaveData waveData;

        [Tooltip("Path mà enemy sẽ đi theo.")]
        [SerializeField] private WaypointPath path;

        [Header("Boss Encounter")]

        [Tooltip("Boss sẽ được gọi ra sau khi wave cuối cùng được dọn sạch. Để trống nếu màn không có boss.")]
        [SerializeField] private BossBase bossPrefab;

        [Tooltip("Nếu > 0, boss spawn lệch ngẫu nhiên trong lòng đường giống enemy thường.")]
        [SerializeField, Range(0f, 1f)] private float bossSpawnOffsetWidthMultiplier = 0f;

        [Header("Debug")]

        [SerializeField] private bool logDebug = true;

        // ============================
        // STATE
        // ============================

        private readonly HashSet<GameObject> activeEnemies = new HashSet<GameObject>();

        private Coroutine countdownCoroutine;
        private Coroutine waveRoutine;

        private int runningSpawnGroups;
        private bool levelStarted;
        private bool bossEncounterStarted;
        private bool bossEncounterCompleted;
        private bool bossEncounterFailed;

        // ============================
        // PROPERTIES
        // ============================

        public int CurrentWaveIndex { get; private set; } = -1;
        public int TotalWaves => waveData != null ? waveData.WaveCount : 0;

        public WaveState State { get; private set; } = WaveState.Prepare;

        public bool IsWaveActive =>
            State == WaveState.Spawning || State == WaveState.WaitingForClear;

        public bool IsCountdownActive =>
            State == WaveState.Prepare;

        public float CountdownRemaining { get; private set; }

        public int ActiveEnemyCount => activeEnemies.Count;

        public int UpcomingWaveIndex =>
            Mathf.Clamp(CurrentWaveIndex + 1, 0, Mathf.Max(0, TotalWaves - 1));

        // ============================
        // UNITY LIFECYCLE
        // ============================

        private void Start()
        {
            TryStartLevelFlow();
        }

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

            StopAllRunningCoroutines();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (waveData == null)
            {
                Debug.LogWarning("[WaveManager] Chua gan WaveData.", this);
            }

            if (path == null)
            {
                Debug.LogWarning("[WaveManager] Chua gan WaypointPath.", this);
            }

            if (bossPrefab != null && bossPrefab.GetComponent<BossSkillController>() == null)
            {
                Debug.LogWarning("[WaveManager] Boss prefab khong co BossSkillController.", this);
            }
        }
#endif

        // ============================
        // PUBLIC API
        // ============================

        /// <summary>
        /// Dùng cho nút UI "Start Wave" / "Next Wave".
        ///
        /// Nếu đang Prepare:
        /// - Bắt đầu wave ngay.
        /// - Tính thưởng dựa trên thời gian còn lại.
        /// </summary>
        public void StartNextWave()
        {
            if (waveData == null || path == null)
            {
                Debug.LogError("[WaveManager] Không thể StartNextWave vì thiếu WaveData hoặc WaypointPath.");
                return;
            }

            if (State == WaveState.Completed)
            {
                return;
            }

            if (!levelStarted)
            {
                TryStartLevelFlow(startFirstWaveImmediately: true);
                return;
            }

            if (State == WaveState.Prepare)
            {
                StartCountdownWaveEarly();
            }
        }

        /// <summary>
        /// Ép hoàn thành wave hiện tại. Chủ yếu dùng debug.
        ///
        /// Chỉ cho phép force khi wave hiện tại đã thật sự bắt đầu.
        /// Không cho force khi CurrentWaveIndex < 0 để tránh RaiseWaveCompleted(-1).
        /// Không cho force ở Prepare vì lúc đó wave tiếp theo chưa bắt đầu.
        /// </summary>
        public void ForceCompleteCurrentWave()
        {
            if (State == WaveState.Completed)
            {
                return;
            }

            if (!levelStarted || CurrentWaveIndex < 0)
            {
                if (logDebug)
                {
                    Debug.LogWarning("[WaveManager] Không thể ForceCompleteCurrentWave vì chưa có wave nào đang hoặc đã bắt đầu.");
                }

                return;
            }

            if (State != WaveState.Spawning && State != WaveState.WaitingForClear)
            {
                if (logDebug)
                {
                    Debug.LogWarning($"[WaveManager] Không thể ForceCompleteCurrentWave trong state {State}.");
                }

                return;
            }

            StopAllRunningCoroutines();

            activeEnemies.Clear();
            GameEvents.RaiseActiveEnemyCountChanged(activeEnemies.Count);

            runningSpawnGroups = 0;
            State = WaveState.WaitingForClear;

            CompleteCurrentWave();
        }

        // ============================
        // LEVEL FLOW
        // ============================

        private void TryStartLevelFlow(bool startFirstWaveImmediately = false)
        {
            if (levelStarted) return;

            if (waveData == null)
            {
                Debug.LogError("[WaveManager] Thiếu WaveData.");
                return;
            }

            if (path == null)
            {
                Debug.LogError("[WaveManager] Thiếu WaypointPath.");
                return;
            }

            if (waveData.WaveCount <= 0)
            {
                Debug.LogWarning("[WaveManager] WaveData không có wave nào.");

                State = WaveState.Completed;
                CountdownRemaining = 0f;

                GameEvents.RaiseAllWavesCleared();
                return;
            }

            levelStarted = true;
            CurrentWaveIndex = -1;
            CountdownRemaining = 0f;
            runningSpawnGroups = 0;
            bossEncounterStarted = false;
            bossEncounterCompleted = false;
            bossEncounterFailed = false;

            activeEnemies.Clear();
            GameEvents.RaiseActiveEnemyCountChanged(activeEnemies.Count);

            float prepareTime = startFirstWaveImmediately ? 0f : waveData.initialPreparationTime;
            BeginCountdownForNextWave(prepareTime);
        }

        private void BeginCountdownForNextWave(float duration)
        {
            if (CurrentWaveIndex + 1 >= TotalWaves)
            {
                State = WaveState.Completed;
                CountdownRemaining = 0f;

                GameEvents.RaiseAllWavesCleared();
                return;
            }

            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            countdownCoroutine = StartCoroutine(CountdownThenStartWave(duration));
        }

        private IEnumerator CountdownThenStartWave(float duration)
        {
            State = WaveState.Prepare;
            CountdownRemaining = Mathf.Max(0f, duration);

            int upcomingWaveIndex = CurrentWaveIndex + 1;

            if (logDebug)
            {
                Debug.Log($"[WaveManager] Prepare Wave {upcomingWaveIndex + 1}. Countdown = {CountdownRemaining:F1}s.");
            }

            GameEvents.RaiseWaveCountdownChanged(upcomingWaveIndex, CountdownRemaining);

            while (CountdownRemaining > 0f)
            {
                CountdownRemaining -= Time.deltaTime;

                if (CountdownRemaining < 0f)
                {
                    CountdownRemaining = 0f;
                }

                GameEvents.RaiseWaveCountdownChanged(upcomingWaveIndex, CountdownRemaining);

                yield return null;
            }

            countdownCoroutine = null;
            StartWaveInternal();
        }

        private void StartCountdownWaveEarly()
        {
            if (State != WaveState.Prepare) return;

            int upcomingWaveIndex = CurrentWaveIndex + 1;

            if (upcomingWaveIndex < 0 || upcomingWaveIndex >= TotalWaves)
            {
                if (logDebug)
                {
                    Debug.LogWarning($"[WaveManager] Không thể start wave sớm vì upcomingWaveIndex không hợp lệ: {upcomingWaveIndex}");
                }

                return;
            }

            int bonus = CalculateEarlyStartBonus(upcomingWaveIndex, CountdownRemaining);

            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            CountdownRemaining = 0f;
            GameEvents.RaiseWaveCountdownChanged(upcomingWaveIndex, CountdownRemaining);

            if (bonus > 0)
            {
                GameEvents.RaiseEarlyStartBonusAwarded(upcomingWaveIndex, bonus);

                if (logDebug)
                {
                    Debug.Log($"<color=yellow>[WaveManager]</color> Start wave sớm! Bonus = {bonus}");
                }
            }

            StartWaveInternal();
        }

        private int CalculateEarlyStartBonus(int waveIndex, float timeRemaining)
        {
            if (waveData == null) return 0;

            int bonusPerSecond = waveData.GetEarlyStartBonusPerSecond(waveIndex);
            return Mathf.CeilToInt(Mathf.Max(0f, timeRemaining) * bonusPerSecond);
        }

        // ============================
        // WAVE ROUTINE
        // ============================

        private void StartWaveInternal()
        {
            if (waveRoutine != null)
            {
                StopCoroutine(waveRoutine);
                waveRoutine = null;
            }

            waveRoutine = StartCoroutine(WaveRoutine());
        }

        private IEnumerator WaveRoutine()
        {
            CurrentWaveIndex++;

            if (CurrentWaveIndex < 0 || CurrentWaveIndex >= TotalWaves)
            {
                State = WaveState.Completed;
                CountdownRemaining = 0f;

                GameEvents.RaiseAllWavesCleared();
                yield break;
            }

            WaveDefinition wave = waveData.GetWave(CurrentWaveIndex);

            if (wave == null)
            {
                Debug.LogError($"[WaveManager] Wave {CurrentWaveIndex} bị null.");

                State = WaveState.WaitingForClear;
                CompleteCurrentWave();

                yield break;
            }

            State = WaveState.Spawning;
            runningSpawnGroups = 0;

            GameEvents.RaiseWaveStarted(CurrentWaveIndex);

            if (logDebug)
            {
                Debug.Log($"<color=cyan>[WaveManager]</color> START WAVE {CurrentWaveIndex + 1}: {wave.waveName}");
            }

            EnemySpawnGroup[] groups = wave.enemyGroups;

            if (groups != null)
            {
                for (int i = 0; i < groups.Length; i++)
                {
                    EnemySpawnGroup group = groups[i];

                    if (group == null) continue;
                    if (group.enemyPrefab == null) continue;
                    if (group.count <= 0) continue;

                    runningSpawnGroups++;
                    StartCoroutine(SpawnGroupRoutine(group));
                }
            }

            while (runningSpawnGroups > 0)
            {
                yield return null;
            }

            State = WaveState.WaitingForClear;
            GameEvents.RaiseWaveSpawnCompleted(CurrentWaveIndex);

            if (logDebug)
            {
                Debug.Log($"[WaveManager] Wave {CurrentWaveIndex + 1} đã spawn xong. Còn {activeEnemies.Count} enemy.");
            }

            CheckWaveComplete();

            waveRoutine = null;
        }

        private IEnumerator SpawnGroupRoutine(EnemySpawnGroup group)
        {
            if (group.startDelay > 0f)
            {
                yield return new WaitForSeconds(group.startDelay);
            }

            for (int i = 0; i < group.count; i++)
            {
                SpawnEnemy(group);

                if (group.spawnInterval > 0f && i < group.count - 1)
                {
                    yield return new WaitForSeconds(group.spawnInterval);
                }
            }

            runningSpawnGroups--;
        }

        // ============================
        // SPAWN
        // ============================

        private void SpawnEnemy(EnemySpawnGroup group)
        {
            if (group == null || group.enemyPrefab == null) return;

            if (PoolManager.Instance == null)
            {
                Debug.LogError("[WaveManager] Không tìm thấy PoolManager.Instance.");
                return;
            }

            Vector2 spawnPosition = GetRandomSpawnPosition(group.offsetWidthMultiplier);

            EnemyBase enemy = PoolManager.Instance.GetEnemy(group.enemyPrefab, spawnPosition);
            if (enemy == null) return;

            enemy.Initialize(path, spawnPosition, 1, group.speedMultiplier);

            GameEvents.RaiseEnemySpawned(enemy.gameObject);
        }

        private Vector2 GetRandomSpawnPosition(float offsetWidthMultiplier)
        {
            Vector2 center = path.GetSpawnPoint();

            if (path.Length < 2 || path.PathHalfWidth <= 0f)
            {
                return center;
            }

            Vector2 normal = path.GetSegmentNormal(1);

            float maxOffset = path.PathHalfWidth * Mathf.Clamp01(offsetWidthMultiplier);
            float offset = Random.Range(-maxOffset, maxOffset);

            return center + normal * offset;
        }

        // ============================
        // EVENT HANDLERS
        // ============================

        private void HandleEnemySpawned(GameObject enemy)
        {
            if (enemy == null) return;

            activeEnemies.Add(enemy);
            GameEvents.RaiseActiveEnemyCountChanged(activeEnemies.Count);
        }

        private void HandleEnemyDied(GameObject enemy)
        {
            if (enemy == null) return;

            activeEnemies.Remove(enemy);
            GameEvents.RaiseActiveEnemyCountChanged(activeEnemies.Count);

            CheckWaveComplete();
        }

        private void HandleEnemyReachedEnd(GameObject enemy)
        {
            if (enemy == null) return;

            if (bossEncounterStarted && !bossEncounterCompleted && enemy.GetComponent<BossBase>() != null)
            {
                bossEncounterFailed = true;

                if (EconomyManager.Instance != null && EconomyManager.Instance.CurrentLives > 0)
                {
                    EconomyManager.Instance.LoseLife(EconomyManager.Instance.CurrentLives);
                }
            }

            activeEnemies.Remove(enemy);
            GameEvents.RaiseActiveEnemyCountChanged(activeEnemies.Count);

            CheckWaveComplete();
        }

        // ============================
        // COMPLETE
        // ============================

        private void CheckWaveComplete()
        {
            if (State != WaveState.WaitingForClear) return;
            if (runningSpawnGroups > 0) return;
            if (activeEnemies.Count > 0) return;

            if (bossEncounterStarted && !bossEncounterCompleted)
            {
                if (bossEncounterFailed)
                {
                    State = WaveState.Completed;
                    CountdownRemaining = 0f;
                    return;
                }

                FinishBossEncounter();
                return;
            }

            CompleteCurrentWave();
        }

        private void CompleteCurrentWave()
        {
            if (State == WaveState.Completed) return;

            if (CurrentWaveIndex < 0 || CurrentWaveIndex >= TotalWaves)
            {
                if (logDebug)
                {
                    Debug.LogWarning($"[WaveManager] CompleteCurrentWave bị chặn vì CurrentWaveIndex không hợp lệ: {CurrentWaveIndex}");
                }

                return;
            }

            GameEvents.RaiseWaveCompleted(CurrentWaveIndex);

            if (logDebug)
            {
                Debug.Log($"<color=green>[WaveManager]</color> COMPLETE WAVE {CurrentWaveIndex + 1}");
            }

            bool hasNextWave = CurrentWaveIndex + 1 < TotalWaves;

            if (!hasNextWave)
            {
                if (CanStartBossEncounter())
                {
                    StartBossEncounter();
                    return;
                }

                State = WaveState.Completed;
                CountdownRemaining = 0f;

                GameEvents.RaiseAllWavesCleared();

                if (logDebug)
                {
                    Debug.Log("<color=green>[WaveManager]</color> ALL WAVES CLEARED!");
                }

                return;
            }

            float prepareTime = waveData.GetPrepareTimeAfterWave(CurrentWaveIndex);
            BeginCountdownForNextWave(prepareTime);
        }

        private bool CanStartBossEncounter()
        {
            return bossPrefab != null && !bossEncounterStarted && !bossEncounterCompleted;
        }

        private void StartBossEncounter()
        {
            bossEncounterStarted = true;
            bossEncounterFailed = false;
            State = WaveState.WaitingForClear;

            if (logDebug)
            {
                Debug.Log($"<color=magenta>[WaveManager]</color> START BOSS ENCOUNTER: {bossPrefab.name}");
            }

            Vector2 spawnPosition = GetRandomSpawnPosition(bossSpawnOffsetWidthMultiplier);
            BossBase boss = PoolManager.Instance != null
                ? PoolManager.Instance.GetEnemy(bossPrefab, spawnPosition)
                : null;

            if (boss == null)
            {
                Debug.LogError("[WaveManager] Không spawn được boss. Kết thúc màn theo flow cũ.");
                FinishBossEncounter();
                return;
            }

            boss.Initialize(path, spawnPosition, 1);
            GameEvents.RaiseEnemySpawned(boss.gameObject);
        }

        private void FinishBossEncounter()
        {
            bossEncounterCompleted = true;
            State = WaveState.Completed;
            CountdownRemaining = 0f;

            GameEvents.RaiseAllWavesCleared();

            if (logDebug)
            {
                Debug.Log("<color=green>[WaveManager]</color> BOSS CLEARED. LEVEL COMPLETE FLOW UNLOCKED.");
            }
        }

        // ============================
        // CLEANUP
        // ============================

        private void StopAllRunningCoroutines()
        {
            if (countdownCoroutine != null)
            {
                StopCoroutine(countdownCoroutine);
                countdownCoroutine = null;
            }

            if (waveRoutine != null)
            {
                StopCoroutine(waveRoutine);
                waveRoutine = null;
            }

            StopAllCoroutines();

            runningSpawnGroups = 0;
        }
    }
}
