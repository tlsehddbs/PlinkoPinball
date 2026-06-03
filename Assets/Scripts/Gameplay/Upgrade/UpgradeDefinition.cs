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


        [Header("Presentation")]
        public string category;
        public string flavorText;

        [Header("Unlock")]
        public bool requirePrerequisitesMaxLevel;

        [Header("Balance")]
        public bool isMajorNode;

        [Header("Cost")]
        public int baseCost = 10;
        public float costMultiplier = 1.35f;
        public int maxLevel = 5;


        [Header("Tree")]
        public UpgradeDefinition[] prerequisites;
        public Vector2Int gridPosition;

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