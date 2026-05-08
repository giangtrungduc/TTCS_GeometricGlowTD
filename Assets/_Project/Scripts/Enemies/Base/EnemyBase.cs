using System;
using UnityEngine;
using TowerDefense.Core;
using TowerDefense.StatusEffects;
using TowerDefense.Utils;

namespace TowerDefense.Enemies
{
    public interface IEnemyDeathListener
    {
        void OnEnemyDeath(EnemyBase enemy);
    }

    [RequireComponent(typeof(PathFollower))]
    [RequireComponent(typeof(StatusEffectHandler))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class EnemyBase : MonoBehaviour, IDamageable, IPoolable
    {
        [Header("Enemy Data")]
        [Tooltip("Dữ liệu cấu hình của enemy: máu, tốc độ, phần thưởng, sprite, màu chủ đạo.")]
        [SerializeField] private EnemyData enemyData;

        protected PathFollower pathFollower;
        protected StatusEffectHandler statusHandler;
        protected SpriteRenderer spriteRenderer;

        private float currentHp;
        private bool isDead;
        private bool isInitialized;
        private Action _returnCallback;

        public float CurrentHp => currentHp;
        public float MaxHp => enemyData != null ? enemyData.maxHp : 0f;
        public float HpPercent => MaxHp > 0f ? currentHp / MaxHp : 0f;
        public bool IsDead => isDead;

        public EnemyData Data => enemyData;
        public PathFollower PathFollower => pathFollower;
        public StatusEffectHandler StatusHandler => statusHandler;
        public bool IsInitialized => isInitialized;

        public void SetReturnCallback(Action returnCallback)
        {
            _returnCallback = returnCallback;
        }

        /// <summary>Gọi bởi ObjectPool sau khi SetActive(true) và position đã đúng.</summary>
        public virtual void OnGetFromPool() { }

        /// <summary>Gọi bởi ObjectPool trước SetActive(false). Dọn dẹp trail, particle, v.v.</summary>
        public virtual void OnReturnToPool()
        {
            isInitialized = false;
        }

        protected virtual void Awake()
        {
            pathFollower = GetComponent<PathFollower>();
            statusHandler = GetComponent<StatusEffectHandler>();
            spriteRenderer = GetComponent<SpriteRenderer>();

#if UNITY_EDITOR
            // Validate data asset ngay từ đầu để phát hiện lỗi config sớm
            if (enemyData == null)
            {
                Debug.LogError($"[{GetType().Name}] '{name}' thiếu EnemyData!", this);
            }
            else if (!enemyData.Validate(out string err))
            {
                Debug.LogError($"[{GetType().Name}] EnemyData không hợp lệ: {err}", this);
            }
#endif

            OnEnemyAwake();
        }

        protected virtual void OnEnable()
        {
            if (enemyData == null) return;

            // Reset trạng thái mỗi lần lấy từ pool
            currentHp = enemyData.maxHp;
            isDead = false;
            isInitialized = false;

            // Reset màu sprite
            if (spriteRenderer != null)
            {
                if (enemyData.enemySprite != null)
                {
                    spriteRenderer.sprite = enemyData.enemySprite;
                }

                spriteRenderer.color = enemyData.themeColor;
                statusHandler?.SetOriginalColor(enemyData.themeColor);
            }
        }

        /// <summary>
        /// Khởi tạo PathFollower với path và speed.
        /// Gọi bởi WaveManager sau khi lấy từ pool.
        /// </summary>
        public void Initialize(WaypointPath path, float speedMultiplier = 1f)
        {
            if (enemyData == null)
            {
                Debug.LogError($"[{GetType().Name}] Initialize thất bại: EnemyData null ({name})");
                return;
            }
            if (path == null)
            {
                Debug.LogError($"[{GetType().Name}] Initialize thất bại: path null ({name})");
                return;
            }

            pathFollower?.Initialize(path, enemyData.moveSpeed * speedMultiplier);
            isInitialized = true;

            OnSpawned();
        }

        public void Initialize(WaypointPath path, Vector2 startPosition, int waypointIndex, float speedMultiplier = 1f)
        {
            if (enemyData == null)
            {
                Debug.LogError($"[{GetType().Name}] Initialize thất bại: EnemyData null ({name})");
                return;
            }
            if (path == null)
            {
                Debug.LogError($"[{GetType().Name}] Initialize thất bại: path null ({name})");
                return;
            }

            pathFollower?.Initialize(path, enemyData.moveSpeed * speedMultiplier, startPosition, waypointIndex);
            isInitialized = true;

            OnSpawned();
        }

        public void TakeDamage(float damage)
        {
            if (isDead || damage <= 0f) return;

            float previousHp = currentHp;
            currentHp = Mathf.Max(0f, currentHp - damage);
            float appliedDamage = previousHp - currentHp;

            OnDamaged(damage);
            OnDamaged(damage, appliedDamage, previousHp, currentHp);

            if (currentHp <= 0f)
                Die();
        }

        public void Heal(float amount)
        {
            if (isDead || amount <= 0f) return;

            float before = currentHp;
            currentHp = Mathf.Min(MaxHp, currentHp + amount);

            OnHealed(currentHp - before);
        }

        /// <summary>Gọi trong Awake. Cache component, khởi tạo ability riêng.</summary>
        protected virtual void OnEnemyAwake() { }

        /// <summary>Gọi sau Initialize() — enemy đã có path, chuẩn bị di chuyển.</summary>
        protected virtual void OnSpawned() { }

        /// <summary>Gọi mỗi lần nhận damage (trước khi Die nếu HP = 0).</summary>
        protected virtual void OnDamaged(float damageAmount) { }

        /// <summary>
        /// Hook mở rộng cho các subclass cần biết damage thực áp dụng và snapshot HP.
        /// Mặc định giữ tương thích ngược bằng cách không làm gì thêm.
        /// </summary>
        protected virtual void OnDamaged(float incomingDamage, float appliedDamage, float previousHp, float currentHp) { }

        /// <summary>Gọi mỗi lần được heal.</summary>
        protected virtual void OnHealed(float healAmount) { }

        /// <summary>Gọi khi HP = 0, trước khi trả về pool.</summary>
        protected virtual void OnDeath() { }

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            // Subclass xử lý animation/loot/particle trước khi về pool
            OnDeath();

            var deathListeners = GetComponents<IEnemyDeathListener>();
            for (int i = 0; i < deathListeners.Length; i++)
            {
                deathListeners[i].OnEnemyDeath(this);
            }

            GameEvents.RaiseEnemyDied(gameObject);

            ReturnToPool();
        }

        private void ReturnToPool()
        {
            // Lấy callback ra và clear trước khi invoke — chống re-entrance
            Action cb = _returnCallback;
            _returnCallback = null;

            if (cb != null)
                cb.Invoke();
            else
                gameObject.SetActive(false);
        }

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (enemyData == null) return;

            string label = $"{enemyData.enemyName}\nHP: {currentHp:F0}/{MaxHp:F0} ({HpPercent:P0})";

            if (statusHandler != null)
            {
                if (statusHandler.IsSlowed) label += "\n🐢 SLOWED";
                if (statusHandler.IsSpeedBuffed) label += "\n⚡ BUFFED";
            }

            if (!isInitialized) label += "\n[Not Initialized]";

            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, label);
        }
#endif
    }
}
