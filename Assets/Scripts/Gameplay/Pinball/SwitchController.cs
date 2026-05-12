using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Components
{
    [DisallowMultipleComponent]
    public sealed class SwitchController : MonoBehaviour, ITableEventReaction
    {
        public enum OutputMode
        {
            Pulse,
            Pass,
            Toggle
        }


        [Header("Switch Event")]
        [SerializeField] private string switchId = "none";


        [Header("Input Filter(EventId)")]
        [Tooltip("값이 있는 경우, eventId가 이 문자열로 시작할 때만 입력으로 받음")]
        [SerializeField] private string requiredEventIdPrefix = "";

        [Tooltip("값이 있는 경우, eventId가 정확히 일치할 때만 입력으로 받음.")]
        [SerializeField] private string requiredEventIdExact = "";


        [Header("Output")]
        [SerializeField] private OutputMode mode = OutputMode.Pulse;

        [Tooltip("semantic 이벤트의 baseValue. 사용하지 않으면 0으로 유지")]
        [SerializeField] private int semanticBaseVlaue = 0;

        [Tooltip("semantic 이벤트에 포함할 태그")]  // switch, progress, score 등등
        [SerializeField] private string[] semanticTags = new[] { "swtich" };

        [Header("semantic 이벤트 타입")]
        [SerializeField] private TableEventType semanticEventType = TableEventType.Custom;


        [Header("Toggle State")]
        [SerializeField] private bool initialOn = false;


        // [Header("Rate Limit")]
        // [Tooltip("전체 쿨다운(s). 0이면 제한 없음")]
        // [Min(0f)]
        // [SerializeField] private float cooldownSeconds = 0.05f;

        // [Tooltip("연타 방지 쿨다운(s). 0이면 제한 없음")]
        // [Min(0f)]
        // [SerializeField] private float ballDebounceSeconds = 0.05f;


        [Header("Local Forward (optional)")]
        [Tooltip("true일 경우, switch의 이벤트를 같은 오브젝트의 ITableEventReaction에게 다시 전달함")]
        [SerializeField] private bool forwardSemanticToLocal = false;

        // internal state
        private bool _isOn;
        private float _nextAllowedTime;
        private int _lastBallId;
        private float _lastBallTime;

        private ITableEventReaction[] _localReactions;

        // semantic을 로컬로 재전달할 때 이 클래스가 다시 받는 것을 방지
        private bool _emittingSemantic;


        private void Awake()
        {
            _isOn = initialOn;

            if (forwardSemanticToLocal)
            {
                _localReactions = GetComponents<ITableEventReaction>();
            }
        }

        public void OnTableEvent(in TableEvent e)
        {
            // 스위치 이벤트는 입력으로 처리하지 않음
            if (e.eventId != null && e.eventId.StartsWith("switch."))
            {
                return;
            }

            // semantic 발행 중 로컬로 재전달된 이벤트가 돌아오는 케이스 방지
            if(_emittingSemantic) 
            {
                return;
            }

            // 입력 필터(exact 우선 처리, 없을 경우 prefix로 처리)
            if(!string.IsNullOrEmpty(requiredEventIdExact))
            {
                if(e.eventId != requiredEventIdExact) 
                {
                    return;
                }
            }
            else if(!string.IsNullOrEmpty(requiredEventIdPrefix))
            {
                if(e.eventId == null || !e.eventId.StartsWith(requiredEventIdPrefix)) 
                {
                    return;
                }
            }

            if(e.ball == null) 
            {
                return;
            }

            // semantic 이벤트 발행
            switch(mode)
            {
                case OutputMode.Pulse:
                    EmitSemantic("switch.pulse", in e);
                    break;
                
                case OutputMode.Pass:
                    EmitSemantic("switch.pass", in e);
                    break;
                
                case OutputMode.Toggle:
                _isOn = !_isOn;
                    EmitSemantic(_isOn ? "switch.On" : "swtich.off", in e);
                    break;
            }
        }

        private void EmitSemantic(string semanticPrefix, in TableEvent input)
        {
            // switch.<종류>.switchId
            string outId = $"{semanticPrefix}:{switchId}";

            var se = new TableEvent
            {
                eventId = outId,
                eventType = semanticEventType,
                baseValue = semanticBaseVlaue,
                tags = semanticTags,
                source = transform,
                position = input.position,
                time = Time.time,
                ball = input.ball
            };
            
            TableEventBus.Publish(in se);

            if(forwardSemanticToLocal && _localReactions != null && _localReactions.Length > 0)
            {
                _emittingSemantic = true;
                try
                {
                    for (int i = 0; i < _localReactions.Length; i++)
                    {
                        // _emittingSemantic 플래그로 무한 루프 방지
                        _localReactions[i].OnTableEvent(in se);
                    }
                }
                finally
                {
                    _emittingSemantic = false;
                }
            }
        }
    }
}