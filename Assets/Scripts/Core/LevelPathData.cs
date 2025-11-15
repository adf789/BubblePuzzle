using UnityEngine;
using System.Collections.Generic;

namespace BubblePuzzle.Core
{
    /// <summary>
    /// ScriptableObject containing bubble spawn path data for dynamic level generation
    /// Defines multiple spawn paths (typically left and right)
    /// </summary>
    [CreateAssetMenu(fileName = "LevelPathData", menuName = "BubblePuzzle/Level Path Data", order = 1)]
    public class LevelPathData : ScriptableObject
    {
        [Header("Spawn Paths")]
        [Tooltip("Array of spawn paths (typically 2: left and right)")]
        public BubbleSpawnPath[] spawnPaths;

        [Header("Animation Settings")]
        [Tooltip("Time in seconds for each bubble movement step")]
        [Range(0.1f, 1.0f)]
        public float moveSpeed = 0.3f;

        [Header("Grid Reference")]
        [Tooltip("Center position reference (usually 0,0)")]
        public HexCoordinate centerPosition = new HexCoordinate(0, 0);

        /// <summary>
        /// Get all coordinates for a specific path
        /// </summary>
        public List<HexCoordinate> GetCoordinatesForPath(int pathIndex)
        {
            if (spawnPaths == null || pathIndex < 0 || pathIndex >= spawnPaths.Length)
            {
                Debug.LogError($"[LevelPathData] Invalid path index {pathIndex} (total paths: {spawnPaths?.Length ?? 0})");
                return new List<HexCoordinate>();
            }

            BubbleSpawnPath path = spawnPaths[pathIndex];
            if (!path.IsValid())
            {
                Debug.LogError($"[LevelPathData] Path {pathIndex} is invalid");
                return new List<HexCoordinate>();
            }

            return path.GetAllCoordinates();
        }

        /// <summary>
        /// Get coordinate at specific index in a path
        /// </summary>
        public HexCoordinate GetCoordinateAt(int pathIndex, int coordinateIndex)
        {
            if (spawnPaths == null || pathIndex < 0 || pathIndex >= spawnPaths.Length)
            {
                Debug.LogError($"[LevelPathData] Invalid path index {pathIndex}");
                return new HexCoordinate(0, 0);
            }

            return spawnPaths[pathIndex].GetCoordinateAt(coordinateIndex);
        }

        /// <summary>
        /// Get total number of paths
        /// </summary>
        public int GetPathCount()
        {
            return spawnPaths?.Length ?? 0;
        }

        /// <summary>
        /// Get total bubbles in a specific path
        /// </summary>
        public int GetBubbleCountForPath(int pathIndex)
        {
            if (spawnPaths == null || pathIndex < 0 || pathIndex >= spawnPaths.Length)
            {
                return 0;
            }

            return spawnPaths[pathIndex].GetTotalBubbleCount();
        }

        /// <summary>
        /// Validate all paths
        /// </summary>
        public bool ValidateAllPaths()
        {
            if (spawnPaths == null || spawnPaths.Length == 0)
            {
                Debug.LogError("[LevelPathData] No spawn paths defined");
                return false;
            }

            bool allValid = true;
            for (int i = 0; i < spawnPaths.Length; i++)
            {
                if (!spawnPaths[i].IsValid())
                {
                    Debug.LogError($"[LevelPathData] Path {i} is invalid");
                    allValid = false;
                }
            }

            return allValid;
        }

#if UNITY_EDITOR
        /// <summary>
        /// Validate and log path information
        /// </summary>
        [ContextMenu("Validate Paths")]
        public void ValidatePathsMenu()
        {
            Debug.Log("========== LEVEL PATH DATA VALIDATION ==========");
            Debug.Log($"Total Paths: {GetPathCount()}");

            for (int i = 0; i < GetPathCount(); i++)
            {
                Debug.Log($"Path {i}: {spawnPaths[i].GetSummary()}");

                // Log first few coordinates
                var coords = GetCoordinatesForPath(i);
                string coordsPreview = "";
                for (int j = 0; j < Mathf.Min(5, coords.Count); j++)
                {
                    coordsPreview += $"{coords[j]} ";
                }
                if (coords.Count > 5)
                    coordsPreview += "...";

                Debug.Log($"  First coordinates: {coordsPreview}");
            }

            bool isValid = ValidateAllPaths();
            Debug.Log($"Validation Result: {(isValid ? "✓ VALID" : "✗ INVALID")}");
            Debug.Log("================================================");
        }

        /// <summary>
        /// Create example data for right path (wall at 4)
        /// </summary>
        [ContextMenu("Create Example Right Path (Wall 4)")]
        public void CreateExampleRightPath()
        {
            spawnPaths = new BubbleSpawnPath[1];
            spawnPaths[0] = new BubbleSpawnPath
            {
                startPosition = new HexCoordinate(1, 0),
                directions = new HexCoordinate.Direction[]
                {
                    HexCoordinate.Direction.Right,       // (1,0) → (2,0)
                    HexCoordinate.Direction.Right,       // (2,0) → (3,0)
                    HexCoordinate.Direction.BottomRight, // (3,0) → (4,-1)
                    HexCoordinate.Direction.BottomLeft,  // (4,-1) → (4,-2)
                    HexCoordinate.Direction.Left,        // (4,-2) → (3,-2)
                    HexCoordinate.Direction.Left,        // (3,-2) → (2,-2)
                    HexCoordinate.Direction.BottomLeft,  // (2,-2) → (2,-3)
                    HexCoordinate.Direction.BottomRight, // (2,-3) → (3,-4)
                    HexCoordinate.Direction.Right,       // (3,-4) → (4,-4)
                    HexCoordinate.Direction.Right        // (4,-4) → (5,-4)
                }
            };

            Debug.Log("Example right path created with 11 bubbles");
            UnityEditor.EditorUtility.SetDirty(this);
        }

        /// <summary>
        /// Create example data for both left and right paths
        /// </summary>
        [ContextMenu("Create Example Both Paths (Wall 4)")]
        public void CreateExampleBothPaths()
        {
            spawnPaths = new BubbleSpawnPath[2];

            // Right path
            spawnPaths[0] = new BubbleSpawnPath
            {
                startPosition = new HexCoordinate(1, 0),
                directions = new HexCoordinate.Direction[]
                {
                    HexCoordinate.Direction.Right,
                    HexCoordinate.Direction.Right,
                    HexCoordinate.Direction.BottomRight,
                    HexCoordinate.Direction.BottomLeft,
                    HexCoordinate.Direction.Left,
                    HexCoordinate.Direction.Left,
                    HexCoordinate.Direction.BottomLeft,
                    HexCoordinate.Direction.BottomRight,
                    HexCoordinate.Direction.Right,
                    HexCoordinate.Direction.Right
                }
            };

            // Left path (mirror of right)
            spawnPaths[1] = new BubbleSpawnPath
            {
                startPosition = new HexCoordinate(-1, 0),
                directions = new HexCoordinate.Direction[]
                {
                    HexCoordinate.Direction.Left,
                    HexCoordinate.Direction.Left,
                    HexCoordinate.Direction.BottomLeft,
                    HexCoordinate.Direction.BottomRight,
                    HexCoordinate.Direction.Right,
                    HexCoordinate.Direction.Right,
                    HexCoordinate.Direction.BottomRight,
                    HexCoordinate.Direction.BottomLeft,
                    HexCoordinate.Direction.Left,
                    HexCoordinate.Direction.Left
                }
            };

            Debug.Log("Example both paths created (left and right, 11 bubbles each)");
            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}
