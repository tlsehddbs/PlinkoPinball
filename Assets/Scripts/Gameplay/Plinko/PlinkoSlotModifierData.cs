namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 개별 플링코 슬롯의 런타임 보정 데이터
    /// </summary>
    public struct PlinkoSlotModifierData
    {
        public PlinkoSlotStateKind StateKind;
        public int ValueBonus;
        public float Multiplier;

        public static PlinkoSlotModifierData Default => new PlinkoSlotModifierData
        {
            StateKind = PlinkoSlotStateKind.Normal,
            ValueBonus = 0,
            Multiplier = 1f
        };

        public int CalculateReward(int baseReward)
        {
            if (StateKind == PlinkoSlotStateKind.Error)
            {
                return 0;
            }

            float value = (baseReward + ValueBonus) * UnityEngine.Mathf.Max(0f, Multiplier);
            return UnityEngine.Mathf.Max(0, UnityEngine.Mathf.RoundToInt(value));
        }
    }
}