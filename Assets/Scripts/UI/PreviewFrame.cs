using UnityEngine;
using BubblePuzzle.Core;
using BubblePuzzle.Grid;

namespace BubblePuzzle.UI
{
    /// <summary>
    /// Hexagonal preview frame at placement point
    /// </summary>
    public class PreviewFrame : MonoBehaviour
    {
        [Header("Visual Settings")]
        [SerializeField] private LineRenderer lineRenderer;
        [SerializeField] private float frameSize = 0.6f;
        [SerializeField] private Color frameColor = new Color(1f, 1f, 1f, 0.5f);
        [SerializeField] private float lineWidth = 0.05f;

        private Vector3 gridOffset;

        private void Awake()
        {
            if (lineRenderer == null)
            {
                lineRenderer = gameObject.AddComponent<LineRenderer>();
            }

            SetupLineRenderer();
            Hide();
        }

        public void SetGridOffset(Vector3 gridOffset)
        {
            this.gridOffset = gridOffset;
        }

        private void SetupLineRenderer()
        {
            lineRenderer.loop = true;
            lineRenderer.startWidth = lineWidth;
            lineRenderer.endWidth = lineWidth;
            lineRenderer.startColor = frameColor;
            lineRenderer.endColor = frameColor;
            lineRenderer.positionCount = 6;
            lineRenderer.sortingOrder = 15; // Draw on top of aim guide
            lineRenderer.useWorldSpace = false;

            // Create hexagon shape
            UpdateHexagonShape();
        }

        /// <summary>
        /// Update hexagon shape based on frame size (Pointy-Top)
        /// </summary>
        private void UpdateHexagonShape()
        {
            Vector3[] positions = new Vector3[6];

            // Pointy-top hexagon: start at 30 degrees (top-right vertex)
            for (int i = 0; i < 6; i++)
            {
                float angle = (60f * i + 30f) * Mathf.Deg2Rad;
                float x = Mathf.Cos(angle) * frameSize;
                float y = Mathf.Sin(angle) * frameSize;
                positions[i] = new Vector3(x, y, 0f);
            }

            lineRenderer.SetPositions(positions);
        }

        /// <summary>
        /// Show frame at hex coordinate
        /// </summary>
        public void ShowAtCoordinate(HexCoordinate coord, float hexSize)
        {
            Vector2 worldPos = coord.ToWorldPosition(hexSize);
            transform.position = new Vector3(worldPos.x, worldPos.y, 0f) + gridOffset;
            Show();
        }

        /// <summary>
        /// Show frame at world position
        /// </summary>
        public void ShowAtPosition(Vector2 worldPos)
        {
            transform.position = new Vector3(worldPos.x, worldPos.y, 0f);
            Show();
        }

        /// <summary>
        /// Show frame
        /// </summary>
        public void Show()
        {
            lineRenderer.enabled = true;
        }

        /// <summary>
        /// Hide frame
        /// </summary>
        public void Hide()
        {
            lineRenderer.enabled = false;
        }

        /// <summary>
        /// Set frame color
        /// </summary>
        public void SetColor(Color color)
        {
            frameColor = color;
            lineRenderer.startColor = color;
            lineRenderer.endColor = color;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (lineRenderer != null)
            {
                lineRenderer.startWidth = lineWidth;
                lineRenderer.endWidth = lineWidth;
                lineRenderer.startColor = frameColor;
                lineRenderer.endColor = frameColor;
                UpdateHexagonShape();
            }
        }
#endif
    }
}
