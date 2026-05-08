using System.Collections.Generic;
using TowerDefense.Core;
using UnityEngine;

namespace TowerDefense.Enemies
{
    /// <summary>
    /// Điều khiển enemy di chuyển dọc theo WaypointPath.
    ///
    /// Lane movement:
    /// - Enemy không đi thẳng vào tim waypoint.
    /// - Enemy giữ laneOffset cố định so với tim đường.
    /// - Với mỗi waypoint ngoặt, target được tính bằng miter joint:
    ///   giao điểm hình học của hai đường offset liền kề.
    /// - Nhờ vậy enemy không bị cắt góc / drift lane khi chuyển segment.
    /// - laneOffset chỉ được tính khi Initialize, không tính lại mỗi frame.
    ///
    /// Visual direction:
    /// - Enemy xoay bằng transform.rotation theo hướng đang di chuyển.
    /// - Mặc định giả định sprite enemy nhìn sang phải theo trục X dương.
    /// </summary>
    public class PathFollower : MonoBehaviour
    {
        private const float WaypointReachThresholdSqr = 0.01f;
        private const float MinimumSpeed = 0.05f;
        private const float GeometryEpsilon = 0.0001f;
        private const float MiterDotEpsilon = 0.001f;
        private const float RotationOffsetDegrees = -90f;

        private const string ModifierSlow = "Slow";
        private const string ModifierBuff = "SpeedBuff";

        private WaypointPath currentPath;
        private float moveSpeed;
        private float baseMoveSpeed;
        private int currentWaypointIndex;
        private bool hasReachedEnd;
        private bool isInitialized;

        /// <summary>
        /// Độ lệch ngang cố định của enemy so với tim đường.
        /// Giá trị này được tính lúc spawn / initialize và giữ nguyên khi đi qua các segment.
        /// </summary>
        private float laneOffset;

        private readonly Dictionary<string, Dictionary<int, float>> speedModifiers =
            new Dictionary<string, Dictionary<int, float>>();

        public int CurrentWaypointIndex => currentWaypointIndex;
        public bool HasReachedEnd => hasReachedEnd;
        public float CurrentSpeed => moveSpeed;
        public float BaseMoveSpeed => baseMoveSpeed;
        public WaypointPath CurrentPath => currentPath;
        public float LaneOffset => laneOffset;

        public float DistanceToNextWaypoint
        {
            get
            {
                if (!isInitialized || currentPath == null) return float.MaxValue;
                if (hasReachedEnd) return 0f;

                Vector2 target = GetOffsetTargetPosition();
                return Vector2.Distance((Vector2)transform.position, target);
            }
        }

        public float ProgressRatio
        {
            get
            {
                if (!isInitialized || currentPath == null || currentPath.Length < 2) return 0f;
                if (hasReachedEnd) return 1f;

                int completedSegments = Mathf.Clamp(currentWaypointIndex - 1, 0, currentPath.Length - 1);

                Vector2 start = GetCurrentSegmentStart();
                Vector2 end = GetCurrentSegmentEnd();
                Vector2 direction = GetCurrentSegmentDirection();

                float segmentLength = Vector2.Distance(start, end);
                if (segmentLength <= GeometryEpsilon) return 0f;

                // transform.position có chứa laneOffset, nhưng dot với direction sẽ triệt tiêu phần lệch ngang.
                Vector2 fromStartToEnemy = (Vector2)transform.position - start;
                float alongDistance = Vector2.Dot(fromStartToEnemy, direction);
                float segmentProgress = Mathf.Clamp01(alongDistance / segmentLength);

                float totalSegments = currentPath.Length - 1;
                return Mathf.Clamp01((completedSegments + segmentProgress) / totalSegments);
            }
        }

        // ===========================
        // INITIALIZE
        // ===========================

        public void Initialize(WaypointPath path, float speed)
        {
            if (!ValidatePathAndSpeed(path, speed)) return;

            currentPath = path;
            baseMoveSpeed = speed;
            currentWaypointIndex = 1;
            hasReachedEnd = false;
            isInitialized = true;

            laneOffset = 0f;

            transform.position = (Vector3)GetOffsetPointOnCurrentSegment(path.GetSpawnPoint());

            speedModifiers.Clear();
            RecalculateSpeed();

            FaceCurrentSegmentDirection();
        }

        public void Initialize(WaypointPath path, float speed, Vector2 startPosition, int waypointIndex)
        {
            if (!ValidatePathAndSpeed(path, speed)) return;

            currentPath = path;
            baseMoveSpeed = speed;
            currentWaypointIndex = Mathf.Clamp(waypointIndex, 1, currentPath.Length - 1);
            hasReachedEnd = false;
            isInitialized = true;

            laneOffset = CalculateLaneOffsetFromPosition(startPosition);
            transform.position = GetClampedPositionOnCurrentSegment(startPosition);

            speedModifiers.Clear();
            RecalculateSpeed();

            FaceCurrentSegmentDirection();
        }

        private bool ValidatePathAndSpeed(WaypointPath path, float speed)
        {
            if (path == null)
            {
                Debug.LogWarning($"[PathFollower] Initialize thất bại: path là null ({name})");
                return false;
            }

            if (path.Length < 2)
            {
                Debug.LogWarning($"[PathFollower] Initialize thất bại: path cần ít nhất 2 waypoint ({name})");
                return false;
            }

            if (speed <= 0f)
            {
                Debug.LogWarning($"[PathFollower] Initialize: speed <= 0, enemy sẽ không di chuyển ({name})");
            }

            return true;
        }

        // ===========================
        // UPDATE
        // ===========================

        private void Update()
        {
            if (!isInitialized || hasReachedEnd || currentPath == null) return;
            if (moveSpeed <= 0f) return;

            MoveTowardsNextWaypoint();
        }

        // ===========================
        // MOVEMENT
        // ===========================

        private void MoveTowardsNextWaypoint()
        {
            Vector2 currentPos = transform.position;
            Vector2 targetPos = GetOffsetTargetPosition();

            Vector2 newPos = Vector2.MoveTowards(
                currentPos,
                targetPos,
                moveSpeed * Time.deltaTime
            );

            transform.position = newPos;

            RotateTowards(currentPos, newPos);

            if (HasReachedOrPassedTarget(currentPos, newPos, targetPos))
            {
                transform.position = targetPos;
                AdvanceWaypoint();
            }
        }

        private bool HasReachedOrPassedTarget(Vector2 previousPos, Vector2 currentPos, Vector2 targetPos)
        {
            if ((currentPos - targetPos).sqrMagnitude <= WaypointReachThresholdSqr)
            {
                return true;
            }

            Vector2 previousToTarget = targetPos - previousPos;
            Vector2 currentToTarget = targetPos - currentPos;

            if (previousToTarget.sqrMagnitude <= GeometryEpsilon)
            {
                return true;
            }

            // Nếu dot <= 0 nghĩa là enemy đã đi qua target theo hướng di chuyển tới target.
            return Vector2.Dot(previousToTarget, currentToTarget) <= 0f;
        }

        private void AdvanceWaypoint()
        {
            currentWaypointIndex++;

            if (currentWaypointIndex >= currentPath.Length)
            {
                OnReachedEnd();
                return;
            }

            // Không tính lại laneOffset ở đây.
            // laneOffset phải giữ nguyên để enemy không bị kéo dần về tim đường.
            FaceCurrentSegmentDirection();
        }

        private void OnReachedEnd()
        {
            if (hasReachedEnd) return;

            hasReachedEnd = true;

            EnemyBase enemy = GetComponent<EnemyBase>();
            if (enemy != null)
            {
                enemy.HandleReachedEnd();
                return;
            }

            GameEvents.RaiseEnemyReachedEnd(gameObject);
            gameObject.SetActive(false);
        }

        // ===========================
        // LANE OFFSET LOGIC
        // ===========================

        private Vector2 GetCurrentSegmentStart()
        {
            if (currentPath == null) return transform.position;
            return currentPath.GetSegmentStart(currentWaypointIndex);
        }

        private Vector2 GetCurrentSegmentEnd()
        {
            if (currentPath == null) return transform.position;
            return currentPath.GetSegmentEnd(currentWaypointIndex);
        }

        private Vector2 GetCurrentSegmentDirection()
        {
            if (currentPath == null) return Vector2.right;
            return currentPath.GetSegmentDirection(currentWaypointIndex);
        }

        private Vector2 GetCurrentSegmentNormal()
        {
            if (currentPath == null) return Vector2.up;
            return currentPath.GetSegmentNormal(currentWaypointIndex);
        }

        private float CalculateLaneOffsetFromPosition(Vector2 position)
        {
            Vector2 start = GetCurrentSegmentStart();
            Vector2 normal = GetCurrentSegmentNormal();

            float rawOffset = Vector2.Dot(position - start, normal);
            return Mathf.Clamp(rawOffset, -currentPath.PathHalfWidth, currentPath.PathHalfWidth);
        }

        private Vector2 GetOffsetTargetPosition()
        {
            if (currentPath == null) return transform.position;
            return GetMiterTargetAtWaypoint(currentWaypointIndex);
        }

        /// <summary>
        /// Tính target offset đúng tại waypoint hiện tại.
        ///
        /// Với path A -> B -> C và enemy có laneOffset:
        /// - Khi đang đi tới B, target không phải B + normal(A->B) * offset một cách độc lập.
        /// - Target phải là giao điểm / miter của hai đường offset:
        ///     line AB offset theo normal AB
        ///     line BC offset theo normal BC
        ///
        /// Công thức bisector:
        ///     miter = waypoint + normalize(n1 + n2) * (offset / dot(bisector, n1))
        ///
        /// Fallback cho waypoint cuối, đoạn thẳng hàng, hoặc góc suy biến sẽ dùng normal segment hiện tại.
        /// </summary>
        private Vector2 GetMiterTargetAtWaypoint(int waypointIndex)
        {
            if (currentPath == null) return transform.position;

            int lastWaypointIndex = currentPath.Length - 1;
            int clampedWaypointIndex = Mathf.Clamp(waypointIndex, 1, lastWaypointIndex);
            Vector2 waypoint = currentPath.GetWaypoint(clampedWaypointIndex);

            // Waypoint cuối không có segment kế tiếp để tạo miter joint.
            if (clampedWaypointIndex >= lastWaypointIndex)
            {
                Vector2 finalNormal = currentPath.GetSegmentNormal(clampedWaypointIndex);
                return waypoint + finalNormal * laneOffset;
            }

            Vector2 previousNormal = currentPath.GetSegmentNormal(clampedWaypointIndex);
            Vector2 nextNormal = currentPath.GetSegmentNormal(clampedWaypointIndex + 1);

            Vector2 normalSum = previousNormal + nextNormal;

            // Hai normal gần như đối nhau: góc 180 độ / path quay đầu / trường hợp suy biến.
            if (normalSum.sqrMagnitude <= GeometryEpsilon)
            {
                return waypoint + previousNormal * laneOffset;
            }

            Vector2 bisector = normalSum.normalized;
            float dot = Vector2.Dot(bisector, previousNormal);

            if (Mathf.Abs(dot) < MiterDotEpsilon)
            {
                return waypoint + previousNormal * laneOffset;
            }

            float miterLength = laneOffset / dot;
            return waypoint + bisector * miterLength;
        }

        private Vector2 GetOffsetPointOnCurrentSegment(Vector2 centerPoint)
        {
            if (currentPath == null) return centerPoint;

            Vector2 normal = GetCurrentSegmentNormal();
            return centerPoint + normal * laneOffset;
        }

        private Vector2 GetClampedPositionOnCurrentSegment(Vector2 position)
        {
            if (currentPath == null) return position;

            Vector2 start = GetCurrentSegmentStart();
            Vector2 direction = GetCurrentSegmentDirection();
            Vector2 normal = GetCurrentSegmentNormal();

            Vector2 fromStart = position - start;

            float along = Vector2.Dot(fromStart, direction);
            float segmentLength = currentPath.GetSegmentLength(currentWaypointIndex);
            along = Mathf.Clamp(along, 0f, segmentLength);

            return start + direction * along + normal * laneOffset;
        }

        // ===========================
        // VISUAL DIRECTION
        // ===========================

        private void FaceCurrentSegmentDirection()
        {
            if (currentPath == null || currentWaypointIndex >= currentPath.Length) return;

            Vector2 currentPos = transform.position;
            Vector2 target = GetOffsetTargetPosition();

            RotateTowards(currentPos, target);
        }

        private void RotateTowards(Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;

            if (direction.sqrMagnitude <= GeometryEpsilon)
            {
                return;
            }

            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(0f, 0f, angle + RotationOffsetDegrees);
        }

        // ===========================
        // SPEED MODIFIER API
        // ===========================

        public void AddSpeedModifier(string type, int sourceId, float multiplier)
        {
            if (!speedModifiers.ContainsKey(type))
            {
                speedModifiers[type] = new Dictionary<int, float>();
            }

            speedModifiers[type][sourceId] = multiplier;
            RecalculateSpeed();
        }

        public void RemoveSpeedModifier(string type, int sourceId)
        {
            if (!speedModifiers.TryGetValue(type, out var bucket)) return;

            bucket.Remove(sourceId);
            RecalculateSpeed();
        }

        public void RemoveAllModifiersOfType(string type)
        {
            if (!speedModifiers.ContainsKey(type)) return;

            speedModifiers.Remove(type);
            RecalculateSpeed();
        }

        public void ClearAllModifiers()
        {
            speedModifiers.Clear();
            RecalculateSpeed();
        }

        private void RecalculateSpeed()
        {
            float strongestSlowPercent = 0f;

            if (speedModifiers.TryGetValue(ModifierSlow, out var slowBucket) &&
                slowBucket.Count > 0)
            {
                foreach (float multiplier in slowBucket.Values)
                {
                    float slowPercent = 1f - multiplier;
                    if (slowPercent > strongestSlowPercent)
                    {
                        strongestSlowPercent = slowPercent;
                    }
                }
            }

            float strongestBuffPercent = 0f;

            if (speedModifiers.TryGetValue(ModifierBuff, out var buffBucket) &&
                buffBucket.Count > 0)
            {
                foreach (float multiplier in buffBucket.Values)
                {
                    float buffPercent = multiplier - 1f;
                    if (buffPercent > strongestBuffPercent)
                    {
                        strongestBuffPercent = buffPercent;
                    }
                }
            }

            float netPercent = strongestBuffPercent - strongestSlowPercent;
            moveSpeed = baseMoveSpeed * (1f + netPercent);

            if (moveSpeed < MinimumSpeed)
            {
                moveSpeed = MinimumSpeed;
            }
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (currentPath == null || currentPath.Length < 2) return;

            Gizmos.color = Color.gray;
            for (int i = 0; i < currentPath.Length - 1; i++)
            {
                Gizmos.DrawLine(
                    (Vector3)currentPath.GetWaypoint(i),
                    (Vector3)currentPath.GetWaypoint(i + 1)
                );
            }

            if (isInitialized && currentWaypointIndex < currentPath.Length)
            {
                Vector3 adjustedTarget = (Vector3)GetOffsetTargetPosition();

                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(adjustedTarget, 0.2f);

                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, adjustedTarget);
            }

            for (int i = 0; i < currentPath.Length; i++)
            {
                Gizmos.color = i < currentWaypointIndex ? Color.cyan : Color.red;
                Gizmos.DrawSphere((Vector3)currentPath.GetWaypoint(i), 0.12f);
            }

            UnityEditor.Handles.Label(
                transform.position + Vector3.up * 0.5f,
                $"Progress: {ProgressRatio:P0}\n" +
                $"Speed: {moveSpeed:F2}\n" +
                $"Offset: {laneOffset:F2}\n" +
                $"Dist: {DistanceToNextWaypoint:F2}"
            );
        }
#endif
    }
}
