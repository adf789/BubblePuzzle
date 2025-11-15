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
        public HexCoordinate CenterPosition => centerPosition;
        public float MoveSpeed => moveSpeed;
        public BubbleSpawnPath[] SpawnPaths => spawnPaths;

        [Header("Spawn Paths")]
        [Tooltip("Array of spawn paths (typically 2: left and right)")]
        [SerializeField] private BubbleSpawnPath[] spawnPaths;

        [Header("Spawn count Settings")]
        [Tooltip("Spawn max and min count")]
        [SerializeField] private int maxSpawnCount;
        [SerializeField] private int minSpawnCount;

        [Header("Animation Settings")]
        [Tooltip("Time in seconds for each bubble movement step")]
        [Range(0.1f, 1.0f)]
        [SerializeField] private float moveSpeed = 0.3f;

        [Header("Grid Reference")]
        [Tooltip("Center position reference (usually 0,0)")]
        [SerializeField] private HexCoordinate centerPosition = new HexCoordinate(0, 0);

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
        void OnValidate()
        {
            InitializeSpawnCount();
        }

        private void InitializeSpawnCount()
        {
            if (spawnPaths == null || spawnPaths.Length == 0)
                return;

            maxSpawnCount = Mathf.Max(1, maxSpawnCount);
            minSpawnCount = Mathf.Clamp(minSpawnCount, 1, maxSpawnCount);

            for (int i = 0; i < spawnPaths.Length; i++)
            {
                if (spawnPaths[i].directions.Length == maxSpawnCount)
                    continue;

                var newDirections = new HexCoordinate.Direction[maxSpawnCount];
                int minCount = Mathf.Min(spawnPaths[i].directions.Length, maxSpawnCount);

                System.Array.Copy(spawnPaths[i].directions, newDirections, minCount);

                spawnPaths[i].directions = newDirections;
            }
        }
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
#endif
    }
}
