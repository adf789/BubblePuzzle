using System.Collections.Generic;
using UnityEngine;
using BubblePuzzle.Core;
using BubblePuzzle.Grid;
using BubblePuzzle.Bubble;

namespace BubblePuzzle.GameLogic
{
    /// <summary>
    /// Detect matching bubbles (BFS for 3+ same color) and bomb explosions
    /// </summary>
    public class MatchDetector : MonoBehaviour
    {
        /// <summary>
        /// Find all bubbles matching with the start bubble (BFS) or triggered by bombs
        /// Combines both bomb explosions and 3-match results
        /// </summary>
        public List<Bubble.Bubble> FindMatchingCluster(Bubble.Bubble startBubble, BubbleGrid grid)
        {
            Debug.Log("---------- MATCH DETECTION START ----------");

            if (startBubble == null || grid == null)
            {
                Debug.LogWarning("[MatchDetector] StartBubble or grid is null!");
                return new List<Bubble.Bubble>();
            }

            Debug.Log($"[MatchDetector] Starting from {startBubble.Coordinate}, ColorType: {startBubble.ColorType}, Type: {startBubble.Type}");

            HashSet<Bubble.Bubble> totalCluster = new HashSet<Bubble.Bubble>();

            // Check for bomb triggers near the placed bubble
            List<Bubble.Bubble> bombCluster = FindBombTargets(startBubble, grid);
            if (bombCluster.Count > 0)
            {
                Debug.Log($"[MatchDetector] ✓ BOMB TRIGGERED! {bombCluster.Count} bubbles affected");
                foreach (var bubble in bombCluster)
                {
                    totalCluster.Add(bubble);
                }
            }

            // Normal color matching (BFS for 3+ same color)
            Queue<Bubble.Bubble> queue = new Queue<Bubble.Bubble>();
            HashSet<Bubble.Bubble> visited = new HashSet<Bubble.Bubble>();
            List<Bubble.Bubble> colorCluster = new List<Bubble.Bubble>();

            queue.Enqueue(startBubble);
            visited.Add(startBubble);

            int iteration = 0;
            while (queue.Count > 0)
            {
                Bubble.Bubble current = queue.Dequeue();
                colorCluster.Add(current);
                iteration++;

                Debug.Log($"[MatchDetector] Iteration {iteration}: Processing {current.Coordinate}");

                // Check all 6 neighbors
                List<Bubble.Bubble> neighbors = grid.GetNeighbors(current.Coordinate);
                Debug.Log($"[MatchDetector]   Found {neighbors.Count} neighbors");

                foreach (Bubble.Bubble neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor) && neighbor.ColorType == startBubble.ColorType)
                    {
                        Debug.Log($"[MatchDetector]   -> Adding matching neighbor at {neighbor.Coordinate}");
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            Debug.Log($"[MatchDetector] BFS complete. Cluster size: {colorCluster.Count}, MinMatch: {IntDefine.MIN_MATCH_COUNT}");

            // Add color match cluster if it meets minimum count
            if (colorCluster.Count >= IntDefine.MIN_MATCH_COUNT)
            {
                Debug.Log($"[MatchDetector] ✓ COLOR MATCH FOUND! {colorCluster.Count} {startBubble.ColorType} bubbles");
                foreach (var bubble in colorCluster)
                {
                    totalCluster.Add(bubble);
                }
            }
            else
            {
                Debug.Log($"[MatchDetector] ✗ No color match (cluster size {colorCluster.Count} < {IntDefine.MIN_MATCH_COUNT})");
            }

            // Return combined results
            if (totalCluster.Count > 0)
            {
                Debug.Log($"[MatchDetector] ✓ TOTAL DESTRUCTION: {totalCluster.Count} bubbles (Bomb: {bombCluster.Count}, Color: {(colorCluster.Count >= IntDefine.MIN_MATCH_COUNT ? colorCluster.Count : 0)})");
                string coords = "";
                foreach (var b in totalCluster)
                    coords += $"{b.Coordinate} ";
                Debug.Log($"[MatchDetector] Total coordinates: {coords}");
                Debug.Log("---------- MATCH DETECTION END ----------");
                return new List<Bubble.Bubble>(totalCluster);
            }

            Debug.Log($"[MatchDetector] ✗ No matches found");
            Debug.Log("---------- MATCH DETECTION END ----------");
            return new List<Bubble.Bubble>();
        }

        /// <summary>
        /// Find bombs within 1 tile of the placed bubble and collect all affected bubbles
        /// </summary>
        private List<Bubble.Bubble> FindBombTargets(Bubble.Bubble placedBubble, BubbleGrid grid)
        {
            List<Bubble.Bubble> nearbyBombs = new List<Bubble.Bubble>();

            // Check all neighbors (1 tile radius) for bombs
            List<Bubble.Bubble> neighbors = grid.GetNeighbors(placedBubble.Coordinate);
            foreach (Bubble.Bubble neighbor in neighbors)
            {
                if (neighbor.Type == BubbleType.Bomb || neighbor.Type == BubbleType.LargeBomb)
                {
                    nearbyBombs.Add(neighbor);
                    Debug.Log($"[MatchDetector] Found {neighbor.Type} at {neighbor.Coordinate}");
                }
            }

            if (nearbyBombs.Count == 0)
            {
                return new List<Bubble.Bubble>();
            }

            // Collect all bubbles affected by chain explosions
            return CollectExplosionCluster(nearbyBombs, grid);
        }

        /// <summary>
        /// Recursively calculate all bubbles affected by bomb chain explosions
        /// </summary>
        private List<Bubble.Bubble> CollectExplosionCluster(List<Bubble.Bubble> initialBombs, BubbleGrid grid)
        {
            HashSet<Bubble.Bubble> affectedBubbles = new HashSet<Bubble.Bubble>();
            Queue<Bubble.Bubble> bombQueue = new Queue<Bubble.Bubble>();

            // Start with initial bombs
            foreach (Bubble.Bubble bomb in initialBombs)
            {
                bombQueue.Enqueue(bomb);
                affectedBubbles.Add(bomb);
            }

            // Process chain explosions
            while (bombQueue.Count > 0)
            {
                Bubble.Bubble currentBomb = bombQueue.Dequeue();
                int explosionRadius = currentBomb.Type == BubbleType.LargeBomb ? 2 : 1;

                Debug.Log($"[MatchDetector] Processing {currentBomb.Type} at {currentBomb.Coordinate} with radius {explosionRadius}");

                // Get all bubbles in explosion range
                List<Bubble.Bubble> explosionRange = GetExplosionRange(currentBomb.Coordinate, explosionRadius, grid);

                foreach (Bubble.Bubble bubble in explosionRange)
                {
                    if (!affectedBubbles.Contains(bubble))
                    {
                        affectedBubbles.Add(bubble);

                        // If this bubble is also a bomb, add it to the queue for chain explosion
                        if (bubble.Type == BubbleType.Bomb || bubble.Type == BubbleType.LargeBomb)
                        {
                            bombQueue.Enqueue(bubble);
                            Debug.Log($"[MatchDetector] Chain explosion: {bubble.Type} at {bubble.Coordinate}");
                        }
                    }
                }
            }

            Debug.Log($"[MatchDetector] Total explosion cluster: {affectedBubbles.Count} bubbles");
            return new List<Bubble.Bubble>(affectedBubbles);
        }

        /// <summary>
        /// Get all bubbles within N tiles of center coordinate using BFS
        /// </summary>
        private List<Bubble.Bubble> GetExplosionRange(HexCoordinate center, int radius, BubbleGrid grid)
        {
            List<Bubble.Bubble> result = new List<Bubble.Bubble>();
            HashSet<HexCoordinate> visited = new HashSet<HexCoordinate>();
            Queue<(HexCoordinate coord, int distance)> queue = new Queue<(HexCoordinate, int)>();

            queue.Enqueue((center, 0));
            visited.Add(center);

            while (queue.Count > 0)
            {
                var (currentCoord, currentDistance) = queue.Dequeue();

                // Get bubble at current coordinate
                Bubble.Bubble bubble = grid.GetBubble(currentCoord);
                if (bubble != null)
                {
                    result.Add(bubble);
                }

                // Expand to neighbors if within radius
                if (currentDistance < radius)
                {
                    List<Bubble.Bubble> neighbors = grid.GetNeighbors(currentCoord);
                    foreach (Bubble.Bubble neighbor in neighbors)
                    {
                        if (!visited.Contains(neighbor.Coordinate))
                        {
                            visited.Add(neighbor.Coordinate);
                            queue.Enqueue((neighbor.Coordinate, currentDistance + 1));
                        }
                    }
                }
            }

            return result;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Visualize matching clusters
        /// </summary>
        public void DrawMatchGizmos(List<Bubble.Bubble> cluster, Color color)
        {
            if (cluster == null || cluster.Count == 0)
                return;

            Gizmos.color = color;

            foreach (Bubble.Bubble bubble in cluster)
            {
                Gizmos.DrawWireSphere(bubble.transform.position, 0.4f);
            }
        }
#endif
    }
}
