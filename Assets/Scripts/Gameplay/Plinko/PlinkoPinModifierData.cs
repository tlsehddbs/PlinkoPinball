namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 개별 핀 런타임에 적용되는 보정 데이터
    /// </summary>
    public struct PlinkoPinModifierData
    {
        public PlinkoPinStateKind StateKind;
        public int ValueBonus;
        public float Multiplier;

        public static PlinkoPinModifierData Default => new PlinkoPinModifierData
        {
            StateKind = PlinkoPinStateKind.Normal,
            ValueBonus = 0,
            Multiplier = 1f
        };

        public int CalculateReward(int baseReward)
        {
            if (StateKind == PlinkoPinStateKind.Error)
            {
                return 0;
            }

            float value = (baseReward + ValueBonus) * UnityEngine.Mathf.Max(0f, Multiplier);
            return UnityEngine.Mathf.Max(0, UnityEngine.Mathf.RoundToInt(value));
        }
    }
}