#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;

public static partial class BalloonSceneBuilder
{
    private const string MaterialsFolder = "Assets/Materials";
    private const string ScenePath = "Assets/Scenes/InkshotScene.unity";
    private const string BackWallMaterialPath = MaterialsFolder + "/BackWall.mat";
    private const string WallFrameMaterialPath = MaterialsFolder + "/WallFrame.mat";
    private const string LaneFloorMaterialPath = MaterialsFolder + "/LaneFloor.mat";
    private const string WallBounceMaterialPath = MaterialsFolder + "/WallBounce.asset";
    private const string WallDeadMaterialPath = MaterialsFolder + "/WallDead.asset";
    private const string BalloonRedMaterialPath = MaterialsFolder + "/BalloonRed.mat";
    private const string BalloonBlueMaterialPath = MaterialsFolder + "/BalloonBlue.mat";
    private const string BalloonYellowMaterialPath = MaterialsFolder + "/BalloonYellow.mat";
    private const string BalloonGreenMaterialPath = MaterialsFolder + "/BalloonGreen.mat";
    private const string BalloonPurpleMaterialPath = MaterialsFolder + "/BalloonPurple.mat";

    [MenuItem("BalloonGame/Setup Render Pipeline")]
    public static void SetupRenderPipeline()
    {
        EnsureRenderPipeline();
        Debug.Log("URP render pipeline configured.");
    }

    [MenuItem("BalloonGame/Build Scene")]
    public static void BuildScene()
    {
        EnsureRenderPipeline();
        EnsureFolder("Assets/Scenes");
        EnsureFolder(MaterialsFolder);

        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        var backWallMaterial = CreateOrUpdateMaterial(BackWallMaterialPath, new Color(0.18f, 0.16f, 0.14f), 0.1f);
        var frameMaterial = CreateOrUpdateMaterial(WallFrameMaterialPath, new Color(0.15f, 0.12f, 0.10f), 0.05f);
        var laneMaterial = CreateOrUpdateMaterial(LaneFloorMaterialPath, new Color(0.10f, 0.09f, 0.08f), 0.15f);

        var wallBounce = CreateOrUpdatePhysicsMaterial(
            WallBounceMaterialPath,
            GameConstants.WALL_BOUNCINESS,
            GameConstants.WALL_FRICTION,
            GameConstants.WALL_FRICTION,
            PhysicsMaterialCombine.Maximum);

        var wallDead = CreateOrUpdatePhysicsMaterial(
            WallDeadMaterialPath,
            0f,
            1f,
            1f,
            PhysicsMaterialCombine.Minimum);

        CreateBalloonMaterials();

        CreateCamera();
        CreateEventSystem();
        CreateLighting();
        CreateBackWall(backWallMaterial);
        CreateFrameWalls(frameMaterial, wallBounce, wallDead);
        CreateLane(laneMaterial);
        CreateLaunchOrigin();
        CreateGameplayRoots();

        EditorSceneManager.SaveScene(scene, ScenePath);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        Debug.Log("InkshotScene built successfully.");
    }

    private static void CreateCamera()
    {
        var cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        cameraObject.transform.position = new Vector3(0f, 0f, -10f);
        cameraObject.AddComponent<Camera>();
        cameraObject.AddComponent<AudioListener>();
        cameraObject.AddComponent<BalloonCamera>();
    }

    private static void CreateEventSystem()
    {
        var eventSystemObject = new GameObject("EventSystem");
        eventSystemObject.AddComponent<EventSystem>();
        eventSystemObject.AddComponent<InputSystemUIInputModule>();
    }

    private static void CreateLighting()
    {
        var lightObject = new GameObject("Directional Light");
        var light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.95f, 0.9f);
        light.intensity = 1f;
        lightObject.transform.rotation = Quaternion.Euler(50f, -30f, 0f);

        RenderSettings.ambientMode = AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.3f, 0.28f, 0.26f);
    }

    private static void CreateGameplayRoots()
    {
        var balloonWallObject = new GameObject("BalloonWall");
        var balloonWall = balloonWallObject.AddComponent<BalloonWall>();
        balloonWallObject.AddComponent<EnvironmentBuilder>().BuildEnvironment();
        balloonWallObject.AddComponent<AtmosphereController>();
        balloonWallObject.AddComponent<NeonLightRig>().BuildRig();
        balloonWallObject.AddComponent<StuckDartManager>();

        var slingshotControllerObject = new GameObject("SlingshotController");
        slingshotControllerObject.transform.position = GameConstants.LAUNCH_POSITION;
        var slingshotInput = slingshotControllerObject.AddComponent<SlingshotInput>();
        var aimAssist = slingshotControllerObject.AddComponent<AimAssist>();
        slingshotControllerObject.AddComponent<SlingshotVisuals>();
        slingshotControllerObject.AddComponent<LaunchLaneVisuals>();
        slingshotControllerObject.AddComponent<LaunchFeel>();
        aimAssist.SetBalloonWall(balloonWall);

        var gameManagerObject = new GameObject("GameManager");
        var balloonGameManager = gameManagerObject.AddComponent<BalloonGameManager>();
        ComponentUtility.EnsureComponent<ScoreManager>(gameManagerObject);
        ComponentUtility.EnsureComponent<ComboTracker>(gameManagerObject);
        var perkManager = gameManagerObject.AddComponent<PerkManager>();
        gameManagerObject.AddComponent<RoomGenerator>();
        var runManager = gameManagerObject.AddComponent<RunManager>();
        gameManagerObject.AddComponent<JuiceManager>();
        gameManagerObject.AddComponent<PerformanceConfig>();
        var dartLauncher = gameManagerObject.AddComponent<DartLauncher>();

        var inGameHud = new GameObject("InGameHUD").AddComponent<InGameHUD>();
        var runHud = new GameObject("RunHUD").AddComponent<RunHUD>();
        var perkSelectionScreen = new GameObject("PerkSelectionScreen").AddComponent<PerkSelectionScreen>();
        var roomIntroScreen = new GameObject("RoomIntroScreen").AddComponent<RoomIntroScreen>();
        var runEndScreen = new GameObject("RunEndScreen").AddComponent<RunEndScreen>();
        var fadeOverlay = new GameObject("FadeOverlay").AddComponent<FadeOverlay>();
        var settingsPanel = new GameObject("SettingsPanel").AddComponent<SettingsPanel>();
        var titleScreen = new GameObject("TitleScreen").AddComponent<TitleScreen>();
        new GameObject("TutorialOverlay").AddComponent<TutorialOverlay>();
        var pauseManager = new GameObject("PauseManager").AddComponent<PauseManager>();

        balloonGameManager.SetDependencies(balloonWall, slingshotInput, dartLauncher);
        runManager.SetDependencies(balloonGameManager, perkSelectionScreen, roomIntroScreen, runEndScreen, runHud, fadeOverlay);
        titleScreen.SetDependencies(runManager, settingsPanel);
        pauseManager.SetDependencies(settingsPanel);
    }
}
#endif
