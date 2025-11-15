using System.Collections;
using UnityEngine;
using BubblePuzzle.Grid;
using BubblePuzzle.Bubble;
using BubblePuzzle.Core;

namespace BubblePuzzle.Core
{
    /// <summary>
    /// Level loading and management
    /// </summary>
    public class LevelManager : MonoBehaviour
    {
        private static LevelManager instance;
        public static LevelManager Instance => instance;

        [Header("References")]
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private BubblePoolManager poolManager;
        [SerializeField] private GameObject bubblePrefab;
        [SerializeField] private DynamicLevelSpawner dynamicSpawner;

        [Header("Current Level")]
        [SerializeField] private LevelData currentLevel;

        [Header("Level Mode")]
        [SerializeField] private bool useDynamicGeneration = true;

        private int shotsUsed = 0;

        void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Load level data
        /// </summary>
        public void LoadLevel()
        {
            if (currentLevel == null)
            {
                Debug.LogError("Level data is null!");
                return;
            }

            shotsUsed = 0;

            // Clear existing bubbles
            if (bubbleGrid != null)
            {
                bubbleGrid.ClearAll();
            }

            // Choose generation method
            if (useDynamicGeneration && dynamicSpawner != null)
            {
                // Use dynamic snake pattern generation
                Debug.Log($"[LevelManager] Starting dynamic generation for level: {currentLevel.LevelName}");
                dynamicSpawner.StartGeneration();
            }
            else
            {
                // Use traditional pattern loading
                if (currentLevel.InitialPattern != null && currentLevel.InitialPattern.rows != null)
                {
                    LoadPattern(currentLevel.InitialPattern);
                }
                else
                {
                    Debug.LogError($"Failed load to level: {currentLevel.LevelName}");
                }
            }

            Debug.Log($"Level loaded: {currentLevel.LevelName}");
        }

        /// <summary>
        /// Load predefined pattern
        /// </summary>
        private void LoadPattern(LevelPattern pattern)
        {
            for (int r = 0; r < pattern.rows.Length; r++)
            {
                string row = pattern.rows[r];
                int startQ = -row.Length / 2;

                for (int q = 0; q < row.Length; q++)
                {
                    char c = row[q];
                    BubbleType? type = LevelPattern.ParseBubbleType(c);

                    if (type.HasValue)
                    {
                        HexCoordinate coord = new HexCoordinate(startQ + q, r);
                        SpawnBubble(coord, type.Value);
                    }
                }
            }
        }

        /// <summary>
        /// Spawn bubble at coordinate
        /// </summary>
        private void SpawnBubble(HexCoordinate coord, BubbleType type)
        {
            Bubble.Bubble bubble = poolManager?.GetBubble();

            if (bubble != null)
            {
                bubble.Initialize(type, coord);
                bubbleGrid.PlaceBubble(coord, bubble);
            }
        }

        /// <summary>
        /// Increment shot counter
        /// </summary>
        public void IncrementShot()
        {
            shotsUsed++;

            if (currentLevel != null && shotsUsed >= currentLevel.MaxShots)
            {
                OnOutOfShots();
            }
        }

        /// <summary>
        /// Check win condition
        /// </summary>
        public bool CheckWinCondition(int currentScore)
        {
            if (currentLevel == null) return false;

            // Check if target score reached
            if (currentScore >= currentLevel.TargetScore)
            {
                OnLevelComplete();
                return true;
            }

            // Check if all bubbles cleared
            if (bubbleGrid != null && bubbleGrid.GetBubbleCount() == 0)
            {
                OnLevelComplete();
                return true;
            }

            return false;
        }

        /// <summary>
        /// Level completed
        /// </summary>
        private void OnLevelComplete()
        {
            Debug.Log($"Level Complete! Shots used: {shotsUsed}/{currentLevel.MaxShots}");
            // TODO: Show victory UI, load next level
        }

        /// <summary>
        /// Out of shots
        /// </summary>
        private void OnOutOfShots()
        {
            Debug.Log("Out of shots! Game Over");
            // TODO: Show game over UI
        }

        /// <summary>
        /// Get shots remaining
        /// </summary>
        public int GetShotsRemaining()
        {
            if (currentLevel == null) return 0;
            return Mathf.Max(0, currentLevel.MaxShots - shotsUsed);
        }

        /// <summary>
        /// Get current level data
        /// </summary>
        public LevelData GetCurrentLevel()
        {
            return currentLevel;
        }
    }
}
