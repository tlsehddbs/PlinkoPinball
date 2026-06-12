using UnityEngine;
using UnityEngine.EventSystems;

namespace PlinkoPinball.Gameplay.Upgrade.UI
{
    [DisallowMultipleComponent]
    public sealed class UpgradeTreeZoomController : MonoBehaviour, IBeginDragHandler, IDragHandler,IEndDragHandler, IScrollHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform treeContent;

        [Header("Pan")]
        [SerializeField] private float panSpeed = 1f;
        [SerializeField] private float inertiaDamping = 8f;
        [SerializeField] private float stopVelocity = 5f;

        [Header("Zoom")]
        [SerializeField] private float minZoom = 0.45f;
        [SerializeField] private float maxZoom = 1.8f;
        [SerializeField] private float zoomStep = 0.15f;
        [SerializeField] private float zoomSmoothTime = 0.08f;

        private Vector2 lastPointerPosition;
        private Vector2 panVelocity;
        private bool isDragging;

        private float targetZoom = 1f;
        private float zoomVelocity;

        private void Awake()
        {
            if (treeContent == null)
            {
                Debug.LogError("[UpgradeTreePanZoomController] TreeContent is missing.");
                return;
            }

            targetZoom = treeContent.localScale.x;
        }

        private void Update()
        {
            UpdateInertia();
            UpdateZoom();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            lastPointerPosition = eventData.position;
            panVelocity = Vector2.zero;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (treeContent == null)
            {
                return;
            }

            Vector2 delta = eventData.position - lastPointerPosition;
            lastPointerPosition = eventData.position;

            Vector2 movement = delta * panSpeed;
            treeContent.anchoredPosition += movement;

            float deltaTime = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);
            panVelocity = movement / deltaTime;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;
        }

        public void OnScroll(PointerEventData eventData)
        {
            if (treeContent == null)
            {
                return;
            }

            targetZoom += eventData.scrollDelta.y * zoomStep;
            targetZoom = Mathf.Clamp(targetZoom, minZoom, maxZoom);
        }

        private void UpdateInertia()
        {
            if (isDragging || treeContent == null)
            {
                return;
            }

            if (panVelocity.sqrMagnitude <= stopVelocity * stopVelocity)
            {
                panVelocity = Vector2.zero;
                return;
            }

            treeContent.anchoredPosition += panVelocity * Time.unscaledDeltaTime;
            panVelocity = Vector2.Lerp(
                panVelocity,
                Vector2.zero,
                inertiaDamping * Time.unscaledDeltaTime
            );
        }

        private void UpdateZoom()
        {
            if (treeContent == null)
            {
                return;
            }

            float currentZoom = treeContent.localScale.x;

            float nextZoom = Mathf.SmoothDamp(
                currentZoom,
                targetZoom,
                ref zoomVelocity,
                zoomSmoothTime,
                Mathf.Infinity,
                Time.unscaledDeltaTime
            );

            treeContent.localScale = new Vector3(nextZoom, nextZoom, 1f);
        }
    }
}