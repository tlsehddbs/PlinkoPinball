namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 개별 핀 런타임에 적용되는 보정 데이터
    /// </summary>
    public struct PlinkoPinModifierData
    {
        public int FlatCurrencyBonus;
        public float HitMultiplier;
        public int ExtraBounceReward;

        public static PlinkoPinModifierData Default => new PlinkoPinModifierData
        {
            FlatCurrencyBonus = 0,
            HitMultiplier = 1f,
            ExtraBounceReward = 0
        };
    }
}