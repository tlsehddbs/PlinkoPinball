using System;
using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Core.Rewards
{
    [Serializable]
    public sealed class PinballRewardEntry
    {
        [SerializeField] private PinballRewardType rewardType = PinballRewardType.None;

        [Tooltip("Multiplier / Rate / Value 계열 보상값. 예: 0.05 = 5%")]
        [SerializeField] private float amount = 1f;

        [Header("Legacy Token")]
        [SerializeField] private PlinkoBonusTokenType tokenType;
        [Min(1)]
        [SerializeField] private int tokenStackCount = 1;

        public PinballRewardType RewardType => rewardType;
        public float Amount => amount;
        public PlinkoBonusTokenType TokenType => tokenType;
        public int TokenStackCount => tokenStackCount;
    }
}