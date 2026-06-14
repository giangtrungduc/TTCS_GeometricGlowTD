using UnityEngine;

namespace TowerDefense.Enemies
{
    [CreateAssetMenu(fileName = "NewEnemyData", menuName = "TD/Enemy Data")]
    public class EnemyData : ScriptableObject
    {
        public float maxHp = 100f;
        public float moveSpeed = 2f;
        public int goldReward = 10;
        public int livesCost = 1;
        public GameObject deathVfxPrefab;
    }
}
