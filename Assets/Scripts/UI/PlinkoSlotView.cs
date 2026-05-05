using TMPro;
using UnityEngine;
using PlinkoPinball.Gameplay.Components.Plinko;

namespace PlinkoPinball.UI.Presenters
{
    /// <summary>
    /// 플링코 슬롯의 보상 텍스트 및 상태 표현을 담당
    /// 슬롯 런타임 상태와 분리하여 시각 표현만 담당
    /// </summary>
    public sealed class PlinkoSlotView : MonoBehaviour
    {
        [SerializeField] private PlinkoSlotRuntime slotRuntime;
        [SerializeField] private TMP_Text rewardText;
        [SerializeField] private TMP_Text multiplierText;
        [SerializeField] private GameObject jackpotMarker;

        /// <summary>
        /// 현재 슬롯 표시를 갱신
        /// </summary>
        public void Refresh()
        {
            if (slotRuntime == null)
            {
                return;
            }

            var state = slotRuntime.ExportState();

            int displayedReward = slotRuntime.BaseReward + state.ValueBonus;
            float displayedMultiplier = state.Multiplier;

            if (rewardText != null)
            {
                rewardText.text = $"${displayedReward}";
            }

            if (multiplierText != null)
            {
                multiplierText.text = $"x{displayedMultiplier:0.#}";
            }

            if (jackpotMarker != null)
            {
                jackpotMarker.SetActive(displayedMultiplier > 1f || state.ValueBonus > 0);
            }
        }
    }
}