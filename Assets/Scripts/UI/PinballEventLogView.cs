using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.UI
{
    /// <summary>
    /// Pinball 이벤트 로그를 UI에 누적 표시한다.
    /// 새 로그는 아래에 추가되고, 오래된 로그는 위로 밀린다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PinballEventLogView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private PinballEventLogItem itemPrefab;

        [Header("Log")]
        [SerializeField] private int maxVisibleLogs = 30;

        private readonly Queue<PinballEventLogItem> items = new();

        /// <summary>
        /// 로그 한 줄을 추가한다.
        /// </summary>
        public void AddLog(string message)
        {
            if (contentRoot == null || itemPrefab == null)
            {
                Debug.LogWarning($"{nameof(PinballEventLogView)}: Missing reference.", this);
                return;
            }

            PinballEventLogItem item = Instantiate(itemPrefab, contentRoot);
            item.SetMessage(message);

            items.Enqueue(item);

            while (items.Count > maxVisibleLogs)
            {
                PinballEventLogItem oldItem = items.Dequeue();

                if (oldItem != null)
                    Destroy(oldItem.gameObject);
            }
        }

        /// <summary>
        /// 모든 로그를 제거한다.
        /// </summary>
        public void Clear()
        {
            while (items.Count > 0)
            {
                PinballEventLogItem item = items.Dequeue();

                if (item != null)
                    Destroy(item.gameObject);
            }
        }
    }
}