using UnityEngine;

namespace TowerDefense.Enemies
{
    [CreateAssetMenu(fileName = "NewWaypointPath", menuName = "TD/Waypoint Path", order = 0)]
    public class WaypointPath : ScriptableObject
    {
        // ============================
        // DATA
        // ============================
        [Tooltip("Danh sách điểm đường đi. Index 0 = spawn, index cuối = đích.")]
        [SerializeField] private Vector2[] waypoints;

        // ============================
        // PROPERTIES
        // ============================

        // Số lượng waypoint trong path
        public int Length => waypoints != null ? waypoints.Length : 0;

        // ============================
        // PUBLIC METHODS
        // ============================

        public Vector2 GetWaypoint(int index)
        {
            if(waypoints == null || index < 0 || index >= waypoints.Length)
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
        public float GetTotalLength()
        {
            if (Length < 2) return 0f;

            float total = 0f;

            for(int i = 0; i < Length - 1; i++)
            {
                total += Vector2.Distance(waypoints[i], waypoints[i + 1]);
            }
            return total;
        }
    }
}
