using UnityEngine;
using UnityEngine.UI;
using PlinkoPinball.UI.Mainframe;

namespace PlinkoPinball.Gameplay.Upgrade.UI
{
    [RequireComponent(typeof(RectTransform))]
    [RequireComponent(typeof(Image))]
    public sealed class UpgradeConnectionLine : MonoBehaviour
    {
        [SerializeField, Min(1f)] private float thickness = 4f;
        [SerializeField] private Image lineImage;
        [SerializeField] private MainframeTheme theme;

        private RectTransform rectTransform;

        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
            ApplyTheme();
        }

        public void ConfigureTheme(MainframeTheme mainframeTheme)
        {
            theme = mainframeTheme != null ? mainframeTheme : theme;
            ApplyTheme();
        }

        public void SetPoints(Vector2 from, Vector2 to)
        {
            if (rectTransform == null)
            {
                rectTransform = GetComponent<RectTransform>();
            }

            Vector2 delta = to - from;
            float length = delta.magnitude;
            float angle = Mathf.Atan2(delta.y, delta.x) * Mathf.Rad2Deg;

            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0f, 0.5f);

            rectTransform.anchoredPosition = from;
            rectTransform.sizeDelta = new Vector2(length, thickness);
            rectTransform.localRotation = Quaternion.Euler(0f, 0f, angle);
            ApplyTheme();
        }

        private void ApplyTheme()
        {
            if (lineImage == null)
            {
                lineImage = GetComponent<Image>();
            }

            if (theme == null)
            {
                MainframeOSBootstrap bootstrap = FindFirstObjectByType<MainframeOSBootstrap>();
                if (bootstrap != null)
                {
                    theme = bootstrap.Theme;
                }
            }

            if (lineImage != null)
            {
                lineImage.color = theme != null ? theme.dimAccentColor : new Color(0.24f, 0.24f, 0.24f, 0.86f);
            }
        }
    }
}
