using TMPro;
using PlinkoPinball.UI.Mainframe;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace PlinkoPinball.UI.MainMenu
{
    [DisallowMultipleComponent]
    public sealed class MainMenuDosView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Canvas canvas;
        [SerializeField] private MainframeTheme theme;
        [SerializeField] private MainMenuButtonController controller;
        [SerializeField] private Button startButton;
        [SerializeField] private Button quitButton;
        [SerializeField] private GameObject optionButton;
        [SerializeField] private TMP_FontAsset font;

        [Header("Style")]
        [SerializeField] private Color backgroundColor = new Color(0.02f, 0.022f, 0.022f, 0.94f);
        [SerializeField] private Color terminalColor = new Color(0.78f, 0.78f, 0.75f, 1f);
        [SerializeField] private Vector2 panelSize = new Vector2(760f, 430f);

        private RectTransform panelRoot;
        private TMP_FontAsset EffectiveFont => theme != null ? theme.ResolveFont(font) : font;
        private Color BackgroundColor => theme != null ? theme.mainMenuBackgroundColor : backgroundColor;
        private Color TerminalColor => theme != null ? theme.accentColor : terminalColor;
        private Vector2 PanelSize => theme != null ? theme.mainMenuPanelSize : panelSize;

        private void Start()
        {
            Build();
        }

        [ContextMenu("Build DOS Main Menu")]
        public void Build()
        {
            ResolveReferences();

            if (canvas == null || startButton == null || quitButton == null)
            {
                Debug.LogWarning("[MainMenuDosView] Missing menu references.", this);
                return;
            }

            if (!OwnsPanelRoot() && panelRoot != null && panelRoot.GetComponent<MainMenuDosView>() != null)
            {
                enabled = false;
                return;
            }

            EnsureEventSystem();
            HideLegacyTitleTexts();
            optionButton?.SetActive(false);

            if (panelRoot == null)
            {
                DestroyExistingPanel();
                panelRoot = CreatePanel();
            }
            else
            {
                ConfigurePanel(panelRoot);
            }

            CreateLabels(panelRoot);
            ConfigureButton(startButton, "BOOT SYSTEM", new Vector2(0f, -78f), controller.StartGame);
            ConfigureButton(quitButton, "TERMINATE", new Vector2(0f, -132f), controller.QuitGame);
            ConfigureNavigation();

            if (EventSystem.current != null)
            {
                EventSystem.current.SetSelectedGameObject(startButton.gameObject);
            }
        }

        private void ResolveReferences()
        {
            if (canvas == null)
            {
                canvas = FindFirstObjectByType<Canvas>();
            }

            if (controller == null)
            {
                controller = GetComponent<MainMenuButtonController>();
            }

            if (controller == null)
            {
                controller = FindFirstObjectByType<MainMenuButtonController>();
            }

            if (controller == null)
            {
                controller = gameObject.AddComponent<MainMenuButtonController>();
            }

            if (panelRoot == null)
            {
                panelRoot = FindPanelRoot();
            }

            if (startButton == null)
            {
                startButton = FindButtonByName("StartButton", panelRoot);
            }

            if (quitButton == null)
            {
                quitButton = FindButtonByName("QuitButton", panelRoot);
            }

            if (optionButton == null)
            {
                optionButton = FindSceneObjectByTrimmedName("OptionButton", panelRoot);
            }
        }

        private bool OwnsPanelRoot()
        {
            return panelRoot != null && panelRoot == transform;
        }

        private RectTransform FindPanelRoot()
        {
            RectTransform selfRect = transform as RectTransform;
            if (selfRect != null && FindButtonByName("StartButton", selfRect) != null)
            {
                return selfRect;
            }

            if (canvas != null)
            {
                Transform canvasPanel = canvas.transform.Find("DosMainPanel");
                if (canvasPanel is RectTransform canvasPanelRect)
                {
                    return canvasPanelRect;
                }
            }

            GameObject scenePanel = FindSceneObjectByTrimmedName("DosMainPanel", null);
            return scenePanel != null ? scenePanel.GetComponent<RectTransform>() : null;
        }

        private RectTransform CreatePanel()
        {
            GameObject panelObject = new GameObject("DosMainPanel", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panelObject.layer = 5;
            panelObject.transform.SetParent(canvas.transform, false);

            RectTransform rect = panelObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = PanelSize;

            ConfigurePanel(rect);
            CreateBorder(rect);
            return rect;
        }

        private void ConfigurePanel(RectTransform rect)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = PanelSize;
            rect.localScale = Vector3.one;

            Image background = rect.GetComponent<Image>();
            if (background == null)
            {
                background = rect.gameObject.AddComponent<Image>();
            }

            background.color = BackgroundColor;
            background.raycastTarget = false;
        }

        private void CreateBorder(RectTransform parent)
        {
            CreateLine(parent, "Top", new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(0f, 2f));
            CreateLine(parent, "Bottom", new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 2f));
            CreateLine(parent, "Left", new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f), new Vector2(2f, 0f));
            CreateLine(parent, "Right", new Vector2(1f, 0f), new Vector2(1f, 1f), new Vector2(0f, 0f), new Vector2(2f, 0f));
        }

        private void CreateLine(RectTransform parent, string name, Vector2 anchorMin, Vector2 anchorMax, Vector2 position, Vector2 size)
        {
            GameObject lineObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            lineObject.layer = 5;
            lineObject.transform.SetParent(parent, false);

            RectTransform rect = lineObject.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            Image image = lineObject.GetComponent<Image>();
            image.color = TerminalColor;
            image.raycastTarget = false;
        }

        private void CreateLabels(RectTransform parent)
        {
            SetOrCreateText(parent, "Title", "PLINKO PINBALL", new Vector2(0f, 120f), 32f, TextAlignmentOptions.Center);
            SetOrCreateText(parent, "Status", "MAINFRAME OFFLINE", new Vector2(0f, 35f), 24f, TextAlignmentOptions.Center);
        }

        private TMP_Text SetOrCreateText(RectTransform parent, string name, string value, Vector2 position, float size, TextAlignmentOptions alignment)
        {
            Transform existing = parent.Find(name);
            TMP_Text text = existing != null ? existing.GetComponent<TMP_Text>() : null;

            if (text == null)
            {
                GameObject textObject = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
                textObject.layer = 5;
                textObject.transform.SetParent(parent, false);
                text = textObject.GetComponent<TMP_Text>();
            }

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(PanelSize.x - 80f, 48f);

            text.text = value;
            text.fontSize = size;
            text.alignment = alignment;
            text.textWrappingMode = TextWrappingModes.NoWrap;
            text.color = TerminalColor;

            if (EffectiveFont != null)
            {
                text.font = EffectiveFont;
            }

            return text;
        }

        private void ConfigureButton(Button button, string label, Vector2 position, UnityEngine.Events.UnityAction action)
        {
            button.gameObject.SetActive(true);
            button.transform.SetParent(panelRoot, false);

            RectTransform rect = button.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(420f, 44f);
            rect.localScale = Vector3.one;

            Image image = button.GetComponent<Image>();
            if (image != null)
            {
                image.color = theme != null ? theme.transparentColor : new Color(0f, 0f, 0f, 0f);
            }

            TMP_Text text = button.GetComponentInChildren<TMP_Text>(true);
            if (text != null)
            {
                text.text = label;
                text.fontSize = 24f;
                text.alignment = TextAlignmentOptions.MidlineLeft;
                text.color = TerminalColor;
                text.textWrappingMode = TextWrappingModes.NoWrap;

                if (EffectiveFont != null)
                {
                    text.font = EffectiveFont;
                }

                RectTransform textRect = text.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = new Vector2(82f, 0f);
                textRect.offsetMax = new Vector2(-20f, 0f);
            }

            MainMenuSelectionArrow arrow = button.GetComponent<MainMenuSelectionArrow>();
            if (arrow == null)
            {
                arrow = button.gameObject.AddComponent<MainMenuSelectionArrow>();
            }

            arrow.Initialize(TerminalColor, EffectiveFont);

            button.onClick.RemoveListener(action);
            button.onClick.AddListener(action);
        }

        private void ConfigureNavigation()
        {
            Navigation startNavigation = startButton.navigation;
            startNavigation.mode = Navigation.Mode.Explicit;
            startNavigation.selectOnDown = quitButton;
            startNavigation.selectOnUp = quitButton;
            startButton.navigation = startNavigation;

            Navigation quitNavigation = quitButton.navigation;
            quitNavigation.mode = Navigation.Mode.Explicit;
            quitNavigation.selectOnDown = startButton;
            quitNavigation.selectOnUp = startButton;
            quitButton.navigation = quitNavigation;
        }

        private void HideLegacyTitleTexts()
        {
            TMP_Text[] texts = FindObjectsByType<TMP_Text>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            for (int i = 0; i < texts.Length; i++)
            {
                TMP_Text text = texts[i];
                if (text == null)
                {
                    continue;
                }

                string objectName = text.gameObject.name;
                if (objectName == "TitleText" || objectName == "TitleText (1)")
                {
                    text.gameObject.SetActive(false);
                }
            }
        }

        private void DestroyExistingPanel()
        {
            Transform existing = canvas.transform.Find("DosMainPanel");
            if (existing == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(existing.gameObject);
            }
            else
            {
                DestroyImmediate(existing.gameObject);
            }
        }

        private static Button FindButtonByName(string objectName, Transform scope)
        {
            GameObject found = FindSceneObjectByTrimmedName(objectName, scope);
            return found != null ? found.GetComponent<Button>() : null;
        }

        private static GameObject FindSceneObjectByTrimmedName(string objectName, Transform scope)
        {
            Transform[] transforms = scope != null
                ? scope.GetComponentsInChildren<Transform>(true)
                : Resources.FindObjectsOfTypeAll<Transform>();

            for (int i = 0; i < transforms.Length; i++)
            {
                Transform transform = transforms[i];
                if (transform == null || transform.gameObject == null)
                {
                    continue;
                }

                if (scope == null && (!transform.gameObject.scene.IsValid() || !transform.gameObject.scene.isLoaded))
                {
                    continue;
                }

                if (transform.name.Trim() == objectName)
                {
                    return transform.gameObject;
                }
            }

            return null;
        }

        private static void EnsureEventSystem()
        {
            if (FindFirstObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystemObject = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
            eventSystemObject.layer = 5;
        }
    }
}
