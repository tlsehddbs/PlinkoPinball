using UnityEngine;

namespace PlinkoPinball.Gameplay.Upgrade
{
    /// <summary>
    /// UpgradeSystem의 영구 업그레이드 상태를 실제 게임 수치로 변환하는 단일 진입점.
    /// Gameplay System은 UpgradeSystem 대신 이 Resolver만 참조한다.
    /// </summary>
    public sealed class UpgradeEffectResolver : MonoBehaviour
    {
        public static UpgradeEffectResolver Instance { get; private set; }

        [Header("References")]
        [SerializeField] private UpgradeSystem upgradeSystem;

        [Header("Compression")]
        [SerializeField, Min(0f)] private float baseCompressionThreshold = 10f;
        [SerializeField, Min(0f)] private float minCompressionThreshold = 5f;

        [Header("Plinko Value")]
        [SerializeField, Min(0)] private int basePinValue = 1;
        [SerializeField, Min(0)] private int baseSlotValue = 5;

        [Header("Plinko Error Rate")]
        [SerializeField, Range(0f, 1f)] private float baseErrorPinRate = 0.25f;
        [SerializeField, Range(0f, 1f)] private float baseErrorSlotRate = 0.25f;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }

            ResolveUpgradeSystem();
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }
        }

        public void Initialize(UpgradeSystem system)
        {
            upgradeSystem = system;
        }

        public float GetCompressionThreshold()
        {
            float reduction = GetUpgradeTotal(UpgradeType.CompressionThresholdReduction);
            return Mathf.Max(minCompressionThreshold, baseCompressionThreshold - reduction);
        }

        public int GetBasePinValue()
        {
            float increase = GetUpgradeTotal(UpgradeType.BasePinValueIncrease);
            return Mathf.Max(0, Mathf.RoundToInt(basePinValue + increase));
        }

        public int GetBaseSlotValue()
        {
            float increase = GetUpgradeTotal(UpgradeType.BaseSlotValueIncrease);
            return Mathf.Max(0, Mathf.RoundToInt(baseSlotValue + increase));
        }

        public float GetErrorPinRate()
        {
            float reduction = GetUpgradeTotal(UpgradeType.ErrorPinRateReduction);
            return Mathf.Max(0f, baseErrorPinRate - reduction);
        }

        public float GetErrorSlotRate()
        {
            float reduction = GetUpgradeTotal(UpgradeType.ErrorSlotRateReduction);
            return Mathf.Max(0f, baseErrorSlotRate - reduction);
        }

        public int GetUnlockedAmplifierTier()
        {
            return GetUnlockedTier(UpgradeType.ChipsetTierAmplifier);
        }

        public int GetUnlockedOverclockTier()
        {
            return GetUnlockedTier(UpgradeType.ChipsetTierOverclock);
        }

        public int GetUnlockedErrorCorrectionTier()
        {
            return GetUnlockedTier(UpgradeType.ChipsetTierErrorCorrection);
        }

        private float GetUpgradeTotal(UpgradeType type)
        {
            ResolveUpgradeSystem();
            return upgradeSystem != null ? upgradeSystem.GetTotalValue(type) : 0f;
        }

        private int GetUnlockedTier(UpgradeType type)
        {
            ResolveUpgradeSystem();
            return upgradeSystem != null ? upgradeSystem.GetUnlockedTier(type) : 0;
        }

        private void ResolveUpgradeSystem()
        {
            if (upgradeSystem != null)
            {
                return;
            }

            upgradeSystem = UpgradeSystem.Instance;
            if (upgradeSystem != null)
            {
                return;
            }

            upgradeSystem = FindFirstObjectByType<UpgradeSystem>();
        }
    }
}
