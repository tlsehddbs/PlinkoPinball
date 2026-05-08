Shader "Hidden/PlinkoPinball/Pixelation"
{
    Properties
    {
        _PixelSize ("Pixel Size", Float) = 4
        _ColorSteps ("Color Steps", Float) = 24
    }

    SubShader
    {
        Tags
        {
            "RenderType"="Opaque"
            "RenderPipeline"="UniversalPipeline"
        }

        ZWrite Off
        Cull Off
        ZTest Always

        Pass
        {
            Name "Pixelation"

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/Runtime/Utilities/Blit.hlsl"

            float _PixelSize;
            float _ColorSteps;

            half4 Frag(Varyings input) : SV_Target
            {
                float2 screenSize = _BlitTexture_TexelSize.zw;
                float pixelSize = max(1.0, _PixelSize);

                float2 pixelatedUV =
                    floor(input.texcoord * screenSize / pixelSize)
                    * pixelSize / screenSize;

                half4 color = SAMPLE_TEXTURE2D_X(
                    _BlitTexture,
                    sampler_LinearClamp,
                    pixelatedUV);

                if (_ColorSteps > 1.0)
                {
                    color.rgb = floor(color.rgb * _ColorSteps) / _ColorSteps;
                }

                return color;
            }

            ENDHLSL
        }
    }
}