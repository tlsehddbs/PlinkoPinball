using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 핀 충돌 및 슬롯 정산 시 실제 획득 재화를 계산
    /// </summary>
    public static class PlinkoRewardCalculator
    {
        /// <summary>
        /// 핀 충돌 시 획득 재화를 계산
        /// </summary>
        public static int CalculatePinHitReward(int baseReward, in PlinkoPinModifierData modifier)
        {
            if (modifier.StateKind == PlinkoPinStateKind.Error)
            {
                return 0;
            }

            int total = baseReward + modifier.ValueBonus;
            float multiplied = total * Mathf.Max(0f, modifier.Multiplier);

            return Mathf.Max(0, Mathf.RoundToInt(multiplied));
        }

        /// <summary>
        /// 슬롯 도달 시 획득 재화를 계산
        /// </summary>
        public static int CalculateSlotReward(int baseReward, in PlinkoSlotModifierData modifier)
        {
            if (modifier.StateKind == PlinkoSlotStateKind.Error)
            {
                return 0;
            }

            int total = baseReward + modifier.ValueBonus;
            float multiplied = total * Mathf.Max(0f, modifier.Multiplier);
            
            return Mathf.Max(0, Mathf.RoundToInt(multiplied));
        }
    }
}