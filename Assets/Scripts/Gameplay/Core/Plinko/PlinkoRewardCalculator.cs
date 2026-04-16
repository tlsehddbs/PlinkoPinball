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
        public static int CalculatePinHitReward(int baseReward, int globalCurrencyPerPinHit, in PlinkoPinModifierData modifier)
        {
            int total = baseReward + globalCurrencyPerPinHit + modifier.FlatCurrencyBonus + modifier.ExtraBounceReward;
            float multiplied = total * Mathf.Max(1f, modifier.HitMultiplier);

            return Mathf.Max(0, Mathf.RoundToInt(multiplied));
        }

        /// <summary>
        /// 슬롯 도달 시 획득 재화를 계산
        /// </summary>
        public static int CalculateSlotReward(int baseReward, in PlinkoSlotModifierData modifier)
        {
            int total = baseReward + modifier.FlatCurrencyBonus;
            float multiplied = total * Mathf.Max(1f, modifier.JackpotMultiplier);
            
            return Mathf.Max(0, Mathf.RoundToInt(multiplied));
        }
    }
}