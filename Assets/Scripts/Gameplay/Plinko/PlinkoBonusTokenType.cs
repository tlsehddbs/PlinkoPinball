namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 핀볼 페이즈에서 획득하여 플링코 시작 시 보드에 주입되는 보너스 토큰 유형
    /// </summary>
    public enum PlinkoBonusTokenType
    {
        RandomPinValueBonus,
        RandomPinMultiplier,

        RandomSlotValueBonus,
        RandomSlotMultiplier,

        GlobalPinMultiplier,
        GlobalSlotMultiplier,

        ErrorPinRateReduction,
        ErrorSlotRateReduction
    }
}