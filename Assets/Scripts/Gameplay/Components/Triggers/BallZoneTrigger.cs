using System.Collections.Generic;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;
using PlinkoPinball.Gameplay.Utility;
using UnityEngine;


namespace PlinkoPinball.Gameplay.Components.Triggers
{
    [DisallowMultipleComponent]
    public class BallZoneTrigger : MonoBehaviour
    {
        [Header("Event")]
        [SerializeField] private string eventId = "unset.zone.enter";
        [SerializeField] private TableEventType enterEventType = TableEventType.Custom;
        [SerializeField] private TableEventType exitEventType = TableEventType.Custom;
        [SerializeField] private int baseValue = 0;
        [SerializeField] private string[] tags;

        [Header("Behavior")]
        [SerializeField] private bool fireEnter = true;
        [SerializeField] private bool fireExit = true;

        private ITableEventReaction[] _reactions;
        private readonly HashSet<int> _inside = new HashSet<int>(8);


        private void Awake()
        {
            _reactions = GetComponents<ITableEventReaction>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!fireEnter) return;
            if (!BallRigidbodyUnility.TryGetBallRigidbody(other, out var ballRb)) return;

            int id = ballRb.GetInstanceID();
            if (!_inside.Add(id)) return;    // 이미 Zone 안에 있는 경우 중복 enter 방지

            Emit(enterEventType, eventId, ballRb);
        }

        private void OnTriggerExit(Collider other)
        {
            if (!fireExit) return;
            if (!BallRigidbodyUnility.TryGetBallRigidbody(other, out var ballRb)) return;

            int id = ballRb.GetInstanceID();
            if (!_inside.Remove(id)) return;    // ball 추적이 안될 경우 exit 무시

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

            if (_reactions == null || _reactions.Length == 0) return;

            Debug.Log($"reaction count = {_reactions.Length}");

            // for Local Reactions
            for (int i = 0; i < _reactions.Length; i++)
                _reactions[i].OnTableEvent(in e);
        }
    }
}
