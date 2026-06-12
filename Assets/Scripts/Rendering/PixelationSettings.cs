using UnityEngine;

namespace PlinkoPinball.Rendering
{
    /// <summary>
    /// 픽셀레이션을 위한 런타임 세팅
    /// </summary>
    [CreateAssetMenu(fileName = "PixelationSettings", menuName = "PlinkoPinball/Rendering/Pixelation Settings")]
    public sealed class PixelationSettings : ScriptableObject
    {
        [Header("Pixelation")]
        [Min(1)]
        public int pixelSize = 4;

        [Header("Color Quantization")]
        [Range(0, 64)]
        public int colorSteps = 24;

        [Header("Debug")]
        public bool enabledInSceneView = false;
    }
}