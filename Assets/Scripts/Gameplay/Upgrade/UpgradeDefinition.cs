using UnityEngine;

namespace PlinkoPinball.Gameplay.Upgrade
{
    [CreateAssetMenu(fileName = "UpgradeDefinition", menuName = "PlinkoPinball/Upgrade/Upgrade Definition")]
    public sealed class UpgradeDefinition : ScriptableObject
    {
        [Header("Identity")]
        public string upgradeId;
        public string displayName;
        [TextArea] public string description;

        [Header("Tree")]
        public UpgradeDefinition[] prerequisites;
        public Vector2 treePosition;

        [Header("Cost")]
        public int baseCost = 10;
        public float costMultiplier = 1.35f;
        public int maxLevel = 5;

        [Header("Effect")]
        public UpgradeType upgradeType;
        public float valuePerLevel = 1f;

        public int GetCost(int currentLevel)
        {
            return Mathf.RoundToInt(baseCost * Mathf.Pow(costMultiplier, currentLevel));
        }

        public float GetTotalValue(int level)
        {
            return valuePerLevel * level;
        }
    }
}