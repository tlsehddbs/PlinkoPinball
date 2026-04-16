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
        /// 외부 스냅샷 결과를 이 핀에 적용
        /// </summary>
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
    }
}