using System;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Core.Flow;
using PlinkoPinball.Gameplay.Core.Plinko;
using PlinkoPinball.Gameplay.Upgrade;

namespace PlinkoPinball.Gameplay.Core.Compression
{
    /// <summary>
    /// Pinball 기물 Hit 성과를 Plinko Start Ball로 변환하는 Global System.
    /// </summary>
    public sealed class CompressionSystem : MonoBehaviour, IRoundResettable
    {
        [Header("References")]
        [SerializeField] private CompressionSettings settings;
        [SerializeField] private PlinkoRunSnapshotBuilder snapshotBuilder;
        [SerializeField] private UpgradeEffectResolver upgradeEffectResolver;

        [Header("Event Filter")]
        [SerializeField] private string requiredTag = "compression";

        [Header("Runtime Modifiers")]
        [Tooltip("런타임 보정 배수. Permanent Upgrade / Chipset 효과가 있다면 외부에서 설정 가능.")]
        [Min(0f)]
        [SerializeField] private float gainMultiplier = 1f;

        [Header("Debug")]
        [SerializeField] private bool logEvents = false;

        private CompressionReserveViewer _viewer;

        private float _progress;

        public float Progress => _progress;
        public float Threshold => GetCompressionThreshold();
        public float NormalizedProgress => Threshold > 0f ? Mathf.Clamp01(_progress / Threshold) : 0f;
        public float GainMultiplier => gainMultiplier;

        public event Action<float, float> ProgressChanged;
        public event Action<int> StartBallsGranted;

        private void Awake()
        {
            ResolveUpgradeEffectResolver();
        }

        private void OnEnable()
        {
            ResolveUpgradeEffectResolver();
            TableEventBus.OnEvent += OnTableEvent;
        }

        private void OnDisable()
        {
            TableEventBus.OnEvent -= OnTableEvent;
        }

        public void Start()
        {
            _viewer = FindAnyObjectByType<CompressionReserveViewer>();
        }

        private void OnTableEvent(TableEvent tableEvent)
        {
            if (settings == null || snapshotBuilder == null)
            {
                return;
            }

            if (!tableEvent.HasTag(requiredTag))
            {
                return;
            }

            float gain = CalculateGain(in tableEvent);
            if (gain <= 0f)
            {
                return;
            }

            AddProgress(gain, tableEvent.eventId);
        }

        /// <summary>
        /// 외부 업그레이드 시스템에서 Compression 효율 배수를 설정할 때 사용
        /// </summary>
        public void SetGainMultiplier(float multiplier)
        {
            gainMultiplier = Mathf.Max(0f, multiplier);
        }

        /// <summary>
        /// 외부 업그레이드 시스템에서 Compression 효율 배수를 더할 때 사용한
        /// 예: +0.25f = 25% 증가.
        /// </summary>
        public void AddGainMultiplierBonus(float bonus)
        {
            gainMultiplier = Mathf.Max(0f, gainMultiplier + bonus);
        }

        /// <summary>
        /// 라운드 시작 시 Runtime Progress를 초기화
        /// </summary>
        public void ResetProgress()
        {
            _progress = 0f;
            ProgressChanged?.Invoke(_progress, Threshold);
        }

        public void ResetForRound()
        {
            ResetProgress();
        }

        private float CalculateGain(in TableEvent tableEvent)
        {
            // float gain = settings.BaseGainPerEvent * gainMultiplier;

            // if (settings.MaxGainPerEvent > 0f)
            // {
            //     gain = Mathf.Min(gain, settings.MaxGainPerEvent);
            // }

            float gain = tableEvent.baseValue * gainMultiplier;

            return gain;
        }

        private float GetCompressionThreshold()
        {
            ResolveUpgradeEffectResolver();
            return upgradeEffectResolver != null ? upgradeEffectResolver.GetCompressionThreshold() : settings != null ? settings.Threshold : 1f;
        }

        private void ResolveUpgradeEffectResolver()
        {
            if (upgradeEffectResolver != null)
            {
                return;
            }

            upgradeEffectResolver = UpgradeEffectResolver.Instance;
            if (upgradeEffectResolver != null)
            {
                return;
            }

            upgradeEffectResolver = FindFirstObjectByType<UpgradeEffectResolver>();
        }

        private void AddProgress(float amount, string reason)
        {
            // TODO: Topbar
            _progress += amount;

            int grantedBalls = 0;
            float threshold = Threshold;

            while (_progress >= threshold)
            {
                if (settings.MaxBallsGrantedPerEvent > 0 && grantedBalls >= settings.MaxBallsGrantedPerEvent)
                {
                    break;
                }

                //TODO: grantedBalls가 2를 초과하여 작동하는 것에 대한 대응을 추가할 것
                _progress -= threshold;
                grantedBalls++;

                snapshotBuilder.AddStartBalls(1);
                _viewer?.SpawnBall();
            }

            ProgressChanged?.Invoke(_progress, threshold);

            if (grantedBalls > 0)
            {
                StartBallsGranted?.Invoke(grantedBalls);
            }

            if (logEvents)
            {
                Debug.Log($"[CompressionSystem] +{amount:0.##} progress / reason={reason} / progress={_progress:0.##}/{threshold:0.##} / balls+={grantedBalls}", this);
            }
        }
    }
}
