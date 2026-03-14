#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Inkshot.Editor.AgentBridge
{
    public static class ScreenshotCapture
    {
        private const string OutputDirectory = "Logs/agent-feedback/screenshots";
        private const string ScenePath = "Assets/Scenes/InkshotScene.unity";
        private const int CaptureWidth = 1080;
        private const int CaptureHeight = 1920;
        private const int DepthBits = 24;

        public static string CaptureScene(string label = "scene")
        {
            EnsureOutputDirectory();

            if (!EnsureSceneLoaded())
            {
                return null;
            }

            Camera camera = FindSceneCamera();
            if (camera == null)
            {
                return null;
            }

            string fileName = $"{label}_{System.DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
            string outputPath = Path.Combine(OutputDirectory, fileName);

            RenderCameraToFile(camera, outputPath);
            return outputPath;
        }

        public static string CaptureGameObject(string objectName, string label = "object")
        {
            EnsureOutputDirectory();

            if (!EnsureSceneLoaded())
            {
                return null;
            }

            GameObject target = GameObject.Find(objectName);
            if (target == null)
            {
                return null;
            }

            Camera camera = FindSceneCamera();
            if (camera == null)
            {
                return null;
            }

            Vector3 originalPos = camera.transform.position;
            float originalSize = camera.orthographicSize;

            Renderer renderer = target.GetComponent<Renderer>();
            if (renderer != null)
            {
                Bounds bounds = renderer.bounds;
                camera.transform.position = new Vector3(
                    bounds.center.x,
                    bounds.center.y,
                    camera.transform.position.z);
                camera.orthographicSize = Mathf.Max(bounds.extents.x, bounds.extents.y) + 1f;
            }

            string fileName = $"{label}_{objectName}_{System.DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
            string outputPath = Path.Combine(OutputDirectory, fileName);

            RenderCameraToFile(camera, outputPath);

            camera.transform.position = originalPos;
            camera.orthographicSize = originalSize;

            return outputPath;
        }

        public static string[] CaptureAllCameras()
        {
            EnsureOutputDirectory();

            if (!EnsureSceneLoaded())
            {
                return System.Array.Empty<string>();
            }

            Camera[] cameras = Object.FindObjectsByType<Camera>(FindObjectsSortMode.None);
            var paths = new System.Collections.Generic.List<string>();

            foreach (Camera camera in cameras)
            {
                string fileName = $"camera_{camera.name}_{System.DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
                string outputPath = Path.Combine(OutputDirectory, fileName);
                RenderCameraToFile(camera, outputPath);
                paths.Add(outputPath);
            }

            return paths.ToArray();
        }

        private static void RenderCameraToFile(Camera camera, string outputPath)
        {
            var renderTexture = new RenderTexture(CaptureWidth, CaptureHeight, DepthBits);
            var screenShot = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);

            RenderTexture previousTarget = camera.targetTexture;
            RenderTexture previousActive = RenderTexture.active;

            camera.targetTexture = renderTexture;
            camera.Render();

            RenderTexture.active = renderTexture;
            screenShot.ReadPixels(new Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
            screenShot.Apply();

            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;

            byte[] pngBytes = screenShot.EncodeToPNG();
            File.WriteAllBytes(outputPath, pngBytes);

            Object.DestroyImmediate(screenShot);
            Object.DestroyImmediate(renderTexture);
        }

        private static Camera FindSceneCamera()
        {
            Camera camera = Camera.main;
            if (camera != null)
            {
                return camera;
            }

            return Object.FindAnyObjectByType<Camera>();
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

        private static void EnsureOutputDirectory()
        {
            if (!Directory.Exists(OutputDirectory))
            {
                Directory.CreateDirectory(OutputDirectory);
            }
        }
    }
}
#endif
