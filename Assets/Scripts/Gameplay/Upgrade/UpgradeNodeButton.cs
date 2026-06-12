using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using PlinkoPinball.Core;
using PlinkoPinball.UI.Mainframe;

namespace PlinkoPinball.Gameplay.Upgrade.UI
{
    public sealed class UpgradeNodeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
    {
        [SerializeField] private Button button;
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text costText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private Image backgroundImage;
        [SerializeField] private MainframeTheme theme;

        private UpgradeDefinition definition;
        private UpgradeSystem upgradeSystem;
        private System.Action<UpgradeDefinition> purchaseRequested;
        private System.Action<UpgradeDefinition> infoRequested;
        private System.Action<UpgradeDefinition> infoCleared;

        public void Initialize(
            UpgradeDefinition definition,
            UpgradeSystem upgradeSystem,
            System.Action<UpgradeDefinition> purchaseRequested,
            System.Action<UpgradeDefinition> infoRequested = null,
            System.Action<UpgradeDefinition> infoCleared = null,
            MainframeTheme theme = null)
        {
            this.definition = definition;
            this.upgradeSystem = upgradeSystem;
            this.purchaseRequested = purchaseRequested;
            this.infoRequested = infoRequested;
            this.infoCleared = infoCleared;
            this.theme = theme != null ? theme : this.theme;

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
            bool canPurchase = unlocked && affordable && !maxed;

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

            ApplyTheme(unlocked, affordable, maxed);
            SetInteractable(canPurchase);
        }

        private void OnClicked()
        {
            purchaseRequested?.Invoke(definition);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            infoRequested?.Invoke(definition);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            infoCleared?.Invoke(definition);
        }

        public void OnSelect(BaseEventData eventData)
        {
            infoRequested?.Invoke(definition);
        }

        public void OnDeselect(BaseEventData eventData)
        {
            infoCleared?.Invoke(definition);
        }

        private void SetInteractable(bool value)
        {
            if (button != null)
            {
                button.interactable = value;
            }
        }

        private void ApplyTheme(bool unlocked, bool affordable, bool maxed)
        {
            ResolveReferences();

            Color textColor = ResolveAccentColor();
            Color mutedColor = ResolveDimColor();
            Color buttonColor = ResolveButtonColor(maxed, unlocked, affordable);

            if (backgroundImage != null)
            {
                backgroundImage.color = buttonColor;
            }

            if (button != null)
            {
                ColorBlock colors = button.colors;
                colors.normalColor = buttonColor;
                colors.highlightedColor = theme != null ? theme.buttonHighlightedColor : new Color(0.88f, 0.88f, 0.86f, 1f);
                colors.pressedColor = theme != null ? theme.buttonPressedColor : new Color(0.56f, 0.56f, 0.54f, 1f);
                colors.selectedColor = colors.highlightedColor;
                colors.disabledColor = !unlocked ? new Color(0.48f, 0.48f, 0.46f, 0.82f) : new Color(0.62f, 0.62f, 0.60f, 0.9f);
                colors.colorMultiplier = 1f;
                colors.fadeDuration = 0.08f;
                button.colors = colors;
            }

            Color resolvedTextColor = unlocked ? textColor : mutedColor;
            SetTextColor(titleText, resolvedTextColor);
            SetTextColor(levelText, resolvedTextColor);
            SetTextColor(descriptionText, resolvedTextColor);
            SetTextColor(costText, maxed ? textColor : (affordable && unlocked ? textColor : mutedColor));
            SetTextFont(titleText);
            SetTextFont(levelText);
            SetTextFont(descriptionText);
            SetTextFont(costText);
        }

        private Color ResolveButtonColor(bool maxed, bool unlocked, bool affordable)
        {
            if (maxed)
            {
                return theme != null ? theme.buttonHighlightedColor : new Color(0.88f, 0.88f, 0.86f, 1f);
            }

            if (!unlocked)
            {
                return new Color(0.52f, 0.52f, 0.50f, 0.9f);
            }

            if (!affordable)
            {
                return new Color(0.66f, 0.66f, 0.64f, 0.95f);
            }

            return theme != null ? theme.buttonNormalColor : new Color(0.76f, 0.76f, 0.74f, 1f);
        }

        private void ResolveReferences()
        {
            if (button == null)
            {
                button = GetComponent<Button>();
            }

            if (backgroundImage == null)
            {
                backgroundImage = GetComponent<Image>();
            }

            if (theme == null)
            {
                MainframeOSBootstrap bootstrap = FindFirstObjectByType<MainframeOSBootstrap>();
                if (bootstrap != null)
                {
                    theme = bootstrap.Theme;
                }
            }
        }

        private Color ResolveAccentColor()
        {
            return theme != null ? theme.accentColor : new Color(0.05f, 0.05f, 0.05f, 1f);
        }

        private Color ResolveDimColor()
        {
            return theme != null ? theme.dimAccentColor : new Color(0.24f, 0.24f, 0.24f, 0.86f);
        }

        private static void SetTextColor(TMP_Text text, Color color)
        {
            if (text != null)
            {
                text.color = color;
            }
        }

        private void SetTextFont(TMP_Text text)
        {
            if (text != null && theme != null)
            {
                text.font = theme.ResolveFont(text.font);
            }
        }
    }
}
