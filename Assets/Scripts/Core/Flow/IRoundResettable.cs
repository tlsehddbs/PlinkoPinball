namespace PlinkoPinball.Gameplay.Core.Flow
{
    /// <summary>
    /// 새 라운드 시작 시 상태를 초기화해야 하는 시스템 인터페이스
    /// </summary>
    public interface IRoundResettable
    {
        /// <summary>
        /// 새로운 라운드를 위해 상태를 초기화
        /// </summary>
        void ResetForRound();
    }
}