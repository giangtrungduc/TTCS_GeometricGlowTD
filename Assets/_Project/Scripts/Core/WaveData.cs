using UnityEngine;

namespace TowerDefense.Core
{
    [System.Serializable]
    public class WaveSpawnGroup
    {
        public GameObject enemyPrefab;
        public int count = 5;
        [Tooltip("Khoảng cách thời gian đẻ từng con quái (giây)")]
        public float spawnInterval = 1f;
        [Tooltip("Số giây chờ SAU KHI đẻ xong nhóm này trước khi đẻ nhóm tiếp theo")]
        public float postGroupDelay = 2f;
    }

    [CreateAssetMenu(fileName = "NewWaveData", menuName = "TD/Wave Data")]
    public class WaveData : ScriptableObject
    {
        public WaveSpawnGroup[] spawnGroups;
        [Tooltip("Thưởng thêm tiền khi giết sạch đợt này")]
        public int goldRewardEndWave = 50;
    }
}
