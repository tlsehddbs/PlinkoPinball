using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 보드의 기준 영역과 주요 앵커를 제공하는 루트 컴포넌트
    /// 씬 내 배치 기준을 통일하고, 카메라와 UI 정렬 기준으로 사용
    /// </summary>
    public sealed class PlinkoBoardLayoutRoot : MonoBehaviour
    {
        [Header("Board Bounds")]
        [SerializeField] private Transform topLeft;
        [SerializeField] private Transform topRight;
        [SerializeField] private Transform bottomLeft;
        [SerializeField] private Transform bottomRight;

        [Header("Anchors")]
        [SerializeField] private Transform ballSpawnAnchor;
        [SerializeField] private Transform slotStripAnchor;
        [SerializeField] private Transform boardCenterAnchor;

        public Transform TopLeft => topLeft;
        public Transform TopRight => topRight;
        public Transform BottomLeft => bottomLeft;
        public Transform BottomRight => bottomRight;
        // 공 스폰은 영역 범위로 설정할 수 있게 변경할 수도 있음
        public Transform BallSpawnAnchor => ballSpawnAnchor;
        public Transform SlotStripAnchor => slotStripAnchor;
        public Transform BoardCenterAnchor => boardCenterAnchor;

        /// <summary>
        /// 현재 보드 월드 중심점을 반환
        /// </summary>
        public Vector3 GetBoardCenter()
        {
            if (boardCenterAnchor != null)
            {
                return boardCenterAnchor.position;
            }

            if (topLeft != null && bottomRight != null)
            {
                return (topLeft.position + bottomRight.position) * 0.5f;
            }

            return transform.position;
        }

        /// <summary>
        /// 현재 보드의 월드 폭을 반환
        /// </summary>
        public float GetBoardWidth()
        {
            if (topLeft == null || topRight == null)
            {
                return 0f;
            }

            return Vector3.Distance(topLeft.position, topRight.position);
        }

        /// <summary>
        /// 현재 보드의 월드 높이를 반환
        /// </summary>
        public float GetBoardHeight()
        {
            if (topLeft == null || bottomLeft == null)
            {
                return 0f;
            }

            return Vector3.Distance(topLeft.position, bottomLeft.position);
        }

        /// <summary>
        /// Authoring 단계에서 보드 기준 앵커를 자동 배치
        /// remarks:
        /// 보드는 XY 평면 기준으로 작성하며, Z는 0을 유지
        /// </remarks>
        /// </summary>
        /// <param name="left">좌측 경계 X</param>
        /// <param name="right">우측 경계 X</param>
        /// <param name="top">상단 경계 Y</param>
        /// <param name="bottom">하단 경계 Y</param>
        /// <param name="spawnYOffset">스폰 앵커 상단 오프셋</param>
        /// <param name="slotYOffset">슬롯 스트립 하단 오프셋</param>
        public void ApplyAuthoringBounds(float left, float right, float top, float bottom, float spawnYOffset, float slotYOffset)
        {
            if (topLeft != null)
            {
                topLeft.localPosition = new Vector3(left, top, 0f);
            }

            if (topRight != null)
            {
                topRight.localPosition = new Vector3(right, top, 0f);
            }

            if (bottomLeft != null)
            {
                bottomLeft.localPosition = new Vector3(left, bottom, 0f);
            }

            if (bottomRight != null)
            {
                bottomRight.localPosition = new Vector3(right, bottom, 0f);
            }

            float centerX = (left + right) * 0.5f;
            float centerY = (top + bottom) * 0.5f;

            if (boardCenterAnchor != null)
            {
                boardCenterAnchor.localPosition = new Vector3(centerX, centerY, 0f);
            }

            if (ballSpawnAnchor != null)
            {
                ballSpawnAnchor.localPosition = new Vector3(centerX, top + spawnYOffset, 0f);
            }

            if (slotStripAnchor != null)
            {
                slotStripAnchor.localPosition = new Vector3(centerX, bottom - slotYOffset, 0f);
            }
        }
    }
}