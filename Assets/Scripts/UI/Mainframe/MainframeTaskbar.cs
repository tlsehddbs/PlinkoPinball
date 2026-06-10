using System.Collections.Generic;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlinkoPinball.UI.Mainframe
{
    [DisallowMultipleComponent]
    public sealed class MainframeTaskbar : MonoBehaviour
    {
        [SerializeField] private RectTransform buttonRoot;
        [SerializeField] private RectTransform tooltipRoot;
        [SerializeField] private TMP_Text tooltipText;
        [SerializeField] private MainframeTheme theme;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private Color accentColor = new Color(0.05f, 0.05f, 0.05f, 1f);
        [SerializeField] private Vector2 tooltipOffset = new Vector2(0f, 40f);

        private readonly List<MainframeLauncher.Entry> entries = new();
        private Button shutdownButton;
        private Action shutdownRequested;

        public void Initialize(RectTransform root, TMP_FontAsset fontAsset, Color accent, MainframeTheme mainframeTheme = null)
        {
            if (root != null)
            {
                buttonRoot = root;
            }

            theme = mainframeTheme != null ? mainframeTheme : theme;
            font = theme != null ? theme.ResolveFont(fontAsset) : fontAsset;
            accentColor = theme != null ? theme.accentColor : accent;
            ResolveReferences();
        }

        public void SetEntries(IReadOnlyList<MainframeLauncher.Entry> newEntries)
        {
            entries.Clear();

            if (newEntries != null)
            {
                entries.AddRange(newEntries);
            }

            BuildButtons();
        }

        public void SetShutdownHandler(Action handler)
        {
            shutdownRequested = handler;
        }

        private void BuildButtons()
        {
            ResolveReferences();

            if (buttonRoot == null)
            {
                return;
            }

            foreach (Transform child in buttonRoot)
            {
                Destroy(child.gameObject);
            }

            CreateOrUpdateShutdownButton();

            for (int i = 0; i < entries.Count; i++)
            {
                MainframeLauncher.Entry entry = entries[i];
                Button button = CreateButton(entry.label, i);
                button.onClick.AddListener(() => Activate(entry));

                MainframeTaskbarButton taskbarButton = button.gameObject.AddComponent<MainframeTaskbarButton>();
                taskbarButton.Initialize(this, entry);
            }

            CenterButtonRoot();
        }

        private Button CreateButton(string label, int index)
        {
            GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.layer = 5;
            buttonObject.transform.SetParent(buttonRoot, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(index * 54f + 22f, 0f);
            rect.sizeDelta = new Vector2(44f, 34f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = theme != null ? theme.buttonNormalColor : new Color(0.76f, 0.76f, 0.74f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = theme != null ? theme.buttonNormalColor : new Color(0.76f, 0.76f, 0.74f, 1f);
            colors.highlightedColor = theme != null ? theme.buttonHighlightedColor : new Color(0.88f, 0.88f, 0.86f, 1f);
            colors.pressedColor = theme != null ? theme.buttonPressedColor : new Color(0.56f, 0.56f, 0.54f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            TMP_Text text = MainframeWindowFrame.CreateText(
                "FallbackIcon",
                rect,
                GetFallbackIcon(label),
                font,
                13f,
                TextAlignmentOptions.Center);
            text.color = accentColor;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(6f, 0f);
            textRect.offsetMax = new Vector2(-6f, 0f);

            if (entries[index].icon != null)
            {
                text.gameObject.SetActive(false);
                Image iconImage = CreateIcon(rect, entries[index].icon);
                iconImage.color = accentColor;
            }

            return button;
        }

        private void CreateOrUpdateShutdownButton()
        {
            RectTransform parent = transform as RectTransform;
            if (parent == null)
            {
                return;
            }

            if (shutdownButton == null)
            {
                Transform existing = transform.Find("ShutdownButton");
                shutdownButton = existing != null ? existing.GetComponent<Button>() : null;
            }

            if (shutdownButton == null)
            {
                GameObject buttonObject = new GameObject("ShutdownButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                buttonObject.layer = 5;
                buttonObject.transform.SetParent(parent, false);
                shutdownButton = buttonObject.GetComponent<Button>();

                TMP_Text text = MainframeWindowFrame.CreateText(
                    "FallbackIcon",
                    buttonObject.GetComponent<RectTransform>(),
                    "OFF",
                    font,
                    12f,
                    TextAlignmentOptions.Center);
                text.color = accentColor;
                StretchToParent(text.GetComponent<RectTransform>(), new Vector2(5f, 0f), new Vector2(-5f, 0f));

                buttonObject.AddComponent<MainframeShutdownButton>();
            }

            RectTransform rect = shutdownButton.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 0.5f);
            rect.anchorMax = new Vector2(0f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(34f, 0f);
            rect.sizeDelta = new Vector2(58f, 34f);

            Image image = shutdownButton.GetComponent<Image>();
            image.color = theme != null ? theme.buttonNormalColor : new Color(0.76f, 0.76f, 0.74f, 1f);

            ColorBlock colors = shutdownButton.colors;
            colors.normalColor = theme != null ? theme.buttonNormalColor : new Color(0.76f, 0.76f, 0.74f, 1f);
            colors.highlightedColor = theme != null ? theme.buttonHighlightedColor : new Color(0.88f, 0.88f, 0.86f, 1f);
            colors.pressedColor = theme != null ? theme.buttonPressedColor : new Color(0.56f, 0.56f, 0.54f, 1f);
            colors.selectedColor = colors.highlightedColor;
            shutdownButton.colors = colors;

            shutdownButton.onClick.RemoveAllListeners();
            shutdownButton.onClick.AddListener(RequestShutdown);

            MainframeShutdownButton shutdownHover = shutdownButton.GetComponent<MainframeShutdownButton>();
            if (shutdownHover == null)
            {
                shutdownHover = shutdownButton.gameObject.AddComponent<MainframeShutdownButton>();
            }

            shutdownHover.Initialize(this, rect);
        }

        public void ShowTooltip(MainframeLauncher.Entry entry, RectTransform source)
        {
            ResolveReferences();

            if (entry == null || tooltipRoot == null || tooltipText == null || source == null)
            {
                return;
            }

            tooltipText.text = entry.label;
            tooltipRoot.gameObject.SetActive(true);
            tooltipRoot.position = source.position + new Vector3(tooltipOffset.x, tooltipOffset.y, 0f);
        }

        public void ShowTextTooltip(string label, RectTransform source)
        {
            ResolveReferences();

            if (string.IsNullOrWhiteSpace(label) || tooltipRoot == null || tooltipText == null || source == null)
            {
                return;
            }

            tooltipText.text = label;
            tooltipRoot.gameObject.SetActive(true);
            tooltipRoot.position = source.position + new Vector3(tooltipOffset.x, tooltipOffset.y, 0f);
        }

        public void HideTooltip(MainframeLauncher.Entry entry)
        {
            if (tooltipRoot != null)
            {
                tooltipRoot.gameObject.SetActive(false);
            }
        }

        private void ResolveReferences()
        {
            if (buttonRoot == null)
            {
                Transform taskButtons = transform.Find("TaskButtons");
                buttonRoot = taskButtons != null ? taskButtons as RectTransform : transform as RectTransform;
            }

            if (tooltipRoot == null)
            {
                Transform tooltip = transform.Find("Tooltip");
                tooltipRoot = tooltip as RectTransform;
            }

            if (tooltipText == null && tooltipRoot != null)
            {
                tooltipText = tooltipRoot.GetComponentInChildren<TMP_Text>(true);
            }

            if (tooltipRoot == null || tooltipText == null)
            {
                CreateTooltip();
            }

            if (tooltipRoot != null)
            {
                tooltipRoot.gameObject.SetActive(false);
            }
        }

        private void CenterButtonRoot()
        {
            if (buttonRoot == null)
            {
                return;
            }

            const float buttonWidth = 44f;
            const float gap = 10f;
            int count = entries.Count;
            float totalWidth = count <= 0 ? 0f : count * buttonWidth + (count - 1) * gap;

            buttonRoot.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRoot.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRoot.pivot = new Vector2(0.5f, 0.5f);
            buttonRoot.anchoredPosition = Vector2.zero;
            buttonRoot.sizeDelta = new Vector2(totalWidth, 38f);
        }

        private void CreateTooltip()
        {
            RectTransform parent = transform as RectTransform;
            if (parent == null)
            {
                return;
            }

            Color tooltipColor = theme != null ? theme.systemHeaderColor : new Color(0.72f, 0.72f, 0.7f, 1f);
            tooltipRoot = MainframeWindowFrame.CreatePanel("Tooltip", parent, tooltipColor, false);
            tooltipRoot.anchorMin = new Vector2(0.5f, 0f);
            tooltipRoot.anchorMax = new Vector2(0.5f, 0f);
            tooltipRoot.pivot = new Vector2(0.5f, 0f);
            tooltipRoot.sizeDelta = new Vector2(150f, 26f);

            tooltipText = MainframeWindowFrame.CreateText(
                "Label",
                tooltipRoot,
                string.Empty,
                font,
                13f,
                TextAlignmentOptions.Center);
            tooltipText.color = accentColor;

            RectTransform textRect = tooltipText.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 0f);
            textRect.offsetMax = new Vector2(-8f, 0f);
        }

        private static Image CreateIcon(RectTransform parent, Sprite sprite)
        {
            GameObject iconObject = new GameObject("Icon", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            iconObject.layer = 5;
            iconObject.transform.SetParent(parent, false);

            RectTransform iconRect = iconObject.GetComponent<RectTransform>();
            iconRect.anchorMin = Vector2.zero;
            iconRect.anchorMax = Vector2.one;
            iconRect.offsetMin = new Vector2(9f, 6f);
            iconRect.offsetMax = new Vector2(-9f, -6f);

            Image image = iconObject.GetComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            image.preserveAspect = true;
            return image;
        }

        private static string GetFallbackIcon(string label)
        {
            if (string.IsNullOrWhiteSpace(label))
            {
                return "--";
            }

            string value = label.ToUpperInvariant()
                .Replace(".EXE", string.Empty)
                .Replace(".SYS", string.Empty)
                .Replace(".", string.Empty);

            if (value.Length >= 2)
            {
                return value.Substring(0, 2);
            }

            return value.PadRight(2, '-');
        }

        private void RequestShutdown()
        {
            shutdownRequested?.Invoke();
        }

        private static void StretchToParent(RectTransform rect, Vector2 offsetMin, Vector2 offsetMax)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = offsetMin;
            rect.offsetMax = offsetMax;
        }

        private static void Activate(MainframeLauncher.Entry entry)
        {
            if (entry?.window == null)
            {
                return;
            }

            if (entry.processManager != null)
            {
                entry.processManager.TryOpen(entry);
                return;
            }

            entry.window.ShowWindow();
        }
    }

    public sealed class MainframeTaskbarButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private MainframeTaskbar owner;
        private MainframeLauncher.Entry entry;
        private RectTransform rect;

        public void Initialize(MainframeTaskbar taskbar, MainframeLauncher.Entry launcherEntry)
        {
            owner = taskbar;
            entry = launcherEntry;
            rect = GetComponent<RectTransform>();
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            owner?.ShowTooltip(entry, rect);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            owner?.HideTooltip(entry);
        }
    }

    public sealed class MainframeShutdownButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
    {
        private MainframeTaskbar owner;
        private RectTransform rect;

        public void Initialize(MainframeTaskbar taskbar, RectTransform source)
        {
            owner = taskbar;
            rect = source;
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            owner?.ShowTextTooltip("SHUT DOWN", rect);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            owner?.HideTooltip(null);
        }
    }
}
