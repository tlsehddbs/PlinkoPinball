using UnityEngine;
using PlinkoPinball.InputRuntime;

namespace PlinkoPinball.Pinball
{
    public enum FlipperSide
    {
        Left,
        Right
    }

    
    [RequireComponent(typeof(Rigidbody))]
    public sealed class PinballFlipper : MonoBehaviour
    {
        [Header("Actuator")]
        [SerializeField] private FlipperSide side;

        [Header("Rotation")]
        [SerializeField] private Vector3 localRotationAxis = Vector3.up;
        [SerializeField] private float restAngle = -25f;
        [SerializeField] private float activatedAngle = 35f;
        [SerializeField] private float activationDuration = 0.035f;
        [SerializeField] private float returnDuration = 0.065f;

        [Header("Ball Assist")]
        [SerializeField] private bool applyBallImpulseAssist = true;
        [SerializeField] private float impulseStrength = 22f;

        private Rigidbody rb;
        private Quaternion baseLocalRotation;
        private float currentAngle;
        private float targetAngle;
        private bool isActivated;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousSpeculative;

            baseLocalRotation = transform.localRotation;
            currentAngle = restAngle;
            targetAngle = restAngle;

            ApplyRotation(restAngle);
        }

        private void OnEnable()
        {
            if (InputRouter.Instance == null)
            {
                return;
            }

            if (side == FlipperSide.Left)
            {
                InputRouter.Instance.OnLeftFlipperPressed += Activate;
                InputRouter.Instance.OnLeftFlipperReleased += Deactivate;
            }
            else
            {
                InputRouter.Instance.OnRightFlipperPressed += Activate;
                InputRouter.Instance.OnRightFlipperReleased += Deactivate;
            }
        }

        private void OnDisable()
        {
            if (InputRouter.Instance == null)
            {
                return;
            }

            if (side == FlipperSide.Left)
            {
                InputRouter.Instance.OnLeftFlipperPressed -= Activate;
                InputRouter.Instance.OnLeftFlipperReleased -= Deactivate;
            }
            else
            {
                InputRouter.Instance.OnRightFlipperPressed -= Activate;
                InputRouter.Instance.OnRightFlipperReleased -= Deactivate;
            }
        }

        private void FixedUpdate()
        {
            float duration = isActivated ? activationDuration : returnDuration;
            float maxDelta = Mathf.Abs(activatedAngle - restAngle) / Mathf.Max(0.001f, duration);

            currentAngle = Mathf.MoveTowards(currentAngle, targetAngle, maxDelta * Time.fixedDeltaTime);

            ApplyRotation(currentAngle);
        }

        private void Activate()
        {
            isActivated = true;
            targetAngle = activatedAngle;
        }

        private void Deactivate()
        {
            isActivated = false;
            targetAngle = restAngle;
        }

        private void ApplyRotation(float angle)
        {
            Quaternion offset = Quaternion.AngleAxis(angle, localRotationAxis.normalized);
            rb.MoveRotation(transform.parent != null ? transform.parent.rotation * baseLocalRotation * offset : baseLocalRotation * offset);
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (!applyBallImpulseAssist || !isActivated)
            {
                return;
            }

            Rigidbody ballRb = collision.rigidbody;

            if (ballRb == null)
            {
                return;
            }

            Vector3 impulseDirection = transform.forward.normalized;
            ballRb.AddForce(impulseDirection * impulseStrength, ForceMode.Impulse);
        }
    }
}
