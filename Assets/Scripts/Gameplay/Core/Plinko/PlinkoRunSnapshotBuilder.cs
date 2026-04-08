using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 핀볼 플레이 중 획득한 플링코 특전을 누적
    /// 라운드 종료 시 플링코 런 스냅샷을 생성
    /// </summary>
    public sealed class PlinkoRunSnapshotBuilder : MonoBehaviour
    {
        [SerializeField, Min(0)] private int startBalls;
        [SerializeField, Min(0)] private int globalCurrencyPerPinHit;

        private readonly List<PlinkoBonusToken> _tokens = new List<PlinkoBonusToken>(32);

        /// <summary>
        /// 시작할 볼의 개수를 추가
        /// </summary>
        public void AddStartBalls(int amount)
        {
            if (amount <= 0)
                return;

            startBalls += amount;
        }

        /// <summary>
        /// 모든 핀에 공통 적용되는 핀 히트 재화 보너스 추가
        /// </summary>
        public void AddGlobalCurrencyPerPinHit(int amount)
        {
            if(amount <= 0)
                return;

            globalCurrencyPerPinHit += amount;
        }

        /// <summary>
        /// 개별 핀 또는 슬록에 주입할 토큰을 추가
        /// </summary>
        public void AddToken(PlinkoBonusTokenType tokenType, int amount = 1, int stackCount = 1)
        {
            if (amount <= 0 || stackCount <= 0)
                return;

            _tokens.Add(new PlinkoBonusToken(tokenType, amount, stackCount));
        }


        /// <summary>
        /// 누적 상태를 바탕으로 플링코 런 스냅샷을 생성
        /// </summary>
        public PlinkoRunSnapshot BuildSnapshot()
        {
            int seed = Random.Range(int.MinValue, int.MaxValue);

            return new PlinkoRunSnapshot(
                startBalls,
                globalCurrencyPerPinHit,
                new List<PlinkoBonusToken>(_tokens),
                seed
            );
        }

        /// <summary>
        /// 다음 라운드를 위해 누적 상태를 초기화
        /// </summary>
        public void ResetForRound()
        {
            startBalls = 0;
            globalCurrencyPerPinHit = 0;
            _tokens.Clear();
        }
    }
}
