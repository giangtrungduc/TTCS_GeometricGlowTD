using System;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace TowerDefense.Core
{
    public static class GameEvents
    {
        // ╔══════════════════════════════════════════════╗
        // ║           GAME STATE EVENTS                  ║
        // ║  Thay đổi trạng thái tổng thể của game       ║
        // ╚══════════════════════════════════════════════╝


        /// <summary>
        /// Phát khi game chuyển trạng thái.
        /// Subscriber: UI (pause menu, HUD), AudioManager
        /// Raiser: GameManager
        /// </summary>
        public static event Action<GameState> OnGameStateChanged;
        public static void RaiseGameStateChanged(GameState state)
        {
            Debug.Log($"[GameEvents] GameState → {state}");
            OnGameStateChanged?.Invoke(state);
        }

        /// <summary>
        /// Phát khi level kết thúc (thắng hoặc thua).
        /// Subscriber: EndScreen (hiển thị kết quả), SaveSystem (lưu sao)
        /// Raiser: GameManager
        /// </summary>
        public static event Action<LevelResult> OnLevelCompleted;
        public static void RaiseLevelCompleted(LevelResult levelResult)
        {
            Debug.Log($"[GameEvents] LevelCompleted → {levelResult}");
            OnLevelCompleted?.Invoke(levelResult);
        }

        // ╔══════════════════════════════════════════════╗
        // ║           ECONOMY EVENTS                     ║
        // ║  Gold và Lives thay đổi                      ║
        // ╚══════════════════════════════════════════════╝

        /// <summary>
        /// Phát khi gold thay đổi (mua tháp, bán tháp, enemy chết, wave bonus).
        /// Subscriber: HUDController (cập nhật text), RadialMenu (check affordable)
        /// Raiser: EconomyManager
        /// </summary>
        public static event Action<int> OnGoldChanged;

        public static void RaiseGoldChanged(int currentGold)
        {
            OnGoldChanged?.Invoke(currentGold);
        }

        /// <summary>
        /// Phát khi lives thay đổi (enemy qua đích).
        /// Subscriber: HUDController, GameManager (check lose condition)
        /// Raiser: EconomyManager
        /// </summary>
        public static event Action<int> OnLivesChanged;

        public static void RaiseLivesChanged(int currentLives)
        {
            OnLivesChanged?.Invoke(currentLives);
        }

        // ╔══════════════════════════════════════════════╗
        // ║           WAVE EVENTS                        ║
        // ║  Hệ thống wave / đợt quái                    ║
        // ╚══════════════════════════════════════════════╝

        /// <summary>
        /// Phát khi 1 wave bắt đầu spawn enemy.
        /// Subscriber: HUDController (hiện "Wave X/Y"), WaveUI (ẩn timer)
        /// Raiser: WaveManager
        /// </summary>
        public static event Action<int> OnWaveStarted;

        public static void RaiseWaveStarted(int waveIndex)
        {
            Debug.Log($"[GameEvents] Wave {waveIndex + 1} Started");
            OnWaveStarted?.Invoke(waveIndex);
        }

        /// <summary>
        /// Phát khi tất cả enemy trong 1 wave đã chết hoặc qua đích.
        /// Subscriber: WaveUI (hiện countdown đến wave tiếp), EconomyManager (wave bonus)
        /// Raiser: WaveManager
        /// </summary>
        public static event Action<int> OnWaveCompleted;

        public static void RaiseWaveCompleted(int waveIndex)
        {
            Debug.Log($"[GameEvents] Wave {waveIndex + 1} Completed");
            OnWaveCompleted?.Invoke(waveIndex);
        }

        /// <summary>
        /// Phát khi tất cả wave trong level đã hoàn thành.
        /// Subscriber: GameManager (→ tính sao → RaiseLevelCompleted)
        /// Raiser: WaveManager
        /// </summary>
        public static event Action OnAllWavesCleared;

        public static void RaiseAllWavesCleared()
        {
            Debug.Log("[GameEvents] All Waves Cleared!");
            OnAllWavesCleared?.Invoke();
        }

        // ╔══════════════════════════════════════════════╗
        // ║           ENEMY EVENTS                       ║
        // ║  Enemy spawn, chết, qua đích                 ║
        // ╚══════════════════════════════════════════════╝

        /// <summary>
        /// Phát khi 1 enemy được spawn (lấy từ pool và kích hoạt).
        /// Subscriber: WaveManager (thêm vào activeEnemies set)
        /// Raiser: WaveManager.SpawnEnemy()
        /// </summary>
        public static event Action<GameObject> OnEnemySpawned;

        public static void RaiseEnemySpawned(GameObject enemy)
        {
            OnEnemySpawned?.Invoke(enemy);
        }

        /// <summary>
        /// Phát khi enemy chết (HP <= 0).
        /// Subscriber: EconomyManager (cộng gold), WaveManager (xoá khỏi active set),
        ///             AudioManager (play SFX)
        /// Raiser: EnemyBase.Die()
        /// </summary>
        public static event Action<GameObject> OnEnemyDied;

        public static void RaiseEnemyDied(GameObject enemy)
        {
            OnEnemyDied?.Invoke(enemy);
        }

        /// <summary>
        /// Phát khi enemy đi hết waypoint (qua đích).
        /// Subscriber: EconomyManager (trừ lives), WaveManager (xoá khỏi active set)
        /// Raiser: PathFollower (khi đến waypoint cuối)
        /// </summary>
        public static event Action<GameObject> OnEnemyReachedEnd;

        public static void RaiseEnemyReachedEnd(GameObject enemy)
        {
            OnEnemyReachedEnd?.Invoke(enemy);
        }

        // ╔══════════════════════════════════════════════╗
        // ║           TOWER EVENTS                       ║
        // ║  Đặt, nâng cấp, bán tháp                     ║
        // ╚══════════════════════════════════════════════╝

        /// <summary>
        /// Phát khi player đặt tháp mới lên BuildSlot.
        /// Subscriber: AudioManager (play SFX), có thể dùng cho tutorial
        /// Raiser: BuildSlot.PlaceTower()
        /// </summary>
        public static event Action<GameObject> OnTowerPlaced;

        public static void RaiseTowerPlaced(GameObject tower)
        {
            Debug.Log($"[GameEvents] Tower Placed: {tower.name}");
            OnTowerPlaced?.Invoke(tower);
        }

        /// <summary>
        /// Phát khi player nâng cấp tháp (Cấp 1→2 hoặc 2→3).
        /// Subscriber: AudioManager, HUDController (có thể hiện upgrade effect)
        /// Raiser: BuildSlot.UpgradeTower()
        /// </summary>
        public static event Action<GameObject> OnTowerUpgraded;

        public static void RaiseTowerUpgraded(GameObject tower)
        {
            Debug.Log($"[GameEvents] Tower Upgraded: {tower.name}");
            OnTowerUpgraded?.Invoke(tower);
        }

        /// <summary>
        /// Phát khi player bán tháp (nhận lại 60% gold đã đầu tư).
        /// Subscriber: AudioManager, EconomyManager (hoàn gold)
        /// Raiser: BuildSlot.SellTower()
        /// </summary>
        public static event Action<GameObject> OnTowerSold;

        public static void RaiseTowerSold(GameObject tower)
        {
            Debug.Log($"[GameEvents] Tower Sold: {tower.name}");
            OnTowerSold?.Invoke(tower);
        }

        // ╔══════════════════════════════════════════════╗
        // ║           CLEANUP                            ║
        // ║  Reset tất cả event khi đổi scene            ║
        // ╚══════════════════════════════════════════════╝

        /// <summary>
        /// Gọi khi chuyển scene để đảm bảo không còn subscriber cũ.
        /// Static event KHÔNG tự reset khi đổi scene.
        /// GameManager.OnDestroy() sẽ gọi method này.
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

            OnEnemySpawned = null;
            OnEnemyDied = null;
            OnEnemyReachedEnd = null;

            OnTowerPlaced = null;
            OnTowerUpgraded = null;
            OnTowerSold = null;
        }
    }
}
