using System.Collections;
using TowerDefense.Core;
using TowerDefense.Utils;
using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Triệu hồi quái con mỗi khi boss đi qua một mốc HP cấu hình sẵn.
    /// </summary>
    public class SummonOnHealthChunkSkill : BossAbility
    {
        [Header("Summon Thresholds")]
        [Tooltip("Danh sách mốc HP chuẩn hóa sẽ trigger summon. Ví dụ 0.8, 0.6, 0.4, 0.2.")]
        [SerializeField] private float[] summonThresholds = { 0.8f, 0.6f, 0.4f, 0.2f };

        [Header("Summon Payload")]
        [Tooltip("Prefab quái nhỏ sẽ được triệu hồi.")]
        [SerializeField] private EnemyBase minionPrefab;

        [Tooltip("Số lượng quái nhỏ triệu hồi mỗi lần trigger.")]
        [SerializeField, Min(1)] private int summonCountPerTrigger = 3;

        [Tooltip("Hệ số tốc độ áp lên minion nếu cần.")]
        [SerializeField, Min(0.1f)] private float minionSpeedMultiplier = 1f;

        private int nextThresholdIndex;
        private int pendingTriggerCount;

        public override void Initialize(BossBase boss, BossSkillController skillController)
        {
            base.Initialize(boss, skillController);
            nextThresholdIndex = 0;
            pendingTriggerCount = 0;
            SortThresholdsDescending();
        }

        public override void OnHealthThresholdCrossed(float thresholdNormalized, float previousHp, float currentHp)
        {
            if (minionPrefab == null || summonThresholds == null || summonThresholds.Length == 0)
            {
                return;
            }

            QueueThresholdTriggers(thresholdNormalized);
        }

        public override bool CanTrigger()
        {
            if (!base.CanTrigger())
            {
                return false;
            }

            return pendingTriggerCount > 0;
        }

        public override void Execute()
        {
            if (owner == null || minionPrefab == null || pendingTriggerCount <= 0)
            {
                return;
            }

            WaypointPath path = owner.PathFollower != null ? owner.PathFollower.CurrentPath : null;
            if (path == null || PoolManager.Instance == null)
            {
                return;
            }

            Vector2 spawnPosition = owner.transform.position;
            int waypointIndex = owner.PathFollower.CurrentWaypointIndex;

            for (int i = 0; i < summonCountPerTrigger; i++)
            {
                EnemyBase minion = PoolManager.Instance.GetEnemy(minionPrefab, spawnPosition);
                if (minion == null)
                {
                    continue;
                }

                minion.Initialize(path, spawnPosition, waypointIndex, minionSpeedMultiplier);
                GameEvents.RaiseEnemySpawned(minion.gameObject);
            }

            pendingTriggerCount = Mathf.Max(0, pendingTriggerCount - 1);
            MarkTriggered();

            if (pendingTriggerCount > 0)
            {
                StartCoroutine(QueueNextSummonCast());
            }
        }

        public override void ResetRuntimeState()
        {
            base.ResetRuntimeState();
            nextThresholdIndex = 0;
            pendingTriggerCount = 0;
            SortThresholdsDescending();
        }

        private IEnumerator QueueNextSummonCast()
        {
            yield return null;
            controller?.TryActivateAbility(this);
        }

        private void QueueThresholdTriggers(float thresholdNormalized)
        {
            while (nextThresholdIndex < summonThresholds.Length)
            {
                float configuredThreshold = summonThresholds[nextThresholdIndex];
                if (!Mathf.Approximately(configuredThreshold, thresholdNormalized))
                {
                    break;
                }

                pendingTriggerCount++;
                nextThresholdIndex++;
            }

            if (pendingTriggerCount > 0)
            {
                controller?.TryActivateAbility(this);
            }
        }

        private void SortThresholdsDescending()
        {
            if (summonThresholds == null || summonThresholds.Length == 0)
            {
                return;
            }

            for (int i = 0; i < summonThresholds.Length - 1; i++)
            {
                for (int j = i + 1; j < summonThresholds.Length; j++)
                {
                    if (summonThresholds[j] > summonThresholds[i])
                    {
                        float temp = summonThresholds[i];
                        summonThresholds[i] = summonThresholds[j];
                        summonThresholds[j] = temp;
                    }
                }

                summonThresholds[i] = Mathf.Clamp01(summonThresholds[i]);
            }

            summonThresholds[summonThresholds.Length - 1] = Mathf.Clamp01(summonThresholds[summonThresholds.Length - 1]);
        }
    }
}
