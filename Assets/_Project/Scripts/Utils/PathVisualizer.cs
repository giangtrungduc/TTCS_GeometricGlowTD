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
        [SerializeField] private Color borderColor = new Color(0f, 0.8f, 1f, 0.8f);
        [SerializeField] private float waypointRadius = 0.2f;
        [SerializeField] private bool showLabels = true;
        [SerializeField] private bool showRoadBorders = true;

        [Header("Border Join Settings")]
        [Tooltip("Giới hạn độ dài miter ở góc cua để tránh biên bị kéo quá dài ở góc nhọn.")]
        [SerializeField] private float maxMiterLengthMultiplier = 4f;

        private void OnDrawGizmos()
        {
            if (path == null || path.Length < 2) return;

            DrawCenterLine();
            DrawRoadBorders();
            DrawWaypoints();
        }

        private void DrawCenterLine()
        {
            Gizmos.color = pathColor;

            for (int i = 0; i < path.Length - 1; i++)
            {
                Vector3 from = path.GetWaypoint(i);
                Vector3 to = path.GetWaypoint(i + 1);

                Gizmos.DrawLine(from, to);
            }
        }

        private void DrawRoadBorders()
        {
            if (!showRoadBorders) return;
            if (path.PathHalfWidth <= 0f) return;

            int count = path.Length;
            Vector2[] leftPoints = new Vector2[count];
            Vector2[] rightPoints = new Vector2[count];

            for (int i = 0; i < count; i++)
            {
                CalculateBorderPointsAtWaypoint(i, out leftPoints[i], out rightPoints[i]);
            }

            Gizmos.color = borderColor;

            for (int i = 0; i < count - 1; i++)
            {
                Gizmos.DrawLine(leftPoints[i], leftPoints[i + 1]);
                Gizmos.DrawLine(rightPoints[i], rightPoints[i + 1]);
            }
        }

        private void CalculateBorderPointsAtWaypoint(int waypointIndex, out Vector2 left, out Vector2 right)
        {
            Vector2 point = path.GetWaypoint(waypointIndex);
            float width = path.PathHalfWidth;

            // Start point: dùng normal của segment đầu
            if (waypointIndex == 0)
            {
                Vector2 normal = path.GetSegmentNormal(1);
                left = point + normal * width;
                right = point - normal * width;
                return;
            }

            // End point: dùng normal của segment cuối
            if (waypointIndex == path.Length - 1)
            {
                Vector2 normal = path.GetSegmentNormal(path.Length - 1);
                left = point + normal * width;
                right = point - normal * width;
                return;
            }

            // Interior point:
            // Giao giữa đường offset của segment trước và segment sau.
            int prevTargetIndex = waypointIndex;
            int nextTargetIndex = waypointIndex + 1;

            Vector2 prevStart = path.GetSegmentStart(prevTargetIndex);
            Vector2 prevEnd = path.GetSegmentEnd(prevTargetIndex);
            Vector2 prevDir = path.GetSegmentDirection(prevTargetIndex);
            Vector2 prevNormal = path.GetSegmentNormal(prevTargetIndex);

            Vector2 nextStart = path.GetSegmentStart(nextTargetIndex);
            Vector2 nextEnd = path.GetSegmentEnd(nextTargetIndex);
            Vector2 nextDir = path.GetSegmentDirection(nextTargetIndex);
            Vector2 nextNormal = path.GetSegmentNormal(nextTargetIndex);

            Vector2 prevLeftPoint = prevEnd + prevNormal * width;
            Vector2 nextLeftPoint = nextStart + nextNormal * width;

            Vector2 prevRightPoint = prevEnd - prevNormal * width;
            Vector2 nextRightPoint = nextStart - nextNormal * width;

            bool hasLeftIntersection = TryLineIntersection(
                prevLeftPoint,
                prevDir,
                nextLeftPoint,
                nextDir,
                out left
            );

            bool hasRightIntersection = TryLineIntersection(
                prevRightPoint,
                prevDir,
                nextRightPoint,
                nextDir,
                out right
            );

            if (!hasLeftIntersection)
            {
                left = point + nextNormal * width;
            }

            if (!hasRightIntersection)
            {
                right = point - nextNormal * width;
            }

            // Chống góc quá nhọn làm miter kéo quá dài
            float maxDistance = width * Mathf.Max(1f, maxMiterLengthMultiplier);

            if (Vector2.Distance(left, point) > maxDistance)
            {
                Vector2 averagedNormal = (prevNormal + nextNormal).normalized;
                if (averagedNormal.sqrMagnitude < 0.0001f)
                {
                    averagedNormal = nextNormal;
                }

                left = point + averagedNormal * width;
            }

            if (Vector2.Distance(right, point) > maxDistance)
            {
                Vector2 averagedNormal = (prevNormal + nextNormal).normalized;
                if (averagedNormal.sqrMagnitude < 0.0001f)
                {
                    averagedNormal = nextNormal;
                }

                right = point - averagedNormal * width;
            }
        }

        private bool TryLineIntersection(
            Vector2 pointA,
            Vector2 directionA,
            Vector2 pointB,
            Vector2 directionB,
            out Vector2 intersection
        )
        {
            intersection = pointA;

            float cross = Cross(directionA, directionB);

            if (Mathf.Abs(cross) < 0.0001f)
            {
                return false;
            }

            Vector2 delta = pointB - pointA;
            float t = Cross(delta, directionB) / cross;

            intersection = pointA + directionA * t;
            return true;
        }

        private float Cross(Vector2 a, Vector2 b)
        {
            return a.x * b.y - a.y * b.x;
        }

        private void DrawWaypoints()
        {
            for (int i = 0; i < path.Length; i++)
            {
                Vector3 pos = path.GetWaypoint(i);

                if (i == 0)
                {
                    Gizmos.color = Color.green;
                }
                else if (i == path.Length - 1)
                {
                    Gizmos.color = Color.red;
                }
                else
                {
                    Gizmos.color = waypointColor;
                }

                Gizmos.DrawSphere(pos, waypointRadius);

#if UNITY_EDITOR
                if (showLabels)
                {
                    string label = i == 0
                        ? "START"
                        : i == path.Length - 1
                            ? "END"
                            : i.ToString();

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