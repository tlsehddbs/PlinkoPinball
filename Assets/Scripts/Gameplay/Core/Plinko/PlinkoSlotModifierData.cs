namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 개별 플링코 슬롯의 런타임 보정 데이터
    /// </summary>
    public struct PlinkoSlotModifierData
    {
        public int FlatCurrencyBonus;
        public float JackpotMultiplier;

        /// <summary>
        ///  기본 슬롯 상태
        /// </summary>
        public static PlinkoSlotModifierData Default => new PlinkoSlotModifierData
        {
            FlatCurrencyBonus = 0,
            JackpotMultiplier = 1f
        };

        /// <summary>
        /// 기본 보상에 슬롯 보정을 적용한 최종 재화 값을 계산
        /// </summary>
        public int CalculateReward(int baseReward)
        {
            float value = (baseReward + FlatCurrencyBonus) * JackpotMultiplier;
            return UnityEngine.Mathf.Max(0, UnityEngine.Mathf.RoundToInt(value));
        }
    }
}