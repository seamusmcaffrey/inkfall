[EnvironmentBuilder] Destroyed 1 root 'BackWall' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:37)

[EnvironmentBuilder] Destroyed 1 root 'LaneFloor' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:37)

[EnvironmentBuilder] Destroyed 1 root 'Directional Light' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:37)

[EnvironmentBuilder] Destroyed 1 root 'LaunchOrigin' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:37)

The character with Unicode value \u25C6 was not found in the [LiberationSans SDF] font asset or any potential fallbacks. It was replaced by Unicode character \u25A1 in text object [Currency].
UnityEngine.Debug:LogWarning (object,UnityEngine.Object)
TMPro.TextMeshProUGUI:SetArraySizes (TMPro.TMP_Text/TextProcessingElement[]) (at ./Library/PackageCache/com.unity.ugui@bb329a87fcdc/Runtime/TMP/TextMeshProUGUI.cs:2012)
TMPro.TMP_Text:ParseInputText () (at ./Library/PackageCache/com.unity.ugui@bb329a87fcdc/Runtime/TMP/TMP_Text.cs:2044)
TMPro.TextMeshProUGUI:OnPreRenderCanvas () (at ./Library/PackageCache/com.unity.ugui@bb329a87fcdc/Runtime/TMP/TextMeshProUGUI.cs:2496)
TMPro.TextMeshProUGUI:Rebuild (UnityEngine.UI.CanvasUpdate) (at ./Library/PackageCache/com.unity.ugui@bb329a87fcdc/Runtime/TMP/TextMeshProUGUI.cs:229)
UnityEngine.Canvas:SendWillRenderCanvases () (at /Users/bokken/build/output/unity/unity/Modules/UI/ScriptBindings/UICanvas.bindings.cs:121)

Mesh.uv is out of bounds. The supplied array needs to be the same size as the Mesh.vertices array.
UnityEngine.Mesh:SetUVs (int,System.Collections.Generic.List`1<UnityEngine.Vector2>)
DartMeshGenerator:GenerateBody () (at Assets/Scripts/BalloonGame/Visuals/DartMeshGenerator.cs:69)
DartMeshGenerator:GetBodyMesh () (at Assets/Scripts/BalloonGame/Visuals/DartMeshGenerator.cs:29)
DartLauncher:CreateDartObject () (at Assets/Scripts/BalloonGame/Darts/DartLauncher.cs:60)
DartLauncher:EnsurePool () (at Assets/Scripts/BalloonGame/Darts/DartLauncher.cs:45)
DartLauncher:SpawnAndLaunch (UnityEngine.Vector3) (at Assets/Scripts/BalloonGame/Darts/DartLauncher.cs:14)
SceneViewDartTester:FireDart () (at Assets/Scripts/BalloonGame/Editor/SceneViewDartTester.cs:130)
SceneViewDartTester:HandleInput (UnityEditor.SceneView) (at Assets/Scripts/BalloonGame/Editor/SceneViewDartTester.cs:95)
SceneViewDartTester:OnSceneGUI (UnityEditor.SceneView) (at Assets/Scripts/BalloonGame/Editor/SceneViewDartTester.cs:47)
UnityEngine.GUIUtility:ProcessEvent (int,intptr,bool&)

iPhoneUtils.Vibrate()
UnityEngine.StackTraceUtility:ExtractStackTrace ()
Haptics:Play (Haptics/HapticType) (at Assets/Scripts/BalloonGame/Core/Haptics.cs:27)
HapticsUtility:Light () (at Assets/Scripts/BalloonGame/Haptics/HapticsUtility.cs:6)
JuiceManager:HandleBalloonPopped (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Juice/JuiceManager.cs:95)
EventBus/Binding`1<BalloonPoppedEvent>:Publish (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Core/EventBus.cs:37)
EventBus:Publish<BalloonPoppedEvent> (BalloonPoppedEvent) (at Assets/Scripts/BalloonGame/Core/EventBus.cs:53)
BalloonNode:Pop (DartController) (at Assets/Scripts/BalloonGame/Balloons/BalloonNode.cs:91)
BalloonNode:OnCollisionEnter (UnityEngine.Collision) (at Assets/Scripts/BalloonGame/Balloons/BalloonNode.cs:129)
UnityEngine.Physics:OnSceneContact (UnityEngine.PhysicsScene,intptr,int)

