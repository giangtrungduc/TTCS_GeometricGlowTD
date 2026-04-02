using UnityEngine;

namespace TowerDefense.Projectiles
{
    [RequireComponent(typeof(SpriteRenderer))]
    public abstract class ProjectileBase : MonoBehaviour
    {
        // ============================
        // CẤU HÌNH
        // ============================

        [Header("Projectile Settings")]
        [Tooltip("Tốc độ bay (units/s)")]
        [SerializeField] protected float moveSpeed = 10f;

        [Tooltip("Khoảng cách va chạm")]
        [SerializeField] protected float hitDistance = 0.15f;

        [Tooltip("Tự huỷ sau X giây (chống bug đạn bay mãi)")]
        [SerializeField] protected float maxLifetime = 5f;

        [Tooltip("Xoay đầu đạn theo hướng bay")]
        [SerializeField] protected bool rotateTowardsTarget = true;

        // ============================
        // STATE
        // ============================

        protected GameObject target;
        protected float damage;
        protected bool isLaunched;
        protected float lifetimeTimer;
        protected Vector3 lastKnownTargetPos;
        protected SpriteRenderer spriteRenderer;

        // ============================
        // PROPERTIES
        // ============================

        public GameObject Target => target;
        public float Damage => damage;
        public bool IsActive => isLaunched;

        // ============================
        // UNITY LIFECYCLE
        // ============================

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

            // 2. Kiểm tra target hợp lệ
            if (target == null || !target.activeInHierarchy)
            {
                OnTargetLost();
                return;
            }

            lastKnownTargetPos = target.transform.position;

            // 3. Di chuyển
            MoveTowardsTarget();

            // 4. Kiểm tra va chạm
            if (Vector2.Distance(transform.position, lastKnownTargetPos) <= hitDistance)
            {
                HandleHit();
            }

            OnProjectileUpdate();
        }

        // ============================
        // PUBLIC METHODS
        // ============================

        /// <summary>Khởi động đạn.</summary>
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

            if (rotateTowardsTarget) RotateTowards(lastKnownTargetPos);

            OnLaunched();
        }

        public void Launch(GameObject newTarget, float newDamage, float customSpeed)
        {
            moveSpeed = customSpeed;
            Launch(newTarget, newDamage);
        }

        // ============================
        // ABSTRACT & VIRTUAL
        // ============================

        /// <summary>Logic khi chạm mục tiêu (Class con tự định nghĩa).</summary>
        protected abstract void OnHit(GameObject hitTarget, float hitDamage);

        /// <summary>Gọi ngay sau Launch(). Dùng để init visual, trail...</summary>
        protected virtual void OnLaunched() { }

        /// <summary>Gọi cuối hàm Update() nếu đạn đang bay.</summary>
        protected virtual void OnProjectileUpdate() { }

        /// <summary>Xử lý khi mục tiêu chết trước khi đạn tới.</summary>
        protected virtual void OnTargetLost()
        {
            if (Vector2.Distance(transform.position, lastKnownTargetPos) <= hitDistance)
            {
                ReturnToPool();
                return;
            }

            // Tiếp tục bay đến vị trí cuối cùng
            transform.position = Vector2.MoveTowards(transform.position, lastKnownTargetPos, moveSpeed * Time.deltaTime);
        }

        protected virtual void OnTimeout()
        {
            Debug.LogWarning($"[ProjectileBase] '{gameObject.name}' timed out. Vượt quá {maxLifetime}s.");
        }

        // ============================
        // PRIVATE
        // ============================

        private void MoveTowardsTarget()
        {
            transform.position = Vector2.MoveTowards(transform.position, lastKnownTargetPos, moveSpeed * Time.deltaTime);

            if (rotateTowardsTarget) RotateTowards(lastKnownTargetPos);
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

        protected void ReturnToPool()
        {
            isLaunched = false;
            target = null;
            gameObject.SetActive(false); // Trả về pool thông qua OnDisable
        }

        // ============================
        // GIZMOS
        // ============================

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
    }
}