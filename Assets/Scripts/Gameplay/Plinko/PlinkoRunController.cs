using UnityEngine;

using TMPro;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 런 전체를 관리
    /// 볼 생성, 활성 볼 제한, 진행 상태, 종료 및 정산을 담당
    /// </summary>
    public sealed class PlinkoRunController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlinkoBallSpawner ballSpawner;
        [SerializeField] private PlinkoRewardAccumulator rewardAccumulator;
        [SerializeField] private PlinkoSettlementController settlementController;

        [Header("Ball Drop Rules")]
        [SerializeField, Min(1)] private int maxActiveBalls = 8;
        [SerializeField, Min(0f)] private float spawnInterval = 0.08f;

        [Header("Debug")]
        [SerializeField] private TMP_Text text;

        private int _roundIndex;
        private int _pinballScore;

        private int _remainingBalls;
        private int _activeBalls;
        private int _spawnedBalls;

        private float _nextSpawnTime;
        private bool _isRunning;

        /// <summary>
        /// 현재 드랍 대기 중인 볼 개수
        /// </summary>
        public int RemainingBalls => _remainingBalls;

        /// <summary>
        /// 현재 보드 위에 활성화되어 있는 볼 개수
        /// </summary>
        public int ActiveBalls => _activeBalls;

        /// <summary>
        /// 이번 플링코 런에서 생성된 누적 볼 개수
        /// </summary>
        public int SpawnedBalls => _spawnedBalls;

        /// <summary>
        /// 플링코 런 시작
        /// </summary>
        public void BeginRun(int roundIndex, int pinballScore, int startBalls)
        {
            _roundIndex = roundIndex;
            _pinballScore = pinballScore;

            _remainingBalls = Mathf.Max(0, startBalls);
            _activeBalls = 0;
            _spawnedBalls = 0;

            _nextSpawnTime = 0f;
            _isRunning = true;

            if (rewardAccumulator != null)
            {
                rewardAccumulator.ResetForRun();
            }

            TrySpawnAvailableBalls();
            TryCompleteRun();
        }

        private void Update()
        {
            if (!_isRunning)
            {
                return;
            }

            if (text != null)
            {
                text.text = $"BallSpawned\n{SpawnedBalls}\n\nActiveBalls\n{ActiveBalls}";
            }

            TrySpawnAvailableBalls();
            TryCompleteRun();
        }

        /// <summary>
        /// 볼 하나가 해결 완료되었을 때 호출
        /// </summary>
        public void OnBallResolved(PlinkoBallActor actor)
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

            TrySpawnAvailableBalls();
            TryCompleteRun();
        }

        private void TrySpawnAvailableBalls()
        {
            if (!_isRunning)
            {
                return;
            }

            if (ballSpawner == null)
            {
                return;
            }

            if (_remainingBalls <= 0)
            {
                return;
            }

            if (_activeBalls >= maxActiveBalls)
            {
                return;
            }

            if (Time.time < _nextSpawnTime)
            {
                return;
            }

            while (_remainingBalls > 0 && _activeBalls < maxActiveBalls)
            {
                PlinkoBallActor actor = ballSpawner.SpawnBall(this);

                if (actor == null)
                {
                    return;
                }

                _remainingBalls--;
                _activeBalls++;
                _spawnedBalls++;

                _nextSpawnTime = Time.time + spawnInterval;

                if (spawnInterval > 0f)
                {
                    break;
                }
            }
        }

        private void TryCompleteRun()
        {
            if (!_isRunning)
            {
                return;
            }

            if (_remainingBalls > 0)
            {
                return;
            }

            if (_activeBalls > 0)
            {
                return;
            }

            CompleteRun();
        }

        private void CompleteRun()
        {
            _isRunning = false;
            rewardAccumulator?.FlushPendingCurrency();

            PlinkoRunResult result = rewardAccumulator != null ? rewardAccumulator.BuildResult() : new PlinkoRunResult(0, 0, 0);

            if (settlementController != null)
            {
                settlementController.CompleteRun(_roundIndex, _pinballScore, result);
            }
        }
    }
}
