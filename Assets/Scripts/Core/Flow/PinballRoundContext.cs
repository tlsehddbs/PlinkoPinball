namespace PlinkoPinball.Gameplay.Core.Flow
{
    /// <summary>
    /// 현재 핀볼 라운드의 최소 문맥 정보를 보관
    /// </summary>
    public struct PinballRoundContext
    {
        public int RoundIndex;
        public bool HasEnded;
    }
}