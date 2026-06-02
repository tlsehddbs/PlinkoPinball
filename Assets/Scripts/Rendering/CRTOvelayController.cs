using UnityEngine;
using UnityEngine.UI;

namespace PlinkoPinball.Rendering
{
    /// <summary>
    /// 전체 화면 CRT Overlay Image의 Material 값을 제어합니다.
    /// 게임 로직과 분리된 순수 표시 계층입니다.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public sealed class CRTOverlayController : MonoBehaviour
    {
        private static readonly int TintColorId = Shader.PropertyToID("_TintColor");
        private static readonly int OverlayStrengthId = Shader.PropertyToID("_OverlayStrength");
        private static readonly int ScanlineStrengthId = Shader.PropertyToID("_ScanlineStrength");
        private static readonly int ScanlineCountId = Shader.PropertyToID("_ScanlineCount");
        private static readonly int VignetteStrengthId = Shader.PropertyToID("_VignetteStrength");
        private static readonly int CornerDarknessId = Shader.PropertyToID("_CornerDarkness");
        private static readonly int NoiseStrengthId = Shader.PropertyToID("_NoiseStrength");
        private static readonly int NoiseSpeedId = Shader.PropertyToID("_NoiseSpeed");
        private static readonly int HorizontalJitterId = Shader.PropertyToID("_HorizontalJitter");
        private static readonly int JitterSpeedId = Shader.PropertyToID("_JitterSpeed");
        private static readonly int GlassAlphaId = Shader.PropertyToID("_GlassAlpha");

        [Header("Profile")]
        [SerializeField] private CRTOverlayProfile profile;

        [Header("Runtime")]
        [SerializeField] private bool applyOnAwake = true;
        [SerializeField] private bool cloneMaterialInstance = true;

        private Image image;
        private Material runtimeMaterial;

        /// <summary>
        /// 현재 Overlay에 적용 중인 런타임 Material 인스턴스입니다.
        /// </summary>
        public Material RuntimeMaterial => runtimeMaterial;

        private void Awake()
        {
            image = GetComponent<Image>();

            if (cloneMaterialInstance && image.material != null)
            {
                runtimeMaterial = new Material(image.material);
                runtimeMaterial.name = $"{image.material.name}_Runtime";
                image.material = runtimeMaterial;
            }
            else
            {
                runtimeMaterial = image.material;
            }

            image.raycastTarget = false;

            if (applyOnAwake)
            {
                ApplyProfile();
            }
        }

        private void OnDestroy()
        {
            if (cloneMaterialInstance && runtimeMaterial != null)
            {
                Destroy(runtimeMaterial);
            }
        }

        /// <summary>
        /// CRT Overlay Profile을 교체하고 즉시 적용합니다.
        /// </summary>
        public void SetProfile(CRTOverlayProfile newProfile)
        {
            profile = newProfile;
            ApplyProfile();
        }

        /// <summary>
        /// 현재 Profile 값을 CRT Overlay Material에 적용합니다.
        /// </summary>
        [ContextMenu("Apply CRT Overlay Profile")]
        public void ApplyProfile()
        {
            if (profile == null || runtimeMaterial == null)
            {
                return;
            }

            runtimeMaterial.SetColor(TintColorId, profile.tintColor);
            runtimeMaterial.SetFloat(OverlayStrengthId, profile.overlayStrength);
            runtimeMaterial.SetFloat(ScanlineStrengthId, profile.scanlineStrength);
            runtimeMaterial.SetFloat(ScanlineCountId, profile.scanlineCount);
            runtimeMaterial.SetFloat(VignetteStrengthId, profile.vignetteStrength);
            runtimeMaterial.SetFloat(CornerDarknessId, profile.cornerDarkness);
            runtimeMaterial.SetFloat(NoiseStrengthId, profile.noiseStrength);
            runtimeMaterial.SetFloat(NoiseSpeedId, profile.noiseSpeed);
            runtimeMaterial.SetFloat(HorizontalJitterId, profile.horizontalJitter);
            runtimeMaterial.SetFloat(JitterSpeedId, profile.jitterSpeed);
            runtimeMaterial.SetFloat(GlassAlphaId, profile.glassAlpha);
        }
    }
}