using UnityEngine;
using BubblePuzzle.Core;

namespace BubblePuzzle.Bubble
{
    /// <summary>
    /// Bubble entity component
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class Bubble : MonoBehaviour
    {
        [SerializeField] private BubbleType bubbleType;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private new Collider2D collider;

        private System.Action<Bubble> onEventReturnPool;
        private HexCoordinate coordinate;
        private bool isPlaced = false;

        public BubbleType Type
        {
            get => bubbleType;
            set
            {
                bubbleType = value;
                UpdateVisual();
            }
        }

        public HexCoordinate Coordinate
        {
            get => coordinate;
            set => coordinate = value;
        }

        public bool IsPlaced
        {
            get => isPlaced;
            set => isPlaced = value;
        }

        private void Start()
        {
            UpdateVisual();
        }

        /// <summary>
        /// Update bubble visual based on type
        /// </summary>
        private void UpdateVisual()
        {
            if (spriteRenderer == null) return;

            // Temporary color mapping (will be replaced with sprites)
            Color color = bubbleType switch
            {
                BubbleType.Red => Color.red,
                BubbleType.Blue => Color.blue,
                BubbleType.Green => Color.green,
                BubbleType.Yellow => Color.yellow,
                BubbleType.Purple => new Color(0.5f, 0f, 0.5f),
                _ => Color.white
            };

            spriteRenderer.color = color;
        }

        /// <summary>
        /// Initialize bubble with type and coordinate
        /// </summary>
        public void Initialize(BubbleType type, HexCoordinate coord)
        {
            Type = type;
            Coordinate = coord;
            isPlaced = false;
        }

        /// <summary>
        /// Reset bubble to default state (for pooling)
        /// </summary>
        public void ResetBubble()
        {
            isPlaced = false;
            coordinate = new HexCoordinate(0, 0);
        }

        public void ReturnToPool()
        {
            if (onEventReturnPool != null)
            {
                ResetBubble();

                onEventReturnPool(this);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public void SetActiveCollider(bool isActive)
        {
            if (collider)
                collider.enabled = isActive;
        }

        public void SetEventReturnPool(System.Action<Bubble> onEvent)
        {
            onEventReturnPool = onEvent;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            UpdateVisual();
        }
#endif
    }
}
