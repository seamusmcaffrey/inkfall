Shader "Inkshot/ChromeDart"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.78, 0.82, 0.88, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.95
        _Metallic ("Metallic", Range(0, 1)) = 1
        _FresnelTint ("Fresnel Tint", Color) = (0.6, 0.9, 1, 1)
        _FresnelPower ("Fresnel Power", Range(0.5, 8)) = 3
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }

        Pass
        {
            Name "Forward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Smoothness;
                half _Metallic;
                half4 _FresnelTint;
                half _FresnelPower;
            CBUFFER_END

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

            Varyings vert(Attributes input)
            {
                Varyings output;
                VertexPositionInputs pos = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = pos.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(pos.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                half3 normalWS = normalize(input.normalWS);
                half3 viewDir = normalize(input.viewDirWS);
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 halfDir = normalize(lightDir + viewDir);
                half spec = pow(saturate(dot(normalWS, halfDir)), lerp(16, 128, _Smoothness));
                half fresnel = pow(1.0 - saturate(dot(normalWS, viewDir)), _FresnelPower);
                half3 color = _BaseColor.rgb * 0.25;
                color += spec * lerp(0.5, 1.2, _Metallic) * mainLight.color;
                color += fresnel * _FresnelTint.rgb * 0.35;
                return half4(color, 1);
            }
            ENDHLSL
        }
    }
}
