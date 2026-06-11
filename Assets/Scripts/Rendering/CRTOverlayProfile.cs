using UnityEngine;

namespace PlinkoPinball.Rendering
{
    /// <summary>
    /// 전체 게임 화면을 감싸는 CRT Overlay Layer의 표시 값을 정의합니다.
    /// Pinball / Plinko RenderTexture 구조와 독립적으로 동작합니다.
    /// </summary>
    [CreateAssetMenu(fileName = "CRT_Overlay_Profile", menuName = "PlinkoPinball/Rendering/CRT Overlay Profile")]
    public sealed class CRTOverlayProfile : ScriptableObject
    {
        [Header("Base")]
        [Range(0f, 1f)] public float overlayStrength = 0.55f;
        public Color tintColor = new(0.55f, 0.9f, 1f, 1f);

        [Header("Scanline")]
        [Range(0f, 1f)] public float scanlineStrength = 0.18f;
        [Range(100f, 1600f)] public float scanlineCount = 720f;

        [Header("Vignette")]
        [Range(0f, 1f)] public float vignetteStrength = 0.35f;
        [Range(0f, 2f)] public float cornerDarkness = 0.8f;

        [Header("Noise")]
        [Range(0f, 1f)] public float noiseStrength = 0.035f;
        [Range(0f, 30f)] public float noiseSpeed = 12f;

        [Header("Signal Jitter")]
        [Range(0f, 0.02f)] public float horizontalJitter = 0.002f;
        [Range(0f, 30f)] public float jitterSpeed = 8f;

        [Header("Glass")]
        [Range(0f, 1f)] public float glassAlpha = 0.08f;
    }
}
