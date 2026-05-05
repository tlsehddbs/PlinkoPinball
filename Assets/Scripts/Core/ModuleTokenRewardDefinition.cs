using System;
using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Core.Modules
{
    /// <summary>
    /// 모듈 상태 변화가 어떤 플링코 보상을 지급하는지 정의
    /// </summary>
    [CreateAssetMenu(fileName = "ModuleTokenRewardDefinition", menuName = "PlinkoPinball/Modules/Module Token Reward Definition")]
    public sealed class ModuleTokenRewardDefinition : ScriptableObject
    {
        [Header("Match")]
        [SerializeField] private string moduleId;
        [SerializeField] private ModuleRewardState requiredState;

        [Header("Rewards")]
        [SerializeField] private RewardEntry[] rewards;

        public string ModuleId => moduleId;
        public ModuleRewardState RequiredState => requiredState;
        public RewardEntry[] Rewards => rewards;


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
    }
}