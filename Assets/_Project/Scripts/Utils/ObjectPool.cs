using System;
using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Utils
{
    public class ObjectPool<T> where T : Component
    {
        // ===========================
        // STATE
        // ===========================

        private readonly T prefab;
        private readonly Transform parent;
        private readonly Stack<T> available;
        private readonly List<T> allObjects;

        // ===========================
        // PROPERTIES
        // ===========================

        public int AvailableCount => available.Count;
        public int TotalCreated => allObjects.Count;
        public int ActiveCount => TotalCreated - AvailableCount;

        // ===========================
        // CONSTRUCTOR
        // ===========================

        public ObjectPool(T prefab, Transform parent = null, int initialSize = 0)
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPool] Prefab is null!");
                return;
            }

            this.prefab = prefab;
            this.available = new Stack<T>();
            this.allObjects = new List<T>();

            if (parent == null)
            {
                string poolName = $"Pool_{prefab.name}";
                this.parent = new GameObject(poolName).transform;
            }
            else
            {
                this.parent = parent;
            }

            if (initialSize > 0)
            {
                PreWarm(initialSize);
            }
        }

        // ===========================
        // GET
        // ===========================

        /// <summary>
        /// Lấy object từ pool, không set vị trí.
        /// Dùng khi cần tự set transform sau.
        /// </summary>
        public T Get()
        {
            T obj = GetRaw();
            if (obj == null) return null;

            InjectCallback(obj);
            obj.gameObject.SetActive(true);
            NotifyGetFromPool(obj);

            return obj;
        }

        /// <summary>
        /// Lấy object từ pool và đặt tại vị trí chỉ định.
        /// Position được set TRƯỚC SetActive — collider bật đúng vị trí ngay từ đầu.
        /// </summary>
        public T Get(Vector3 pos)
        {
            T obj = GetRaw();
            if (obj == null) return null;

            obj.transform.position = pos;

            InjectCallback(obj);
            obj.gameObject.SetActive(true);
            NotifyGetFromPool(obj);

            return obj;
        }

        /// <summary>
        /// Lấy object từ pool với vị trí và rotation chỉ định.
        /// Position và rotation được set TRƯỚC SetActive.
        /// </summary>
        public T Get(Vector3 pos, Quaternion rot)
        {
            T obj = GetRaw();
            if (obj == null) return null;

            obj.transform.SetPositionAndRotation(pos, rot);

            InjectCallback(obj);
            obj.gameObject.SetActive(true);
            NotifyGetFromPool(obj);

            return obj;
        }

        // ===========================
        // RETURN
        // ===========================

        /// <summary>
        /// Trả object về pool.
        /// </summary>
        public void Return(T obj)
        {
            if (obj == null) return;

            // Thông báo trước khi disable
            if (obj is IPoolable poolable)
                poolable.OnReturnToPool();

            obj.gameObject.SetActive(false);
            obj.transform.SetParent(parent);

            available.Push(obj);
        }

        public void ReturnAll()
        {
            foreach (T obj in allObjects)
            {
                if (obj != null && obj.gameObject.activeInHierarchy)
                {
                    Return(obj);
                }
            }
        }

        // ===========================
        // UTILITIES
        // ===========================

        public void PreWarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                T obj = CreateNewObject();
                if (obj != null)
                {
                    obj.gameObject.SetActive(false);
                    available.Push(obj);
                }
            }
        }

        public void Clear()
        {
            foreach (T obj in allObjects)
            {
                if (obj != null && obj.gameObject != null)
                {
                    UnityEngine.Object.Destroy(obj.gameObject);
                }
            }

            available.Clear();
            allObjects.Clear();
        }

        public override string ToString()
        {
            string name = prefab != null ? prefab.name : "null";
            return $"[Pool '{name}'] Available: {AvailableCount} | Active: {ActiveCount} | Total: {TotalCreated}";
        }

        // ===========================
        // PRIVATE HELPERS
        // ===========================

        /// <summary>
        /// Lấy object từ stack mà KHÔNG activate.
        /// Cho phép caller set transform trước khi bật.
        /// </summary>
        private T GetRaw()
        {
            if (available.Count > 0)
            {
                T obj = available.Pop();

                if (obj == null || obj.gameObject == null)
                {
                    return CreateNewObject();
                }

                return obj;
            }

            return CreateNewObject();
        }

        /// <summary>
        /// Inject ReturnCallback vào object nếu implement IPoolable.
        /// Object dùng callback này để tự trả về đúng pool khi cần.
        /// </summary>
        private void InjectCallback(T obj)
        {
            if (obj is IPoolable poolable)
            {
                poolable.SetReturnCallback(() => Return(obj));
            }
        }

        private void NotifyGetFromPool(T obj)
        {
            if (obj is IPoolable poolable)
            {
                poolable.OnGetFromPool();
            }
        }

        /// <summary>
        /// Tạo object mới và set PrefabId cho PooledObject (nếu có)
        /// để PoolManager định tuyến về đúng pool khi Return.
        /// </summary>
        private T CreateNewObject()
        {
            if (prefab == null)
            {
                Debug.LogError("[ObjectPool] Cannot create: prefab is null!");
                return null;
            }

            T newObj = UnityEngine.Object.Instantiate(prefab, parent);
            newObj.gameObject.name = $"{prefab.name}_Pool_{allObjects.Count}";
            allObjects.Add(newObj);

            if (newObj.TryGetComponent(out PooledObject pooledObj))
            {
                pooledObj.PrefabId = prefab.gameObject.GetInstanceID();
            }

            return newObj;
        }
    }
}