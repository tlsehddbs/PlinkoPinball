using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlinkoPinball.UI.Mainframe
{
    [DisallowMultipleComponent]
    public sealed class MainframePopupDialog : MonoBehaviour
    {
        [SerializeField] private CanvasGroup canvasGroup;
        [SerializeField] private RectTransform panelRoot;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text messageText;
        [SerializeField] private Button primaryButton;
        [SerializeField] private Button secondaryButton;
        [SerializeField] private TMP_Text primaryButtonText;
        [SerializeField] private TMP_Text secondaryButtonText;
        [SerializeField] private MainframeTheme theme;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private Color accentColor = new Color(0.05f, 0.05f, 0.05f, 1f);

        private Action primaryAction;
        private Action secondaryAction;

        private TMP_FontAsset EffectiveFont => theme != null ? theme.ResolveFont(font) : font;
        private Color AccentColor => theme != null ? theme.accentColor : accentColor;
        private Color WindowColor => theme != null ? theme.windowBackgroundColor : new Color(0.78f, 0.78f, 0.76f, 0.98f);
        private Color HeaderColor => theme != null ? theme.windowHeaderColor : new Color(0.62f, 0.62f, 0.60f, 1f);
        private Color ButtonColor => theme != null ? theme.buttonNormalColor : new Color(0.76f, 0.76f, 0.74f, 1f);
        private Color ButtonHighlightColor => theme != null ? theme.buttonHighlightedColor : new Color(0.88f, 0.88f, 0.86f, 1f);
        private Color ButtonPressedColor => theme != null ? theme.buttonPressedColor : new Color(0.56f, 0.56f, 0.54f, 1f);

        public void Initialize(MainframeTheme mainframeTheme, TMP_FontAsset fontAsset)
        {
            theme = mainframeTheme != null ? mainframeTheme : theme;
            font = theme != null ? theme.ResolveFont(fontAsset) : fontAsset;
            EnsureBuilt();
            Hide();
        }

        public void ShowAlert(string title, string message, string okLabel = "OK")
        {
            Show(title, message, okLabel, null, Hide, null);
        }

        public void ShowConfirm(string title, string message, string confirmLabel, string cancelLabel, Action onConfirm)
        {
            Show(title, message, confirmLabel, cancelLabel, onConfirm, Hide);
        }

        public void Hide()
        {
            EnsureBuilt();
            primaryAction = null;
            secondaryAction = null;
            canvasGroup.alpha = 0f;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
        }

        private void Show(
            string title,
            string message,
            string primaryLabel,
            string secondaryLabel,
            Action onPrimary,
            Action onSecondary)
        {
            EnsureBuilt();

            titleText.text = title;
            messageText.text = message;
            primaryButtonText.text = primaryLabel;
            secondaryButton.gameObject.SetActive(!string.IsNullOrWhiteSpace(secondaryLabel));
            secondaryButtonText.text = secondaryLabel ?? string.Empty;

            primaryAction = onPrimary;
            secondaryAction = onSecondary;

            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            transform.SetAsLastSibling();
            primaryButton.Select();
        }

        private void EnsureBuilt()
        {
            RectTransform root = transform as RectTransform;
            if (root == null)
            {
                root = gameObject.AddComponent<RectTransform>();
            }

            root.anchorMin = Vector2.zero;
            root.anchorMax = Vector2.one;
            root.offsetMin = Vector2.zero;
            root.offsetMax = Vector2.zero;

            if (canvasGroup == null)
            {
                canvasGroup = GetComponent<CanvasGroup>();
                if (canvasGroup == null)
                {
                    canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }

            if (panelRoot != null)
            {
                return;
            }

            Image overlay = GetComponent<Image>();
            if (overlay == null)
            {
                overlay = gameObject.AddComponent<Image>();
            }

            overlay.color = new Color(0f, 0f, 0f, 0.12f);
            overlay.raycastTarget = true;

            panelRoot = MainframeWindowFrame.CreatePanel("PopupPanel", root, WindowColor, true);
            panelRoot.anchorMin = new Vector2(0.5f, 0.5f);
            panelRoot.anchorMax = new Vector2(0.5f, 0.5f);
            panelRoot.pivot = new Vector2(0.5f, 0.5f);
            panelRoot.anchoredPosition = Vector2.zero;
            panelRoot.sizeDelta = new Vector2(460f, 210f);

            RectTransform header = MainframeWindowFrame.CreatePanel("Header", panelRoot, HeaderColor, false);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = new Vector2(0f, 30f);

            titleText = MainframeWindowFrame.CreateText("Title", header, string.Empty, EffectiveFont, 15f, TextAlignmentOptions.MidlineLeft);
            titleText.color = AccentColor;
            Stretch(titleText.GetComponent<RectTransform>(), new Vector2(12f, 0f), new Vector2(-12f, 0f));

            RectTransform divider = MainframeWindowFrame.CreatePanel("Divider", panelRoot, AccentColor, false);
            divider.anchorMin = new Vector2(0f, 1f);
            divider.anchorMax = new Vector2(1f, 1f);
            divider.pivot = new Vector2(0.5f, 1f);
            divider.anchoredPosition = new Vector2(0f, -30f);
            divider.sizeDelta = new Vector2(0f, 1f);

            messageText = MainframeWindowFrame.CreateText("Message", panelRoot, string.Empty, EffectiveFont, 16f, TextAlignmentOptions.TopLeft);
            messageText.color = AccentColor;
            messageText.textWrappingMode = TextWrappingModes.Normal;
            Stretch(messageText.GetComponent<RectTransform>(), new Vector2(22f, 56f), new Vector2(-22f, -76f));

            primaryButton = CreateButton("PrimaryButton", panelRoot, new Vector2(92f, 30f));
            primaryButtonText = primaryButton.GetComponentInChildren<TMP_Text>(true);
            primaryButton.onClick.AddListener(HandlePrimary);

            secondaryButton = CreateButton("SecondaryButton", panelRoot, new Vector2(-92f, 30f));
            secondaryButtonText = secondaryButton.GetComponentInChildren<TMP_Text>(true);
            secondaryButton.onClick.AddListener(HandleSecondary);
        }

        private Button CreateButton(string name, RectTransform parent, Vector2 anchoredPosition)
        {
            GameObject buttonObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.layer = 5;
            buttonObject.transform.SetParent(parent, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0f);
            rect.anchorMax = new Vector2(0.5f, 0f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = anchoredPosition;
            rect.sizeDelta = new Vector2(148f, 32f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = ButtonColor;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = ButtonColor;
            colors.highlightedColor = ButtonHighlightColor;
            colors.pressedColor = ButtonPressedColor;
            colors.selectedColor = ButtonHighlightColor;
            button.colors = colors;

            TMP_Text label = MainframeWindowFrame.CreateText("Label", rect, string.Empty, EffectiveFont, 14f, TextAlignmentOptions.Center);
            label.color = AccentColor;
            Stretch(label.GetComponent<RectTransform>(), Vector2.zero, Vector2.zero);
            return button;
        }

        private void HandlePrimary()
        {
            Action action = primaryAction;
            Hide();
            action?.Invoke();
        }

        private void HandleSecondary()
        {
            Action action = secondaryAction;
            Hide();
            action?.Invoke();
        }

        private static void Stretch(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }
    }
}
