using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;

namespace PlinkoPinball.Gameplay.Pinball
{
    /// <summary>
    /// Slingshot / kicker style pinball part.
    /// Detects ball contact, applies an impulse, publishes a TableEvent, and notifies local reactions.
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class PinballSling : MonoBehaviour
    {
        private enum DirectionMode
        {
            TransformForward,
            AwayFromAnchor,
            LocalVector
        }

        [Header("Event")]
        [SerializeField] private string eventId = TableEventIds.HitSling;
        [SerializeField, Min(0)] private int baseValue = 5;
        [SerializeField] private string[] tags = { TableEventTags.Score, TableEventTags.Compression, TableEventTags.Sling };
        [SerializeField] private Transform eventSourceOverride;

        [Header("Ball Filter")]
        [Tooltip("Ball layer. -1 disables layer filtering.")]
        [SerializeField] private int ballLayer = -1;

        [Header("Impulse")]
        [SerializeField] private DirectionMode directionMode = DirectionMode.TransformForward;
        [SerializeField] private Transform directionAnchor;
        [SerializeField] private Vector3 localDirection = Vector3.forward;
        [SerializeField, Min(0f)] private float impulseStrength = 28f;
        [SerializeField] private float upwardBias = 0.05f;
        [SerializeField] private ForceMode forceMode = ForceMode.Impulse;

        [Header("Rate Limit")]
        [SerializeField, Min(0f)] private float cooldownSeconds = 0.06f;

        [Header("Debug")]
        [SerializeField] private bool logEvents;

        private float _nextAllowedTime;
        private ITableEventReaction[] _localReactions;

        private void Awake()
        {
            _localReactions = GetComponents<ITableEventReaction>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            Rigidbody ball = collision.rigidbody;
            Vector3 contactPoint = collision.contactCount > 0 ? collision.GetContact(0).point : transform.position;
            TryFire(ball, contactPoint);
        }

        private void OnTriggerEnter(Collider other)
        {
            TryFire(other.attachedRigidbody, other.ClosestPoint(transform.position));
        }

        private void TryFire(Rigidbody ball, Vector3 contactPoint)
        {
            if (ball == null)
            {
                return;
            }

            if (ballLayer >= 0 && ball.gameObject.layer != ballLayer)
            {
                return;
            }

            if (cooldownSeconds > 0f && Time.time < _nextAllowedTime)
            {
                return;
            }

            _nextAllowedTime = Time.time + cooldownSeconds;

            Vector3 impulseDirection = ResolveImpulseDirection(ball);
            if (impulseDirection.sqrMagnitude < 0.0001f)
            {
                impulseDirection = transform.forward;
            }

            ball.AddForce(impulseDirection.normalized * impulseStrength, forceMode);

            TableEvent tableEvent = new TableEvent
            {
                eventId = eventId,
                eventType = TableEventType.Hit,
                baseValue = baseValue,
                tags = tags,
                source = eventSourceOverride != null ? eventSourceOverride : transform,
                position = contactPoint,
                time = Time.time,
                ball = ball
            };

            TableEventBus.Publish(in tableEvent);
            NotifyLocal(in tableEvent);

            if (logEvents)
            {
                Debug.Log($"[PinballSling] {eventId} ball={ball.name}", this);
            }
        }

        private Vector3 ResolveImpulseDirection(Rigidbody ball)
        {
            Vector3 direction;

            switch (directionMode)
            {
                case DirectionMode.AwayFromAnchor:
                    Transform anchor = directionAnchor != null ? directionAnchor : transform;
                    direction = ball.worldCenterOfMass - anchor.position;
                    break;

                case DirectionMode.LocalVector:
                    direction = transform.TransformDirection(localDirection);
                    break;

                default:
                    Transform forwardSource = directionAnchor != null ? directionAnchor : transform;
                    direction = forwardSource.forward;
                    break;
            }

            if (!Mathf.Approximately(upwardBias, 0f))
            {
                direction += Vector3.up * upwardBias;
            }

            return direction;
        }

        private void NotifyLocal(in TableEvent tableEvent)
        {
            if (_localReactions == null)
            {
                return;
            }

            for (int i = 0; i < _localReactions.Length; i++)
            {
                _localReactions[i]?.OnTableEvent(in tableEvent);
            }
        }
    }
}
