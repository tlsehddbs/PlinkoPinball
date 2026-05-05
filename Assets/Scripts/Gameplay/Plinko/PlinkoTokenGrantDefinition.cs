using System;
using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 이벤트 ID 문자열 비교 방식을 정의
    /// </summary>
    public enum EventIdMatchMode
    {
        Exact = 0,
        Prefix = 1
    }

    /// <summary>
    /// 하나의 핀볼 TableEvent가 어떤 플링코 보상을 지급할 수 있는지를 정의
    /// </summary>
    [CreateAssetMenu(fileName = "PlinkoTokenGrantDefinition", menuName = "PlinkoPinball/Plinko/Token Grant Definition")]
    public sealed class PlinkoTokenGrantDefinition : ScriptableObject
    {
        [Serializable]
        public struct RewardEntry
        {
            [SerializeField] private bool grantStartBalls;
            [SerializeField, Min(1)] private int startBallAmount;

            [SerializeField] private bool grantToken;
            [SerializeField] private PlinkoBonusTokenType tokenType;
            [SerializeField, Min(1)] private int tokenAmount;
            [SerializeField, Min(1)] private int tokenStackCount;

            public bool GrantStartBalls => grantStartBalls;
            public int StartBallAmount => startBallAmount;


            public bool GrantToken => grantToken;
            public PlinkoBonusTokenType TokenType => tokenType;
            public int TokenAmount => tokenAmount;
            public int TokenStackCount => tokenStackCount;
        }

        [Header("Match")]
        [SerializeField] private string requiredEventId;
        [SerializeField] private EventIdMatchMode eventIdMatchMode = EventIdMatchMode.Exact;
        [SerializeField] private string[] requiredTags;
        [SerializeField, Range(0f, 1f)] private float chance = 1f;

        [Header("Rewards")]
        [SerializeField] private RewardEntry[] rewards;

        public float Chance => chance;
        public IReadOnlyList<RewardEntry> Rewards => rewards;

        /// <summary>
        /// 지정한 TableEvent가 이 정의와 일치하는지
        /// </summary>
        /// <param name="tableEvent">검사할 테이블 이벤트</param>
        public bool Matches(in TableEvent tableEvent)
        {
            if (!MatchesEventId(tableEvent.eventId))
            {
                return false;
            }

            if (requiredTags == null || requiredTags.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < requiredTags.Length; i++)
            {
                if (!tableEvent.HasTag(requiredTags[i]))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 현재 정의의 이벤트 ID 조건과 입력 eventId가 일치하는지 검사
        /// </summary>
        /// <param name="eventId">검사할 이벤트 ID</param>
        private bool MatchesEventId(string eventId)
        {
            if (string.IsNullOrWhiteSpace(requiredEventId))
            {
                return true;
            }

            if (string.IsNullOrWhiteSpace(eventId))
            {
                return false;
            }

            switch (eventIdMatchMode)
            {
                case EventIdMatchMode.Exact:
                    return string.Equals(requiredEventId, eventId, StringComparison.Ordinal);

                case EventIdMatchMode.Prefix:
                    // "hit" 로 설정했을 때
                    // "hit" 자체도 허용하고 "hit.xxx" 계열도 허용
                    if (string.Equals(requiredEventId, eventId, StringComparison.Ordinal))
                    {
                        return true;
                    }

                    string prefixWithSeparator = requiredEventId + ".";
                    return eventId.StartsWith(prefixWithSeparator, StringComparison.Ordinal);

                default:
                    return false;
            }
        }
    }
}