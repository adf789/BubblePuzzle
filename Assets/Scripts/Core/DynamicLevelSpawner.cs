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

        [Header("References")]
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private BubblePoolManager poolManager;

        // Precomputed coordinates from path data
        private List<HexCoordinate> leftCoords = new List<HexCoordinate>();
        private List<HexCoordinate> rightCoords = new List<HexCoordinate>();

        // Bubble chains for animation
        private List<Bubble.Bubble> leftChain = new List<Bubble.Bubble>();
        private List<Bubble.Bubble> rightChain = new List<Bubble.Bubble>();

        private bool isSpawning = false;

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
            Debug.Log($"[DynamicLevelSpawner] Paths: {levelPathData.GetPathCount()}, MoveSpeed: {levelPathData.moveSpeed}s");

            // Clear existing bubbles
            if (bubbleGrid != null)
            {
                bubbleGrid.ClearAll();
            }

            // Load coordinates from path data
            leftCoords = levelPathData.GetCoordinatesForPath(0);   // Left path (first path)
            rightCoords = levelPathData.GetCoordinatesForPath(1);  // Right path (second path)

            Debug.Log($"[DynamicLevelSpawner] Left path: {leftCoords.Count} bubbles");
            Debug.Log($"[DynamicLevelSpawner] Right path: {rightCoords.Count} bubbles");

            // Clear chains
            leftChain.Clear();
            rightChain.Clear();

            // Start generation coroutine
            StartCoroutine(GenerationCoroutine());
        }

        /// <summary>
        /// Main generation coroutine - Chain push animation
        /// Phase N: Move all existing bubbles → Create new bubble at start position
        /// </summary>
        private IEnumerator GenerationCoroutine()
        {
            isSpawning = true;

            // Get minimum count (both paths should have same count, but just in case)
            int totalPhases = Mathf.Min(leftCoords.Count, rightCoords.Count);

            for (int phase = 0; phase < totalPhases; phase++)
            {
                Debug.Log($"[DynamicLevelSpawner] ========== Phase {phase + 1}/{totalPhases} ==========");

                // Step 1: Move all existing bubbles in chains simultaneously (if any exist)
                if (leftChain.Count > 0 || rightChain.Count > 0)
                {
                    Debug.Log($"[DynamicLevelSpawner] Moving chains - Left: {leftChain.Count}, Right: {rightChain.Count}");
                    yield return AnimateBothChains(phase);
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
            // Get start positions (always first coordinate in each path)
            HexCoordinate leftStartCoord = leftCoords[0];
            HexCoordinate rightStartCoord = rightCoords[0];

            Vector2 leftStartWorldPos = bubbleGrid.GetWorldPosition(leftStartCoord);
            Vector2 rightStartWorldPos = bubbleGrid.GetWorldPosition(rightStartCoord);

            // Create left bubble at start position
            Bubble.Bubble leftBubble = CreateBubble(leftStartCoord);
            if (leftBubble != null)
            {
                leftBubble.transform.position = leftStartWorldPos;
                leftChain.Insert(0, leftBubble); // Insert at beginning of chain
                Debug.Log($"[DynamicLevelSpawner] Created left bubble at {leftStartCoord} (phase {phase + 1})");
            }

            // Create right bubble at start position
            Bubble.Bubble rightBubble = CreateBubble(rightStartCoord);
            if (rightBubble != null)
            {
                rightBubble.transform.position = rightStartWorldPos;
                rightChain.Insert(0, rightBubble); // Insert at beginning of chain
                Debug.Log($"[DynamicLevelSpawner] Created right bubble at {rightStartCoord} (phase {phase + 1})");
            }
        }

        /// <summary>
        /// Create a bubble with random color
        /// </summary>
        private Bubble.Bubble CreateBubble(HexCoordinate coord)
        {
            if (poolManager == null)
            {
                Debug.LogError("[DynamicLevelSpawner] PoolManager is null!");
                return null;
            }

            Bubble.Bubble bubble = poolManager.GetBubble();
            if (bubble != null)
            {
                // Random color
                BubbleType randomType = (BubbleType)Random.Range(0, 5);
                bubble.Initialize(randomType, coord);
                bubble.gameObject.SetActive(true);

                Debug.Log($"[DynamicLevelSpawner] Created bubble at {coord}, Type: {randomType}");
            }

            return bubble;
        }

        /// <summary>
        /// Animate both chains simultaneously - all bubbles move one step forward
        /// </summary>
        private IEnumerator AnimateBothChains(int phase)
        {
            // Start both animations in parallel
            Coroutine leftAnim = StartCoroutine(AnimateChain(leftChain, leftCoords, "Left", phase));
            Coroutine rightAnim = StartCoroutine(AnimateChain(rightChain, rightCoords, "Right", phase));

            // Wait for both to complete
            yield return leftAnim;
            yield return rightAnim;

            Debug.Log("[DynamicLevelSpawner] Both chains animation complete");
        }

        /// <summary>
        /// Animate a single chain (all bubbles move one step forward simultaneously)
        /// Each bubble at index i moves to coords[i+1], creating a chain push effect
        /// </summary>
        private IEnumerator AnimateChain(List<Bubble.Bubble> chain, List<HexCoordinate> coords, string side, int phase)
        {
            if (chain.Count == 0)
            {
                Debug.Log($"[DynamicLevelSpawner] {side} chain is empty, skipping animation");
                yield break;
            }

            Debug.Log($"[DynamicLevelSpawner] Animating {side} chain - {chain.Count} bubbles moving forward");

            // Record start positions (current positions of all bubbles)
            Vector3[] startPositions = new Vector3[chain.Count];
            for (int i = 0; i < chain.Count; i++)
            {
                startPositions[i] = chain[i].transform.position;
            }

            // Calculate target positions (each bubble moves to next coordinate in path)
            // Bubble at index i moves to coords[i+1]
            Vector3[] targetPositions = new Vector3[chain.Count];
            for (int i = 0; i < chain.Count; i++)
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
            float moveSpeed = levelPathData.moveSpeed;

            while (elapsed < moveSpeed)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / moveSpeed;

                // Ease-out curve for smooth animation
                float smoothT = 1f - Mathf.Pow(1f - t, 3f);

                // Move all bubbles simultaneously
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

            Debug.Log($"[DynamicLevelSpawner] {side} chain animation complete (phase {phase + 1})");
        }

        /// <summary>
        /// Place all bubbles in grid after animation complete
        /// </summary>
        private void PlaceAllBubblesInGrid()
        {
            Debug.Log("[DynamicLevelSpawner] Placing all bubbles in grid...");

            // Place left chain bubbles
            for (int i = 0; i < leftChain.Count; i++)
            {
                if (leftChain[i] != null && i < leftCoords.Count)
                {
                    bubbleGrid.PlaceBubble(leftCoords[i], leftChain[i]);
                    Debug.Log($"[DynamicLevelSpawner] Placed left bubble at {leftCoords[i]}");
                }
            }

            // Place right chain bubbles
            for (int i = 0; i < rightChain.Count; i++)
            {
                if (rightChain[i] != null && i < rightCoords.Count)
                {
                    bubbleGrid.PlaceBubble(rightCoords[i], rightChain[i]);
                    Debug.Log($"[DynamicLevelSpawner] Placed right bubble at {rightCoords[i]}");
                }
            }

            Debug.Log($"[DynamicLevelSpawner] Grid placement complete - Left: {leftChain.Count}, Right: {rightChain.Count}");
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
            Vector2 centerWorld = bubbleGrid.GetWorldPosition(levelPathData.centerPosition);
            Gizmos.DrawWireSphere(centerWorld, 0.3f);

            // Draw path preview
            if (levelPathData.spawnPaths != null)
            {
                for (int pathIndex = 0; pathIndex < levelPathData.spawnPaths.Length; pathIndex++)
                {
                    List<HexCoordinate> coords = levelPathData.GetCoordinatesForPath(pathIndex);

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
