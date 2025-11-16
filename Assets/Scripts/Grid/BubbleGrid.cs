using System.Collections.Generic;
using UnityEngine;
using BubblePuzzle.Core;
using BubblePuzzle.Bubble;
using System;

namespace BubblePuzzle.Grid
{
    /// <summary>
    /// Grid state management using Dictionary
    /// </summary>
    public class BubbleGrid : MonoBehaviour
    {
        public Vector2 GridOffset => gridOrigin;

        [SerializeField] private float hexSize = 0.5f;
        [SerializeField] private Vector2 gridOrigin = Vector2.zero;

        private readonly Dictionary<HexCoordinate, Bubble.Bubble> grid = new Dictionary<HexCoordinate, Bubble.Bubble>();

        public float HexSize => hexSize;

        void Awake()
        {
            gridOrigin += new Vector2(transform.position.x, transform.position.y);
        }

        /// <summary>
        /// Place bubble at coordinate
        /// </summary>
        public void PlaceBubble(HexCoordinate coord, Bubble.Bubble bubble)
        {
            if (bubble == null)
            {
                Debug.LogWarning("[BubbleGrid] Attempted to place null bubble!");
                return;
            }

            if (grid.ContainsKey(coord))
            {
                Debug.LogWarning($"[BubbleGrid] Overwriting bubble at {coord}! Old: {grid[coord].ColorType}, New: {bubble.ColorType}");
            }

            grid[coord] = bubble;
            bubble.Coordinate = coord;
            bubble.IsPlaced = true;
            bubble.transform.position = GetWorldPosition(coord);

            Debug.Log($"[BubbleGrid] Placed bubble at {coord}, Type: {bubble.ColorType}, Total bubbles: {grid.Count}");
        }

        /// <summary>
        /// Get bubble at coordinate
        /// </summary>
        public Bubble.Bubble GetBubble(HexCoordinate coord)
        {
            return grid.TryGetValue(coord, out Bubble.Bubble bubble) ? bubble : null;
        }

        /// <summary>
        /// Remove bubble from grid
        /// </summary>
        public void RemoveBubble(HexCoordinate coord)
        {
            if (grid.ContainsKey(coord))
            {
                var bubble = grid[coord];
                grid.Remove(coord);
                Debug.Log($"[BubbleGrid] Removed bubble at {coord}, Type: {bubble.ColorType}, Remaining bubbles: {grid.Count}");
            }
            else
            {
                Debug.LogWarning($"[BubbleGrid] Attempted to remove non-existent bubble at {coord}");
            }
        }

        /// <summary>
        /// Get all 6 neighbors of a coordinate
        /// </summary>
        public List<Bubble.Bubble> GetNeighbors(HexCoordinate coord)
        {
            List<Bubble.Bubble> neighbors = new List<Bubble.Bubble>();

            for (HexCoordinate.Direction dir = HexCoordinate.Direction.TopRight;
            Enum.IsDefined(typeof(HexCoordinate.Direction), dir);
            dir++)
            {
                HexCoordinate neighborCoord = coord.GetNeighbor(dir);
                Bubble.Bubble neighbor = GetBubble(neighborCoord);

                if (neighbor != null)
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        /// <summary>
        /// Get all bubbles in a specific row
        /// </summary>
        public List<Bubble.Bubble> GetBubblesInRow(int row)
        {
            List<Bubble.Bubble> bubbles = new List<Bubble.Bubble>();

            foreach (var kvp in grid)
            {
                if (kvp.Key.r == row)
                {
                    bubbles.Add(kvp.Value);
                }
            }

            return bubbles;
        }

        /// <summary>
        /// Get all bubbles in grid
        /// </summary>
        public List<Bubble.Bubble> GetAllBubbles()
        {
            return new List<Bubble.Bubble>(grid.Values);
        }

        /// <summary>
        /// Check if coordinate is occupied
        /// </summary>
        public bool IsOccupied(HexCoordinate coord)
        {
            return grid.ContainsKey(coord);
        }

        /// <summary>
        /// Clear all bubbles from grid
        /// </summary>
        public void ClearAll()
        {
            foreach (var bubble in grid.Values)
            {
                if (bubble != null)
                {
                    Destroy(bubble.gameObject);
                }
            }
            grid.Clear();
        }

        /// <summary>
        /// Convert hex coordinate to world position
        /// </summary>
        public Vector2 GetWorldPosition(HexCoordinate coord)
        {
            return gridOrigin + coord.ToWorldPosition(hexSize);
        }

        /// <summary>
        /// Convert world position to hex coordinate
        /// </summary>
        public HexCoordinate GetHexCoordinate(Vector2 worldPos)
        {
            Vector2 localPos = worldPos - gridOrigin;
            return HexCoordinate.FromWorldPosition(localPos, hexSize);
        }

        /// <summary>
        /// Get total bubble count
        /// </summary>
        public int GetBubbleCount()
        {
            return grid.Count;
        }

#if UNITY_EDITOR
        [SerializeField] private bool isDrawGrids = true;

        private void OnDrawGizmos()
        {
            if (!Application.isPlaying)
            {
                if (isDrawGrids)
                    OnDrawEditorGrid();
                return;
            }

            Gizmos.color = Color.yellow;

            // Draw grid positions and coordinates
            foreach (var kvp in grid)
            {
                Vector2 pos = GetWorldPosition(kvp.Key);
                DrawHexagon(pos, hexSize);

                // Draw coordinate text
                UnityEditor.Handles.Label(
                    new Vector3(pos.x, pos.y, 0f),
                    kvp.Key.ToString(),
                    new GUIStyle()
                    {
                        normal = new GUIStyleState() { textColor = Color.white },
                        fontSize = 10,
                        alignment = TextAnchor.MiddleCenter
                    }
                );
            }

            // Draw grid origin
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(gridOrigin, 0.2f);
        }

        private void OnDrawEditorGrid()
        {
            Gizmos.color = Color.yellow;

            // Draw grid positions and coordinates
            for (int q = -10; q <= 10; q++)
            {
                for (int r = -10; r <= 10; r++)
                {
                    HexCoordinate coord = new HexCoordinate(q, r);
                    Vector2 pos = GetWorldPosition(coord);
                    DrawHexagon(pos, hexSize);

                    // Draw coordinate text
                    UnityEditor.Handles.Label(
                        new Vector3(pos.x, pos.y, 0f),
                        coord.ToString(),
                        new GUIStyle()
                        {
                            normal = new GUIStyleState() { textColor = Color.white },
                            fontSize = 10,
                            alignment = TextAnchor.MiddleCenter
                        }
                    );
                }
            }
        }

        private void DrawHexagon(Vector2 center, float size)
        {
            // Pointy-top hexagon: start at 30 degrees
            for (int i = 0; i < 6; i++)
            {
                float angle1 = (60f * i + 30f) * Mathf.Deg2Rad;
                float angle2 = (60f * (i + 1) + 30f) * Mathf.Deg2Rad;

                Vector2 p1 = center + new Vector2(Mathf.Cos(angle1), Mathf.Sin(angle1)) * size;
                Vector2 p2 = center + new Vector2(Mathf.Cos(angle2), Mathf.Sin(angle2)) * size;

                Gizmos.DrawLine(p1, p2);
            }
        }
#endif
    }
}
