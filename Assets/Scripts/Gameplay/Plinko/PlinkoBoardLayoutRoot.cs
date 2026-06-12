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
        /// 보드의 폭 반환
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
        /// 보드의 높이 반환
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
        /// 최상단 기준 월드 좌표를 반환
        /// </summary>
        public Vector3 GetTopLegtPosition()
        {
            return topLeft != null ? topLeft.position : transform.position;
        }

        /// <summary>
        /// 슬롯 스트립 기준 월드 좌표를 반환
        /// </summary>
        public Vector3 GetSlotStripPosition()
        {
            return slotStripAnchor != null ? slotStripAnchor.position : transform.position;
        }
    }
}