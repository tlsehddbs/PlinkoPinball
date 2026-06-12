using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    /// <summary>
    /// 개별 플링코 핀의 런타임 상태를 보관
    /// </summary>
    public sealed class PlinkoPinRuntime : MonoBehaviour
    {
        [Header("Identity")]
        [SerializeField] private string pinId;

        [Header("Runtime Modifier")]
        [SerializeField] private PlinkoPinStateKind stateKind = PlinkoPinStateKind.Normal;
        [SerializeField, Min(0)] private int valueBonus;
        [SerializeField, Min(0f)] private float multiplier = 1f;

        /// <summary>
        /// 현재 핀의 고유 ID
        /// </summary>
        public string PinId => pinId;

        /// <summary>
        /// 핀 상태를 기본값으로 초기화
        /// </summary>
        public void ResetRuntimeState()
        {
            stateKind = PlinkoPinStateKind.Normal;
            valueBonus = 0;
            multiplier = 1f;
        }

        /// <summary>
        /// 외부 스냅샷 결과를 해당 핀에 적용
        /// </summary>
        public void ApplyState(PlinkoPinStateKind newStateKind, int newValueBonus, float newMultiplier)
        {
            stateKind = newStateKind;
            valueBonus = Mathf.Max(0, newValueBonus);
            multiplier = Mathf.Max(0f, newMultiplier);
        }

        /// <summary>
        /// 현재 상태를 계산용 데이터로 반환
        /// </summary>
        public PlinkoPinModifierData ExportState()
        {
            return new PlinkoPinModifierData
            {
                StateKind = stateKind,
                ValueBonus = valueBonus,
                Multiplier = multiplier
            };
        }

        /// <summary>
        /// Authoring 단계에서 핀 ID를 설정
        /// 런타임 플레이 중에는 호출하지 않도록 한다
        /// </summary>
        /// <param name="newPinId">설정할 핀 id</param>
        public void SetPinIdForAuthoring(string newPinId)
        {
            pinId = newPinId;
        }
    }
}