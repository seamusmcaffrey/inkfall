# Plan 01: Scene Setup, Camera & Environment

## Goal
Create `InkshotScene` with a fixed portrait orthographic camera, background wall surface, frame walls with colliders, and the shared constants file.

## Deliverables
- [ ] `Assets/Scripts/BalloonGame/GameConstants.cs`
- [ ] `Assets/Scripts/BalloonGame/BalloonCamera.cs`
- [ ] `Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs` (editor utility to construct scene)
- [ ] `Assets/Scenes/InkshotScene.unity`
- [ ] Materials: `WallFrame.mat`, `BackWall.mat`, `LaneFloor.mat`

---

## Step 1: Create GameConstants.cs

**File**: `Assets/Scripts/BalloonGame/GameConstants.cs`

Single source of truth for all tuning values. Every other script references this.

```csharp
using UnityEngine;

public static class GameConstants
{
    // ── Camera ──
    public const float CAMERA_ORTHO_SIZE = 10f;

    // ── Board ──
    public const int BOARD_COLUMNS = 8;
    public const int BOARD_ROWS = 9;
    public const int TOTAL_BALLOONS = BOARD_COLUMNS * BOARD_ROWS; // 72

    // Board rect in world space
    public const float BOARD_LEFT = -4.0f;
    public const float BOARD_RIGHT = 4.0f;
    public const float BOARD_TOP = 8.5f;
    public const float BOARD_BOTTOM = -1.0f;
    public const float BOARD_WIDTH = BOARD_RIGHT - BOARD_LEFT;   // 8.0
    public const float BOARD_HEIGHT = BOARD_TOP - BOARD_BOTTOM;  // 9.5

    // ── Lane (launch area) ──
    public const float LANE_TOP = -2.0f;
    public const float LANE_BOTTOM = -8.5f;

    // ── Launch Point ──
    public static readonly Vector3 LAUNCH_POSITION = new(0f, -5.5f, 0f);

    // ── Balloon Dimensions ──
    public const float BALLOON_MAX_WIDTH = 0.78f;
    public const float BALLOON_MAX_HEIGHT = 0.94f;
    public const float BALLOON_SLOT_RATIO_X = 0.9f;
    public const float BALLOON_SLOT_RATIO_Y = 0.95f;

    // ── Perspective Scaling ──
    public const float PERSPECTIVE_MIN_SCALE = 0.72f;
    public const float PERSPECTIVE_SCALE_RANGE = 0.28f;
    public const float PERSPECTIVE_HORIZONTAL_PINCH = 0.12f;
    public const float PERSPECTIVE_VERTICAL_COMPRESSION = 0.14f;

    // ── Slingshot Input ──
    public const float MAX_PULL_DISTANCE = 3.0f;     // World units of max drag
    public const float PULL_SPEED_EXPONENT = 1.45f;   // Non-linear speed curve
    public const float MIN_LAUNCH_SPEED = 10f;        // Tuned for Unity Rigidbody
    public const float MAX_LAUNCH_SPEED = 40f;        // Tuned for Unity Rigidbody
    public const float AIM_ACTIVATION_RADIUS = 4f;    // Must click within this of launch point

    // ── Scoring ──
    public const int BASE_TARGET_SCORE = 3000;
    public const int STARTING_DARTS = 4;
    public const int SCORE_PER_BALLOON = 100;

    // ── Wall Frame ──
    public const float SIDE_WALL_WIDTH = 0.3f;
    public const float WALL_BOUNCINESS = 0.8f;
    public const float WALL_FRICTION = 0.1f;

    // ── Physics Layers ──
    // Existing: Projectiles=8, Enemies=7
    // New: Balloons — assign in Tag Manager or use Enemies layer
    public const int LAYER_PROJECTILES = 8;
    public const int LAYER_BALLOONS = 7; // Reuse Enemies layer for balloon collisions
}
```

---

## Step 2: Create BalloonCamera.cs

**File**: `Assets/Scripts/BalloonGame/BalloonCamera.cs`

```csharp
using UnityEngine;

/// <summary>
/// Configures camera for portrait orthographic view.
/// Enforces 9:16 aspect ratio with pillarboxing/letterboxing.
/// </summary>
public class BalloonCamera : MonoBehaviour
{
    private Camera _camera;
    private const float TARGET_ASPECT = 9f / 16f;

    private void Awake()
    {
        _camera = GetComponent<Camera>();
        _camera.orthographic = true;
        _camera.orthographicSize = GameConstants.CAMERA_ORTHO_SIZE;
        _camera.nearClipPlane = 0.1f;
        _camera.farClipPlane = 50f;
        _camera.clearFlags = CameraClearFlags.SolidColor;
        _camera.backgroundColor = new Color(0.08f, 0.07f, 0.06f);
        EnforceAspect();
    }

    private void EnforceAspect()
    {
        float currentAspect = (float)Screen.width / Screen.height;
        if (currentAspect > TARGET_ASPECT)
        {
            float w = TARGET_ASPECT / currentAspect;
            _camera.rect = new Rect((1f - w) / 2f, 0, w, 1);
        }
        else if (currentAspect < TARGET_ASPECT)
        {
            float h = currentAspect / TARGET_ASPECT;
            _camera.rect = new Rect(0, (1f - h) / 2f, 1, h);
        }
    }
}
```

---

## Step 3: Scene Construction via Editor Script

**File**: `Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs`

```csharp
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public static class BalloonSceneBuilder
{
    [MenuItem("BalloonGame/Build Scene")]
    public static void BuildScene()
    {
        var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

        // Ensure material directories exist
        EnsureFolder("Assets/Materials");

        // === CAMERA ===
        var cameraGO = new GameObject("Main Camera");
        cameraGO.tag = "MainCamera";
        cameraGO.AddComponent<Camera>();
        cameraGO.AddComponent<BalloonCamera>();
        cameraGO.transform.position = new Vector3(0, 0, -10);

        // === DIRECTIONAL LIGHT ===
        var lightGO = new GameObject("Directional Light");
        var light = lightGO.AddComponent<Light>();
        light.type = LightType.Directional;
        light.color = new Color(1f, 0.95f, 0.9f);
        light.intensity = 1.0f;
        lightGO.transform.rotation = Quaternion.Euler(50, -30, 0);

        // === AMBIENT LIGHT ===
        RenderSettings.ambientMode = UnityEngine.Rendering.AmbientMode.Flat;
        RenderSettings.ambientLight = new Color(0.3f, 0.28f, 0.26f);

        // === BACK WALL ===
        var backWall = GameObject.CreatePrimitive(PrimitiveType.Quad);
        backWall.name = "BackWall";
        float boardCenterY = (GameConstants.BOARD_TOP + GameConstants.BOARD_BOTTOM) / 2f;
        backWall.transform.position = new Vector3(0, boardCenterY, 1f);
        backWall.transform.localScale = new Vector3(
            GameConstants.BOARD_WIDTH + 1f,
            GameConstants.BOARD_HEIGHT + 1f, 1f);
        var wallMat = CreateMat("BackWall", new Color(0.18f, 0.16f, 0.14f), 0.1f);
        backWall.GetComponent<Renderer>().material = wallMat;
        // Back wall quad has no collider needed (MeshCollider from CreatePrimitive is fine to remove)
        Object.DestroyImmediate(backWall.GetComponent<MeshCollider>());

        // === FRAME MATERIAL ===
        var frameMat = CreateMat("WallFrame", new Color(0.15f, 0.12f, 0.10f), 0.05f);

        // === BOUNCY PHYSICS MATERIAL ===
        var bouncyPhysMat = new PhysicMaterial("WallBounce");
        bouncyPhysMat.bounciness = GameConstants.WALL_BOUNCINESS;
        bouncyPhysMat.dynamicFriction = GameConstants.WALL_FRICTION;
        bouncyPhysMat.staticFriction = GameConstants.WALL_FRICTION;
        bouncyPhysMat.bounceCombine = PhysicMaterialCombine.Maximum;
        AssetDatabase.CreateAsset(bouncyPhysMat, "Assets/Materials/WallBounce.physicMaterial");

        // === DEAD PHYSICS MATERIAL (top wall — dart stops) ===
        var deadPhysMat = new PhysicMaterial("WallDead");
        deadPhysMat.bounciness = 0f;
        deadPhysMat.dynamicFriction = 1f;
        deadPhysMat.staticFriction = 1f;
        deadPhysMat.bounceCombine = PhysicMaterialCombine.Minimum;
        AssetDatabase.CreateAsset(deadPhysMat, "Assets/Materials/WallDead.physicMaterial");

        // === LEFT WALL ===
        var leftWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        leftWall.name = "LeftWall";
        leftWall.transform.position = new Vector3(
            GameConstants.BOARD_LEFT - GameConstants.SIDE_WALL_WIDTH / 2f,
            boardCenterY, 0.5f);
        leftWall.transform.localScale = new Vector3(
            GameConstants.SIDE_WALL_WIDTH,
            GameConstants.BOARD_HEIGHT + 1f, 2f);
        leftWall.GetComponent<Renderer>().material = frameMat;
        leftWall.GetComponent<BoxCollider>().material = bouncyPhysMat;

        // === RIGHT WALL ===
        var rightWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        rightWall.name = "RightWall";
        rightWall.transform.position = new Vector3(
            GameConstants.BOARD_RIGHT + GameConstants.SIDE_WALL_WIDTH / 2f,
            boardCenterY, 0.5f);
        rightWall.transform.localScale = leftWall.transform.localScale;
        rightWall.GetComponent<Renderer>().material = frameMat;
        rightWall.GetComponent<BoxCollider>().material = bouncyPhysMat;

        // === TOP WALL (dart-stopping) ===
        var topWall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        topWall.name = "TopWall";
        topWall.transform.position = new Vector3(0,
            GameConstants.BOARD_TOP + GameConstants.SIDE_WALL_WIDTH / 2f, 0.5f);
        topWall.transform.localScale = new Vector3(
            GameConstants.BOARD_WIDTH + 2 * GameConstants.SIDE_WALL_WIDTH + 1f,
            GameConstants.SIDE_WALL_WIDTH, 2f);
        topWall.GetComponent<Renderer>().material = frameMat;
        topWall.GetComponent<BoxCollider>().material = deadPhysMat;

        // === LANE FLOOR ===
        var laneFloor = GameObject.CreatePrimitive(PrimitiveType.Quad);
        laneFloor.name = "LaneFloor";
        float laneCenterY = (GameConstants.LANE_TOP + GameConstants.LANE_BOTTOM) / 2f;
        laneFloor.transform.position = new Vector3(0, laneCenterY, 1f);
        laneFloor.transform.localScale = new Vector3(8f,
            GameConstants.LANE_TOP - GameConstants.LANE_BOTTOM, 1f);
        var laneMat = CreateMat("LaneFloor", new Color(0.1f, 0.09f, 0.08f), 0.15f);
        laneFloor.GetComponent<Renderer>().material = laneMat;
        Object.DestroyImmediate(laneFloor.GetComponent<MeshCollider>());

        // === LAUNCH ORIGIN MARKER ===
        var launchMarker = new GameObject("LaunchOrigin");
        launchMarker.transform.position = GameConstants.LAUNCH_POSITION;

        // === BALLOON WALL CONTAINER ===
        var wallContainer = new GameObject("BalloonWall");
        wallContainer.AddComponent<BalloonWall>();

        // === SLINGSHOT CONTROLLER ===
        var slingshotGO = new GameObject("SlingshotController");
        slingshotGO.transform.position = GameConstants.LAUNCH_POSITION;
        slingshotGO.AddComponent<SlingshotInput>();
        slingshotGO.AddComponent<SlingshotVisuals>();

        // === GAME MANAGER ===
        var managerGO = new GameObject("GameManager");
        managerGO.AddComponent<BalloonGameManager>();
        managerGO.AddComponent<ScoreManager>();
        managerGO.AddComponent<GameHUD>();

        // Save
        EditorSceneManager.SaveScene(scene, "Assets/Scenes/InkshotScene.unity");
        AssetDatabase.Refresh();
        Debug.Log("InkshotScene built successfully!");
    }

    private static Material CreateMat(string name, Color color, float smoothness)
    {
        var mat = new Material(Shader.Find("Standard"));
        mat.color = color;
        mat.SetFloat("_Glossiness", smoothness);
        AssetDatabase.CreateAsset(mat, $"Assets/Materials/{name}.mat");
        return mat;
    }

    private static void EnsureFolder(string path)
    {
        if (!AssetDatabase.IsValidFolder(path))
        {
            var parts = path.Split('/');
            string current = parts[0];
            for (int i = 1; i < parts.Length; i++)
            {
                string next = current + "/" + parts[i];
                if (!AssetDatabase.IsValidFolder(next))
                    AssetDatabase.CreateFolder(current, parts[i]);
                current = next;
            }
        }
    }
}
#endif
```

---

## Step 4: Physics Configuration

Configure in Project Settings (or via script):
- **Physics Layer**: Use layer 7 (`Enemies`) for balloons — already defined, no changes needed
- **Collision Matrix**: Ensure layer 8 (`Projectiles`) collides with layer 7 (`Enemies`/Balloons)
- **Gravity**: Default Unity gravity (-9.81) is fine — adjust `Rigidbody.mass` and launch speed to feel right

---

## Verification Checklist

- [ ] InkshotScene loads with no errors
- [ ] Portrait orthographic camera visible
- [ ] Dark back wall behind balloon area
- [ ] Left, right, top frame walls with colliders
- [ ] Side walls have bouncy PhysicMaterial
- [ ] Top wall has dead PhysicMaterial (no bounce)
- [ ] Launch lane area visible at bottom
- [ ] LaunchOrigin marker at (0, -5.5, 0)

## Files Created
1. `Assets/Scripts/BalloonGame/GameConstants.cs`
2. `Assets/Scripts/BalloonGame/BalloonCamera.cs`
3. `Assets/Scripts/BalloonGame/Editor/SceneBuilder.cs`
4. `Assets/Scenes/InkshotScene.unity` (via editor script)
5. `Assets/Materials/BackWall.mat`
6. `Assets/Materials/WallFrame.mat`
7. `Assets/Materials/LaneFloor.mat`
8. `Assets/Materials/WallBounce.physicMaterial`
9. `Assets/Materials/WallDead.physicMaterial`
