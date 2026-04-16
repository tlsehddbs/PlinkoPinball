using UnityEngine;

namespace PlinkoPinball.Debugging
{
    /// <summary>
    /// 플링코 핀 그리드 초안을 빠르게 배치하기 위한 보조 컴포넌트
    /// 초기 블록아웃용이며, 최종 배치 후 제거하거나 비활성화할 수 있음
    /// </summary>
    public sealed class PlinkoPinGridPreview : MonoBehaviour
    {
        [SerializeField] private GameObject pinPrefab;
        [SerializeField, Min(1)] private int rows = 8;
        [SerializeField, Min(1)] private int columns = 12;
        [SerializeField, Min(0.1f)] private float horizontalSpacing = 1f;
        [SerializeField, Min(0.1f)] private float verticalSpacing = 1f;
        [SerializeField] private bool staggerOddRows = true;

        /// <summary>
        /// 현재 설정 기준으로 핀 프리뷰 생성
        /// </summary>
        [ContextMenu("Generate Preview Grid")]
        public void GeneratePreviewGrid()
        {
            if (pinPrefab == null)
            {
                return;
            }

            ClearChildren();

            for (int row = 0; row < rows; row++)
            {
                float offset = (row % 2 == 1) ? horizontalSpacing * 0.5f : 0f;

                float totalWidth = (columns - 1) * horizontalSpacing;

                for (int col = 0; col < columns; col++)
                {
                    float x = col * horizontalSpacing - totalWidth * 0.5f + offset;
                    float y = -row * verticalSpacing;

                    Vector3 pos = new Vector3(x, y, 0);

                    Instantiate(pinPrefab, pos, Quaternion.identity, transform);
                }
            }
        }

        /// <summary>
        /// 하위 프리뷰 오브젝트 제거
        /// </summary>
        [ContextMenu("Clear Preview Grid")]
        public void ClearChildren()
        {
            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(transform.GetChild(i).gameObject);
            }
        }
    }
}