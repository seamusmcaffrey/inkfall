[EnvironmentBuilder] Destroyed 1 root 'BackWall' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:27)

[EnvironmentBuilder] Destroyed 1 root 'LaneFloor' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:27)

[EnvironmentBuilder] Destroyed 1 root 'Directional Light' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:27)

[EnvironmentBuilder] Destroyed 1 root 'LaunchOrigin' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:27)

The character with Unicode value \u25C6 was not found in the [LiberationSans SDF] font asset or any potential fallbacks. It was replaced by Unicode character \u25A1 in text object [Currency].
UnityEngine.Debug:LogWarning (object,UnityEngine.Object)
TMPro.TextMeshProUGUI:SetArraySizes (TMPro.TMP_Text/TextProcessingElement[]) (at ./Library/PackageCache/com.unity.ugui@bb329a87fcdc/Runtime/TMP/TextMeshProUGUI.cs:2012)
TMPro.TMP_Text:ParseInputText () (at ./Library/PackageCache/com.unity.ugui@bb329a87fcdc/Runtime/TMP/TMP_Text.cs:2044)
TMPro.TextMeshProUGUI:OnPreRenderCanvas () (at ./Library/PackageCache/com.unity.ugui@bb329a87fcdc/Runtime/TMP/TextMeshProUGUI.cs:2496)
TMPro.TextMeshProUGUI:Rebuild (UnityEngine.UI.CanvasUpdate) (at ./Library/PackageCache/com.unity.ugui@bb329a87fcdc/Runtime/TMP/TextMeshProUGUI.cs:229)
UnityEngine.Canvas:SendWillRenderCanvases () (at /Users/bokken/build/output/unity/unity/Modules/UI/ScriptBindings/UICanvas.bindings.cs:121)

Shader error in 'Inkshot/BalloonEmblem': syntax error: unexpected token 'triangle' at Assets/Shaders/BalloonEmblem.shader(72) (on metal)

Compiling Subshader: 0, Pass: EmblemForward, Fragment program with <no keywords>
Platform defines: SHADER_API_DESKTOP UNITY_COLORSPACE_GAMMA UNITY_ENABLE_DETAIL_NORMALMAP UNITY_ENABLE_REFLECTION_BUFFERS UNITY_FRAMEBUFFER_FETCH_AVAILABLE UNITY_LIGHTMAP_FULL_HDR UNITY_LIGHT_PROBE_PROXY_VOLUME UNITY_NEEDS_RENDERPASS_FBFETCH_FALLBACK UNITY_PBS_USE_BRDF1 UNITY_SPECCUBE_BLENDING UNITY_SPECCUBE_BOX_PROJECTION UNITY_USE_DITHER_MASK_FOR_ALPHABLENDED_SHADOWS
Disabled keywords: INSTANCING_ON SHADER_API_GLES30 SHADER_API_GLES31 SHADER_API_GLES32 UNITY_ASTC_NORMALMAP_ENCODING UNITY_HARDWARE_TIER1 UNITY_HARDWARE_TIER2 UNITY_HARDWARE_TIER3 UNITY_LIGHTMAP_DLDR_ENCODING UNITY_LIGHTMAP_RGBM_ENCODING UNITY_METAL_SHADOWS_USE_POINT_FILTERING UNITY_NO_DXT5nm UNITY_NO_SCREENSPACE_SHADOWS UNITY_PBS_USE_BRDF2 UNITY_PBS_USE_BRDF3 UNITY_PRETRANSFORM_TO_DISPLAY_ORIENTATION UNITY_UNIFIED_SHADER_PRECISION_MODEL UNITY_VIRTUAL_TEXTURING

Mesh.uv is out of bounds. The supplied array needs to be the same size as the Mesh.vertices array.
UnityEngine.Mesh:SetUVs (int,System.Collections.Generic.List`1<UnityEngine.Vector2>)
DartMeshGenerator:GenerateBody () (at Assets/Scripts/BalloonGame/Visuals/DartMeshGenerator.cs:69)
DartMeshGenerator:GetBodyMesh () (at Assets/Scripts/BalloonGame/Visuals/DartMeshGenerator.cs:29)
DartLauncher:CreateDartObject () (at Assets/Scripts/BalloonGame/Darts/DartLauncher.cs:60)
DartLauncher:EnsurePool () (at Assets/Scripts/BalloonGame/Darts/DartLauncher.cs:45)
DartLauncher:SpawnAndLaunch (UnityEngine.Vector3) (at Assets/Scripts/BalloonGame/Darts/DartLauncher.cs:14)
BalloonGameManager:HandleLaunch (UnityEngine.Vector3) (at Assets/Scripts/BalloonGame/Managers/BalloonGameManager.Events.cs:13)
SlingshotInput:ReleasePull () (at Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs:217)
SlingshotInput:HandleTouchInput () (at Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs:137)
SlingshotInput:Update () (at Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs:79)

[Haptics] Light
UnityEngine.Debug:Log (object)
Haptics:Play (Haptics/HapticType) (at Assets/Scripts/BalloonGame/Core/Haptics.cs:44)
HapticsUtility:Light () (at Assets/Scripts/BalloonGame/Haptics/HapticsUtility.cs:6)
JuiceManager:HandleBalloonPopped (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Juice/JuiceManager.cs:95)
EventBus/Binding`1<BalloonPoppedEvent>:Publish (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Core/EventBus.cs:37)
EventBus:Publish<BalloonPoppedEvent> (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Core/EventBus.cs:53)
BalloonNode:Pop (DartController) (at Assets/Scripts/BalloonGame/Balloons/BalloonNode.cs:91)
BalloonNode:OnCollisionEnter (UnityEngine.Collision) (at Assets/Scripts/BalloonGame/Balloons/BalloonNode.cs:129)
UnityEngine.Physics:OnSceneContact (UnityEngine.PhysicsScene,intptr,int) (at /Users/bokken/build/output/unity/unity/Modules/Physics/ScriptBindings/PhysicsContact.bindings.cs:45)

[Haptics] Light
UnityEngine.Debug:Log (object)
Haptics:Play (Haptics/HapticType) (at Assets/Scripts/BalloonGame/Core/Haptics.cs:44)
HapticsUtility:Light () (at Assets/Scripts/BalloonGame/Haptics/HapticsUtility.cs:6)
JuiceManager:HandleBalloonPopped (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Juice/JuiceManager.cs:95)
EventBus/Binding`1<BalloonPoppedEvent>:Publish (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Core/EventBus.cs:37)
EventBus:Publish<BalloonPoppedEvent> (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Core/EventBus.cs:53)
BalloonNode:Pop (DartController) (at Assets/Scripts/BalloonGame/Balloons/BalloonNode.cs:91)
DartController:CheckPeakOverlap () (at Assets/Scripts/BalloonGame/Darts/DartController.cs:269)
DartController:FixedUpdate () (at Assets/Scripts/BalloonGame/Darts/DartController.cs:102)

[Haptics] Light
UnityEngine.Debug:Log (object)
Haptics:Play (Haptics/HapticType) (at Assets/Scripts/BalloonGame/Core/Haptics.cs:44)
HapticsUtility:Light () (at Assets/Scripts/BalloonGame/Haptics/HapticsUtility.cs:6)
JuiceManager:HandleBalloonPopped (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Juice/JuiceManager.cs:95)
EventBus/Binding`1<BalloonPoppedEvent>:Publish (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Core/EventBus.cs:37)
EventBus:Publish<BalloonPoppedEvent> (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Core/EventBus.cs:53)
BalloonNode:Pop (DartController) (at Assets/Scripts/BalloonGame/Balloons/BalloonNode.cs:91)
DartController:CheckPeakOverlap () (at Assets/Scripts/BalloonGame/Darts/DartController.cs:269)
DartController:FixedUpdate () (at Assets/Scripts/BalloonGame/Darts/DartController.cs:102)

[Haptics] Heavy
UnityEngine.Debug:Log (object)
Haptics:Play (Haptics/HapticType) (at Assets/Scripts/BalloonGame/Core/Haptics.cs:44)
HapticsUtility:Heavy () (at Assets/Scripts/BalloonGame/Haptics/HapticsUtility.cs:8)
JuiceManager:HandleRoomFailed (RoomFailedEvent) (at Assets/Scripts/BalloonGame/Juice/JuiceManager.cs:178)
EventBus/Binding`1<RoomFailedEvent>:Publish (RoomFailedEvent) (at Assets/Scripts/BalloonGame/Core/EventBus.cs:37)
EventBus:Publish<RoomFailedEvent> (RoomFailedEvent) (at Assets/Scripts/BalloonGame/Core/EventBus.cs:53)
BalloonGameManager:HandleDartFinished (DartController) (at Assets/Scripts/BalloonGame/Managers/BalloonGameManager.Events.cs:45)
DartController:StopDart (string) (at Assets/Scripts/BalloonGame/Darts/DartController.cs:205)
DartController:FixedUpdate () (at Assets/Scripts/BalloonGame/Darts/DartController.cs:130)

