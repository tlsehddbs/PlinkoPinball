using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RenderGraphModule.Util;
using UnityEngine.Rendering.Universal;

namespace PlinkoPinball.Rendering
{
    public sealed class PixelationRenderer : ScriptableRendererFeature
    {
        [System.Serializable]
        public sealed class Settings
        {
            public PixelationSettings pixelationSettings;
            public RenderPassEvent passEvent = RenderPassEvent.AfterRenderingPostProcessing;
            public Shader shader;
        }

        [SerializeField]
        private Settings settings = new();

        private Material material;
        private PixelationPass pass;

        public override void Create()
        {
            if (settings.shader == null)
            {
                settings.shader = Shader.Find("Hidden/PlinkoPinball/Pixelation");
            }

            CoreUtils.Destroy(material);

            material = settings.shader != null
                ? CoreUtils.CreateEngineMaterial(settings.shader)
                : null;

            pass = new PixelationPass(material, settings.pixelationSettings)
            {
                renderPassEvent = settings.passEvent
            };
        }

        public override void AddRenderPasses(
            ScriptableRenderer renderer,
            ref RenderingData renderingData)
        {
            if (material == null || settings.pixelationSettings == null || pass == null)
            {
                return;
            }

            Camera camera = renderingData.cameraData.camera;

            if (camera.cameraType == CameraType.SceneView &&
                !settings.pixelationSettings.enabledInSceneView)
            {
                return;
            }

            renderer.EnqueuePass(pass);
        }

        protected override void Dispose(bool disposing)
        {
            CoreUtils.Destroy(material);
            material = null;
            pass = null;
        }

        private sealed class PixelationPass : ScriptableRenderPass
        {
            private static readonly int PixelSizeId = Shader.PropertyToID("_PixelSize");
            private static readonly int ColorStepsId = Shader.PropertyToID("_ColorSteps");

            private readonly Material material;
            private readonly PixelationSettings settings;

            public PixelationPass(Material material, PixelationSettings settings)
            {
                this.material = material;
                this.settings = settings;
            }

            public override void RecordRenderGraph(
                RenderGraph renderGraph,
                ContextContainer frameData)
            {
                if (material == null || settings == null)
                {
                    return;
                }

                UniversalResourceData resourceData =
                    frameData.Get<UniversalResourceData>();

                TextureHandle source = resourceData.activeColorTexture;

                if (!source.IsValid())
                {
                    return;
                }

                TextureDesc destinationDesc = renderGraph.GetTextureDesc(source);
                destinationDesc.name = "_PlinkoPixelationColor";
                destinationDesc.clearBuffer = false;

                TextureHandle destination =
                    renderGraph.CreateTexture(destinationDesc);

                material.SetFloat(PixelSizeId, Mathf.Max(1, settings.pixelSize));
                material.SetFloat(ColorStepsId, settings.colorSteps);

                RenderGraphUtils.BlitMaterialParameters blitParameters =
                    new(source, destination, material, 0);

                renderGraph.AddBlitPass(
                    blitParameters,
                    "PlinkoPinball Pixelation");

                resourceData.cameraColor = destination;
            }
        }
    }
}