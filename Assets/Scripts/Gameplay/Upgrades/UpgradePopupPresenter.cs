using System.Collections.Generic;
using TMPro;
using UnityEngine;
using PlinkoPinball.Gameplay.Upgrades;

namespace PlinkoPinball.UI.Upgrades
{
    /// <summary>
    /// 업그레이드 팝업 전체 UI를 구성하고 동기화
    /// </summary>
    public sealed class UpgradePopupPresenter : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private UpgradeDatabase upgradeDatabase;
        [SerializeField] private RectTransform nodeContainer;
        [SerializeField] private UpgradeNodeView nodeViewPrefab;
        [SerializeField] private UpgradeDetailPanelView detailPanel;
        [SerializeField] private TMP_Text currencyText;

        private readonly List<UpgradeNodeView> _spawnedViews = new();

        private void OnEnable()
        {
            if (upgradeSystem != null)
            {
                upgradeSystem.CurrencyChanged += HandleCurrencyChanged;
                upgradeSystem.UpgradesChanged += RefreshAll;
            }
        }

        private void OnDisable()
        {
            if (upgradeSystem != null)
            {
                upgradeSystem.CurrencyChanged -= HandleCurrencyChanged;
                upgradeSystem.UpgradesChanged -= RefreshAll;
            }
        }

        private void Start()
        {
            BuildGraph();
            RefreshAll();
        }

        public void Open()
        {
            gameObject.SetActive(true);
            RefreshAll();
        }

        public void Close()
        {
            if (detailPanel != null)
            {
                detailPanel.Hide();
            }

            gameObject.SetActive(false);
        }

        private void BuildGraph()
        {
            ClearGraph();

            if (upgradeDatabase == null || nodeContainer == null || nodeViewPrefab == null)
            {
                return;
            }

            var nodes = upgradeDatabase.Nodes;
            for (int i = 0; i < nodes.Count; i++)
            {
                var node = nodes[i];
                if (node == null)
                {
                    continue;
                }

                var view = Instantiate(nodeViewPrefab, nodeContainer);
                view.Bind(node, HandleNodeClicked);
                _spawnedViews.Add(view);
            }
        }

        private void ClearGraph()
        {
            for (int i = 0; i < _spawnedViews.Count; i++)
            {
                if (_spawnedViews[i] != null)
                {
                    Destroy(_spawnedViews[i].gameObject);
                }
            }

            _spawnedViews.Clear();
        }

        private void HandleNodeClicked(UpgradeNodeData node)
        {
            if (detailPanel == null || upgradeSystem == null)
            {
                return;
            }

            detailPanel.Show(node, upgradeSystem);
        }

        private void HandleCurrencyChanged(int amount)
        {
            if (currencyText != null)
            {
                currencyText.text = amount.ToString();
            }
        }

        private void RefreshAll()
        {
            if (upgradeSystem == null)
            {
                return;
            }

            HandleCurrencyChanged(upgradeSystem.CurrentCurrency);

            for (int i = 0; i < _spawnedViews.Count; i++)
            {
                var view = _spawnedViews[i];
                if (view == null || view.NodeData == null)
                {
                    continue;
                }

                view.RefreshVisual(upgradeSystem.GetVisualState(view.NodeData));
            }

            if (detailPanel != null)
            {
                detailPanel.RefreshInteractable();
            }
        }
    }
}