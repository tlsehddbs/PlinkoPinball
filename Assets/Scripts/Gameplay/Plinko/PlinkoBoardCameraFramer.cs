using UnityEngine;

namespace PlinkoPinball.Gameplay.Core.Plinko
{
    /// <summary>
    /// 플링코 보드가 화면 안에 안정적으로 들어오도록 카메라 위치와 크기를 보정
    /// MVP 단계에서는 수동 세팅을 보조하는 용도로 사용
    /// </summary>
    [RequireComponent(typeof(Camera))]
    public sealed class PlinkoBoardCameraFramer : MonoBehaviour
    {
        [SerializeField] private PlinkoBoardLayoutRoot layoutRoot;
        [SerializeField, Min(0f)] private float padding = 1f;
        [SerializeField] private bool applyOnStart = true;

        private Camera _camera;

        private void Awake()
        {
            _camera = GetComponent<Camera>();
        }

        private void Start()
        {
            if (applyOnStart)
            {
                FrameBoard();
            }
        }

        /// <summary>
        /// 현재 보드 영역을 기준으로 카메라를 보정
        /// </summary>
        public void FrameBoard()
        {
            if (_camera == null || layoutRoot == null)
            {
                return;
            }

            //Vector3 center = layoutRoot.GetBoardCenter();
            float width = layoutRoot.GetBoardWidth();
            float height = layoutRoot.GetBoardHeight();

            //transform.position = new Vector3(center.x, transform.position.y, center.z);

            if (_camera.orthographic)
            {
                float aspect = _camera.aspect;
                float halfHeight = (height * 0.5f) + padding;
                float halfWidthAsHeight = ((width * 0.5f) + padding) / Mathf.Max(0.0001f, aspect);

                _camera.orthographicSize = Mathf.Max(halfHeight, halfWidthAsHeight);
            }
        }
    }
}