using UnityEngine;

namespace PlinkoPinball.Gameplay
{
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PinballBallPhysicsTuner : MonoBehaviour
    {
        [Tooltip("Table-scale gravity multiplier applied on top of Physics.gravity. Does not modify global gravity.")]
        [SerializeField, Min(1f)] private float gravityMultiplier = 3.5f;

        [Tooltip("Maximum linear speed for the pinball ball. Set to 0 to disable clamping.")]
        [SerializeField, Min(0f)] private float maxLinearSpeed = 42f;

        [Tooltip("When enabled, extra gravity is applied only while the ball is falling.")]
        [SerializeField] private bool applyExtraGravityOnlyWhenDescending = true;

        private Rigidbody ball;

        public float GravityMultiplier => gravityMultiplier;
        public float MaxLinearSpeed => maxLinearSpeed;
        public bool ApplyExtraGravityOnlyWhenDescending => applyExtraGravityOnlyWhenDescending;

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
