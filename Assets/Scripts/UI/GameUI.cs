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
        [SerializeField] private TextMeshProUGUI bossHpText;
        [SerializeField] private TextMeshProUGUI bubblesRemainingText;
        [SerializeField] private Image bossHpBarInnerground;
        [SerializeField] private Image bossHpBar;

        [Header("Settings")]
        [SerializeField] private float hpBarAnimationSpeed = 0.5f; // HP bar animation duration

        // Boss HP bar animation
        private Coroutine hpBarAnimationCoroutine;
        private float targetHpRatio = 1f; // Target HP ratio for smooth animation transition

        // Format
        private const string BOSS_HP_FORMAT = "{0} / {1} ({2:F1}%)";
        private const string SCORE_FORMAT = "Score: {0}";

        public void SetBossHp(in BossHp bossHp)
        {
            bossHpText.text = string.Format(BOSS_HP_FORMAT, bossHp.CurrentHp, bossHp.MaxHp, bossHp.Rate * 100);
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
        /// Reset game stats
        /// </summary>
        public void ResetGame()
        {
            targetHpRatio = 1f;

            UpdateScore(0);
            UpdateBossHp(new BossHp(1));
        }

        public void UpdateScore(int score)
        {
            scoreText.text = string.Format(SCORE_FORMAT, score);
        }

        /// <summary>
        /// Update boss HP bar with smooth animation
        /// If already animating, updates target ratio for seamless transition
        /// </summary>
        /// <param name="hpRatio">Target HP ratio (0.0 ~ 1.0)</param>
        public void UpdateBossHp(in BossHp bossHp, System.Action onEventFinished = null)
        {
            // Update target ratio
            targetHpRatio = bossHp.Rate;
            SetBossHp(in bossHp);

            // If animation is already running, update target and yield break new calls
            if (hpBarAnimationCoroutine != null)
            {
                return; // Don't start new coroutine, let existing one continue with new target
            }

            // Start new animation coroutine
            hpBarAnimationCoroutine = StartCoroutine(AnimateBossHpBar(onEventFinished));
        }

        /// <summary>
        /// Animate boss HP bar to follow main HP bar with delay effect
        /// Smoothly transitions to new target if updated during animation
        /// </summary>
        private System.Collections.IEnumerator AnimateBossHpBar(System.Action onEventFinished = null)
        {
            if (bossHpBar == null || bossHpBarInnerground == null)
            {
                Debug.LogWarning("[GameUI] Boss HP bar references are null!");
                hpBarAnimationCoroutine = null;
                yield break;
            }

            // Update main HP bar immediately
            Vector3 targetRatio = new Vector3(targetHpRatio, 1f, 1f);
            Vector3 currentRatio = bossHpBarInnerground.transform.localScale;

            bossHpBar.transform.localScale = targetRatio;

            while (Mathf.Abs(currentRatio.x - targetHpRatio) > 0.001f)
            {
                // Smoothly interpolate to target ratio
                currentRatio.x = Mathf.Lerp(
                    currentRatio.x,
                    targetHpRatio,
                    Time.deltaTime / hpBarAnimationSpeed
                );

                bossHpBarInnerground.transform.localScale = currentRatio;

                yield return null;
            }

            // Snap to final value
            bossHpBarInnerground.transform.localScale = new Vector3(targetHpRatio, 1f, 1f);

            // Animation complete
            hpBarAnimationCoroutine = null;

            onEventFinished?.Invoke();
        }
    }
}
