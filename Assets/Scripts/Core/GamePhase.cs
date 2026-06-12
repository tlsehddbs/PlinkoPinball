namespace PlinkoPinball.Core
{
    /// <summary>
    /// 현재 게임 진행 상태를 나타냄
    /// </summary>
    public enum GamePhase
    {
        None,
        MainMenu,
        Pinball,
        Plinko,

        Transition,
        Paused
    }
}