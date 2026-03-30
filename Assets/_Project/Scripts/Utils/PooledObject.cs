using UnityEngine;
using TowerDefense.Core;

namespace TowerDefense.Utils
{
    public class PooledObject : MonoBehaviour
    {
        // ============================
        // CẤU HÌNH
        // ============================

        [Header("Auto Return Settings")]

        [Tooltip("Tự trả về pool sau X giây. 0 = không tự trả.")]
        [SerializeField] private float autoReturnDelay = 0f;

        // ============================
        // STATE
        // ============================

        /// <summary>
        /// Prefab gốc Instance ID — dùng để PoolManager tìm đúng pool.
        /// Được set bởi ObjectPool khi Instantiate.
        /// </summary>
        public int PrefabId { get; set; }
        public PoolType PoolCategory { get; set; } = PoolType.None;

        // ============================
        // UNITY LIFECYCLE
        // ============================

        private void OnEnable()
        {
            // Auto return sau delay (dùng cho particle effects)
            if (autoReturnDelay > 0f)
            {
                Invoke(nameof(ReturnToPool), autoReturnDelay);
            }
        }

        private void OnDisable()
        {
            // Cancel auto return nếu bị return trước thời hạn
            CancelInvoke(nameof(ReturnToPool));
        }

        // ============================
        // PUBLIC METHODS
        // ============================

        // Trả object này về pool.
        public void ReturnToPool()
        {
            if (PoolManager.Instance == null)
            {
                // Fallback: nếu không có PoolManager, disable thủ công
                gameObject.SetActive(false);
                return;
            }

            switch (PoolCategory)
            {
                case PoolType.Enemy:
                    PoolManager.Instance.ReturnEnemy(this);
                    break;

                case PoolType.Projectile:
                    PoolManager.Instance.ReturnProjectile(this);
                    break;

                case PoolType.Particle:
                    PoolManager.Instance.ReturnParticle(this);
                    break;

                default:
                    gameObject.SetActive(false);
                    break;
            }
        }
    }

    // Phân loại pool để trả về đúng dictionary.
    public enum PoolType
    {
        None,
        Enemy,
        Projectile,
        Particle
    }
}