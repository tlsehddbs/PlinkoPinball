using UnityEngine;
using UnityEngine.EventSystems;

namespace PlinkoPinball.UI.Upgrades
{
    [DisallowMultipleComponent]
    public sealed class UpgradeMapPanView : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        [Header("References")]
        [SerializeField] private RectTransform viewport;
        [SerializeField] private RectTransform content;

        [Header("Drag")]
        [SerializeField] private float dragSensitivity = 1f;

        [Header("Inertia")]
        [SerializeField] private bool useInertia = true;
        [SerializeField] private float inertiaMultiplier = 1f;
        [SerializeField] private float deceleration = 8f;
        [SerializeField] private float stopSpeed = 5f;

        [Header("Bounds")]
        [SerializeField] private bool clampToBounds = true;

        private Vector2 lastPointerPosition;
        private Vector2 velocity;
        private bool isDragging;

        public void OnBeginDrag(PointerEventData eventData)
        {
            isDragging = true;
            velocity = Vector2.zero;
            lastPointerPosition = eventData.position;
        }

        public void OnDrag(PointerEventData eventData)
        {
            if (viewport == null || content == null)
            {
                return;
            }

            float dt = Mathf.Max(Time.unscaledDeltaTime, 0.0001f);

            Vector2 currentPosition = eventData.position;
            Vector2 delta = currentPosition - lastPointerPosition;
            lastPointerPosition = currentPosition;

            Vector2 movement = delta * dragSensitivity;
            content.anchoredPosition += movement;

            velocity = movement / dt;

            if (clampToBounds)
            {
                ClampContent();
            }
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            isDragging = false;

            if (!useInertia)
            {
                velocity = Vector2.zero;
            }
            else
            {
                velocity *= inertiaMultiplier;
            }
        }

        private void Update()
        {
            if (isDragging || !useInertia || content == null)
            {
                return;
            }

            if (velocity.sqrMagnitude <= stopSpeed * stopSpeed)
            {
                velocity = Vector2.zero;
                return;
            }

            float dt = Time.unscaledDeltaTime;

            content.anchoredPosition += velocity * dt;

            velocity = Vector2.Lerp(velocity, Vector2.zero, deceleration * dt
            );

            if (clampToBounds)
            {
                ClampContent();
            }
        }

        private void ClampContent()
        {
            Vector2 viewportSize = viewport.rect.size;
            Vector2 contentSize = content.rect.size;

            Vector2 position = content.anchoredPosition;

            float limitX = Mathf.Max(0f, (contentSize.x - viewportSize.x) * 0.5f);
            float limitY = Mathf.Max(0f, (contentSize.y - viewportSize.y) * 0.5f);

            bool hitX = false;
            bool hitY = false;

            if (position.x < -limitX)
            {
                position.x = -limitX;
                hitX = true;
            }
            else if (position.x > limitX)
            {
                position.x = limitX;
                hitX = true;
            }

            if (position.y < -limitY)
            {
                position.y = -limitY;
                hitY = true;
            }
            else if (position.y > limitY)
            {
                position.y = limitY;
                hitY = true;
            }

            if (hitX)
            {
                velocity.x = 0f;
            }

            if (hitY)
            {
                velocity.y = 0f;
            }

            content.anchoredPosition = position;
        }
    }
}