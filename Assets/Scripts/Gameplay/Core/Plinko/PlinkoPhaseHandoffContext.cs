using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Core.Flow
{
    /// <summary>
    /// 핀볼 씬에서 플링코 씬으로 전달되는 실행 컨텍스트
    /// </summary>
    public readonly struct PlinkoPhaseHandoffContext
    {
        public readonly int RoundIndex;
        public readonly int PinballScore;
        public readonly PlinkoRunSnapshot RunSnapshot;

        public PlinkoPhaseHandoffContext(int roundIndex, int pinballScore, PlinkoRunSnapshot runSnapshot)
        {
            RoundIndex = roundIndex;
            PinballScore = pinballScore;
            RunSnapshot = runSnapshot;
        }
    }
}