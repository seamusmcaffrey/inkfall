Shader "Inkshot/BalloonLit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.9, 0.2, 0.27, 1)
        _Glossiness ("Glossiness", Range(0, 1)) = 0.85
        _RimPower ("Rim Power", Range(0.5, 8)) = 2.5
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimIntensity ("Rim Intensity", Range(0, 2)) = 0.25
        _GradientStrength ("Gradient Strength", Range(0, 0.5)) = 0.12
        _SpecularIntensity ("Specular Intensity", Range(0, 2)) = 1.4
        _SpecularSize ("Specular Size", Range(1, 256)) = 80
        _AmbientBoost ("Ambient Boost", Range(0, 1)) = 0.2
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "BalloonForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Off

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

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

            #ifdef UNITY_INSTANCING_ENABLED
            UNITY_INSTANCING_BUFFER_START(Props)
                UNITY_DEFINE_INSTANCED_PROP(half4, _BaseColor)
            UNITY_INSTANCING_BUFFER_END(Props)
            #endif

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 positionWS : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                VertexPositionInputs positionInputs = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = positionInputs.positionCS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(positionInputs.positionWS);
                output.positionWS = positionInputs.positionWS;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input, half facing : VFACE) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                #ifdef UNITY_INSTANCING_ENABLED
                half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);
                #else
                half4 baseColor = _BaseColor;
                #endif
                half3 normalWS = normalize(input.normalWS) * (facing > 0 ? 1 : -1);
                half3 viewDir = normalize(input.viewDirWS);
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);

                // Wrap lighting for soft balloon shading
                half NdotL = saturate(dot(normalWS, lightDir) * 0.5 + 0.5);
                half NdotL_hard = saturate(dot(normalWS, lightDir));

                // Dual specular: sharp highlight + broad sheen
                half3 halfDir = normalize(lightDir + viewDir);
                half NdotH = saturate(dot(normalWS, halfDir));
                half specSharp = pow(NdotH, _SpecularSize) * _SpecularIntensity;
                half specBroad = pow(NdotH, 8.0) * 0.35 * _Glossiness;

                // Fresnel rim with color tint
                half fresnel = 1.0 - saturate(dot(normalWS, viewDir));
                half rim = pow(fresnel, _RimPower) * _RimIntensity;

                // Subsurface scattering approximation for latex translucency
                half sss = saturate(dot(viewDir, -lightDir)) * fresnel * 0.15;

                // Vertical gradient for depth curvature
                half gradient = lerp(1.0, 1.0 + _GradientStrength, input.uv.y);

                // Darken underside subtly
                half topLight = lerp(0.85, 1.0, saturate(input.uv.y));

                half3 color = baseColor.rgb * (NdotL + _AmbientBoost) * gradient * topLight;
                color += (specSharp + specBroad) * mainLight.color;
                color += rim * _RimColor.rgb;
                color += sss * baseColor.rgb * mainLight.color;

                // Additional lights (neon point lights)
                #ifdef _ADDITIONAL_LIGHTS
                uint additionalLightCount = GetAdditionalLightsCount();
                for (uint li = 0u; li < additionalLightCount; li++)
                {
                    Light addLight = GetAdditionalLight(li, float4(input.positionWS, 1));
                    half addNdotL = saturate(dot(normalWS, normalize(addLight.direction)));
                    half3 addHalf = normalize(normalize(addLight.direction) + viewDir);
                    half addSpec = pow(saturate(dot(normalWS, addHalf)), _SpecularSize * 0.5) * 0.4;
                    half atten = addLight.distanceAttenuation * addLight.shadowAttenuation;
                    color += baseColor.rgb * addNdotL * addLight.color * atten * 0.45;
                    color += addSpec * addLight.color * atten * 0.6;
                }
                #endif

                // Slight saturation boost for vibrancy
                half luma = dot(color, half3(0.299, 0.587, 0.114));
                color = lerp(half3(luma, luma, luma), color, 1.15);

                return half4(color, 1);
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Lit"
}
