using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Components.Reactions;

namespace PlinkoPinball.Gameplay.Components.Triggers
{
    /// <summary>
    /// 공이 Collider을 통과할 때 Pass event 발행
    /// Lane/Orbit/InLane/OutLane 등
    /// 
    /// - TableEventBus로 글로벌 Publish
    /// - 같은 오브젝트의 로컬 ITableEventReaction들에게 전달 (옵저버)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public sealed class BallPassTrigger : MonoBehaviour
    {
        [Header("Event")]
        [SerializeField] private string eventId = "unset.hit";
        [SerializeField] private int baseValue = 1;
        [SerializeField] private string[] tags;

        [Header("Ball Filter")]
        [Tooltip("공 레이어. -1이면 레이어 필터를 사용하지 않음")]
        [SerializeField] private int ballLayer = -1;

        [Header("Rate Limit")]
        [Min(0f)]
        [SerializeField] private float cooldownSeconds = 0.10f;

        private float _nextAllowedTime;
        private ITableEventReaction[] _reactions;

        private void Reset()
        {
            var c = GetComponent<Collider>();
            if (c != null) c.isTrigger = true;
        }

        private void Awake()
        {
            var c = GetComponent<Collider>();
            if (c != null && !c.isTrigger)
                c.isTrigger = true;

            _reactions = GetComponents<ITableEventReaction>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (cooldownSeconds > 0f && Time.time < _nextAllowedTime) return;

            var rb = other.attachedRigidbody;
            if (rb == null) return;

            if (ballLayer >= 0 && rb.gameObject.layer != ballLayer) return;

            _nextAllowedTime = Time.time + cooldownSeconds;

            var e = new TableEvent
            {
                eventId = eventId,
                eventType = TableEventType.Pass,
                baseValue = baseValue,
                tags = tags,
                source = transform,
                position = other.ClosestPoint(transform.position),
                time = Time.time
            };

            TableEventBus.Publish(in e);
            NotifyLocal(in e);
        }


        private void NotifyLocal(in TableEvent e)
        {
            if (_reactions == null || _reactions.Length == 0) return;

            for (int i = 0; i < _reactions.Length; i++)
                _reactions[i].OnTableEvent(in e);
        }
    }
}
