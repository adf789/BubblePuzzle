using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BubblePuzzle.UI
{
    /// <summary>
    /// Main game UI controller
    /// </summary>
    public class GameUI : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TextMeshProUGUI scoreText;
        [SerializeField] private TextMeshProUGUI comboText;
        [SerializeField] private TextMeshProUGUI bubblesRemainingText;
        [SerializeField] private Image comboBar;

        [Header("Settings")]
        [SerializeField] private bool showCombo = true;
        [SerializeField] private float comboDecayTime = 3f;

        private int currentScore;
        private int currentCombo;
        private float comboTimer;

        // Scoring
        private const int BASE_MATCH_SCORE = 10;
        private const int COMBO_MULTIPLIER = 5;
        private const int FALL_BONUS = 5;

        private void Update()
        {
            // Combo decay
            if (currentCombo > 0)
            {
                comboTimer -= Time.deltaTime;
                if (comboTimer <= 0f)
                {
                    ResetCombo();
                }
                UpdateComboBar();
            }
        }

        /// <summary>
        /// Add score from matched bubbles
        /// </summary>
        public void AddMatchScore(int bubbleCount)
        {
            int matchScore = bubbleCount * BASE_MATCH_SCORE;
            int comboBonus = currentCombo * COMBO_MULTIPLIER;

            currentScore += matchScore + comboBonus;
            currentCombo++;
            comboTimer = comboDecayTime;

            UpdateUI();
        }

        /// <summary>
        /// Add score from falling bubbles
        /// </summary>
        public void AddFallScore(int bubbleCount)
        {
            currentScore += bubbleCount * FALL_BONUS;
            UpdateUI();
        }

        /// <summary>
        /// Reset combo to zero
        /// </summary>
        private void ResetCombo()
        {
            currentCombo = 0;
            comboTimer = 0f;
            UpdateUI();
        }

        /// <summary>
        /// Update all UI elements
        /// </summary>
        private void UpdateUI()
        {
            if (scoreText != null)
            {
                scoreText.text = $"Score: {currentScore}";
            }

            if (comboText != null && showCombo)
            {
                if (currentCombo > 0)
                {
                    comboText.text = $"Combo x{currentCombo}";
                    comboText.gameObject.SetActive(true);
                }
                else
                {
                    comboText.gameObject.SetActive(false);
                }
            }
        }

        /// <summary>
        /// Update combo bar fill amount
        /// </summary>
        private void UpdateComboBar()
        {
            if (comboBar != null)
            {
                float fillAmount = comboTimer / comboDecayTime;
                comboBar.transform.localScale = new Vector3(fillAmount, 1, 1);
            }
        }

        /// <summary>
        /// Update bubbles remaining counter
        /// </summary>
        public void UpdateBubblesRemaining(int count)
        {
            if (bubblesRemainingText != null)
            {
                bubblesRemainingText.text = $"Bubbles: {count}";
            }
        }

        /// <summary>
        /// Get current score
        /// </summary>
        public int GetScore()
        {
            return currentScore;
        }

        /// <summary>
        /// Get current combo
        /// </summary>
        public int GetCombo()
        {
            return currentCombo;
        }

        /// <summary>
        /// Reset game stats
        /// </summary>
        public void ResetGame()
        {
            currentScore = 0;
            currentCombo = 0;
            comboTimer = 0f;
            UpdateUI();
        }
    }
}
