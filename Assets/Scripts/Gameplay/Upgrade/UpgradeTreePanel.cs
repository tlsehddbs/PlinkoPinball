using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core;

namespace PlinkoPinball.Gameplay.Upgrade.UI
{
    /// <summary>
    /// UpgradeDefinition의 prerequisite 데이터를 기반으로 업그레이드 노드를 자동 생성하고 4방향 격자 형태로 배치
    /// </summary>
    public sealed class UpgradeTreePanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UpgradeDatabase database;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private RectTransform nodeRoot;
        [SerializeField] private UpgradeNodeButton nodePrefab;

        [Header("Grid Layout")]
        [SerializeField] private Vector2 gridSize = new Vector2(280f, 140f);
        [SerializeField] private bool centerVerticalGroup = true;

        private readonly List<UpgradeNodeButton> spawnedNodes = new();

        private void OnEnable()
        {
            if (upgradeSystem != null)
            {
                upgradeSystem.OnUpgradeStateChanged += BuildTree;
            }

            if (CurrencySystem.Instance != null)
            {
                CurrencySystem.Instance.OnCurrencyChanged += HandleCurrencyChanged;
            }
        }

        private void OnDisable()
        {
            if (upgradeSystem != null)
            {
                upgradeSystem.OnUpgradeStateChanged -= BuildTree;
            }

            if (CurrencySystem.Instance != null)
            {
                CurrencySystem.Instance.OnCurrencyChanged -= HandleCurrencyChanged;
            }
        }

        private void Start()
        {
            BuildTree();
        }

        private void BuildTree()
        {
            if (database == null || upgradeSystem == null || nodeRoot == null || nodePrefab == null)
            {
                Debug.LogError("[UpgradeTreePanel] Missing reference.");
                return;
            }

            ClearNodes();

            Dictionary<UpgradeDefinition, int> depthMap = BuildDepthMap();
            Dictionary<int, List<UpgradeDefinition>> depthGroups = BuildDepthGroups(depthMap);

            foreach (var pair in depthGroups)
            {
                int depth = pair.Key;

                List<UpgradeDefinition> visibleUpgrades = new();

                foreach (var upgrade in pair.Value)
                {
                    if (ShouldShowNode(upgrade))
                    {
                        visibleUpgrades.Add(upgrade);
                    }
                }

                for (int index = 0; index < visibleUpgrades.Count; index++)
                {
                    UpgradeDefinition upgrade = visibleUpgrades[index];

                    Vector2Int gridPosition = CalculateGridPosition(depth, index, visibleUpgrades.Count);

                    SpawnNode(upgrade, gridPosition);
                }
            }
        }

        private void ClearNodes()
        {
            foreach (Transform child in nodeRoot)
            {
                Destroy(child.gameObject);
            }

            spawnedNodes.Clear();
        }

        private void SpawnNode(UpgradeDefinition upgrade, Vector2Int gridPosition)
        {
            UpgradeNodeButton node = Instantiate(nodePrefab, nodeRoot);

            RectTransform rect = node.GetComponent<RectTransform>();

            rect.anchoredPosition = new Vector2(gridPosition.x * gridSize.x, gridPosition.y * gridSize.y);

            node.Initialize(upgrade, upgradeSystem, RequestPurchase);
            spawnedNodes.Add(node);
        }

        /// <summary>
        /// 선행 업그레이드가 없으면 항상 표시
        /// 선행 업그레이드가 있으면 모든 prerequisite이 최소 1레벨 이상이어야 표시함.
        /// </summary>
        private bool ShouldShowNode(UpgradeDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            if (definition.prerequisites == null || definition.prerequisites.Length == 0)
            {
                return true;
            }

            foreach (var prerequisite in definition.prerequisites)
            {
                if (prerequisite == null)
                {
                    continue;
                }

                if (upgradeSystem.GetLevel(prerequisite) <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// depth는 좌우 방향, index는 상하 방향으로 변환
        /// 따라서 노드 위치는 항상 4방향 격자 위에 놓임.
        /// </summary>
        private Vector2Int CalculateGridPosition(int depth, int index, int groupCount)
        {
            int x = depth;
            int y;

            if (!centerVerticalGroup)
            {
                y = -index;
            }
            else
            {
                int offset = groupCount - 1;
                y = offset - index * 2;
            }

            return new Vector2Int(x, y);
        }

        /// <summary>
        /// DAG Depth Layout.
        /// prerequisite 그래프를 DFS로 순회하여 각 업그레이드의 depth를 계산한다.
        /// prerequisite이 없는 노드는 depth 0이다.
        /// prerequisite이 있는 노드는 가장 깊은 prerequisite depth + 1이 된다.
        /// </summary>
        private Dictionary<UpgradeDefinition, int> BuildDepthMap()
        {
            Dictionary<UpgradeDefinition, int> result = new();

            foreach (var upgrade in database.upgrades)
            {
                if (upgrade == null)
                {
                    continue;
                }

                int depth = CalculateDepth(upgrade, result, new HashSet<UpgradeDefinition>());

                result[upgrade] = depth;
            }

            return result;
        }

        private int CalculateDepth(UpgradeDefinition upgrade, Dictionary<UpgradeDefinition, int> cache, HashSet<UpgradeDefinition> visiting)
        {
            if (upgrade == null)
            {
                return 0;
            }

            if (cache.TryGetValue(upgrade, out int cachedDepth))
            {
                return cachedDepth;
            }

            if (visiting.Contains(upgrade))
            {
                Debug.LogError($"[UpgradeTreePanel] Circular prerequisite detected: {upgrade.name}");
                return 0;
            }

            visiting.Add(upgrade);

            int maxPrerequisiteDepth = -1;

            if (upgrade.prerequisites != null)
            {
                foreach (var prerequisite in upgrade.prerequisites)
                {
                    if (prerequisite == null)
                    {
                        continue;
                    }

                    int prerequisiteDepth = CalculateDepth(prerequisite, cache, visiting);

                    if (prerequisiteDepth > maxPrerequisiteDepth)
                    {
                        maxPrerequisiteDepth = prerequisiteDepth;
                    }
                }
            }

            visiting.Remove(upgrade);

            int depth = maxPrerequisiteDepth + 1;
            cache[upgrade] = depth;

            return depth;
        }

        private Dictionary<int, List<UpgradeDefinition>> BuildDepthGroups(Dictionary<UpgradeDefinition, int> depthMap)
        {
            Dictionary<int, List<UpgradeDefinition>> result = new();

            foreach (var pair in depthMap)
            {
                UpgradeDefinition upgrade = pair.Key;
                int depth = pair.Value;

                if (!result.TryGetValue(depth, out List<UpgradeDefinition> group))
                {
                    group = new List<UpgradeDefinition>();
                    result.Add(depth, group);
                }

                group.Add(upgrade);
            }

            foreach (var pair in result)
            {
                pair.Value.Sort((a, b) => string.CompareOrdinal(a.upgradeId, b.upgradeId));
            }

            return result;
        }

        private void RequestPurchase(UpgradeDefinition definition)
        {
            if (upgradeSystem == null)
            {
                return;
            }

            if (!upgradeSystem.TryPurchase(definition))
            {
                return;
            }

            BuildTree();
        }

        private void HandleCurrencyChanged(long currentCurrency)
        {
            RefreshAll();
        }

        private void RefreshAll()
        {
            foreach (var node in spawnedNodes)
            {
                node.Refresh();
            }
        }
    }
}