using UnityEngine;

namespace TowerDefense.Core
{
    public class EconomyManager : ManagerBase<EconomyManager>
    {
        public int CurrentGold { get; private set; }
        public int CurrentLives { get; private set; }

        public void Initialize(int startGold, int startLives)
        {
            CurrentGold = startGold;
            CurrentLives = startLives;

            GameEvents.RaiseGoldChanged(CurrentGold);
            GameEvents.RaiseLivesChanged(CurrentLives);
        }

        private void OnEnable()
        {
            GameEvents.OnEnemyDied += HandleEnemyDied;
            GameEvents.OnEnemyReachedEnd += HandleEnemyReachedEnd;
            GameEvents.OnEarlyStartBonusAwarded += HandleEarlyStartBonusAwarded;
        }

        private void OnDisable()
        {
            GameEvents.OnEnemyDied -= HandleEnemyDied;
            GameEvents.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
            GameEvents.OnEarlyStartBonusAwarded -= HandleEarlyStartBonusAwarded;
        }

        public void AddGold(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            CurrentGold += amount;
            GameEvents.RaiseGoldChanged(CurrentGold);
        }

        public bool TrySpendGold(int amount)
        {
            if (amount <= 0)
            {
                return false;
            }

            if (CurrentGold >= amount)
            {
                CurrentGold -= amount;
                GameEvents.RaiseGoldChanged(CurrentGold);
                return true;
            }

            return false;
        }

        public bool CanAfford(int amount)
        {
            return CurrentGold >= amount;
        }

        public void LoseLife(int amount)
        {
            if (amount <= 0) return;

            CurrentLives -= amount;
            if (CurrentLives < 0) CurrentLives = 0;

            GameEvents.RaiseLivesChanged(CurrentLives);
        }

        private void HandleEnemyDied(GameObject enemy)
        {
            var enemyBase = enemy.GetComponent<TowerDefense.Enemies.EnemyBase>();
            if (enemyBase != null && enemyBase.Data != null)
            {
                AddGold(enemyBase.Data.goldReward);
            }
        }

        private void HandleEnemyReachedEnd(GameObject enemy)
        {
            var enemyBase = enemy.GetComponent<TowerDefense.Enemies.EnemyBase>();
            int cost = 1;

            if (enemyBase != null && enemyBase.Data != null)
            {
                cost = enemyBase.Data.livesCost;
            }

            LoseLife(cost);
        }

        private void HandleEarlyStartBonusAwarded(int waveIndex, int bonusAmount)
        {
            AddGold(bonusAmount);
        }
    }
}
