using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Core.Utility;
using PlinkoPinball.Gameplay.Core;

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
        [SerializeField] private bool autoGenerateEventId = true;
        [SerializeField] private string eventId = "hit.none";
        [SerializeField] private int baseValue = 1;
        [SerializeField] private string[] tags;

        // 모듈에 포함되어 있는 트리거의 경우 모듈의 source 위치를 파악하기 위해 사용함
        [SerializeField] private Transform eventSourceOverride;

        [Header("Ball Filter")]
        [Tooltip("공 레이어. -1이면 레이어 필터를 사용하지 않음")]
        [SerializeField] private int ballLayer = -1;

        [Header("Rate Limit")]
        [Tooltip("연타 방지 쿨다운(s). 0이면 제한 없음")]
        [Min(0f)]
        [SerializeField] private float cooldownSeconds = 0.05f;

        private float _nextAllowedTime;
        private ITableEventReaction[] _localReactions;


        private void Awake()
        {
            // 로컬 옵저버 캐싱(성능, 가독 향상)
            _localReactions = GetComponents<ITableEventReaction>();
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (cooldownSeconds > 0f && Time.time < _nextAllowedTime)
                return;

            // 공 판별 (RigidBody가 있어야 함. (설정 시) 레이어가 맞아야 함)
            var ballRb = collision.rigidbody;
            if (ballRb == null)
                return;

            if (ballLayer >= 0 && ballRb.gameObject.layer != ballLayer)
                return;

            _nextAllowedTime = Time.time + cooldownSeconds;

            var pos = collision.contactCount > 0 ? collision.GetContact(0).point : ballRb.worldCenterOfMass;

            var e = new TableEvent
            {
                eventId = eventId,
                eventType = TableEventType.Hit,
                baseValue = baseValue,
                tags = tags,
                source = (eventSourceOverride != null) ? eventSourceOverride : transform,
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
            if (_localReactions == null || _localReactions.Length == 0)
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                //Debug.Log($"[BallHitTrigger:{name}] No local reactions.", this);
#endif
                return;
            }

#if UNITY_EDITOR || DEVELOPMENT_BUILD
            //Debug.Log($"[BallHitTrigger:{name}] reaction count={_localReactions.Length}");
#endif

            for (int i = 0; i < _localReactions.Length; i++)
            {
                _localReactions[i].OnTableEvent(in e);
            }
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (autoGenerateEventId)
            {
                eventId = TableIdentityGenerator.CreateEventId("hit", transform);
            }
        }
#endif
    }
}
