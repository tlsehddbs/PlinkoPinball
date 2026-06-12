using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Gameplay.Core.Flow;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// Plinko Scene 단독 테스트를 위한 디버그 Handoff Context 공급자입니다.
    /// Pinball Phase 없이 Snapshot 기반 Plinko 흐름을 검증할 때만 사용합니다.
    /// </summary>
    public sealed class PlinkoDebugSnapshotProvider : MonoBehaviour
    {
        [Header("Debug Run")]
        [SerializeField, Min(1)] private int debugRoundIndex = 1;
        [SerializeField, Min(0)] private int debugPinballScore = 1000;
        [SerializeField, Min(1)] private int debugStartBalls = 5;
        [SerializeField] private int debugSeed = 12345;

        [Header("Global Runtime Modifiers")]
        [SerializeField, Min(0f)] private float debugGlobalPinMultiplier = 1f;
        [SerializeField, Min(0f)] private float debugGlobalSlotMultiplier = 1f;

        [Header("Error Correction")]
        [SerializeField, Range(0f, 1f)] private float debugErrorPinRateReduction = 0f;
        [SerializeField, Range(0f, 1f)] private float debugErrorSlotRateReduction = 0f;

        [Header("Debug Tokens")]
        [SerializeField, Min(0)] private int randomPinMultiplierStacks = 3;
        [SerializeField, Min(0)] private int randomPinValueBonusStacks = 4;
        [SerializeField, Min(0)] private int randomSlotMultiplierStacks = 1;
        [SerializeField, Min(0)] private int randomSlotValueBonusStacks = 2;

        [SerializeField, Min(1)] private int randomPinMultiplierAmount = 1;
        [SerializeField, Min(1)] private int randomPinValueBonusAmount = 2;
        [SerializeField, Min(1)] private int randomSlotMultiplierAmount = 1;
        [SerializeField, Min(1)] private int randomSlotValueBonusAmount = 3;

        /// <summary>
        /// 디버그용 Plinko Handoff Context를 생성합니다.
        /// </summary>
        public PlinkoPhaseHandoffContext BuildDebugContext()
        {
            List<PlinkoBonusToken> tokens = BuildDebugTokens();

            PlinkoRunSnapshot snapshot = new PlinkoRunSnapshot(debugStartBalls, debugGlobalPinMultiplier, debugGlobalSlotMultiplier, debugErrorPinRateReduction, debugErrorSlotRateReduction, tokens, debugSeed);

            return new PlinkoPhaseHandoffContext(debugRoundIndex, debugPinballScore, snapshot);
        }

        private List<PlinkoBonusToken> BuildDebugTokens()
        {
            var tokens = new List<PlinkoBonusToken>();

            AddTokenIfNeeded(
                tokens,
                PlinkoBonusTokenType.RandomPinMultiplier,
                randomPinMultiplierAmount,
                randomPinMultiplierStacks);

            AddTokenIfNeeded(
                tokens,
                PlinkoBonusTokenType.RandomPinValueBonus,
                randomPinValueBonusAmount,
                randomPinValueBonusStacks);

            AddTokenIfNeeded(
                tokens,
                PlinkoBonusTokenType.RandomSlotMultiplier,
                randomSlotMultiplierAmount,
                randomSlotMultiplierStacks);

            AddTokenIfNeeded(
                tokens,
                PlinkoBonusTokenType.RandomSlotValueBonus,
                randomSlotValueBonusAmount,
                randomSlotValueBonusStacks);

            return tokens;
        }

        private static void AddTokenIfNeeded(List<PlinkoBonusToken> tokens, PlinkoBonusTokenType tokenType, int amount, int stackCount)
        {
            if (stackCount <= 0)
            {
                return;
            }

            tokens.Add(new PlinkoBonusToken(tokenType, Mathf.Max(1, amount), stackCount));
        }
    }
}