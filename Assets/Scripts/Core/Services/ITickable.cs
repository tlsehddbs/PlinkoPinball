namespace PlinkoPinball.Services
{
    /// <summary>
    /// GameManager가 매 프레임 호출해 줄 순수 서비스 업데이트 인터페이스
    /// </summary>
    public interface ITickable
    {
        /// <summary>
        /// 프레임 단위 업데이트
        /// </summary
        void Tick(float deltaTime);
    }
}