using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlinkoPinball.Gameplay.Upgrades;

namespace PlinkoPinball.UI.Upgrades
{
    /// <summary>
    /// 업그레이드 그래프 위에 배치되는 단일 노드 UI
    /// </summary>
    public sealed class UpgradeNodeView : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private RectTransform root;
        [SerializeField] private Button button;
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text costText;

        [Header("State Objects")]
        [SerializeField] private GameObject lockedMarker;
        [SerializeField] private GameObject availableMarker;
        [SerializeField] private GameObject purchasedMarker;

        private UpgradeNodeData _nodeData;
        private Action<UpgradeNodeData> _onClick;

        public UpgradeNodeData NodeData => _nodeData;

        /// <summary>
        /// 노드 데이터를 바인딩
        /// </summary>
        public void Bind(UpgradeNodeData nodeData, Action<UpgradeNodeData> onClick)
        {
            _nodeData = nodeData;
            _onClick = onClick;

            if (_nodeData == null)
            {
                gameObject.SetActive(false);
                return;
            }

            gameObject.SetActive(true);

            if (root != null)
            {
                root.anchoredPosition = _nodeData.GraphPosition;
            }

            if (iconImage != null)
            {
                iconImage.sprite = _nodeData.Icon;
                iconImage.enabled = _nodeData.Icon != null;
            }

            if (costText != null)
            {
                costText.text = _nodeData.Cost.ToString();
            }

            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(HandleClick);
        }

        /// <summary>
        /// 현재 상태를 시각적으로 반영
        /// </summary>
        public void RefreshVisual(UpgradeNodeVisualState state)
        {
            if (lockedMarker != null)
            {
                lockedMarker.SetActive(state == UpgradeNodeVisualState.Locked);
            }

            if (availableMarker != null)
            {
                availableMarker.SetActive(state == UpgradeNodeVisualState.Available);
            }

            if (purchasedMarker != null)
            {
                purchasedMarker.SetActive(state == UpgradeNodeVisualState.Purchased);
            }

            if (button != null)
            {
                button.interactable = true;
            }
        }

        private void HandleClick()
        {
            _onClick?.Invoke(_nodeData);
        }
    }
}