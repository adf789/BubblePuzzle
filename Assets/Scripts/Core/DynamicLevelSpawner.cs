using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BubblePuzzle.Grid;
using BubblePuzzle.Bubble;

namespace BubblePuzzle.Core
{
    /// <summary>
    /// Dynamic level spawner with data-driven path generation
    /// Spawns bubbles from center in both directions simultaneously using predefined paths
    /// </summary>
    public class DynamicLevelSpawner : MonoBehaviour
    {
        [Header("Path Data")]
        [SerializeField] private LevelPathData levelPathData;

        // Precomputed coordinates from path data
        private IReadOnlyList<HexCoordinate>[] coordinates = null;

        // Bubble chains for animation
        private LinkedList<Bubble.Bubble>[] bubbleChains = null;
        private BubbleGrid bubbleGrid = null;

        private bool isSpawning = false;

        public void SetBubbleGrid(BubbleGrid bubbleGrid)
        {
            this.bubbleGrid = bubbleGrid;
        }

        /// <summary>
        /// Start dynamic level generation
        /// </summary>
        public void StartGeneration()
        {
            if (isSpawning)
            {
                Debug.LogWarning("[DynamicLevelSpawner] Already spawning!");
                return;
            }

            if (levelPathData == null)
            {
                Debug.LogError("[DynamicLevelSpawner] LevelPathData is null! Assign a LevelPathData asset.");
                return;
            }

            if (!levelPathData.ValidateAllPaths())
            {
                Debug.LogError("[DynamicLevelSpawner] LevelPathData validation failed!");
                return;
            }

            Debug.Log("========== DYNAMIC LEVEL GENERATION START ==========");
            Debug.Log($"[DynamicLevelSpawner] Using path data: {levelPathData.name}");
            Debug.Log($"[DynamicLevelSpawner] Paths: {levelPathData.GetPathCount()}, MoveSpeed: {levelPathData.MoveSpeed}s");

            // Clear existing bubbles
            bubbleGrid?.ClearAll();

            // Load coordinates from path data
            int pathCount = levelPathData.SpawnPaths.Length;
            coordinates = new IReadOnlyList<HexCoordinate>[pathCount];
            bubbleChains = new LinkedList<Bubble.Bubble>[pathCount];

            for (int i = 0; i < pathCount; i++)
            {
                coordinates[i] = levelPathData.GetCoordinatesForPath(i);
                bubbleChains[i] = new LinkedList<Bubble.Bubble>();
            }

            // Start generation coroutine
            StartCoroutine(GenerationCoroutine());
        }

        /// <summary>
        /// Check if all required bubbles exist in grid and regenerate only missing segments
        /// Generates bubbles only for continuous empty segments along each path
        /// </summary>
        public void RegenerateIfNeeded()
        {
            if (levelPathData == null)
            {
                Debug.LogError("[DynamicLevelSpawner] LevelPathData is null!");
                return;
            }

            if (isSpawning)
            {
                Debug.LogWarning("[DynamicLevelSpawner] Already spawning, cannot regenerate!");
                return;
            }

            var pathIndices = GetNeedRegeneratePathIndices();

            // Start partial regeneration
            Debug.Log("[DynamicLevelSpawner] Starting partial regeneration for missing segments...");
            for (int i = 0; pathIndices != null && i < pathIndices.Count; i++)
            {
                StartCoroutine(RegenerateMissingSegments(pathIndices[i]));
            }
        }

        private List<int> GetNeedRegeneratePathIndices()
        {
            Debug.Log("========== CHECKING MISSING BUBBLES ==========");

            // Check each path for missing segments
            List<int> indices = null;

            for (int pathIndex = 0; pathIndex < levelPathData.SpawnPaths.Length; pathIndex++)
            {
                var pathCoords = levelPathData.GetCoordinatesForPath(pathIndex);

                // Find first missing bubble in path
                int firstMissingIndex = -1;
                for (int i = 0; i < pathCoords.Count; i++)
                {
                    if (!bubbleGrid.IsOccupied(pathCoords[i]))
                    {
                        firstMissingIndex = i;
                        break;
                    }
                }

                // If no missing bubbles in this path, continue to next path
                if (firstMissingIndex == -1)
                {
                    Debug.Log($"[DynamicLevelSpawner] Path {pathIndex}: All bubbles exist, skipping");
                    continue;
                }

                if (indices == null)
                    indices = new List<int>() { pathIndex };
                else
                    indices.Add(pathIndex);
            }

            if (indices == null)
            {
                Debug.Log("[DynamicLevelSpawner] All required bubbles exist in grid. No regeneration needed.");
            }

            return indices;
        }

        /// <summary>
        /// Regenerate only missing continuous segments for each path
        /// </summary>
        private IEnumerator RegenerateMissingSegments(int pathIndex)
        {
            if (!levelPathData.CheckHasPath(pathIndex))
                yield break;

            // Process each path independently
            var pathCoords = levelPathData.GetCoordinatesForPath(pathIndex);

            // Find missing bubbles
            int firstMissingIndex = -1;
            int lastMissingIndex = -1;
            bool findMissing = false;
            for (int i = 0; i < pathCoords.Count; i++)
            {
                // Find missing bubble
                if (!findMissing)
                {
                    if (!bubbleGrid.IsOccupied(pathCoords[i]))
                    {
                        firstMissingIndex = i;
                        lastMissingIndex = i;
                        findMissing = true;
                    }
                }
                else
                {
                    // After find missing bubble, find the bubble
                    if (bubbleGrid.IsOccupied(pathCoords[i]))
                        break;

                    lastMissingIndex = i;
                }
            }

            // Not find missing bubble
            if (!findMissing) yield break;

            // Generate missing segment using chain animation
            Debug.Log($"[DynamicLevelSpawner] Generating path {pathIndex} segment [{firstMissingIndex}~{lastMissingIndex}]");

            // Build chain: collect existing bubbles from start to firstMissingIndex-1
            List<Bubble.Bubble> chain = new();
            List<int> existingBubbleIndices = new(); // Track which positions had bubbles

            for (int i = 0; i < firstMissingIndex; i++)
            {
                Bubble.Bubble existingBubble = bubbleGrid.GetBubble(pathCoords[i]);
                if (existingBubble != null)
                {
                    chain.Add(existingBubble);
                    existingBubbleIndices.Add(i);
                    // Remove from grid temporarily for animation
                    bubbleGrid.RemoveBubble(pathCoords[i]);
                }
            }

            Debug.Log($"[DynamicLevelSpawner] Collected {chain.Count} existing bubbles for chain push");

            int segmentLength = lastMissingIndex - firstMissingIndex + 1;

            for (int phase = 0; phase < segmentLength; phase++)
            {
                // Step 1: Move all bubbles in chain (existing + newly created) forward
                if (chain.Count > 0)
                {
                    yield return AnimateSegmentChain(chain, pathCoords, 0);
                }

                // Step 2: Create new bubble at path start position (index 0)
                HexCoordinate startCoord = pathCoords[0];
                Vector2 startWorldPos = bubbleGrid.GetWorldPosition(startCoord);

                Bubble.Bubble newBubble = CreateBubble(startCoord);
                if (newBubble != null)
                {
                    newBubble.transform.position = startWorldPos;
                    chain.Insert(0, newBubble);
                    Debug.Log($"[DynamicLevelSpawner] Path {pathIndex} Phase {phase + 1}: Created bubble at {startCoord}");
                }
            }

            // Place bubbles back in grid
            // Only place up to lastMissingIndex (fill the missing segment completely)
            int bubblesNeeded = lastMissingIndex + 1; // Total bubbles needed to fill from 0 to lastMissingIndex

            for (int i = 0; i < bubblesNeeded && i < chain.Count; i++)
            {
                if (chain[i] != null)
                {
                    // Skip if this position already has a bubble (shouldn't happen, but safety check)
                    if (!bubbleGrid.IsOccupied(pathCoords[i]))
                    {
                        bubbleGrid.PlaceBubble(pathCoords[i], chain[i]);
                        Debug.Log($"[DynamicLevelSpawner] Placed bubble at {pathCoords[i]}");
                    }
                    else
                    {
                        Debug.LogWarning($"[DynamicLevelSpawner] Position {pathCoords[i]} already occupied, destroying extra bubble");
                        Destroy(chain[i].gameObject);
                    }
                }
            }

            chain.Clear();
        }

        /// <summary>
        /// Animate a segment chain (similar to AnimateChain but for partial path)
        /// </summary>
        private IEnumerator AnimateSegmentChain(IReadOnlyList<Bubble.Bubble> chain, IReadOnlyList<HexCoordinate> pathCoords, int segmentStartIndex)
        {
            if (chain.Count == 0) yield break;

            Debug.Log($"[DynamicLevelSpawner] Animating segment chain - {chain.Count} bubbles");

            // Record start positions
            Vector3[] startPositions = new Vector3[chain.Count];
            for (int i = 0; i < chain.Count; i++)
            {
                startPositions[i] = chain[i].transform.position;
            }

            // Calculate target positions (each bubble moves to next coordinate in segment)
            Vector3[] targetPositions = new Vector3[chain.Count];
            for (int i = 0; i < chain.Count; i++)
            {
                int targetIndex = segmentStartIndex + i + 1;
                if (targetIndex < pathCoords.Count)
                {
                    targetPositions[i] = bubbleGrid.GetWorldPosition(pathCoords[targetIndex]);
                }
                else
                {
                    targetPositions[i] = startPositions[i];
                }
            }

            // Animate movement
            float elapsed = 0f;
            float moveSpeed = levelPathData.MoveSpeed;

            while (elapsed < moveSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveSpeed;
                float smoothT = 1f - Mathf.Pow(1f - t, 3f);

                for (int i = 0; i < chain.Count; i++)
                {
                    if (chain[i] != null)
                    {
                        chain[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], smoothT);
                    }
                }

                yield return null;
            }

            // Snap to final positions
            for (int i = 0; i < chain.Count; i++)
            {
                if (chain[i] != null)
                {
                    chain[i].transform.position = targetPositions[i];
                }
            }
        }

        /// <summary>
        /// Main generation coroutine - Chain push animation
        /// Phase N: Move all existing bubbles → Create new bubble at start position
        /// </summary>
        private IEnumerator GenerationCoroutine()
        {
            isSpawning = true;

            // Get minimum count across all paths
            int totalPhases = int.MaxValue;
            for (int i = 0; i < coordinates.Length; i++)
            {
                totalPhases = Mathf.Min(totalPhases, coordinates[i].Count);
            }

            for (int phase = 0; phase < totalPhases; phase++)
            {
                Debug.Log($"[DynamicLevelSpawner] ========== Phase {phase + 1}/{totalPhases} ==========");

                // Step 1: Move all existing bubbles in chains simultaneously (if any exist)
                bool hasAnyBubbles = false;
                for (int i = 0; i < bubbleChains.Length; i++)
                {
                    if (bubbleChains[i].Count > 0)
                    {
                        hasAnyBubbles = true;
                        break;
                    }
                }

                if (hasAnyBubbles)
                {
                    string chainInfo = "";
                    for (int i = 0; i < bubbleChains.Length; i++)
                    {
                        chainInfo += $"Chain{i}: {bubbleChains[i].Count}, ";
                    }
                    Debug.Log($"[DynamicLevelSpawner] Moving chains - {chainInfo.TrimEnd(',', ' ')}");
                    yield return AnimateAllChains(phase);
                }

                // Step 2: After movement complete, create new bubbles at start positions
                Debug.Log($"[DynamicLevelSpawner] Creating new bubbles for phase {phase + 1}");
                CreateNewBubblesAtStart(phase);

                Debug.Log($"[DynamicLevelSpawner] Phase {phase + 1} complete");
            }

            // Final step: Place all bubbles in grid
            PlaceAllBubblesInGrid();

            isSpawning = false;
            Debug.Log("========== DYNAMIC LEVEL GENERATION COMPLETE ==========");
        }

        /// <summary>
        /// Create new bubbles at start positions (always at index 0 of each path)
        /// </summary>
        private void CreateNewBubblesAtStart(int phase)
        {
            for (int i = 0; i < coordinates.Length; i++)
            {
                // Get start position (always first coordinate in path)
                HexCoordinate startCoord = coordinates[i][0];
                Vector2 startWorldPos = bubbleGrid.GetWorldPosition(startCoord);

                // Create bubble at start position
                Bubble.Bubble bubble = CreateBubble(startCoord);
                if (bubble != null)
                {
                    bubble.transform.position = startWorldPos;
                    bubbleChains[i].AddFirst(bubble); // Insert at beginning of chain
                    Debug.Log($"[DynamicLevelSpawner] Created bubble for chain {i} at {startCoord} (phase {phase + 1})");
                }
            }
        }

        /// <summary>
        /// Create a bubble with random color
        /// </summary>
        private Bubble.Bubble CreateBubble(HexCoordinate coord)
        {
            if (!BubblePoolManager.Instance)
            {
                Debug.LogError("[DynamicLevelSpawner] PoolManager is null!");
                return null;
            }

            Bubble.Bubble bubble = BubblePoolManager.Instance.GetBubble();
            if (bubble != null)
            {
                // Random color
                BubbleColorType randomColorType = (BubbleColorType)Random.Range(0, 5);
                int randomValue = Random.Range(0, 100);
                BubbleType randomType = randomValue switch
                {
                    < 60 => BubbleType.None,
                    >= 60 and < 80 => BubbleType.Fairy,
                    >= 80 and <= 99 => BubbleType.Bomb,
                    _ => BubbleType.None,
                };

                bubble.Initialize(randomColorType, randomType, coord);
                bubble.gameObject.SetActive(true);

                Debug.Log($"[DynamicLevelSpawner] Created bubble at {coord}, ColorType: {randomColorType}, Type: {randomType}");
            }

            return bubble;
        }

        /// <summary>
        /// Animate all chains simultaneously - all bubbles move one step forward
        /// </summary>
        private IEnumerator AnimateAllChains(int phase)
        {
            // Start all animations in parallel
            Coroutine[] animations = new Coroutine[bubbleChains.Length];
            for (int i = 0; i < bubbleChains.Length; i++)
            {
                animations[i] = StartCoroutine(AnimateChain(bubbleChains[i], coordinates[i], $"Chain{i}", phase));
            }

            // Wait for all to complete
            for (int i = 0; i < animations.Length; i++)
            {
                yield return animations[i];
            }

            Debug.Log("[DynamicLevelSpawner] All chains animation complete");
        }

        /// <summary>
        /// Animate a single chain (all bubbles move one step forward simultaneously)
        /// Each bubble at index i moves to coords[i+1], creating a chain push effect
        /// </summary>
        private IEnumerator AnimateChain(LinkedList<Bubble.Bubble> chain, IReadOnlyList<HexCoordinate> coords, string chainName, int phase)
        {
            if (chain.Count == 0)
            {
                Debug.Log($"[DynamicLevelSpawner] {chainName} is empty, skipping animation");
                yield break;
            }

            Debug.Log($"[DynamicLevelSpawner] Animating {chainName} - {chain.Count} bubbles moving forward");

            // Convert LinkedList to array for indexed access
            Bubble.Bubble[] bubbles = new Bubble.Bubble[chain.Count];
            int index = 0;
            foreach (var bubble in chain)
            {
                bubbles[index++] = bubble;
            }

            // Record start positions (current positions of all bubbles)
            Vector3[] startPositions = new Vector3[bubbles.Length];
            for (int i = 0; i < bubbles.Length; i++)
            {
                startPositions[i] = bubbles[i].transform.position;
            }

            // Calculate target positions (each bubble moves to next coordinate in path)
            // Bubble at index i moves to coords[i+1]
            Vector3[] targetPositions = new Vector3[bubbles.Length];
            for (int i = 0; i < bubbles.Length; i++)
            {
                // Each bubble moves one step forward in the path
                int targetIndex = i + 1;
                if (targetIndex < coords.Count)
                {
                    targetPositions[i] = bubbleGrid.GetWorldPosition(coords[targetIndex]);
                }
                else
                {
                    // If beyond path, stay at current position (shouldn't happen)
                    targetPositions[i] = startPositions[i];
                }
            }

            // Animate movement - all bubbles move simultaneously
            float elapsed = 0f;
            float moveSpeed = levelPathData.MoveSpeed;

            while (elapsed < moveSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveSpeed;

                // Ease-out curve for smooth animation
                float smoothT = 1f - Mathf.Pow(1f - t, 3f);

                // Move all bubbles simultaneously
                for (int i = 0; i < bubbles.Length; i++)
                {
                    if (bubbles[i] != null)
                    {
                        bubbles[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], smoothT);
                    }
                }

                yield return null;
            }

            // Snap to final positions
            for (int i = 0; i < bubbles.Length; i++)
            {
                if (bubbles[i] != null)
                {
                    bubbles[i].transform.position = targetPositions[i];
                }
            }

            Debug.Log($"[DynamicLevelSpawner] {chainName} animation complete (phase {phase + 1})");
        }

        /// <summary>
        /// Place all bubbles in grid after animation complete
        /// </summary>
        private void PlaceAllBubblesInGrid()
        {
            Debug.Log("[DynamicLevelSpawner] Placing all bubbles in grid...");

            for (int chainIndex = 0; chainIndex < bubbleChains.Length; chainIndex++)
            {
                // Convert LinkedList to array for indexed access
                Bubble.Bubble[] bubbles = new Bubble.Bubble[bubbleChains[chainIndex].Count];
                int index = 0;
                foreach (var bubble in bubbleChains[chainIndex])
                {
                    bubbles[index++] = bubble;
                }

                // Place bubbles in grid
                for (int i = 0; i < bubbles.Length; i++)
                {
                    if (bubbles[i] != null && i < coordinates[chainIndex].Count)
                    {
                        bubbleGrid.PlaceBubble(coordinates[chainIndex][i], bubbles[i]);
                        Debug.Log($"[DynamicLevelSpawner] Placed chain {chainIndex} bubble at {coordinates[chainIndex][i]}");
                    }
                }
            }

            // Log total counts
            string countInfo = "";
            for (int i = 0; i < bubbleChains.Length; i++)
            {
                countInfo += $"Chain{i}: {bubbleChains[i].Count}, ";
            }
            Debug.Log($"[DynamicLevelSpawner] Grid placement complete - {countInfo.TrimEnd(',', ' ')}");
        }

        /// <summary>
        /// Stop generation if in progress
        /// </summary>
        public void StopGeneration()
        {
            if (isSpawning)
            {
                StopAllCoroutines();
                isSpawning = false;
                Debug.Log("[DynamicLevelSpawner] Generation stopped");
            }
        }

        /// <summary>
        /// Check if currently spawning
        /// </summary>
        public bool IsSpawning()
        {
            return isSpawning;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (bubbleGrid == null || levelPathData == null)
                return;

            // Draw center position
            Gizmos.color = Color.cyan;
            Vector2 centerWorld = bubbleGrid.GetWorldPosition(levelPathData.CenterPosition);
            Gizmos.DrawWireSphere(centerWorld, 0.3f);

            // Draw path preview
            if (levelPathData.SpawnPaths != null)
            {
                for (int pathIndex = 0; pathIndex < levelPathData.SpawnPaths.Length; pathIndex++)
                {
                    var coords = levelPathData.GetCoordinatesForPath(pathIndex);

                    // Different colors for different paths
                    Gizmos.color = pathIndex == 0 ? Color.yellow : Color.green;

                    // Draw path as connected spheres
                    for (int i = 0; i < coords.Count; i++)
                    {
                        Vector2 worldPos = bubbleGrid.GetWorldPosition(coords[i]);
                        Gizmos.DrawWireSphere(worldPos, 0.2f);

                        // Draw connection line to next position
                        if (i < coords.Count - 1)
                        {
                            Vector2 nextWorldPos = bubbleGrid.GetWorldPosition(coords[i + 1]);
                            Gizmos.DrawLine(worldPos, nextWorldPos);
                        }
                    }
                }
            }
        }
#endif
    }
}
