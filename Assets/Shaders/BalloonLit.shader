Shader "Inkshot/BalloonLit"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.9, 0.2, 0.27, 1)
        _Glossiness ("Glossiness", Range(0, 1)) = 0.85
        [NoScaleOffset] _BumpMap ("Normal Map", 2D) = "bump" {}
        _BumpScale ("Normal Scale", Range(0, 2)) = 0.3
        _RimPower ("Rim Power", Range(0.5, 8)) = 5.0
        _RimColor ("Rim Color", Color) = (1, 1, 1, 1)
        _RimIntensity ("Rim Intensity", Range(0, 3)) = 0.08
        _GradientStrength ("Gradient Strength", Range(0, 0.5)) = 0.32
        _SpecularIntensity ("Specular Intensity", Range(0, 8)) = 2.5
        _SpecularSize ("Specular Size", Range(1, 512)) = 180
        _AmbientBoost ("Ambient Boost", Range(0, 1)) = 0.06
        _EnvReflection ("Environment Reflection", Range(0, 1)) = 0.3
        _EdgeDarken ("Edge Darken", Range(0, 1)) = 0.2
        _ShadowContrast ("Shadow Contrast", Range(0, 1)) = 0.5
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }

        Pass
        {
            Name "BalloonForward"
            Tags { "LightMode"="UniversalForward" }
            Cull Back

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/UnityInstancing.hlsl"

            TEXTURE2D(_BumpMap);
            SAMPLER(sampler_BumpMap);

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Glossiness;
                half _BumpScale;
                half _RimPower;
                half4 _RimColor;
                half _RimIntensity;
                half _GradientStrength;
                half _SpecularIntensity;
                half _SpecularSize;
                half _AmbientBoost;
                half _EnvReflection;
                half _EdgeDarken;
                half _ShadowContrast;
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
                float4 tangentOS : TANGENT;
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
                float3 tangentWS : TEXCOORD4;
                float3 bitangentWS : TEXCOORD5;
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
                output.tangentWS = TransformObjectToWorldDir(input.tangentOS.xyz);
                output.bitangentWS = cross(output.normalWS, output.tangentWS) * input.tangentOS.w;
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(positionInputs.positionWS);
                output.positionWS = positionInputs.positionWS;
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);
                #ifdef UNITY_INSTANCING_ENABLED
                half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);
                #else
                half4 baseColor = _BaseColor;
                #endif

                half3 geomNormal = normalize(input.normalWS);
                half3 tangentWS = normalize(input.tangentWS);
                half3 bitangentWS = normalize(input.bitangentWS);

                half4 normalSample = SAMPLE_TEXTURE2D(_BumpMap, sampler_BumpMap, input.uv);
                half3 normalTS = UnpackNormalScale(normalSample, _BumpScale);
                half3 normalWS = normalize(
                    normalTS.x * tangentWS +
                    normalTS.y * bitangentWS +
                    normalTS.z * geomNormal
                );

                half3 viewDir = normalize(input.viewDirWS);
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);

                // Wrap lighting with shadow contrast
                half NdotL = dot(normalWS, lightDir);
                half wrap = saturate(NdotL * 0.5 + 0.5);
                half shadowMask = saturate(NdotL * 1.2 + 0.1);
                half lighting = lerp(wrap, shadowMask, _ShadowContrast);

                // Dual specular: sharp highlight + broad latex sheen
                half3 halfDir = normalize(lightDir + viewDir);
                half NdotH = saturate(dot(normalWS, halfDir));
                half specSharp = pow(NdotH, _SpecularSize) * _SpecularIntensity;
                half specBroad = pow(NdotH, 16.0) * 0.15 * _Glossiness;

                // Fresnel and edge darkening (simulates latex thickness at edges)
                half fresnel = 1.0 - saturate(dot(normalWS, viewDir));
                half edgeFactor = 1.0 - pow(fresnel, 1.5) * _EdgeDarken;
                half rim = pow(fresnel, _RimPower) * _RimIntensity;

                // Subsurface scattering approximation for latex translucency
                half sss = saturate(dot(viewDir, -lightDir)) * fresnel * 0.08;

                // Fake environment reflection (matcap-like overhead booth lighting)
                half3 viewNormal = mul((half3x3)UNITY_MATRIX_V, normalWS);
                half envGradient = viewNormal.y * 0.5 + 0.5;
                half envReflect = pow(envGradient, 2.0) * _EnvReflection * _Glossiness;
                half3 reflColor = lerp(half3(1, 1, 1), baseColor.rgb * 2.0, 0.15) * envReflect;

                // Vertical gradient for depth curvature (darkens bottom)
                half gradient = lerp(1.0, 1.0 - _GradientStrength, saturate(1.0 - input.uv.y));

                // Diffuse with edge darkening applied before specular
                half3 diffuse = baseColor.rgb * (lighting + _AmbientBoost) * gradient;
                diffuse += rim * lerp(_RimColor.rgb, baseColor.rgb, 0.3);
                diffuse += sss * baseColor.rgb * mainLight.color;

                // Additional lights
                #ifdef _ADDITIONAL_LIGHTS
                uint additionalLightCount = GetAdditionalLightsCount();
                for (uint li = 0u; li < additionalLightCount; li++)
                {
                    Light addLight = GetAdditionalLight(li, float4(input.positionWS, 1));
                    half addNdotL = dot(normalWS, normalize(addLight.direction));
                    half addWrap = saturate(addNdotL * 0.5 + 0.5);
                    half addShadow = saturate(addNdotL * 1.2 + 0.1);
                    half addLighting = lerp(addWrap, addShadow, _ShadowContrast);
                    half atten = addLight.distanceAttenuation * addLight.shadowAttenuation;
                    diffuse += baseColor.rgb * addLighting * addLight.color * atten * 0.35;
                }
                #endif

                // Tone curve on diffuse (gentler — preserves color richness)
                diffuse = diffuse / (diffuse + 0.8) * 1.8;

                // Saturation boost for vibrancy
                half luma = dot(diffuse, half3(0.299, 0.587, 0.114));
                diffuse = lerp(half3(luma, luma, luma), diffuse, 1.45);

                // Apply edge darkening to diffuse
                diffuse *= edgeFactor;

                // Specular (added after tone curve so highlights stay bright and white)
                half3 specTint = lerp(mainLight.color, half3(1, 1, 1), 0.85);
                half3 specular = (specSharp + specBroad) * specTint;

                // Additional lights specular
                #ifdef _ADDITIONAL_LIGHTS
                for (uint si = 0u; si < additionalLightCount; si++)
                {
                    Light addLight = GetAdditionalLight(si, float4(input.positionWS, 1));
                    half3 addHalf = normalize(normalize(addLight.direction) + viewDir);
                    half addSpec = pow(saturate(dot(normalWS, addHalf)), _SpecularSize * 0.5) * 0.4;
                    half atten = addLight.distanceAttenuation * addLight.shadowAttenuation;
                    specular += addSpec * addLight.color * atten * 0.65;
                }
                #endif

                // Final composition: diffuse + specular + environment reflection
                half3 color = diffuse + specular + reflColor;
                return half4(saturate(color), 1);
            }
            ENDHLSL
        }
    }

    Fallback "Universal Render Pipeline/Lit"
}
