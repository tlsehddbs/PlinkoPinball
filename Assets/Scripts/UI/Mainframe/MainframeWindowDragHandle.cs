using UnityEngine;
using UnityEngine.EventSystems;

namespace PlinkoPinball.UI.Mainframe
{
    [DisallowMultipleComponent]
    public sealed class MainframeWindowDragHandle : MonoBehaviour, IPointerDownHandler, IBeginDragHandler, IDragHandler
    {
        [SerializeField] private MainframeWindowFrame frame;
        [SerializeField] private RectTransform windowRect;

        private RectTransform parentRect;
        private Vector2 dragStartPointer;
        private Vector2 dragStartWindowPosition;

        public void Initialize(MainframeWindowFrame owner, RectTransform target)
        {
            frame = owner;
            windowRect = target;
            parentRect = target != null ? target.parent as RectTransform : null;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            frame?.BringToFront();
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            ResolveReferences();
            frame?.BringToFront();

            if (parentRect == null || windowRect == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out dragStartPointer);

            dragStartWindowPosition = windowRect.anchoredPosition;
        }

        public void OnDrag(PointerEventData eventData)
        {
            ResolveReferences();

            if (parentRect == null || windowRect == null)
            {
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                parentRect,
                eventData.position,
                eventData.pressEventCamera,
                out Vector2 currentPointer);

            windowRect.anchoredPosition = dragStartWindowPosition + currentPointer - dragStartPointer;
        }

        private void ResolveReferences()
        {
            if (windowRect == null && frame != null)
            {
                windowRect = frame.GetComponent<RectTransform>();
            }

            if (parentRect == null && windowRect != null)
            {
                parentRect = windowRect.parent as RectTransform;
            }
        }
    }
}
