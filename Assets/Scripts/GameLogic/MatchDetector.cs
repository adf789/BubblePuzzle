using System.Collections.Generic;
using UnityEngine;
using BubblePuzzle.Core;
using BubblePuzzle.Grid;
using BubblePuzzle.Bubble;
using System.Linq;

namespace BubblePuzzle.GameLogic
{
    public class MatchDetector : MonoBehaviour
    {
        /// <summary>
        /// Find all bubbles matching with the start bubble (BFS) or triggered by bombs
        /// Combines both bomb explosions and 3-match results
        /// </summary>
        public List<Bubble.Bubble> FindMatchingCluster(Bubble.Bubble startBubble, BubbleGrid grid)
        {
            if (startBubble == null || grid == null)
            {
                Debug.LogWarning("[MatchDetector] StartBubble or grid is null!");
                return null;
            }

            // Check for bomb triggers near the placed bubble
            List<Bubble.Bubble> totalCluster = FindBombTargets(startBubble, grid);

            // Normal color matching
            Queue<Bubble.Bubble> queue = new Queue<Bubble.Bubble>();
            HashSet<HexCoordinate> checkCoords = new HashSet<HexCoordinate>();
            List<Bubble.Bubble> colorCluster = null;

            queue.Enqueue(startBubble);
            checkCoords.Add(startBubble.Coordinate);

            int depth = 1;
            while (queue.Count > 0)
            {
                Bubble.Bubble current = queue.Dequeue();

                if (colorCluster == null)
                    colorCluster = new List<Bubble.Bubble>() { current };
                else
                    colorCluster.Add(current);

                // Check all 6 neighbors
                var neighbors = grid.GetNeighborsWithSelf(current.Coordinate, depth, checkCoords);
                foreach (Bubble.Bubble neighbor in neighbors)
                {
                    if (neighbor.IsBomb)
                        continue;

                    if (neighbor.ColorType == startBubble.ColorType)
                        queue.Enqueue(neighbor);
                }
            }

            // Add color match cluster if it meets minimum count
            if (colorCluster != null && colorCluster.Count >= IntDefine.MIN_MATCH_COUNT)
            {
                foreach (var bubble in colorCluster)
                {
                    totalCluster.Add(bubble);
                }
            }

            // Return combined results
            return totalCluster;
        }

        /// <summary>
        /// Find bombs within 1 tile of the placed bubble and collect all affected bubbles
        /// </summary>
        private List<Bubble.Bubble> FindBombTargets(Bubble.Bubble placedBubble, BubbleGrid grid)
        {
            // Check if the placed bubble itself is a bomb
            int depth = placedBubble.Type switch
            {
                BubbleType.Bomb => 1,
                BubbleType.LargeBomb => 2,
                _ => 1,
            };

            Debug.Log($"[MatchDetector] Placed bubble is {placedBubble.Type} at {placedBubble.Coordinate}");

            // Check all neighbors for bombs
            var affectBubbles = grid.GetNeighbors(placedBubble.Coordinate, depth).ToList();

            // Add self
            affectBubbles.Add(placedBubble);

            // Collect all bubbles affected by chain explosions
            if (!CollectExplosionCluster(ref affectBubbles, grid))
            {
                // If not bomb range, reset affect bubble list
                affectBubbles.Clear();
            }

            return affectBubbles;
        }

        /// <summary>
        /// Recursively calculate all bubbles affected by bomb chain explosions
        /// </summary>
        private bool CollectExplosionCluster(ref List<Bubble.Bubble> initialBubbles, BubbleGrid grid)
        {
            HashSet<HexCoordinate> checkBubbles = new HashSet<HexCoordinate>();
            Queue<Bubble.Bubble> bombQueue = new Queue<Bubble.Bubble>();
            bool isFindBomb = false;

            // Start with initial bombs
            foreach (Bubble.Bubble bubble in initialBubbles)
            {
                if (bubble.IsBomb)
                {
                    bombQueue.Enqueue(bubble);
                    isFindBomb = true;
                }
            }

            // Reset affected bubbles
            initialBubbles.Clear();

            // Process chain explosions
            while (bombQueue.Count > 0)
            {
                Bubble.Bubble currentBomb = bombQueue.Dequeue();
                int depth = currentBomb.Type == BubbleType.LargeBomb ? 2 : 1;

                Debug.Log($"[MatchDetector] Processing {currentBomb.Type} at {currentBomb.Coordinate} with radius {depth}");

                // Get all bubbles in explosion range
                var explosionRange = grid.GetNeighborsWithSelf(currentBomb.Coordinate, depth, checkBubbles);

                foreach (Bubble.Bubble bubble in explosionRange)
                {
                    // Add affected bubble
                    initialBubbles.Add(bubble);

                    // If this bubble is also a bomb, add it to the queue for chain explosion
                    if (!bubble.IsBomb)
                        continue;

                    bombQueue.Enqueue(bubble);
                    Debug.Log($"[MatchDetector] Chain explosion: {bubble.Type} at {bubble.Coordinate}");
                }
            }

            Debug.Log($"[MatchDetector] Total explosion cluster: {initialBubbles.Count} bubbles");

            return isFindBomb;
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
