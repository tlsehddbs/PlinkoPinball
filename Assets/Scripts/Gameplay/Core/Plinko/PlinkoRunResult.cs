namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 런 종료 결과
    /// </summary>
    public readonly struct PlinkoRunResult
    {
        public readonly long EarnedCurrency;
        public readonly int TotalPinHits;
        public readonly int BallsResolved;

        public PlinkoRunResult(long earnedCurrency, int totalPinHits, int ballsResolved)
        {
            EarnedCurrency = earnedCurrency;
            TotalPinHits = totalPinHits;
            BallsResolved = ballsResolved;
        }
    }
}