using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BubblePuzzle.Grid;
using BubblePuzzle.Bubble;

namespace BubblePuzzle.GameLogic
{
    /// <summary>
    /// Handle bubble destruction with animations
    /// </summary>
    public class DestructionHandler : MonoBehaviour
    {
        [Header("Animation Settings")]
        [SerializeField] private float destructionDelay = 0.1f;
        [SerializeField] private float destructionDuration = 0.3f;
        [SerializeField] private AnimationCurve destructionCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [Header("Fall Settings")]
        [SerializeField] private float fallDuration = 1.0f;
        [SerializeField] private float fallGravity = 10f;
        [SerializeField] private float fallRotationSpeed = 360f;

        [Header("References")]
        [SerializeField] private BubbleGrid grid;
        [SerializeField] private BubblePoolManager poolManager;

        /// <summary>
        /// Destroy bubbles with animation
        /// </summary>
        public IEnumerator DestroyBubbles(List<Bubble.Bubble> bubbles)
        {
            Debug.Log("---------- DESTRUCTION START ----------");

            if (bubbles == null || bubbles.Count == 0)
            {
                Debug.Log("[DestructionHandler] No bubbles to destroy");
                yield break;
            }

            Debug.Log($"[DestructionHandler] Destroying {bubbles.Count} bubbles");

            // Remove from grid first
            foreach (Bubble.Bubble bubble in bubbles)
            {
                if (bubble != null && grid != null)
                {
                    Debug.Log($"[DestructionHandler] Removing bubble at {bubble.Coordinate} from grid");
                    grid.RemoveBubble(bubble.Coordinate);
                }
                else if (bubble == null)
                {
                    Debug.LogWarning("[DestructionHandler] Null bubble in destruction list!");
                }
            }

            Debug.Log($"[DestructionHandler] All {bubbles.Count} bubbles removed from grid");

            // Animate destruction
            List<Coroutine> animations = new List<Coroutine>();

            foreach (Bubble.Bubble bubble in bubbles)
            {
                if (bubble != null)
                {
                    animations.Add(StartCoroutine(DestructionAnimation(bubble)));
                }
            }

            // Wait for delay between bubbles
            float waitTime = destructionDelay * bubbles.Count;
            Debug.Log($"[DestructionHandler] Waiting {waitTime}s for animations");
            yield return new WaitForSeconds(waitTime);

            Debug.Log("---------- DESTRUCTION END ----------");
        }

        /// <summary>
        /// Single bubble destruction animation
        /// </summary>
        private IEnumerator DestructionAnimation(Bubble.Bubble bubble)
        {
            if (bubble == null) yield break;

            Vector3 startScale = bubble.transform.localScale;
            float elapsed = 0f;

            while (elapsed < destructionDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / destructionDuration;
                float scale = destructionCurve.Evaluate(t);

                bubble.transform.localScale = startScale * scale;

                yield return null;
            }

            // Return to pool or destroy
            if (poolManager != null)
            {
                poolManager.ReturnBubble(bubble);
            }
            else
            {
                Destroy(bubble.gameObject);
            }
        }

        /// <summary>
        /// Make bubbles fall with animation
        /// </summary>
        public IEnumerator MakeBubblesFall(List<Bubble.Bubble> bubbles)
        {
            Debug.Log("---------- FALL START ----------");

            if (bubbles == null || bubbles.Count == 0)
            {
                Debug.Log("[DestructionHandler] No bubbles to make fall");
                yield break;
            }

            Debug.Log($"[DestructionHandler] Making {bubbles.Count} bubbles fall");

            // Remove from grid
            foreach (Bubble.Bubble bubble in bubbles)
            {
                if (bubble != null && grid != null)
                {
                    Debug.Log($"[DestructionHandler] Removing falling bubble at {bubble.Coordinate} from grid");
                    grid.RemoveBubble(bubble.Coordinate);
                }
                else if (bubble == null)
                {
                    Debug.LogWarning("[DestructionHandler] Null bubble in fall list!");
                }
            }

            Debug.Log($"[DestructionHandler] All {bubbles.Count} falling bubbles removed from grid");

            // Start fall animations
            List<Coroutine> animations = new List<Coroutine>();

            foreach (Bubble.Bubble bubble in bubbles)
            {
                if (bubble != null)
                {
                    animations.Add(StartCoroutine(FallAnimation(bubble)));
                }
            }

            // Wait for fall to complete
            Debug.Log($"[DestructionHandler] Waiting {fallDuration}s for fall animations");
            yield return new WaitForSeconds(fallDuration);

            Debug.Log("[ProcessGameLogic] Fall animation complete");

            Debug.Log("---------- FALL END ----------");
        }

        /// <summary>
        /// Single bubble fall animation
        /// </summary>
        private IEnumerator FallAnimation(Bubble.Bubble bubble)
        {
            if (bubble == null) yield break;

            Vector3 startPos = bubble.transform.position;
            float elapsed = 0f;

            while (elapsed < fallDuration)
            {
                elapsed += Time.deltaTime;

                // Accelerating fall (gravity)
                float fallDistance = 0.5f * fallGravity * elapsed * elapsed;
                bubble.transform.position = startPos + Vector3.down * fallDistance;

                // Rotate while falling
                bubble.transform.Rotate(0, 0, fallRotationSpeed * Time.deltaTime);

                // Check if off-screen
                if (bubble.transform.position.y < -15f)
                {
                    break;
                }

                yield return null;
            }

            // Return to pool or destroy
            if (poolManager != null)
            {
                poolManager.ReturnBubble(bubble);
            }
            else
            {
                Destroy(bubble.gameObject);
            }
        }

        /// <summary>
        /// Destroy single bubble immediately (no animation)
        /// </summary>
        public void DestroyBubbleImmediate(Bubble.Bubble bubble)
        {
            if (bubble == null) return;

            if (grid != null)
            {
                grid.RemoveBubble(bubble.Coordinate);
            }

            if (poolManager != null)
            {
                poolManager.ReturnBubble(bubble);
            }
            else
            {
                Destroy(bubble.gameObject);
            }
        }
    }
}
