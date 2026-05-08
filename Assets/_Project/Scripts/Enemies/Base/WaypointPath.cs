using UnityEngine;

namespace TowerDefense.Enemies
{
    [CreateAssetMenu(fileName = "NewWaypointPath", menuName = "TD/Waypoint Path", order = 0)]
    public class WaypointPath : ScriptableObject
    {
        // ============================
        // DATA
        // ============================

        [Header("Center Line")]

        [Tooltip("Danh sách điểm tim đường. Index 0 = spawn, index cuối = đích.")]
        [SerializeField] private Vector2[] waypoints;

        [Header("Road Bounds")]

        [Tooltip("Nửa chiều rộng đường. Enemy được phép lệch ngang tối đa trong khoảng [-pathHalfWidth, +pathHalfWidth].")]
        [SerializeField] [Min(0f)] private float pathHalfWidth = 0.5f;

        // ============================
        // PROPERTIES
        // ============================

        public int Length => waypoints != null ? waypoints.Length : 0;

        public float PathHalfWidth => Mathf.Max(0f, pathHalfWidth);

        public bool IsValid => waypoints != null && waypoints.Length >= 2;

        // ============================
        // WAYPOINT API
        // ============================

        public Vector2 GetWaypoint(int index)
        {
            if (waypoints == null || index < 0 || index >= waypoints.Length)
            {
                return Vector2.zero;
            }

            return waypoints[index];
        }

        public Vector2 GetSpawnPoint()
        {
            return GetWaypoint(0);
        }

        public Vector2 GetEndPoint()
        {
            return GetWaypoint(Length - 1);
        }

        // ============================
        // SEGMENT API
        // ============================

        /// <summary>
        /// targetWaypointIndex là waypoint enemy đang đi tới.
        /// Segment hiện tại là targetWaypointIndex - 1 -> targetWaypointIndex.
        /// </summary>
        public Vector2 GetSegmentStart(int targetWaypointIndex)
        {
            if (!IsValid) return Vector2.zero;

            int startIndex = Mathf.Clamp(targetWaypointIndex - 1, 0, Length - 1);
            return GetWaypoint(startIndex);
        }

        /// <summary>
        /// targetWaypointIndex là waypoint enemy đang đi tới.
        /// </summary>
        public Vector2 GetSegmentEnd(int targetWaypointIndex)
        {
            if (!IsValid) return Vector2.zero;

            int endIndex = Mathf.Clamp(targetWaypointIndex, 0, Length - 1);
            return GetWaypoint(endIndex);
        }

        /// <summary>
        /// Hướng chuẩn hóa từ start -> end của segment hiện tại.
        /// </summary>
        public Vector2 GetSegmentDirection(int targetWaypointIndex)
        {
            Vector2 start = GetSegmentStart(targetWaypointIndex);
            Vector2 end = GetSegmentEnd(targetWaypointIndex);

            Vector2 direction = end - start;

            if (direction.sqrMagnitude < 0.0001f)
            {
                return Vector2.right;
            }

            return direction.normalized;
        }

        /// <summary>
        /// Normal vuông góc với segment hiện tại.
        /// Dùng để tính độ lệch ngang của enemy so với tim đường.
        /// </summary>
        public Vector2 GetSegmentNormal(int targetWaypointIndex)
        {
            Vector2 direction = GetSegmentDirection(targetWaypointIndex);
            return new Vector2(-direction.y, direction.x);
        }

        // ============================
        // LENGTH
        // ============================

        public float GetTotalLength()
        {
            if (Length < 2) return 0f;

            float total = 0f;

            for (int i = 0; i < Length - 1; i++)
            {
                total += Vector2.Distance(waypoints[i], waypoints[i + 1]);
            }

            return total;
        }

        public float GetSegmentLength(int targetWaypointIndex)
        {
            if (!IsValid) return 0f;

            Vector2 start = GetSegmentStart(targetWaypointIndex);
            Vector2 end = GetSegmentEnd(targetWaypointIndex);

            return Vector2.Distance(start, end);
        }
    }
}