using System;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    [Serializable]
    public sealed class PlinkoGrantRewardEntry
    {
        [Header("Start Balls")]
        [SerializeField] private bool grantStartBalls;

        [Min(0)]
        [SerializeField] private int startBallAmount = 0;

        [Header("Token / Modifier")]
        [SerializeField] private bool grantToken;

        [SerializeField] private PlinkoBonusTokenType tokenType;

        [Tooltip("Multiplier 계열은 정수 amount로 누적한다. Error Rate 계열은 percent 값으로 해석된다. 예: 3 = 0.03")]
        [SerializeField] private int tokenAmount = 1;

        [Min(1)]
        [SerializeField] private int tokenStackCount = 1;

        public bool GrantStartBalls => grantStartBalls;
        public int StartBallAmount => startBallAmount;

        public bool GrantToken => grantToken;
        public PlinkoBonusTokenType TokenType => tokenType;
        public int TokenAmount => tokenAmount;
        public int TokenStackCount => tokenStackCount;
    }
}