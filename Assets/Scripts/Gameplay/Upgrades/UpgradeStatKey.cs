namespace PlinkoPinball.Gameplay.Upgrades
{
    /// <summary>
    /// 업그레이드가 수정할 수 있는 공용 스탯 키
    /// 각 시스템은 필요한 키만 조회해서 실제 룰에 반영
    /// </summary>
    public enum UpgradeStatKey
    {
        None = 0,

        // Score / Pinball
        ModuleScoreMultiplierFlat,
        ScoreMultiplierFlat,
        TimeBonusFlat,

        // Reward / Perk
        PerkChanceFlat,
        HighTierModuleChanceFlat,

        // Plinko
        PlinkoStartBallsFlat,
        PlinkoPinValueFlat,
        PlinkoSlotValueFlat,

        // Economy
        CurrencyGainMultiplierFlat
    }
}