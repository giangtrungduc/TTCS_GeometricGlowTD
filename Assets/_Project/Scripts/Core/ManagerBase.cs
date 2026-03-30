using UnityEngine;

namespace TowerDefense.Core
{
    public abstract class ManagerBase<T> : MonoBehaviour where T : MonoBehaviour
    {
        // Instance duy nhất của Manager này.
        public static T Instance { get; private set; }

        // Nếu true: Manager tồn tại xuyên suốt các scene.
        // Nếu false: Manager bị huỷ khi đổi scene (mặc định).
        protected virtual bool Persistent => false;

        protected virtual void Awake()
        {
            if(Instance != null && Instance != this as T)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this as T;

            if (Persistent)
            {
                DontDestroyOnLoad(gameObject);
            }

            OnAwake();

        }
        protected virtual void OnDestroy()
        {
            if(Instance == this as T)
            {
                Instance = null;
            }
        }
        protected virtual void OnAwake() { }
    }
}
