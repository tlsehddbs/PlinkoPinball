using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlinkoPinball.UI.Mainframe
{
    [DisallowMultipleComponent]
    public sealed class MainframeWindowFrame : MonoBehaviour, IPointerDownHandler
    {
        [SerializeField] private RectTransform root;
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private CanvasGroup canvasGroup;

        public RectTransform ContentRoot => contentRoot;
        public bool IsVisible => canvasGroup == null ? gameObject.activeSelf : canvasGroup.alpha > 0.001f;
        public event System.Action<MainframeWindowFrame, bool> VisibilityChanged;

        public void Initialize(string title)
        {
            if (root == null)
            {
                root = GetComponent<RectTransform>();
            }

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (titleText != null)
            {
                titleText.text = title;
            }
        }

        public void BringToFront()
        {
            transform.SetAsLastSibling();
        }

        public void ShowWindow()
        {
            bool wasVisible = IsVisible;

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                gameObject.SetActive(true);
                NotifyVisibilityChanged(wasVisible);
                return;
            }

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            BringToFront();
            NotifyVisibilityChanged(wasVisible);
        }

        public void HideWindow()
        {
            bool wasVisible = IsVisible;

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
            }

            if (canvasGroup == null)
            {
                return;
            }

            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            NotifyVisibilityChanged(wasVisible);
        }

        private void NotifyVisibilityChanged(bool wasVisible)
        {
            bool isVisible = IsVisible;
            if (wasVisible != isVisible)
            {
                VisibilityChanged?.Invoke(this, isVisible);
            }
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            BringToFront();
        }

        public GameObject AttachContent(GameObject prefab)
        {
            if (prefab == null || contentRoot == null)
            {
                return null;
            }

            GameObject instance = Instantiate(prefab, contentRoot);
            instance.name = prefab.name;
            StretchToParent(instance.GetComponent<RectTransform>());
            return instance;
        }

        public static MainframeWindowFrame Create(
            RectTransform parent,
            string title,
            Vector2 anchoredPosition,
            Vector2 size,
            Color accentColor,
            TMP_FontAsset font = null,
            MainframeTheme theme = null)
        {
            GameObject windowObject = new GameObject(title, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
            windowObject.layer = 5;
            windowObject.transform.SetParent(parent, false);

            RectTransform rect = windowObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = size;

            Image background = windowObject.GetComponent<Image>();
            background.color = theme != null ? theme.windowBackgroundColor : new Color(0.78f, 0.78f, 0.76f, 0.96f);
            background.raycastTarget = true;

            MainframeWindowFrame frame = windowObject.AddComponent<MainframeWindowFrame>();
            frame.root = rect;
            frame.canvasGroup = windowObject.GetComponent<CanvasGroup>();

            Color headerColor = theme != null ? theme.windowHeaderColor : new Color(0.62f, 0.62f, 0.6f, 1f);
            RectTransform header = CreatePanel("Header", rect, headerColor, true);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = new Vector2(0f, 30f);

            MainframeWindowDragHandle dragHandle = header.gameObject.AddComponent<MainframeWindowDragHandle>();
            dragHandle.Initialize(frame, rect);

            Button closeButton = CreateCloseButton(header, accentColor, font, theme);
            closeButton.onClick.AddListener(frame.HideWindow);

            TMP_Text titleLabel = CreateText("Title", header, title, font, 16f, TextAlignmentOptions.MidlineLeft);
            RectTransform titleRect = titleLabel.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(36f, 0f);
            titleRect.offsetMax = new Vector2(-10f, 0f);
            titleLabel.color = accentColor;
            frame.titleText = titleLabel;

            RectTransform divider = CreatePanel("Divider", rect, accentColor, false);
            divider.anchorMin = new Vector2(0f, 1f);
            divider.anchorMax = new Vector2(1f, 1f);
            divider.pivot = new Vector2(0.5f, 1f);
            divider.anchoredPosition = new Vector2(0f, -30f);
            divider.sizeDelta = new Vector2(0f, 1f);

            Color transparent = theme != null ? theme.transparentColor : new Color(0f, 0f, 0f, 0f);
            RectTransform content = CreatePanel("ContentRoot", rect, transparent, false);
            content.anchorMin = Vector2.zero;
            content.anchorMax = Vector2.one;
            content.offsetMin = new Vector2(6f, 6f);
            content.offsetMax = new Vector2(-6f, -36f);
            content.gameObject.AddComponent<RectMask2D>();
            frame.contentRoot = content;

            frame.Initialize(title);
            return frame;
        }

        private static Button CreateCloseButton(RectTransform parent, Color accentColor, TMP_FontAsset font, MainframeTheme theme)
        {
            GameObject buttonObject = new GameObject("HideButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.layer = 5;
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(17f, 0f);
            rect.sizeDelta = new Vector2(18f, 18f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = theme != null ? theme.closeNormalColor : new Color(0.74f, 0.74f, 0.72f, 1f);
            image.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = theme != null ? theme.closeNormalColor : new Color(0.74f, 0.74f, 0.72f, 1f);
            colors.highlightedColor = theme != null ? theme.closeHighlightedColor : new Color(0.88f, 0.88f, 0.86f, 1f);
            colors.pressedColor = theme != null ? theme.closePressedColor : new Color(0.58f, 0.18f, 0.16f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            TMP_Text label = CreateText("Label", rect, "x", font, 14f, TextAlignmentOptions.Center);
            label.color = accentColor;

            RectTransform labelRect = label.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            return button;
        }

        public static TMP_Text CreateText(
            string name,
            RectTransform parent,
            string value,
            TMP_FontAsset font,
            float size,
            TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObject.layer = 5;
            textObject.transform.SetParent(parent, false);

            TMP_Text text = textObject.GetComponent<TMP_Text>();
            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.color = Color.white;

            if (font != null)
            {
                text.font = font;
            }

            return text;
        }

        public static RectTransform CreatePanel(string name, RectTransform parent, Color color, bool raycastTarget)
        {
            GameObject panelObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.layer = 5;
            panelObject.transform.SetParent(parent, false);

            Image image = panelObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = raycastTarget;

            return panelObject.GetComponent<RectTransform>();
        }

        public static void StretchToParent(RectTransform rect)
        {
            if (rect == null)
            {
                return;
            }

            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = Vector2.zero;
            rect.localScale = Vector3.one;
        }
    }
}
