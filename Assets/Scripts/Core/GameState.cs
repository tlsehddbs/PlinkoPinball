namespace PlinkoPinball.Core
{
    /// <summary>
    /// 게임의 메인 흐름 상태 정의
    /// </summary>
    public enum GameState
    {
        Boot = 0,
        MainMenu = 10,
        InRound = 20,
        Paused = 30,
        RoundEnded = 40
    }
}
