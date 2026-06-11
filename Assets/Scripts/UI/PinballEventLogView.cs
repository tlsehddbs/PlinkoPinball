using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace PlinkoPinball.UI
{
    /// <summary>
    /// Pinball 이벤트 로그를 UI에 누적 표시한다.
    /// 새 로그는 아래에 추가되고, 오래된 로그는 위로 밀린다.
    /// 동일한 연속 로그는 선택적으로 xN 형태로 압축한다.
    /// </summary>
    [DisallowMultipleComponent]
    public sealed class PinballEventLogView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform contentRoot;
        [SerializeField] private PinballEventLogItem itemPrefab;

        [Header("Log")]
        [SerializeField, Min(1)] private int maxVisibleLogs = 14;
        [SerializeField] private bool collapseRepeatedLogs = true;
        [SerializeField] private bool lockContentToViewport = true;

        private readonly Queue<PinballEventLogItem> items = new();

        private string lastRawMessage;
        private int repeatCount;
        private PinballEventLogItem lastItem;

        private void OnEnable()
        {
            ConfigureLayout();
        }

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

            if (collapseRepeatedLogs && lastItem != null && message == lastRawMessage)
            {
                repeatCount++;
                lastItem.SetMessage($"{message}  x{repeatCount}");
                return;
            }

            repeatCount = 1;
            lastRawMessage = message;

            PinballEventLogItem item = Instantiate(itemPrefab, contentRoot);
            item.SetMessage(message);
            item.transform.SetAsLastSibling();

            items.Enqueue(item);
            lastItem = item;

            while (items.Count > maxVisibleLogs)
            {
                PinballEventLogItem oldItem = items.Dequeue();

                if (oldItem != null)
                {
                    if (oldItem == lastItem)
                    {
                        lastItem = null;
                    }

                    Destroy(oldItem.gameObject);
                }
            }

            RebuildLayout();
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
                {
                    Destroy(item.gameObject);
                }
            }

            lastRawMessage = null;
            repeatCount = 0;
            lastItem = null;
            RebuildLayout();
        }

        private void ConfigureLayout()
        {
            if (contentRoot == null)
            {
                return;
            }

            if (lockContentToViewport)
            {
                contentRoot.anchorMin = Vector2.zero;
                contentRoot.anchorMax = Vector2.one;
                contentRoot.pivot = new Vector2(0.5f, 0f);
                contentRoot.anchoredPosition = Vector2.zero;
                contentRoot.offsetMin = Vector2.zero;
                contentRoot.offsetMax = Vector2.zero;

                ContentSizeFitter fitter = contentRoot.GetComponent<ContentSizeFitter>();
                if (fitter != null)
                {
                    fitter.enabled = false;
                }
            }

            VerticalLayoutGroup layout = contentRoot.GetComponent<VerticalLayoutGroup>();
            if (layout != null)
            {
                layout.childAlignment = TextAnchor.LowerLeft;
                layout.childControlWidth = true;
                layout.childControlHeight = true;
                layout.childForceExpandWidth = true;
                layout.childForceExpandHeight = false;
                layout.reverseArrangement = false;
            }
        }

        private void RebuildLayout()
        {
            if (contentRoot == null)
            {
                return;
            }

            ConfigureLayout();
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentRoot);
        }
    }
}
