using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Components.Reactions;

namespace PlinkoPinball.Gameplay.Components.Triggers
{
    /// <summary>
    /// 공이 Collider와 충돌할 때 Hit event 발행
    /// 
    /// - TableEventBus로 글로벌 Publish
    /// - 같은 오브젝트의 로컬 ITableEventReaction들에게 전달 (옵저버)
    /// </summary>
    [RequireComponent(typeof(Collider))]
    [DisallowMultipleComponent]
    public sealed class BallHitTrigger : MonoBehaviour
    {
        [Header("Event")]
        [SerializeField] private string eventId = "unset.hit";
        [SerializeField] private int baseValue = 1;
        [SerializeField] private string[] tags;

        [Header("Ball Filter")]
        [Tooltip("공 레이어. -1이면 레이어 필터를 사용하지 않음")]
        [SerializeField] private int ballLayer = -1;

        [Header("Rate Limit")]
        [Tooltip("연타 방지 쿨다운(s). 0이면 제한 없음")]
        [Min(0f)]
        [SerializeField] private float cooldownSeconds = 0.05f;

        private float _nextAllowedTime;
        private ITableEventReaction[] _reactions;


        private void Awake()
        {
            // 로컬 옵저버 캐싱(성능, 가독 향상)
            _reactions = GetComponents<ITableEventReaction>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (cooldownSeconds > 0f && Time.time < _nextAllowedTime) return;

            // 공 판별 (RigidBody가 있어야 함. (설정 시) 레이어가 맞아야 함)
            var ballRb = collision.rigidbody;
            if (ballRb == null) return;

            if (ballLayer >= 0 && ballRb.gameObject.layer != ballLayer) return;

            _nextAllowedTime = Time.time + cooldownSeconds;

            var pos = collision.contactCount > 0 ? collision.GetContact(0).point : ballRb.worldCenterOfMass;

            var e = new TableEvent
            {
                eventId = eventId,
                eventType = TableEventType.Hit,
                baseValue = baseValue,
                tags = tags,
                source = transform,
                position = pos,
                time = Time.time,
                ball = ballRb
            };

            // global publish(for 규칙, 점수 등)
            TableEventBus.Publish(in e);

            // local notify(오브젝트 단위 연출, 물리 등)
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
