using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Compression
{
    /// <summary>
    /// UI RectTransform 공간에서 Reserve Ball들의 간단한 물리 시뮬레이션을 수행
    /// 실제 Unity Physics를 사용하지 않고, UI 표현용 위치/속도/충돌만 계산
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CompressionReserveBallField : MonoBehaviour
    {
        [Header("Container")]
        [SerializeField] private RectTransform container;

        [Header("Physics")]
        [SerializeField] private float gravity = -700f;
        [SerializeField] private float wallBounce = 0.45f;
        [SerializeField] private float ballBounce = 0.35f;
        [SerializeField] private float friction = 0.985f;
        [SerializeField] private float sleepSpeed = 6f;

        [Header("Stability")]
        [SerializeField] private int solverIterations = 2;

        private readonly List<CompressionReserveBall> balls = new();

        private void Reset()
        {
            container = GetComponent<RectTransform>();
        }

        private void Update()
        {
            if (container == null || balls.Count == 0)
            {
                return;
            }

            float dt = Time.unscaledDeltaTime;

            Integrate(dt);
            SolveWallCollisions();
            SolveBallCollisions();
            ApplySleep();
        }

        public void AddBall(CompressionReserveBall ball, Vector2 anchoredPosition, Vector2 velocity)
        {
            if (ball == null)
            {
                return;
            }

            ball.RectTransform.anchoredPosition = anchoredPosition;
            ball.Velocity = velocity;
            ball.Wake();

            if (!balls.Contains(ball))
            {
                balls.Add(ball);
            }
        }

        public void Clear()
        {
            balls.Clear();
        }

        private void Integrate(float dt)
        {
            for (int i = 0; i < balls.Count; i++)
            {
                CompressionReserveBall ball = balls[i];

                if (ball == null || ball.IsSleeping)
                {
                    continue;
                }

                Vector2 velocity = ball.Velocity;
                velocity.y += gravity * dt;
                velocity.x *= friction;

                Vector2 position = ball.RectTransform.anchoredPosition;
                position += velocity * dt;

                ball.Velocity = velocity;
                ball.RectTransform.anchoredPosition = position;
            }
        }

        private void SolveWallCollisions()
        {
            Rect rect = container.rect;

            float left = rect.xMin;
            float right = rect.xMax;
            float bottom = rect.yMin;
            float top = rect.yMax;

            for (int i = 0; i < balls.Count; i++)
            {
                CompressionReserveBall ball = balls[i];

                if (ball == null)
                {
                    continue;
                }

                Vector2 position = ball.RectTransform.anchoredPosition;
                Vector2 velocity = ball.Velocity;
                float radius = ball.Radius;

                if (position.x - radius < left)
                {
                    position.x = left + radius;
                    velocity.x = Mathf.Abs(velocity.x) * wallBounce;
                    ball.Wake();
                }
                else if (position.x + radius > right)
                {
                    position.x = right - radius;
                    velocity.x = -Mathf.Abs(velocity.x) * wallBounce;
                    ball.Wake();
                }

                if (position.y - radius < bottom)
                {
                    position.y = bottom + radius;
                    velocity.y = Mathf.Abs(velocity.y) * wallBounce;
                    velocity.x *= friction;
                    ball.Wake();
                }
                else if (position.y + radius > top)
                {
                    position.y = top - radius;
                    velocity.y = -Mathf.Abs(velocity.y) * wallBounce;
                    ball.Wake();
                }

                ball.RectTransform.anchoredPosition = position;
                ball.Velocity = velocity;
            }
        }

        private void SolveBallCollisions()
        {
            for (int iteration = 0; iteration < solverIterations; iteration++)
            {
                for (int i = 0; i < balls.Count; i++)
                {
                    CompressionReserveBall a = balls[i];

                    if (a == null)
                    {
                        continue;
                    }

                    for (int j = i + 1; j < balls.Count; j++)
                    {
                        CompressionReserveBall b = balls[j];

                        if (b == null)
                        {
                            continue;
                        }

                        ResolvePair(a, b);
                    }
                }
            }
        }

        private void ResolvePair(CompressionReserveBall a, CompressionReserveBall b)
        {
            Vector2 pa = a.RectTransform.anchoredPosition;
            Vector2 pb = b.RectTransform.anchoredPosition;

            Vector2 delta = pb - pa;
            float distance = delta.magnitude;
            float minDistance = a.Radius + b.Radius;

            if (distance >= minDistance)
            {
                return;
            }

            Vector2 normal = distance > 0.001f ? delta / distance : Random.insideUnitCircle.normalized;

            float penetration = minDistance - distance;
            Vector2 correction = normal * (penetration * 0.5f);

            pa -= correction;
            pb += correction;

            Vector2 va = a.Velocity;
            Vector2 vb = b.Velocity;

            float relativeVelocity = Vector2.Dot(vb - va, normal);

            if (relativeVelocity < 0f)
            {
                float impulse = -(1f + ballBounce) * relativeVelocity * 0.5f;
                Vector2 impulseVector = normal * impulse;

                va -= impulseVector;
                vb += impulseVector;
            }

            a.RectTransform.anchoredPosition = pa;
            b.RectTransform.anchoredPosition = pb;

            a.Velocity = va;
            b.Velocity = vb;

            a.Wake();
            b.Wake();
        }

        private void ApplySleep()
        {
            Rect rect = container.rect;
            float bottom = rect.yMin;

            for (int i = 0; i < balls.Count; i++)
            {
                CompressionReserveBall ball = balls[i];

                if (ball == null)
                {
                    continue;
                }

                Vector2 position = ball.RectTransform.anchoredPosition;
                bool touchingBottom = position.y - ball.Radius <= bottom + 0.5f;

                if (touchingBottom && ball.Velocity.sqrMagnitude <= sleepSpeed * sleepSpeed)
                {
                    ball.Sleep();
                }
            }
        }
    }
}
