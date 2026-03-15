#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Inkshot.Editor.AgentBridge
{
    [InitializeOnLoad]
    public static class PlayModeCapture
    {
        private const string ScenePath = "Assets/Scenes/InkshotScene.unity";
        private const string OutputDirectory = "Logs/agent-feedback/screenshots";
        private const string ActiveKey = "Inkshot.PlayCapture.Active";
        private const string PhaseKey = "Inkshot.PlayCapture.Phase";
        private const string FinishingKey = "Inkshot.PlayCapture.Finishing";
        private const string AutoExitKey = "Inkshot.PlayCapture.AutoExit";
        private const double BootstrapWaitSeconds = 1.0;
        private const double PostClickWaitSeconds = 0.5;
        private const double GameBoardWaitSeconds = 4.0;
        private const double PostCaptureWaitSeconds = 0.5;

        private enum CapturePhase
        {
            EnterPlayMode,
            WaitForBootstrap,
            ClickStartRun,
            WaitForGameBoard,
            CaptureScreenshot,
            ExitPlayMode,
        }

        private static CapturePhase _phase;
        private static double _phaseStartedAt;
        private static bool _isFinishing;

        static PlayModeCapture()
        {
            RestoreStateIfNeeded();
        }

        public static void Run()
        {
            SessionState.SetBool(ActiveKey, true);
            SessionState.SetBool(FinishingKey, false);
            SessionState.SetBool(AutoExitKey, Application.isBatchMode || IsExecuteMethodLaunch());
            _isFinishing = false;
            Advance(CapturePhase.EnterPlayMode);
            EnsureOutputDirectory();
            AttachCallbacks();
            EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }

        private static void Update()
        {
            try
            {
                switch (_phase)
                {
                    case CapturePhase.EnterPlayMode:
                        if (EditorApplication.isPlayingOrWillChangePlaymode) break;
                        Advance(CapturePhase.WaitForBootstrap);
                        EditorApplication.isPlaying = true;
                        break;
                    case CapturePhase.WaitForBootstrap:
                        if (EditorApplication.isPlaying && HasElapsed(BootstrapWaitSeconds))
                            Advance(CapturePhase.ClickStartRun);
                        break;
                    case CapturePhase.ClickStartRun:
                        if (!HasElapsed(PostClickWaitSeconds)) break;
                        ClickButton("TitleScreen", "START RUN");
                        Advance(CapturePhase.WaitForGameBoard);
                        break;
                    case CapturePhase.WaitForGameBoard:
                        if (HasElapsed(GameBoardWaitSeconds))
                            Advance(CapturePhase.CaptureScreenshot);
                        break;
                    case CapturePhase.CaptureScreenshot:
                        CaptureGameView();
                        Advance(CapturePhase.ExitPlayMode);
                        break;
                    case CapturePhase.ExitPlayMode:
                        if (HasElapsed(PostCaptureWaitSeconds)) StartFinish();
                        break;
                }
            }
            catch (Exception ex)
            {
                if (Application.isBatchMode)
                    Console.WriteLine($"[PlayModeCapture] ERROR: {ex.Message}");
                StartFinish();
            }
        }

        private static void Advance(CapturePhase next)
        {
            SetPhase(next);
            _phaseStartedAt = EditorApplication.timeSinceStartup;
        }

        private const int CaptureWidth = 1080;
        private const int CaptureHeight = 1920;

        private static void CaptureGameView()
        {
            string gameFile = $"gameplay_{DateTime.UtcNow:yyyyMMdd_HHmmss}.png";
            string gamePath = Path.Combine(OutputDirectory, gameFile);
            RenderPortraitScreenshot(gamePath);
            if (!Application.isBatchMode) return;
            Console.WriteLine($"[PlayModeCapture] Game View: {gamePath}");
        }

        private static void RenderPortraitScreenshot(string path)
        {
            Camera cam = Camera.main;
            if (cam == null) cam = UnityEngine.Object.FindAnyObjectByType<Camera>();
            if (cam == null) return;

            var rt = new RenderTexture(CaptureWidth, CaptureHeight, 24);
            var tex = new Texture2D(CaptureWidth, CaptureHeight, TextureFormat.RGB24, false);

            RenderTexture prevTarget = cam.targetTexture;
            RenderTexture prevActive = RenderTexture.active;

            cam.targetTexture = rt;
            cam.Render();
            RenderTexture.active = rt;
            tex.ReadPixels(new UnityEngine.Rect(0, 0, CaptureWidth, CaptureHeight), 0, 0);
            tex.Apply();

            cam.targetTexture = prevTarget;
            RenderTexture.active = prevActive;

            File.WriteAllBytes(path, tex.EncodeToPNG());
            UnityEngine.Object.Destroy(tex);
            UnityEngine.Object.Destroy(rt);
        }

        private static void ClickButton(string rootName, string buttonName)
        {
            foreach (GameObject root in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                Transform match = FindNamed(root.transform, rootName);
                if (match == null) continue;
                foreach (Button btn in match.GetComponentsInChildren<Button>(true))
                    if (btn.name == buttonName) { btn.onClick.Invoke(); return; }
            }
        }

        private static Transform FindNamed(Transform t, string name)
        {
            if (t.name == name) return t;
            for (int i = 0; i < t.childCount; i++)
            {
                Transform m = FindNamed(t.GetChild(i), name);
                if (m != null) return m;
            }
            return null;
        }

        private static bool HasElapsed(double s) => EditorApplication.timeSinceStartup - _phaseStartedAt >= s;

        private static void StartFinish()
        {
            if (_isFinishing) return;
            _isFinishing = true;
            SessionState.SetBool(FinishingKey, true);
            if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
            else FinishAndExit();
        }

        private static void HandlePlayModeChanged(PlayModeStateChange c)
        {
            if (c == PlayModeStateChange.EnteredEditMode && _isFinishing) FinishAndExit();
        }

        private static void FinishAndExit()
        {
            DetachCallbacks();
            bool shouldExit = SessionState.GetBool(AutoExitKey, false);
            SessionState.EraseBool(ActiveKey);
            SessionState.EraseBool(FinishingKey);
            SessionState.EraseInt(PhaseKey);
            SessionState.EraseBool(AutoExitKey);
            if (shouldExit)
                EditorApplication.delayCall += () => EditorApplication.Exit(0);
        }

        private static void RestoreStateIfNeeded()
        {
            if (!SessionState.GetBool(ActiveKey, false)) return;
            _phase = (CapturePhase)SessionState.GetInt(PhaseKey, 0);
            _isFinishing = SessionState.GetBool(FinishingKey, false);
            _phaseStartedAt = EditorApplication.timeSinceStartup;
            AttachCallbacks();
            if (_isFinishing && !EditorApplication.isPlaying) FinishAndExit();
        }

        private static bool IsExecuteMethodLaunch() =>
            Array.Exists(Environment.GetCommandLineArgs(), a => a == "-executeMethod");

        private static void AttachCallbacks()
        {
            DetachCallbacks();
            EditorApplication.playModeStateChanged += HandlePlayModeChanged;
            EditorApplication.update += Update;
        }

        private static void DetachCallbacks()
        {
            EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
            EditorApplication.update -= Update;
        }
        private static void SetPhase(CapturePhase p) { _phase = p; SessionState.SetInt(PhaseKey, (int)p); }
        private static void EnsureOutputDirectory() { Directory.CreateDirectory(OutputDirectory); }
    }
}
#endif
