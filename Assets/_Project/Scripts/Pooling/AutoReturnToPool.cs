using UnityEngine;

namespace TowerDefense.Pooling
{
    /// <summary>
    /// Gắn script này lên các Prefab hiệu ứng VFX (Ví dụ: Hiệu ứng đạn nổ, quái chết).
    /// Khi được đẻ ra (OnEnable), nó đếm ngược, hết thời gian tự động thu hồi về Pool.
    /// </summary>
    public class AutoReturnToPool : MonoBehaviour
    {
        [Tooltip("Số giây tồn tại trước khi tự động biến mất")]
        [SerializeField] private float lifetime = 1f;

        private float timer;

        private void OnEnable()
        {
            timer = lifetime;
        }

        private void Update()
        {
            if (timer > 0f)
            {
                timer -= Time.deltaTime;
                if (timer <= 0f)
                {
                    SimplePool.Instance.Return(gameObject);
                }
            }
        }
    }
}
