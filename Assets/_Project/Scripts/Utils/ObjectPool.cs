using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace TowerDefense.Utils
{
    public class ObjectPool<T> where T : Component
    {
        private readonly T prefab;

        private readonly Transform parent;

        private readonly Stack<T> available;

        private readonly List<T> allObjects;
        public int AvailableCount => available.Count;// Số object sẵn sàng trong pool
        public int TotalCreated => allObjects.Count;// Tổng số object đã tạo
        public int ActiveCount => TotalCreated - AvailableCount;// Số object đang sử dụng

        public ObjectPool(T prefab, Transform parent = null, int initialSize = 0)
        {
            if(prefab == null)
            {
                Debug.LogError("[ObjectPool] Prefab is null!");
            }

            this.prefab = prefab;
            this.available = new Stack<T>();
            this.allObjects = new List<T>();

            if(parent == null)
            {
                string poolName = prefab != null ? $"Pool_{prefab.name}" : "Pool_Unknown";
                GameObject container = new GameObject(poolName);
                this.parent = container.transform;
            }
            else
            {
                this.parent = parent;
            }

            if(initialSize > 0)
            {
                PreWarm(initialSize);
            }
        }

        public T Get()
        {
            T obj;
            if(available.Count > 0)
            {
                obj = available.Pop();
                if(obj == null || obj.gameObject == null)
                {
                    return CreateNewObject();
                }
            }
            else
            {
                obj = CreateNewObject();

                if (obj == null) return null;
            }

            obj.gameObject.SetActive(true);
            return obj;
        }

        /// <summary>
        /// Lấy 1 object từ pool và đặt tại vị trí cụ thể.
        /// </summary>
        public T Get(Vector3 pos)
        {
            T obj = Get();
            if (obj != null)
            {
                obj.transform.position = pos;
            }
            return obj;
        }

        /// <summary>
        /// Lấy 1 object từ pool với vị trí và rotation.
        /// </summary>
        public T Get(Vector3 pos, Quaternion rot)
        {
            T obj = Get();
            if(obj != null)
            {
                obj.transform.position = pos;
                obj.transform.rotation = rot;
            }
            return obj;
        }
        
        public void Return(T obj)
        {
            if (obj == null)
            {
                return;
            }
            obj.gameObject.SetActive(false);

            obj.transform.SetParent(parent);

            available.Push(obj);
        }

        public void ReturnAll()
        {
            int returnedCount = 0;
            foreach(T obj in allObjects)
            {
                if(obj != null && obj.gameObject.activeInHierarchy)
                {
                    Return(obj);
                    returnedCount++;
                }
            }
        }

        public void PreWarm(int count)
        {
            for(int i = 0; i < count; i++)
            {
                T obj = CreateNewObject();
                if(obj != null)
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
                    Object.Destroy(obj.gameObject);
                }
            }

            available.Clear();
            allObjects.Clear();

            Debug.Log("[ObjectPool] Pool cleared and all objects destroyed.");
        }

        private T CreateNewObject()
        {
            if(prefab == null)
            {
                Debug.LogError("[ObjectPool] Cannot create: prefab is null!");
                return null;
            }

            T newObj = Object.Instantiate(prefab, parent);

            newObj.gameObject.name = $"{prefab.name}_Pool_{allObjects.Count}";

            allObjects.Add(newObj);

            return newObj;
        }

        public override string ToString()
        {
            string prefabName = prefab != null ? prefab.name : "null";
            return $"[Pool '{prefabName}'] " +
                   $"Available: {AvailableCount} | " +
                   $"Active: {ActiveCount} | " +
                   $"Total: {TotalCreated}";
        }
    }
}
