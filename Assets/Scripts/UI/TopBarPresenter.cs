using UnityEngine;
using PlinkoPinball.Core;
using PlinkoPinball.Gameplay.Core.Compression;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.UI
{
    /// <summary>
    /// 전역 시스템 상태를 TopBarView에 전달
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class TopBarPresenter : MonoBehaviour
    {
        public enum TopBarMode
        {
            CreditsOnly,
            Pinball,
            Plinko
        }

        [Header("References")]
        [SerializeField] private TopBarView view;
        [SerializeField] private GameManager gameManager;
        [SerializeField] private ScoreSystem scoreSystem;
        [SerializeField] private CompressionSystem compressionSystem;
        [SerializeField] private PlinkoRunSnapshotBuilder snapshotBuilder;
        [SerializeField] private CurrencySystem currencySystem;
        [SerializeField] private bool autoResolveReferences = true;

        [Header("Mode")]
        [SerializeField] private TopBarMode mode;
        [SerializeField, Min(0.05f)] private float plinkoChargingDuration = 1.25f;

        [Header("Debug Values")]
        [SerializeField] private float powerCurrent = 100f;
        [SerializeField] private float powerMax = 100f;

        [SerializeField] private long throughput = 248520;
        [SerializeField] private int compressionCurrent = 7;
        [SerializeField] private int compressionThreshold = 10;
        [SerializeField] private int startBallCount = 12;
        [SerializeField] private long credits = 12480;

        private bool _subscribed;
        private float _plinkoCharge01;

        private void OnEnable()
        {
            Rebind();
        }

        private void Start()
        {
            Rebind();
        }

        private void Update()
        {
            if (mode != TopBarMode.Plinko || view == null)
            {
                return;
            }

            _plinkoCharge01 += Time.deltaTime / plinkoChargingDuration;
            if (_plinkoCharge01 > 1f)
            {
                _plinkoCharge01 -= Mathf.Floor(_plinkoCharge01);
            }

            view.SetSystemStatus("Charging...", _plinkoCharge01);
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        public void Rebind()
        {
            Unsubscribe();
            ResolveReferences();
            Subscribe();
            RefreshFromSystems();
            Refresh();
        }

        /// <summary>
        /// 현재 상태를 TopBar에 갱신한다.
        /// </summary>
        public void Refresh()
        {
            if (view == null)
            {
                return;
            }

            switch (mode)
            {
                case TopBarMode.CreditsOnly:
                    {
                        view.SetPowerVisible(false);
                        view.SetThroughputVisible(false);
                        view.SetCompressionVisible(false);
                        view.SetStartBallVisible(false);
                        view.SetCreditsVisible(true);
                        break;
                    }

                case TopBarMode.Pinball:
                    {
                        view.SetPowerVisible(true);
                        view.SetThroughputVisible(true);
                        view.SetCompressionVisible(true);
                        view.SetStartBallVisible(true);
                        view.SetCreditsVisible(true);
                        view.SetPower(powerCurrent, powerMax);
                        break;
                    }

                case TopBarMode.Plinko:
                    {
                        view.SetPowerVisible(true);
                        view.SetThroughputVisible(false);
                        view.SetCompressionVisible(false);
                        view.SetStartBallVisible(false);
                        view.SetCreditsVisible(true);
                        view.SetSystemStatus("Charging...", _plinkoCharge01);
                        break;
                    }
            }

            view.SetThroughput(throughput);
            view.SetCompression(compressionCurrent, compressionThreshold);
            view.SetStartBallCount(startBallCount);
            view.SetCredits(credits);
        }

        private void ResolveReferences()
        {
            if (view == null)
            {
                view = GetComponent<TopBarView>();
            }

            if (!autoResolveReferences)
            {
                return;
            }

            if (gameManager == null)
            {
                gameManager = GameManager.Instance;
            }

            if (scoreSystem == null)
            {
                scoreSystem = FindFirstObjectByType<ScoreSystem>();
            }

            if (compressionSystem == null)
            {
                compressionSystem = FindFirstObjectByType<CompressionSystem>();
            }

            if (snapshotBuilder == null)
            {
                snapshotBuilder = FindFirstObjectByType<PlinkoRunSnapshotBuilder>();
            }

            if (currencySystem == null)
            {
                currencySystem = CurrencySystem.Instance;
            }
        }

        private void Subscribe()
        {
            if (_subscribed)
            {
                return;
            }

            if (gameManager != null)
            {
                gameManager.OnPhaseChanged += HandlePhaseChanged;
                if (gameManager.Time != null)
                {
                    gameManager.Time.OnTimeChanged += HandleTimeChanged;
                }
            }

            if (scoreSystem != null)
            {
                scoreSystem.OnScoreChanged += HandleScoreChanged;
            }

            if (compressionSystem != null)
            {
                compressionSystem.ProgressChanged += HandleCompressionChanged;
            }

            if (snapshotBuilder != null)
            {
                snapshotBuilder.StartBallsChanged += SetStartBallCount;
            }

            if (currencySystem != null)
            {
                currencySystem.OnCurrencyChanged += SetCredits;
            }

            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed)
            {
                return;
            }

            if (gameManager != null)
            {
                gameManager.OnPhaseChanged -= HandlePhaseChanged;
                if (gameManager.Time != null)
                {
                    gameManager.Time.OnTimeChanged -= HandleTimeChanged;
                }
            }

            if (scoreSystem != null)
            {
                scoreSystem.OnScoreChanged -= HandleScoreChanged;
            }

            if (compressionSystem != null)
            {
                compressionSystem.ProgressChanged -= HandleCompressionChanged;
            }

            if (snapshotBuilder != null)
            {
                snapshotBuilder.StartBallsChanged -= SetStartBallCount;
            }

            if (currencySystem != null)
            {
                currencySystem.OnCurrencyChanged -= SetCredits;
            }

            _subscribed = false;
        }

        private void RefreshFromSystems()
        {
            if (gameManager != null)
            {
                ApplyPhase(gameManager.Phase);
                if (gameManager.Time != null)
                {
                    powerCurrent = gameManager.Time.RemainingSeconds;
                    powerMax = gameManager.StartTimeSeconds;
                }
            }
            else
            {
                mode = TopBarMode.CreditsOnly;
            }

            if (scoreSystem != null)
            {
                throughput = scoreSystem.CurrentScore;
            }

            if (compressionSystem != null)
            {
                compressionCurrent = Mathf.FloorToInt(compressionSystem.Progress);
                compressionThreshold = Mathf.CeilToInt(compressionSystem.Threshold);
            }

            if (snapshotBuilder != null)
            {
                startBallCount = snapshotBuilder.StartBalls;
            }

            if (currencySystem != null)
            {
                credits = currencySystem.CurrentCurrency;
            }
        }

        private void HandlePhaseChanged(GamePhase previous, GamePhase next)
        {
            if (next == GamePhase.Plinko)
            {
                _plinkoCharge01 = 0f;
            }

            ApplyPhase(next);
            Refresh();
        }

        private void ApplyPhase(GamePhase phase)
        {
            switch (phase)
            {
                case GamePhase.MainMenu:
                case GamePhase.None:
                    mode = TopBarMode.CreditsOnly;
                    break;

                case GamePhase.Pinball:
                    mode = TopBarMode.Pinball;
                    break;

                case GamePhase.Plinko:
                    mode = TopBarMode.Plinko;
                    break;
            }
        }

        private void HandleTimeChanged(float remainingSeconds)
        {
            powerCurrent = remainingSeconds;

            if (gameManager != null)
            {
                powerMax = gameManager.StartTimeSeconds;
            }

            if (mode == TopBarMode.Pinball)
            {
                Refresh();
            }
        }

        private void HandleCompressionChanged(float current, float threshold)
        {
            SetCompression(Mathf.FloorToInt(current), Mathf.CeilToInt(threshold));
        }

        private void HandleScoreChanged(int score)
        {
            SetThroughput(score);
        }

        public void SetMode(TopBarMode newMode)
        {
            mode = newMode;
            Refresh();
        }

        public void SetThroughput(long value)
        {
            throughput = value;

            Refresh();
        }

        public void SetCompression(int current, int threshold)
        {
            compressionCurrent = current;
            compressionThreshold = threshold;

            Refresh();
        }

        public void SetStartBallCount(int count)
        {
            startBallCount = count;

            Refresh();
        }

        public void SetCredits(long value)
        {
            credits = value;

            Refresh();
        }
    }
}
