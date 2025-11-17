using UnityEngine;
using BubblePuzzle.Core;
using BubblePuzzle.Grid;
using BubblePuzzle.Bubble;
using System;
using System.Collections.Generic;

namespace BubblePuzzle.UI
{
    /// <summary>
    /// Hexagonal preview frame at placement point
    /// </summary>
    public class PreviewFrame : MonoBehaviour
    {
        [SerializeField] private GameObject previewTarget;
        [SerializeField] private Transform bombRendererParent;
        [SerializeField] private Transform largeBombRendererParent;

        private BubbleGrid bubbleGrid;
        private BubbleType previewType;
        private bool isInitialized = false;

        private void Awake()
        {
            Hide();
        }

        private void Start()
        {
            // Pre-generate all renderers on start to avoid runtime lag
            InitializeRenderers();
        }

        public void SetGrid(BubbleGrid bubbleGrid)
        {
            this.bubbleGrid = bubbleGrid;
        }

        /// <summary>
        /// Show frame at hex coordinate
        /// </summary>
        public void ShowAtCoordinate(HexCoordinate coord, float hexSize)
        {
            if (!bubbleGrid)
            {
                Debug.LogError("[PreviewFrame]: Not found grid");
                return;
            }

            Vector2 worldPos = coord.ToWorldPosition(hexSize);

            worldPos.x += bubbleGrid.GridOffset.x;
            worldPos.y += bubbleGrid.GridOffset.y;
            transform.position = new Vector3(worldPos.x, worldPos.y, 0f);

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
            previewTarget.SetActive(true);

            switch (previewType)
            {
                case BubbleType.Bomb:
                    bombRendererParent.gameObject.SetActive(true);
                    break;

                case BubbleType.LargeBomb:
                    bombRendererParent.gameObject.SetActive(true);
                    largeBombRendererParent.gameObject.SetActive(true);
                    break;
            }
        }

        /// <summary>
        /// Hide frame
        /// </summary>
        public void Hide()
        {
            previewTarget.SetActive(false);
            bombRendererParent.gameObject.SetActive(false);
            largeBombRendererParent.gameObject.SetActive(false);
        }

        /// <summary>
        /// Set preview type
        /// </summary>
        public void SetPreviewType(BubbleType type)
        {
            previewType = type;

            // Ensure renderers are initialized
            if (!isInitialized)
            {
                InitializeRenderers();
            }
        }

        /// <summary>
        /// Pre-generate all renderers once to avoid runtime lag
        /// </summary>
        private void InitializeRenderers()
        {
            if (isInitialized)
                return;

            CreateBombRange();
            CreateLargeBombRange();

            isInitialized = true;
        }

        private void CreateBombRange()
        {
            if (bombRendererParent.childCount > 0)
                return;

            // Create renderers at 1-tile distance (6 neighbors)
            CreateRenderersAtDistance(1, bombRendererParent, includeDistance: true);
        }

        private void CreateLargeBombRange()
        {
            if (largeBombRendererParent.childCount > 0)
                return;

            // Create renderers at 2-tile distance, excluding 1-tile distance
            CreateRenderersAtDistance(2, largeBombRendererParent, includeDistance: false);
        }

        /// <summary>
        /// Create renderer objects at specified hex grid distance
        /// </summary>
        /// <param name="distance">Hex grid distance from center (0,0,0)</param>
        /// <param name="parent">Parent transform for instantiated objects</param>
        /// <param name="includeDistance">If false, excludes (distance-1) tiles (for ring-only rendering)</param>
        private void CreateRenderersAtDistance(int distance, Transform parent, bool includeDistance)
        {
            if (previewTarget == null || parent == null)
            {
                Debug.LogError("[PreviewFrame] targetRenderer or parent is null!");
                return;
            }

            // Get hex size from grid (default fallback)
            float hexSize = bubbleGrid != null ? bubbleGrid.HexSize : 0.6f;

            // BFS to find all tiles at specified distance
            HexCoordinate center = new HexCoordinate(0, 0);
            HashSet<HexCoordinate> visited = new HashSet<HexCoordinate>();
            Queue<(HexCoordinate coord, int dist)> queue = new Queue<(HexCoordinate, int)>();

            queue.Enqueue((center, 0));
            visited.Add(center);

            while (queue.Count > 0)
            {
                var (currentCoord, currentDist) = queue.Dequeue();

                // Create renderer at this position if it matches distance criteria
                bool shouldCreate = false;
                if (includeDistance)
                {
                    // Include all tiles up to and including target distance
                    shouldCreate = currentDist > 0 && currentDist <= distance;
                }
                else
                {
                    // Only include tiles exactly at target distance (ring only)
                    shouldCreate = currentDist == distance;
                }

                if (shouldCreate)
                {
                    Vector2 worldPos = currentCoord.ToWorldPosition(hexSize);
                    GameObject rendererObj = Instantiate(previewTarget, parent);
                    rendererObj.transform.localPosition = new Vector3(worldPos.x, worldPos.y, 0f);
                    rendererObj.SetActive(true);
                }

                // Expand to neighbors if within distance
                if (currentDist < distance)
                {
                    // Get all 6 hex neighbors
                    HexCoordinate[] neighbors = new HexCoordinate[]
                    {
                        new HexCoordinate(currentCoord.q + 1, currentCoord.r),     // Right
                        new HexCoordinate(currentCoord.q + 1, currentCoord.r - 1), // TopRight
                        new HexCoordinate(currentCoord.q, currentCoord.r - 1),     // TopLeft
                        new HexCoordinate(currentCoord.q - 1, currentCoord.r),     // Left
                        new HexCoordinate(currentCoord.q - 1, currentCoord.r + 1), // BottomLeft
                        new HexCoordinate(currentCoord.q, currentCoord.r + 1)      // BottomRight
                    };

                    foreach (HexCoordinate neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor))
                        {
                            visited.Add(neighbor);
                            queue.Enqueue((neighbor, currentDist + 1));
                        }
                    }
                }
            }
        }
    }
}
