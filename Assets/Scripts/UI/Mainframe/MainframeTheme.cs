using TMPro;
using UnityEngine;

namespace PlinkoPinball.UI.Mainframe
{
    [CreateAssetMenu(menuName = "Plinko Pinball/UI/Mainframe Theme", fileName = "MainframeTheme")]
    public sealed class MainframeTheme : ScriptableObject
    {
        [Header("Typography")]
        public TMP_FontAsset font;

        [Header("Terminal")]
        public Color accentColor = new Color(0.05f, 0.05f, 0.05f, 1f);
        public Color dimAccentColor = new Color(0.24f, 0.24f, 0.24f, 0.86f);
        public Color warningColor = new Color(0.62f, 0.12f, 0.08f, 1f);

        [Header("Surfaces")]
        public Color desktopBackgroundColor = new Color(0f, 0.36f, 0.45f, 1f);
        public Color systemHeaderColor = new Color(0.72f, 0.72f, 0.70f, 1f);
        public Color taskbarBackgroundColor = new Color(0.72f, 0.72f, 0.70f, 1f);
        public Color windowBackgroundColor = new Color(0.78f, 0.78f, 0.76f, 0.96f);
        public Color windowHeaderColor = new Color(0.62f, 0.62f, 0.60f, 1f);
        public Color transparentColor = new Color(0f, 0f, 0f, 0f);

        [Header("Buttons")]
        public Color buttonNormalColor = new Color(0.76f, 0.76f, 0.74f, 1f);
        public Color buttonHighlightedColor = new Color(0.88f, 0.88f, 0.86f, 1f);
        public Color buttonPressedColor = new Color(0.56f, 0.56f, 0.54f, 1f);
        public Color closeNormalColor = new Color(0.74f, 0.74f, 0.72f, 1f);
        public Color closeHighlightedColor = new Color(0.88f, 0.88f, 0.86f, 1f);
        public Color closePressedColor = new Color(0.58f, 0.18f, 0.16f, 1f);

        [Header("Main Menu")]
        public Color mainMenuBackgroundColor = new Color(0f, 0.36f, 0.45f, 1f);
        public Vector2 mainMenuPanelSize = new Vector2(760f, 430f);

        public TMP_FontAsset ResolveFont(TMP_FontAsset fallback)
        {
            return font != null ? font : fallback;
        }
    }
}
