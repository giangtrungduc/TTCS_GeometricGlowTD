using System;
using UnityEngine;
using TowerDefense.Utils;

namespace TowerDefense.Projectiles
{
    /// <summary>
    /// Base class cho tất cả projectile.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class ProjectileBase : MonoBehaviour, IPoolable
    {
        // ===========================
        // CẤU HÌNH
        // ===========================

        [Header("Projectile Settings")]

        [Tooltip("Tốc độ bay (units/s)")]
        [SerializeField] protected float moveSpeed = 10f;

        [Tooltip("Khoảng cách để tính va chạm với target (world units)")]
        [SerializeField] protected float hitDistance = 0.15f;

        [Tooltip("Tự huỷ sau X giây — chống bug đạn bay mãi không trúng")]
        [SerializeField] protected float maxLifetime = 5f;

        [Tooltip("Xoay đầu đạn theo hướng bay")]
        [SerializeField] protected bool rotateTowardsTarget = true;

        // ===========================
        // STATE
        // ===========================

        protected GameObject target;
        protected float damage;
        protected bool isLaunched;
        protected float lifetimeTimer;
        protected Vector3 lastKnownTargetPos;
        protected SpriteRenderer spriteRenderer;

        private Action _returnCallback;

        // ===========================
        // PROPERTIES
        // ===========================

        public GameObject Target => target;
        public float Damage => damage;
        public bool IsActive => isLaunched;

        // ===========================
        // IPOOLABLE IMPLEMENTATION
        // ===========================

        /// <summary>
        /// Được ObjectPool.Get() gọi để inject callback trả về đúng pool.
        /// Tower không cần gọi method này — ObjectPool tự xử lý.
        /// </summary>
        public void SetReturnCallback(Action returnCallback)
        {
            _returnCallback = returnCallback;
        }

        /// <summary>
        /// Gọi sau SetActive(true) và position đã đúng.
        /// Override để reset visual, particle, animation.
        /// </summary>
        public virtual void OnGetFromPool() { }

        /// <summary>
        /// Gọi trước SetActive(false).
        /// Override để dọn dẹp trail, particle, unsubscribe event.
        /// </summary>
        public virtual void OnReturnToPool() { }

        // ===========================
        // UNITY LIFECYCLE
        // ===========================

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        protected virtual void OnEnable()
        {
            isLaunched = false;
            target = null;
            damage = 0f;
            lifetimeTimer = 0f;
        }

        protected virtual void OnDisable()
        {
            isLaunched = false;
            target = null;
        }

        protected virtual void Update()
        {
            if (!isLaunched) return;

            // 1. Kiểm tra timeout
            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= maxLifetime)
            {
                OnTimeout();
                ReturnToPool();
                return;
            }

            // 2. Kiểm tra target còn hợp lệ
            if (target == null || !target.activeInHierarchy)
            {
                OnTargetLost();
                return;
            }

            lastKnownTargetPos = target.transform.position;

            // 3. Di chuyển
            MoveTowardsTarget();

            // 4. Kiểm tra va chạm
            if ((Vector2)transform.position == (Vector2)lastKnownTargetPos || Vector2.Distance(transform.position, lastKnownTargetPos) <= hitDistance)
            {
                HandleHit();
                return;
            }

            OnProjectileUpdate();
        }

        // ===========================
        // PUBLIC API
        // ===========================

        /// <summary>
        /// Khởi động đạn hướng về target.
        /// Gọi SAU khi đã lấy từ pool và set các thông số cần thiết.
        /// </summary>
        public virtual void Launch(GameObject newTarget, float newDamage)
        {
            if (newTarget == null)
            {
                ReturnToPool();
                return;
            }

            target = newTarget;
            damage = newDamage;
            lifetimeTimer = 0f;
            isLaunched = true;
            lastKnownTargetPos = target.transform.position;

            if (rotateTowardsTarget)
            {
                RotateTowards(lastKnownTargetPos);

            }
            OnLaunched();
        }

        /// <summary>Launch với custom speed (override moveSpeed cho lần bắn này).</summary>
        public void Launch(GameObject newTarget, float newDamage, float customSpeed)
        {
            moveSpeed = customSpeed;
            Launch(newTarget, newDamage);
        }

        // ===========================
        // ABSTRACT & VIRTUAL HOOKS
        // ===========================

        /// <summary>Xử lý logic khi chạm mục tiêu. Bắt buộc override.</summary>
        protected abstract void OnHit(GameObject hitTarget, float hitDamage);

        /// <summary>Gọi ngay sau Launch(). Dùng để khởi tạo visual, trail, particle.</summary>
        protected virtual void OnLaunched() { }

        /// <summary>Gọi cuối Update() mỗi frame khi đạn đang bay.</summary>
        protected virtual void OnProjectileUpdate() { }

        /// <summary>
        /// Gọi khi target chết/mất trước khi đạn tới.
        /// Mặc định: tiếp tục bay đến lastKnownTargetPos rồi return pool.
        /// </summary>
        protected virtual void OnTargetLost()
        {
            if (Vector2.Distance(transform.position, lastKnownTargetPos) <= hitDistance)
            {
                ReturnToPool();
                return;
            }

            // Tiếp tục bay về vị trí cuối cùng đã biết
            transform.position = Vector2.MoveTowards(
                transform.position, lastKnownTargetPos, moveSpeed * Time.deltaTime
            );

            if (rotateTowardsTarget)
            {
                RotateTowards(lastKnownTargetPos);
            }
        }

        protected virtual void OnTimeout()
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.LogWarning($"[ProjectileBase] '{name}' timeout sau {maxLifetime}s.");
#endif
        }

        // ===========================
        // PRIVATE HELPERS
        // ===========================

        private void MoveTowardsTarget()
        {
            transform.position = Vector2.MoveTowards(transform.position, lastKnownTargetPos, moveSpeed * Time.deltaTime);

            if (rotateTowardsTarget)
            {
                RotateTowards(lastKnownTargetPos);
            }
        }

        private void RotateTowards(Vector3 targetPos)
        {
            Vector2 direction = (targetPos - transform.position).normalized;
            if (direction.sqrMagnitude < 0.001f) return;

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void HandleHit()
        {
            if (target != null && target.activeInHierarchy)
            {
                OnHit(target, damage);
            }

            ReturnToPool();
        }

        // ===========================
        // POOL RETURN
        // ===========================

        /// <summary>
        /// Trả đạn về pool đúng cách.
        /// Nếu có _returnCallback (inject bởi ObjectPool): gọi pool.Return(this) → push vào stack.
        /// Nếu không có (fallback): chỉ SetActive(false).
        /// </summary>
        protected void ReturnToPool()
        {
            isLaunched = false;
            target = null;

            // Lấy callback ra và clear trước khi invoke — chống re-entrance
            Action cb = _returnCallback;
            _returnCallback = null;

            if (cb != null)
            {
                cb.Invoke(); // → pool.Return(this) → OnReturnToPool() → SetActive(false) → Push
            }
            else
            {
                gameObject.SetActive(false); // fallback nếu không qua ObjectPool
            }
        }

        // ===========================
        // GIZMOS
        // ===========================
#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            if (!isLaunched) return;

            if (target != null && target.activeInHierarchy)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, target.transform.position);
            }

            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, hitDistance);
        }
#endif
    }
}