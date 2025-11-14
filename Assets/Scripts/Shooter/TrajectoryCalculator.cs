using UnityEngine;
using System.Collections.Generic;

namespace BubblePuzzle.Shooter
{
    /// <summary>
    /// Calculate trajectory with wall reflection (max 1 bounce)
    /// </summary>
    public class TrajectoryCalculator : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float maxDistance = 20f;
        [SerializeField] private LayerMask wallLayer;
        [SerializeField] private LayerMask bubbleLayer;

        public struct TrajectoryResult
        {
            public Vector2[] points;
            public bool hasReflection;
            public Vector2 finalPosition;
            public bool hitBubble;
            public RaycastHit2D hitInfo;
        }

        /// <summary>
        /// Calculate trajectory with max 1 reflection
        /// </summary>
        public TrajectoryResult CalculateTrajectory(Vector2 origin, Vector2 direction)
        {
            TrajectoryResult result = new TrajectoryResult
            {
                hasReflection = false,
                hitBubble = false
            };

            List<Vector2> points = new List<Vector2>();
            points.Add(origin);

            // First raycast - check for wall or bubble
            RaycastHit2D hit = Physics2D.Raycast(origin, direction, maxDistance, wallLayer | bubbleLayer);

            if (hit.collider != null)
            {
                if (hit.collider.gameObject.layer == LayerMask.NameToLayer("Wall"))
                {
                    // Hit wall - calculate reflection
                    points.Add(hit.point);
                    result.hasReflection = true;

                    // Reflect direction (angle of incidence = angle of reflection)
                    Vector2 reflectedDir = Vector2.Reflect(direction, hit.normal).normalized;

                    // Second raycast from reflection point (NO MORE REFLECTIONS)
                    RaycastHit2D secondHit = Physics2D.Raycast(
                        hit.point + reflectedDir * 0.01f, // Small offset to avoid self-collision
                        reflectedDir,
                        maxDistance,
                        wallLayer | bubbleLayer
                    );

                    if (secondHit.collider != null)
                    {
                        points.Add(secondHit.point);
                        result.finalPosition = secondHit.point;
                        result.hitBubble = secondHit.collider.gameObject.layer == LayerMask.NameToLayer("Bubble");
                        result.hitInfo = secondHit;
                    }
                    else
                    {
                        // No second hit, extend to max distance
                        Vector2 endPoint = hit.point + reflectedDir * maxDistance;
                        points.Add(endPoint);
                        result.finalPosition = endPoint;
                    }
                }
                else
                {
                    // Hit bubble directly (no reflection)
                    points.Add(hit.point);
                    result.finalPosition = hit.point;
                    result.hitBubble = true;
                    result.hitInfo = hit;
                }
            }
            else
            {
                // No hit - extend to max distance
                Vector2 endPoint = origin + direction * maxDistance;
                points.Add(endPoint);
                result.finalPosition = endPoint;
            }

            result.points = points.ToArray();
            return result;
        }

        /// <summary>
        /// Visualize trajectory in Scene view (for debugging)
        /// </summary>
        public void DrawTrajectoryGizmos(TrajectoryResult result, Color color)
        {
            if (result.points == null || result.points.Length < 2)
                return;

            Gizmos.color = color;

            for (int i = 0; i < result.points.Length - 1; i++)
            {
                Gizmos.DrawLine(result.points[i], result.points[i + 1]);
            }

            // Draw final position
            Gizmos.color = result.hitBubble ? Color.green : Color.red;
            Gizmos.DrawWireSphere(result.finalPosition, 0.2f);

            // Draw reflection point if exists
            if (result.hasReflection && result.points.Length > 1)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawWireSphere(result.points[1], 0.15f);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            // Auto-assign layers if not set
            if (wallLayer == 0)
            {
                wallLayer = LayerMask.GetMask("Wall");
            }
            if (bubbleLayer == 0)
            {
                bubbleLayer = LayerMask.GetMask("Bubble");
            }
        }
#endif
    }
}
