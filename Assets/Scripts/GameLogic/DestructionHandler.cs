using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BubblePuzzle.Grid;
using BubblePuzzle.Bubble;
using BubblePuzzle.Core;

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

        [Header("Projectile Settings")]
        [SerializeField] private GameObject projectilePrefab; // Prefab for flying projectile
        [SerializeField] private Transform projectileTarget; // Target position (e.g., boss position)
        [SerializeField] private float projectileInitialSpeed = 5f;
        [SerializeField] private float projectileAcceleration = 10f;
        [SerializeField] private float projectileMaxSpeed = 30f;
        [SerializeField] private float projectileRotationSpeed = 720f;

        [Header("References")]
        [SerializeField] private BubbleGrid grid;

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

            // Categorize bubbles by type
            List<Bubble.Bubble> normalBubbles = new List<Bubble.Bubble>();
            List<Bubble.Bubble> projectileBubbles = new List<Bubble.Bubble>();

            foreach (Bubble.Bubble bubble in bubbles)
            {
                if (bubble != null)
                {
                    // Determine bubble type event
                    // Example: Red bubbles spawn projectiles
                    if (ShouldSpawnProjectile(bubble))
                    {
                        projectileBubbles.Add(bubble);
                    }
                    else
                    {
                        normalBubbles.Add(bubble);
                    }
                }
            }

            Debug.Log($"[DestructionHandler] Normal: {normalBubbles.Count}, Projectile: {projectileBubbles.Count}");

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

            // Normal destruction animation
            foreach (Bubble.Bubble bubble in normalBubbles)
            {
                if (bubble != null)
                {
                    animations.Add(StartCoroutine(DestructionAnimation(bubble)));
                }
            }

            // Projectile bubbles - spawn projectile and destroy
            foreach (Bubble.Bubble bubble in projectileBubbles)
            {
                if (bubble != null)
                {
                    SpawnProjectile(bubble.transform.position);
                    animations.Add(StartCoroutine(DestructionAnimation(bubble)));
                }
            }

            if (projectileBubbles.Count > 0)
            {
                int damage = 1;
                GameManager.Instance.OnDamagedBoss(projectileBubbles.Count * damage);
            }

            // Wait for delay between bubbles
            float waitTime = destructionDelay * bubbles.Count;
            Debug.Log($"[DestructionHandler] Waiting {waitTime}s for animations");
            yield return new WaitForSeconds(waitTime);

            Debug.Log("---------- DESTRUCTION END ----------");
        }

        /// <summary>
        /// Determine if bubble should spawn projectile
        /// </summary>
        private bool ShouldSpawnProjectile(Bubble.Bubble bubble)
        {
            // Modify this logic based on your game design
            return bubble.Type == BubbleType.Fairy;
        }

        /// <summary>
        /// Spawn a projectile at bubble position that flies to target
        /// </summary>
        private void SpawnProjectile(Vector3 startPosition)
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning("[DestructionHandler] Projectile prefab is null!");
                return;
            }

            if (projectileTarget == null)
            {
                Debug.LogWarning("[DestructionHandler] Projectile target is null!");
                return;
            }

            // Instantiate projectile
            GameObject projectile = Instantiate(projectilePrefab, startPosition, Quaternion.identity);

            // Start flying animation
            StartCoroutine(ProjectileFlightAnimation(projectile, startPosition));
        }

        /// <summary>
        /// Projectile flight animation with acceleration like a missile
        /// </summary>
        private IEnumerator ProjectileFlightAnimation(GameObject projectile, Vector3 startPosition)
        {
            if (projectile == null) yield break;

            float currentSpeed = projectileInitialSpeed;
            Vector3 currentPosition = startPosition;

            while (projectile != null && projectileTarget != null)
            {
                // Calculate direction to target
                Vector3 direction = (projectileTarget.position - currentPosition).normalized;

                // Accelerate
                currentSpeed += projectileAcceleration * Time.deltaTime;
                currentSpeed = Mathf.Min(currentSpeed, projectileMaxSpeed);

                // Move towards target
                currentPosition += direction * currentSpeed * Time.deltaTime;
                projectile.transform.position = currentPosition;

                // Rotate towards direction
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
                projectile.transform.rotation = Quaternion.Euler(0, 0, angle - 90f);

                // Add spinning effect
                projectile.transform.Rotate(0, 0, projectileRotationSpeed * Time.deltaTime);

                // Check if reached target
                float distanceToTarget = Vector3.Distance(currentPosition, projectileTarget.position);
                if (distanceToTarget < 0.5f)
                {
                    // Reached target - apply effect here if needed
                    Debug.Log("[DestructionHandler] Projectile reached target!");

                    // Destroy projectile
                    Destroy(projectile);
                    yield break;
                }

                yield return null;
            }

            // Cleanup if something went wrong
            if (projectile != null)
            {
                Destroy(projectile);
            }
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

            bubble.ReturnToPool();
            bubble.transform.localScale = Vector3.one;
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

            bubble.ReturnToPool();
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

            bubble.ReturnToPool();
        }
    }
}
