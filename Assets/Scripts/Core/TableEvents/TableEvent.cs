using System;
using System.Linq;
using UnityEditor.Rendering.LookDev;
using UnityEngine;

namespace PlinkoPinball.Core.TableEvents
{
    [Serializable]
    public struct TableEvent
    {
        public string eventId;
        public TableEventType eventType;
        public int baseValue;       // 점수 계산에 활용할 각 요소별 기본 점수   
        public string[] tags;       // score, gate, bonus, timebonus, penalty 등을 위한 tag

        public Transform source;    // 이벤트를 발생시키게 한 오브젝트 (모듈 내부 요소 추적 등에 사용 예정) + 연출 + 디버그 등에도 사용 예정
        public Vector3 position;    // 이벤트 발생 위치 (연출 + 디버그)
        public float time;          // 이벤트 발생 시각 (ITickable Time tick 기준)

        /// <summary>
        /// 이벤트를 발생시킨 공의 rb
        /// Trigger가 감지 시점에 주입하며, Reaction에서 물리/연출 타겟 지정을 확실히 하는데 사용
        /// </summary>
        public Rigidbody ball;

        public bool HasTag(string tag)
        {
            if (tags == null || tags.Length == 0 || string.IsNullOrEmpty(tag))
            {
                return false;
            }

            for (int i = 0; i < tags.Length; i++)
            {
                if (string.Equals(tags[i], tag, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }

        public override string ToString() => $"{eventId} | source={source} | base={baseValue} | tags={tags.Length}";
    }
}
