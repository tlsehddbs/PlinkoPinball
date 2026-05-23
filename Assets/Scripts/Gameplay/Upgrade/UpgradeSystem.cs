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
                return false;
            }

            if (IsMaxLevel(definition))
            {
                return false;
            }

            int currentLevel = GetLevel(definition);
            int cost = definition.GetCost(currentLevel);

            CurrencySystem currencySystem = CurrencySystem.Instance;

            if (currencySystem == null)
            {
                return false;
            }

            if (!currencySystem.CanAfford(cost))
            {
                return false;
            }

            if (!currencySystem.SpendCurrency(cost))
            {
                return false;
            }

            levels[definition.upgradeId] = currentLevel + 1;

            Debug.Log($"[PermanentUpgrade] Purchased {definition.displayName} level={currentLevel + 1}");

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

                total += upgrade.GetTotalValue(GetLevel(upgrade));
            }

            return total;
        }
    }
}