using System;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 보드에 주입될 하나의 보너스 토큰
    /// </summary>
    [Serializable]
    public struct PlinkoBonusToken
    {
        [SerializeField] private PlinkoBonusTokenType tokenType;
        [SerializeField, Min(1)] private int amount;
        [SerializeField, Min(1)] private int stackCount;

        public PlinkoBonusTokenType TokenType => tokenType;
        public int Amount => amount;
        public int StackCount => stackCount;

        public PlinkoBonusToken(PlinkoBonusTokenType tokenType, int amount, int stackCount)
        {
            this.tokenType = tokenType;
            this.amount = amount;
            this.stackCount = stackCount;
        }
    }
}