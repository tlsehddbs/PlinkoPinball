using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core.Flow;
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
        [SerializeField, Min(0f)] private float globalPinMultiplier = 1f;
        [SerializeField, Min(0f)] private float globalSlotMultiplier = 1f;
        [SerializeField, Range(0f, 1f)] private float errorPinRateReduction;
        [SerializeField, Range(0f, 1f)] private float errorSlotRateReduction;

        public float GlobalPinMultiplier => globalPinMultiplier;
        public float GlobalSlotMultiplier => globalSlotMultiplier;
        public float ErrorPinRateReduction => errorPinRateReduction;
        public float ErrorSlotRateReduction => errorSlotRateReduction;

        private readonly List<PlinkoBonusToken> _tokens = new List<PlinkoBonusToken>(32);

        public void AddStartBalls(int amount)
        {
            if (amount <= 0)
            {
                return;
            }
            
            startBalls += amount;

            Debug.Log($"[PlinkoRunSnapshotBuilder] AddStartBalls +{amount} => {startBalls}", this);
        }

        public void AddGlobalPinMultiplier(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            globalPinMultiplier += amount;

            Debug.Log($"[PlinkoRunSnapshotBuilder] AddGlobalPinMultiplier +{amount} => {globalPinMultiplier}", this);
        }

        public void AddGlobalSlotMultiplier(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            globalSlotMultiplier += amount;

            Debug.Log($"[PlinkoRunSnapshotBuilder] AddGlobalSlotMultiplier +{amount} => {globalSlotMultiplier}", this);
        }

        public void AddErrorPinRateReduction(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            errorPinRateReduction = Mathf.Clamp01(errorPinRateReduction + amount);

            Debug.Log($"[PlinkoRunSnapshotBuilder] AddErrorPinRateReduction +{amount} => {errorPinRateReduction}", this);
        }

        public void AddErrorSlotRateReduction(float amount)
        {
            if (amount <= 0f)
            {
                return;
            }

            errorSlotRateReduction = Mathf.Clamp01(errorSlotRateReduction + amount);

            Debug.Log($"[PlinkoRunSnapshotBuilder] AddErrorSlotRateReduction +{amount} => {errorSlotRateReduction}", this);
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
            sb.AppendLine($"  GlobalPinMultiplier = {globalPinMultiplier}");
            sb.AppendLine($"  GlobalSlotMultiplier = {globalSlotMultiplier}");
            sb.AppendLine($"  ErrorPinRateReduction = {errorPinRateReduction}");
            sb.AppendLine($"  ErrorSlotRateReduction = {errorSlotRateReduction}");
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
                globalPinMultiplier,
                globalSlotMultiplier,
                errorPinRateReduction,
                errorSlotRateReduction,
                new List<PlinkoBonusToken>(_tokens),
                seed);
        }


        /// <summary>
        /// 다음 라운드를 위해 누적 상태를 초기화
        /// </summary>
        public void ResetForRound()
        {
            startBalls = 0;
            globalPinMultiplier = 1f;
            globalSlotMultiplier = 1f;
            errorPinRateReduction = 0f;
            errorSlotRateReduction = 0f;
            _tokens.Clear();
        }
    }
}