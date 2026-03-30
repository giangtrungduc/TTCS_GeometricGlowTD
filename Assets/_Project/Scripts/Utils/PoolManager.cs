using System.Collections.Generic;
using TowerDefense.Core;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.Utils
{
    public class PoolManager : ManagerBase<PoolManager>
    {
        [Header("Pool Settings")]

        [Tooltip("Số enemy tạo sẵn mỗi loại khi bắt đầu")]
        [SerializeField] private int enemyPreWarmCount = 10;

        [Tooltip("Số projectile tạo sẵn mỗi loại khi bắt đầu")]
        [SerializeField] private int projectilePreWarmCount = 15;

        [Tooltip("Số particle tạo sẵn mỗi loại khi bắt đầu")]
        [SerializeField] private int particlePreWarmCount = 10;

        private Transform enemyContainer;
        private Transform projectileContainer;
        private Transform particleContainer;

        private Dictionary<int, ObjectPool<Component>> enemyPools;
        private Dictionary<int, ObjectPool<Component>> projectilePools;
        private Dictionary<int, ObjectPool<Component>> particlePools;

        protected override void OnAwake()
        {
            // Tạo containers trong Hierarchy
            enemyContainer = CreateContainer("--- ENEMY POOL ---");
            projectileContainer = CreateContainer("--- PROJECTILE POOL ---");
            particleContainer = CreateContainer("--- PARTICLE POOL ---");

            // Khởi tạo dictionaries
            enemyPools = new Dictionary<int, ObjectPool<Component>>();
            projectilePools = new Dictionary<int, ObjectPool<Component>>();
            particlePools = new Dictionary<int, ObjectPool<Component>>();
        }

        // ============================
        // ENEMY POOL
        // ============================

        // Lấy 1 enemy từ pool.
        public Component GetEnemy(Component prefab, Vector3 pos)
        {
            ObjectPool<Component> pool = GetOrCreatePool(prefab, enemyPools, enemyContainer, enemyPreWarmCount);

            Component enemy = pool.Get(pos);
            return enemy;
        }

        // Lấy enemy và cast về type cụ thể.
        public T GetEnemy<T>(T prefab, Vector3 pos) where T : Component
        {
            Component enemy = GetEnemy(prefab as Component, pos);
            return enemy as T;
        }

        // Trả enemy về pool.
        public void ReturnEnemy(Component enemy)
        {
            if (enemy == null) return;
            ReturnToPool(enemy, enemyPools);
        }

        // ============================
        // PROJECTILE POOL
        // ============================

        // Lấy 1 projectile từ pool.
        public Component GetProjectile(Component prefab, Vector3 position)
        {
            ObjectPool<Component> pool = GetOrCreatePool(prefab, projectilePools, projectileContainer, projectilePreWarmCount);
            Component projectile = pool.Get(position);
            return projectile;
        }

        // Lấy projectile và cast về type cụ thể.
        public T GetProjectile<T>(T prefab, Vector3 position) where T : Component
        {
            Component proj = GetProjectile(prefab as Component, position);
            return proj as T;
        }

        // Trả projectile về pool.
        public void ReturnProjectile(Component projectile)
        {
            if (projectile == null) return;
            ReturnToPool(projectile, projectilePools);
        }

        // ============================
        // PARTICLE POOL
        // ============================

        // Lấy 1 particle effect từ pool.
        public Component GetParticle(Component prefab, Vector3 position)
        {
            ObjectPool<Component> pool = GetOrCreatePool(prefab, particlePools, particleContainer, particlePreWarmCount);

            Component particle = pool.Get(position);
            return particle;
        }

        // Trả particle về pool.
        public void ReturnParticle(Component particle)
        {
            if (particle == null) return;
            ReturnToPool(particle, particlePools);
        }


        // ============================
        // CLEANUP
        // ============================

        // Trả TẤT CẢ objects về pool. Gọi khi restart level.
        public void ReturnAllToPool()
        {
            ReturnAllInDict(enemyPools, "Enemy");
            ReturnAllInDict(projectilePools, "Projectile");
            ReturnAllInDict(particlePools, "Particle");
        }
        // Huỷ tất cả pool và objects. Gọi khi đổi scene.
        public void ClearAllPools()
        {
            ClearDict(enemyPools, "Enemy");
            ClearDict(projectilePools, "Projectile");
            ClearDict(particlePools, "Particle");
        }
        // In trạng thái tất cả pools.
        public void LogPoolStatus()
        {
            Debug.Log("=== POOL STATUS ===");
            LogDict(enemyPools, "Enemy");
            LogDict(projectilePools, "Projectile");
            LogDict(particlePools, "Particle");
            Debug.Log("===================");
        }

        // ============================
        // PRIVATE HELPERS
        // ============================

        // Lấy pool cho prefab, hoặc tạo mới nếu chưa có.
        private ObjectPool<Component> GetOrCreatePool(Component prefab, Dictionary<int, ObjectPool<Component>> dict, Transform container, int preWarmCount)
        {
            if(prefab == null)
            {
                Debug.LogError("[PoolManager] Prefab is null!");
                return null;
            }
            int prefabId = prefab.gameObject.GetInstanceID();
            if (!dict.ContainsKey(prefabId))
            {
                Transform subContainer = CreateContainer($"Pool_{prefab.name}", container);

                ObjectPool<Component> newPool = new ObjectPool<Component>(prefab, subContainer, preWarmCount);

                dict.Add(prefabId, newPool);
            }
            return dict[prefabId];
        }

        // Trả object về đúng pool dựa trên tên prefab gốc.
        private void ReturnToPool(Component obj, Dictionary<int, ObjectPool<Component>> dict)
        {
            // Tìm pool phù hợp
            foreach(var pool in dict.Values)
            {
                pool.Return(obj);
                return;
            }
            obj.gameObject.SetActive(false);
        }
        private Transform CreateContainer(string name, Transform containerParent = null)
        {
            GameObject container = new GameObject(name);

            if (containerParent != null)
            {
                container.transform.SetParent(containerParent);
            }
            else
            {
                container.transform.SetParent(transform);
            }

            return container.transform;
        }
        private void ReturnAllInDict(Dictionary<int, ObjectPool<Component>> dict, string category)
        {
            foreach (var pool in dict.Values)
            {
                pool.ReturnAll();
            }
            Debug.Log($"[PoolManager] ReturnAll: {category} pools");
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
                Debug.Log($"    {pool}");
            }
        }

        protected override void OnDestroy()
        {
            ClearAllPools();
            base.OnDestroy();
        }
    }
}
