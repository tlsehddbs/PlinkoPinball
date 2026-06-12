using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 핀볼 라운드 종료 시 확정된 플링코 입력 스냅샷
    /// </summary>
    public readonly struct PlinkoRunSnapshot
    {
        public readonly int StartBalls;
        public readonly float GlobalPinMultiplier;
        public readonly float GlobalSlotMultiplier;
        public readonly float ErrorPinRateReduction;
        public readonly float ErrorSlotRateReduction;
        public readonly IReadOnlyList<PlinkoBonusToken> Tokens;
        public readonly int BoardSeed;

        public PlinkoRunSnapshot(int startBalls, float globalPinMultiplier, float globalSlotMultiplier, float errorPinRateReduction, float errorSlotRateReduction, IReadOnlyList<PlinkoBonusToken> tokens, int boardSeed)
        {
            StartBalls = Mathf.Max(0, startBalls);
            GlobalPinMultiplier = Mathf.Max(0f, globalPinMultiplier);
            GlobalSlotMultiplier = Mathf.Max(0f, globalSlotMultiplier);
            ErrorPinRateReduction = Mathf.Clamp01(errorPinRateReduction);
            ErrorSlotRateReduction = Mathf.Clamp01(errorSlotRateReduction);
            Tokens = tokens;
            BoardSeed = boardSeed;
        }
    }
}