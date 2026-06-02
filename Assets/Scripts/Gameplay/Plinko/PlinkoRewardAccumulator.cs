using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 런 동안 획득한 재화와 통계를 누적
    /// </summary>
    public sealed class PlinkoRewardAccumulator : MonoBehaviour
    {
        [SerializeField] private CurrencySystem currencySystem;

        private long _currency;
        private long _unassignedPendingCurrency;
        private int _pinHits;
        private int _ballsResolved;
        private readonly Dictionary<int, long> _pendingCurrencyByBall = new Dictionary<int, long>();

        private void Awake()
        {
            ResolveCurrencySystem();
        }

        /// <summary>
        /// 새로운 플링코 런을 위해 누적 상태를 초기화
        /// </summary>
        public void ResetForRun()
        {
            _currency = 0;
            _unassignedPendingCurrency = 0;
            _pinHits = 0;
            _ballsResolved = 0;
            _pendingCurrencyByBall.Clear();
        }

        /// <summary>
        /// 핀 충돌 보상을 누적
        /// </summary>
        public void RegisterPinHit(int baseReward, in PlinkoPinModifierData modifier)
        {
            RegisterPinHit(null, baseReward, in modifier);
        }

        public void RegisterPinHit(PlinkoBallActor ball, int baseReward, in PlinkoPinModifierData modifier)
        {
            int reward = PlinkoRewardCalculator.CalculatePinHitReward(baseReward, modifier);

            _pinHits += 1;
            _currency += reward;
            AddPendingCurrency(ball, reward);
        }

        /// <summary>
        /// 슬롯 보상을 누적
        /// </summary>
        public void RegisterSlotReward(int baseReward, in PlinkoSlotModifierData modifier)
        {
            RegisterSlotReward(null, baseReward, in modifier);
        }

        public void RegisterSlotReward(PlinkoBallActor ball, int baseReward, in PlinkoSlotModifierData modifier)
        {
            int reward = PlinkoRewardCalculator.CalculateSlotReward(baseReward, modifier);
            
            _currency += reward;
            AddPendingCurrency(ball, reward);
            FlushBallCurrency(ball);
        }

        /// <summary>
        /// 해결 완료된 볼 수를 증가
        /// </summary>
        public void RegisterResolvedBall()
        {
            _ballsResolved += 1;
        }

        public void FlushPendingCurrency()
        {
            long amount = _unassignedPendingCurrency;
            _unassignedPendingCurrency = 0;

            foreach (long pending in _pendingCurrencyByBall.Values)
            {
                amount += pending;
            }

            _pendingCurrencyByBall.Clear();
            AddCurrency(amount);
        }

        /// <summary>
        /// 현재 누적 결과를 런 결과로 반환
        /// </summary>
        public PlinkoRunResult BuildResult()
        {
            return new PlinkoRunResult(_currency, _pinHits, _ballsResolved);
        }

        private void AddPendingCurrency(PlinkoBallActor ball, long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            if (ball == null)
            {
                _unassignedPendingCurrency += amount;
                return;
            }

            int ballId = ball.GetInstanceID();
            _pendingCurrencyByBall.TryGetValue(ballId, out long current);
            _pendingCurrencyByBall[ballId] = current + amount;
        }

        private void FlushBallCurrency(PlinkoBallActor ball)
        {
            if (ball == null)
            {
                FlushPendingCurrency();
                return;
            }

            int ballId = ball.GetInstanceID();
            if (!_pendingCurrencyByBall.TryGetValue(ballId, out long amount))
            {
                return;
            }

            _pendingCurrencyByBall.Remove(ballId);
            AddCurrency(amount);
        }

        private void AddCurrency(long amount)
        {
            if (amount <= 0)
            {
                return;
            }

            ResolveCurrencySystem();
            currencySystem?.AddCurrency(amount);
        }

        private void ResolveCurrencySystem()
        {
            if (currencySystem != null)
            {
                return;
            }

            currencySystem = CurrencySystem.Instance;
            if (currencySystem == null)
            {
                currencySystem = FindAnyObjectByType<CurrencySystem>();
            }
        }
    }
}
