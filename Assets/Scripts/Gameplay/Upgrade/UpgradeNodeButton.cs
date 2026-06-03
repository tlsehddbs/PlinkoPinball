using TMPro;
using UnityEngine;
using UnityEngine.UI;
using PlinkoPinball.Core;

namespace PlinkoPinball.Gameplay.Upgrade.UI
{
    public sealed class UpgradeNodeButton : MonoBehaviour
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text descriptionText;

        private UpgradeDefinition definition;
        private UpgradeSystem upgradeSystem;
        private System.Action<UpgradeDefinition> purchaseRequested;

        public void Initialize(UpgradeDefinition definition, UpgradeSystem upgradeSystem, System.Action<UpgradeDefinition> purchaseRequested)
        {
            this.definition = definition;
            this.upgradeSystem = upgradeSystem;
            this.purchaseRequested = purchaseRequested;

            if (button != null)
            {
                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(OnClicked);
            }

            Refresh();
        }

        public void Refresh()
        {
            if (definition == null || upgradeSystem == null)
            {
                SetInteractable(false);
                return;
            }

            int level = upgradeSystem.GetLevel(definition);
            int cost = definition.GetCost(level);

            bool unlocked = upgradeSystem.IsUnlocked(definition);
            bool maxed = upgradeSystem.IsMaxLevel(definition);
            bool affordable = CurrencySystem.Instance != null && CurrencySystem.Instance.CanAfford(cost);

            if (titleText != null)
            {
                titleText.text = definition.displayName;
            }

            if (levelText != null)
            {
                levelText.text = $"LV {level}/{definition.maxLevel}";
            }

            if (descriptionText != null)
            {
                descriptionText.text = definition.description;
            }

            if (costText != null)
            {
                if (maxed)
                {
                    costText.text = "MAX";
                }
                else if (cost <= 0)
                {
                    costText.text = "FREE";
                }
                else
                {
                    costText.text = $"{cost:N0}";
                }
            }

            SetInteractable(unlocked && affordable && !maxed);
        }

        private void OnClicked()
        {
            purchaseRequested?.Invoke(definition);
        }

        private void SetInteractable(bool value)
        {
            if (button != null)
            {
                button.interactable = value;
            }
        }
    }
}