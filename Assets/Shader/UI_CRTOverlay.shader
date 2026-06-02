Shader "PlinkoPinball/UI/CRT Overlay"
{
    Properties
    {
        _TintColor ("Tint Color", Color) = (0.55, 0.9, 1.0, 1.0)

        _OverlayStrength ("Overlay Strength", Range(0, 1)) = 0.55

        _ScanlineStrength ("Scanline Strength", Range(0, 1)) = 0.18
        _ScanlineCount ("Scanline Count", Range(100, 1600)) = 720

        _VignetteStrength ("Vignette Strength", Range(0, 1)) = 0.35
        _CornerDarkness ("Corner Darkness", Range(0, 2)) = 0.8

        _NoiseStrength ("Noise Strength", Range(0, 1)) = 0.035
        _NoiseSpeed ("Noise Speed", Range(0, 30)) = 12

        _HorizontalJitter ("Horizontal Jitter", Range(0, 0.02)) = 0.002
        _JitterSpeed ("Jitter Speed", Range(0, 30)) = 8

        _GlassAlpha ("Glass Alpha", Range(0, 1)) = 0.08
    }

    SubShader
    {
        Tags
        {
            "Queue"="Overlay"
            "RenderType"="Transparent"
            "IgnoreProjector"="True"
            "CanUseSpriteAtlas"="True"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            Name "CRT Overlay"

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            float4 _TintColor;

            float _OverlayStrength;

            float _ScanlineStrength;
            float _ScanlineCount;

            float _VignetteStrength;
            float _CornerDarkness;

            float _NoiseStrength;
            float _NoiseSpeed;

            float _HorizontalJitter;
            float _JitterSpeed;

            float _GlassAlpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv : TEXCOORD0;
                float4 color : COLOR;
            };

            v2f vert(appdata input)
            {
                v2f output;
                output.vertex = UnityObjectToClipPos(input.vertex);
                output.uv = input.uv;
                output.color = input.color;
                return output;
            }

            float random(float2 uv)
            {
                return frac(sin(dot(uv, float2(12.9898, 78.233))) * 43758.5453);
            }

            fixed4 frag(v2f input) : SV_Target
            {
                float2 uv = input.uv;

                float lineIndex = floor(uv.y * _ScanlineCount);
                float scanNoise = random(float2(lineIndex, floor(_Time.y * _JitterSpeed)));

                uv.x += (scanNoise - 0.5) * _HorizontalJitter;

                float scan = sin(uv.y * _ScanlineCount * 3.14159265);
                scan = 0.5 + 0.5 * scan;

                float scanlineAlpha = scan * _ScanlineStrength;

                float2 centered = input.uv * 2.0 - 1.0;
                float radial = dot(centered, centered);

                float vignette = saturate(radial * _VignetteStrength);
                float corner = pow(saturate(radial), 1.8) * _CornerDarkness;

                float noise = random(uv * _ScreenParams.xy + _Time.y * _NoiseSpeed);
                float noiseAlpha = noise * _NoiseStrength;

                float alpha =
                    _GlassAlpha +
                    scanlineAlpha +
                    vignette +
                    corner +
                    noiseAlpha;

                alpha *= _OverlayStrength;
                alpha *= input.color.a;

                float3 color = _TintColor.rgb;

                return float4(color, saturate(alpha));
            }
            ENDCG
        }
    }
}