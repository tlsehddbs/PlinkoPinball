Shader "PlinkoPinball/URP/SimpleCelShader"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.55, 0.55, 0.5, 1)
        _ShadowColor ("Shadow Color", Color) = (0.22, 0.22, 0.2, 1)

        _LightThreshold ("Light Threshold", Range(0, 1)) = 0.55
        _ShadowSoftness ("Shadow Softness", Range(0.001, 0.25)) = 0.03

        _RimColor ("Rim Color", Color) = (0.8, 0.75, 0.55, 1)
        _RimPower ("Rim Power", Range(0.5, 8)) = 3
        _RimStrength ("Rim Strength", Range(0, 1)) = 0.15

        _OutlineColor ("Outline Color", Color) = (0.03, 0.03, 0.025, 1)
        _OutlineWidth ("Outline Width", Range(0, 0.08)) = 0.015
    }

    SubShader
    {
        Tags
        {
            "RenderPipeline"="UniversalPipeline"
            "RenderType"="Opaque"
            "Queue"="Geometry"
        }

        // ------------------------------------------------------------
        // Cel Lit Pass
        // ------------------------------------------------------------
        Pass
        {
            Name "ForwardCelLit"
            Tags { "LightMode"="UniversalForward" }

            Cull Back
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma vertex Vert
            #pragma fragment Frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                half _LightThreshold;
                half _ShadowSoftness;

                half4 _RimColor;
                half _RimPower;
                half _RimStrength;

                half4 _OutlineColor;
                half _OutlineWidth;
            CBUFFER_END

            Varyings Vert(Attributes input)
            {
                Varyings output;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(input.positionOS.xyz);

                VertexNormalInputs normalInputs =
                    GetVertexNormalInputs(input.normalOS);

                output.positionCS = positionInputs.positionCS;
                output.normalWS = normalize(normalInputs.normalWS);
                output.viewDirWS = normalize(GetWorldSpaceViewDir(positionInputs.positionWS));

                return output;
            }

            half4 Frag(Varyings input) : SV_Target
            {
                Light mainLight = GetMainLight();

                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(input.viewDirWS);

                half ndotl = saturate(dot(normalWS, mainLight.direction));

                half lightStep = smoothstep(
                    _LightThreshold - _ShadowSoftness,
                    _LightThreshold + _ShadowSoftness,
                    ndotl
                );

                half3 color = lerp(_ShadowColor.rgb, _BaseColor.rgb, lightStep);

                half rim = pow(1.0h - saturate(dot(normalWS, viewDirWS)), _RimPower);
                color += _RimColor.rgb * rim * _RimStrength;

                return half4(color, _BaseColor.a);
            }

            ENDHLSL
        }

        // ------------------------------------------------------------
        // Outline Pass
        // ------------------------------------------------------------
        Pass
        {
            Name "Outline"
            Tags { "LightMode"="SRPDefaultUnlit" }

            Cull Front
            ZWrite On
            ZTest LEqual

            HLSLPROGRAM

            #pragma vertex OutlineVert
            #pragma fragment OutlineFrag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _ShadowColor;
                half _LightThreshold;
                half _ShadowSoftness;

                half4 _RimColor;
                half _RimPower;
                half _RimStrength;

                half4 _OutlineColor;
                half _OutlineWidth;
            CBUFFER_END

            Varyings OutlineVert(Attributes input)
            {
                Varyings output;

                float3 expandedPositionOS =
                    input.positionOS.xyz + normalize(input.normalOS) * _OutlineWidth;

                VertexPositionInputs positionInputs =
                    GetVertexPositionInputs(expandedPositionOS);

                output.positionCS = positionInputs.positionCS;

                return output;
            }

            half4 OutlineFrag(Varyings input) : SV_Target
            {
                return _OutlineColor;
            }

            ENDHLSL
        }
    }
}