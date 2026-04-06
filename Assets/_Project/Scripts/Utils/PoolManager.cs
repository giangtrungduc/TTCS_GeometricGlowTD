using System.Collections.Generic;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Utils
{
    public class PoolManager : ManagerBase<PoolManager>
    {
        // ===========================
        // CẤU HÌNH
        // ===========================

        [Header("Pool Pre-warm Settings")]

        [Tooltip("Số enemy tạo sẵn mỗi loại khi bắt đầu")]
        [SerializeField] private int enemyPreWarmCount = 10;

        [Tooltip("Số projectile tạo sẵn mỗi loại khi bắt đầu")]
        [SerializeField] private int projectilePreWarmCount = 15;

        [Tooltip("Số particle tạo sẵn mỗi loại khi bắt đầu")]
        [SerializeField] private int particlePreWarmCount = 10;

        // ===========================
        // STATE
        // ===========================

        private Transform enemyContainer;
        private Transform projectileContainer;
        private Transform particleContainer;

        private Dictionary<int, ObjectPool<Component>> enemyPools;
        private Dictionary<int, ObjectPool<Component>> projectilePools;
        private Dictionary<int, ObjectPool<Component>> particlePools;

        // ===========================
        // INIT
        // ===========================

        protected override void OnAwake()
        {
            enemyContainer = CreateContainer("--- ENEMY POOL ---");
            projectileContainer = CreateContainer("--- PROJECTILE POOL ---");
            particleContainer = CreateContainer("--- PARTICLE POOL ---");

            enemyPools = new Dictionary<int, ObjectPool<Component>>();
            projectilePools = new Dictionary<int, ObjectPool<Component>>();
            particlePools = new Dictionary<int, ObjectPool<Component>>();
        }

        // ===========================
        // ENEMY POOL
        // ===========================

        public Component GetEnemy(Component prefab, Vector3 pos)
        {
            var pool = GetOrCreatePool(prefab, enemyPools, enemyContainer, enemyPreWarmCount);
            if (pool == null) return null;
            return pool.Get(pos);
        }

        public T GetEnemy<T>(T prefab, Vector3 pos) where T : Component => GetEnemy(prefab as Component, pos) as T;

        public void ReturnEnemy(Component enemy)
        {
            if (enemy == null) return;
            ReturnToCorrectPool(enemy, enemyPools, "Enemy");
        }

        // ===========================
        // PROJECTILE POOL
        // ===========================

        public Component GetProjectile(Component prefab, Vector3 pos)
        {
            var pool = GetOrCreatePool(prefab, projectilePools, projectileContainer, projectilePreWarmCount);
            if (pool == null) return null;
            return pool.Get(pos);
        }

        public T GetProjectile<T>(T prefab, Vector3 pos) where T : Component => GetProjectile(prefab as Component, pos) as T;

        public void ReturnProjectile(Component projectile)
        {
            if (projectile == null) return;
            ReturnToCorrectPool(projectile, projectilePools, "Projectile");
        }

        // ===========================
        // PARTICLE POOL
        // ===========================

        public Component GetParticle(Component prefab, Vector3 pos)
        {
            var pool = GetOrCreatePool(prefab, particlePools, particleContainer, particlePreWarmCount);
            if (pool == null) return null;
            return pool.Get(pos);
        }

        public void ReturnParticle(Component particle)
        {
            if (particle == null) return;
            ReturnToCorrectPool(particle, particlePools, "Particle");
        }

        // ===========================
        // CLEANUP
        // ===========================

        /// <summary>Trả tất cả active objects về pool. Dùng khi restart level.</summary>
        public void ReturnAllToPool()
        {
            ReturnAllInDict(enemyPools);
            ReturnAllInDict(projectilePools);
            ReturnAllInDict(particlePools);
        }

        /// <summary>Destroy tất cả objects và clear pool. Dùng khi đổi scene.</summary>
        public void ClearAllPools()
        {
            ClearDict(enemyPools, "Enemy");
            ClearDict(projectilePools, "Projectile");
            ClearDict(particlePools, "Particle");
        }

        public void LogPoolStatus()
        {
            Debug.Log("=== POOL STATUS ===");
            LogDict(enemyPools, "Enemy");
            LogDict(projectilePools, "Projectile");
            LogDict(particlePools, "Particle");
            Debug.Log("===================");
        }

        protected override void OnDestroy()
        {
            ClearAllPools();
            base.OnDestroy();
        }

        // ===========================
        // PRIVATE HELPERS
        // ===========================

        private ObjectPool<Component> GetOrCreatePool(Component prefab, Dictionary<int, ObjectPool<Component>> dict, Transform container, int preWarmCount)
        {
            if (prefab == null)
            {
                Debug.LogError("[PoolManager] Prefab is null!");
                return null;
            }

            int prefabId = prefab.gameObject.GetInstanceID();

            if (!dict.TryGetValue(prefabId, out var pool))
            {
                Transform sub = CreateContainer($"Pool_{prefab.name}", container);
                pool = new ObjectPool<Component>(prefab, sub, preWarmCount);
                dict.Add(prefabId, pool);
            }

            return pool;
        }

        /// <summary>
        /// Tìm đúng pool theo PrefabId được set bởi ObjectPool.CreateNewObject().
        /// </summary>
        private void ReturnToCorrectPool(Component obj, Dictionary<int, ObjectPool<Component>> dict, string category)
        {
            if (!obj.TryGetComponent(out PooledObject pooledObj))
            {
                Debug.LogWarning($"[PoolManager] {obj.name} không có PooledObject component. Fallback SetActive(false).");
                obj.gameObject.SetActive(false);
                return;
            }

            if (dict.TryGetValue(pooledObj.PrefabId, out var pool))
            {
                pool.Return(obj);
            }
            else
            {
                Debug.LogWarning($"[PoolManager] Không tìm thấy pool [{category}] cho PrefabId={pooledObj.PrefabId} ({obj.name}). Fallback SetActive(false).");
                obj.gameObject.SetActive(false);
            }
        }

        private Transform CreateContainer(string name, Transform parent = null)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent != null ? parent : transform);
            return go.transform;
        }

        private void ReturnAllInDict(Dictionary<int, ObjectPool<Component>> dict)
        {
            foreach (var pool in dict.Values)
            {
                pool.ReturnAll();
            }
        }

        private void ClearDict(Dictionary<int, ObjectPool<Component>> dict, string category)
        {
            foreach (var pool in dict.Values)
            {
                pool.Clear();
            }
            dict.Clear();
            Debug.Log($"[PoolManager] Cleared: {category} pools");
        }

        private void LogDict(Dictionary<int, ObjectPool<Component>> dict, string category)
        {
            foreach (var pool in dict.Values)
            {
                Debug.Log($"  [{category}] {pool}");
            }
        }
    }
}