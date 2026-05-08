using System;
using UnityEngine;
using TowerDefense.Utils;
using TowerDefense.Enemies;

namespace TowerDefense.Projectiles
{
    /// <summary>
    /// Base class cho tất cả projectile.
    /// Chịu trách nhiệm:
    /// - Bay tới target.
    /// - Kiểm tra va chạm.
    /// - Gây damage.
    /// - Truyền ProjectileEffect nếu có.
    /// - Gọi callback effect do tower truyền vào nếu có.
    /// - Spawn VFX nổ riêng của từng prefab đạn.
    /// - Return về ObjectPool.
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

        [Header("Projectile Effects")]

        [Tooltip("Effect mặc định gắn trên prefab đạn. Có thể để trống.")]
        [SerializeField] private ProjectileEffect[] defaultEffects;

        [Header("Impact VFX")]

        [Tooltip("Hiệu ứng nổ / va chạm riêng của prefab đạn này.")]
        [SerializeField] private GameObject impactVfxPrefab;

        [Tooltip("Tự huỷ VFX sau X giây. Đặt <= 0 nếu VFX tự huỷ bằng ParticleSystem.")]
        [SerializeField] private float impactVfxLifetime = 2f;

        [Tooltip("Copy rotation của projectile sang VFX nổ.")]
        [SerializeField] private bool impactVfxUseProjectileRotation = false;

        // ===========================
        // STATE
        // ===========================

        protected GameObject target;
        protected float damage;
        protected bool isLaunched;
        protected float lifetimeTimer;
        protected Vector3 lastKnownTargetPos;
        protected SpriteRenderer spriteRenderer;
        protected ProjectileEffect[] effects;
        protected float defaultMoveSpeed;

        private Action _returnCallback;
        private Action<GameObject> onApplyEffectCallback;

        private Color originalSpriteColor;

        // ===========================
        // PROPERTIES
        // ===========================

        public GameObject Target => target;
        public float Damage => damage;
        public bool IsActive => isLaunched;

        // ===========================
        // IPOOLABLE IMPLEMENTATION
        // ===========================

        public void SetReturnCallback(Action returnCallback)
        {
            _returnCallback = returnCallback;
        }

        public virtual void OnGetFromPool() { }

        public virtual void OnReturnToPool() { }

        // ===========================
        // UNITY LIFECYCLE
        // ===========================

        protected virtual void Awake()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            defaultMoveSpeed = moveSpeed;

            if (spriteRenderer != null)
            {
                originalSpriteColor = spriteRenderer.color;
            }
        }

        protected virtual void OnEnable()
        {
            isLaunched = false;
            target = null;
            damage = 0f;
            lifetimeTimer = 0f;
            moveSpeed = defaultMoveSpeed;

            effects = defaultEffects;
            onApplyEffectCallback = null;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = originalSpriteColor;
            }
        }

        protected virtual void OnDisable()
        {
            isLaunched = false;
            target = null;
            onApplyEffectCallback = null;
        }

        protected virtual void Update()
        {
            if (!isLaunched) return;

            lifetimeTimer += Time.deltaTime;
            if (lifetimeTimer >= maxLifetime)
            {
                OnTimeout();
                ReturnToPool();
                return;
            }

            if (target == null || !target.activeInHierarchy)
            {
                OnTargetLost();
                return;
            }

            lastKnownTargetPos = target.transform.position;

            MoveTowardsTarget();

            if ((Vector2)transform.position == (Vector2)lastKnownTargetPos ||
                Vector2.Distance(transform.position, lastKnownTargetPos) <= hitDistance)
            {
                HandleHit();
                return;
            }

            OnProjectileUpdate();
        }

        // ===========================
        // PUBLIC API
        // ===========================

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

        public void Launch(GameObject newTarget, float newDamage, float customSpeed)
        {
            moveSpeed = customSpeed;
            Launch(newTarget, newDamage);
        }

        public void SetEffects(params ProjectileEffect[] newEffects)
        {
            effects = newEffects;
        }

        /// <summary>
        /// Dùng cho tower truyền logic effect trực tiếp vào projectile.
        /// Ví dụ IceTower truyền callback add SlowEffect theo stats từng cấp.
        /// </summary>
        public void SetOnApplyEffect(Action<GameObject> callback)
        {
            onApplyEffectCallback = callback;
        }

        // ===========================
        // ABSTRACT & VIRTUAL HOOKS
        // ===========================

        protected abstract void OnHit(GameObject hitTarget, float hitDamage);

        protected virtual void OnLaunched() { }

        protected virtual void OnProjectileUpdate() { }

        protected virtual void OnTargetLost()
        {
            if (Vector2.Distance(transform.position, lastKnownTargetPos) <= hitDistance)
            {
                ReturnToPool();
                return;
            }

            transform.position = Vector2.MoveTowards(
                transform.position,
                lastKnownTargetPos,
                moveSpeed * Time.deltaTime
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
        // DAMAGE + EFFECT
        // ===========================

        protected void ApplyDamageAndEffects(GameObject enemyObject, float hitDamage)
        {
            if (enemyObject == null) return;
            if (!enemyObject.activeInHierarchy) return;

            IDamageable damageable = enemyObject.GetComponent<IDamageable>();

            if (damageable != null)
            {
                damageable.TakeDamage(hitDamage);
            }

            if (!enemyObject.activeInHierarchy) return;

            if (effects != null)
            {
                for (int i = 0; i < effects.Length; i++)
                {
                    ProjectileEffect effect = effects[i];
                    if (effect == null) continue;

                    effect.Apply(enemyObject);
                }
            }

            onApplyEffectCallback?.Invoke(enemyObject);
        }

        // ===========================
        // PRIVATE HELPERS
        // ===========================

        private void MoveTowardsTarget()
        {
            transform.position = Vector2.MoveTowards(
                transform.position,
                lastKnownTargetPos,
                moveSpeed * Time.deltaTime
            );

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
            Vector3 hitPosition = transform.position;

            SpawnImpactVfx(hitPosition);

            if (target != null && target.activeInHierarchy)
            {
                OnHit(target, damage);
            }

            ReturnToPool();
        }

        private void SpawnImpactVfx(Vector3 position)
        {
            if (impactVfxPrefab == null) return;

            Quaternion rotation = impactVfxUseProjectileRotation
                ? transform.rotation
                : Quaternion.identity;

            GameObject vfx = Instantiate(impactVfxPrefab, position, rotation);

            if (impactVfxLifetime > 0f)
            {
                Destroy(vfx, impactVfxLifetime);
            }
        }

        // ===========================
        // POOL RETURN
        // ===========================

        protected void ReturnToPool()
        {
            isLaunched = false;
            target = null;
            onApplyEffectCallback = null;

            Action cb = _returnCallback;
            _returnCallback = null;

            if (cb != null)
            {
                cb.Invoke();
            }
            else
            {
                gameObject.SetActive(false);
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
