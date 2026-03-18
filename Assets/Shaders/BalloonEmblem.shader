Shader "Inkshot/BalloonEmblem"
{
    Properties
    {
        _EmblemColor ("Emblem Color", Color) = (1, 0.85, 0.2, 1)
        _Shape ("Shape", Range(0, 8)) = 0
        _Glow ("Glow Intensity", Range(0, 2)) = 0.4
    }

    SubShader
    {
        Tags { "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Back

        Pass
        {
            Name "EmblemForward"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _EmblemColor;
                half _Shape;
                half _Glow;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                output.positionCS = TransformObjectToHClip(input.positionOS.xyz);
                output.uv = input.uv;
                return output;
            }

            half circle(half2 uv)
            {
                half dist = length(uv - 0.5);
                return smoothstep(0.38, 0.32, dist);
            }

            half star(half2 uv)
            {
                half2 p = uv - 0.5;
                half angle = atan2(p.y, p.x);
                half r = length(p);
                half wave = cos(angle * 5.0) * 0.12 + 0.22;
                return smoothstep(wave + 0.04, wave, r);
            }

            half triangle(half2 uv)
            {
                half2 p = uv - half2(0.5, 0.42);
                half edge = abs(p.x) * 1.6 + p.y * 0.8;
                half bottom = -p.y - 0.18;
                half shape = max(edge - 0.28, bottom);
                return smoothstep(0.02, 0.0, shape);
            }

            half crown(half2 uv)
            {
                half2 p = uv - 0.5;
                half base_rect = step(abs(p.x), 0.28) * step(-p.y, 0.18) * step(p.y, 0.08);
                half peak1 = smoothstep(0.06, 0.0, length(half2(p.x + 0.2, p.y - 0.22)));
                half peak2 = smoothstep(0.06, 0.0, length(half2(p.x, p.y - 0.28)));
                half peak3 = smoothstep(0.06, 0.0, length(half2(p.x - 0.2, p.y - 0.22)));
                return saturate(base_rect + peak1 + peak2 + peak3);
            }

            half shield(half2 uv)
            {
                half2 p = uv - half2(0.5, 0.52);
                half top = step(abs(p.x), 0.24) * step(p.y, 0.14);
                half bottom_curve = smoothstep(0.26, 0.22, length(half2(p.x * 0.9, p.y + 0.06)));
                return saturate(top + bottom_curve) * step(-0.24, p.y);
            }

            half skull(half2 uv)
            {
                half2 p = uv - half2(0.5, 0.56);
                half head = smoothstep(0.24, 0.2, length(p));
                half jaw = step(abs(uv.x - 0.5), 0.12) * step(abs(uv.y - 0.28), 0.08);
                half eyes = smoothstep(0.04, 0.02, length(uv - half2(0.42, 0.56)))
                          + smoothstep(0.04, 0.02, length(uv - half2(0.58, 0.56)));
                return saturate(head + jaw - eyes);
            }

            half bolt(half2 uv)
            {
                half2 p = uv - 0.5;
                half a = smoothstep(0.05, 0.0, abs(p.x + p.y * 0.35 + 0.03));
                half b = smoothstep(0.05, 0.0, abs(p.x - p.y * 0.45 - 0.04));
                half mask = step(abs(p.x), 0.24) * step(abs(p.y), 0.32);
                return saturate((a + b) * mask);
            }

            half clover(half2 uv)
            {
                half c1 = smoothstep(0.14, 0.1, length(uv - half2(0.42, 0.58)));
                half c2 = smoothstep(0.14, 0.1, length(uv - half2(0.58, 0.58)));
                half c3 = smoothstep(0.14, 0.1, length(uv - half2(0.48, 0.42)));
                half stem = step(abs(uv.x - 0.5), 0.04) * step(uv.y, 0.34) * step(0.18, uv.y);
                return saturate(c1 + c2 + c3 + stem);
            }

            half target(half2 uv)
            {
                half2 p = uv - 0.5;
                half r = length(p);
                half ring1 = smoothstep(0.26, 0.23, r) - smoothstep(0.18, 0.15, r);
                half ring2 = smoothstep(0.1, 0.07, r);
                half cross = step(abs(p.x), 0.02) * step(r, 0.28) + step(abs(p.y), 0.02) * step(r, 0.28);
                return saturate(ring1 + ring2 + cross);
            }

            half4 frag(Varyings input) : SV_Target
            {
                half2 uv = input.uv;
                half alpha = 0;

                int shape = (int)round(_Shape);
                if (shape == 0) alpha = circle(uv);
                else if (shape == 1) alpha = star(uv);
                else if (shape == 2) alpha = triangle(uv);
                else if (shape == 3) alpha = crown(uv);
                else if (shape == 4) alpha = shield(uv);
                else if (shape == 5) alpha = skull(uv);
                else if (shape == 6) alpha = bolt(uv);
                else if (shape == 7) alpha = clover(uv);
                else alpha = target(uv);

                half3 color = _EmblemColor.rgb * (1.0 + _Glow);
                return half4(color, alpha * _EmblemColor.a);
            }
            ENDHLSL
        }
    }

    Fallback Off
}
