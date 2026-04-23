using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlinkoPinball.Gameplay.Upgrades;

namespace PlinkoPinball.UI.Upgrades
{
    /// <summary>
    /// 선택된 업그레이드 노드의 상세 정보와 구매 버튼을 표시
    /// </summary>
    public sealed class UpgradeDetailPanelView : MonoBehaviour
    {
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private Button purchaseButton;

        private UpgradeNodeData _currentNode;
        private UpgradeSystem _upgradeSystem;

        /// <summary>
        /// 상세 패널을 갱신하고 연다
        /// </summary>
        public void Show(UpgradeNodeData node, UpgradeSystem upgradeSystem)
        {
            _currentNode = node;
            _upgradeSystem = upgradeSystem;

            if (_currentNode == null || _upgradeSystem == null)
            {
                gameObject.SetActive(false);
                return;
            }

            titleText.text = _currentNode.DisplayName;
            descriptionText.text = _currentNode.Description;
            costText.text = _currentNode.Cost.ToString();

            purchaseButton.onClick.RemoveAllListeners();
            purchaseButton.onClick.AddListener(HandlePurchase);
            purchaseButton.interactable = _upgradeSystem.CanPurchase(_currentNode);

            gameObject.SetActive(true);
        }

        /// <summary>
        /// 현재 선택 상태를 기준으로 버튼 상태를 다시 평가
        /// </summary>
        public void RefreshInteractable()
        {
            if (_currentNode == null || _upgradeSystem == null)
            {
                return;
            }

            purchaseButton.interactable = _upgradeSystem.CanPurchase(_currentNode);
        }

        public void Hide()
        {
            _currentNode = null;
            _upgradeSystem = null;
            gameObject.SetActive(false);
        }

        private void HandlePurchase()
        {
            if (_currentNode == null || _upgradeSystem == null)
            {
                return;
            }

            _upgradeSystem.TryPurchase(_currentNode);
            RefreshInteractable();
        }
    }
}