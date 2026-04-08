using System.Collections.Generic;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 핀볼 라운드 종료 시점에 확정된 플링코 런 입력값
    /// 글로벌 보너스와 보드 주입 토큰을 함께 보관
    /// </summary>
    public readonly struct PlinkoRunSnapshot
    {
        public readonly int StartBalls;
        public readonly int GloabalCurrencyPerPinHit;
        public readonly IReadOnlyList<PlinkoBonusToken> Tokens;
        public readonly int BoardSeed;      // 같은 스냅샷인 경우 보드 배치 결과를 재현할 수 있는 이점이 있음

        public PlinkoRunSnapshot(int startBalls, int gloabalCurrencyPerPinHit, IReadOnlyList<PlinkoBonusToken> tokens, int boardSeed)
        {
            StartBalls = startBalls;
            GloabalCurrencyPerPinHit = gloabalCurrencyPerPinHit;
            Tokens = tokens;
            BoardSeed = boardSeed;
        }
    }
}
