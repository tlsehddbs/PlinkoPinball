using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PlinkoPinball.Gameplay.Core.Compression;
using PlinkoPinball.UI;

namespace PlinkoPinball.UI.Mainframe
{
    [DisallowMultipleComponent]
    public sealed class MainframeOSBootstrap : MonoBehaviour
    {
        [Header("Content Prefabs")]
        [SerializeField] private GameObject pinballViewPrefab;
        [SerializeField] private GameObject plinkoViewPrefab;
        [SerializeField] private GameObject upgradeViewPrefab;
        [SerializeField] private GameObject statusViewPrefab;
        [SerializeField] private GameObject logViewPrefab;
        [SerializeField] private GameObject taskbarPrefab;
        [SerializeField] private GameObject popupDialogPrefab;
        [SerializeField] private GameObject crtOverlayPrefab;

        [Header("Runtime Prefabs")]
        [SerializeField] private GameObject plinkoBoardRootPrefab;
        [SerializeField] private GameObject plinkoRuntimeSystemsPrefab;
        [SerializeField] private CompressionSettings compressionSettings;

        [Header("Runtime Render Textures")]
        [SerializeField] private RenderTexture pinballRenderTexture;
        [SerializeField] private RenderTexture plinkoRenderTexture;

        [Header("Style")]
        [SerializeField] private MainframeTheme theme;
        [SerializeField] private TMP_FontAsset font;
        [SerializeField] private Color accentColor = new Color(0.05f, 0.05f, 0.05f, 1f);
        [SerializeField] private Color dimAccentColor = new Color(0.24f, 0.24f, 0.24f, 0.86f);

        [Header("Taskbar Icons")]
        [SerializeField] private Sprite pinballIcon;
        [SerializeField] private Sprite plinkoIcon;
        [SerializeField] private Sprite upgradeIcon;
        [SerializeField] private Sprite statusIcon;
        [SerializeField] private Sprite logIcon;

        [Header("Startup")]
        [SerializeField] private bool showPinballOnStart;
        [SerializeField] private bool showStatusOnStart;
        [SerializeField] private bool showLogOnStart;

        [Header("Memory")]
        [SerializeField, Min(1)] private int memoryLimit = 8;
        [SerializeField, Min(0)] private int pinballMemoryCost = 6;
        [SerializeField, Min(0)] private int plinkoMemoryCost = 6;
        [SerializeField, Min(0)] private int upgradeMemoryCost = 4;
        [SerializeField, Min(0)] private int statusMemoryCost = 1;
        [SerializeField, Min(0)] private int logMemoryCost = 1;

        private RectTransform desktopRoot;
        private RectTransform windowLayer;
        private TMP_Text memoryText;
        private TMP_Text processStatusText;
        private MainframeProcessManager processManager;
        private MainframePopupDialog popupDialog;
        private readonly List<MainframeLauncher.Entry> launcherEntries = new();
        private static readonly Vector2 WindowChromeSize = new Vector2(12f, 42f);
        private TMP_FontAsset EffectiveFont => theme != null ? theme.ResolveFont(font) : font;
        private Color AccentColor => theme != null ? theme.accentColor : accentColor;
        private Color DimAccentColor => theme != null ? theme.dimAccentColor : dimAccentColor;
        public MainframeTheme Theme => theme;

        private void Awake()
        {
            Build();
        }

        [ContextMenu("Rebuild Mainframe Desktop")]
        public void Build()
        {
            ClearGeneratedChildren();
            EnsureEventSystem();

            Canvas canvas = CreateCanvas();
            desktopRoot = canvas.GetComponent<RectTransform>();

            CreateBackground(desktopRoot);
            CreateSystemHeader(desktopRoot);
            processManager = ResolveProcessManager();

            windowLayer = CreateWindowLayer(desktopRoot);

            MainframeWindowFrame pinballWindow = CreateWindow(
                "PINBALL.EXE",
                pinballViewPrefab,
                new Vector2(-420f, 2f),
                WindowSizeForContent(new Vector2(720f, 960f)),
                showPinballOnStart,
                true);

            MainframeWindowFrame statusWindow = CreateWindow(
                "STATUS.SYS",
                statusViewPrefab,
                new Vector2(455f, 340f),
                new Vector2(470f, 220f),
                showStatusOnStart);

            MainframeWindowFrame logWindow = CreateWindow(
                "LOGVIEW.EXE",
                logViewPrefab,
                new Vector2(455f, -80f),
                new Vector2(470f, 520f),
                showLogOnStart);

            MainframeWindowFrame plinkoWindow = CreateWindow(
                "PLINKO.EXE",
                plinkoViewPrefab,
                new Vector2(-400f, 14f),
                WindowSizeForContent(new Vector2(790f, 920f)),
                false,
                true);

            MainframeWindowFrame upgradeWindow = CreateWindow(
                "UPGRADE.EXE",
                upgradeViewPrefab,
                new Vector2(60f, 0f),
                new Vector2(1050f, 820f),
                false);

            RegisterLauncherEntry("PINBALL.EXE", pinballWindow, pinballIcon, pinballMemoryCost);
            RegisterLauncherEntry("PLINKO.EXE", plinkoWindow, plinkoIcon, plinkoMemoryCost);
            RegisterLauncherEntry("UPGRADE.EXE", upgradeWindow, upgradeIcon, upgradeMemoryCost);
            RegisterLauncherEntry("STATUS.SYS", statusWindow, statusIcon, statusMemoryCost);
            RegisterLauncherEntry("LOGVIEW.EXE", logWindow, logIcon, logMemoryCost);
            processManager.SetEntries(launcherEntries);

            MainframeTaskbar taskbar = CreateTaskbar(desktopRoot);
            taskbar.SetShutdownHandler(ShowShutdownConfirmation);
            taskbar.SetEntries(launcherEntries);

            popupDialog = CreatePopupDialog(desktopRoot);
            processManager.MemoryLimitExceeded -= HandleMemoryLimitExceeded;
            processManager.MemoryLimitExceeded += HandleMemoryLimitExceeded;

            CreateCrtOverlay(desktopRoot);

            ConfigureRuntimeCoordinator();
            BringVisibleStartupWindowsToFront(pinballWindow, statusWindow, logWindow);
        }

        private void CreateCrtOverlay(RectTransform parent)
        {
            if (crtOverlayPrefab == null || parent == null)
            {
                return;
            }

            GameObject instance = Instantiate(crtOverlayPrefab, parent);
            instance.name = crtOverlayPrefab.name;
            instance.transform.SetAsLastSibling();

            RectTransform rect = instance.GetComponent<RectTransform>();
            MainframeWindowFrame.StretchToParent(rect);

            Graphic[] graphics = instance.GetComponentsInChildren<Graphic>(true);
            for (int i = 0; i < graphics.Length; i++)
            {
                if (graphics[i] != null)
                {
                    graphics[i].raycastTarget = false;
                }
            }
        }

        private MainframePopupDialog CreatePopupDialog(RectTransform parent)
        {
            MainframePopupDialog dialog = null;

            if (popupDialogPrefab != null)
            {
                GameObject instance = Instantiate(popupDialogPrefab, parent);
                instance.name = popupDialogPrefab.name;
                dialog = instance.GetComponent<MainframePopupDialog>();
            }

            if (dialog == null)
            {
                GameObject dialogObject = new GameObject("MainframePopupDialog", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
                dialogObject.layer = 5;
                dialogObject.transform.SetParent(parent, false);
                dialog = dialogObject.AddComponent<MainframePopupDialog>();
            }

            dialog.Initialize(theme, EffectiveFont);
            return dialog;
        }

        private void ConfigureRuntimeCoordinator()
        {
            if (!Application.isPlaying)
            {
                return;
            }

            MainframeRuntimeCoordinator coordinator = GetComponent<MainframeRuntimeCoordinator>();
            if (coordinator == null)
            {
                coordinator = gameObject.AddComponent<MainframeRuntimeCoordinator>();
            }

            coordinator.Initialize(
                processManager,
                plinkoBoardRootPrefab,
                plinkoRuntimeSystemsPrefab,
                compressionSettings,
                pinballRenderTexture,
                plinkoRenderTexture);
        }

        private void BringVisibleStartupWindowsToFront(params MainframeWindowFrame[] windows)
        {
            for (int i = 0; i < windows.Length; i++)
            {
                MainframeWindowFrame window = windows[i];
                if (window != null && window.IsVisible)
                {
                    window.BringToFront();
                }
            }
        }

        private void ClearGeneratedChildren()
        {
            if (processManager != null)
            {
                processManager.MemoryLimitExceeded -= HandleMemoryLimitExceeded;
            }

            for (int i = transform.childCount - 1; i >= 0; i--)
            {
                Transform child = transform.GetChild(i);

                if (Application.isPlaying)
                {
                    Destroy(child.gameObject);
                }
                else
                {
                    DestroyImmediate(child.gameObject);
                }
            }

            launcherEntries.Clear();
            popupDialog = null;
        }

        private Canvas CreateCanvas()
        {
            GameObject canvasObject = new GameObject("MainframeOSCanvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.layer = 5;
            canvasObject.transform.SetParent(transform, false);

            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 50;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;

            return canvas;
        }

        private void CreateBackground(RectTransform parent)
        {
            Color backgroundColor = theme != null ? theme.desktopBackgroundColor : new Color(0f, 0.36f, 0.45f, 1f);
            RectTransform background = MainframeWindowFrame.CreatePanel("DesktopBackground", parent, backgroundColor, false);
            background.anchorMin = Vector2.zero;
            background.anchorMax = Vector2.one;
            background.offsetMin = Vector2.zero;
            background.offsetMax = Vector2.zero;

            TMP_Text bootText = MainframeWindowFrame.CreateText(
                "BootSignature",
                background,
                "MAINFRAME OS // PLINKO PINBALL CONTROL LAYER",
                EffectiveFont,
                15f,
                TextAlignmentOptions.TopRight);
            bootText.color = DimAccentColor;

            RectTransform bootRect = bootText.GetComponent<RectTransform>();
            bootRect.anchorMin = Vector2.zero;
            bootRect.anchorMax = Vector2.one;
            bootRect.offsetMin = new Vector2(16f, 10f);
            bootRect.offsetMax = new Vector2(-18f, -14f);
        }

        private void CreateSystemHeader(RectTransform parent)
        {
            Color headerColor = theme != null ? theme.systemHeaderColor : new Color(0.72f, 0.72f, 0.7f, 1f);
            RectTransform header = MainframeWindowFrame.CreatePanel("SystemHeader", parent, headerColor, false);
            header.anchorMin = new Vector2(0f, 1f);
            header.anchorMax = new Vector2(1f, 1f);
            header.pivot = new Vector2(0.5f, 1f);
            header.anchoredPosition = Vector2.zero;
            header.sizeDelta = new Vector2(0f, 42f);

            TMP_Text title = MainframeWindowFrame.CreateText(
                "Title",
                header,
                "MAINFRAME OS",
                EffectiveFont,
                22f,
                TextAlignmentOptions.MidlineLeft);
            title.color = AccentColor;

            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchorMin = Vector2.zero;
            titleRect.anchorMax = Vector2.one;
            titleRect.offsetMin = new Vector2(16f, 0f);
            titleRect.offsetMax = new Vector2(-760f, 0f);

            processStatusText = MainframeWindowFrame.CreateText(
                "ProcessStatus",
                header,
                "READY",
                EffectiveFont,
                15f,
                TextAlignmentOptions.MidlineRight);
            processStatusText.color = AccentColor;

            RectTransform statusRect = processStatusText.GetComponent<RectTransform>();
            statusRect.anchorMin = new Vector2(0.5f, 0f);
            statusRect.anchorMax = new Vector2(1f, 1f);
            statusRect.offsetMin = new Vector2(110f, 0f);
            statusRect.offsetMax = new Vector2(-520f, 0f);

            memoryText = MainframeWindowFrame.CreateText(
                "Memory",
                header,
                "MEM [----------------] 0/8MB",
                EffectiveFont,
                15f,
                TextAlignmentOptions.MidlineRight);
            memoryText.color = AccentColor;

            RectTransform memoryRect = memoryText.GetComponent<RectTransform>();
            memoryRect.anchorMin = new Vector2(0.5f, 0f);
            memoryRect.anchorMax = Vector2.one;
            memoryRect.offsetMin = new Vector2(430f, 0f);
            memoryRect.offsetMax = new Vector2(-16f, 0f);

            RectTransform divider = MainframeWindowFrame.CreatePanel("Divider", parent, AccentColor, false);
            divider.anchorMin = new Vector2(0f, 1f);
            divider.anchorMax = new Vector2(1f, 1f);
            divider.pivot = new Vector2(0.5f, 1f);
            divider.anchoredPosition = new Vector2(0f, -42f);
            divider.sizeDelta = new Vector2(0f, 1f);
        }

        private RectTransform CreateWindowLayer(RectTransform parent)
        {
            Color transparent = theme != null ? theme.transparentColor : new Color(0f, 0f, 0f, 0f);
            RectTransform layer = MainframeWindowFrame.CreatePanel("WindowLayer", parent, transparent, false);
            layer.anchorMin = Vector2.zero;
            layer.anchorMax = Vector2.one;
            layer.offsetMin = new Vector2(20f, 64f);
            layer.offsetMax = new Vector2(-20f, -62f);
            return layer;
        }

        private MainframeTaskbar CreateTaskbar(RectTransform parent)
        {
            if (taskbarPrefab != null)
            {
                GameObject instance = Instantiate(taskbarPrefab, parent);
                instance.name = taskbarPrefab.name;

                RectTransform rect = instance.GetComponent<RectTransform>();
                ApplyTaskbarRect(rect);

                MainframeTaskbar prefabTaskbar = instance.GetComponent<MainframeTaskbar>();
                if (prefabTaskbar != null)
                {
                    prefabTaskbar.Initialize(null, EffectiveFont, AccentColor, theme);
                    return prefabTaskbar;
                }
            }

            return CreateRuntimeTaskbar(parent);
        }

        private MainframeTaskbar CreateRuntimeTaskbar(RectTransform parent)
        {
            Color taskbarColor = theme != null ? theme.taskbarBackgroundColor : new Color(0.72f, 0.72f, 0.7f, 1f);
            RectTransform taskbar = MainframeWindowFrame.CreatePanel("Taskbar", parent, taskbarColor, true);
            ApplyTaskbarRect(taskbar);

            RectTransform divider = MainframeWindowFrame.CreatePanel("Divider", taskbar, AccentColor, false);
            divider.anchorMin = new Vector2(0f, 1f);
            divider.anchorMax = new Vector2(1f, 1f);
            divider.pivot = new Vector2(0.5f, 1f);
            divider.anchoredPosition = Vector2.zero;
            divider.sizeDelta = new Vector2(0f, 1f);

            Color transparent = theme != null ? theme.transparentColor : new Color(0f, 0f, 0f, 0f);
            RectTransform buttonRoot = MainframeWindowFrame.CreatePanel("TaskButtons", taskbar, transparent, false);
            buttonRoot.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRoot.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRoot.pivot = new Vector2(0.5f, 0.5f);
            buttonRoot.anchoredPosition = Vector2.zero;
            buttonRoot.sizeDelta = new Vector2(0f, 38f);

            MainframeTaskbar taskbarComponent = taskbar.gameObject.AddComponent<MainframeTaskbar>();
            taskbarComponent.Initialize(buttonRoot, EffectiveFont, AccentColor, theme);
            return taskbarComponent;
        }

        private static Vector2 WindowSizeForContent(Vector2 contentSize)
        {
            return contentSize + WindowChromeSize;
        }

        private static void ApplyTaskbarRect(RectTransform taskbar)
        {
            if (taskbar == null)
            {
                return;
            }

            taskbar.anchorMin = Vector2.zero;
            taskbar.anchorMax = new Vector2(1f, 0f);
            taskbar.pivot = new Vector2(0.5f, 0f);
            taskbar.anchoredPosition = Vector2.zero;
            taskbar.sizeDelta = new Vector2(0f, 48f);
        }

        private MainframeWindowFrame CreateWindow(
            string title,
            GameObject contentPrefab,
            Vector2 anchoredPosition,
            Vector2 size,
            bool visible,
            bool fitRenderTextureContent = false)
        {
            MainframeWindowFrame window = MainframeWindowFrame.Create(
                windowLayer,
                title,
                anchoredPosition,
                size,
                AccentColor,
                EffectiveFont,
                theme);

            if (contentPrefab != null)
            {
                GameObject content = window.AttachContent(contentPrefab);
                if (fitRenderTextureContent)
                {
                    FitRenderTextureContent(content);
                }

                FitStatusAndLogContent(content);
            }
            else
            {
                CreatePlaceholder(window.ContentRoot, $"{title}\nNO CONTENT ROUTE");
            }

            if (visible)
            {
                window.ShowWindow();
            }
            else
            {
                window.HideWindow();
            }

            return window;
        }

        private static void FitRenderTextureContent(GameObject content)
        {
            if (content == null)
            {
                return;
            }

            RawImage[] rawImages = content.GetComponentsInChildren<RawImage>(true);
            for (int i = 0; i < rawImages.Length; i++)
            {
                RectTransform rect = rawImages[i].GetComponent<RectTransform>();
                MainframeWindowFrame.StretchToParent(rect);
                rawImages[i].raycastTarget = false;
            }
        }

        private static void FitStatusAndLogContent(GameObject content)
        {
            if (content == null)
            {
                return;
            }

            if (content.GetComponentInChildren<TopBarView>(true) != null)
            {
                TopBarView topBarView = content.GetComponentInChildren<TopBarView>(true);
                topBarView.SetTextColor(Color.black);
                StretchNamedChild(content.transform, "TopBar", new Vector2(8f, 8f), new Vector2(-8f, -8f));
                ArrangeStatusSections(content.transform);
            }

            PinballEventLogView logView = content.GetComponentInChildren<PinballEventLogView>(true);
            if (logView != null)
            {
                logView.SetTextColor(Color.black);
                StretchNamedChild(content.transform, "ViewPort", Vector2.zero, Vector2.zero);
                StretchNamedChild(content.transform, "Content", new Vector2(6f, 0f), new Vector2(-6f, 0f));
                ConfigureLogContentLayout(content.transform);
            }
        }

        private static void StretchNamedChild(Transform root, string childName, Vector2 offsetMin, Vector2 offsetMax)
        {
            if (root == null)
            {
                return;
            }

            RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++)
            {
                RectTransform rect = rects[i];
                if (rect == null || rect.name != childName)
                {
                    continue;
                }

                rect.anchorMin = Vector2.zero;
                rect.anchorMax = Vector2.one;
                rect.pivot = new Vector2(0.5f, 0.5f);
                rect.anchoredPosition = Vector2.zero;
                rect.sizeDelta = Vector2.zero;
                rect.offsetMin = offsetMin;
                rect.offsetMax = offsetMax;
                return;
            }
        }

        private static void ArrangeStatusSections(Transform root)
        {
            string[] sectionNames =
            {
                "Power",
                "Compression",
                "StartBall",
                "Throughput",
                "Credits"
            };

            const float topPadding = 6f;
            const float rowHeight = 29f;
            const float rowGap = 3f;

            for (int i = 0; i < sectionNames.Length; i++)
            {
                RectTransform section = FindRect(root, sectionNames[i]);
                if (section == null)
                {
                    continue;
                }

                float top = topPadding + i * (rowHeight + rowGap);
                section.anchorMin = new Vector2(0f, 1f);
                section.anchorMax = new Vector2(1f, 1f);
                section.pivot = new Vector2(0.5f, 1f);
                section.anchoredPosition = new Vector2(0f, -top);
                section.sizeDelta = new Vector2(0f, rowHeight);

                ArrangeStatusText(section, "Label", 0f, 0.34f, TextAlignmentOptions.MidlineLeft);
                ArrangeStatusText(section, "Value", 0.36f, 1f, TextAlignmentOptions.MidlineRight);
            }
        }

        private static void ArrangeStatusText(RectTransform section, string childName, float anchorMinX, float anchorMaxX, TextAlignmentOptions alignment)
        {
            RectTransform child = FindDirectRect(section, childName);
            if (child == null)
            {
                return;
            }

            child.anchorMin = new Vector2(anchorMinX, 0f);
            child.anchorMax = new Vector2(anchorMaxX, 1f);
            child.pivot = new Vector2(0.5f, 0.5f);
            child.anchoredPosition = Vector2.zero;
            child.offsetMin = Vector2.zero;
            child.offsetMax = Vector2.zero;

            TMP_Text text = child.GetComponent<TMP_Text>();
            if (text != null)
            {
                text.alignment = alignment;
                text.fontSize = Mathf.Min(text.fontSize, 16f);
                text.enableAutoSizing = true;
                text.fontSizeMin = 10f;
                text.fontSizeMax = 16f;
            }
        }

        private static RectTransform FindRect(Transform root, string childName)
        {
            RectTransform[] rects = root.GetComponentsInChildren<RectTransform>(true);
            for (int i = 0; i < rects.Length; i++)
            {
                if (rects[i] != null && rects[i].name == childName)
                {
                    return rects[i];
                }
            }

            return null;
        }

        private static void ConfigureLogContentLayout(Transform root)
        {
            RectTransform content = FindRect(root, "Content");
            if (content == null)
            {
                return;
            }

            ContentSizeFitter fitter = content.GetComponent<ContentSizeFitter>();
            if (fitter != null)
            {
                fitter.enabled = false;
            }

            VerticalLayoutGroup layout = content.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                layout.enabled = true;
                layout.childAlignment = TextAnchor.LowerLeft;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.reverseArrangement = false;
            }
        }

        private static RectTransform FindDirectRect(Transform root, string childName)
        {
            for (int i = 0; i < root.childCount; i++)
            {
                Transform child = root.GetChild(i);
                if (child != null && child.name == childName)
                {
                    return child as RectTransform;
                }
            }

            return null;
        }

        private void CreatePlaceholder(RectTransform parent, string message)
        {
            TMP_Text text = MainframeWindowFrame.CreateText(
                "Placeholder",
                parent,
                message,
                EffectiveFont,
                18f,
                TextAlignmentOptions.Center);
            text.color = DimAccentColor;

            RectTransform rect = text.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private MainframeProcessManager ResolveProcessManager()
        {
            MainframeProcessManager manager = GetComponent<MainframeProcessManager>();
            if (manager == null)
            {
                manager = gameObject.AddComponent<MainframeProcessManager>();
            }

            manager.Initialize(
                memoryLimit,
                memoryText,
                processStatusText,
                AccentColor,
                theme != null ? theme.warningColor : new Color(0.62f, 0.12f, 0.08f));
            return manager;
        }

        private void RegisterLauncherEntry(string label, MainframeWindowFrame window, Sprite icon, int memoryCost)
        {
            launcherEntries.Add(new MainframeLauncher.Entry
            {
                label = label,
                icon = icon,
                window = window,
                memoryCost = memoryCost,
                processManager = processManager
            });
        }

        private void HandleMemoryLimitExceeded(string processLabel, int requestedMemory, int usedMemory, int limit)
        {
            if (popupDialog == null)
            {
                return;
            }

            popupDialog.ShowAlert(
                "MEMORY LIMIT",
                $"{processLabel} cannot be loaded.\n\nRequired: {requestedMemory}MB\nAvailable: {Mathf.Max(0, limit - usedMemory)}MB\n\nSuspend another window and try again.",
                "OK");
        }

        private void ShowShutdownConfirmation()
        {
            if (popupDialog == null)
            {
                QuitGame();
                return;
            }

            popupDialog.ShowConfirm(
                "SHUT DOWN",
                "It is now safe to shut down Plinko Pinball.\n\nDo you want to terminate the system?",
                "SHUT DOWN",
                "CANCEL",
                QuitGame);
        }

        private static void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
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
