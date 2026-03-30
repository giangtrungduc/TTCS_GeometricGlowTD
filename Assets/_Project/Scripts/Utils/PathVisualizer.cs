using UnityEngine;
using TowerDefense.Enemies;

namespace TowerDefense.Utils
{
    public class PathVisualizer : MonoBehaviour
    {
        [Header("Path to Visualize")]
        [SerializeField] private WaypointPath path;

        [Header("Visual Settings")]
        [SerializeField] private Color pathColor = Color.yellow;
        [SerializeField] private Color waypointColor = Color.red;
        [SerializeField] private float waypointRadius = 0.2f;
        [SerializeField] private bool showLabels = true;

        private void OnDrawGizmos()
        {
            if (path == null || path.Length < 2) return;

            // Vẽ đường nối các waypoint
            Gizmos.color = pathColor;
            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector3 from = (Vector3)path.GetWaypoint(i);
                Vector3 to = (Vector3)path.GetWaypoint(i + 1);
                Gizmos.DrawLine(from, to);
            }

            // Vẽ từng waypoint
            for (int i = 0; i < path.Length; i++)
            {
                Vector3 pos = (Vector3)path.GetWaypoint(i);

                // Điểm đầu = xanh lá, điểm cuối = đỏ, giữa = vàng
                if (i == 0)
                    Gizmos.color = Color.green;
                else if (i == path.Length - 1)
                    Gizmos.color = Color.red;
                else
                    Gizmos.color = waypointColor;

                Gizmos.DrawSphere(pos, waypointRadius);

                // Hiện số thứ tự
#if UNITY_EDITOR
                if (showLabels)
                {
                    string label = i == 0 ? "START" :
                                   i == path.Length - 1 ? "END" :
                                   i.ToString();

                    UnityEditor.Handles.Label(
                        pos + Vector3.up * 0.4f,
                        label
                    );
                }
#endif
            }
        }
    }
}