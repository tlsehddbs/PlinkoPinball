using System;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 시작 전 보드에 주입될 하나의 보너스 토큰
    /// amount는 효과 강도, stackCount는 동일 토큰 수량을 의미
    /// </summary>
    [Serializable]
    public struct PlinkoBonusToken
    {
        [SerializeField] private PlinkoBonusTokenType tokenType;
        [SerializeField] private int amount;
        [SerializeField] private int stackCount;

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
