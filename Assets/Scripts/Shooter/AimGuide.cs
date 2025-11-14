using UnityEngine;

namespace BubblePuzzle.Shooter
{
    /// <summary>
    /// Visual aim guide using LineRenderer
    /// </summary>
    [RequireComponent(typeof(LineRenderer))]
    public class AimGuide : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float lineWidth = 0.1f;
        [SerializeField] private Color normalColor = Color.white;
        [SerializeField] private Color reflectionColor = Color.yellow;
        [SerializeField] private Material lineMaterial;

        private void Awake()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            SetupLineRenderer();
        }

        private void SetupLineRenderer()
        {
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.material = lineMaterial;
            lineRenderer.startColor = normalColor;
            lineRenderer.endColor = normalColor;
            lineRenderer.positionCount = 0;
            lineRenderer.sortingOrder = 10; // Draw on top
            lineRenderer.useWorldSpace = true;
            lineRenderer.loop = false;

            // Hide initially
            Hide();
        }

        /// <summary>
        /// Update guide line with trajectory points
        /// </summary>
        public void UpdateTrajectory(in TrajectoryCalculator.TrajectoryResult trajectory)
        {
            if (trajectory.points == null || trajectory.points.Length < 2)
            {
                Hide();
                return;
            }

            lineRenderer.positionCount = trajectory.points.Length;

            for (int i = 0; i < trajectory.points.Length; i++)
            {
                lineRenderer.SetPosition(i, trajectory.points[i]);
            }

            // Change color if reflection occurred
            if (trajectory.hasReflection)
            {
                lineRenderer.startColor = reflectionColor;
                lineRenderer.endColor = reflectionColor;
            }
            else
            {
                lineRenderer.startColor = normalColor;
                lineRenderer.endColor = normalColor;
            }

            Show();
        }

        /// <summary>
        /// Show guide line
        /// </summary>
        public void Show()
        {
            lineRenderer.enabled = true;
        }

        /// <summary>
        /// Hide guide line
        /// </summary>
        public void Hide()
        {
            lineRenderer.enabled = false;
        }

        /// <summary>
        /// Clear guide line
        /// </summary>
        public void Clear()
        {
            lineRenderer.positionCount = 0;
            Hide();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (lineRenderer == null)
            {
                lineRenderer = GetComponent<LineRenderer>();
            }

            if (lineRenderer != null)
            {
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
                lineRenderer.startColor = normalColor;
                lineRenderer.endColor = normalColor;
            }
        }
#endif
    }
}
