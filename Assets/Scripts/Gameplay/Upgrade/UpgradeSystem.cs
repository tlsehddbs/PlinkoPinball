using System;
using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core;

namespace PlinkoPinball.Gameplay.Upgrade
{
    public sealed class UpgradeSystem : MonoBehaviour
    {
        public event Action OnUpgradeStateChanged;

        [SerializeField] private UpgradeDatabase database;

        private readonly Dictionary<string, int> levels = new();

        public int GetLevel(UpgradeDefinition definition)
        {
            if (definition == null)
            {
                return 0;
            }

            return levels.TryGetValue(definition.upgradeId, out int level) ? level : 0;
        }

        public bool IsMaxLevel(UpgradeDefinition definition)
        {
            return GetLevel(definition) >= definition.maxLevel;
        }

        public bool IsUnlocked(UpgradeDefinition definition)
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
                if (GetLevel(prerequisite) <= 0)
                {
                    return false;
                }
            }

            return true;
        }

        public bool CanPurchase(UpgradeDefinition definition, int currentCurrency)
        {
            if (definition == null)
            {
                return false;
            }

            if (!IsUnlocked(definition))
            {
                return false;
            }

            if (IsMaxLevel(definition))
            {
                return false;
            }

            int cost = definition.GetCost(GetLevel(definition));
            return currentCurrency >= cost;
        }

        public bool TryPurchase(UpgradeDefinition definition)
        {
            if (definition == null)
            {
                return false;
            }

            if (!IsUnlocked(definition))
            {
                Debug.LogWarning($"[UpgradeSystem] Locked upgrade: {definition.upgradeId}");
                return false;
            }

            if (IsMaxLevel(definition))
            {
                Debug.LogWarning($"[UpgradeSystem] Already max level: {definition.upgradeId}");
                return false;
            }

            int currentLevel = GetLevel(definition);
            int cost = definition.GetCost(currentLevel);

            if (cost > 0)
            {
                if (CurrencySystem.Instance == null)
                {
                    Debug.LogError("[UpgradeSystem] CurrencySystem.Instance is null.");
                    return false;
                }

                if (!CurrencySystem.Instance.SpendCurrency(cost))
                {
                    Debug.LogWarning($"[UpgradeSystem] Not enough currency. upgrade={definition.upgradeId}, cost={cost}");

                    return false;
                }
            }

            levels[definition.upgradeId] = currentLevel + 1;

            Debug.Log(
                $"[UpgradeSystem] Purchased upgrade={definition.upgradeId}, level={currentLevel + 1}/{definition.maxLevel}, cost={cost}");

            OnUpgradeStateChanged?.Invoke();

            return true;
        }

        public float GetTotalValue(UpgradeType type)
        {
            float total = 0f;

            foreach (var upgrade in database.upgrades)
            {
                if (upgrade == null)
                {
                    continue;
                }

                if (upgrade.upgradeType != type)
                {
                    continue;
                }

                int level = GetLevel(upgrade);
                total += upgrade.GetTotalValue(level);
            }

            return total;
        }
    }
}