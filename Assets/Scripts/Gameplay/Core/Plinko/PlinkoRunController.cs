using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 런 전체를 관리
    /// 볼 생성, 진행 상태, 종료 및 정산을 담당
    /// </summary>
    public sealed class PlinkoRunController : MonoBehaviour
    {
        [SerializeField] private PlinkoBallSpawner ballSpawner;
        [SerializeField] private PlinkoRewardAccumulator rewardAccumulator;
        [SerializeField] private PlinkoSettlementController settlementController;

        private int _roundIndex;
        private int _pinballScore;
        private int _remainingBalls;
        private int _activeBalls;
        private bool _isRunning;

        /// <summary>
        /// 플링코 런 시작
        /// </summary>
        public void BeginRun(int roundIndex, int pinballScore, int startBalls, int globalCurrencyPerPinHit)
        {
            _roundIndex = roundIndex;
            _pinballScore = pinballScore;
            _remainingBalls = Mathf.Max(0, startBalls);
            _activeBalls = 0;
            _isRunning = true;

            if (rewardAccumulator != null)
            {
                rewardAccumulator.ResetForRun(globalCurrencyPerPinHit);
            }

            SpawnNextBallOrComplete();
        }

        /// <summary>
        /// 볼 하나가 해결 완료되었을 때 호출
        /// </summary>
        public void OnBallResolved(PlinkoBallActor _)
        {
            if (!_isRunning)
            {
                return;
            }

            _activeBalls = Mathf.Max(0, _activeBalls - 1);

            if (rewardAccumulator != null)
            {
                rewardAccumulator.RegisterResolvedBall();
            }

            SpawnNextBallOrComplete();
        }

        private void SpawnNextBallOrComplete()
        {
            if (_remainingBalls > 0)
            {
                SpawnNextBall();
                return;
            }

            if (_activeBalls <= 0)
            {
                CompleteRun();
            }
        }

        private void SpawnNextBall()
        {
            if (ballSpawner == null || _remainingBalls <= 0)
            {
                return;
            }

            PlinkoBallActor actor = ballSpawner.SpawnBall(this);
            if (actor == null)
            {
                return;
            }

            _remainingBalls -= 1;
            _activeBalls += 1;
        }

        private void CompleteRun()
        {
            _isRunning = false;

            PlinkoRunResult result = rewardAccumulator != null
                ? rewardAccumulator.BuildResult()
                : new PlinkoRunResult(0, 0, 0);

            if (settlementController != null)
            {
                settlementController.CompleteRun(_roundIndex, _pinballScore, result);
            }
        }
    }
}