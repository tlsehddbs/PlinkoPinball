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
        [SerializeField] private TMP_Text messageText;

        public void SetMessage(string message)
        {
            if (messageText != null)
            {
                messageText.text = $"<mspace=20>{message}</mspcae>";
            }
        }
    }
}