using UnityEngine;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Components.Plinko
{
    /// <summary>
    /// 개별 플링코 핀의 런타임 상태를 보관
    /// </summary>
    public sealed class PlinkoPinRuntime : MonoBehaviour
    {
        [SerializeField] private string pinId;
        [SerializeField] private int flatCurrencyBonus;
        [SerializeField, Min(1f)] private float hitMultiplier = 1f;
        [SerializeField] private int extraBounceReward;

        /// <summary>
        /// 현재 핀의 고유 ID
        /// </summary>
        public string PinId => pinId;

        /// <summary>
        /// 핀 상태를 기본값으로 초기화
        /// </summary>
        public void ResetRuntimeState()
        {
            flatCurrencyBonus = 0;
            hitMultiplier = 1f;
            extraBounceReward = 0;
        }

        /// <summary>
        /// 외부 스냅샷 결과를 해당 핀에 적용
        /// </summary>
        /// <param name="currencyBonus">고정 재화 보너스</param>
        /// <param name="multiplier">히트 배율</param>
        /// <param name="bounceReward">추가 바운스 보상</param>
        public void ApplyState(int currencyBonus, float multiplier, int bounceReward)
        {
            flatCurrencyBonus = currencyBonus;
            hitMultiplier = Mathf.Max(1f, multiplier);
            extraBounceReward = bounceReward;
        }

        /// <summary>
        /// 현재 상태를 계산용 데이터로 반환
        /// </summary>
        public PlinkoPinModifierData ExportState()
        {
            return new PlinkoPinModifierData
            {
                FlatCurrencyBonus = flatCurrencyBonus,
                HitMultiplier = hitMultiplier,
                ExtraBounceReward = extraBounceReward
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