using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace PlinkoPinball.UI.Mainframe
{
    [DisallowMultipleComponent]
    public sealed class MainframeLauncher : MonoBehaviour
    {
        [Serializable]
        public sealed class Entry
        {
            public string label;
            public Sprite icon;
            public MainframeWindowFrame window;
            public int memoryCost;
            public MainframeProcessManager processManager;
        }

        [SerializeField] private RectTransform buttonRoot;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private Color accentColor = new Color(0.05f, 0.05f, 0.05f, 1f);

        private readonly List<Entry> entries = new();

        public void Initialize(RectTransform root, TMP_FontAsset fontAsset, Color accent)
        {
            buttonRoot = root;
            font = fontAsset;
            accentColor = accent;
        }

        public void SetEntries(IReadOnlyList<Entry> newEntries)
        {
            entries.Clear();

            if (newEntries != null)
            {
                entries.AddRange(newEntries);
            }

            BuildButtons();
        }

        private void BuildButtons()
        {
            if (buttonRoot == null)
            {
                return;
            }

            foreach (Transform child in buttonRoot)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < entries.Count; i++)
            {
                Entry entry = entries[i];
                Button button = CreateButton(entry.label, i);
                button.onClick.AddListener(() => Activate(entry));
            }
        }

        private Button CreateButton(string label, int index)
        {
            GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.layer = 5;
            buttonObject.transform.SetParent(buttonRoot, false);

            RectTransform rect = buttonObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -index * 34f);
            rect.sizeDelta = new Vector2(0f, 28f);

            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.76f, 0.76f, 0.74f, 1f);

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.76f, 0.76f, 0.74f, 1f);
            colors.highlightedColor = new Color(0.88f, 0.88f, 0.86f, 1f);
            colors.pressedColor = new Color(0.56f, 0.56f, 0.54f, 1f);
            colors.selectedColor = colors.highlightedColor;
            button.colors = colors;

            TMP_Text text = MainframeWindowFrame.CreateText(
                "Label",
                rect,
                $"> {label}",
                font,
                15f,
                TextAlignmentOptions.MidlineLeft);
            text.color = accentColor;

            RectTransform textRect = text.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(8f, 0f);
            textRect.offsetMax = new Vector2(-8f, 0f);

            return button;
        }

        private static void Activate(Entry entry)
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
}
