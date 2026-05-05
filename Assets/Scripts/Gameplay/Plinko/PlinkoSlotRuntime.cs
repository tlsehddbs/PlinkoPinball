using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    /// <summary>
    /// 개별 플링코 슬롯의 런타임 상태를 보관
    /// </summary>
    public sealed class PlinkoSlotRuntime : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string slotId;

        [Header("Reward")]
        [SerializeField, Min(0)] private int baseReward = 5;

        [Header("Runtime Modifier")]
        [SerializeField] private PlinkoSlotStateKind stateKind = PlinkoSlotStateKind.Normal;
        [SerializeField, Min(0)] private int valueBonus;
        [SerializeField, Min(0f)] private float multiplier = 1f;

        /// <summary>
        /// 현재 슬롯의 고유 ID
        /// </summary>
        public string SlotId => slotId;

        /// <summary>
        /// 슬롯 기본 보상
        /// </summary>
        public int BaseReward => baseReward;

        /// <summary>
        /// 슬롯 상태를 기본값으로 초기화
        /// </summary>
        public void ResetRuntimeState()
        {
            stateKind = PlinkoSlotStateKind.Normal;
            valueBonus = 0;
            multiplier = 1f;
        }

        /// <summary>
        /// 외부 스냅샷 결과를 이 슬롯에 적용
        /// </summary>
        public void ApplyState(PlinkoSlotStateKind newStateKind, int newValueBonus, float newMultiplier)
        {
            stateKind = newStateKind;
            valueBonus = Mathf.Max(0, newValueBonus);
            multiplier = Mathf.Max(0f, newMultiplier);
        }

        /// <summary>
        /// 현재 상태를 계산용 데이터로 반환
        /// </summary>
        public PlinkoSlotModifierData ExportState()
        {
            return new PlinkoSlotModifierData
            {
                StateKind = stateKind,
                ValueBonus = valueBonus,
                Multiplier = multiplier
            };
        }

        /// <summary>
        /// 현재 슬롯 상태 기준 최종 보상을 계산
        /// </summary>
        public int CalculateReward()
        {
            return ExportState().CalculateReward(baseReward);
        }

        /// <summary>
        /// Authoring 단계에서 슬롯 ID를 설정
        /// </summary>
        /// <param name="newSlotId">설정할 슬롯 ID</param>
        public void SetSlotIdForAuthoring(string newSlotId)
        {
            slotId = newSlotId;
        }

        /// <summary>
        /// Authoring 단계에서 슬롯 기본 보상을 설정
        /// </summary>
        /// <param name="newBaseReward">설정할 기본 보상값</param>
        public void SetBaseRewardForAuthoring(int newBaseReward)
        {
            baseReward = Mathf.Max(0, newBaseReward);
        }
    }
}