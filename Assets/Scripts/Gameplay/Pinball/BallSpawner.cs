using UnityEngine;

namespace PlinkoPinball.Gameplay
{
    /// <summary>
    /// 공 생성/재생성 담당
    /// </summary>
    public sealed class BallSpawner : MonoBehaviour
    {
        [Header("Ball")]
        [SerializeField] private Rigidbody ballPrefab;
        [SerializeField] private Transform spawnPoint;

        [Header("Physics Tuning")]
        [Tooltip("테이블 스케일 보정용 핀볼 공 중력 배수. 전역 Physics.gravity는 변경하지 않는다.")]
        [SerializeField, Min(1f)] private float gravityMultiplier = 3.5f;

        [Tooltip("과도한 가속으로 충돌이 불안정해지는 것을 막는 최대 속도. 0이면 제한하지 않는다.")]
        [SerializeField, Min(0f)] private float maxLinearSpeed = 42f;

        [Tooltip("켜면 공이 올라가는 동안에는 추가 중력을 적용하지 않는다.")]
        [SerializeField] private bool applyExtraGravityOnlyWhenDescending = true;

        public Rigidbody CurrentBall { get; private set; }

        public Rigidbody Spawn()
        {
            if (ballPrefab == null || spawnPoint == null)
            {
                Debug.LogError("[BallSpawner] Prefab or SpawnPoint is null");
                return null;
            }

            if (CurrentBall != null)
            {
                Destroy(CurrentBall.gameObject);
            }

            CurrentBall = Instantiate(ballPrefab, spawnPoint.position, spawnPoint.rotation);
            ConfigureBallPhysics(CurrentBall);
            return CurrentBall;
        }

        public void DespawnCurrent()
        {
            if (CurrentBall == null) 
            {
                return;
            }
            
            Destroy(CurrentBall.gameObject);
            CurrentBall = null;
        }

        private void ConfigureBallPhysics(Rigidbody ball)
        {
            if (ball == null)
            {
                return;
            }

            PinballBallPhysicsTuner tuner = ball.GetComponent<PinballBallPhysicsTuner>();
            if (tuner == null)
            {
                tuner = ball.gameObject.AddComponent<PinballBallPhysicsTuner>();
            }

            tuner.Configure(gravityMultiplier, maxLinearSpeed, applyExtraGravityOnlyWhenDescending);
        }
    }

    [RequireComponent(typeof(Rigidbody))]
    public sealed class PinballBallPhysicsTuner : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float gravityMultiplier = 3.5f;
        [SerializeField, Min(0f)] private float maxLinearSpeed = 42f;
        [SerializeField] private bool applyExtraGravityOnlyWhenDescending = true;

        private Rigidbody ball;

        public void Configure(float newGravityMultiplier, float newMaxLinearSpeed, bool descendingOnly)
        {
            gravityMultiplier = Mathf.Max(1f, newGravityMultiplier);
            maxLinearSpeed = Mathf.Max(0f, newMaxLinearSpeed);
            applyExtraGravityOnlyWhenDescending = descendingOnly;
        }

        private void Awake()
        {
            ball = GetComponent<Rigidbody>();
        }

        private void FixedUpdate()
        {
            if (ball == null)
            {
                return;
            }

            if (ball.useGravity && gravityMultiplier > 1f && ShouldApplyExtraGravity())
            {
                ball.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
            }

            if (maxLinearSpeed <= 0f)
            {
                return;
            }

            Vector3 velocity = ball.linearVelocity;
            float maxSpeedSqr = maxLinearSpeed * maxLinearSpeed;
            if (velocity.sqrMagnitude > maxSpeedSqr)
            {
                ball.linearVelocity = velocity.normalized * maxLinearSpeed;
            }
        }

        private bool ShouldApplyExtraGravity()
        {
            if (!applyExtraGravityOnlyWhenDescending)
            {
                return true;
            }

            return Vector3.Dot(ball.linearVelocity, -Physics.gravity.normalized) <= 0f;
        }
    }
}
