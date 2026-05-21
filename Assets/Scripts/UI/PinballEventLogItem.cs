using TMPro;
using UnityEngine;

namespace PlinkoPinball.UI
{
    /// <summary>
    /// Pinball Event Log의 단일 로그 라인 UI다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PinballEventLogItem : MonoBehaviour
    {
        [SerializeField] private TMP_Text messageText;

        public void SetMessage(string message)
        {
            if (messageText == null)
                return;

            messageText.text = message;
        }
    }
}