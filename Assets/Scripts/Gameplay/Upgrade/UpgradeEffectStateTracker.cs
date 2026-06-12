using System.Text;
using UnityEngine;

namespace PlinkoPinball.Gameplay.Upgrade
{
    /// <summary>
    /// Runtime debug helper for inspecting the final gameplay values produced by upgrades.
    /// </summary>
    [DefaultExecutionOrder(1000)]
    public sealed class UpgradeEffectStateTracker : MonoBehaviour
    {
        private const string AutoObjectName = "UpgradeEffectStateTracker";

        [Header("Runtime References")]
        [SerializeField] private UpgradeSystem upgradeSystem;
        [SerializeField] private UpgradeEffectResolver resolver;

        [Header("Final Gameplay Values")]
        [SerializeField] private float compressionThreshold;
        [SerializeField] private int basePinValue;
        [SerializeField] private int baseSlotValue;
        [SerializeField] private float errorPinRate;
        [SerializeField] private float errorSlotRate;
        [SerializeField] private int unlockedAmplifierTier;
        [SerializeField] private int unlockedOverclockTier;
        [SerializeField] private int unlockedErrorCorrectionTier;

        [Header("Raw Upgrade Totals")]
        [SerializeField] private float compressionThresholdReduction;
        [SerializeField] private float basePinValueIncrease;
        [SerializeField] private float baseSlotValueIncrease;
        [SerializeField] private float errorPinRateReduction;
        [SerializeField] private float errorSlotRateReduction;

        [Header("Logging")]
        [SerializeField] private bool logOnRefresh = true;
        [SerializeField] private bool logOnlyWhenChanged = true;

        private string _lastSnapshotText;
        private bool _subscribed;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void CreateRuntimeTracker()
        {
            if (FindFirstObjectByType<UpgradeEffectStateTracker>() != null)
            {
                return;
            }

            GameObject trackerObject = new GameObject(AutoObjectName);
            DontDestroyOnLoad(trackerObject);
            trackerObject.AddComponent<UpgradeEffectStateTracker>();
        }

        private void OnEnable()
        {
            ResolveReferences();
            Subscribe();
            Refresh("OnEnable");
        }

        private void OnDisable()
        {
            Unsubscribe();
        }

        private void Update()
        {
            if (resolver == null || upgradeSystem == null)
            {
                ResolveReferences();
                Subscribe();
                Refresh("ReferencesResolved");
            }
        }

        [ContextMenu("Refresh Upgrade Effect State")]
        public void RefreshFromContextMenu()
        {
            Refresh("ContextMenu");
        }

        private void HandleUpgradeStateChanged()
        {
            Refresh("UpgradePurchased");
        }

        private void Refresh(string reason)
        {
            ResolveReferences();

            if (resolver == null)
            {
                return;
            }

            compressionThreshold = resolver.GetCompressionThreshold();
            basePinValue = resolver.GetBasePinValue();
            baseSlotValue = resolver.GetBaseSlotValue();
            errorPinRate = resolver.GetErrorPinRate();
            errorSlotRate = resolver.GetErrorSlotRate();
            unlockedAmplifierTier = resolver.GetUnlockedAmplifierTier();
            unlockedOverclockTier = resolver.GetUnlockedOverclockTier();
            unlockedErrorCorrectionTier = resolver.GetUnlockedErrorCorrectionTier();

            if (upgradeSystem != null)
            {
                compressionThresholdReduction = upgradeSystem.GetTotalValue(UpgradeType.CompressionThresholdReduction);
                basePinValueIncrease = upgradeSystem.GetTotalValue(UpgradeType.BasePinValueIncrease);
                baseSlotValueIncrease = upgradeSystem.GetTotalValue(UpgradeType.BaseSlotValueIncrease);
                errorPinRateReduction = upgradeSystem.GetTotalValue(UpgradeType.ErrorPinRateReduction);
                errorSlotRateReduction = upgradeSystem.GetTotalValue(UpgradeType.ErrorSlotRateReduction);
            }

            LogSnapshot(reason);
        }

        private void ResolveReferences()
        {
            if (upgradeSystem == null)
            {
                upgradeSystem = UpgradeSystem.Instance;
            }

            if (upgradeSystem == null)
            {
                upgradeSystem = FindFirstObjectByType<UpgradeSystem>();
            }

            if (resolver == null)
            {
                resolver = UpgradeEffectResolver.Instance;
            }

            if (resolver == null)
            {
                resolver = FindFirstObjectByType<UpgradeEffectResolver>();
            }
        }

        private void Subscribe()
        {
            if (_subscribed || upgradeSystem == null)
            {
                return;
            }

            upgradeSystem.OnUpgradeStateChanged += HandleUpgradeStateChanged;
            _subscribed = true;
        }

        private void Unsubscribe()
        {
            if (!_subscribed || upgradeSystem == null)
            {
                return;
            }

            upgradeSystem.OnUpgradeStateChanged -= HandleUpgradeStateChanged;
            _subscribed = false;
        }

        private void LogSnapshot(string reason)
        {
            if (!logOnRefresh)
            {
                return;
            }

            string snapshotText = BuildSnapshotText(reason);
            if (logOnlyWhenChanged && snapshotText == _lastSnapshotText)
            {
                return;
            }

            _lastSnapshotText = snapshotText;
            Debug.Log(snapshotText, this);
        }

        private string BuildSnapshotText(string reason)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"[UpgradeEffectStateTracker] {reason}");
            sb.AppendLine($"  CompressionThreshold = {compressionThreshold:0.###} (reduction={compressionThresholdReduction:0.###})");
            sb.AppendLine($"  BasePinValue = {basePinValue} (increase={basePinValueIncrease:0.###})");
            sb.AppendLine($"  BaseSlotValue = {baseSlotValue} (increase={baseSlotValueIncrease:0.###})");
            sb.AppendLine($"  ErrorPinRate = {errorPinRate:0.###} (reduction={errorPinRateReduction:0.###})");
            sb.AppendLine($"  ErrorSlotRate = {errorSlotRate:0.###} (reduction={errorSlotRateReduction:0.###})");
            sb.AppendLine($"  AmplifierTier = {unlockedAmplifierTier}");
            sb.AppendLine($"  OverclockTier = {unlockedOverclockTier}");
            sb.AppendLine($"  ErrorCorrectionTier = {unlockedErrorCorrectionTier}");
            return sb.ToString();
        }
    }
}
