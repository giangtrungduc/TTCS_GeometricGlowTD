using System.Collections.Generic;
using UnityEngine;

namespace TowerDefense.Pooling
{
    /// <summary>
    /// Hệ thống Object Pool siêu tối giản, dùng chung toàn dự án.
    /// Không cần Interface phức tạp, tự động gán nhãn để biết trả về hàng đợi nào.
    /// </summary>
    public class SimplePool : MonoBehaviour
    {
        public static SimplePool Instance { get; private set; }

        private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new Dictionary<GameObject, Queue<GameObject>>();

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        public GameObject Get(GameObject prefab, Vector3 position, Quaternion rotation)
        {
            if (prefab == null) return null;

            if (!poolDictionary.ContainsKey(prefab))
            {
                poolDictionary[prefab] = new Queue<GameObject>();
            }

            GameObject objToSpawn = null;

            if (poolDictionary[prefab].Count > 0)
            {
                objToSpawn = poolDictionary[prefab].Dequeue();
            }
            else
            {
                objToSpawn = Instantiate(prefab);
                // Dán nhãn để biết đường thu hồi
                PoolIdentity identity = objToSpawn.AddComponent<PoolIdentity>();
                identity.originalPrefab = prefab;
                objToSpawn.transform.SetParent(transform); // Gom hết vào chung 1 cục cho gọn Hierarchy
            }

            objToSpawn.transform.position = position;
            objToSpawn.transform.rotation = rotation;
            objToSpawn.SetActive(true);

            return objToSpawn;
        }

        public void Return(GameObject obj)
        {
            if (obj == null) return;

            obj.SetActive(false);

            PoolIdentity identity = obj.GetComponent<PoolIdentity>();
            if (identity != null && identity.originalPrefab != null)
            {
                if (!poolDictionary.ContainsKey(identity.originalPrefab))
                {
                    poolDictionary[identity.originalPrefab] = new Queue<GameObject>();
                }
                poolDictionary[identity.originalPrefab].Enqueue(obj);
            }
            else
            {
                // Nếu gọi Return vào một Object không sinh ra từ Pool này thì đành Destroy
                Destroy(obj); 
            }
        }
    }

    // Component nhỏ dùng làm "Chứng minh nhân dân" cho Object biết nó đẻ ra từ Prefab nào
    public class PoolIdentity : MonoBehaviour
    {
        public GameObject originalPrefab;
    }
}
