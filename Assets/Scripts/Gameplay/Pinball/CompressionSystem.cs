using System;
using UnityEngine;
using PlinkoPinball.Core.TableEvents;
using PlinkoPinball.Gameplay.Core.Plinko;

namespace PlinkoPinball.Gameplay.Core.Compression
{
    /// <summary>
    /// Pinball 기물 Hit 성과를 Plinko Start Ball로 변환하는 Global System.
    /// </summary>
    public sealed class CompressionSystem : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private CompressionSettings settings;
        [SerializeField] private PlinkoRunSnapshotBuilder snapshotBuilder;

        [Header("Event Filter")]
        [SerializeField] private string requiredTag = "compression";

        [Header("Runtime Modifiers")]
        [Tooltip("런타임 보정 배수. Permanent Upgrade / Chipset 효과가 있다면 외부에서 설정 가능.")]
        [Min(0f)]
        [SerializeField] private float gainMultiplier = 1f;

        [Header("Debug")]
        [SerializeField] private bool logEvents = false;

        private float _progress;

        public float Progress => _progress;
        public float Threshold => settings != null ? settings.Threshold : 1f;
        public float NormalizedProgress => Threshold > 0f ? Mathf.Clamp01(_progress / Threshold) : 0f;
        public float GainMultiplier => gainMultiplier;

        public event Action<float, float> ProgressChanged;
        public event Action<int> StartBallsGranted;

        private void OnEnable()
        {
            TableEventBus.OnEvent += OnTableEvent;
        }

        private void OnDisable()
        {
            TableEventBus.OnEvent -= OnTableEvent;
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

        private void AddProgress(float amount, string reason)
        {
            _progress += amount;

            int grantedBalls = 0;
            float threshold = Threshold;

            while (_progress >= threshold)
            {
                if (settings.MaxBallsGrantedPerEvent > 0 && grantedBalls >= settings.MaxBallsGrantedPerEvent)
                {
                    break;
                }

                _progress -= threshold;
                grantedBalls++;

                snapshotBuilder.AddStartBalls(1);
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