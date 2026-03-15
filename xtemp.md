[EnvironmentBuilder] Destroyed 1 root 'BackWall' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:43)

[EnvironmentBuilder] Destroyed 1 root 'LaneFloor' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:43)

[EnvironmentBuilder] Destroyed 1 root 'Directional Light' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:43)

[EnvironmentBuilder] Destroyed 1 root 'LaunchOrigin' objects
UnityEngine.Debug:Log (object)
EnvironmentBuilder:DestroyAllRootObjects (string) (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.Cleanup.cs:51)
EnvironmentBuilder:Awake () (at Assets/Scripts/BalloonGame/Visuals/EnvironmentBuilder.cs:43)

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
BalloonGameManager:HandleLaunch (UnityEngine.Vector3) (at Assets/Scripts/BalloonGame/Managers/BalloonGameManager.Events.cs:13)
SlingshotInput:ReleasePull () (at Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs:218)
SlingshotInput:HandleTouchInput () (at Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs:138)
SlingshotInput:Update () (at Assets/Scripts/BalloonGame/Darts/SlingshotInput.cs:80)

