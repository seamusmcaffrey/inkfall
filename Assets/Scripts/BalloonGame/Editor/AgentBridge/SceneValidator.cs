#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Inkshot.Editor.AgentBridge
{
    public static class SceneValidator
    {
        private const string ScenePath = "Assets/Scenes/InkshotScene.unity";

        public static ValidationResult Validate()
        {
            var checks = new List<ValidationCheck>();

            bool sceneLoaded = EnsureSceneLoaded();
            checks.Add(new ValidationCheck
            {
                category = "Scene",
                description = "InkshotScene exists and loads",
                passed = sceneLoaded,
                detail = sceneLoaded ? ScenePath : "Scene not found at " + ScenePath,
            });

            if (!sceneLoaded)
            {
                return BuildResult(checks);
            }

            ValidateCamera(checks);
            ValidateLayers(checks);
            ValidateRequiredObjects(checks);
            ValidatePhysicsMaterials(checks);

            return BuildResult(checks);
        }

        private static void ValidateCamera(List<ValidationCheck> checks)
        {
            var camera = Object.FindAnyObjectByType<Camera>();
            bool cameraExists = camera != null;
            checks.Add(new ValidationCheck
            {
                category = "Camera",
                description = "Main Camera exists",
                passed = cameraExists,
                detail = cameraExists ? "Found" : "No Camera in scene",
            });

            if (!cameraExists)
            {
                return;
            }

            bool isPerspective = !camera.orthographic;
            checks.Add(new ValidationCheck
            {
                category = "Camera",
                description = "Camera is perspective",
                passed = isPerspective,
                detail = isPerspective ? "Perspective" : "Orthographic (expected perspective)",
            });

            float fov = camera.fieldOfView;
            bool correctFov = Mathf.Abs(fov - GameConstants.CAMERA_FOV) < 1f;
            checks.Add(new ValidationCheck
            {
                category = "Camera",
                description = "Field of view matches config",
                passed = correctFov,
                detail = $"Expected {GameConstants.CAMERA_FOV}, got {fov}",
            });

            float cameraY = camera.transform.position.y;
            const float positionTolerance = 0.1f;
            bool correctPos = Mathf.Abs(cameraY - GameConstants.CAMERA_Y_CENTER) < positionTolerance;
            checks.Add(new ValidationCheck
            {
                category = "Camera",
                description = "Camera Y at expected center",
                passed = correctPos,
                detail = $"Expected Y≈{GameConstants.CAMERA_Y_CENTER}, got {cameraY}",
            });
        }

        private static void ValidateLayers(List<ValidationCheck> checks)
        {
            ValidateLayer(checks, "Environment", GameConstants.LAYER_ENVIRONMENT);
            ValidateLayer(checks, "Balloons", GameConstants.LAYER_BALLOONS);
            ValidateLayer(checks, "Projectiles", GameConstants.LAYER_PROJECTILES);
            ValidateLayer(checks, "Ricochet", GameConstants.LAYER_RICOCHET);
            ValidateLayer(checks, "UIWorld", GameConstants.LAYER_UI_WORLD);
        }

        private static void ValidateLayer(List<ValidationCheck> checks, string expectedName, int layerIndex)
        {
            string actualName = LayerMask.LayerToName(layerIndex);
            bool valid = !string.IsNullOrEmpty(actualName);
            checks.Add(new ValidationCheck
            {
                category = "Layers",
                description = $"Layer {layerIndex} ({expectedName}) is defined",
                passed = valid,
                detail = valid ? $"Named '{actualName}'" : $"Layer {layerIndex} is undefined",
            });
        }

        private static void ValidateRequiredObjects(List<ValidationCheck> checks)
        {
            RequireComponent<BalloonWall>(checks, "BalloonWall");
            RequireComponent<BalloonGameManager>(checks, "BalloonGameManager");
            RequireComponent<SlingshotInput>(checks, "SlingshotInput");
            RequireComponent<DartLauncher>(checks, "DartLauncher");
            RequireComponent<RunManager>(checks, "RunManager");
            RequireComponent<ScoreManager>(checks, "ScoreManager");
            RequireComponent<InGameHUD>(checks, "InGameHUD");
            RequireComponent<TitleScreen>(checks, "TitleScreen");
        }

        private static void RequireComponent<T>(List<ValidationCheck> checks, string label) where T : Component
        {
            var component = Object.FindAnyObjectByType<T>();
            bool found = component != null;
            checks.Add(new ValidationCheck
            {
                category = "RequiredComponents",
                description = $"{label} exists in scene",
                passed = found,
                detail = found ? $"Found on '{component.gameObject.name}'" : "Not found",
            });
        }

        private static void ValidatePhysicsMaterials(List<ValidationCheck> checks)
        {
            var wallBounce = AssetDatabase.LoadAssetAtPath<PhysicsMaterial>("Assets/Materials/WallBounce.asset");
            bool bounceExists = wallBounce != null;
            checks.Add(new ValidationCheck
            {
                category = "Physics",
                description = "WallBounce physics material exists",
                passed = bounceExists,
                detail = bounceExists
                    ? $"Bounciness={wallBounce.bounciness}"
                    : "Asset not found",
            });
        }

        private static bool EnsureSceneLoaded()
        {
            if (!File.Exists(ScenePath))
            {
                return false;
            }

            var scene = EditorSceneManager.GetActiveScene();
            if (scene.path != ScenePath)
            {
                EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
            }

            return true;
        }

        private static ValidationResult BuildResult(List<ValidationCheck> checks)
        {
            int pass = 0;
            int fail = 0;
            foreach (var check in checks)
            {
                if (check.passed) pass++;
                else fail++;
            }

            return new ValidationResult
            {
                allPassed = fail == 0,
                passCount = pass,
                failCount = fail,
                checks = checks,
            };
        }

    }
}
#endif
