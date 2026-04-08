namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 핀볼 라운드에서 획득 후 플링코 보드에 주입하는 보너스 토큰 유형
    /// 토큰은 플링코 시작 전에 해석되어 개별 핀에 적용됨
    /// </summary>
    public enum PlinkoBonusTokenType
    {
        StartBall,
        GloabalCurrencyPerPinHit,
        RandomPinFlatCurrencyBonus,
        RandomPinHitMultuplier,
        RandomPinExtraBonusReward,
        RandomSlotFlatCurrencyBonus,
        RandomSlotJackpotMultiplier
    }
}
