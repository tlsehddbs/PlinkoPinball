using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Gameplay.Core.Flow;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 씬 단독 테스트를 위한 디버그 컨텍스트 공급
    /// </summary>
    public sealed class PlinkoDebugSnapshotProvider : MonoBehaviour
    {
        [SerializeField, Min(1)] private int debugRoundIndex = 1;
        [SerializeField, Min(0)] private int debugPinballScore = 1000;
        [SerializeField, Min(1)] private int debugStartBalls = 5;
        [SerializeField, Min(0)] private int debugGlobalCurrencyPerPinHit = 1;
        [SerializeField] private int debugSeed = 12345;

        /// <summary>
        /// 디버그용 플링코 컨텍스트를 생성
        /// </summary>
        public PlinkoPhaseHandoffContext BuildDebugContext()
        {
            var tokens = new List<PlinkoBonusToken>
            {
                new PlinkoBonusToken(PlinkoBonusTokenType.RandomPinHitMultiplier, 1, 3),
                new PlinkoBonusToken(PlinkoBonusTokenType.RandomPinFlatCurrencyBonus, 2, 4),
                new PlinkoBonusToken(PlinkoBonusTokenType.RandomSlotJackpotMultiplier, 1, 1)
            };

            var snapshot = new PlinkoRunSnapshot(
                debugStartBalls,
                debugGlobalCurrencyPerPinHit,
                tokens,
                debugSeed);

            return new PlinkoPhaseHandoffContext(
                debugRoundIndex,
                debugPinballScore,
                snapshot);
        }
    }
}