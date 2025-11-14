using System.Collections.Generic;
using UnityEngine;
using BubblePuzzle.Core;
using BubblePuzzle.Grid;
using BubblePuzzle.Bubble;

namespace BubblePuzzle.GameLogic
{
    /// <summary>
    /// Detect matching bubbles (BFS for 3+ same color)
    /// </summary>
    public class MatchDetector : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private int minMatchCount = 3;

        /// <summary>
        /// Find all bubbles matching with the start bubble (BFS)
        /// </summary>
        public List<Bubble.Bubble> FindMatchingCluster(Bubble.Bubble startBubble, BubbleGrid grid)
        {
            Debug.Log("---------- MATCH DETECTION START ----------");

            if (startBubble == null || grid == null)
            {
                Debug.LogWarning("[MatchDetector] StartBubble or grid is null!");
                return new List<Bubble.Bubble>();
            }

            Debug.Log($"[MatchDetector] Starting BFS from {startBubble.Coordinate}, Type: {startBubble.Type}");

            Queue<Bubble.Bubble> queue = new Queue<Bubble.Bubble>();
            HashSet<Bubble.Bubble> visited = new HashSet<Bubble.Bubble>();
            List<Bubble.Bubble> cluster = new List<Bubble.Bubble>();

            queue.Enqueue(startBubble);
            visited.Add(startBubble);

            int iteration = 0;
            while (queue.Count > 0)
            {
                Bubble.Bubble current = queue.Dequeue();
                cluster.Add(current);
                iteration++;

                Debug.Log($"[MatchDetector] Iteration {iteration}: Processing {current.Coordinate}");

                // Check all 6 neighbors
                List<Bubble.Bubble> neighbors = grid.GetNeighbors(current.Coordinate);
                Debug.Log($"[MatchDetector]   Found {neighbors.Count} neighbors");

                foreach (Bubble.Bubble neighbor in neighbors)
                {
                    if (!visited.Contains(neighbor) && neighbor.Type == startBubble.Type)
                    {
                        Debug.Log($"[MatchDetector]   -> Adding matching neighbor at {neighbor.Coordinate}");
                        visited.Add(neighbor);
                        queue.Enqueue(neighbor);
                    }
                }
            }

            Debug.Log($"[MatchDetector] BFS complete. Cluster size: {cluster.Count}, MinMatch: {minMatchCount}");

            // Return cluster only if matches minimum count
            if (cluster.Count >= minMatchCount)
            {
                Debug.Log($"[MatchDetector] ✓ MATCH FOUND! {cluster.Count} {startBubble.Type} bubbles");
                string coords = "";
                foreach (var b in cluster)
                    coords += $"{b.Coordinate} ";
                Debug.Log($"[MatchDetector] Match coordinates: {coords}");
                Debug.Log("---------- MATCH DETECTION END ----------");
                return cluster;
            }

            Debug.Log($"[MatchDetector] ✗ No match (cluster size {cluster.Count} < {minMatchCount})");
            Debug.Log("---------- MATCH DETECTION END ----------");
            return new List<Bubble.Bubble>();
        }

        /// <summary>
        /// Check if bubble would create a match at coordinate
        /// </summary>
        public bool WouldCreateMatch(HexCoordinate coord, BubbleType type, BubbleGrid grid)
        {
            // Temporarily place bubble
            GameObject tempObj = new GameObject("TempBubble");
            Bubble.Bubble tempBubble = tempObj.AddComponent<Bubble.Bubble>();
            tempBubble.Initialize(type, coord);

            grid.PlaceBubble(coord, tempBubble);

            // Check for match
            List<Bubble.Bubble> matches = FindMatchingCluster(tempBubble, grid);
            bool hasMatch = matches.Count > 0;

            // Remove temporary bubble
            grid.RemoveBubble(coord);
            Destroy(tempObj);

            return hasMatch;
        }

        /// <summary>
        /// Find all matching clusters in grid
        /// </summary>
        public List<List<Bubble.Bubble>> FindAllMatchingClusters(BubbleGrid grid)
        {
            List<List<Bubble.Bubble>> clusters = new List<List<Bubble.Bubble>>();
            HashSet<Bubble.Bubble> processed = new HashSet<Bubble.Bubble>();

            List<Bubble.Bubble> allBubbles = grid.GetAllBubbles();

            foreach (Bubble.Bubble bubble in allBubbles)
            {
                if (processed.Contains(bubble))
                    continue;

                List<Bubble.Bubble> cluster = FindMatchingCluster(bubble, grid);

                if (cluster.Count > 0)
                {
                    clusters.Add(cluster);
                    foreach (Bubble.Bubble b in cluster)
                    {
                        processed.Add(b);
                    }
                }
            }

            return clusters;
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
