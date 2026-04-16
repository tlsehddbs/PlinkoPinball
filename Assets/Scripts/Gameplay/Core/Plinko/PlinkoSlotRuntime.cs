using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    /// <summary>
    /// 개별 플링코 슬롯의 런타임 상태를 보관
    /// </summary>
    public sealed class PlinkoSlotRuntime : MonoBehaviour
    {
        [SerializeField] private string slotId;
        [SerializeField, Min(0)] private int baseReward = 5;
        [SerializeField] private int flatCurrencyBonus;
        [SerializeField, Min(1f)] private float jackpotMultiplier = 1f;

        public string SlotId => slotId;
        public int BaseReward => baseReward;

        /// <summary>
        /// 슬롯 상태를 기본값으로 초기화
        /// </summary>
        public void ResetRuntimeState()
        {
            flatCurrencyBonus = 0;
            jackpotMultiplier = 1f;
        }

        /// <summary>
        /// 외부 스냅샷 결과를 이 슬롯에 적용
        /// </summary>
        public void ApplyState(int currencyBonus, float multiplier)
        {
            flatCurrencyBonus = currencyBonus;
            jackpotMultiplier = Mathf.Max(1f, multiplier);
        }

        /// <summary>
        /// 현재 상태를 계산용 데이터로 반환
        /// </summary>
        public PlinkoSlotModifierData ExportState()
        {
            return new PlinkoSlotModifierData
            {
                FlatCurrencyBonus = flatCurrencyBonus,
                JackpotMultiplier = jackpotMultiplier
            };
        }
    }
}