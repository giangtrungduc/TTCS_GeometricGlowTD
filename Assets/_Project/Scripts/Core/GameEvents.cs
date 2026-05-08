using System;
using UnityEngine;

namespace TowerDefense.Core
{
    public static class GameEvents
    {
        /// <summary>
        /// Báo thay đổi trạng thái game tổng thể (Playing, Paused, Win, Lose).
        /// </summary>
        public static event Action<GameState> OnGameStateChanged;

        /// <summary>
        /// Báo khi level kết thúc với đầy đủ kết quả trận.
        /// </summary>
        public static event Action<LevelResult> OnLevelCompleted;

        /// <summary>
        /// Báo khi lượng vàng hiện tại thay đổi.
        /// </summary>
        public static event Action<int> OnGoldChanged;

        /// <summary>
        /// Báo khi số mạng hiện tại thay đổi.
        /// </summary>
        public static event Action<int> OnLivesChanged;

        /// <summary>
        /// Báo khi một wave bắt đầu (tham số là index wave, 0-based).
        /// </summary>
        public static event Action<int> OnWaveStarted;

        /// <summary>
        /// Báo khi một wave hoàn thành (tham số là index wave, 0-based).
        /// </summary>
        public static event Action<int> OnWaveCompleted;

        /// <summary>
        /// Báo khi toàn bộ wave trong level đã hoàn thành.
        /// </summary>
        public static event Action OnAllWavesCleared;

        /// <summary>
        /// Báo cập nhật countdown trước wave kế tiếp.
        /// Param1: upcomingWaveIndex (0-based), Param2: timeRemaining (giây).
        /// </summary>
        public static event Action<int, float> OnWaveCountdownChanged;

        /// <summary>
        /// Báo khi người chơi start wave sớm và nhận thưởng.
        /// Param1: waveIndex (0-based), Param2: bonusAmount.
        /// </summary>
        public static event Action<int, int> OnEarlyStartBonusAwarded;

        /// <summary>
        /// Báo thay đổi số enemy đang còn sống trên map.
        /// </summary>
        public static event Action<int> OnActiveEnemyCountChanged;

        /// <summary>
        /// Báo khi wave đã spawn xong toàn bộ enemy (không đồng nghĩa đã clear wave).
        /// </summary>
        public static event Action<int> OnWaveSpawnCompleted;

        /// <summary>
        /// Báo khi một enemy được spawn.
        /// </summary>
        public static event Action<GameObject> OnEnemySpawned;

        /// <summary>
        /// Báo khi enemy chết.
        /// </summary>
        public static event Action<GameObject> OnEnemyDied;

        /// <summary>
        /// Báo khi enemy đi đến đích.
        /// </summary>
        public static event Action<GameObject> OnEnemyReachedEnd;

        /// <summary>
        /// Báo khi đặt tháp mới thành công.
        /// </summary>
        public static event Action<GameObject> OnTowerPlaced;

        /// <summary>
        /// Báo khi nâng cấp tháp thành công.
        /// </summary>
        public static event Action<GameObject> OnTowerUpgraded;

        /// <summary>
        /// Báo khi bán tháp.
        /// </summary>
        public static event Action<GameObject> OnTowerSold;

        public static void RaiseGameStateChanged(GameState state)
        {
            Debug.Log($"[GameEvents] GameState -> {state}");
            OnGameStateChanged?.Invoke(state);
        }

        public static void RaiseLevelCompleted(LevelResult levelResult)
        {
            Debug.Log($"[GameEvents] LevelCompleted -> {levelResult}");
            OnLevelCompleted?.Invoke(levelResult);
        }

        public static void RaiseGoldChanged(int currentGold)
        {
            OnGoldChanged?.Invoke(currentGold);
        }

        public static void RaiseLivesChanged(int currentLives)
        {
            OnLivesChanged?.Invoke(currentLives);
        }

        public static void RaiseWaveStarted(int waveIndex)
        {
            Debug.Log($"[GameEvents] Wave {waveIndex + 1} Started");
            OnWaveStarted?.Invoke(waveIndex);
        }

        public static void RaiseWaveCompleted(int waveIndex)
        {
            Debug.Log($"[GameEvents] Wave {waveIndex + 1} Completed");
            OnWaveCompleted?.Invoke(waveIndex);
        }

        public static void RaiseAllWavesCleared()
        {
            Debug.Log("[GameEvents] All Waves Cleared");
            OnAllWavesCleared?.Invoke();
        }

        public static void RaiseWaveCountdownChanged(int upcomingWaveIndex, float timeRemaining)
        {
            OnWaveCountdownChanged?.Invoke(upcomingWaveIndex, timeRemaining);
        }

        public static void RaiseEarlyStartBonusAwarded(int waveIndex, int bonusAmount)
        {
            OnEarlyStartBonusAwarded?.Invoke(waveIndex, bonusAmount);
        }

        public static void RaiseActiveEnemyCountChanged(int activeEnemyCount)
        {
            OnActiveEnemyCountChanged?.Invoke(activeEnemyCount);
        }

        public static void RaiseWaveSpawnCompleted(int waveIndex)
        {
            OnWaveSpawnCompleted?.Invoke(waveIndex);
        }

        public static void RaiseEnemySpawned(GameObject enemy)
        {
            OnEnemySpawned?.Invoke(enemy);
        }

        public static void RaiseEnemyDied(GameObject enemy)
        {
            OnEnemyDied?.Invoke(enemy);
        }

        public static void RaiseEnemyReachedEnd(GameObject enemy)
        {
            OnEnemyReachedEnd?.Invoke(enemy);
        }

        public static void RaiseTowerPlaced(GameObject tower)
        {
            Debug.Log($"[GameEvents] Tower Placed: {tower.name}");
            OnTowerPlaced?.Invoke(tower);
        }

        public static void RaiseTowerUpgraded(GameObject tower)
        {
            Debug.Log($"[GameEvents] Tower Upgraded: {tower.name}");
            OnTowerUpgraded?.Invoke(tower);
        }

        public static void RaiseTowerSold(GameObject tower)
        {
            Debug.Log($"[GameEvents] Tower Sold: {tower.name}");
            OnTowerSold?.Invoke(tower);
        }

        /// <summary>
        /// Xóa toàn bộ subscriber của event tĩnh, nên gọi khi đổi scene để tránh leak listener cũ.
        /// </summary>
        public static void ClearAllEvents()
        {
            Debug.Log("[GameEvents] Clearing all event subscriptions");

            OnGameStateChanged = null;
            OnLevelCompleted = null;

            OnGoldChanged = null;
            OnLivesChanged = null;

            OnWaveStarted = null;
            OnWaveCompleted = null;
            OnAllWavesCleared = null;
            OnWaveCountdownChanged = null;
            OnEarlyStartBonusAwarded = null;
            OnActiveEnemyCountChanged = null;
            OnWaveSpawnCompleted = null;

            OnEnemySpawned = null;
            OnEnemyDied = null;
            OnEnemyReachedEnd = null;

            OnTowerPlaced = null;
            OnTowerUpgraded = null;
            OnTowerSold = null;
        }
    }
}
