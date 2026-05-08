using TowerDefense.Enemies;
using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Cấu hình toàn bộ wave cho 1 màn chơi.
    /// Mỗi màn nên có 1 WaveData asset riêng.
    /// </summary>
    [CreateAssetMenu(fileName = "NewWaveData", menuName = "TD/Wave Data", order = 3)]
    public class WaveData : ScriptableObject
    {
        // ============================
        // LEVEL WAVE SETTINGS
        // ============================

        [Header("Level Wave Settings")]

        [Tooltip("Thời gian chờ trước wave 1.")]
        [Min(0f)]
        public float initialPreparationTime = 10f;

        [Tooltip("Thời gian chờ mặc định sau khi clear 1 wave, trước khi wave tiếp theo bắt đầu.")]
        [Min(0f)]
        public float defaultPrepareTimeAfterWave = 10f;

        [Tooltip("Thưởng mặc định mỗi giây còn lại nếu người chơi bắt đầu wave sớm.")]
        [Min(0)]
        public int defaultEarlyStartBonusPerSecond = 1;

        // ============================
        // WAVES
        // ============================

        [Header("Waves")]

        [Tooltip("Danh sách wave trong màn.")]
        public WaveDefinition[] waves = new WaveDefinition[0];

        // ============================
        // PROPERTIES
        // ============================

        public int WaveCount => waves != null ? waves.Length : 0;

        // ============================
        // PUBLIC API
        // ============================

        public WaveDefinition GetWave(int index)
        {
            if (waves == null || waves.Length == 0) return null;
            if (index < 0 || index >= waves.Length) return null;

            return waves[index];
        }

        public float GetPrepareTimeAfterWave(int waveIndex)
        {
            WaveDefinition wave = GetWave(waveIndex);
            if (wave == null) return defaultPrepareTimeAfterWave;

            return wave.useCustomPrepareTime
                ? Mathf.Max(0f, wave.prepareTimeAfterWave)
                : Mathf.Max(0f, defaultPrepareTimeAfterWave);
        }

        public int GetEarlyStartBonusPerSecond(int waveIndex)
        {
            WaveDefinition wave = GetWave(waveIndex);
            if (wave == null) return defaultEarlyStartBonusPerSecond;

            return wave.useCustomEarlyStartBonus
                ? Mathf.Max(0, wave.earlyStartBonusPerSecond)
                : Mathf.Max(0, defaultEarlyStartBonusPerSecond);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            initialPreparationTime = Mathf.Max(0f, initialPreparationTime);
            defaultPrepareTimeAfterWave = Mathf.Max(0f, defaultPrepareTimeAfterWave);
            defaultEarlyStartBonusPerSecond = Mathf.Max(0, defaultEarlyStartBonusPerSecond);

            if (waves == null) return;

            for (int i = 0; i < waves.Length; i++)
            {
                if (waves[i] == null) continue;
                waves[i].Validate();
            }
        }
#endif
    }

    // ========================================================================
    // WAVE DEFINITION
    // ========================================================================

    [System.Serializable]
    public class WaveDefinition
    {
        [Header("Identity")]

        public string waveName = "New Wave";

        [Header("Prepare Timing")]

        [Tooltip("Nếu bật, wave này dùng thời gian chờ riêng sau khi clear wave này.")]
        public bool useCustomPrepareTime = false;

        [Tooltip("Thời gian chờ sau khi clear wave này, trước khi wave tiếp theo bắt đầu.")]
        [Min(0f)]
        public float prepareTimeAfterWave = 10f;

        [Header("Early Start Bonus")]

        [Tooltip("Nếu bật, wave này dùng mức thưởng bắt đầu sớm riêng.")]
        public bool useCustomEarlyStartBonus = false;

        [Tooltip("Thưởng mỗi giây còn lại nếu người chơi bắt đầu wave sớm.")]
        [Min(0)]
        public int earlyStartBonusPerSecond = 1;

        [Header("Enemy Groups")]

        [Tooltip("Một wave có thể gồm nhiều nhóm quái spawn theo delay / interval khác nhau.")]
        public EnemySpawnGroup[] enemyGroups = new EnemySpawnGroup[0];

        public int TotalEnemyCount
        {
            get
            {
                if (enemyGroups == null) return 0;

                int total = 0;

                for (int i = 0; i < enemyGroups.Length; i++)
                {
                    if (enemyGroups[i] == null) continue;
                    total += Mathf.Max(0, enemyGroups[i].count);
                }

                return total;
            }
        }

        public void Validate()
        {
            if (string.IsNullOrWhiteSpace(waveName))
            {
                waveName = "New Wave";
            }

            prepareTimeAfterWave = Mathf.Max(0f, prepareTimeAfterWave);
            earlyStartBonusPerSecond = Mathf.Max(0, earlyStartBonusPerSecond);

            if (enemyGroups == null) return;

            for (int i = 0; i < enemyGroups.Length; i++)
            {
                if (enemyGroups[i] == null) continue;
                enemyGroups[i].Validate();

#if UNITY_EDITOR
                if (enemyGroups[i].enemyPrefab == null && enemyGroups[i].count > 0)
                {
                    Debug.LogWarning($"[WaveData] Wave '{waveName}' co group spawn count > 0 nhung thieu enemyPrefab.");
                }
#endif
            }
        }
    }

    // ========================================================================
    // ENEMY SPAWN GROUP
    // ========================================================================

    [System.Serializable]
    public class EnemySpawnGroup
    {
        [Header("Enemy")]

        [Tooltip("Prefab quái cần spawn.")]
        public EnemyBase enemyPrefab;

        [Tooltip("Số lượng quái trong group này.")]
        [Min(0)]
        public int count = 5;

        [Header("Timing")]

        [Tooltip("Delay tính từ lúc wave bắt đầu trước khi group này bắt đầu spawn.")]
        [Min(0f)]
        public float startDelay = 0f;

        [Tooltip("Thời gian giữa mỗi lần spawn trong group.")]
        [Min(0f)]
        public float spawnInterval = 0.5f;

        [Header("Modifier")]

        [Tooltip("Nhân tốc độ riêng cho group này. 1 = tốc độ gốc từ EnemyData.")]
        [Min(0.01f)]
        public float speedMultiplier = 1f;

        [Header("Spawn Offset")]

        [Tooltip("Tỉ lệ dùng lòng đường khi random offset. 1 = toàn bộ pathHalfWidth, 0.5 = nửa lòng đường.")]
        [Range(0f, 1f)]
        public float offsetWidthMultiplier = 1f;

        public void Validate()
        {
            count = Mathf.Max(0, count);
            startDelay = Mathf.Max(0f, startDelay);
            spawnInterval = Mathf.Max(0f, spawnInterval);
            speedMultiplier = Mathf.Max(0.01f, speedMultiplier);
            offsetWidthMultiplier = Mathf.Clamp01(offsetWidthMultiplier);
        }
    }
}
