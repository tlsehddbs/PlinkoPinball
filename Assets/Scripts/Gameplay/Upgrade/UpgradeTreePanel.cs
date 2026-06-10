using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlinkoPinball.Core;
using PlinkoPinball.UI.Mainframe;

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
        [SerializeField] private MainframeTheme theme;

        [Header("Info Panel")]
        [SerializeField] private GameObject upgradeInfoPanel;
        [SerializeField] private TMP_Text upgradeInfoText;

        [Header("Grid Layout")]
        [SerializeField] private Vector2 gridSize = new Vector2(280f, 140f);

        private readonly List<UpgradeNodeButton> spawnedNodes = new();
        private readonly List<UpgradeConnectionLine> spawnedLines = new();
        private readonly Dictionary<UpgradeDefinition, UpgradeNodeButton> nodeMap = new();
        private readonly StringBuilder infoBuilder = new();
        private UpgradeDefinition currentInfoDefinition;

        private void OnEnable()
        {
            ResolveReferences();
            ApplyTheme();

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

            HideUpgradeInfo(currentInfoDefinition);
        }

        private void Start()
        {
            ResolveReferences();
            ApplyTheme();
            BuildTree();
        }

        private void BuildTree()
        {
            ResolveReferences();

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
            HideUpgradeInfo(currentInfoDefinition);

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

            node.Initialize(upgrade, upgradeSystem, RequestPurchase, ShowUpgradeInfo, HideUpgradeInfo, theme);

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
            line.ConfigureTheme(theme);
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
            ResolveReferences();

            if (upgradeSystem == null)
            {
                return;
            }

            if (!upgradeSystem.TryPurchase(definition))
            {
                return;
            }

            BuildTree();
            ShowUpgradeInfo(definition);
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

            if (currentInfoDefinition != null)
            {
                ShowUpgradeInfo(currentInfoDefinition);
            }
        }

        private void ResolveReferences()
        {
            if (UpgradeSystem.Instance != null)
            {
                upgradeSystem = UpgradeSystem.Instance;
            }
            else if (upgradeSystem == null)
            {
                upgradeSystem = FindFirstObjectByType<UpgradeSystem>();
            }

            if (database == null && upgradeSystem != null)
            {
                database = upgradeSystem.Database;
            }

            if (theme == null)
            {
                MainframeOSBootstrap bootstrap = FindFirstObjectByType<MainframeOSBootstrap>();
                if (bootstrap != null)
                {
                    theme = bootstrap.Theme;
                }
            }

            ResolveInfoPanelReferences();
        }

        private void ApplyTheme()
        {
            if (theme == null)
            {
                return;
            }

            Image[] images = GetComponentsInChildren<Image>(true);
            for (int i = 0; i < images.Length; i++)
            {
                Image image = images[i];
                if (image == null)
                {
                    continue;
                }

                if (image.GetComponent<UpgradeNodeButton>() != null ||
                    image.GetComponent<UpgradeConnectionLine>() != null)
                {
                    continue;
                }

                string objectName = image.gameObject.name;
                if (objectName.Contains("Info") || objectName.Contains("Panel") || objectName.Contains("Content"))
                {
                    image.color = theme.windowBackgroundColor;
                }
                else if (objectName.Contains("Viewport"))
                {
                    image.color = theme.transparentColor;
                }
            }

            TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);
            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i] != null)
                {
                    texts[i].color = theme.accentColor;
                    texts[i].font = theme.ResolveFont(texts[i].font);
                }
            }
        }

        private void ResolveInfoPanelReferences()
        {
            if (upgradeInfoPanel == null)
            {
                upgradeInfoPanel = FindSceneObjectByName("UpgradeInfoPanel");
            }

            if (upgradeInfoPanel == null)
            {
                upgradeInfoPanel = FindSceneObjectByName("UpgradeInfoContent");
            }

            if (upgradeInfoText == null)
            {
                GameObject textObject = FindSceneObjectByName("UpgradeInfoText");
                if (textObject != null)
                {
                    upgradeInfoText = textObject.GetComponent<TMP_Text>();
                }
            }

            if (upgradeInfoPanel == null && upgradeInfoText != null)
            {
                Transform parent = upgradeInfoText.transform.parent;
                upgradeInfoPanel = parent != null ? parent.gameObject : upgradeInfoText.gameObject;
            }

            if (upgradeInfoText == null && upgradeInfoPanel != null)
            {
                upgradeInfoText = upgradeInfoPanel.GetComponentInChildren<TMP_Text>(true);
            }

            if (upgradeInfoPanel != null && currentInfoDefinition == null)
            {
                upgradeInfoPanel.SetActive(false);
            }
        }

        private void ShowUpgradeInfo(UpgradeDefinition definition)
        {
            if (definition == null)
            {
                return;
            }

            ResolveInfoPanelReferences();

            if (upgradeInfoText == null)
            {
                return;
            }

            currentInfoDefinition = definition;
            upgradeInfoText.text = BuildUpgradeInfo(definition);

            if (upgradeInfoPanel != null)
            {
                upgradeInfoPanel.SetActive(true);
                upgradeInfoPanel.transform.SetAsLastSibling();
            }
        }

        private void HideUpgradeInfo(UpgradeDefinition definition)
        {
            if (definition != null && currentInfoDefinition != definition)
            {
                return;
            }

            currentInfoDefinition = null;

            if (upgradeInfoPanel != null)
            {
                upgradeInfoPanel.SetActive(false);
            }
        }

        private string BuildUpgradeInfo(UpgradeDefinition definition)
        {
            infoBuilder.Clear();

            int level = upgradeSystem != null ? upgradeSystem.GetLevel(definition) : 0;
            int nextLevel = Mathf.Min(level + 1, definition.maxLevel);
            bool maxed = level >= definition.maxLevel;
            bool unlocked = upgradeSystem != null && upgradeSystem.IsUnlocked(definition);
            int cost = maxed ? 0 : definition.GetCost(level);
            long currency = CurrencySystem.Instance != null ? CurrencySystem.Instance.CurrentCurrency : 0;
            bool affordable = CurrencySystem.Instance != null && CurrencySystem.Instance.CanAfford(cost);

            infoBuilder.AppendLine(definition.displayName);
            infoBuilder.AppendLine($"LV {level}/{definition.maxLevel}");
            infoBuilder.AppendLine();

            AppendDescription(definition);
            AppendEffectPreview(definition, level, nextLevel, maxed);

            infoBuilder.AppendLine();
            if (maxed)
            {
                infoBuilder.AppendLine("상태: 최대 레벨");
            }
            else if (!unlocked)
            {
                infoBuilder.AppendLine("상태: 선행 업그레이드 필요");
            }
            else
            {
                string affordText = affordable ? "구매 가능" : $"재화 부족 ({currency:N0}/{cost:N0})";
                infoBuilder.AppendLine($"비용: {cost:N0}");
                infoBuilder.AppendLine($"상태: {affordText}");
            }

            return infoBuilder.ToString();
        }

        private void AppendDescription(UpgradeDefinition definition)
        {
            if (!string.IsNullOrWhiteSpace(definition.description))
            {
                infoBuilder.AppendLine(definition.description);
            }

            if (!string.IsNullOrWhiteSpace(definition.flavorText))
            {
                infoBuilder.AppendLine(definition.flavorText);
            }
        }

        private void AppendEffectPreview(UpgradeDefinition definition, int level, int nextLevel, bool maxed)
        {
            infoBuilder.AppendLine();
            infoBuilder.AppendLine($"효과: {GetEffectLabel(definition.upgradeType)}");

            if (IsChipsetType(definition.upgradeType))
            {
                AppendChipsetPreview(definition, nextLevel, maxed);
                return;
            }

            float nodeCurrentTotal = definition.GetTotalValue(level);
            float nodeNextTotal = definition.GetTotalValue(nextLevel);
            float delta = maxed ? 0f : nodeNextTotal - nodeCurrentTotal;
            float currentTypeTotal = upgradeSystem != null ? upgradeSystem.GetTotalValue(definition.upgradeType) : nodeCurrentTotal;
            float nextTypeTotal = currentTypeTotal + delta;

            infoBuilder.AppendLine($"현재 효과 총합: {FormatEffectValue(definition.upgradeType, currentTypeTotal)}");

            if (!maxed)
            {
                infoBuilder.AppendLine($"구매 후 효과 총합: {FormatEffectValue(definition.upgradeType, nextTypeTotal)}");
                infoBuilder.AppendLine($"변화량: {FormatEffectDelta(definition.upgradeType, delta)}");
            }

            string appliedText = GetAppliedText(definition.upgradeType);
            if (!string.IsNullOrEmpty(appliedText))
            {
                infoBuilder.AppendLine($"적용: {appliedText}");
            }
        }

        private void AppendChipsetPreview(UpgradeDefinition definition, int nextLevel, bool maxed)
        {
            int currentTier = upgradeSystem != null ? upgradeSystem.GetUnlockedTier(definition.upgradeType) : 0;
            infoBuilder.AppendLine($"현재 해금 Tier: {currentTier}");

            if (!maxed)
            {
                int nodeNextTier = Mathf.Max(nextLevel, Mathf.RoundToInt(definition.GetTotalValue(nextLevel)));
                int nextTier = Mathf.Max(currentTier, nodeNextTier);
                infoBuilder.AppendLine($"구매 후 해금 Tier: {nextTier}");
                infoBuilder.AppendLine($"변화량: +{Mathf.Max(0, nextTier - currentTier)} Tier");
            }

            string appliedText = GetAppliedText(definition.upgradeType);
            if (!string.IsNullOrEmpty(appliedText))
            {
                infoBuilder.AppendLine($"적용: {appliedText}");
            }
        }

        private static string GetEffectLabel(UpgradeType type)
        {
            return type switch
            {
                UpgradeType.CompressionThresholdReduction => "압축 임계값 감소",
                UpgradeType.BasePinValueIncrease => "핀 기본 보상 증가",
                UpgradeType.BaseSlotValueIncrease => "슬롯 기본 보상 증가",
                UpgradeType.ErrorPinRateReduction => "핀 에러율 감소",
                UpgradeType.ErrorSlotRateReduction => "슬롯 에러율 감소",
                UpgradeType.ChipsetTierAmplifier => "Amplifier 칩셋 Tier 해금",
                UpgradeType.ChipsetTierOverclock => "Overclock 칩셋 Tier 해금",
                UpgradeType.ChipsetTierErrorCorrection => "Error Correction 칩셋 Tier 해금",
                _ => type.ToString()
            };
        }

        private static string GetAppliedText(UpgradeType type)
        {
            return type switch
            {
                UpgradeType.CompressionThresholdReduction => "핀볼 압축 시스템의 공 압축 기준에 반영",
                UpgradeType.BasePinValueIncrease => "플링코 핀 보상 계산에 반영",
                UpgradeType.BaseSlotValueIncrease => "플링코 슬롯 보상 계산에 반영",
                UpgradeType.ErrorPinRateReduction => "플링코 핀 에러 확률 계산에 반영",
                UpgradeType.ErrorSlotRateReduction => "플링코 슬롯 에러 확률 계산에 반영",
                UpgradeType.ChipsetTierAmplifier => "플링코 칩셋 선택 가능 Tier에 반영",
                UpgradeType.ChipsetTierOverclock => "플링코 칩셋 선택 가능 Tier에 반영",
                UpgradeType.ChipsetTierErrorCorrection => "플링코 칩셋 선택 가능 Tier에 반영",
                _ => string.Empty
            };
        }

        private static string FormatEffectValue(UpgradeType type, float value)
        {
            return IsRateType(type) ? $"{value:P1}" : $"{value:0.##}";
        }

        private static string FormatEffectDelta(UpgradeType type, float delta)
        {
            if (IsReductionType(type))
            {
                return IsRateType(type) ? $"-{delta:P1}" : $"-{delta:0.##}";
            }

            return IsRateType(type) ? $"+{delta:P1}" : $"+{delta:0.##}";
        }

        private static bool IsReductionType(UpgradeType type)
        {
            return type == UpgradeType.CompressionThresholdReduction ||
                   type == UpgradeType.ErrorPinRateReduction ||
                   type == UpgradeType.ErrorSlotRateReduction;
        }

        private static bool IsRateType(UpgradeType type)
        {
            return type == UpgradeType.ErrorPinRateReduction ||
                   type == UpgradeType.ErrorSlotRateReduction;
        }

        private static bool IsChipsetType(UpgradeType type)
        {
            return type == UpgradeType.ChipsetTierAmplifier ||
                   type == UpgradeType.ChipsetTierOverclock ||
                   type == UpgradeType.ChipsetTierErrorCorrection;
        }

        private static GameObject FindSceneObjectByName(string objectName)
        {
            Transform[] transforms = Resources.FindObjectsOfTypeAll<Transform>();
            foreach (Transform transform in transforms)
            {
                if (transform == null || transform.name != objectName)
                {
                    continue;
                }

                if (transform.gameObject.scene.IsValid() && transform.gameObject.scene.isLoaded)
                {
                    return transform.gameObject;
                }
            }

            return null;
        }
    }
}
