using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core;

namespace PlinkoPinball.Gameplay.Upgrade.UI
{
    public sealed class UpgradeTreePanel : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private UpgradeDatabase database;
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private RectTransform nodeRoot;
        [SerializeField] private RectTransform lineRoot;
        [SerializeField] private UpgradeNodeButton nodePrefab;
        [SerializeField] private UpgradeConnectionLine linePrefab;

        [Header("Grid Layout")]
        [SerializeField] private Vector2 gridSize = new Vector2(280f, 140f);

        private readonly List<UpgradeNodeButton> spawnedNodes = new();
        private readonly List<UpgradeConnectionLine> spawnedLines = new();
        private readonly Dictionary<UpgradeDefinition, UpgradeNodeButton> nodeMap = new();

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
            if (database == null ||
                upgradeSystem == null ||
                nodeRoot == null ||
                lineRoot == null ||
                nodePrefab == null ||
                linePrefab == null)
            {
                Debug.LogError("[UpgradeTreePanel] Missing reference.");
                return;
            }

            ClearTree();

            foreach (var upgrade in database.upgrades)
            {
                if (!ShouldShowNode(upgrade))
                {
                    continue;
                }

                SpawnNode(upgrade);
            }

            foreach (var upgrade in database.upgrades)
            {
                if (!ShouldShowNode(upgrade))
                {
                    continue;
                }

                SpawnLines(upgrade);
            }
        }

        private void ClearTree()
        {
            foreach (Transform child in nodeRoot)
            {
                Destroy(child.gameObject);
            }

            foreach (Transform child in lineRoot)
            {
                Destroy(child.gameObject);
            }

            spawnedNodes.Clear();
            spawnedLines.Clear();
            nodeMap.Clear();
        }

        private void SpawnNode(UpgradeDefinition upgrade)
        {
            UpgradeNodeButton node = Instantiate(nodePrefab, nodeRoot);

            RectTransform rect = node.GetComponent<RectTransform>();
            rect.anchoredPosition = GridToAnchoredPosition(upgrade.gridPosition);

            node.Initialize(upgrade, upgradeSystem, RequestPurchase);

            spawnedNodes.Add(node);
            nodeMap.Add(upgrade, node);
        }

        private void SpawnLines(UpgradeDefinition upgrade)
        {
            if (upgrade == null || upgrade.prerequisites == null)
            {
                return;
            }

            if (!nodeMap.TryGetValue(upgrade, out UpgradeNodeButton childNode))
            {
                return;
            }

            Vector2 childPosition = childNode.GetComponent<RectTransform>().anchoredPosition;

            foreach (var prerequisite in upgrade.prerequisites)
            {
                if (prerequisite == null)
                {
                    continue;
                }

                if (!nodeMap.TryGetValue(prerequisite, out UpgradeNodeButton parentNode))
                {
                    continue;
                }

                Vector2 parentPosition = parentNode.GetComponent<RectTransform>().anchoredPosition;

                SpawnOrthogonalLine(parentPosition, childPosition);
            }
        }

        private void SpawnOrthogonalLine(Vector2 from, Vector2 to)
        {
            float midX = (from.x + to.x) * 0.5f;

            Vector2 p0 = from;
            Vector2 p1 = new Vector2(midX, from.y);
            Vector2 p2 = new Vector2(midX, to.y);
            Vector2 p3 = to;

            SpawnLineSegment(p0, p1);
            SpawnLineSegment(p1, p2);
            SpawnLineSegment(p2, p3);
        }

        private void SpawnLineSegment(Vector2 from, Vector2 to)
        {
            if (Vector2.Distance(from, to) <= 0.01f)
            {
                return;
            }

            UpgradeConnectionLine line = Instantiate(linePrefab, lineRoot);
            line.SetPoints(from, to);
            spawnedLines.Add(line);
        }

        private Vector2 GridToAnchoredPosition(Vector2Int gridPosition)
        {
            return new Vector2(
                gridPosition.x * gridSize.x,
                gridPosition.y * gridSize.y
            );
        }

        private bool IsPrerequisiteSatisfied(UpgradeDefinition child, UpgradeDefinition prerequisite)
        {
            if (child.requirePrerequisitesMaxLevel)
            {
                return upgradeSystem.IsMaxLevel(prerequisite);
            }

            return upgradeSystem.GetLevel(prerequisite) > 0;
        }

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

                if (!IsPrerequisiteSatisfied(definition, prerequisite))
                {
                    return false;
                }
            }

            return true;
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