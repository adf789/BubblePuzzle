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
        [SerializeField] private LevelManager levelManager;
        [SerializeField] private UIManager uiManager;

        [Header("Boss HP")]
        [SerializeField] private int bossHpValue;

        public LevelManager LevelManager => levelManager;
        public UIManager UIManager => uiManager;

        private BubbleGrid bubbleGrid;
        private BossHp bossHp;
        private int currentScore;

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

        public void SetupGame()
        {
            Debug.Log("BubblePuzzle Game Started");

            // Reset score
            currentScore = 0;

            // Init boss hp
            bossHp = new BossHp(bossHpValue);
            var gameUI = uiManager.GetUI<GameUI>(UIType.GameUI);
            gameUI?.SetBossHp(in bossHp);

            // Setup
            BubblePoolManager.Instance?.InitializePool();
            levelManager.LoadLevel();
        }

        public void SetBubbleGrid(BubbleGrid bubbleGrid)
        {
            this.bubbleGrid = bubbleGrid;

            levelManager?.SetBubbleGrid(bubbleGrid);
        }

        /// <summary>
        /// Handle bubble placement (called from shooter)
        /// </summary>
        public void OnBubblePlaced()
        {
            // Update UI
            var gameUI = uiManager.GetUI<GameUI>(UIType.GameUI);
            if (gameUI && bubbleGrid)
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
            int matchScore = bubbleCount * IntDefine.BASE_MATCH_SCORE;
            currentScore += matchScore;

            var gameUI = uiManager.GetUI<GameUI>(UIType.GameUI);
            gameUI?.UpdateScore(currentScore);

            CheckWinCondition();
        }

        /// <summary>
        /// Handle bubbles fallen
        /// </summary>
        public void OnBubblesFallen(int bubbleCount)
        {
            int matchScore = bubbleCount * IntDefine.FALL_BONUS;
            currentScore += matchScore;

            var gameUI = uiManager.GetUI<GameUI>(UIType.GameUI);
            gameUI?.UpdateScore(currentScore);

            CheckWinCondition();
        }

        public void OnDamagedBoss(int damage)
        {
            var gameUI = uiManager.GetUI<GameUI>(UIType.GameUI);
            if (gameUI)
            {
                bool isDeath = bossHp.Damage(damage);

                if (isDeath)
                    gameUI.UpdateBossHp(in bossHp, () => OnShowResultPopup(true));
                else
                    gameUI.UpdateBossHp(in bossHp);
            }
            else
            {
                OnShowResultPopup(true);
            }
        }

        private void OnShowResultPopup(bool isWin)
        {
            var resultPopup = uiManager.OpenUI<ResultPopup>(UIType.ResultPopup);

            if (resultPopup)
            {
                resultPopup.SetScore(currentScore);
                resultPopup.SetResult(isWin);
                resultPopup.SetEventResetGame(OnResetGame);
            }
        }

        private void OnResetGame()
        {
            foreach (var bubble in bubbleGrid.GetAllBubbles())
            {
                bubble.ReturnToPool();
            }

            SetBubbleGrid(null);

            uiManager.CloseAllUI();
        }

        /// <summary>
        /// Check if level is complete
        /// </summary>
        private void CheckWinCondition()
        {
            var gameUI = uiManager.GetUI<GameUI>(UIType.GameUI);
            if (levelManager != null && gameUI)
            {
                levelManager.CheckWinCondition(currentScore);
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
            if (bubbleGrid)
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
