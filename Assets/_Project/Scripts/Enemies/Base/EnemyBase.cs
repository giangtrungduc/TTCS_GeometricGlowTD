using System;
using UnityEngine;
using TowerDefense.Core;
using TowerDefense.StatusEffects;
using TowerDefense.Utils;

namespace TowerDefense.Enemies
{
    [RequireComponent(typeof(PathFollower))]
    [RequireComponent(typeof(StatusEffectHandler))]
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class EnemyBase : MonoBehaviour, IDamageable, IPoolable
    {
        // ===========================
        // CẤU HÌNH
        // ===========================

        [Header("Enemy Data")]
        [SerializeField] private EnemyData enemyData;

        // ===========================
        // REFERENCES
        // ===========================

        protected PathFollower pathFollower;
        protected StatusEffectHandler statusHandler;
        protected SpriteRenderer spriteRenderer;

        // ===========================
        // STATE
        // ===========================

        private float currentHp;
        private bool isDead;
        private bool isInitialized;

        private Action _returnCallback;

        // ===========================
        // PROPERTIES (IDamageable)
        // ===========================

        public float CurrentHp => currentHp;
        public float MaxHp => enemyData != null ? enemyData.maxHp : 0f;
        public float HpPercent => MaxHp > 0f ? currentHp / MaxHp : 0f;
        public bool IsDead => isDead;

        // ===========================
        // PROPERTIES
        // ===========================

        public EnemyData Data => enemyData;
        public PathFollower PathFollower => pathFollower;
        public StatusEffectHandler StatusHandler => statusHandler;
        public bool IsInitialized => isInitialized;

        // ===========================
        // IPOOLABLE
        // ===========================

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

        // ===========================
        // UNITY LIFECYCLE
        // ===========================

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

        // ===========================
        // PUBLIC API
        // ===========================

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

        // ===========================
        // IDAMAGEABLE
        // ===========================

        public void TakeDamage(float damage)
        {
            if (isDead || damage <= 0f) return;

            currentHp = Mathf.Max(0f, currentHp - damage);

            OnDamaged(damage);

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

        // ===========================
        // HOOKS (subclass override)
        // ===========================

        /// <summary>Gọi trong Awake. Cache component, khởi tạo ability riêng.</summary>
        protected virtual void OnEnemyAwake() { }

        /// <summary>Gọi sau Initialize() — enemy đã có path, chuẩn bị di chuyển.</summary>
        protected virtual void OnSpawned() { }

        /// <summary>Gọi mỗi lần nhận damage (trước khi Die nếu HP = 0).</summary>
        protected virtual void OnDamaged(float damageAmount) { }

        /// <summary>Gọi mỗi lần được heal.</summary>
        protected virtual void OnHealed(float healAmount) { }

        /// <summary>Gọi khi HP = 0, trước khi trả về pool.</summary>
        protected virtual void OnDeath() { }

        // ===========================
        // DEATH
        // ===========================

        private void Die()
        {
            if (isDead) return;
            isDead = true;

            // Subclass xử lý animation/loot/particle trước khi về pool
            OnDeath();

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

        // ===========================
        // GIZMOS
        // ===========================
#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (enemyData == null) return;

            string label = $"{enemyData.enemyName}\nHP: {currentHp:F0}/{MaxHp:F0} ({HpPercent:P0})";

            if (statusHandler != null)
            {
                if (statusHandler.IsFrozen) label += "\n❄ FROZEN";
                else if (statusHandler.IsSlowed) label += "\n🐢 SLOWED";
                if (statusHandler.IsSpeedBuffed) label += "\n⚡ BUFFED";
            }

            if (!isInitialized) label += "\n[Not Initialized]";

            UnityEditor.Handles.Label(transform.position + Vector3.up * 0.6f, label);
        }
#endif
    }
}