using UnityEngine;

namespace TowerDefense.Core
{
    public class EconomyManager : ManagerBase<EconomyManager>
    {
        // Gold hiện tại
        public int CurrentGold { get; private set; }
        // Mạng hiện tại
        public int CurrentLives {  get; private set; }

        private bool isInitialized = false;

        // ============================
        // INIT
        // ============================
        public void Initialize(int startGold, int startLives)
        {
            CurrentGold = startGold;
            CurrentLives = startLives;
            isInitialized = true;

            GameEvents.RaiseGoldChanged(CurrentGold);
            GameEvents.RaiseLivesChanged(CurrentLives);
        }

        // ============================
        // EVENT SUBSCRIBE
        // ============================
        private void OnEnable()
        {
            GameEvents.OnEnemyDied += HandleEnemyDied;
            GameEvents.OnEnemyReachedEnd += HandleEnemyReachedEnd;
        }
        private void OnDisable()
        {
            GameEvents.OnEnemyDied -= HandleEnemyDied;
            GameEvents.OnEnemyReachedEnd -= HandleEnemyReachedEnd;
        }

        // ============================
        // GOLD METHODS
        // ============================
        public void AddGold(int amount)
        {
            if(amount <= 0)
            {
                return;
            }
            CurrentGold += amount;
            GameEvents.RaiseGoldChanged(CurrentGold);
        }
        public bool TrySpendGold(int amount)
        {
            if(amount <= 0)
            {
                return false;
            }

            if(CurrentGold >= amount)
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

        // ============================
        // LIVES METHODS
        // ============================
        public void LoseLife(int amount)
        {
            if (amount <= 0) return;

            CurrentLives -= amount;

            if (CurrentLives < 0) CurrentLives = 0;

            GameEvents.RaiseLivesChanged(CurrentLives);
        }

        // ============================
        // EVENT HANDLERS
        // ============================
        private void HandleEnemyDied(GameObject enemy)
        {
            // TODO 
            int reward = 10;
            AddGold(reward);
        }
        private void HandleEnemyReachedEnd(GameObject enemy)
        {
            // TODO
            int cost = 1;
            LoseLife(cost);
        }
    }
}
