using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BubblePuzzle.Bubble
{
    /// <summary>
    /// Object pooling for bubbles (performance optimization)
    /// </summary>
    public class BubbleReadyPool : MonoBehaviour
    {
        public bool IsReloading { get; private set; }
        private Queue<Bubble> readyBubbles = new Queue<Bubble>();
        private readonly int CIRCLE_RADIUS = 1;

        public void Reload()
        {
            int currentCount = readyBubbles.Count;
            for (int num = currentCount; num < IntDefine.MAX_READY_POOL_SIZE; num++)
            {
                Bubble bubble = BubblePoolManager.Instance.GetBubble();

                // Initialize bubble with random color (temp)
                BubbleType randomType = (BubbleType)Random.Range(0, 5);
                bubble.Initialize(randomType, new BubblePuzzle.Core.HexCoordinate(0, 0));
                bubble.SetReadyFire();
                bubble.gameObject.SetActive(true);

                InitPosition(bubble.transform);

                readyBubbles.Enqueue(bubble);
            }

            StartCoroutine(Replace());
        }

        private void InitPosition(Transform bubbleTransform)
        {
            if (bubbleTransform == null)
                return;

            float angleStep = 360f / IntDefine.MAX_READY_POOL_SIZE;

            // 시작 각도: -90도
            float angle = -90f + (angleStep * (IntDefine.MAX_READY_POOL_SIZE - 1));
            float radians = angle * Mathf.Deg2Rad;

            // 원형 좌표 계산
            float x = transform.position.x + CIRCLE_RADIUS * Mathf.Cos(radians);
            float y = transform.position.y + CIRCLE_RADIUS * Mathf.Sin(radians);

            bubbleTransform.position = new Vector3(x, y, 0f);
        }

        public IEnumerator Replace()
        {
            if (readyBubbles == null || readyBubbles.Count == 0)
                yield break;

            if (IsReloading)
                yield break;

            IsReloading = true;
            Bubble[] rotateBubbles = new Bubble[readyBubbles.Count];
            float angleStep = 360f / IntDefine.MAX_READY_POOL_SIZE;
            float animationDuration = 1f; // 1초 동안 애니메이션

            // 회전 애니메이션을 진행할 버블 배열 생성 및 시작/목표 위치 저장
            Vector3[] startPositions = new Vector3[rotateBubbles.Length];
            Vector3[] targetPositions = new Vector3[rotateBubbles.Length];

            int index = 0;
            foreach (var bubble in readyBubbles)
            {
                rotateBubbles[index] = bubble;

                // 시작 위치: 현재 위치 (90도 수직 위)
                startPositions[index] = bubble.transform.position;

                // 목표 각도: 90도에서 반시계방향으로 회전 (angleStep만큼 빼기)
                float targetAngle = 90f - (angleStep * index);
                float radians = targetAngle * Mathf.Deg2Rad;

                // 목표 위치 계산
                float x = transform.position.x + CIRCLE_RADIUS * Mathf.Cos(radians);
                float y = transform.position.y + CIRCLE_RADIUS * Mathf.Sin(radians);
                targetPositions[index] = new Vector3(x, y, 0f);

                index++;
            }

            // 큐 재구성 (회전 후 순서 반영)
            readyBubbles.Clear();
            foreach (var bubble in rotateBubbles)
            {
                readyBubbles.Enqueue(bubble);
            }

            // 애니메이션 실행
            float elapsed = 0f;
            while (elapsed < animationDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / animationDuration;

                // Ease-out 곡선 적용 (부드러운 감속)
                float smoothT = 1f - Mathf.Pow(1f - t, 3f);

                // 모든 버블 동시에 이동
                for (int i = 0; i < rotateBubbles.Length; i++)
                {
                    if (rotateBubbles[i] != null)
                    {
                        rotateBubbles[i].transform.position = Vector3.Lerp(startPositions[i], targetPositions[i], smoothT);
                    }
                }

                yield return null;
            }

            // 최종 위치 보정
            for (int i = 0; i < rotateBubbles.Length; i++)
            {
                if (rotateBubbles[i] != null)
                {
                    rotateBubbles[i].transform.position = targetPositions[i];
                }
            }

            IsReloading = false;
        }

        public Bubble Fire()
        {
            if (readyBubbles == null || readyBubbles.Count == 0)
                return null;

            Bubble bubble = readyBubbles.Dequeue();
            bubble.SetFire();

            return bubble;
        }

        public Bubble GetCurrent()
        {
            if (readyBubbles == null || readyBubbles.Count == 0)
                return null;

            return readyBubbles.Peek();
        }
    }
}
