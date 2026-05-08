using TowerDefense.Utils;
using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Enemies
{
    [RequireComponent(typeof(EnemyBase))]
    public class SplitOnDeathAbility : MonoBehaviour, IEnemyDeathListener
    {
        [Header("Split Settings")]
        [SerializeField] private EnemyBase minionPrefab;
        [SerializeField] private int splitCount = 2;
        [SerializeField] private float splitRadius = 0.35f;
        [SerializeField] private float minionSpeedMultiplier = 1f;

        private EnemyBase owner;

        private void Awake()
        {
            owner = GetComponent<EnemyBase>();
        }

        public void OnEnemyDeath(EnemyBase enemy)
        {
            if (enemy == null || enemy != owner) return;
            if (PoolManager.Instance == null || minionPrefab == null) return;
            if (owner.PathFollower == null || owner.PathFollower.CurrentPath == null) return;
            if (splitCount <= 0) return;

            Vector2 center = owner.transform.position;
            WaypointPath path = owner.PathFollower.CurrentPath;
            int waypointIndex = owner.PathFollower.CurrentWaypointIndex;

            for (int i = 0; i < splitCount; i++)
            {
                Vector2 spawnPos = center + Random.insideUnitCircle * splitRadius;
                EnemyBase minion = PoolManager.Instance.GetEnemy(minionPrefab, spawnPos);
                if (minion != null)
                {
                    minion.Initialize(path, spawnPos, waypointIndex, minionSpeedMultiplier);
                    GameEvents.RaiseEnemySpawned(minion.gameObject);
                }
            }
        }
    }
}
