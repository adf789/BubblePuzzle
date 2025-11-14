using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

namespace BubblePuzzle.Bubble
{
    /// <summary>
    /// Object pooling for bubbles (performance optimization)
    /// </summary>
    public class BubblePoolManager : MonoBehaviour
    {
        private static BubblePoolManager instance;
        public static BubblePoolManager Instance => instance;
        public int PoolCount => pool.Count;

        [Header("Pool Settings")]
        [SerializeField] private Bubble bubblePrefab;
        [SerializeField] private int initialPoolSize = 50;
        [SerializeField] private Transform poolParent;

        private Queue<Bubble> pool = new Queue<Bubble>();

#if UNITY_EDITOR
        private int createCount = 0;
#endif

        void Awake()
        {
            instance = this;
        }

        /// <summary>
        /// Create initial pool
        /// </summary>
        public void InitializePool()
        {
            if (bubblePrefab == null)
            {
                Debug.LogError("Bubble prefab not assigned!");
                return;
            }

            if (poolParent == null)
            {
                GameObject poolObj = new GameObject("BubblePool");
                poolParent = poolObj.transform;
                poolParent.SetParent(transform);
            }

            for (int i = 0; i < initialPoolSize; i++)
            {
                CreateNewBubble();
            }

            Debug.Log($"Bubble pool initialized with {initialPoolSize} bubbles");
        }

        /// <summary>
        /// Create new bubble and add to pool
        /// </summary>
        private Bubble CreateNewBubble()
        {
            Bubble bubble = Instantiate(bubblePrefab, poolParent);
            bubble.gameObject.SetActive(false);

            pool.Enqueue(bubble);

#if UNITY_EDITOR
            bubble.gameObject.name = $"Bubble {++createCount}";
#endif

            return bubble;
        }

        /// <summary>
        /// Get bubble from pool
        /// </summary>
        public Bubble GetBubble()
        {
            Bubble bubble;

            if (PoolCount > 0)
            {
                bubble = pool.Dequeue();
            }
            else
            {
                Debug.LogWarning("Pool empty, creating new bubble");
                bubble = CreateNewBubble();
            }

            bubble.gameObject.SetActive(true);
            return bubble;
        }

        /// <summary>
        /// Return bubble to pool
        /// </summary>
        public void ReturnBubble(Bubble bubble)
        {
            if (bubble == null) return;

            Bubble bubbleComponent = bubble.GetComponent<Bubble>();
            if (bubbleComponent != null)
            {
                bubbleComponent.ResetBubble();
            }

            bubble.gameObject.SetActive(false);
            bubble.transform.SetParent(poolParent);
            pool.Enqueue(bubble);
        }

        /// <summary>
        /// Get current pool size
        /// </summary>
        public int GetPoolSize()
        {
            return pool.Count;
        }
    }
}
