namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 핀볼 페이즈에서 획득하여 플링코 시작 시 보드에 주입되는 보너스 토큰 유형
    /// </summary>
    public enum PlinkoBonusTokenType
    {
        RandomPinValueBonus,        // 사용하지 않을 수 있음 -> 업그레이드 시스템으로 이관 가능성 있음
        RandomPinMultiplier,

        RandomSlotValueBonus,       // 사용하지 않을 수 있음
        RandomSlotMultiplier,

        GlobalPinMultiplier,        // 사용하지 않을 수 있음
        GlobalSlotMultiplier,       // 사용하지 않을 수 있음

        ErrorPinRateReduction,
        ErrorSlotRateReduction
    }
}