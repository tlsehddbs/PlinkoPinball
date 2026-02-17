using System;
using UnityEditor.EditorTools;
using UnityEngine;

namespace PlinkoPinball.Core.TableEvents
{
    [Serializable]
    public struct TableEvent
    {
        public string eventId;
        public TableEventType eventType;
        public int baseValue;   // 점수 계산에 활용
        public string[] tags;   // score, gate, bonus, timebonus, penalty 등을 위한 tag

        public Transform source;    // 이벤트를 발생시키게한 오브젝트 (모듈 내부 요소 추적 등에 사용 예정) + 연출 + 디버그 등에도 사용 예정
        public Vector3 position;    // 이벤트 발생 위치 (연출 + 디버그)
        public float time;          // 이벤트 발생 시각(ITickable Time tick 기준)

        public bool HasTag(string tag)
        {
            if (tags == null || tags.Length == 0 || string.IsNullOrEmpty(tag)) return false;

            for (int i = 0; i < tags.Length; i++)
            {
                if (string.Equals(tags[i], tag, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public override string ToString() => $"{eventType} | {eventId} | base={baseValue} | src={(source != null ? source.name : "null")}";
    }

}
