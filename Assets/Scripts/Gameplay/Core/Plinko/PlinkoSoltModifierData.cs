namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 개별 슬롯 런타임에 적용되는 보정 데이터
    /// </summary>
    public struct PlinkoSlotModifierData
    {
        public int FlatCurrencyBonus;
        public float JackpotMultiplier;

        public static PlinkoSlotModifierData Default => new PlinkoSlotModifierData
        {
            FlatCurrencyBonus = 0,
            JackpotMultiplier = 1f
        };
    }
}