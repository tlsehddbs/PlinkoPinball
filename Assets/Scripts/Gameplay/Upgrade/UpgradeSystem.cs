using System;
using System.Collections.Generic;
using UnityEngine;
using PlinkoPinball.Core;
using PlinkoPinball.Gameplay.Core.Flow;

namespace PlinkoPinball.Gameplay.Upgrade
{
    public sealed class UpgradeSystem : MonoBehaviour
    {
        public static UpgradeSystem Instance { get; private set; }

        public event Action OnUpgradeStateChanged;

        [SerializeField] private UpgradeDatabase database;
        [SerializeField] private GameSessionState sessionState;

        public UpgradeDatabase Database => database;

        private readonly Dictionary<string, int> fallbackLevels = new();

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                if (Instance.database == null && database != null)
                {
                    Instance.database = database;
                }

                Instance.ResolveSessionState();
                Destroy(this);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);
            ResolveSessionState();

            UpgradeEffectResolver resolver = GetComponent<UpgradeEffectResolver>();
            if (resolver == null)
            {
                resolver = gameObject.AddComponent<UpgradeEffectResolver>();
            }

            resolver.Initialize(this);
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Initialize(UpgradeDatabase upgradeDatabase, GameSessionState state)
        {
            if (database == null && upgradeDatabase != null)
            {
                database = upgradeDatabase;
            }

            if (sessionState == null && state != null)
            {
                sessionState = state;
            }

            ResolveSessionState();
        }

        public int GetLevel(UpgradeDefinition definition)
        {
            if (definition == null)
            {
                return 0;
            }

            ResolveSessionState();
            if (sessionState != null)
            {
                return sessionState.GetUpgradeLevel(definition.upgradeId);
            }

            return fallbackLevels.TryGetValue(definition.upgradeId, out int level) ? level : 0;
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

            SetLevel(definition, currentLevel + 1);

            Debug.Log(
                $"[UpgradeSystem] Purchased upgrade={definition.upgradeId}, level={currentLevel + 1}/{definition.maxLevel}, cost={cost}");

            OnUpgradeStateChanged?.Invoke();

            return true;
        }

        public float GetTotalValue(UpgradeType type)
        {
            float total = 0f;

            if (database == null || database.upgrades == null)
            {
                return total;
            }

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

        public int GetUnlockedTier(UpgradeType type)
        {
            int tier = 0;

            if (database == null || database.upgrades == null)
            {
                return tier;
            }

            foreach (var upgrade in database.upgrades)
            {
                if (upgrade == null || upgrade.upgradeType != type)
                {
                    continue;
                }

                int level = GetLevel(upgrade);
                if (level <= 0)
                {
                    continue;
                }

                int unlockedTier = Mathf.Max(level, Mathf.RoundToInt(upgrade.GetTotalValue(level)));
                tier = Mathf.Max(tier, unlockedTier);
            }

            return tier;
        }

        private void SetLevel(UpgradeDefinition definition, int level)
        {
            if (definition == null)
            {
                return;
            }

            ResolveSessionState();
            if (sessionState != null)
            {
                sessionState.SetUpgradeLevel(definition.upgradeId, level);
                return;
            }

            fallbackLevels[definition.upgradeId] = Mathf.Max(0, level);
        }

        private void ResolveSessionState()
        {
            if (sessionState != null)
            {
                return;
            }

            sessionState = GameSessionState.Instance;
            if (sessionState == null)
            {
                sessionState = FindFirstObjectByType<GameSessionState>();
            }
        }
    }
}
