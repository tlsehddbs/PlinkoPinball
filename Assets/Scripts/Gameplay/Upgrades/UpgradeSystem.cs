using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Upgrades
{
    /// <summary>
    /// 런 중 업그레이드 상태와 누적 효과를 관리하는 시스템
    /// UI는 이 시스템을 통해서만 구매 요청 가능
    /// </summary>
    public sealed class UpgradeSystem : MonoBehaviour
    {
        [Header("Config")]
        [SerializeField] private int startingCurrency = 0;

        private readonly HashSet<string> _purchasedNodeIds = new();
        private readonly Dictionary<string, int> _purchaseCounts = new();
        private readonly Dictionary<UpgradeStatKey, float> _statTotals = new();

        private int _currentCurrency;

        /// <summary>
        /// 업그레이드 재화가 변경될 때 호출
        /// </summary>
        public event Action<int> CurrencyChanged;

        /// <summary>
        /// 업그레이드 상태가 변경될 때 호출
        /// </summary>
        public event Action UpgradesChanged;

        public int CurrentCurrency => _currentCurrency;

        private void Awake()
        {
            ResetRunState();
        }

        /// <summary>
        /// 런 시작 상태로 초기화한다.
        /// </summary>
        public void ResetRunState()
        {
            _purchasedNodeIds.Clear();
            _purchaseCounts.Clear();
            _statTotals.Clear();

            _currentCurrency = Mathf.Max(0, startingCurrency);

            CurrencyChanged?.Invoke(_currentCurrency);
            UpgradesChanged?.Invoke();
        }

        /// <summary>
        /// 업그레이드 재화를 추가
        /// </summary>
        public void AddCurrency(int amount)
        {
            if (amount <= 0)
            {
                return;
            }

            _currentCurrency += amount;
            CurrencyChanged?.Invoke(_currentCurrency);
        }

        /// <summary>
        /// 지정한 노드를 구매 가능한지 반환
        /// </summary>
        public bool CanPurchase(UpgradeNodeData node)
        {
            if (node == null)
            {
                return false;
            }

            if (!node.IsRepeatable && _purchasedNodeIds.Contains(node.Id))
            {
                return false;
            }

            if (_currentCurrency < node.Cost)
            {
                return false;
            }

            return ArePrerequisitesMet(node);
        }

        /// <summary>
        /// 지정한 노드 구매를 시도
        /// </summary>
        public bool TryPurchase(UpgradeNodeData node)
        {
            if (!CanPurchase(node))
            {
                return false;
            }

            _currentCurrency -= node.Cost;

            if (!_purchaseCounts.TryGetValue(node.Id, out var count))
            {
                count = 0;
            }

            count++;
            _purchaseCounts[node.Id] = count;
            _purchasedNodeIds.Add(node.Id);

            if (node.StatKey != UpgradeStatKey.None)
            {
                if (!_statTotals.TryGetValue(node.StatKey, out var current))
                {
                    current = 0f;
                }

                _statTotals[node.StatKey] = current + node.AdditiveValue;
            }

            CurrencyChanged?.Invoke(_currentCurrency);
            UpgradesChanged?.Invoke();
            return true;
        }

        /// <summary>
        /// 특정 노드의 구매 여부를 반환
        /// </summary>
        public bool IsPurchased(string nodeId)
        {
            return !string.IsNullOrWhiteSpace(nodeId) && _purchasedNodeIds.Contains(nodeId);
        }

        /// <summary>
        /// 특정 스탯 키의 누적 가산값을 반환
        /// </summary>
        public float GetAdditiveValue(UpgradeStatKey key)
        {
            return _statTotals.TryGetValue(key, out var value) ? value : 0f;
        }

        /// <summary>
        /// 노드의 현재 시각 상태를 계산
        /// </summary>
        public UpgradeNodeVisualState GetVisualState(UpgradeNodeData node)
        {
            if (node == null)
            {
                return UpgradeNodeVisualState.Locked;
            }

            if (!node.IsRepeatable && IsPurchased(node.Id))
            {
                return UpgradeNodeVisualState.Purchased;
            }

            if (CanPurchase(node))
            {
                return UpgradeNodeVisualState.Available;
            }

            return UpgradeNodeVisualState.Locked;
        }

        private bool ArePrerequisitesMet(UpgradeNodeData node)
        {
            if (node.IsRootNode)
            {
                return true;
            }

            var prerequisites = node.PrerequisiteIds;
            if (prerequisites == null || prerequisites.Length == 0)
            {
                return true;
            }

            for (int i = 0; i < prerequisites.Length; i++)
            {
                if (!IsPurchased(prerequisites[i]))
                {
                    return false;
                }
            }

            return true;
        }
    }
}