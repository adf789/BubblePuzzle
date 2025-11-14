using System.Collections.Generic;
using UnityEngine;
using BubblePuzzle.Grid;
using BubblePuzzle.Bubble;

namespace BubblePuzzle.GameLogic
{
    /// <summary>
    /// Check for disconnected bubbles (DFS from top row)
    /// </summary>
    public class GravityChecker : MonoBehaviour
    {
        /// <summary>
        /// Get all bubbles not connected to top (will fall)
        /// </summary>
        public List<Bubble.Bubble> GetDisconnectedBubbles(BubbleGrid grid)
        {
            Debug.Log("---------- GRAVITY CHECK START ----------");

            if (grid == null)
            {
                Debug.LogWarning("[GravityChecker] Grid is null!");
                return new List<Bubble.Bubble>();
            }

            // Step 1: Find all bubbles connected to top row (DFS)
            HashSet<Bubble.Bubble> connectedToTop = new HashSet<Bubble.Bubble>();

            // Start DFS from all bubbles in top row (r = 0)
            List<Bubble.Bubble> topRowBubbles = grid.GetBubblesInRow(0);
            Debug.Log($"[GravityChecker] Found {topRowBubbles.Count} bubbles in top row (r=0)");

            foreach (Bubble.Bubble topBubble in topRowBubbles)
            {
                Debug.Log($"[GravityChecker] Starting DFS from top bubble at {topBubble.Coordinate}");
                if (!connectedToTop.Contains(topBubble))
                {
                    int beforeCount = connectedToTop.Count;
                    DFS(topBubble, connectedToTop, grid);
                    int afterCount = connectedToTop.Count;
                    Debug.Log($"[GravityChecker] DFS found {afterCount - beforeCount} connected bubbles");
                }
            }

            Debug.Log($"[GravityChecker] Total connected to top: {connectedToTop.Count} bubbles");

            // Step 2: All bubbles NOT in connectedToTop set will fall
            List<Bubble.Bubble> allBubbles = grid.GetAllBubbles();
            List<Bubble.Bubble> fallingBubbles = new List<Bubble.Bubble>();

            Debug.Log($"[GravityChecker] Checking {allBubbles.Count} total bubbles for disconnection");

            foreach (Bubble.Bubble bubble in allBubbles)
            {
                if (!connectedToTop.Contains(bubble))
                {
                    Debug.Log($"[GravityChecker] -> Disconnected bubble at {bubble.Coordinate}, Type: {bubble.Type}");
                    fallingBubbles.Add(bubble);
                }
            }

            if (fallingBubbles.Count > 0)
            {
                Debug.Log($"[GravityChecker] ✓ {fallingBubbles.Count} bubbles will fall");
                string coords = "";
                foreach (var b in fallingBubbles)
                    coords += $"{b.Coordinate} ";
                Debug.Log($"[GravityChecker] Falling coordinates: {coords}");
            }
            else
            {
                Debug.Log("[GravityChecker] ✓ All bubbles connected to top");
            }

            Debug.Log("---------- GRAVITY CHECK END ----------");
            return fallingBubbles;
        }

        /// <summary>
        /// Depth-First Search to find all connected bubbles
        /// </summary>
        private void DFS(Bubble.Bubble current, HashSet<Bubble.Bubble> visited, BubbleGrid grid)
        {
            if (current == null || visited.Contains(current))
                return;

            visited.Add(current);

            // Visit all 6 neighbors
            List<Bubble.Bubble> neighbors = grid.GetNeighbors(current.Coordinate);

            foreach (Bubble.Bubble neighbor in neighbors)
            {
                DFS(neighbor, visited, grid);
            }
        }

        /// <summary>
        /// Check if specific bubble is connected to top
        /// </summary>
        public bool IsConnectedToTop(Bubble.Bubble bubble, BubbleGrid grid)
        {
            if (bubble == null || grid == null)
                return false;

            HashSet<Bubble.Bubble> connectedToTop = new HashSet<Bubble.Bubble>();
            List<Bubble.Bubble> topRowBubbles = grid.GetBubblesInRow(0);

            foreach (Bubble.Bubble topBubble in topRowBubbles)
            {
                DFS(topBubble, connectedToTop, grid);
            }

            return connectedToTop.Contains(bubble);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Visualize disconnected bubbles
        /// </summary>
        public void DrawDisconnectedGizmos(List<Bubble.Bubble> disconnected, Color color)
        {
            if (disconnected == null || disconnected.Count == 0)
                return;

            Gizmos.color = color;

            foreach (Bubble.Bubble bubble in disconnected)
            {
                Gizmos.DrawWireCube(bubble.transform.position, Vector3.one * 0.5f);
            }
        }
#endif
    }
}
