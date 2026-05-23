using UnityEditor;
using UnityEngine;
using System.IO;

namespace PlinkoPinball.Gameplay.Upgrade.Editor
{
    public static class UpgradeBatchGenerator
    {
        [MenuItem("PlinkoPinball/Create Default Upgrades")]
        public static void CreateDefaultUpgrades()
        {
            string rootPath = "Assets/Data/Upgrades";

            if (!AssetDatabase.IsValidFolder(rootPath))
            {
                AssetDatabase.CreateFolder("Assets/Data", "Upgrades");
            }

            CreateUpgradeChain(rootPath, "Compression", UpgradeType.CompressionThresholdReduction, 5, 20, 1f);

            CreateUpgradeChain(rootPath, "PinValue", UpgradeType.BasePinValueIncrease, 5, 30, 2f);

            CreateUpgradeChain(rootPath, "SlotValue", UpgradeType.BaseSlotValueIncrease, 5, 40, 3f);

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[UpgradeBatchGenerator] Generated default upgrades.");
        }

        private static void CreateUpgradeChain(string rootPath, string prefix, UpgradeType type, int count, int baseCost, float valuePerLevel)
        {
            UpgradeDefinition previous = null;

            for (int i = 1; i <= count; i++)
            {
                UpgradeDefinition asset = ScriptableObject.CreateInstance<UpgradeDefinition>();

                asset.upgradeId = $"{prefix}_{i}";
                asset.displayName = $"{prefix} {i}";
                asset.description = $"{prefix} upgrade tier {i}";

                asset.upgradeType = type;

                asset.baseCost = baseCost * i;
                asset.costMultiplier = 1.35f;
                asset.maxLevel = 1;

                asset.valuePerLevel = valuePerLevel;

                if (previous != null)
                {
                    asset.prerequisites = new UpgradeDefinition[] { previous };
                }

                string assetPath = $"{rootPath}/{prefix}_{i}.asset";

                AssetDatabase.CreateAsset(asset, assetPath);

                previous = asset;
            }
        }
    }
}