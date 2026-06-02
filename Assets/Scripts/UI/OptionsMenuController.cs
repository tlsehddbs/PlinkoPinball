using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace PlinkoPinball.UI
{
    [DisallowMultipleComponent]
    public sealed class OptionsMenuController : MonoBehaviour
    {
        private const string MasterVolumeKey = "Options.MasterVolume";
        private const string FullscreenKey = "Options.Fullscreen";
        private const string VSyncKey = "Options.VSync";
        private const string TargetFpsKey = "Options.TargetFps";

        [Header("References")]
        [SerializeField] private Canvas targetCanvas;
        [SerializeField] private GameObject root;
        [SerializeField] private Slider masterVolumeSlider;
        [SerializeField] private TMP_Text masterVolumeValueText;
        [SerializeField] private Toggle fullscreenToggle;
        [SerializeField] private Toggle vSyncToggle;
        [SerializeField] private Button targetFpsButton;
        [SerializeField] private TMP_Text targetFpsValueText;
        [SerializeField] private Button openButton;
        [SerializeField] private Button closeButton;
        [SerializeField] private Button resetButton;

        [Header("Behavior")]
        [SerializeField] private bool buildDefaultUI = true;
        [SerializeField] private bool buildDefaultOpenButton = true;
        [SerializeField] private bool openWithEscape = true;
        [SerializeField] private bool pauseGameWhileOpen = true;
        [SerializeField] private bool startOpen;

        private readonly int[] _fpsOptions = { 30, 60, 90, 120, 144, -1 };
        private bool _isOpen;
        private float _previousTimeScale = 1f;

        private void Awake()
        {
            EnsureEventSystem();

            if (root == null && buildDefaultUI)
            {
                BuildDefaultUI();
                root.SetActive(false);
            }

            if (openButton == null && buildDefaultOpenButton)
            {
                openButton = CreateOpenButton();
            }

            BindControls();
            LoadAndApply();
            SetOpen(startOpen, force: true);
        }

        private void Update()
        {
            if (!openWithEscape || Keyboard.current == null)
            {
                return;
            }

            if (Keyboard.current.escapeKey.wasPressedThisFrame)
            {
                Toggle();
            }
        }

        public void Open()
        {
            SetOpen(true);
        }

        public void Close()
        {
            SetOpen(false);
        }

        public void Toggle()
        {
            SetOpen(!_isOpen);
        }

        private void OnDisable()
        {
            if (_isOpen && pauseGameWhileOpen)
            {
                Time.timeScale = _previousTimeScale;
            }
        }

        public void ResetToDefaults()
        {
            ApplyMasterVolume(1f, save: true);
            ApplyFullscreen(true, save: true);
            ApplyVSync(false, save: true);
            ApplyTargetFps(60, save: true);
            RefreshControls();
        }

        private void SetOpen(bool open, bool force = false)
        {
            if (!force && _isOpen == open)
            {
                return;
            }

            _isOpen = open;

            if (root != null)
            {
                root.SetActive(open);
            }

            if (!pauseGameWhileOpen)
            {
                return;
            }

            if (open)
            {
                _previousTimeScale = Time.timeScale;
                Time.timeScale = 0f;
            }
            else
            {
                Time.timeScale = _previousTimeScale;
            }
        }

        private void LoadAndApply()
        {
            float volume = PlayerPrefs.GetFloat(MasterVolumeKey, 1f);
            bool fullscreen = PlayerPrefs.GetInt(FullscreenKey, Screen.fullScreen ? 1 : 0) != 0;
            bool vSync = PlayerPrefs.GetInt(VSyncKey, QualitySettings.vSyncCount > 0 ? 1 : 0) != 0;
            int targetFps = PlayerPrefs.GetInt(TargetFpsKey, 60);

            ApplyMasterVolume(volume, save: false);
            ApplyFullscreen(fullscreen, save: false);
            ApplyVSync(vSync, save: false);
            ApplyTargetFps(targetFps, save: false);
            RefreshControls();
        }

        private void BindControls()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.minValue = 0f;
                masterVolumeSlider.maxValue = 1f;
                masterVolumeSlider.onValueChanged.AddListener(value => ApplyMasterVolume(value, save: true));
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.onValueChanged.AddListener(value => ApplyFullscreen(value, save: true));
            }

            if (vSyncToggle != null)
            {
                vSyncToggle.onValueChanged.AddListener(value => ApplyVSync(value, save: true));
            }

            if (targetFpsButton != null)
            {
                targetFpsButton.onClick.AddListener(CycleTargetFps);
            }

            if (openButton != null)
            {
                openButton.onClick.AddListener(Open);
            }

            if (closeButton != null)
            {
                closeButton.onClick.AddListener(Close);
            }

            if (resetButton != null)
            {
                resetButton.onClick.AddListener(ResetToDefaults);
            }
        }

        private void ApplyMasterVolume(float value, bool save)
        {
            float clamped = Mathf.Clamp01(value);
            AudioListener.volume = clamped;

            if (masterVolumeValueText != null)
            {
                masterVolumeValueText.text = $"{Mathf.RoundToInt(clamped * 100f)}%";
            }

            if (save)
            {
                PlayerPrefs.SetFloat(MasterVolumeKey, clamped);
                PlayerPrefs.Save();
            }
        }

        private void ApplyFullscreen(bool value, bool save)
        {
            Screen.fullScreen = value;

            if (save)
            {
                PlayerPrefs.SetInt(FullscreenKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private void ApplyVSync(bool value, bool save)
        {
            QualitySettings.vSyncCount = value ? 1 : 0;

            if (save)
            {
                PlayerPrefs.SetInt(VSyncKey, value ? 1 : 0);
                PlayerPrefs.Save();
            }
        }

        private void ApplyTargetFps(int value, bool save)
        {
            Application.targetFrameRate = value;

            if (save)
            {
                PlayerPrefs.SetInt(TargetFpsKey, value);
                PlayerPrefs.Save();
            }
        }

        private void RefreshControls()
        {
            if (masterVolumeSlider != null)
            {
                masterVolumeSlider.SetValueWithoutNotify(AudioListener.volume);
            }

            if (masterVolumeValueText != null)
            {
                masterVolumeValueText.text = $"{Mathf.RoundToInt(AudioListener.volume * 100f)}%";
            }

            if (fullscreenToggle != null)
            {
                fullscreenToggle.SetIsOnWithoutNotify(Screen.fullScreen);
            }

            if (vSyncToggle != null)
            {
                vSyncToggle.SetIsOnWithoutNotify(QualitySettings.vSyncCount > 0);
            }

            if (targetFpsValueText != null)
            {
                targetFpsValueText.text = FormatTargetFps(Application.targetFrameRate);
            }
        }

        private int GetFpsOptionIndex(int targetFps)
        {
            for (int i = 0; i < _fpsOptions.Length; i++)
            {
                if (_fpsOptions[i] == targetFps)
                {
                    return i;
                }
            }

            return 1;
        }

        private void CycleTargetFps()
        {
            int nextIndex = GetFpsOptionIndex(Application.targetFrameRate) + 1;
            if (nextIndex >= _fpsOptions.Length)
            {
                nextIndex = 0;
            }

            ApplyTargetFps(_fpsOptions[nextIndex], save: true);
            RefreshControls();
        }

        private static string FormatTargetFps(int targetFps)
        {
            return targetFps <= 0 ? "Unlimited" : targetFps.ToString();
        }

        private void BuildDefaultUI()
        {
            Canvas canvas = targetCanvas != null ? targetCanvas : GetComponentInParent<Canvas>();
            if (canvas == null)
            {
                GameObject canvasObject = new GameObject("OptionsCanvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
                canvas = canvasObject.GetComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;

                CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
                scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920f, 1080f);
            }

            root = CreateRect("OptionsMenu", canvas.transform, new Vector2(0f, 0f), Vector2.zero, Vector2.one).gameObject;
            Image dim = root.AddComponent<Image>();
            dim.color = new Color(0f, 0f, 0f, 0.65f);

            RectTransform panel = CreateRect("Panel", root.transform, new Vector2(520f, 500f), new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f));
            Image panelImage = panel.gameObject.AddComponent<Image>();
            panelImage.color = new Color(0.07f, 0.08f, 0.1f, 0.96f);

            VerticalLayoutGroup layout = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(32, 32, 28, 28);
            layout.spacing = 18f;
            layout.childControlWidth = true;
            layout.childControlHeight = false;
            layout.childForceExpandWidth = true;
            layout.childForceExpandHeight = false;

            ContentSizeFitter fitter = panel.gameObject.AddComponent<ContentSizeFitter>();
            fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            CreateText("OPTIONS", panel, 34, TextAlignmentOptions.Center);
            CreateSliderRow(panel);
            fullscreenToggle = CreateToggleRow(panel, "Fullscreen");
            vSyncToggle = CreateToggleRow(panel, "VSync");
            CreateFpsRow(panel);

            RectTransform buttons = CreateRow(panel, "Buttons", 52f);
            HorizontalLayoutGroup buttonLayout = buttons.gameObject.AddComponent<HorizontalLayoutGroup>();
            buttonLayout.spacing = 12f;
            buttonLayout.childControlWidth = true;
            buttonLayout.childControlHeight = true;
            buttonLayout.childForceExpandWidth = true;
            buttonLayout.childForceExpandHeight = true;

            resetButton = CreateButton(buttons, "Reset");
            closeButton = CreateButton(buttons, "Close");
        }

        private Button CreateOpenButton()
        {
            RectTransform parent = transform as RectTransform;
            if (parent == null)
            {
                return null;
            }

            Button button = CreateButton(parent, "OPT");
            LayoutElement layout = button.GetComponent<LayoutElement>();
            if (layout == null)
            {
                layout = button.gameObject.AddComponent<LayoutElement>();
            }

            layout.preferredWidth = 76f;
            layout.preferredHeight = 44f;
            layout.flexibleWidth = 0f;

            return button;
        }

        private void CreateSliderRow(RectTransform parent)
        {
            RectTransform row = CreateRow(parent, "MasterVolume", 56f);
            CreateText("Master Volume", row, 20, TextAlignmentOptions.Left);

            GameObject sliderObject = new GameObject("Slider", typeof(RectTransform), typeof(Slider), typeof(Image));
            sliderObject.transform.SetParent(row, false);
            masterVolumeSlider = sliderObject.GetComponent<Slider>();
            masterVolumeSlider.targetGraphic = sliderObject.GetComponent<Image>();
            masterVolumeSlider.targetGraphic.color = new Color(0.18f, 0.2f, 0.24f, 1f);

            LayoutElement sliderLayout = sliderObject.AddComponent<LayoutElement>();
            sliderLayout.flexibleWidth = 1f;

            masterVolumeValueText = CreateText("100%", row, 18, TextAlignmentOptions.Right);
            masterVolumeValueText.rectTransform.sizeDelta = new Vector2(64f, 40f);
        }

        private Toggle CreateToggleRow(RectTransform parent, string label)
        {
            RectTransform row = CreateRow(parent, label, 48f);
            CreateText(label, row, 20, TextAlignmentOptions.Left);

            GameObject toggleObject = new GameObject("Toggle", typeof(RectTransform), typeof(Toggle), typeof(Image));
            toggleObject.transform.SetParent(row, false);
            Image image = toggleObject.GetComponent<Image>();
            image.color = new Color(0.2f, 0.22f, 0.27f, 1f);

            Toggle toggle = toggleObject.GetComponent<Toggle>();
            toggle.targetGraphic = image;
            toggle.graphic = image;

            LayoutElement layout = toggleObject.AddComponent<LayoutElement>();
            layout.preferredWidth = 44f;
            layout.preferredHeight = 32f;

            return toggle;
        }

        private void CreateFpsRow(RectTransform parent)
        {
            RectTransform row = CreateRow(parent, "Target FPS", 48f);
            CreateText("Target FPS", row, 20, TextAlignmentOptions.Left);
            targetFpsButton = CreateButton(row, "60");
            targetFpsValueText = targetFpsButton.GetComponentInChildren<TMP_Text>();
        }

        private Button CreateButton(RectTransform parent, string label)
        {
            GameObject buttonObject = new GameObject(label, typeof(RectTransform), typeof(Button), typeof(Image));
            buttonObject.transform.SetParent(parent, false);
            Image image = buttonObject.GetComponent<Image>();
            image.color = new Color(0.18f, 0.24f, 0.34f, 1f);

            CreateText(label, buttonObject.transform, 20, TextAlignmentOptions.Center);
            return buttonObject.GetComponent<Button>();
        }

        private RectTransform CreateRow(Transform parent, string name, float height)
        {
            RectTransform row = CreateRect(name, parent, new Vector2(0f, height), Vector2.zero, Vector2.one);
            HorizontalLayoutGroup layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 14f;
            layout.childControlWidth = true;
            layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;

            LayoutElement rowLayout = row.gameObject.AddComponent<LayoutElement>();
            rowLayout.preferredHeight = height;
            return row;
        }

        private TMP_Text CreateText(string text, Transform parent, int size, TextAlignmentOptions alignment)
        {
            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            TMP_Text tmp = textObject.GetComponent<TMP_Text>();
            tmp.text = text;
            tmp.fontSize = size;
            tmp.alignment = alignment;
            tmp.color = Color.white;

            LayoutElement layout = textObject.AddComponent<LayoutElement>();
            layout.flexibleWidth = 1f;
            layout.preferredHeight = Mathf.Max(36f, size + 12f);
            return tmp;
        }

        private RectTransform CreateRect(string name, Transform parent, Vector2 size, Vector2 anchorMin, Vector2 anchorMax)
        {
            GameObject obj = new GameObject(name, typeof(RectTransform));
            obj.transform.SetParent(parent, false);
            RectTransform rect = obj.GetComponent<RectTransform>();
            rect.anchorMin = anchorMin;
            rect.anchorMax = anchorMax;
            rect.anchoredPosition = Vector2.zero;
            rect.sizeDelta = size;
            return rect;
        }

        private static void EnsureEventSystem()
        {
            if (FindAnyObjectByType<EventSystem>() != null)
            {
                return;
            }

            GameObject eventSystem = new GameObject("EventSystem", typeof(EventSystem));
            eventSystem.AddComponent<InputSystemUIInputModule>();
        }
    }
}
