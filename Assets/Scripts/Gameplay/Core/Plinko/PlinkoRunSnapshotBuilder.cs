using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Gameplay.Core.Flow;
using System.Text;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 핀볼 플레이 중 획득한 플링코 특전을 누적
    /// 라운드 종료 시 스냅샷을 생성
    /// </summary>
    public sealed class PlinkoRunSnapshotBuilder : MonoBehaviour, IRoundResettable
    {
        [SerializeField, Min(0)] private int startBalls;
        [SerializeField, Min(0)] private int globalCurrencyPerPinHit;

        private readonly List<PlinkoBonusToken> _tokens = new List<PlinkoBonusToken>(32);

        /// <summary>
        /// 누적된 플링코 시작 볼의 수
        /// </summary>
        public int StartBalls => startBalls;

        /// <summary>
        /// 누적된 전역 핀 보상
        /// </summary>
        public int GlobalCurrencyPerPinHit => globalCurrencyPerPinHit;

        /// <summary>
        /// 누적된 토큰 수
        /// </summary>
        public int TokenCount => _tokens.Count;

        /// <summary>
        /// 플링코 시작 볼 수를 추가
        /// </summary>
        public void AddStartBalls(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            startBalls += amount;

            Debug.Log($"[PlinkoRunSnapshotBuilder] AddStartBalls +{amount} => {startBalls}", this);
        }

        /// <summary>
        /// 모든 핀 히트에 공통 적용되는 재화 보너스를 추가
        /// </summary>
        public void AddGlobalCurrencyPerPinHit(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            globalCurrencyPerPinHit += amount;
            Debug.Log($"[PlinkoRunSnapshotBuilder] AddglobalCurrencyPerPinHit +{amount} => {globalCurrencyPerPinHit}", this);
        }

        /// <summary>
        /// 개별 핀/슬롯 주입용 토큰을 추가
        /// </summary>
        public void AddToken(PlinkoBonusTokenType tokenType, int amount = 1, int stackCount = 1)
        {
            if (amount <= 0 || stackCount <= 0)
            {
                return;
            }

            _tokens.Add(new PlinkoBonusToken(tokenType, amount, stackCount));
            Debug.Log($"[PlinkoRunSnapshotBuilder] AddToken type={tokenType}, amount={amount}, stack={stackCount}, totalTokens={_tokens.Count}", this);
            
        }

        /// <summary>
        /// 현재 누적 상태를 바탕으로 스냅샷을 생성
        /// </summary>
        public PlinkoRunSnapshot BuildSnapshot()
        {
            int seed = Random.Range(int.MinValue, int.MaxValue);



#if UNITY_EDITOR || DEVELOPMENT_BUILD
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("[PlinkoRunSnapshotBuilder] BuildSnapshot");
            sb.AppendLine($"  StartBalls = {startBalls}");
            sb.AppendLine($"  GlobalCurrencyPerPinHit = {globalCurrencyPerPinHit}");
            sb.AppendLine($"  TokenCount = {_tokens.Count}");
            sb.AppendLine($"  Seed = {seed}");

            for (int i = 0; i < _tokens.Count; i++)
            {
                PlinkoBonusToken token = _tokens[i];
                sb.AppendLine($"  Token[{i}] = {token.TokenType}, amount={token.Amount}, stack={token.StackCount}");
            }
            Debug.Log(sb.ToString(), this);
#endif

            return new PlinkoRunSnapshot(
                startBalls,
                globalCurrencyPerPinHit,
                new List<PlinkoBonusToken>(_tokens),
                seed);
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