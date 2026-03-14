# Plan 02: Visual Overhaul

> **For agentic workers:** REQUIRED: Use superpowers:subagent-driven-development (if subagents available) or superpowers:executing-plans to implement this plan.

**Goal:** Transform INKSHOT from Unity primitives into a premium neon noir carnival aesthetic with custom balloon shaders, procedural meshes, moody lighting, atmospheric effects, and post-processing.

**Architecture:** All visuals are driven by a custom HLSL balloon shader (SRP Batcher compatible, GPU instanced), procedurally generated meshes cached at startup, and URP post-processing. Environment materials use URP/Lit with carefully tuned properties rather than imported textures. The `BalloonWall` and `DartLauncher` are modified to use the new meshes and materials while preserving all existing gameplay logic.

**Tech Stack:** Unity URP 2022.3, custom HLSL shaders, procedural mesh generation (C#), URP Volume post-processing, particle systems for atmosphere.

**Depends on:** Plan 01 (Foundation & Architecture) -- URP pipeline configured, folder structure established, ScriptableObject data layer in place, object pool operational.

---

## File Map

| Action | Path |
|--------|------|
| Create | `Assets/Shaders/BalloonLit.shader` |
| Create | `Assets/Shaders/ChromeDart.shader` |
| Create | `Assets/Scripts/BalloonGame/Visuals/BalloonMeshGenerator.cs` |
| Create | `Assets/Scripts/BalloonGame/Visuals/DartMeshGenerator.cs` |
| Create | `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs` |
| Create | `Assets/Scripts/BalloonGame/Visuals/AtmosphereController.cs` |
| Create | `Assets/Scripts/BalloonGame/Visuals/StuckDartManager.cs` |
| Create | `Assets/Scripts/BalloonGame/Visuals/NeonLightRig.cs` |
| Modify | `Assets/Scripts/BalloonGame/BalloonWall.cs` |
| Modify | `Assets/Scripts/BalloonGame/BalloonNode.cs` |
| Modify | `Assets/Scripts/BalloonGame/BalloonData.cs` |
| Modify | `Assets/Scripts/BalloonGame/DartLauncher.cs` |
| Modify | `Assets/Scripts/BalloonGame/DartController.cs` |
| Modify | `Assets/Scripts/BalloonGame/BalloonCamera.cs` |
| Modify | `Assets/Scripts/BalloonGame/GameConstants.cs` |
| Modify | `Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs` |
| Create | `Assets/Materials/Balloons/BalloonBase.mat` |
| Create | `Assets/Materials/Balloons/BalloonGold.mat` |
| Create | `Assets/Materials/Balloons/BalloonHazard.mat` |
| Create | `Assets/Materials/Environment/BackWallNoir.mat` |
| Create | `Assets/Materials/Environment/WallFrameNoir.mat` |
| Create | `Assets/Materials/Environment/LaneFloorWet.mat` |
| Create | `Assets/Materials/Environment/LaneLine.mat` |
| Create | `Assets/Materials/Darts/ChromeBody.mat` |
| Create | `Assets/Materials/Darts/ChromeTip.mat` |

---

## Task 1: Balloon Shader (Custom HLSL URP)

**Files:**
- Create: `Assets/Shaders/BalloonLit.shader`

This is the hero shader. Custom unlit-with-manual-lighting approach for full control over the latex balloon look. SRP Batcher compatible. Supports GPU instancing with per-instance `_BaseColor` via `MaterialPropertyBlock`.

- [ ] **Step 1: Create the shader file**

```hlsl
// Assets/Shaders/BalloonLit.shader
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
        _SSSColor ("SSS Bleed Color", Color) = (1, 0.4, 0.3, 1)
        _SSSIntensity ("SSS Intensity", Range(0, 1)) = 0.15
        _FresnelColor ("Fresnel Glow Color", Color) = (0.5, 0.2, 1, 1)
        _FresnelIntensity ("Fresnel Glow Intensity", Range(0, 1)) = 0.08
        _AmbientBoost ("Ambient Boost", Range(0, 1)) = 0.12
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "BalloonForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_instancing

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
                half4 _SSSColor;
                half _SSSIntensity;
                half4 _FresnelColor;
                half _FresnelIntensity;
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
                float3 positionWS : TEXCOORD2;
                float2 uv : TEXCOORD3;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                output.uv = input.uv;
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(input.viewDirWS);

                #ifdef UNITY_INSTANCING_ENABLED
                    half4 baseColor = UNITY_ACCESS_INSTANCED_PROP(Props, _BaseColor);
                #else
                    half4 baseColor = _BaseColor;
                #endif

                // --- Main directional light ---
                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);
                half3 lightColor = mainLight.color;

                // Wrapped diffuse for softer balloon shading
                half NdotL = dot(normalWS, lightDir);
                half wrappedDiffuse = saturate(NdotL * 0.5 + 0.5);

                // Blinn-Phong specular
                half3 halfDir = normalize(lightDir + viewDirWS);
                half NdotH = saturate(dot(normalWS, halfDir));
                half specular = pow(NdotH, _SpecularSize) * _SpecularIntensity * _Glossiness;

                // Rim lighting (1 - NdotV)^power
                half NdotV = saturate(dot(normalWS, viewDirWS));
                half rim = pow(1.0 - NdotV, _RimPower) * _RimIntensity;

                // Top-to-bottom gradient (lighter at top in object space)
                // UV.y goes 0->1 bottom to top on a sphere
                half gradient = lerp(1.0, 1.0 + _GradientStrength, input.uv.y);

                // Fake SSS: light bleeding through from behind
                half backlight = saturate(-NdotL * 0.5 + 0.3);
                half3 sss = backlight * _SSSColor.rgb * _SSSIntensity * baseColor.rgb;

                // Fresnel edge glow (neon ambient pickup)
                half fresnel = pow(1.0 - NdotV, 3.0) * _FresnelIntensity;
                half3 fresnelGlow = fresnel * _FresnelColor.rgb;

                // --- Additional lights (neon point lights) ---
                half3 additionalDiffuse = half3(0, 0, 0);
                half3 additionalSpecular = half3(0, 0, 0);
                #ifdef _ADDITIONAL_LIGHTS
                    uint additionalLightCount = GetAdditionalLightsCount();
                    for (uint i = 0u; i < additionalLightCount; i++)
                    {
                        Light addLight = GetAdditionalLight(i, input.positionWS);
                        half addNdotL = saturate(dot(normalWS, addLight.direction) * 0.5 + 0.5);
                        additionalDiffuse += addLight.color * addNdotL * addLight.distanceAttenuation * 0.5;

                        half3 addHalf = normalize(addLight.direction + viewDirWS);
                        half addNdotH = saturate(dot(normalWS, addHalf));
                        additionalSpecular += addLight.color * pow(addNdotH, _SpecularSize * 0.5)
                            * addLight.distanceAttenuation * _Glossiness * 0.3;
                    }
                #endif

                // --- Compose ---
                half3 diffuse = wrappedDiffuse * lightColor + additionalDiffuse + _AmbientBoost;
                half3 color = diffuse * baseColor.rgb * gradient;
                color += specular * lightColor;
                color += additionalSpecular;
                color += rim * _RimColor.rgb;
                color += sss;
                color += fresnelGlow;

                return half4(color, 1.0);
            }
            ENDHLSL
        }

        // Shadow caster pass for receiving shadows
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode" = "ShadowCaster" }

            ZWrite On
            ZTest LEqual
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #pragma multi_compile_instancing

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
                half4 _SSSColor;
                half _SSSIntensity;
                half4 _FresnelColor;
                half _FresnelIntensity;
                half _AmbientBoost;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            float3 _LightDirection;

            Varyings ShadowPassVertex(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                float3 positionWS = TransformObjectToWorld(input.positionOS.xyz);
                float3 normalWS = TransformObjectToWorldNormal(input.normalOS);
                float4 positionCS = TransformWorldToHClip(
                    ApplyShadowBias(positionWS, normalWS, _LightDirection));

                #if UNITY_REVERSED_Z
                    positionCS.z = min(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #else
                    positionCS.z = max(positionCS.z, UNITY_NEAR_CLIP_VALUE);
                #endif

                output.positionCS = positionCS;
                return output;
            }

            half4 ShadowPassFragment(Varyings input) : SV_Target
            {
                return 0;
            }
            ENDHLSL
        }
    }
}
```

- [ ] **Step 2: Create the base balloon material asset**

This is done via `BalloonSceneBuilder` (Task 10), but for reference the material settings are:

| Property | Value |
|----------|-------|
| Shader | `Inkshot/BalloonLit` |
| `_BaseColor` | Set per-instance via `MaterialPropertyBlock` |
| `_Glossiness` | `0.85` |
| `_RimPower` | `2.5` |
| `_RimColor` | `#FFFFFF` (1, 1, 1, 1) |
| `_RimIntensity` | `0.6` |
| `_GradientStrength` | `0.15` |
| `_SpecularIntensity` | `1.2` |
| `_SpecularSize` | `64` |
| `_SSSColor` | `#FF6650` (1, 0.4, 0.31, 1) |
| `_SSSIntensity` | `0.15` |
| `_FresnelColor` | `#8033FF` (0.5, 0.2, 1, 1) |
| `_FresnelIntensity` | `0.08` |
| `_AmbientBoost` | `0.12` |

**Commit:** `git commit -m "feat(visuals): add custom BalloonLit HLSL shader with rim, SSS, specular, fresnel"`

---

## Task 2: Balloon Mesh Generator

**Files:**
- Create: `Assets/Scripts/BalloonGame/Visuals/BalloonMeshGenerator.cs`

Procedurally generates a balloon-shaped mesh: elongated UV sphere (Y scale 1.2) with a small cone tie at the bottom. ~400 triangles. Generated once and cached as a static shared mesh.

- [ ] **Step 1: Create the mesh generator**

```csharp
// Assets/Scripts/BalloonGame/Visuals/BalloonMeshGenerator.cs
using UnityEngine;

/// <summary>
/// Generates and caches procedural balloon meshes. Call EnsureMeshes() once at startup.
/// All balloons share the same Mesh instance to minimize memory and enable batching.
/// </summary>
public static class BalloonMeshGenerator
{
    private const int Segments = 16;
    private const int Rings = 12;
    private const float YElongation = 1.2f;
    private const float BalloonRadius = 0.5f;
    private const float TieHeight = 0.12f;
    private const float TieRadius = 0.06f;

    private static Mesh _standardMesh;
    private static Mesh _hazardMesh;

    /// <summary>Standard balloon mesh: elongated sphere with rounded tie knot.</summary>
    public static Mesh StandardMesh
    {
        get
        {
            if (_standardMesh == null) EnsureMeshes();
            return _standardMesh;
        }
    }

    /// <summary>Hazard balloon mesh: elongated sphere with pointed spike tail.</summary>
    public static Mesh HazardMesh
    {
        get
        {
            if (_hazardMesh == null) EnsureMeshes();
            return _hazardMesh;
        }
    }

    /// <summary>Pre-generate all mesh variants. Call in Awake before any balloons spawn.</summary>
    public static void EnsureMeshes()
    {
        if (_standardMesh != null) return;
        _standardMesh = GenerateBalloonMesh(spikedTail: false);
        _standardMesh.name = "BalloonMesh_Standard";
        _hazardMesh = GenerateBalloonMesh(spikedTail: true);
        _hazardMesh.name = "BalloonMesh_Hazard";
    }

    private static Mesh GenerateBalloonMesh(bool spikedTail)
    {
        // Sphere vertex count: (Rings - 1) * Segments + 2 poles
        int sphereVertCount = (Rings - 1) * Segments + 2;
        int tieVerts = Segments + 1; // ring + tip
        int totalVerts = sphereVertCount + tieVerts;

        var vertices = new Vector3[totalVerts];
        var normals = new Vector3[totalVerts];
        var uvs = new Vector2[totalVerts];

        // === Sphere body ===
        // Top pole
        vertices[0] = new Vector3(0f, BalloonRadius * YElongation, 0f);
        normals[0] = Vector3.up;
        uvs[0] = new Vector2(0.5f, 1f);

        int vi = 1;
        for (int ring = 1; ring < Rings; ring++)
        {
            float phi = Mathf.PI * ring / Rings;
            float sinPhi = Mathf.Sin(phi);
            float cosPhi = Mathf.Cos(phi);
            float v = 1f - (float)ring / Rings;

            for (int seg = 0; seg < Segments; seg++)
            {
                float theta = 2f * Mathf.PI * seg / Segments;
                float sinTheta = Mathf.Sin(theta);
                float cosTheta = Mathf.Cos(theta);

                float x = sinPhi * cosTheta * BalloonRadius;
                float y = cosPhi * BalloonRadius * YElongation;
                float z = sinPhi * sinTheta * BalloonRadius;

                vertices[vi] = new Vector3(x, y, z);
                normals[vi] = new Vector3(
                    sinPhi * cosTheta,
                    cosPhi * (1f / YElongation),
                    sinPhi * sinTheta
                ).normalized;
                uvs[vi] = new Vector2((float)seg / Segments, v);
                vi++;
            }
        }

        // Bottom pole
        int bottomPole = vi;
        vertices[bottomPole] = new Vector3(0f, -BalloonRadius * YElongation, 0f);
        normals[bottomPole] = Vector3.down;
        uvs[bottomPole] = new Vector2(0.5f, 0f);
        vi++;

        // === Tie / tail ===
        int tieStart = vi;
        float tieY = -BalloonRadius * YElongation;
        for (int seg = 0; seg < Segments; seg++)
        {
            float theta = 2f * Mathf.PI * seg / Segments;
            float x = Mathf.Cos(theta) * TieRadius;
            float z = Mathf.Sin(theta) * TieRadius;
            vertices[vi] = new Vector3(x, tieY, z);
            normals[vi] = new Vector3(x, -0.3f, z).normalized;
            uvs[vi] = new Vector2((float)seg / Segments, 0f);
            vi++;
        }

        // Tie tip
        int tieTip = vi;
        float tipY = spikedTail
            ? tieY - TieHeight * 2.5f   // Hazard: longer pointed spike
            : tieY - TieHeight;          // Standard: short knot
        vertices[vi] = new Vector3(0f, tipY, 0f);
        normals[vi] = Vector3.down;
        uvs[vi] = new Vector2(0.5f, 0f);

        // === Triangles ===
        // Sphere: top cap + body quads + bottom cap
        int sphereTriCount = Segments + (Rings - 2) * Segments * 2 + Segments;
        int tieTriCount = Segments; // cone
        int totalTris = sphereTriCount + tieTriCount;
        var triangles = new int[totalTris * 3];
        int ti = 0;

        // Top cap: pole to first ring
        for (int seg = 0; seg < Segments; seg++)
        {
            int next = (seg + 1) % Segments;
            triangles[ti++] = 0;
            triangles[ti++] = 1 + next;
            triangles[ti++] = 1 + seg;
        }

        // Body quads
        for (int ring = 0; ring < Rings - 2; ring++)
        {
            int ringStart = 1 + ring * Segments;
            int nextRingStart = 1 + (ring + 1) * Segments;
            for (int seg = 0; seg < Segments; seg++)
            {
                int next = (seg + 1) % Segments;
                int a = ringStart + seg;
                int b = ringStart + next;
                int c = nextRingStart + seg;
                int d = nextRingStart + next;

                triangles[ti++] = a;
                triangles[ti++] = b;
                triangles[ti++] = c;

                triangles[ti++] = b;
                triangles[ti++] = d;
                triangles[ti++] = c;
            }
        }

        // Bottom cap: last ring to bottom pole
        int lastRingStart = 1 + (Rings - 2) * Segments;
        for (int seg = 0; seg < Segments; seg++)
        {
            int next = (seg + 1) % Segments;
            triangles[ti++] = lastRingStart + seg;
            triangles[ti++] = lastRingStart + next;
            triangles[ti++] = bottomPole;
        }

        // Tie cone: bottom ring of sphere connects to tie ring, tie ring to tip
        // We reuse the last ring of the sphere as the top of the tie,
        // and draw triangles from tie ring to tie tip
        for (int seg = 0; seg < Segments; seg++)
        {
            int next = (seg + 1) % Segments;
            triangles[ti++] = tieStart + seg;
            triangles[ti++] = tieStart + next;
            triangles[ti++] = tieTip;
        }

        var mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.SetNormals(normals);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateBounds();
        mesh.RecalculateTangents();
        return mesh;
    }
}
```

- [ ] **Step 2: Verify triangle count is within budget**

The sphere has `Segments * 2 * (Rings - 1)` = `16 * 2 * 11` = 352 triangles. The tie cone adds 16 triangles. Total: **368 triangles** per balloon. With 72 balloons on the board: 26,496 triangles -- well within the 50k budget.

**Commit:** `git commit -m "feat(visuals): add BalloonMeshGenerator with standard and hazard variants"`

---

## Task 3: Dart Mesh Generator & Chrome Shader

**Files:**
- Create: `Assets/Scripts/BalloonGame/Visuals/DartMeshGenerator.cs`
- Create: `Assets/Shaders/ChromeDart.shader`

Replaces the primitive capsule+sphere dart with a proper dart-shaped procedural mesh and a chrome metallic shader.

- [ ] **Step 1: Create the chrome dart shader**

```hlsl
// Assets/Shaders/ChromeDart.shader
Shader "Inkshot/ChromeDart"
{
    Properties
    {
        _BaseColor ("Base Color", Color) = (0.78, 0.82, 0.88, 1)
        _Metallic ("Metallic", Range(0, 1)) = 1.0
        _Smoothness ("Smoothness", Range(0, 1)) = 0.9
        _ReflectionTint ("Reflection Tint", Color) = (0.7, 0.75, 0.85, 1)
    }

    SubShader
    {
        Tags
        {
            "RenderType" = "Opaque"
            "RenderPipeline" = "UniversalPipeline"
            "Queue" = "Geometry"
        }

        Pass
        {
            Name "ChromeForward"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _ADDITIONAL_LIGHTS
            #pragma multi_compile_instancing

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half _Metallic;
                half _Smoothness;
                half4 _ReflectionTint;
            CBUFFER_END

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct Varyings
            {
                float4 positionCS : SV_POSITION;
                float3 normalWS : TEXCOORD0;
                float3 viewDirWS : TEXCOORD1;
                float3 positionWS : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            Varyings vert(Attributes input)
            {
                Varyings output;
                UNITY_SETUP_INSTANCE_ID(input);
                UNITY_TRANSFER_INSTANCE_ID(input, output);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(input.positionOS.xyz);
                output.positionCS = vertexInput.positionCS;
                output.positionWS = vertexInput.positionWS;
                output.normalWS = TransformObjectToWorldNormal(input.normalOS);
                output.viewDirWS = GetWorldSpaceNormalizeViewDir(vertexInput.positionWS);
                return output;
            }

            half4 frag(Varyings input) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(input);

                half3 normalWS = normalize(input.normalWS);
                half3 viewDirWS = normalize(input.viewDirWS);

                Light mainLight = GetMainLight();
                half3 lightDir = normalize(mainLight.direction);

                // High-contrast chrome: strong specular, metallic reflections
                half NdotL = saturate(dot(normalWS, lightDir));
                half3 halfDir = normalize(lightDir + viewDirWS);
                half NdotH = saturate(dot(normalWS, halfDir));
                half NdotV = saturate(dot(normalWS, viewDirWS));

                // GGX-like specular approximation
                half roughness = 1.0 - _Smoothness;
                half roughSq = roughness * roughness;
                half specPower = 2.0 / (roughSq * roughSq + 0.0001) - 2.0;
                specPower = clamp(specPower, 1, 512);
                half spec = pow(NdotH, specPower) * (specPower + 1.0) * 0.125;

                // Fresnel (Schlick) for metallic reflections
                half3 F0 = lerp(half3(0.04, 0.04, 0.04), _BaseColor.rgb, _Metallic);
                half3 fresnel = F0 + (1.0 - F0) * pow(1.0 - NdotV, 5.0);

                // Fake environment reflection via view-normal
                half3 reflectDir = reflect(-viewDirWS, normalWS);
                half envBrightness = saturate(reflectDir.y * 0.5 + 0.5);
                half3 envReflection = lerp(
                    half3(0.02, 0.02, 0.04),
                    _ReflectionTint.rgb * 0.4,
                    envBrightness
                );

                // Additional lights
                half3 addSpec = half3(0, 0, 0);
                #ifdef _ADDITIONAL_LIGHTS
                    uint addCount = GetAdditionalLightsCount();
                    for (uint i = 0u; i < addCount; i++)
                    {
                        Light addLight = GetAdditionalLight(i, input.positionWS);
                        half3 ah = normalize(addLight.direction + viewDirWS);
                        half aNdotH = saturate(dot(normalWS, ah));
                        addSpec += addLight.color * pow(aNdotH, specPower * 0.5)
                            * addLight.distanceAttenuation * 0.5;
                    }
                #endif

                half3 diffuse = NdotL * mainLight.color * _BaseColor.rgb * (1.0 - _Metallic * 0.8);
                half3 specColor = (spec * mainLight.color + addSpec) * fresnel;
                half3 ambient = envReflection * fresnel * _Smoothness;

                half3 final = diffuse + specColor + ambient;
                return half4(final, 1.0);
            }
            ENDHLSL
        }
    }
}
```

- [ ] **Step 2: Create the dart mesh generator**

```csharp
// Assets/Scripts/BalloonGame/Visuals/DartMeshGenerator.cs
using UnityEngine;

/// <summary>
/// Generates and caches a procedural dart mesh: tapered cylinder body,
/// conical tip, and small fin flights. ~200 triangles total.
/// </summary>
public static class DartMeshGenerator
{
    private const int Segments = 8;
    private const float BodyLength = 0.5f;
    private const float BodyRadius = 0.04f;
    private const float TipLength = 0.15f;
    private const float TipRadius = 0.005f;
    private const float FlightLength = 0.12f;
    private const float FlightWidth = 0.05f;

    private static Mesh _dartMesh;

    /// <summary>The shared dart mesh. Generated on first access.</summary>
    public static Mesh DartMesh
    {
        get
        {
            if (_dartMesh == null) GenerateMesh();
            return _dartMesh;
        }
    }

    private static void GenerateMesh()
    {
        // We build the dart along the +X axis (matching DartController rotation convention).
        // Body: cylinder from x=0 to x=BodyLength
        // Tip: cone from x=BodyLength to x=BodyLength+TipLength
        // Flights: 3 flat quads at x=-FlightLength to x=0

        var builder = new MeshBuilder();

        // --- Body cylinder ---
        int bodyFront = builder.AddRing(BodyLength, BodyRadius, Segments, Vector3.right);
        int bodyBack = builder.AddRing(0f, BodyRadius, Segments, -Vector3.right);
        builder.ConnectRings(bodyFront, bodyBack, Segments);

        // --- Tip cone ---
        int tipBase = builder.AddRing(BodyLength, BodyRadius * 0.8f, Segments, Vector3.right);
        int tipPoint = builder.AddVertex(
            new Vector3(BodyLength + TipLength, 0f, 0f), Vector3.right);
        builder.ConnectRingToPoint(tipBase, tipPoint, Segments);

        // --- Back cap ---
        int backCenter = builder.AddVertex(Vector3.zero, -Vector3.right);
        builder.ConnectRingToPoint(bodyBack, backCenter, Segments, true);

        // --- Flight fins (3 fins at 120 degree intervals) ---
        for (int fin = 0; fin < 3; fin++)
        {
            float angle = fin * 120f * Mathf.Deg2Rad;
            float fy = Mathf.Cos(angle) * FlightWidth;
            float fz = Mathf.Sin(angle) * FlightWidth;

            Vector3 finNormal = new Vector3(0f, -Mathf.Sin(angle), Mathf.Cos(angle)).normalized;

            int v0 = builder.AddVertex(new Vector3(-FlightLength, 0f, 0f), finNormal);
            int v1 = builder.AddVertex(new Vector3(-FlightLength, fy, fz), finNormal);
            int v2 = builder.AddVertex(new Vector3(0f, fy * 0.5f, fz * 0.5f), finNormal);
            int v3 = builder.AddVertex(new Vector3(0f, 0f, 0f), finNormal);

            // Both sides of the fin
            builder.AddQuad(v0, v1, v2, v3);
            builder.AddQuad(v3, v2, v1, v0);
        }

        _dartMesh = builder.Build("DartMesh");
    }

    /// <summary>
    /// Simple mesh construction helper for building procedural geometry.
    /// </summary>
    private class MeshBuilder
    {
        private readonly System.Collections.Generic.List<Vector3> _verts = new();
        private readonly System.Collections.Generic.List<Vector3> _normals = new();
        private readonly System.Collections.Generic.List<Vector2> _uvs = new();
        private readonly System.Collections.Generic.List<int> _tris = new();

        public int AddVertex(Vector3 pos, Vector3 normal)
        {
            int idx = _verts.Count;
            _verts.Add(pos);
            _normals.Add(normal);
            _uvs.Add(new Vector2(pos.x / (BodyLength + TipLength), 0.5f));
            return idx;
        }

        public int AddRing(float x, float radius, int segments, Vector3 faceNormal)
        {
            int startIdx = _verts.Count;
            for (int i = 0; i < segments; i++)
            {
                float theta = 2f * Mathf.PI * i / segments;
                float y = Mathf.Cos(theta) * radius;
                float z = Mathf.Sin(theta) * radius;
                Vector3 radialNormal = new Vector3(0f, Mathf.Cos(theta), Mathf.Sin(theta));
                _verts.Add(new Vector3(x, y, z));
                _normals.Add((radialNormal + faceNormal * 0.3f).normalized);
                _uvs.Add(new Vector2(x / (BodyLength + TipLength), (float)i / segments));
            }
            return startIdx;
        }

        public void ConnectRings(int ringA, int ringB, int segments)
        {
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                _tris.Add(ringA + i);
                _tris.Add(ringA + next);
                _tris.Add(ringB + i);

                _tris.Add(ringA + next);
                _tris.Add(ringB + next);
                _tris.Add(ringB + i);
            }
        }

        public void ConnectRingToPoint(int ring, int point, int segments, bool flip = false)
        {
            for (int i = 0; i < segments; i++)
            {
                int next = (i + 1) % segments;
                if (flip)
                {
                    _tris.Add(ring + next);
                    _tris.Add(ring + i);
                    _tris.Add(point);
                }
                else
                {
                    _tris.Add(ring + i);
                    _tris.Add(ring + next);
                    _tris.Add(point);
                }
            }
        }

        public void AddQuad(int a, int b, int c, int d)
        {
            _tris.Add(a); _tris.Add(b); _tris.Add(c);
            _tris.Add(a); _tris.Add(c); _tris.Add(d);
        }

        public Mesh Build(string name)
        {
            var mesh = new Mesh { name = name };
            mesh.SetVertices(_verts);
            mesh.SetNormals(_normals);
            mesh.SetUVs(0, _uvs);
            mesh.SetTriangles(_tris, 0);
            mesh.RecalculateBounds();
            mesh.RecalculateTangents();
            return mesh;
        }
    }
}
```

**Commit:** `git commit -m "feat(visuals): add ChromeDart shader and DartMeshGenerator procedural mesh"`

---

## Task 4: Update GameConstants & BalloonData for Visual System

**Files:**
- Modify: `Assets/Scripts/BalloonGame/GameConstants.cs`
- Modify: `Assets/Scripts/BalloonGame/BalloonData.cs`

Add constants for the new visual system and extend `BalloonColor` with special types.

- [ ] **Step 1: Add visual constants to GameConstants.cs**

Add the following block at the end of `GameConstants.cs`, before the closing brace:

```csharp
    // --- Visual Constants (Plan 02) ---
    public const float BALLOON_SHADER_GLOSSINESS = 0.85f;
    public const float BALLOON_SHADER_RIM_POWER = 2.5f;
    public const float BALLOON_SHADER_RIM_INTENSITY = 0.6f;
    public const float BALLOON_SHADER_GRADIENT = 0.15f;
    public const float BALLOON_SHADER_SPEC_INTENSITY = 1.2f;
    public const float BALLOON_SHADER_SPEC_SIZE = 64f;
    public const float BALLOON_SHADER_SSS_INTENSITY = 0.15f;
    public const float BALLOON_SHADER_FRESNEL_INTENSITY = 0.08f;
    public const float BALLOON_SHADER_AMBIENT_BOOST = 0.12f;

    public const float GOLD_GLOSSINESS = 0.95f;
    public const float GOLD_SPEC_INTENSITY = 2.0f;
    public const float GOLD_SPEC_SIZE = 128f;

    public const float HAZARD_RIM_INTENSITY = 1.2f;
    public const float HAZARD_FRESNEL_INTENSITY = 0.25f;

    // Stuck dart settings
    public const int MAX_STUCK_DARTS = 12;
    public const float STUCK_DART_Z_OFFSET = 0.8f;

    // Atmosphere
    public const int DUST_PARTICLE_COUNT = 30;
    public const int SMOKE_PARTICLE_COUNT = 8;

    // Camera (Plan 02 perspective switch)
    public const float CAMERA_FOV = 18f;
    public const float CAMERA_Z_POSITION = -32f;

    // Neon light positions
    public static readonly Vector3 NEON_PINK_POSITION = new(-3f, 4f, -2f);
    public static readonly Vector3 NEON_BLUE_POSITION = new(3f, 6f, -2f);
```

- [ ] **Step 2: Extend BalloonColor enum and color table in BalloonData.cs**

Replace the full contents of `BalloonData.cs` with:

```csharp
// Assets/Scripts/BalloonGame/BalloonData.cs
using UnityEngine;

public enum BalloonColor
{
    Red,
    Blue,
    Yellow,
    Green,
    Purple,
    // Special types (Plan 02)
    Gold,
    Paint,
    Prize,
    Hazard
}

/// <summary>
/// Extension methods mapping BalloonColor to visual properties.
/// Standard colors return base balloon color. Special types return their distinct colors.
/// </summary>
public static class BalloonColorExtensions
{
    /// <summary>Returns the balloon body color for the given BalloonColor.</summary>
    public static Color ToUnityColor(this BalloonColor color)
    {
        return color switch
        {
            BalloonColor.Red    => new Color(0.902f, 0.224f, 0.275f),  // #E63946
            BalloonColor.Blue   => new Color(0.271f, 0.482f, 0.616f),  // #457B9D
            BalloonColor.Yellow => new Color(0.945f, 0.890f, 0.533f),  // #F1E388
            BalloonColor.Green  => new Color(0.165f, 0.616f, 0.561f),  // #2A9D8F
            BalloonColor.Purple => new Color(0.608f, 0.349f, 0.714f),  // #9B59B6
            BalloonColor.Gold   => new Color(1.000f, 0.843f, 0.000f),  // #FFD700
            BalloonColor.Paint  => new Color(0.878f, 0.165f, 0.533f),  // #E02A88
            BalloonColor.Prize  => new Color(0.580f, 0.000f, 0.827f),  // #9400D3
            BalloonColor.Hazard => new Color(0.180f, 0.180f, 0.200f),  // #2E2E33
            _ => Color.white
        };
    }

    /// <summary>Returns true if this is a special balloon type with unique visual treatment.</summary>
    public static bool IsSpecial(this BalloonColor color)
    {
        return color is BalloonColor.Gold or BalloonColor.Paint
            or BalloonColor.Prize or BalloonColor.Hazard;
    }

    /// <summary>
    /// Returns the secondary accent color for special balloons.
    /// Used for rim glow, fresnel, or emblem tinting.
    /// </summary>
    public static Color GetAccentColor(this BalloonColor color)
    {
        return color switch
        {
            BalloonColor.Gold   => new Color(1.0f, 0.95f, 0.6f),   // warm gold glow
            BalloonColor.Paint  => new Color(0.2f, 1.0f, 0.4f),    // neon green splat accent
            BalloonColor.Prize  => new Color(0.9f, 0.7f, 1.0f),    // lavender shimmer
            BalloonColor.Hazard => new Color(1.0f, 0.15f, 0.1f),   // danger red glow
            _ => Color.white
        };
    }
}
```

Note: `BalloonColor.Yellow` has been adjusted from `(0.945, 0.980, 0.933)` (nearly white) to `(0.945, 0.890, 0.533)` (actual yellow) to fix the existing color being indistinguishable from white.

**Commit:** `git commit -m "feat(data): extend BalloonColor with special types, add visual constants"`

---

## Task 5: Update BalloonWall to Use New Mesh & Shader

**Files:**
- Modify: `Assets/Scripts/BalloonGame/BalloonWall.cs`
- Modify: `Assets/Scripts/BalloonGame/BalloonNode.cs`

Replace `CreatePrimitive(Sphere)` with the procedural balloon mesh, use the custom `BalloonLit` shader, and apply per-instance color via `MaterialPropertyBlock` instead of per-balloon material instances.

- [ ] **Step 1: Rewrite BalloonWall.cs**

Replace the full contents of `BalloonWall.cs`:

```csharp
// Assets/Scripts/BalloonGame/BalloonWall.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generates and manages the balloon grid. Uses procedural BalloonMeshGenerator meshes
/// and MaterialPropertyBlock for per-instance coloring (SRP Batcher friendly).
/// </summary>
public class BalloonWall : MonoBehaviour
{
    private readonly List<BalloonNode> _balloons = new();
    private readonly BalloonColor[] _standardColors =
    {
        BalloonColor.Red,
        BalloonColor.Blue,
        BalloonColor.Yellow,
        BalloonColor.Green,
        BalloonColor.Purple
    };

    private static readonly int BaseColorId = Shader.PropertyToID("_BaseColor");
    private static readonly int RimColorId = Shader.PropertyToID("_RimColor");
    private static readonly int RimIntensityId = Shader.PropertyToID("_RimIntensity");
    private static readonly int GlossinessId = Shader.PropertyToID("_Glossiness");
    private static readonly int SpecularIntensityId = Shader.PropertyToID("_SpecularIntensity");
    private static readonly int SpecularSizeId = Shader.PropertyToID("_SpecularSize");
    private static readonly int FresnelColorId = Shader.PropertyToID("_FresnelColor");
    private static readonly int FresnelIntensityId = Shader.PropertyToID("_FresnelIntensity");

    private Material _balloonMaterial;

    /// <summary>Read-only access to all balloon nodes in the wall.</summary>
    public IReadOnlyList<BalloonNode> Balloons => _balloons;

    private void Awake()
    {
        BalloonMeshGenerator.EnsureMeshes();
        EnsureMaterial();
    }

    /// <summary>Generates the full balloon grid with perspective scaling.</summary>
    [ContextMenu("Generate Wall")]
    public void GenerateWall()
    {
        ClearWall();
        EnsureMaterial();

        int rows = GameConstants.BOARD_ROWS;
        int columns = GameConstants.BOARD_COLUMNS;
        float slotX = GameConstants.BOARD_WIDTH / columns;
        float slotY = GameConstants.BOARD_HEIGHT / rows;

        for (int row = 0; row < rows; row++)
        {
            for (int column = 0; column < columns; column++)
            {
                BalloonColor color = _standardColors[Random.Range(0, _standardColors.Length)];
                float scale = PerspectiveScale(row, rows);
                Vector3 position = PerspectivePosition(row, column, rows, columns, slotX, slotY);

                GameObject balloon = CreateBalloonObject(color, position, scale, row, rows);
                balloon.name = $"Balloon_{row}_{column}";
                balloon.transform.SetParent(transform);

                var node = balloon.AddComponent<BalloonNode>();
                node.Initialize(row, column, color);
                _balloons.Add(node);
            }
        }
    }

    /// <summary>Destroys all balloon child objects and clears the internal list.</summary>
    [ContextMenu("Clear Wall")]
    public void ClearWall()
    {
        if (_balloons.Count == 0 && transform.childCount > 0)
        {
            for (int childIndex = transform.childCount - 1; childIndex >= 0; childIndex--)
            {
                GameObject child = transform.GetChild(childIndex).gameObject;
                if (Application.isPlaying) Destroy(child);
                else DestroyImmediate(child);
            }
        }

        for (int index = _balloons.Count - 1; index >= 0; index--)
        {
            BalloonNode balloon = _balloons[index];
            if (balloon == null) continue;
            if (Application.isPlaying) Destroy(balloon.gameObject);
            else DestroyImmediate(balloon.gameObject);
        }

        _balloons.Clear();
    }

    /// <summary>Returns the number of non-popped balloons remaining.</summary>
    public int ActiveCount()
    {
        int count = 0;
        foreach (BalloonNode balloon in _balloons)
        {
            if (balloon != null && !balloon.IsPopped) count++;
        }
        return count;
    }

    private GameObject CreateBalloonObject(BalloonColor color, Vector3 localPos, float scale, int row, int totalRows)
    {
        var go = new GameObject();
        go.layer = GameConstants.LAYER_BALLOONS;
        go.transform.localPosition = localPos;

        float width = GameConstants.BALLOON_MAX_WIDTH * GameConstants.BALLOON_SLOT_RATIO_X * scale;
        float height = GameConstants.BALLOON_MAX_HEIGHT * GameConstants.BALLOON_SLOT_RATIO_Y * scale;
        go.transform.localScale = new Vector3(width, height, width);

        // Mesh
        bool isHazard = color == BalloonColor.Hazard;
        var meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = isHazard ? BalloonMeshGenerator.HazardMesh : BalloonMeshGenerator.StandardMesh;

        // Renderer with shared material + per-instance color
        var renderer = go.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = _balloonMaterial;
        ApplyBalloonColor(renderer, color, row, totalRows);

        // Physics
        var collider = go.AddComponent<SphereCollider>();
        collider.radius = 0.5f;
        collider.center = Vector3.zero;

        var rb = go.AddComponent<Rigidbody>();
        rb.isKinematic = true;
        rb.useGravity = false;

        return go;
    }

    private void ApplyBalloonColor(Renderer renderer, BalloonColor color, int row, int totalRows)
    {
        var mpb = new MaterialPropertyBlock();

        // Base color with atmospheric fade for depth
        Color baseColor = color.ToUnityColor();
        float rowRatio = (float)row / (totalRows - 1);
        float topBias = 1f - rowRatio;
        float fade = 1f - topBias * 0.12f;
        Color fadedColor = Color.Lerp(
            new Color(baseColor.r * 0.6f, baseColor.g * 0.6f, baseColor.b * 0.6f),
            baseColor,
            fade
        );
        mpb.SetColor(BaseColorId, fadedColor);

        // Special balloon overrides
        switch (color)
        {
            case BalloonColor.Gold:
                mpb.SetFloat(GlossinessId, GameConstants.GOLD_GLOSSINESS);
                mpb.SetFloat(SpecularIntensityId, GameConstants.GOLD_SPEC_INTENSITY);
                mpb.SetFloat(SpecularSizeId, GameConstants.GOLD_SPEC_SIZE);
                mpb.SetColor(RimColorId, color.GetAccentColor());
                break;

            case BalloonColor.Hazard:
                mpb.SetFloat(RimIntensityId, GameConstants.HAZARD_RIM_INTENSITY);
                mpb.SetColor(RimColorId, color.GetAccentColor());
                mpb.SetColor(FresnelColorId, new Color(1f, 0.1f, 0.05f, 1f));
                mpb.SetFloat(FresnelIntensityId, GameConstants.HAZARD_FRESNEL_INTENSITY);
                break;

            case BalloonColor.Paint:
                mpb.SetColor(RimColorId, color.GetAccentColor());
                mpb.SetFloat(GlossinessId, 0.92f);
                break;

            case BalloonColor.Prize:
                mpb.SetColor(FresnelColorId, color.GetAccentColor());
                mpb.SetFloat(FresnelIntensityId, 0.18f);
                mpb.SetColor(RimColorId, new Color(1f, 0.85f, 1f, 1f));
                break;
        }

        renderer.SetPropertyBlock(mpb);
    }

    private Vector3 PerspectivePosition(int row, int column, int totalRows, int totalColumns, float slotX, float slotY)
    {
        float rowRatio = (float)row / (totalRows - 1);
        float topBias = 1f - rowRatio;

        float baseX = GameConstants.BOARD_LEFT + slotX * 0.5f + column * slotX;
        float centerX = (GameConstants.BOARD_LEFT + GameConstants.BOARD_RIGHT) * 0.5f;
        float pinchedX = centerX + (baseX - centerX) * (1f - topBias * GameConstants.PERSPECTIVE_HORIZONTAL_PINCH);
        float compressedY = GameConstants.BOARD_TOP - slotY * 0.5f - (row * slotY * (1f - topBias * GameConstants.PERSPECTIVE_VERTICAL_COMPRESSION));

        float depthOffset = (totalRows - 1 - row) * 0.01f;
        return new Vector3(pinchedX, compressedY, depthOffset);
    }

    private float PerspectiveScale(int row, int totalRows)
    {
        float rowRatio = (float)row / (totalRows - 1);
        return GameConstants.PERSPECTIVE_MIN_SCALE + rowRatio * GameConstants.PERSPECTIVE_SCALE_RANGE;
    }

    private void EnsureMaterial()
    {
        if (_balloonMaterial != null) return;

        Shader shader = Shader.Find("Inkshot/BalloonLit");
        if (shader == null)
        {
            Debug.LogWarning("BalloonLit shader not found, falling back to URP/Lit");
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        _balloonMaterial = new Material(shader);
        _balloonMaterial.SetFloat("_Glossiness", GameConstants.BALLOON_SHADER_GLOSSINESS);
        _balloonMaterial.SetFloat("_RimPower", GameConstants.BALLOON_SHADER_RIM_POWER);
        _balloonMaterial.SetColor("_RimColor", Color.white);
        _balloonMaterial.SetFloat("_RimIntensity", GameConstants.BALLOON_SHADER_RIM_INTENSITY);
        _balloonMaterial.SetFloat("_GradientStrength", GameConstants.BALLOON_SHADER_GRADIENT);
        _balloonMaterial.SetFloat("_SpecularIntensity", GameConstants.BALLOON_SHADER_SPEC_INTENSITY);
        _balloonMaterial.SetFloat("_SpecularSize", GameConstants.BALLOON_SHADER_SPEC_SIZE);
        _balloonMaterial.SetColor("_SSSColor", new Color(1f, 0.4f, 0.31f, 1f));
        _balloonMaterial.SetFloat("_SSSIntensity", GameConstants.BALLOON_SHADER_SSS_INTENSITY);
        _balloonMaterial.SetColor("_FresnelColor", new Color(0.5f, 0.2f, 1f, 1f));
        _balloonMaterial.SetFloat("_FresnelIntensity", GameConstants.BALLOON_SHADER_FRESNEL_INTENSITY);
        _balloonMaterial.SetFloat("_AmbientBoost", GameConstants.BALLOON_SHADER_AMBIENT_BOOST);
        _balloonMaterial.enableInstancing = true;
    }
}
```

- [ ] **Step 2: Update BalloonNode.cs to store renderer reference for MaterialPropertyBlock access**

Replace the full contents of `BalloonNode.cs`:

```csharp
// Assets/Scripts/BalloonGame/BalloonNode.cs
using System;
using UnityEngine;

/// <summary>
/// Represents a single balloon in the wall grid. Handles collision detection
/// and pop state. Stores a renderer reference for visual updates via MaterialPropertyBlock.
/// </summary>
[RequireComponent(typeof(SphereCollider))]
[DisallowMultipleComponent]
public class BalloonNode : MonoBehaviour
{
    /// <summary>Fired when any balloon is popped. Payload is the popped node.</summary>
    public static event Action<BalloonNode> OnAnyBalloonPopped;

    /// <summary>Row index in the balloon grid (0 = top).</summary>
    public int Row { get; private set; }

    /// <summary>Column index in the balloon grid (0 = left).</summary>
    public int Column { get; private set; }

    /// <summary>The color/type of this balloon.</summary>
    public BalloonColor BalloonColor { get; private set; }

    /// <summary>Whether this balloon has been popped.</summary>
    public bool IsPopped { get; private set; }

    /// <summary>Cached renderer for MaterialPropertyBlock access.</summary>
    public Renderer Renderer { get; private set; }

    /// <summary>Initialize the balloon with its grid position and color.</summary>
    public void Initialize(int row, int column, BalloonColor color)
    {
        Row = row;
        Column = column;
        BalloonColor = color;
        IsPopped = false;
        Renderer = GetComponent<Renderer>();
        gameObject.layer = GameConstants.LAYER_BALLOONS;
    }

    /// <summary>Pop this balloon, firing the event and deactivating the GameObject.</summary>
    public void Pop()
    {
        if (IsPopped) return;

        IsPopped = true;
        OnAnyBalloonPopped?.Invoke(this);
        gameObject.SetActive(false);
    }

    private void OnCollisionEnter(Collision collision)
    {
        GameObject hitObject = collision.rigidbody != null ? collision.rigidbody.gameObject : collision.gameObject;
        if (hitObject.layer != GameConstants.LAYER_PROJECTILES) return;

        Pop();

        var dartController = hitObject.GetComponent<DartController>();
        dartController?.OnHitBalloon();
    }
}
```

**Commit:** `git commit -m "feat(visuals): integrate BalloonLit shader and procedural mesh into BalloonWall"`

---

## Task 6: Update DartLauncher & DartController for New Dart Visuals

**Files:**
- Modify: `Assets/Scripts/BalloonGame/DartLauncher.cs`
- Modify: `Assets/Scripts/BalloonGame/DartController.cs`

Replace primitive dart construction with procedural mesh and chrome shader. Add support for stuck darts.

- [ ] **Step 1: Rewrite DartLauncher.cs**

Replace the full contents of `DartLauncher.cs`:

```csharp
// Assets/Scripts/BalloonGame/DartLauncher.cs
using UnityEngine;

/// <summary>
/// Spawns dart GameObjects using the procedural DartMeshGenerator mesh
/// and ChromeDart shader. Manages dart material caching.
/// </summary>
public class DartLauncher : MonoBehaviour
{
    private static Material _chromeMaterial;

    /// <summary>Spawn a dart at the launch position and launch it with the given velocity.</summary>
    public DartController SpawnAndLaunch(Vector3 velocity)
    {
        GameObject dart = CreateDartObject();
        dart.transform.position = GameConstants.LAUNCH_POSITION;

        var controller = dart.GetComponent<DartController>();
        controller.Launch(velocity);
        return controller;
    }

    private GameObject CreateDartObject()
    {
        var root = new GameObject("Dart");
        root.layer = GameConstants.LAYER_PROJECTILES;

        // Visual mesh child
        var visual = new GameObject("Visual");
        visual.transform.SetParent(root.transform, false);
        visual.layer = GameConstants.LAYER_PROJECTILES;

        var meshFilter = visual.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = DartMeshGenerator.DartMesh;

        var meshRenderer = visual.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = GetChromeMaterial();

        // Collider on root
        var collider = root.AddComponent<CapsuleCollider>();
        collider.direction = 0; // X-axis
        collider.center = new Vector3(0.25f, 0f, 0f);
        collider.radius = 0.05f;
        collider.height = 0.65f;

        // Rigidbody
        var rigidbody = root.AddComponent<Rigidbody>();
        rigidbody.mass = 0.5f;
        rigidbody.linearDamping = 0.1f;
        rigidbody.angularDamping = 0.5f;
        rigidbody.useGravity = false;
        rigidbody.isKinematic = true;
        rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        rigidbody.constraints = RigidbodyConstraints.FreezePositionZ |
                                 RigidbodyConstraints.FreezeRotationX |
                                 RigidbodyConstraints.FreezeRotationY;

        root.AddComponent<DartController>();
        return root;
    }

    private static Material GetChromeMaterial()
    {
        if (_chromeMaterial != null) return _chromeMaterial;

        Shader shader = Shader.Find("Inkshot/ChromeDart");
        if (shader == null)
        {
            Debug.LogWarning("ChromeDart shader not found, falling back to URP/Lit");
            shader = Shader.Find("Universal Render Pipeline/Lit");
        }

        _chromeMaterial = new Material(shader);
        _chromeMaterial.SetColor("_BaseColor", new Color(0.78f, 0.82f, 0.88f, 1f)); // #C7D1E0 blue-chrome
        _chromeMaterial.SetFloat("_Metallic", 1.0f);
        _chromeMaterial.SetFloat("_Smoothness", 0.9f);
        _chromeMaterial.SetColor("_ReflectionTint", new Color(0.7f, 0.75f, 0.85f, 1f));

        return _chromeMaterial;
    }
}
```

- [ ] **Step 2: Update DartController.cs for stuck dart support**

Replace the full contents of `DartController.cs`:

```csharp
// Assets/Scripts/BalloonGame/DartController.cs
using UnityEngine;

/// <summary>
/// Controls dart flight, rotation, collision, and stuck-in-wall behavior.
/// When a dart hits the back wall or top wall, it notifies StuckDartManager
/// instead of immediately deactivating.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
[DisallowMultipleComponent]
public class DartController : MonoBehaviour
{
    public enum DartState
    {
        Ready,
        Flying,
        Stuck,
        Stopped
    }

    /// <summary>Fired when a dart finishes its flight (stopped or stuck).</summary>
    public static event System.Action<DartController> OnDartFinished;

    /// <summary>Fired when a dart embeds in the wall, providing position for StuckDartManager.</summary>
    public static event System.Action<Vector3, Quaternion> OnDartStuck;

    /// <summary>Current state of this dart.</summary>
    public DartState State { get; private set; } = DartState.Ready;

    private const float MaxLifetime = 5f;

    private Rigidbody _rigidbody;
    private float _lifetime;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;
        _rigidbody.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;
        gameObject.layer = GameConstants.LAYER_PROJECTILES;
    }

    /// <summary>Launch the dart with the given velocity vector.</summary>
    public void Launch(Vector3 velocity)
    {
        CancelInvoke(nameof(Deactivate));
        State = DartState.Flying;
        _rigidbody.isKinematic = false;
        _rigidbody.useGravity = true;
        _rigidbody.linearVelocity = velocity;
        _lifetime = 0f;
    }

    private void FixedUpdate()
    {
        if (State != DartState.Flying) return;

        _lifetime += Time.fixedDeltaTime;

        if (_rigidbody.linearVelocity.sqrMagnitude > 0.5f)
        {
            float angle = Mathf.Atan2(_rigidbody.linearVelocity.y, _rigidbody.linearVelocity.x) * Mathf.Rad2Deg;
            _rigidbody.MoveRotation(Quaternion.Euler(0f, 0f, angle));
        }

        if (_lifetime > MaxLifetime)
        {
            StopDart("timeout");
            return;
        }

        Vector3 position = transform.position;
        if (position.y < GameConstants.LANE_BOTTOM - 2f ||
            position.x < GameConstants.BOARD_LEFT - 5f ||
            position.x > GameConstants.BOARD_RIGHT + 5f ||
            position.y > GameConstants.BOARD_TOP + 5f)
        {
            StopDart("out_of_bounds");
        }
    }

    /// <summary>Called by BalloonNode when a dart hits a balloon.</summary>
    public void OnHitBalloon()
    {
        StopDart("balloon");
    }

    private void OnCollisionEnter(Collision collision)
    {
        HandleCollision(collision);
    }

    private void StopDart(string reason)
    {
        if (State == DartState.Stopped || State == DartState.Stuck) return;

        bool shouldStick = reason == "top_wall" || reason == "balloon";

        if (shouldStick)
        {
            State = DartState.Stuck;
            OnDartStuck?.Invoke(transform.position, transform.rotation);
        }
        else
        {
            State = DartState.Stopped;
        }

        _rigidbody.linearVelocity = Vector3.zero;
        _rigidbody.angularVelocity = Vector3.zero;
        _rigidbody.useGravity = false;
        _rigidbody.isKinematic = true;

        OnDartFinished?.Invoke(this);
        Invoke(nameof(Deactivate), 0.3f);
    }

    private void Deactivate()
    {
        gameObject.SetActive(false);
    }

    private void HandleCollision(Collision collision)
    {
        string hitName = collision.collider != null ? collision.collider.name : collision.gameObject.name;
        if (hitName == "LeftWall" || hitName == "RightWall")
        {
            return; // bounce
        }

        if (hitName == "TopWall")
        {
            StopDart("top_wall");
        }
    }
}
```

**Commit:** `git commit -m "feat(visuals): upgrade DartLauncher with procedural mesh + chrome material, add stuck dart events"`

---

## Task 7: Stuck Dart Manager

**Files:**
- Create: `Assets/Scripts/BalloonGame/Visuals/StuckDartManager.cs`

Manages a pool of visual-only stuck dart decorations on the back wall.

- [ ] **Step 1: Create StuckDartManager.cs**

```csharp
// Assets/Scripts/BalloonGame/Visuals/StuckDartManager.cs
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages visual-only stuck darts embedded in the back wall.
/// Listens to DartController.OnDartStuck events and places a dart mesh
/// at the impact position. Limited to MAX_STUCK_DARTS to control draw calls.
/// Old darts are recycled (oldest removed first).
/// </summary>
[DisallowMultipleComponent]
public class StuckDartManager : MonoBehaviour
{
    private readonly Queue<GameObject> _stuckDarts = new();
    private Material _chromeMaterial;

    private void OnEnable()
    {
        DartController.OnDartStuck += HandleDartStuck;
    }

    private void OnDisable()
    {
        DartController.OnDartStuck -= HandleDartStuck;
    }

    /// <summary>Clears all stuck darts (called on room restart).</summary>
    public void ClearAll()
    {
        while (_stuckDarts.Count > 0)
        {
            GameObject dart = _stuckDarts.Dequeue();
            if (dart != null) Destroy(dart);
        }
    }

    private void HandleDartStuck(Vector3 position, Quaternion rotation)
    {
        // Recycle oldest if at limit
        if (_stuckDarts.Count >= GameConstants.MAX_STUCK_DARTS)
        {
            GameObject oldest = _stuckDarts.Dequeue();
            if (oldest != null) Destroy(oldest);
        }

        // Create a visual-only stuck dart (no collider, no rigidbody)
        var stuckDart = new GameObject("StuckDart");
        stuckDart.transform.SetParent(transform);
        stuckDart.transform.position = new Vector3(
            position.x,
            position.y,
            GameConstants.STUCK_DART_Z_OFFSET
        );
        stuckDart.transform.rotation = rotation;

        var meshFilter = stuckDart.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = DartMeshGenerator.DartMesh;

        var meshRenderer = stuckDart.AddComponent<MeshRenderer>();
        meshRenderer.sharedMaterial = GetChromeMaterial();

        _stuckDarts.Enqueue(stuckDart);
    }

    private Material GetChromeMaterial()
    {
        if (_chromeMaterial != null) return _chromeMaterial;

        Shader shader = Shader.Find("Inkshot/ChromeDart");
        if (shader == null) shader = Shader.Find("Universal Render Pipeline/Lit");

        _chromeMaterial = new Material(shader);
        _chromeMaterial.SetColor("_BaseColor", new Color(0.65f, 0.68f, 0.73f, 1f)); // slightly darker chrome for stuck
        _chromeMaterial.SetFloat("_Metallic", 1.0f);
        _chromeMaterial.SetFloat("_Smoothness", 0.75f); // less shiny, been embedded
        _chromeMaterial.SetColor("_ReflectionTint", new Color(0.5f, 0.55f, 0.65f, 1f));

        return _chromeMaterial;
    }
}
```

**Commit:** `git commit -m "feat(visuals): add StuckDartManager for wall-embedded dart decorations"`

---

## Task 8: Environment Builder (Back Wall, Frame, Lane)

**Files:**
- Create: `Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs`

Runtime component that enhances the existing environment primitives with noir materials and adds depth layering.

- [ ] **Step 1: Create EnvironmentBuilder.cs**

```csharp
// Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs
using UnityEngine;

/// <summary>
/// Enhances the existing scene environment objects with neon noir materials.
/// Finds BackWall, LeftWall, RightWall, TopWall, LaneFloor by name and
/// applies URP/Lit materials with tuned properties. Also adds depth layering
/// objects (shadow panels, floor reflections, lane markings).
/// Attach to a root "Environment" GameObject in the scene.
/// </summary>
[DisallowMultipleComponent]
public class EnvironmentBuilder : MonoBehaviour
{
    private void Start()
    {
        ApplyBackWallMaterial();
        ApplyFrameMaterials();
        ApplyLaneMaterial();
        CreateLaneMarkings();
        CreateDepthPanel();
    }

    private void ApplyBackWallMaterial()
    {
        var backWall = GameObject.Find("BackWall");
        if (backWall == null) return;

        var renderer = backWall.GetComponent<Renderer>();
        if (renderer == null) return;

        // Dark corkboard look: very dark brown, low smoothness (matte cork)
        Material mat = CreateURPLitMaterial(
            "BackWallNoir",
            new Color(0.10f, 0.08f, 0.06f, 1f),  // near-black warm brown
            smoothness: 0.05f,
            metallic: 0f
        );
        renderer.sharedMaterial = mat;
    }

    private void ApplyFrameMaterials()
    {
        string[] frameNames = { "LeftWall", "RightWall", "TopWall" };

        // Dark wood with subtle sheen: very dark, slight smoothness for worn varnish
        Material frameMat = CreateURPLitMaterial(
            "WallFrameNoir",
            new Color(0.07f, 0.05f, 0.04f, 1f),  // charcoal-black wood
            smoothness: 0.2f,
            metallic: 0.05f
        );

        foreach (string name in frameNames)
        {
            var obj = GameObject.Find(name);
            if (obj == null) continue;
            var renderer = obj.GetComponent<Renderer>();
            if (renderer != null) renderer.sharedMaterial = frameMat;
        }
    }

    private void ApplyLaneMaterial()
    {
        var laneFloor = GameObject.Find("LaneFloor");
        if (laneFloor == null) return;

        var renderer = laneFloor.GetComponent<Renderer>();
        if (renderer == null) return;

        // Wet floor: dark with high smoothness for reflections
        Material mat = CreateURPLitMaterial(
            "LaneFloorWet",
            new Color(0.04f, 0.04f, 0.05f, 1f),  // near-black with blue tint
            smoothness: 0.7f,
            metallic: 0.1f
        );
        renderer.sharedMaterial = mat;
    }

    private void CreateLaneMarkings()
    {
        float laneCenterY = (GameConstants.LANE_TOP + GameConstants.LANE_BOTTOM) * 0.5f;
        float laneHeight = GameConstants.LANE_TOP - GameConstants.LANE_BOTTOM;

        Material lineMat = CreateURPLitMaterial(
            "LaneLine",
            new Color(0.12f, 0.10f, 0.08f, 1f),  // faded paint line
            smoothness: 0.3f,
            metallic: 0f
        );

        // Left lane line
        CreateLaneLine("LaneLineLeft", new Vector3(-1.5f, laneCenterY, 0.99f),
            new Vector3(0.04f, laneHeight * 0.7f, 1f), lineMat);

        // Right lane line
        CreateLaneLine("LaneLineRight", new Vector3(1.5f, laneCenterY, 0.99f),
            new Vector3(0.04f, laneHeight * 0.7f, 1f), lineMat);

        // Center foul line
        Material foulMat = CreateURPLitMaterial(
            "FoulLine",
            new Color(0.25f, 0.08f, 0.06f, 1f),  // faded red
            smoothness: 0.2f,
            metallic: 0f
        );
        CreateLaneLine("FoulLine", new Vector3(0f, GameConstants.LANE_TOP - 0.2f, 0.98f),
            new Vector3(3.5f, 0.03f, 1f), foulMat);
    }

    private void CreateLaneLine(string name, Vector3 position, Vector3 scale, Material material)
    {
        var line = GameObject.CreatePrimitive(PrimitiveType.Quad);
        line.name = name;
        line.transform.SetParent(transform);
        line.transform.position = position;
        line.transform.localScale = scale;
        line.layer = GameConstants.LAYER_ENVIRONMENT;
        line.GetComponent<Renderer>().sharedMaterial = material;
        Object.Destroy(line.GetComponent<MeshCollider>());
    }

    private void CreateDepthPanel()
    {
        // Dark void panel behind the back wall for depth
        var depthPanel = GameObject.CreatePrimitive(PrimitiveType.Quad);
        depthPanel.name = "DepthVoid";
        depthPanel.transform.SetParent(transform);
        depthPanel.transform.position = new Vector3(0f, 0f, 3f);
        depthPanel.transform.localScale = new Vector3(20f, 25f, 1f);
        depthPanel.layer = GameConstants.LAYER_ENVIRONMENT;
        Object.Destroy(depthPanel.GetComponent<MeshCollider>());

        Material voidMat = CreateURPLitMaterial(
            "DepthVoid",
            new Color(0.01f, 0.01f, 0.02f, 1f),  // pure void
            smoothness: 0f,
            metallic: 0f
        );
        depthPanel.GetComponent<Renderer>().sharedMaterial = voidMat;
    }

    private static Material CreateURPLitMaterial(string name, Color color, float smoothness, float metallic)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null)
        {
            Debug.LogWarning($"URP/Lit shader not found for {name}, falling back to Standard");
            shader = Shader.Find("Standard");
        }

        var mat = new Material(shader) { name = name, color = color };

        if (mat.HasProperty("_Smoothness"))
            mat.SetFloat("_Smoothness", smoothness);
        if (mat.HasProperty("_Glossiness"))
            mat.SetFloat("_Glossiness", smoothness);
        if (mat.HasProperty("_Metallic"))
            mat.SetFloat("_Metallic", metallic);

        return mat;
    }
}
```

**Commit:** `git commit -m "feat(visuals): add EnvironmentBuilder with noir materials and lane markings"`

---

## Task 9: Neon Light Rig & Atmospheric Effects

**Files:**
- Create: `Assets/Scripts/BalloonGame/Visuals/NeonLightRig.cs`
- Create: `Assets/Scripts/BalloonGame/Visuals/AtmosphereController.cs`

Sets up the moody lighting and particle-based atmosphere.

- [ ] **Step 1: Create NeonLightRig.cs**

```csharp
// Assets/Scripts/BalloonGame/Visuals/NeonLightRig.cs
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Creates and manages the neon noir lighting setup at runtime.
/// Configures the main directional light plus colored neon point lights.
/// Attach to a root "LightRig" GameObject.
/// </summary>
[DisallowMultipleComponent]
public class NeonLightRig : MonoBehaviour
{
    private Light _mainLight;
    private Light _neonPink;
    private Light _neonBlue;

    private void Awake()
    {
        SetupMainLight();
        SetupNeonLights();
        SetupAmbient();
    }

    private void SetupMainLight()
    {
        // Find existing directional light or create one
        var existingLight = FindAnyObjectByType<Light>();
        if (existingLight != null && existingLight.type == LightType.Directional)
        {
            _mainLight = existingLight;
        }
        else
        {
            var lightObj = new GameObject("DirectionalLight_Noir");
            lightObj.transform.SetParent(transform);
            _mainLight = lightObj.AddComponent<Light>();
            _mainLight.type = LightType.Directional;
        }

        _mainLight.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        _mainLight.color = HexColor("FFF5E6");       // warm white
        _mainLight.intensity = 0.8f;
        _mainLight.shadows = LightShadows.None;       // mobile: no real-time shadows from directional
        _mainLight.renderMode = LightRenderMode.Auto;
    }

    private void SetupNeonLights()
    {
        // Neon Pink
        _neonPink = CreatePointLight(
            "NeonPink",
            GameConstants.NEON_PINK_POSITION,
            HexColor("FF1493"),   // deep pink
            2.0f,
            8f
        );

        // Neon Blue
        _neonBlue = CreatePointLight(
            "NeonBlue",
            GameConstants.NEON_BLUE_POSITION,
            HexColor("00BFFF"),   // deep sky blue
            1.5f,
            8f
        );
    }

    private Light CreatePointLight(string name, Vector3 position, Color color, float intensity, float range)
    {
        var lightObj = new GameObject($"Light_{name}");
        lightObj.transform.SetParent(transform);
        lightObj.transform.position = position;

        var light = lightObj.AddComponent<Light>();
        light.type = LightType.Point;
        light.color = color;
        light.intensity = intensity;
        light.range = range;
        light.shadows = LightShadows.None;            // mobile: no point light shadows
        light.renderMode = LightRenderMode.ForcePixel; // ensure per-pixel for neon quality

        return light;
    }

    private void SetupAmbient()
    {
        RenderSettings.ambientMode = AmbientMode.Trilight;
        RenderSettings.ambientSkyColor = HexColor("1A0A2E");       // deep purple-black sky
        RenderSettings.ambientEquatorColor = HexColor("0D0D2B");   // dark indigo equator
        RenderSettings.ambientGroundColor = HexColor("050510");    // near-black ground

        // Fog for depth
        RenderSettings.fog = true;
        RenderSettings.fogMode = FogMode.Exponential;
        RenderSettings.fogColor = new Color(0.02f, 0.02f, 0.04f, 1f);
        RenderSettings.fogDensity = 0.03f;
    }

    /// <summary>Converts a hex color string (without #) to a Unity Color.</summary>
    private static Color HexColor(string hex)
    {
        ColorUtility.TryParseHtmlString($"#{hex}", out Color color);
        return color;
    }
}
```

- [ ] **Step 2: Create AtmosphereController.cs**

```csharp
// Assets/Scripts/BalloonGame/Visuals/AtmosphereController.cs
using UnityEngine;

/// <summary>
/// Creates subtle atmospheric particle effects: floating dust motes and
/// low-lying smoke/mist. Uses simple ParticleSystems for mobile performance.
/// Attach to a root "Atmosphere" GameObject.
/// </summary>
[DisallowMultipleComponent]
public class AtmosphereController : MonoBehaviour
{
    private ParticleSystem _dustParticles;
    private ParticleSystem _smokeParticles;

    private void Start()
    {
        CreateDustParticles();
        CreateSmokeParticles();
    }

    /// <summary>Stop all atmospheric effects (e.g., during transitions).</summary>
    public void StopAll()
    {
        if (_dustParticles != null) _dustParticles.Stop();
        if (_smokeParticles != null) _smokeParticles.Stop();
    }

    /// <summary>Resume all atmospheric effects.</summary>
    public void ResumeAll()
    {
        if (_dustParticles != null) _dustParticles.Play();
        if (_smokeParticles != null) _smokeParticles.Play();
    }

    private void CreateDustParticles()
    {
        var dustObj = new GameObject("DustMotes");
        dustObj.transform.SetParent(transform);
        dustObj.transform.position = new Vector3(0f, 3f, -1f);

        _dustParticles = dustObj.AddComponent<ParticleSystem>();
        var main = _dustParticles.main;
        main.maxParticles = GameConstants.DUST_PARTICLE_COUNT;
        main.startLifetime = 8f;
        main.startSpeed = 0.05f;
        main.startSize = new ParticleSystem.MinMaxCurve(0.02f, 0.06f);
        main.startColor = new Color(1f, 0.95f, 0.85f, 0.15f);  // warm faint dust
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.playOnAwake = true;
        main.gravityModifier = -0.01f; // slight upward drift

        var emission = _dustParticles.emission;
        emission.rateOverTime = 4f;

        var shape = _dustParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(8f, 12f, 2f);

        var velocityOverLifetime = _dustParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.02f, 0.02f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(-0.01f, 0.03f);

        var colorOverLifetime = _dustParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient dustGradient = new();
        dustGradient.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.15f, 0.3f),
                    new GradientAlphaKey(0.15f, 0.7f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = dustGradient;

        // Use default particle material
        var renderer = dustObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", new Color(1f, 0.95f, 0.85f, 0.15f));
    }

    private void CreateSmokeParticles()
    {
        var smokeObj = new GameObject("LowSmoke");
        smokeObj.transform.SetParent(transform);
        smokeObj.transform.position = new Vector3(0f, GameConstants.LANE_BOTTOM + 1f, -0.5f);

        _smokeParticles = smokeObj.AddComponent<ParticleSystem>();
        var main = _smokeParticles.main;
        main.maxParticles = GameConstants.SMOKE_PARTICLE_COUNT;
        main.startLifetime = 12f;
        main.startSpeed = 0.03f;
        main.startSize = new ParticleSystem.MinMaxCurve(1.5f, 3.0f);
        main.startColor = new Color(0.15f, 0.12f, 0.18f, 0.06f); // dark purple-gray haze
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.loop = true;
        main.playOnAwake = true;

        var emission = _smokeParticles.emission;
        emission.rateOverTime = 0.8f;

        var shape = _smokeParticles.shape;
        shape.shapeType = ParticleSystemShapeType.Box;
        shape.scale = new Vector3(8f, 1f, 2f);

        var velocityOverLifetime = _smokeParticles.velocityOverLifetime;
        velocityOverLifetime.enabled = true;
        velocityOverLifetime.x = new ParticleSystem.MinMaxCurve(-0.015f, 0.015f);
        velocityOverLifetime.y = new ParticleSystem.MinMaxCurve(0.005f, 0.02f);

        var sizeOverLifetime = _smokeParticles.sizeOverLifetime;
        sizeOverLifetime.enabled = true;
        AnimationCurve smokeSizeCurve = new(
            new Keyframe(0f, 0.5f),
            new Keyframe(0.5f, 1f),
            new Keyframe(1f, 1.2f)
        );
        sizeOverLifetime.size = new ParticleSystem.MinMaxCurve(1f, smokeSizeCurve);

        var colorOverLifetime = _smokeParticles.colorOverLifetime;
        colorOverLifetime.enabled = true;
        Gradient smokeGradient = new();
        smokeGradient.SetKeys(
            new[] { new GradientColorKey(new Color(0.15f, 0.12f, 0.18f), 0f),
                    new GradientColorKey(new Color(0.1f, 0.08f, 0.12f), 1f) },
            new[] { new GradientAlphaKey(0f, 0f), new GradientAlphaKey(0.06f, 0.2f),
                    new GradientAlphaKey(0.04f, 0.8f), new GradientAlphaKey(0f, 1f) }
        );
        colorOverLifetime.color = smokeGradient;

        var renderer = smokeObj.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.material.SetColor("_Color", new Color(0.15f, 0.12f, 0.18f, 0.06f));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
    }
}
```

**Commit:** `git commit -m "feat(visuals): add NeonLightRig and AtmosphereController with dust/smoke particles"`

---

## Task 10: Camera Switch & Post-Processing

**Files:**
- Modify: `Assets/Scripts/BalloonGame/BalloonCamera.cs`

Switch from orthographic to a low-FOV perspective camera for subtle parallax and depth. Add URP post-processing Volume with bloom, vignette, color grading, and film grain.

- [ ] **Step 1: Rewrite BalloonCamera.cs**

Replace the full contents of `BalloonCamera.cs`:

```csharp
// Assets/Scripts/BalloonGame/BalloonCamera.cs
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

/// <summary>
/// Configures the main camera for the neon noir aesthetic.
/// Low-FOV perspective camera for subtle depth parallax.
/// Creates and manages a URP post-processing Volume with bloom,
/// vignette, color grading, and film grain.
/// </summary>
[RequireComponent(typeof(Camera))]
[DisallowMultipleComponent]
public class BalloonCamera : MonoBehaviour
{
    private const float TargetAspect = 9f / 16f;

    private Camera _camera;
    private Volume _postProcessVolume;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        ConfigureCamera();
        EnforceAspect();
        SetupPostProcessing();
    }

    private void OnValidate()
    {
        if (_camera == null) _camera = GetComponent<Camera>();
        if (_camera != null)
        {
            ConfigureCamera();
            EnforceAspect();
        }
    }

    private void ConfigureCamera()
    {
        // Switch to low-FOV perspective for depth parallax
        _camera.orthographic = false;
        _camera.fieldOfView = GameConstants.CAMERA_FOV;
        _camera.nearClipPlane = 0.3f;
        _camera.farClipPlane = 100f;

        // Position camera further back to compensate for perspective
        transform.position = new Vector3(0f, 0f, GameConstants.CAMERA_Z_POSITION);

        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color(0.02f, 0.02f, 0.03f); // near-black with slight blue
    }

    private void EnforceAspect()
    {
        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect > TargetAspect)
        {
            float width = TargetAspect / currentAspect;
            _camera.rect = new Rect((1f - width) / 2f, 0f, width, 1f);
            return;
        }

        if (currentAspect < TargetAspect)
        {
            float height = currentAspect / TargetAspect;
            _camera.rect = new Rect(0f, (1f - height) / 2f, 1f, height);
            return;
        }

        _camera.rect = new Rect(0f, 0f, 1f, 1f);
    }

    private void SetupPostProcessing()
    {
        // Ensure camera has URP additional data for post-processing
        var cameraData = _camera.GetUniversalAdditionalCameraData();
        if (cameraData != null)
        {
            cameraData.renderPostProcessing = true;
        }

        // Create Volume component on the camera for global post-processing
        _postProcessVolume = gameObject.AddComponent<Volume>();
        _postProcessVolume.isGlobal = true;
        _postProcessVolume.priority = 100;

        var profile = ScriptableObject.CreateInstance<VolumeProfile>();
        _postProcessVolume.profile = profile;

        // --- Bloom ---
        var bloom = profile.Add<Bloom>(overrideState: true);
        bloom.threshold.value = 1.2f;
        bloom.threshold.overrideState = true;
        bloom.intensity.value = 0.3f;
        bloom.intensity.overrideState = true;
        bloom.scatter.value = 0.7f;
        bloom.scatter.overrideState = true;

        // --- Vignette ---
        var vignette = profile.Add<Vignette>(overrideState: true);
        vignette.intensity.value = 0.35f;
        vignette.intensity.overrideState = true;
        vignette.smoothness.value = 0.4f;
        vignette.smoothness.overrideState = true;
        vignette.color.value = new Color(0.05f, 0.02f, 0.08f); // dark purple vignette
        vignette.color.overrideState = true;

        // --- Color Grading ---
        var colorGrading = profile.Add<ColorAdjustments>(overrideState: true);
        colorGrading.postExposure.value = -0.3f;
        colorGrading.postExposure.overrideState = true;
        colorGrading.contrast.value = 15f;
        colorGrading.contrast.overrideState = true;
        colorGrading.saturation.value = -10f;
        colorGrading.saturation.overrideState = true;

        var tonemapping = profile.Add<Tonemapping>(overrideState: true);
        tonemapping.mode.value = TonemappingMode.ACES;
        tonemapping.mode.overrideState = true;

        // --- Film Grain ---
        var filmGrain = profile.Add<FilmGrain>(overrideState: true);
        filmGrain.type.value = FilmGrainLookup.Thin1;
        filmGrain.type.overrideState = true;
        filmGrain.intensity.value = 0.15f;
        filmGrain.intensity.overrideState = true;
    }
}
```

- [ ] **Step 2: Verify camera framing**

The camera position `z = -32` with `FOV = 18` produces a vertical view of approximately:

```
viewHeight = 2 * tan(18/2 * deg2rad) * 32 = 2 * 0.1584 * 32 = 10.14 units
```

With a 9:16 aspect, total vertical coverage is `10.14 / (9/16)` = ~18 units, close to the original orthographic size of 20 (ortho size 10 = 20 units tall). Adjust `CAMERA_Z_POSITION` if needed during testing. The slight mismatch is intentional -- perspective mode naturally expands at the edges.

**Commit:** `git commit -m "feat(visuals): switch to perspective camera with URP post-processing (bloom, vignette, ACES, grain)"`

---

## Task 11: Update Scene Builder

**Files:**
- Modify: `Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs`

Update the editor scene builder to include the new visual system components (NeonLightRig, EnvironmentBuilder, AtmosphereController, StuckDartManager).

- [ ] **Step 1: Update BalloonSceneBuilder.cs**

Replace the full contents of `Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs`:

```csharp
// Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

/// <summary>
/// Editor tool that builds the InkshotScene from scratch.
/// Creates camera, lighting, environment, gameplay roots, and visual system components.
/// Menu: BalloonGame > Build Scene
/// </summary>
public static class BalloonSceneBuilder
{
    private const string MaterialsFolder = "Assets/Materials";
    private const string ScenePath = "Assets/Scenes/InkshotScene.unity";
    private const string WallBounceMaterialPath = MaterialsFolder + "/WallBounce.asset";
    private const string WallDeadMaterialPath = MaterialsFolder + "/WallDead.asset";

    /// <summary>Builds the complete InkshotScene and saves it.</summary>
    [MenuItem("BalloonGame/Build Scene")]
    public static void BuildScene()
    {
        EnsureFolder("Assets/Scenes");
        EnsureFolder(MaterialsFolder);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Physics materials
        var wallBounce = CreateOrUpdatePhysicsMaterial(
            WallBounceMaterialPath,
            GameConstants.WALL_BOUNCINESS,
            GameConstants.WALL_FRICTION,
            GameConstants.WALL_FRICTION,
            PhysicsMaterialCombine.Maximum);

        var wallDead = CreateOrUpdatePhysicsMaterial(
            WallDeadMaterialPath,
            0f, 1f, 1f,
            PhysicsMaterialCombine.Minimum);

        // Placeholder materials for scene builder (will be overridden at runtime by EnvironmentBuilder)
        var placeholderMat = new Material(GetSurfaceShader())
        {
            color = new Color(0.1f, 0.08f, 0.06f)
        };

        CreateCamera();
        CreateBackWall(placeholderMat);
        CreateFrameWalls(placeholderMat, wallBounce, wallDead);
        CreateLane(placeholderMat);
        CreateLaunchOrigin();
        CreateGameplayRoots();
        CreateVisualRoots();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("InkshotScene built successfully with visual system components.");
    }

    private static void CreateCamera()
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, GameConstants.CAMERA_Z_POSITION);
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<BalloonCamera>();
    }

    private static void CreateBackWall(Material material)
    {
        float boardCenterY = (GameConstants.BOARD_TOP + GameConstants.BOARD_BOTTOM) * 0.5f;

        var backWall = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backWall.name = "BackWall";
        backWall.layer = GameConstants.LAYER_ENVIRONMENT;
        backWall.transform.position = new Vector3(0f, boardCenterY, 1f);
        backWall.transform.localScale = new Vector3(
            GameConstants.BOARD_WIDTH + 1f,
            GameConstants.BOARD_HEIGHT + 1f,
            1f);
        backWall.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(backWall.GetComponent<MeshCollider>());
    }

    private static void CreateFrameWalls(Material material, PhysicsMaterial bounceMaterial, PhysicsMaterial deadMaterial)
    {
        float boardCenterY = (GameConstants.BOARD_TOP + GameConstants.BOARD_BOTTOM) * 0.5f;
        Vector3 wallScale = new(GameConstants.SIDE_WALL_WIDTH, GameConstants.BOARD_HEIGHT + 1f, 2f);

        var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "LeftWall";
        leftWall.layer = GameConstants.LAYER_ENVIRONMENT;
        leftWall.transform.position = new Vector3(
            GameConstants.BOARD_LEFT - GameConstants.SIDE_WALL_WIDTH * 0.5f,
            boardCenterY, 0.5f);
        leftWall.transform.localScale = wallScale;
        leftWall.GetComponent<Renderer>().sharedMaterial = material;
        leftWall.GetComponent<BoxCollider>().material = bounceMaterial;

        var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "RightWall";
        rightWall.layer = GameConstants.LAYER_ENVIRONMENT;
        rightWall.transform.position = new Vector3(
            GameConstants.BOARD_RIGHT + GameConstants.SIDE_WALL_WIDTH * 0.5f,
            boardCenterY, 0.5f);
        rightWall.transform.localScale = wallScale;
        rightWall.GetComponent<Renderer>().sharedMaterial = material;
        rightWall.GetComponent<BoxCollider>().material = bounceMaterial;

        var topWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topWall.name = "TopWall";
        topWall.layer = GameConstants.LAYER_ENVIRONMENT;
        topWall.transform.position = new Vector3(
            0f,
            GameConstants.BOARD_TOP + GameConstants.SIDE_WALL_WIDTH * 0.5f,
            0.5f);
        topWall.transform.localScale = new Vector3(
            GameConstants.BOARD_WIDTH + GameConstants.SIDE_WALL_WIDTH * 2f + 1f,
            GameConstants.SIDE_WALL_WIDTH, 2f);
        topWall.GetComponent<Renderer>().sharedMaterial = material;
        topWall.GetComponent<BoxCollider>().material = deadMaterial;
    }

    private static void CreateLane(Material material)
    {
        float laneCenterY = (GameConstants.LANE_TOP + GameConstants.LANE_BOTTOM) * 0.5f;

        var laneFloor = GameObject.CreatePrimitive(PrimitiveType.Quad);
        laneFloor.name = "LaneFloor";
        laneFloor.layer = GameConstants.LAYER_ENVIRONMENT;
        laneFloor.transform.position = new Vector3(0f, laneCenterY, 1f);
        laneFloor.transform.localScale = new Vector3(
            8f,
            GameConstants.LANE_TOP - GameConstants.LANE_BOTTOM,
            1f);
        laneFloor.GetComponent<Renderer>().sharedMaterial = material;
        Object.DestroyImmediate(laneFloor.GetComponent<MeshCollider>());
    }

    private static void CreateLaunchOrigin()
    {
        var launchOrigin = new GameObject("LaunchOrigin");
        launchOrigin.transform.position = GameConstants.LAUNCH_POSITION;
    }

    private static void CreateGameplayRoots()
    {
        var balloonWall = new GameObject("BalloonWall");
        balloonWall.AddComponent<BalloonWall>();

        var slingshotController = new GameObject("SlingshotController");
        slingshotController.transform.position = GameConstants.LAUNCH_POSITION;
        slingshotController.AddComponent<SlingshotInput>();
        slingshotController.AddComponent<SlingshotVisuals>();

        var gameManager = new GameObject("GameManager");
        gameManager.AddComponent<BalloonGameManager>();
        gameManager.AddComponent<ScoreManager>();
        gameManager.AddComponent<GameHUD>();
    }

    private static void CreateVisualRoots()
    {
        // Light rig
        var lightRig = new GameObject("LightRig");
        lightRig.AddComponent<NeonLightRig>();

        // Environment visual enhancements
        var environment = new GameObject("Environment");
        environment.AddComponent<EnvironmentBuilder>();

        // Atmospheric effects
        var atmosphere = new GameObject("Atmosphere");
        atmosphere.AddComponent<AtmosphereController>();

        // Stuck dart manager
        var stuckDarts = new GameObject("StuckDarts");
        stuckDarts.AddComponent<StuckDartManager>();
    }

    private static PhysicsMaterial CreateOrUpdatePhysicsMaterial(
        string assetPath, float bounciness, float dynamicFriction,
        float staticFriction, PhysicsMaterialCombine bounceCombine)
    {
        var material = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>(assetPath);
        if (material == null)
        {
            material = new PhysicsMaterial();
            AssetDatabase.CreateAsset(material, assetPath);
        }

        material.bounciness = bounciness;
        material.dynamicFriction = dynamicFriction;
        material.staticFriction = staticFriction;
        material.bounceCombine = bounceCombine;

        EditorUtility.SetDirty(material);
        return material;
    }

    private static Shader GetSurfaceShader()
    {
        return Shader.Find("Universal Render Pipeline/Lit") ?? Shader.Find("Standard");
    }

    private static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;

        string[] parts = path.Split('/');
        string current = parts[0];

        for (int index = 1; index < parts.Length; index++)
        {
            string next = current + "/" + parts[index];
            if (!AssetDatabase.IsValidFolder(next))
            {
                AssetDatabase.CreateFolder(current, parts[index]);
            }
            current = next;
        }
    }
}
#endif
```

**Commit:** `git commit -m "feat(editor): update BalloonSceneBuilder with visual system roots (lights, atmosphere, stuck darts)"`

---

## Task 12: Integration Verification & Performance Check

**Files:** (no new files)

Final verification that everything compiles and the visual pipeline is consistent.

- [ ] **Step 1: Verify folder structure exists**

After all tasks, the project should have these new paths:

```
Assets/
  Shaders/
    BalloonLit.shader
    ChromeDart.shader
  Scripts/BalloonGame/
    Visuals/
      BalloonMeshGenerator.cs
      DartMeshGenerator.cs
      EnvironmentBuilder.cs
      AtmosphereController.cs
      StuckDartManager.cs
      NeonLightRig.cs
```

- [ ] **Step 2: Verify zero compiler errors**

```bash
# From project root, Unity batch mode compile check
# (or simply open Unity and check the Console)
```

- [ ] **Step 3: Rebuild the scene**

In Unity: **BalloonGame > Build Scene** (menu item). This regenerates `InkshotScene.unity` with all new visual components.

- [ ] **Step 4: Enter Play Mode and verify**

Check for:

| Check | Expected |
|-------|----------|
| Balloons render with specular highlights, rim lighting | Glossy latex look, not flat |
| Balloons are elongated with tie at bottom | Visible knot/tail shape |
| Per-balloon color via MaterialPropertyBlock | 5 distinct colors, no material instances in profiler |
| Dart is chrome metallic mesh, not capsule | Shiny metallic dart shape |
| Back wall is near-black matte | Dark corkboard feel |
| Lane floor has subtle reflective quality | Wet look with high smoothness |
| Foul line and lane lines visible | Faded markings on floor |
| Two neon point lights casting color | Pink and blue light on balloons |
| Dust particles floating | Subtle warm motes |
| Smoke near bottom of screen | Low haze |
| Bloom on bright specular highlights | Soft glow on shiniest areas |
| Dark vignette at screen edges | Noir framing |
| Slightly desaturated, dark color grade | Moody, not washed out |
| Subtle film grain | Visible at pause, not distracting |
| Darts that hit top wall leave stuck dart | Visual-only dart embedded in wall |

- [ ] **Step 5: Performance budget check**

| Metric | Budget | Notes |
|--------|--------|-------|
| Draw calls | < 100 | Shared balloon material + MPB = 1 SetPass for all balloons |
| Triangles | < 50k | 72 balloons * 368 = 26.5k + environment ~2k + darts ~200 each |
| SetPass calls | < 20 | BalloonLit (1) + ChromeDart (1) + URP/Lit env (4-5) + particles (2) |
| Point lights | 2 | Neon pink + neon blue, no shadows |
| Particle systems | 2 | Dust (30 particles) + Smoke (8 particles) |

- [ ] **Step 6: Camera framing validation**

If the perspective camera framing doesn't match the original orthographic layout:

1. Adjust `CAMERA_Z_POSITION` in `GameConstants.cs` (more negative = wider view)
2. Adjust `CAMERA_FOV` (larger = wider view)
3. Target: all 8x9 balloons visible with slight margin, launch position visible at bottom

Fallback: If perspective causes gameplay issues, set `_camera.orthographic = true` in `BalloonCamera.ConfigureCamera()` and keep `CAMERA_ORTHO_SIZE = 10f`. The post-processing, lighting, shaders, and atmosphere all work identically in orthographic mode.

**Commit:** `git commit -m "verify: Plan 02 visual overhaul integration complete"`

---

## Quick Reference: All Color Values

| Item | Color | Hex | Usage |
|------|-------|-----|-------|
| Balloon Red | `(0.902, 0.224, 0.275)` | `#E63946` | Standard balloon |
| Balloon Blue | `(0.271, 0.482, 0.616)` | `#457B9D` | Standard balloon |
| Balloon Yellow | `(0.945, 0.890, 0.533)` | `#F1E388` | Standard balloon |
| Balloon Green | `(0.165, 0.616, 0.561)` | `#2A9D8F` | Standard balloon |
| Balloon Purple | `(0.608, 0.349, 0.714)` | `#9B59B6` | Standard balloon |
| Balloon Gold | `(1.000, 0.843, 0.000)` | `#FFD700` | Special: metallic foil |
| Balloon Paint | `(0.878, 0.165, 0.533)` | `#E02A88` | Special: paint splat |
| Balloon Prize | `(0.580, 0.000, 0.827)` | `#9400D3` | Special: crown/jackpot |
| Balloon Hazard | `(0.180, 0.180, 0.200)` | `#2E2E33` | Special: dark threat |
| Chrome dart body | `(0.78, 0.82, 0.88)` | `#C7D1E0` | Dart body |
| Stuck dart body | `(0.65, 0.68, 0.73)` | `#A6ADBA` | Stuck dart (duller) |
| Back wall | `(0.10, 0.08, 0.06)` | `#1A140F` | Dark corkboard |
| Wall frame | `(0.07, 0.05, 0.04)` | `#120D0A` | Charcoal wood |
| Lane floor | `(0.04, 0.04, 0.05)` | `#0A0A0D` | Wet dark floor |
| Lane lines | `(0.12, 0.10, 0.08)` | `#1F1A14` | Faded paint |
| Foul line | `(0.25, 0.08, 0.06)` | `#40140F` | Faded red |
| Directional light | `#FFF5E6` | `#FFF5E6` | Warm white |
| Neon pink light | `#FF1493` | `#FF1493` | Deep pink |
| Neon blue light | `#00BFFF` | `#00BFFF` | Deep sky blue |
| Ambient sky | `#1A0A2E` | `#1A0A2E` | Deep purple-black |
| Ambient equator | `#0D0D2B` | `#0D0D2B` | Dark indigo |
| Ambient ground | `#050510` | `#050510` | Near-black |
| SSS bleed | `(1.0, 0.4, 0.31)` | `#FF6650` | Backlight bleed |
| Fresnel glow | `(0.5, 0.2, 1.0)` | `#8033FF` | Neon edge pickup |
| Camera background | `(0.02, 0.02, 0.03)` | `#050508` | Near-black blue |
| Vignette | `(0.05, 0.02, 0.08)` | `#0D0514` | Dark purple |

## Quick Reference: All Transform Values

| Object | Position | Rotation | Scale |
|--------|----------|----------|-------|
| Main Camera | `(0, 0, -32)` | `(0, 0, 0)` | `(1, 1, 1)` |
| Directional Light | -- | `(50, -30, 0)` | -- |
| Neon Pink Light | `(-3, 4, -2)` | -- | -- |
| Neon Blue Light | `(3, 6, -2)` | -- | -- |
| BackWall | `(0, 3.75, 1)` | `(0, 0, 0)` | `(9, 10.5, 1)` |
| LeftWall | `(-4.15, 3.75, 0.5)` | `(0, 0, 0)` | `(0.3, 10.5, 2)` |
| RightWall | `(4.15, 3.75, 0.5)` | `(0, 0, 0)` | `(0.3, 10.5, 2)` |
| TopWall | `(0, 8.65, 0.5)` | `(0, 0, 0)` | `(9.6, 0.3, 2)` |
| LaneFloor | `(0, -5.25, 1)` | `(0, 0, 0)` | `(8, 6.5, 1)` |
| LaneLineLeft | `(-1.5, -5.25, 0.99)` | `(0, 0, 0)` | `(0.04, 4.55, 1)` |
| LaneLineRight | `(1.5, -5.25, 0.99)` | `(0, 0, 0)` | `(0.04, 4.55, 1)` |
| FoulLine | `(0, -2.2, 0.98)` | `(0, 0, 0)` | `(3.5, 0.03, 1)` |
| DepthVoid | `(0, 0, 3)` | `(0, 0, 0)` | `(20, 25, 1)` |
| DustMotes emitter | `(0, 3, -1)` | -- | shape `(8, 12, 2)` |
| LowSmoke emitter | `(0, -7.5, -0.5)` | -- | shape `(8, 1, 2)` |
| LaunchOrigin | `(0, -5.5, 0)` | -- | -- |
