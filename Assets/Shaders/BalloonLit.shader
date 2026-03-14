Shader "Inkshot/BalloonLit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.9, 0.2, 0.27, 1)
        _Glossiness ("Glossiness", Range(0, 1)) = 0.85
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.5
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 0.6
        _GradientStrength ("Gradient Strength", Range(0, 0.5)) = 0.15
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1.2
        _SpecularSize ("Specular Size", Range(1, 256)) = 64
        _AmbientBoost ("Ambient Boost", Range(0, 1)) = 0.12
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "BalloonForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Glossiness;
                half _RimPower;
                half4 _RimColor;
                half _RimIntensity;
                half _GradientStrength;
                half _SpecularIntensity;
                half _SpecularSize;
                half _AmbientBoost;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(positionInputs.positionWS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half4 baseColor = _BaseColor;
                half3 normalWS = normalize(input.normalWS);
                half3 viewDir = normalize(input.viewDirWS);
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half NdotL = saturate(dot(normalWS, lightDir) * 0.5 + 0.5);
                half3 halfDir = normalize(lightDir + viewDir);
                half specular = pow(saturate(dot(normalWS, halfDir)), _SpecularSize) * _SpecularIntensity * _Glossiness;
                half rim = pow(1.0 - saturate(dot(normalWS, viewDir)), _RimPower) * _RimIntensity;
                half gradient = lerp(1.0, 1.0 + _GradientStrength, input.uv.y);
                half3 color = baseColor.rgb * (NdotL + _AmbientBoost) * gradient;
                color += specular * mainLight.color;
                color += rim * _RimColor.rgb;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
