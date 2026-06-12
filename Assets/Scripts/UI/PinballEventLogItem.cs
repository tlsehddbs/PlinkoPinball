using TMPro;
using UnityEngine;

namespace PlinkoPinball.UI
{
    /// <summary>
    /// Pinball 시스템 로그 한 줄을 표시하는 UI 아이템이다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PinballEventLogItem : MonoBehaviour
    {
        private const int MonospaceWidth = 14;

        [SerializeField] private TMP_Text messageText;

        private void Awake()
        {
            ConfigureText();
        }

        public void SetTextColor(Color color)
        {
            if (messageText != null)
            {
                messageText.color = color;
            }
        }

        public void SetMessage(string message)
        {
            if (messageText != null)
            {
                ConfigureText();
                messageText.text = $"<mspace={MonospaceWidth}>{message}</mspace>";
            }
        }

        private void ConfigureText()
        {
            if (messageText == null)
            {
                return;
            }

            messageText.textWrappingMode = TextWrappingModes.NoWrap;
            messageText.overflowMode = TextOverflowModes.Truncate;
            messageText.enableAutoSizing = true;
            messageText.fontSizeMin = 12f;
            messageText.fontSizeMax = 18f;
            messageText.characterSpacing = 0f;
        }
    }
}
