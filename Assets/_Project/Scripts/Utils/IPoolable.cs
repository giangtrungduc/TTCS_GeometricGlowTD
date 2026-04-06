using System;

namespace TowerDefense.Utils
{
    public interface IPoolable
    {
        /// <summary>
        /// ObjectPool gọi để inject callback trả về đúng pool.
        /// Object lưu callback này và dùng trong ReturnToPool().
        /// </summary>
        void SetReturnCallback(Action returnCallback);

        /// <summary>
        /// Gọi sau khi SetActive(true) và position đã được set đúng.
        /// Dùng để reset state visual, animation, v.v.
        /// </summary>
        void OnGetFromPool();

        /// <summary>
        /// Gọi TRƯỚC khi SetActive(false).
        /// Dùng để dọn dẹp state, unsubscribe event, v.v.
        /// </summary>
        void OnReturnToPool();
    }
}