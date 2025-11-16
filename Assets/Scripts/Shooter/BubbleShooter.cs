using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using BubblePuzzle.Core;
using BubblePuzzle.Grid;
using BubblePuzzle.UI;
using BubblePuzzle.Bubble;
using BubblePuzzle.GameLogic;

namespace BubblePuzzle.Shooter
{
    /// <summary>
    /// Bubble shooter with aiming and input handling
    /// </summary>
    public class BubbleShooter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private TrajectoryCalculator trajectoryCalculator;
        [SerializeField] private AimGuide aimGuide;
        [SerializeField] private PreviewFrame previewFrame;
        [SerializeField] private BubbleGrid bubbleGrid;
        [SerializeField] private MatchDetector matchDetector;
        [SerializeField] private GravityChecker gravityChecker;
        [SerializeField] private DestructionHandler destructionHandler;
        [SerializeField] private BubbleReadyPool bubbleReadyPool;

        [Header("Shooter Settings")]
        [SerializeField] private Transform shooterTransform;
        [Range(1f, 100f)]
        [SerializeField] private float minAimDistance = 1f;
        [SerializeField] private GameObject bubblePrefab;
        [Range(1f, 100f)]
        [SerializeField] private float shootSpeed = 10f;
        [SerializeField] private BoxCollider2D shootArea = null;

        [Header("Debug")]
        [SerializeField] private bool showDebugGizmos = true;

        private bool isShooting;
        private bool isAiming;
        private Vector2 aimDirection;
        private TrajectoryCalculator.TrajectoryResult currentTrajectory;

        void Awake()
        {
            previewFrame.SetGridOffset(bubbleGrid.GridOffset);
        }

        private void Update()
        {
            HandleAimingInput();
        }

        /// <summary>
        /// Handle mouse input for aiming
        /// </summary>
        private void HandleAimingInput()
        {
            // Don't allow aiming while shooting
            if (isShooting)
                return;

            if (bubbleReadyPool.IsReloading)
                return;

            if (GameManager.Instance.LevelManager.IsSpawning)
                return;

            // Check if left mouse button is pressed
            bool isMousePressed = Mouse.current.leftButton.isPressed;

            if (isMousePressed)
            {
                if (!isAiming)
                {
                    StartAiming();
                }

                if (CheckTouchInShootArea())
                    UpdateAiming();
                else
                    HideAimVisuals();
            }
            else
            {
                if (isAiming && CheckTouchInShootArea())
                {
                    Shoot();
                }

                StopAiming();
            }
        }

        /// <summary>
        /// Start aiming mode
        /// </summary>
        private void StartAiming()
        {
            isAiming = true;
        }

        /// <summary>
        /// Update aiming direction and trajectory
        /// </summary>
        private void UpdateAiming()
        {
            if (bubbleReadyPool.Current() == null)
                return;

            Vector2 mousePos = Mouse.current.position.ReadValue();
            Vector2 worldPos = mainCamera.ScreenToWorldPoint(mousePos);
            Vector2 currentBubblePos = bubbleReadyPool.Current().transform.position;

            // Calculate aim direction
            aimDirection = (worldPos - currentBubblePos).normalized;

            // Check minimum distance
            float sqrMagnitude = Vector2.SqrMagnitude(worldPos - currentBubblePos);
            if (sqrMagnitude < minAimDistance)
            {
                HideAimVisuals();
                return;
            }

            // Calculate trajectory
            currentTrajectory = trajectoryCalculator.CalculateTrajectory(currentBubblePos, aimDirection);

            // Update aim guide
            if (aimGuide != null)
            {
                aimGuide.UpdateTrajectory(in currentTrajectory);
            }

            // Update preview frame
            UpdatePreviewFrame(in currentTrajectory);
        }

        /// <summary>
        /// Update hexagonal preview frame position
        /// </summary>
        private void UpdatePreviewFrame(in TrajectoryCalculator.TrajectoryResult trajectory)
        {
            if (previewFrame == null || bubbleGrid == null)
                return;

            if (!trajectory.hitBubble)
            {
                previewFrame.Hide();
                return;
            }

            // Calculate placement coordinate
            HexCoordinate targetCoord = CalculatePlacementCoordinate(in trajectory);

            // Check if position is valid (not occupied)
            if (bubbleGrid.IsOccupied(targetCoord))
            {
                // Show red frame for invalid position
                previewFrame.SetColor(new Color(1f, 0f, 0f, 0.5f));
            }
            else
            {
                // Show white frame for valid position
                previewFrame.SetColor(new Color(1f, 1f, 1f, 0.5f));
            }

            previewFrame.ShowAtCoordinate(targetCoord, bubbleGrid.HexSize);
        }

        /// <summary>
        /// Calculate placement coordinate based on collision point and trajectory direction
        /// </summary>
        private HexCoordinate CalculatePlacementCoordinate(in TrajectoryCalculator.TrajectoryResult trajectory)
        {
            Vector2 collisionPoint = trajectory.finalPosition;
            Vector2 hitBubbleCenter = trajectory.hitInfo.transform.position;

            // Calculate direction from hit bubble to collision point
            Vector2 impactDirection = (collisionPoint - hitBubbleCenter).normalized;

            // Move slightly in the direction of impact to find placement position
            // This ensures we find the empty neighbor closest to the impact point
            float offset = bubbleGrid.HexSize * 0.6f; // 60% of hex size
            Vector2 placementSearchPos = collisionPoint + impactDirection * offset;

            // Get hex coordinate at the search position
            HexCoordinate targetCoord = bubbleGrid.GetHexCoordinate(placementSearchPos);

            return targetCoord;
        }

        /// <summary>
        /// Execute shoot action
        /// </summary>
        private void Shoot()
        {
            if (currentTrajectory.points == null || currentTrajectory.points.Length < 2)
            {
                Debug.Log("Invalid trajectory, cannot shoot");
                return;
            }

            StartCoroutine(ShootBubbleCoroutine());
        }

        /// <summary>
        /// Shoot bubble with animation along trajectory
        /// </summary>
        private IEnumerator ShootBubbleCoroutine()
        {
            isShooting = true;

            // Get bubble from pool
            Bubble.Bubble bubble = bubbleReadyPool?.Get();
            if (bubble == null)
            {
                Debug.LogError("Failed to get bubble from pool!");
                isShooting = false;
                yield break;
            }

            // Animate along trajectory
            yield return LaunchBubbleAlongPath(bubble, currentTrajectory.points);

            // Calculate placement position using same logic as preview
            HexCoordinate placementCoord = CalculatePlacementCoordinate(in currentTrajectory);

            if (bubbleReadyPool)
                bubbleReadyPool.Reload();

            // Check if valid placement position was found
            if (default(HexCoordinate).Equals(placementCoord))
            {
                Debug.LogWarning("Grid is full - no valid placement position!");
                bubble.ReturnToPool();
                isShooting = false;
                yield break;
            }

            // Place bubble on grid
            bubbleGrid.PlaceBubble(placementCoord, bubble);

            // Process game logic
            yield return ProcessGameLogic(bubble);

            isShooting = false;
        }

        /// <summary>
        /// Launch bubble animation along path
        /// </summary>
        private IEnumerator LaunchBubbleAlongPath(Bubble.Bubble bubble, Vector2[] pathPoints)
        {
            int pointCount = pathPoints.Length;

            for (int i = 0; i < pointCount - 1; i++)
            {
                Vector2 start = pathPoints[i];
                Vector2 end = pathPoints[i + 1];
                float distance = Vector2.Distance(start, end);
                float duration = distance / shootSpeed;
                float inverseDuration = shootSpeed / distance;

                float elapsed = 0f;
                while (elapsed < duration)
                {
                    elapsed += Time.deltaTime;
                    float t = elapsed * inverseDuration;
                    bubble.transform.position = Vector2.Lerp(start, end, t);
                    yield return null;
                }
            }

            // Snap to final position
            bubble.transform.position = pathPoints[pathPoints.Length - 1];
        }

        /// <summary>
        /// Process game logic after bubble placement
        /// </summary>
        private IEnumerator ProcessGameLogic(Bubble.Bubble placedBubble)
        {
            Debug.Log("========== GAME LOGIC START ==========");
            Debug.Log($"[ProcessGameLogic] Placed bubble at {placedBubble.Coordinate}, Type: {placedBubble.ColorType}");

            // Notify GameManager of placement
            GameManager.Instance?.OnBubblePlaced();

            // Step 1: Check for matches
            Debug.Log("[ProcessGameLogic] Step 1: Checking for matches...");
            var matches = matchDetector.FindMatchingCluster(placedBubble, bubbleGrid);
            Debug.Log($"[ProcessGameLogic] Found {matches.Count} matching bubbles");

            if (matches.Count > 0)
            {
                // Notify GameManager of match
                GameManager.Instance?.OnMatchScored(matches.Count);

                // Step 2: Destroy matched bubbles
                Debug.Log("[ProcessGameLogic] Step 2: Destroying matched bubbles...");
                yield return destructionHandler.DestroyBubbles(matches);
                Debug.Log("[ProcessGameLogic] Destruction complete");

                // Step 3: Check for disconnected bubbles
                Debug.Log("[ProcessGameLogic] Step 3: Checking for disconnected bubbles...");
                var disconnected = gravityChecker.GetDisconnectedBubbles(bubbleGrid);
                Debug.Log($"[ProcessGameLogic] Found {disconnected.Count} disconnected bubbles");

                if (disconnected.Count > 0)
                {
                    // Notify GameManager of fall
                    GameManager.Instance?.OnBubblesFallen(disconnected.Count);

                    // Step 4: Make them fall
                    Debug.Log("[ProcessGameLogic] Step 4: Making bubbles fall...");
                    StartCoroutine(destructionHandler.MakeBubblesFall(disconnected));
                }

                GameManager.Instance.LevelManager.RegenerateDynamicLevel();
            }
            else
            {
                Debug.Log("[ProcessGameLogic] No matches found, skipping destruction");
            }

            Debug.Log("========== GAME LOGIC END ==========\n");
        }

        /// <summary>
        /// Stop aiming mode
        /// </summary>
        private void StopAiming()
        {
            isAiming = false;
            HideAimVisuals();
        }

        /// <summary>
        /// Hide all aim visuals
        /// </summary>
        private void HideAimVisuals()
        {
            if (aimGuide != null)
                aimGuide.Hide();

            if (previewFrame != null)
                previewFrame.Hide();
        }

        /// <summary>
        /// Get current shooter position
        /// </summary>
        public Vector2 GetShooterPosition()
        {
            return shooterTransform.position;
        }

        private bool CheckTouchInShootArea()
        {
            if (!shootArea)
                return true;

            var position = Mouse.current.position.ReadValue();
            Vector2 worldPos = mainCamera.ScreenToWorldPoint(position);
            var bounds = shootArea.bounds;

            return bounds.min.x <= worldPos.x
            && bounds.max.x >= worldPos.x
            && bounds.min.y <= worldPos.y
            && bounds.max.y >= worldPos.y;
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            if (!showDebugGizmos || !isAiming)
                return;

            // Draw trajectory
            if (trajectoryCalculator != null && currentTrajectory.points != null)
            {
                trajectoryCalculator.DrawTrajectoryGizmos(currentTrajectory, Color.cyan);
            }

            // Draw shooter position
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(shooterTransform.position, 0.3f);

            // Draw aim direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(shooterTransform.position, aimDirection * 2f);
        }
#endif
    }
}
