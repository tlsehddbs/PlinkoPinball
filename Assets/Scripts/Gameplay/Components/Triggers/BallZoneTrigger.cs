using System.Collections.Generic;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Core.Utility;
using PlinkoPinball.Gameplay.Core;
using PlinkoPinball.Gameplay.Utility;
using UnityEngine;


namespace PlinkoPinball.Gameplay.Components.Triggers
{
    [DisallowMultipleComponent]
    public class BallZoneTrigger : MonoBehaviour
    {
        [Header("Event")]
        [SerializeField] private bool autoGenerateEventId = true;
        [SerializeField] private string eventId = "zone.none";
        [SerializeField] private TableEventType enterEventType = TableEventType.Custom;
        [SerializeField] private TableEventType exitEventType = TableEventType.Custom;
        [SerializeField] private int baseValue = 0;
        [SerializeField] private string[] tags;

        [Header("Behavior")]
        [SerializeField] private bool fireEnter = true;
        [SerializeField] private bool fireExit = true;

        private ITableEventReaction[] _localReactions;
        private readonly HashSet<int> _inside = new HashSet<int>(8);


        private void Reset()
        {
            var c = GetComponent<Collider>();
            if (c != null)
                c.isTrigger = true;
        }

        private void Awake()
        {
            var c = GetComponent<Collider>();
            if (c != null && !c.isTrigger)
                c.isTrigger = true;

            _localReactions = GetComponents<ITableEventReaction>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!fireEnter)
                return;

            if (!BallRigidbodyUnility.TryGetBallRigidbody(other, out var ballRb))
                return;

            int id = ballRb.GetInstanceID();

            // 이미 Zone 안에 있는 경우 중복 enter 방지
            if (!_inside.Add(id))
                return;

            string enterEventId = TableIdentityGenerator.CreateDerivedEventId(eventId, "enter");
            Emit(enterEventType, eventId, ballRb);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!fireExit)
                return;

            if (!BallRigidbodyUnility.TryGetBallRigidbody(other, out var ballRb))
                return;

            int id = ballRb.GetInstanceID();

            // ball 추적이 안될 경우 exit 무시
            if (!_inside.Remove(id))
                return;

            string exitEventId = TableIdentityGenerator.CreateDerivedEventId(eventId, "exit");
            Emit(exitEventType, eventId, ballRb);
        }

        private void Emit(TableEventType type, string id, Rigidbody ballRb)
        {
            var e = new TableEvent
            {
                eventId = id,
                eventType = type,
                baseValue = baseValue,
                tags = tags,
                source = transform,
                position = transform.position,
                time = Time.time,
                ball = ballRb
            };

            TableEventBus.Publish(in e);

            if (_localReactions == null || _localReactions.Length == 0)
                return;

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            Debug.Log($"reaction count = {_localReactions.Length}");
#endif

            for (int i = 0; i < _localReactions.Length; i++)
                _localReactions[i].OnTableEvent(in e);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (autoGenerateEventId)
                eventId = TableIdentityGenerator.CreateEventId("zone", transform);

            var c = GetComponent<Collider>();
            if (c != null && !c.isTrigger)
                c.isTrigger = true;
        }
#endif
    }
}
