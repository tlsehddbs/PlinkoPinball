using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 런 동안 획득한 재화와 통계를 누적
    /// </summary>
    public sealed class PlinkoRewardAccumulator : MonoBehaviour
    {
        private long _currency;
        private int _pinHits;
        private int _ballsResolved;

        /// <summary>
        /// 새로운 플링코 런을 위해 누적 상태를 초기화
        /// </summary>
        public void ResetForRun()
        {
            _currency = 0;
            _pinHits = 0;
            _ballsResolved = 0;
        }

        /// <summary>
        /// 핀 충돌 보상을 누적
        /// </summary>
        public void RegisterPinHit(int baseReward, in PlinkoPinModifierData modifier)
        {
            int reward = PlinkoRewardCalculator.CalculatePinHitReward(baseReward, modifier);

            _pinHits += 1;
            _currency += reward;
        }

        /// <summary>
        /// 슬롯 보상을 누적
        /// </summary>
        public void RegisterSlotReward(int baseReward, in PlinkoSlotModifierData modifier)
        {
            int reward = PlinkoRewardCalculator.CalculateSlotReward(baseReward, modifier);
            
            _currency += reward;
        }

        /// <summary>
        /// 해결 완료된 볼 수를 증가
        /// </summary>
        public void RegisterResolvedBall()
        {
            _ballsResolved += 1;
        }

        /// <summary>
        /// 현재 누적 결과를 런 결과로 반환
        /// </summary>
        public PlinkoRunResult BuildResult()
        {
            return new PlinkoRunResult(_currency, _pinHits, _ballsResolved);
        }
    }
}