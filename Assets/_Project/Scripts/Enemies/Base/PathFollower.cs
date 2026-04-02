using System.Collections.Generic;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Điều khiển di chuyển của enemy dọc theo WaypointPath.
    /// Hỗ trợ hệ thống speed modifier đa nguồn (Freeze > Slow > SpeedBuff).
    /// An toàn với Object Pool: mọi trạng thái đều được reset trong Initialize().
    /// </summary>
    public class PathFollower : MonoBehaviour
    {
        // ===========================
        // CONSTANTS
        // ===========================

        private const float WaypointReachThresholdSqr = 0.0025f;
        private const float MinimumSpeed = 0.05f;

        private const string ModifierFreeze = "Freeze";
        private const string ModifierSlow = "Slow";
        private const string ModifierBuff = "SpeedBuff";

        // ===========================
        // STATE
        // ===========================

        private WaypointPath currentPath;
        private float moveSpeed;
        private float baseMoveSpeed;
        private int currentWaypointIndex;
        private bool hasReachedEnd;
        private bool isInitialized;

        private readonly Dictionary<string, Dictionary<int, float>> speedModifiers = new Dictionary<string, Dictionary<int, float>>();

        // ===========================
        // PROPERTIES
        // ===========================

        public int CurrentWaypointIndex => currentWaypointIndex;
        public bool HasReachedEnd => hasReachedEnd;
        public float CurrentSpeed => moveSpeed;
        public float BaseMoveSpeed => baseMoveSpeed;

        /// <summary>Khoảng cách Euclidean đến waypoint tiếp theo.</summary>
        public float DistanceToNextWaypoint
        {
            get
            {
                if (!isInitialized || currentPath == null) return float.MaxValue;
                if (hasReachedEnd) return 0f;

                Vector2 target = currentPath.GetWaypoint(currentWaypointIndex);
                return Vector2.Distance((Vector2)transform.position, target);
            }
        }

        /// <summary>
        /// Tỉ lệ tiến độ [0, 1] tính theo khoảng cách thực trên path.
        /// Chính xác đến từng pixel trong segment hiện tại.
        /// </summary>
        public float ProgressRatio
        {
            get
            {
                if (!isInitialized || currentPath == null || currentPath.Length < 2) return 0f;
                if (hasReachedEnd) return 1f;

                // Các segment đã hoàn thành hoàn toàn
                int completedWaypoints = currentWaypointIndex - 1;

                // Tiến độ trong segment hiện tại
                float segmentProgress = 0f;
                if (currentWaypointIndex > 0 && currentWaypointIndex < currentPath.Length)
                {
                    Vector2 prev = currentPath.GetWaypoint(currentWaypointIndex - 1);
                    Vector2 next = currentPath.GetWaypoint(currentWaypointIndex);
                    float segmentLength = Vector2.Distance(prev, next);

                    if (segmentLength > 0.0001f)
                    {
                        float distanceCovered = segmentLength - DistanceToNextWaypoint;
                        segmentProgress = Mathf.Clamp01(distanceCovered / segmentLength);
                    }
                }

                float totalSegments = currentPath.Length - 1;
                return Mathf.Clamp01((completedWaypoints + segmentProgress) / totalSegments);
            }
        }

        // ===========================
        // LIFECYCLE
        // ===========================

        /// <summary>
        /// Khởi tạo enemy tại spawn point và chuẩn bị di chuyển.
        /// Phải gọi trước khi object được kích hoạt.
        /// An toàn với Object Pool: reset toàn bộ trạng thái cũ.
        /// </summary>
        public void Initialize(WaypointPath path, float speed)
        {
            if (path == null)
            {
                Debug.LogWarning($"[PathFollower] Initialize thất bại: path là null ({name})");
                return;
            }
            if (path.Length < 2)
            {
                Debug.LogWarning($"[PathFollower] Initialize thất bại: path cần ít nhất 2 waypoint ({name})");
                return;
            }
            if (speed <= 0f)
            {
                Debug.LogWarning($"[PathFollower] Initialize: speed <= 0, enemy sẽ không di chuyển ({name})");
            }

            currentPath = path;
            baseMoveSpeed = speed;
            currentWaypointIndex = 1;       // Waypoint 0 là spawn point, bắt đầu tiến đến waypoint 1
            hasReachedEnd = false;
            isInitialized = true;

            // Đặt enemy đúng vị trí spawn
            transform.position = (Vector3)path.GetSpawnPoint();

            // Reset toàn bộ effect cũ (quan trọng khi dùng Object Pool)
            speedModifiers.Clear();
            RecalculateSpeed();

            // Quay mặt về phía waypoint đầu tiên ngay từ đầu
            if (currentWaypointIndex < currentPath.Length)
            {
                FlipSpriteTowards((Vector2)transform.position,
                                  currentPath.GetWaypoint(currentWaypointIndex));
            }
        }

        private void Update()
        {
            if (!isInitialized || hasReachedEnd || currentPath == null) return;

            // Freeze hoặc speed = 0 thì đứng im
            if (moveSpeed <= 0f) return;

            MoveTowardsNextWaypoint();
        }

        // ===========================
        // MOVEMENT
        // ===========================

        private void MoveTowardsNextWaypoint()
        {
            Vector2 currentPos = transform.position;
            Vector2 targetPos = currentPath.GetWaypoint(currentWaypointIndex);

            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);
            transform.position = newPos;

            FlipSpriteTowards(currentPos, newPos);

            // Dùng sqrMagnitude thay Distance để tránh Sqrt mỗi frame
            if ((newPos - targetPos).sqrMagnitude < WaypointReachThresholdSqr)
            {
                AdvanceWaypoint();
            }
        }

        private void AdvanceWaypoint()
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= currentPath.Length)
            {
                OnReachedEnd();
            }
        }

        private void OnReachedEnd()
        {
            if (hasReachedEnd) return;

            hasReachedEnd = true;
            GameEvents.RaiseEnemyReachedEnd(gameObject);
            gameObject.SetActive(false);
        }

        /// <summary>Lật sprite theo hướng di chuyển ngang (không dùng Rotate để tránh ảnh hưởng child).</summary>
        private void FlipSpriteTowards(Vector2 from, Vector2 to)
        {
            float dx = to.x - from.x;
            if (Mathf.Abs(dx) < 0.0001f) return;

            Vector3 scale = transform.localScale;
            scale.x = dx < 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // ===========================
        // SPEED MODIFIER API
        // ===========================

        /// <summary>
        /// Thêm hoặc cập nhật một speed modifier.
        /// </summary>
        /// <param name="type">Loại effect: "Freeze", "Slow", "SpeedBuff"</param>
        /// <param name="sourceId">ID định danh nguồn gốc (GetInstanceID() của tower/skill)</param>
        /// <param name="multiplier">
        /// </param>
        public void AddSpeedModifier(string type, int sourceId, float multiplier)
        {
            if (!speedModifiers.ContainsKey(type))
                speedModifiers[type] = new Dictionary<int, float>();

            speedModifiers[type][sourceId] = multiplier;
            RecalculateSpeed();
        }

        /// <summary>Gỡ bỏ speed modifier của một nguồn cụ thể.</summary>
        public void RemoveSpeedModifier(string type, int sourceId)
        {
            if (!speedModifiers.TryGetValue(type, out var bucket)) return;

            bucket.Remove(sourceId);
            RecalculateSpeed();
        }

        /// <summary>Gỡ bỏ toàn bộ modifier của một loại (VD: hết hiệu ứng slow của một tower bị phá hủy).</summary>
        public void RemoveAllModifiersOfType(string type)
        {
            if (!speedModifiers.ContainsKey(type)) return;

            speedModifiers.Remove(type);
            RecalculateSpeed();
        }

        /// <summary>Gỡ toàn bộ modifier, phục hồi tốc độ gốc.</summary>
        public void ClearAllModifiers()
        {
            speedModifiers.Clear();
            RecalculateSpeed();
        }

        // ===========================
        // SPEED CALCULATION
        // ===========================

        /// <summary>
        /// Tính lại tốc độ theo thứ tự ưu tiên:
        ///   1. Freeze  → moveSpeed = 0, dừng toàn bộ
        ///   2. Slow    → chỉ lấy multiplier NHỎ NHẤT (hiệu ứng mạnh nhất thắng)
        ///   3. SpeedBuff → chỉ lấy multiplier LỚN NHẤT (hiệu ứng mạnh nhất thắng)
        /// </summary>
        private void RecalculateSpeed()
        {
            // --- Freeze: ưu tiên tuyệt đối ---
            if (speedModifiers.TryGetValue(ModifierFreeze, out var freezeBucket)
                && freezeBucket.Count > 0)
            {
                moveSpeed = 0f;
                return;
            }

            moveSpeed = baseMoveSpeed;

            // --- Slow: lấy multiplier nhỏ nhất ---
            if (speedModifiers.TryGetValue(ModifierSlow, out var slowBucket)
                && slowBucket.Count > 0)
            {
                float minMultiplier = 1f;
                foreach (float mult in slowBucket.Values)
                {
                    if (mult < minMultiplier) minMultiplier = mult;
                }
                moveSpeed *= minMultiplier;
            }

            // --- SpeedBuff: lấy multiplier lớn nhất ---
            if (speedModifiers.TryGetValue(ModifierBuff, out var buffBucket)
                && buffBucket.Count > 0)
            {
                float maxMultiplier = 1f;
                foreach (float mult in buffBucket.Values)
                {
                    if (mult > maxMultiplier) maxMultiplier = mult;
                }
                moveSpeed *= maxMultiplier;
            }

            // Sàn tốc độ: không để enemy kẹt cứng do float precision (chỉ áp dụng khi không Freeze)
            if (moveSpeed < MinimumSpeed) moveSpeed = MinimumSpeed;
        }

        // ===========================
        // GIZMOS
        // ===========================
#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (currentPath == null || currentPath.Length < 2) return;

            // Toàn bộ path (xám)
            Gizmos.color = Color.gray;
            for (int i = 0; i < currentPath.Length - 1; i++)
            {
                Gizmos.DrawLine((Vector3)currentPath.GetWaypoint(i),
                                (Vector3)currentPath.GetWaypoint(i + 1));
            }

            // Waypoint đích hiện tại (vàng) + đường dẫn từ enemy (xanh lá)
            if (isInitialized && currentWaypointIndex < currentPath.Length)
            {
                Vector3 currentTarget = (Vector3)currentPath.GetWaypoint(currentWaypointIndex);

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(currentTarget, 0.2f);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, currentTarget);
            }

            // Tất cả waypoints: đã qua = cyan, chưa qua = đỏ
            for (int i = 0; i < currentPath.Length; i++)
            {
                Gizmos.color = i < currentWaypointIndex ? Color.cyan : Color.red;
                Gizmos.DrawSphere((Vector3)currentPath.GetWaypoint(i), 0.12f);
            }

            // Hiển thị ProgressRatio trên Scene view
            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.5f,
                $"Progress: {ProgressRatio:P0}\nSpeed: {moveSpeed:F2}");
        }
#endif
    }
}