using System.Collections.Generic;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 핀볼 라운드 종료 시 확정된 플링코 입력 스냅샷입니다.
    /// </summary>
    public readonly struct PlinkoRunSnapshot
    {
        public readonly int StartBalls;
        public readonly int GlobalCurrencyPerPinHit;
        public readonly IReadOnlyList<PlinkoBonusToken> Tokens;
        public readonly int BoardSeed;

        public PlinkoRunSnapshot(int startBalls, int globalCurrencyPerPinHit, IReadOnlyList<PlinkoBonusToken> tokens, int boardSeed)
        {
            StartBalls = startBalls;
            GlobalCurrencyPerPinHit = globalCurrencyPerPinHit;
            Tokens = tokens;
            BoardSeed = boardSeed;
        }
    }
}