using UnityEngine;
using BubblePuzzle.Grid;
using BubblePuzzle.UI;
using BubblePuzzle.GameLogic;
using BubblePuzzle.Bubble;

namespace BubblePuzzle.Core
{
    /// <summary>
    /// Main game controller (Singleton)
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        [Header("References")]
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private GameUI gameUI;
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private MatchDetector matchDetector;
        [SerializeField] private GravityChecker gravityChecker;
        [SerializeField] private BubbleReadyPool bubbleReadyPool;

        public BubbleGrid Grid => bubbleGrid;
        public GameUI UI => gameUI;
        public LevelManager LevelManager => levelManager;

        private void Awake()
        {
            // Singleton pattern
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
            }
            else
            {
                Destroy(gameObject);
                return;
            }

            Application.runInBackground = true;
        }

        private void Start()
        {
            SetupGame();
        }

        private void SetupGame()
        {
            Debug.Log("BubblePuzzle Game Started");

            BubblePoolManager.Instance.InitializePool();
            LevelManager.Instance.LoadLevel();

            bubbleReadyPool.Reload();
        }

        /// <summary>
        /// Handle bubble placement (called from shooter)
        /// </summary>
        public void OnBubblePlaced()
        {
            // Update UI
            if (gameUI != null && bubbleGrid != null)
            {
                gameUI.UpdateBubblesRemaining(bubbleGrid.GetBubbleCount());
            }

            // Increment shot counter
            if (levelManager != null)
            {
                levelManager.IncrementShot();
            }
        }

        /// <summary>
        /// Handle match scored
        /// </summary>
        public void OnMatchScored(int bubbleCount)
        {
            if (gameUI != null)
            {
                gameUI.AddMatchScore(bubbleCount);
            }

            CheckWinCondition();
        }

        /// <summary>
        /// Handle bubbles fallen
        /// </summary>
        public void OnBubblesFallen(int bubbleCount)
        {
            if (gameUI != null)
            {
                gameUI.AddFallScore(bubbleCount);
            }

            CheckWinCondition();
        }

        /// <summary>
        /// Check if level is complete
        /// </summary>
        private void CheckWinCondition()
        {
            if (levelManager != null && gameUI != null)
            {
                levelManager.CheckWinCondition(gameUI.GetScore());
            }
        }

#if UNITY_EDITOR
        private void Update()
        {
            // Debug shortcuts
            if (Input.GetKeyDown(KeyCode.C))
            {
                ClearGrid();
            }

            if (Input.GetKeyDown(KeyCode.R))
            {
                ReloadScene();
            }
        }

        private void ClearGrid()
        {
            if (bubbleGrid != null)
            {
                bubbleGrid.ClearAll();
                Debug.Log("Grid cleared");
            }
        }

        private void ReloadScene()
        {
            UnityEngine.SceneManagement.SceneManager.LoadScene(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene().name
            );
        }
#endif
    }
}
