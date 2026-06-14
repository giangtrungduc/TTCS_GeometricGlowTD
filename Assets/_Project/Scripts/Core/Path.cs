using UnityEngine;

namespace TowerDefense.Core
{
    /// <summary>
    /// Thay thế cho ScriptableObject WaypointPath rườm rà.
    /// Kéo thả các cục Transform vào mảng này là Quái tự biết đường đi.
    /// </summary>
    public class Path : MonoBehaviour
    {
        [Tooltip("Kéo các điểm mốc (Waypoints) từ đầu map tới cuối map vào đây")]
        public Transform[] waypoints;

        private void OnDrawGizmos()
        {
            if (waypoints == null || waypoints.Length < 2) return;
            
            Gizmos.color = Color.cyan;
            for (int i = 0; i < waypoints.Length - 1; i++)
            {
                if (waypoints[i] != null && waypoints[i+1] != null)
                {
                    Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
                    // Vẽ cục tròn tại mốc
                    Gizmos.DrawSphere(waypoints[i].position, 0.2f);
                }
            }
            // Vẽ cục cuối
            if (waypoints[waypoints.Length - 1] != null)
                Gizmos.DrawSphere(waypoints[waypoints.Length - 1].position, 0.2f);
        }
    }
}
