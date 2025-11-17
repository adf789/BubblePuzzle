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
        [SerializeField] private Image bossHpBarInnerground;
        [SerializeField] private Image bossHpBar;

        [Header("Settings")]
        [SerializeField] private float hpBarAnimationSpeed = 0.5f;

        // Boss HP bar animation
        private Coroutine hpBarAnimationCoroutine;
        private float targetHpRatio = 1f;

        // Format
        private const string BOSS_HP_FORMAT = "{0} / {1} ({2:F1}%)";
        private const string SCORE_FORMAT = "Score: {0}";

        public void SetBossHp(in BossHp bossHp)
        {
            bossHpText.text = string.Format(BOSS_HP_FORMAT, bossHp.CurrentHp, bossHp.MaxHp, bossHp.Rate * 100);
        }

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

        public void UpdateBossHp(BossHp bossHp, System.Action onEventFinished = null)
        {
            targetHpRatio = bossHp.Rate;
            SetBossHp(in bossHp);

            if (hpBarAnimationCoroutine != null)
                return;

            hpBarAnimationCoroutine = StartCoroutine(AnimateBossHpBar(onEventFinished));
        }

        private System.Collections.IEnumerator AnimateBossHpBar(System.Action onEventFinished = null)
        {
            if (bossHpBar == null || bossHpBarInnerground == null)
            {
                Debug.LogWarning("[GameUI] Boss HP bar references are null!");
                hpBarAnimationCoroutine = null;
                yield break;
            }

            Vector3 targetRatio = new Vector3(targetHpRatio, 1f, 1f);
            Vector3 currentRatio = bossHpBarInnerground.transform.localScale;

            bossHpBar.transform.localScale = targetRatio;

            while (Mathf.Abs(currentRatio.x - targetHpRatio) > 0.001f)
            {
                currentRatio.x = Mathf.Lerp(
                    currentRatio.x,
                    targetHpRatio,
                    Time.deltaTime / hpBarAnimationSpeed
                );

                bossHpBarInnerground.transform.localScale = currentRatio;

                yield return null;
            }

            bossHpBarInnerground.transform.localScale = new Vector3(targetHpRatio, 1f, 1f);

            hpBarAnimationCoroutine = null;

            onEventFinished?.Invoke();
        }
    }
}
