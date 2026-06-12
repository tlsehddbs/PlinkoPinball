using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Compression
{
    /// <summary>
    /// 확보된 Plinko Start Ball을 Reserve UI에 시각적으로 추가한다.
    /// CompressionSystem의 계산 로직에는 관여하지 않는 표시 전용 Presenter다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class CompressionReserveViewer : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform container;
        [SerializeField] private CompressionReserveBall ballPrefab;
        [SerializeField] private CompressionReserveBallField field;

        [Header("Spawn")]
        [SerializeField] private Vector2 spawnPosition = new Vector2(0f, 40f);
        [SerializeField] private Vector2 velocityMin = new Vector2(-160f, 120f);
        [SerializeField] private Vector2 velocityMax = new Vector2(160f, 260f);

        /// <summary>
        /// Reserve UI 안에 Ball 하나를 생성한다.
        /// </summary>
        public void SpawnBall()
        {
            if (container == null || ballPrefab == null || field == null)
            {
                Debug.LogWarning($"{nameof(CompressionReserveViewer)}: Missing reference.", this);
                return;
            }

            CompressionReserveBall ball = Instantiate(ballPrefab, container);

            Vector2 velocity = new Vector2(Random.Range(velocityMin.x, velocityMax.x), Random.Range(velocityMin.y, velocityMax.y));

            field.AddBall(ball, spawnPosition, velocity);
        }
    }
}
