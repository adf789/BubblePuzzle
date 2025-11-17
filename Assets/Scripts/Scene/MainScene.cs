using System.Collections;
using BubblePuzzle.Bubble;
using BubblePuzzle.Core;
using BubblePuzzle.GameLogic;
using BubblePuzzle.Grid;
using BubblePuzzle.Shooter;
using UnityEngine;

public class MainScene : MonoBehaviour
{
    [SerializeField] private BubbleGrid bubbleGrid;
    [SerializeField] private BubbleShooter bubbleShooter;
    [SerializeField] private MatchDetector matchDetector;
    [SerializeField] private GravityChecker gravityChecker;
    [SerializeField] private DestructionHandler destructionHandler;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameManager.Instance.SetBubbleGrid(bubbleGrid);
        GameManager.Instance.SetupGame();
        GameManager.Instance.SetActiveDim(false);

        bubbleShooter.SetEventSacrificeBubble(OnSacrificeBubble);
        bubbleShooter.SetEventAfterShootCoroutine(ProcessGameLogic);
        bubbleShooter.Initialize(GameManager.Instance.LevelManager.BubbleShotCount);
    }

    /// <summary>
    /// Process game logic after bubble placement
    /// </summary>
    private IEnumerator ProcessGameLogic(Bubble placedBubble)
    {
        // Step 1: Check for matches
        var matches = matchDetector.FindMatchingCluster(placedBubble, bubbleGrid);
        int matchCount = matches != null ? matches.Count : 0;

        if (matchCount > 0)
        {
            // Notify GameManager of match
            GameManager.Instance.OnMatchScored(matchCount);

            // Step 2: Destroy matched bubbles
            yield return destructionHandler.DestroyBubbles(matches);

            // Step 2-1: Check game end conditions
            if (GameManager.Instance.LevelManager.BossHp.IsDeath)
            {
                bubbleShooter.SetLock(true);
                yield break;
            }
            else if (bubbleShooter.RemainShotCount == 0)
            {
                GameManager.Instance.OnDefeatResult();
                bubbleShooter.SetLock(true);
                yield break;
            }

            // Step 3: Check for disconnected bubbles
            var disconnected = gravityChecker.GetDisconnectedBubbles(bubbleGrid);
            if (disconnected.Count > 0)
            {
                // Notify GameManager of fall
                GameManager.Instance?.OnBubblesFallen(disconnected.Count);

                // Step 4: Make them fall
                StartCoroutine(destructionHandler.MakeBubblesFall(disconnected));
            }

            yield return GameManager.Instance.LevelManager.RegenerateIfNeeded();
        }
    }

    private void OnSacrificeBubble()
    {
        if (bubbleShooter.RemainShotCount == 0)
        {
            GameManager.Instance.OnDefeatResult();
            bubbleShooter.SetLock(true);
        }
    }
}
