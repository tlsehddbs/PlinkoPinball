using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Compression
{
    /// <summary>
    /// UI 패널 내부에서 물리처럼 움직이는 Plinko Start Ball 시각화
    // 실제 Plinko Runtime Ball과는 무관한
    /// </summary>
    [DisallowMultipleComponent]
    [RequireComponent(typeof(RectTransform))]
    public sealed class CompressionReserveBall : MonoBehaviour
    {
        [SerializeField] private float radius = 13f;

        public RectTransform RectTransform { get; private set; }
        public Vector2 Velocity { get; set; }
        public float Radius => radius;
        public bool IsSleeping { get; private set; }

        private void Awake()
        {
            RectTransform = GetComponent<RectTransform>();
        }

        public void Wake()
        {
            IsSleeping = false;
        }

        public void Sleep()
        {
            Velocity = Vector2.zero;
            IsSleeping = true;
        }
    }
}
