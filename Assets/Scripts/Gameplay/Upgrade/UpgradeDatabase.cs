using UnityEngine;

namespace PlinkoPinball.Gameplay.Upgrade
{
    [CreateAssetMenu(fileName = "UpgradeDatabase", menuName = "PlinkoPinball/Upgrade/Upgrade Database")]
    public sealed class UpgradeDatabase : ScriptableObject
    {
        public UpgradeDefinition[] upgrades;

        public UpgradeDefinition FindById(string upgradeId)
        {
            foreach (var upgrade in upgrades)
            {
                if (upgrade != null && upgrade.upgradeId == upgradeId)
                {
                    return upgrade;
                }
            }

            return null;
        }
    }
}