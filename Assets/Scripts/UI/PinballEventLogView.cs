using System.Collections.Generic;
using TMPro;
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
        [SerializeField] private Color textColor = Color.black;

        private readonly Queue<PinballEventLogItem> items = new();

        private string lastRawMessage;
        private int repeatCount;
        private PinballEventLogItem lastItem;
        private VerticalLayoutGroup cachedLayout;

        private void OnEnable()
        {
            ConfigureLayout();
            ApplyTextColor();
        }

        public void SetTextColor(Color color)
        {
            textColor = color;
            ApplyTextColor();
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
                RebuildLayout();
                return;
            }

            repeatCount = 1;
            lastRawMessage = message;

            PinballEventLogItem item = Instantiate(itemPrefab, contentRoot);
            item.SetTextColor(textColor);
            item.SetMessage(message);
            item.transform.SetAsLastSibling();

            items.Enqueue(item);
            lastItem = item;

            TrimOverflowLogs();

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

            cachedLayout = contentRoot.GetComponent<VerticalLayoutGroup>();
            if (cachedLayout != null)
            {
                cachedLayout.enabled = true;
                cachedLayout.childAlignment = TextAnchor.LowerLeft;
                cachedLayout.childControlWidth = true;
                cachedLayout.childControlHeight = true;
                cachedLayout.childForceExpandWidth = true;
                cachedLayout.childForceExpandHeight = false;
                cachedLayout.reverseArrangement = false;
            }
        }

        private void TrimOverflowLogs()
        {
            int limit = GetVisibleLogLimit();
            while (items.Count > limit)
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
        }

        private int GetVisibleLogLimit()
        {
            int limit = Mathf.Max(1, maxVisibleLogs);

            if (!lockContentToViewport || contentRoot == null)
            {
                return limit;
            }

            float contentHeight = contentRoot.rect.height;
            if (contentHeight <= 0f)
            {
                return limit;
            }

            VerticalLayoutGroup layout = cachedLayout != null ? cachedLayout : contentRoot.GetComponent<VerticalLayoutGroup>();
            float spacing = layout != null ? layout.spacing : 0f;
            float padding = layout != null ? layout.padding.top + layout.padding.bottom : 0f;
            float rowHeight = GetItemHeight();

            if (rowHeight <= 0f)
            {
                return limit;
            }

            float usableHeight = Mathf.Max(0f, contentHeight - padding);
            int visibleRows = Mathf.FloorToInt((usableHeight + spacing) / (rowHeight + spacing));
            return Mathf.Clamp(visibleRows, 1, limit);
        }

        private float GetItemHeight()
        {
            if (itemPrefab == null)
            {
                return 0f;
            }

            RectTransform itemRect = itemPrefab.GetComponent<RectTransform>();
            if (itemRect != null && itemRect.rect.height > 0f)
            {
                return itemRect.rect.height;
            }

            LayoutElement layoutElement = itemPrefab.GetComponent<LayoutElement>();
            if (layoutElement != null)
            {
                if (layoutElement.preferredHeight > 0f)
                {
                    return layoutElement.preferredHeight;
                }

                if (layoutElement.minHeight > 0f)
                {
                    return layoutElement.minHeight;
                }
            }

            return 24f;
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

        private void ApplyTextColor()
        {
            foreach (PinballEventLogItem item in items)
            {
                item?.SetTextColor(textColor);
            }

            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null)
                {
                    texts[i].color = textColor;
                }
            }
        }
    }
}
