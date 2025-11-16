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
        [SerializeField] private BubbleColorType bubbleColorType;
        [SerializeField] private BubbleType bubbleType;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private new Collider2D collider;
        [SerializeField] private GameObject[] bubbleStates;

        private System.Action<Bubble> onEventReturnPool;
        private HexCoordinate coordinate;
        private bool isPlaced = false;

        public BubbleColorType ColorType
        {
            get => bubbleColorType;
            set
            {
                bubbleColorType = value;
                UpdateVisual();
            }
        }

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
            UpdateBubbleColor();

            UpdateBubbleState();
        }

        private void UpdateBubbleColor()
        {
            if (spriteRenderer == null) return;

            // Temporary color mapping (will be replaced with sprites)
            Color color = bubbleColorType switch
            {
                BubbleColorType.Red => Color.red,
                BubbleColorType.Blue => Color.blue,
                BubbleColorType.Green => Color.green,
                BubbleColorType.Yellow => Color.yellow,
                BubbleColorType.Purple => new Color(0.5f, 0f, 0.5f),
                _ => Color.white
            };

            spriteRenderer.color = color;
        }

        private void UpdateBubbleState()
        {
            for (BubbleType type = BubbleType.None;
            System.Enum.IsDefined(typeof(BubbleType), type);
            type++)
            {
                int index = (int)type;

                if (bubbleStates.Length <= index)
                    break;

                bubbleStates[index].SetActive(type == bubbleType);
            }
        }

        /// <summary>
        /// Initialize bubble with type and coordinate
        /// </summary>
        public void Initialize(BubbleColorType colorType, BubbleType type, HexCoordinate coord)
        {
            ColorType = colorType;
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
