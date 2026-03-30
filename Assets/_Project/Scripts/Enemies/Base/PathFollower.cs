using TowerDefense.Core;
using UnityEditor;
using UnityEngine;

namespace TowerDefense.Enemies
{
    public class PathFollower : MonoBehaviour
    {
        // ============================
        // STATE
        // ============================
        private WaypointPath currentPath;

        // Tốc độ di chuyển
        private float moveSpeed;

        // Tốc độ gốc
        private float baseMoveSpeed;

        // Index waypoint đang đi tới
        private int currentWaypointIndex;

        // Đã đến cuối đường chưa
        private bool hasReachedEnd;

        //Đã được khởi tạo chưa (tránh di chuyển khi chưa init)
        private bool isInitialized;

        // ============================
        // PROPERTIES
        // ============================
        public int CurrentWaypointIndex => currentWaypointIndex;
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
        public float ProgressRatio
        {
            get
            {
                if (!isInitialized || currentPath == null || currentPath.Length < 2) return 0f;
                if (hasReachedEnd) return 1f;

                float segmentProgress = 0f;
                if(currentWaypointIndex > 0)
                {
                    Vector2 prevWaypoint = currentPath.GetWaypoint(currentWaypointIndex - 1);
                    Vector2 nextWaypoint = currentPath.GetWaypoint(currentWaypointIndex);
                    float segmentLength = Vector2.Distance(prevWaypoint, nextWaypoint);

                    if (segmentProgress > 1.0f)
                    {
                        float distCovered = segmentLength - DistanceToNextWaypoint;
                        segmentProgress = distCovered / segmentLength;
                    }
                }
                float totalSegments = currentPath.Length - 1;
                float completedSegments = currentWaypointIndex - 1 + segmentProgress;

                return Mathf.Clamp01(completedSegments / totalSegments);
            }
        }

        public float CurrentSpeed => moveSpeed;
        public float BaseMoveSpeed => baseMoveSpeed;
        public bool HasReachedEnd => hasReachedEnd;

        // ============================
        // INIT
        // ============================

        public void Initialize(WaypointPath path, float speed)
        {
            if (path == null) return;
            if (path.Length < 2) return;

            currentPath = path;
            baseMoveSpeed = speed;
            moveSpeed = speed;

            transform.position = (Vector3)path.GetSpawnPoint();

            currentWaypointIndex = 1;

            hasReachedEnd = false;
            isInitialized = true;
        }

        public void OnEnable()
        {
            hasReachedEnd = false;
        }
        private void Update()
        {
            if (!isInitialized || hasReachedEnd) return;

            if (moveSpeed <= 0f) return;

            MoveTowardsNextWaypoint();
        }

        private void MoveTowardsNextWaypoint()
        {
            // Lấy vị trí waypoint đích
            Vector2 targetPos = currentPath.GetWaypoint(currentWaypointIndex);

            // Di chuyển
            Vector2 currentPos = transform.position;
            Vector2 newPos = Vector2.MoveTowards(currentPos, targetPos, moveSpeed * Time.deltaTime);

            transform.position = newPos;

            RotateTowardsMovement(currentPos, newPos);
            
            float distanceToTarget = Vector2.Distance(newPos, targetPos);

            if(distanceToTarget < 0.05f)
            {
                currentWaypointIndex++;
                if(currentWaypointIndex >= currentPath.Length)
                {
                    ReachEnd();
                }
            }
        }
        private void RotateTowardsMovement(Vector2 from, Vector2 to)
        {
            Vector2 direction = to - from;

            if (direction.sqrMagnitude < 0.0001f) return;

            if(direction.x != 0)
            {
                Vector3 scale = transform.localScale;
                scale.x = direction.x < 0 ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);

                transform.localScale = scale;
            }
        }
        private void ReachEnd()
        {
            if (hasReachedEnd) return;

            hasReachedEnd = true;

            GameEvents.RaiseEnemyReachedEnd(gameObject);

            gameObject.SetActive(false);
        }
        public void ModifySpeed(float multiplier)
        {
            moveSpeed = baseMoveSpeed * multiplier;
            if (moveSpeed < 0f) moveSpeed = 0f;
        }
        public void ResetSpeed()
        {
            moveSpeed = baseMoveSpeed;
        }

        private void OnDrawGizmosSelected()
        {
            if (currentPath == null || currentPath.Length < 2) return;

            // Vẽ toàn bộ path (xám)
            Gizmos.color = Color.gray;
            for (int i = 0; i < currentPath.Length - 1; i++)
            {
                Vector2 from = currentPath.GetWaypoint(i);
                Vector2 to = currentPath.GetWaypoint(i + 1);
                Gizmos.DrawLine((Vector3)from, (Vector3)to);
            }

            // Vẽ waypoint hiện tại (vàng)
            if (isInitialized && currentWaypointIndex < currentPath.Length)
            {
                Gizmos.color = Color.yellow;
                Vector2 currentTarget = currentPath.GetWaypoint(currentWaypointIndex);
                Gizmos.DrawWireSphere((Vector3)currentTarget, 0.2f);

                // Đường từ enemy đến waypoint hiện tại (xanh)
                Gizmos.color = Color.green;
                Gizmos.DrawLine(transform.position, (Vector3)currentTarget);
            }

            // Vẽ các waypoint (chấm tròn)
            for (int i = 0; i < currentPath.Length; i++)
            {
                // Waypoint đã qua = xanh dương, chưa qua = đỏ
                Gizmos.color = i < currentWaypointIndex ? Color.cyan : Color.red;
                Gizmos.DrawSphere((Vector3)currentPath.GetWaypoint(i), 0.15f);
            }
        }
    }
}
