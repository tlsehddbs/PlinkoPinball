using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Compression
{
    /// <summary>
    /// Pinball Phase 중 Compression Progress 누적 규칙을 정의
    /// </summary>
    [CreateAssetMenu(menuName = "PlinkoPinball/Compression/Compression Settings")]
    public sealed class CompressionSettings : ScriptableObject
    {
        [Header("Progress")]
        [Tooltip("Plinko Start Ball 1개를 생성하기 위해 필요한 Compression Progress.")]
        [Min(1f)]
        [SerializeField] private float threshold = 10f;

        [Tooltip("compression 태그 이벤트 1회당 기본 Progress 증가량.")]
        [Min(0f)]
        [SerializeField] private float baseGainPerEvent = 1f;

        [Header("Clamp")]
        [Tooltip("한 이벤트에서 증가 가능한 최대 Progress. 0 이하이면 제한 없음.")]
        [Min(0f)]
        [SerializeField] private float maxGainPerEvent = 0f;

        [Tooltip("한 이벤트 처리로 생성 가능한 최대 Start Ball. 0 이하이면 제한 없음.")]
        [Min(0)]
        [SerializeField] private int maxBallsGrantedPerEvent = 0;

        public float Threshold => threshold;
        public float BaseGainPerEvent => baseGainPerEvent;
        public float MaxGainPerEvent => maxGainPerEvent;
        public int MaxBallsGrantedPerEvent => maxBallsGrantedPerEvent;
    }
}