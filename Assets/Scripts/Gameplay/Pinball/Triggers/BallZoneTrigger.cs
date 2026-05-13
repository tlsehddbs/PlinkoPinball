using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;

namespace PlinkoPinball.Gameplay.Pinball.Triggers
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Collider))]
    public sealed class BallZoneTrigger : MonoBehaviour
    {
        [Header("Event IDs")]
        [SerializeField] private string enterEventId = "zone.undefined.enter";
        [SerializeField] private string exitEventId = "zone.undefined.exit";
        [SerializeField] private string tickEventId = "zone.undefined.tick";

        [Header("Event")]
        [SerializeField] private int baseValue = 0;
        [SerializeField] private string[] tags;

        [Header("Tick")]
        [SerializeField] private bool emitTickEvent = false;
        [Min(0.05f)]
        [SerializeField] private float tickIntervalSeconds = 0.5f;

        [Header("Ball Filter")]
        [SerializeField] private int ballLayer = -1;

        [Header("Debug")]
        [SerializeField] private bool logEvents = false;

        private readonly Dictionary<Rigidbody, float> _nextTickTimes = new();
        private ITableEventReaction[] _localReactions;

        private void Awake()
        {
            GetComponent<Collider>().isTrigger = true;
            _localReactions = GetComponents<ITableEventReaction>();
        }

        private void OnTriggerEnter(Collider other)
        {
            Rigidbody ball = ResolveBall(other);
            if (ball == null)
            {
                return;
            }

            _nextTickTimes[ball] = Time.time + tickIntervalSeconds;
            Emit(enterEventId, ball);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!emitTickEvent)
            {
                return;
            }

            Rigidbody ball = ResolveBall(other);
            if (ball == null)
            {
                return;
            }

            if (!_nextTickTimes.TryGetValue(ball, out float nextTick))
            {
                _nextTickTimes[ball] = Time.time + tickIntervalSeconds;
                return;
            }

            if (Time.time < nextTick)
            {
                return;
            }

            _nextTickTimes[ball] = Time.time + tickIntervalSeconds;
            Emit(tickEventId, ball);
        }

        private void OnTriggerExit(Collider other)
        {
            Rigidbody ball = ResolveBall(other);
            if (ball == null)
            {
                return;
            }

            Emit(exitEventId, ball);
            _nextTickTimes.Remove(ball);
        }

        private Rigidbody ResolveBall(Collider other)
        {
            Rigidbody ball = other.attachedRigidbody;
            if (ball == null)
            {
                return null;
            }

            if (ballLayer >= 0 && ball.gameObject.layer != ballLayer)
            {
                return null;
            }

            return ball;
        }

        private void Emit(string eventId, Rigidbody ball)
        {
            if (string.IsNullOrWhiteSpace(eventId))
            {
                return;
            }

            var e = new TableEvent
            {
                eventId = eventId,
                eventType = TableEventType.Zone,
                baseValue = baseValue,
                tags = tags,
                source = transform,
                position = transform.position,
                time = Time.time,
                ball = ball
            };

            if (logEvents)
            {
                Debug.Log($"[BallZoneTrigger] {eventId} / source={name}", this);
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

            if (tickIntervalSeconds < 0.05f)
            {
                tickIntervalSeconds = 0.05f;
            }
        }
#endif
    }
}