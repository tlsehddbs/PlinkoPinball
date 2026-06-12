using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;

namespace PlinkoPinball.Gameplay.Pinball.Triggers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class BallPassTrigger : MonoBehaviour
    {
        [Header("Event")]
        [SerializeField] private string eventId = "pass.undefined";
        [SerializeField] private int baseValue = 0;
        [SerializeField] private string[] tags;

        [Header("Ball Filter")]
        [SerializeField] private int ballLayer = -1;

        [Header("Rate Limit")]
        [Min(0f)]
        [SerializeField] private float cooldownSeconds = 0.15f;

        [Header("Debug")]
        [SerializeField] private bool logEvents = false;

        private ITableEventReaction[] _localReactions;
        private float _nextAllowedTime;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            _localReactions = GetComponents<ITableEventReaction>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (cooldownSeconds > 0f && Time.time < _nextAllowedTime)
            {
                return;
            }

            Rigidbody ball = other.attachedRigidbody;
            if (ball == null)
            {
                return;
            }

            if (ballLayer >= 0 && ball.gameObject.layer != ballLayer)
            {
                return;
            }

            _nextAllowedTime = Time.time + cooldownSeconds;

            var e = new TableEvent
            {
                eventId = eventId,
                eventType = TableEventType.Pass,
                baseValue = baseValue,
                tags = tags,
                source = transform,
                position = transform.position,
                time = Time.time,
                ball = ball
            };

            if (logEvents)
            {
                Debug.Log($"[BallPassTrigger] {eventId} / source={name}", this);
            }

            TableEventBus.Publish(in e);
            NotifyLocal(in e);
        }

        private void NotifyLocal(in TableEvent e)
        {
            if (_localReactions == null)
            {
                return;
            }

            for (int i = 0; i < _localReactions.Length; i++)
            {
                _localReactions[i]?.OnTableEvent(in e);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            Collider c = GetComponent<Collider>();
            if (c != null)
            {
                c.isTrigger = true;
            }
        }
#endif
    }
}