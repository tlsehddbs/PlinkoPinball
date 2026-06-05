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
        public Color accentColor = new Color(0.78f, 0.78f, 0.75f, 1f);
        public Color dimAccentColor = new Color(0.43f, 0.44f, 0.42f, 0.78f);
        public Color warningColor = new Color(1f, 0.64f, 0.32f, 1f);

        [Header("Surfaces")]
        public Color desktopBackgroundColor = new Color(0.02f, 0.022f, 0.022f, 0.96f);
        public Color systemHeaderColor = new Color(0.03f, 0.03f, 0.03f, 0.78f);
        public Color taskbarBackgroundColor = new Color(0.025f, 0.025f, 0.025f, 0.78f);
        public Color windowBackgroundColor = new Color(0.025f, 0.025f, 0.025f, 0.58f);
        public Color windowHeaderColor = new Color(0.04f, 0.04f, 0.04f, 0.82f);
        public Color transparentColor = new Color(0f, 0f, 0f, 0f);

        [Header("Buttons")]
        public Color buttonNormalColor = new Color(0.08f, 0.08f, 0.08f, 0.42f);
        public Color buttonHighlightedColor = new Color(0.24f, 0.24f, 0.23f, 0.74f);
        public Color buttonPressedColor = new Color(0.36f, 0.36f, 0.34f, 0.88f);
        public Color closeNormalColor = new Color(0.08f, 0.08f, 0.08f, 0.32f);
        public Color closeHighlightedColor = new Color(0.42f, 0.25f, 0.16f, 0.78f);
        public Color closePressedColor = new Color(0.58f, 0.32f, 0.18f, 0.9f);

        [Header("Main Menu")]
        public Color mainMenuBackgroundColor = new Color(0.02f, 0.022f, 0.022f, 0.94f);
        public Vector2 mainMenuPanelSize = new Vector2(760f, 430f);

        public TMP_FontAsset ResolveFont(TMP_FontAsset fallback)
        {
            return font != null ? font : fallback;
        }
    }
}
