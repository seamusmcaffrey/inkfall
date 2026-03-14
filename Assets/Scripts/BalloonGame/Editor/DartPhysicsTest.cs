#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Play-mode dart physics test with screenshot capture.
/// Enters play mode, clicks START RUN, fires darts at 30/60/100% pull,
/// and captures before/after screenshots for each pull level.
/// </summary>
public static class DartPhysicsTest
{
    private const string ScenePath = "Assets/Scenes/InkshotScene.unity";
    private const string OutputDir = "Logs/agent-feedback/screenshots";
    private const string ActiveKey = "Inkshot.DartTest.Active";
    private const string PhaseKey = "Inkshot.DartTest.Phase";
    private const string FinishKey = "Inkshot.DartTest.Finishing";

    private enum TestPhase
    {
        EnterPlayMode, WaitForBootstrap, ClickStartRun,
        WaitForGameBoard, CaptureBeforeShot, AttachRunner,
        WaitForRunner, ExitPlayMode,
    }

    private static TestPhase _phase;
    private static double _phaseStart;
    private static bool _isFinishing;

    [InitializeOnLoadMethod]
    private static void RestoreStateIfNeeded()
    {
        if (!SessionState.GetBool(ActiveKey, false)) return;
        _phase = (TestPhase)SessionState.GetInt(PhaseKey, 0);
        _isFinishing = SessionState.GetBool(FinishKey, false);
        _phaseStart = EditorApplication.timeSinceStartup;
        AttachCallbacks();
        if (_isFinishing && !EditorApplication.isPlaying) FinishAndExit();
    }

    [MenuItem("BalloonGame/Test Dart Physics (Screenshots)")]
    public static void Run()
    {
        SessionState.SetBool(ActiveKey, true);
        SessionState.SetBool(FinishKey, false);
        _isFinishing = false;
        DartPhysicsTestRunner.IsComplete = false;
        Advance(TestPhase.EnterPlayMode);
        System.IO.Directory.CreateDirectory(OutputDir);
        AttachCallbacks();
        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        Log("Dart physics test starting...");
    }

    private static void Update()
    {
        try
        {
            switch (_phase)
            {
                case TestPhase.EnterPlayMode:
                    if (EditorApplication.isPlayingOrWillChangePlaymode) break;
                    Advance(TestPhase.WaitForBootstrap);
                    EditorApplication.isPlaying = true;
                    break;
                case TestPhase.WaitForBootstrap:
                    if (EditorApplication.isPlaying && Elapsed(1.0)) Advance(TestPhase.ClickStartRun);
                    break;
                case TestPhase.ClickStartRun:
                    if (!Elapsed(0.5)) break;
                    ClickButton("TitleScreen", "START RUN");
                    Advance(TestPhase.WaitForGameBoard);
                    break;
                case TestPhase.WaitForGameBoard:
                    if (Elapsed(4.0)) Advance(TestPhase.CaptureBeforeShot);
                    break;
                case TestPhase.CaptureBeforeShot:
                    CaptureScreenshot("dart_test_before");
                    Advance(TestPhase.AttachRunner);
                    break;
                case TestPhase.AttachRunner:
                    if (!Elapsed(0.5)) break;
                    var go = new GameObject("DartPhysicsTestRunner");
                    go.AddComponent<DartPhysicsTestRunner>()
                      .StartCoroutine(go.GetComponent<DartPhysicsTestRunner>().FireSequenceWithScreenshots());
                    Advance(TestPhase.WaitForRunner);
                    break;
                case TestPhase.WaitForRunner:
                    if (DartPhysicsTestRunner.IsComplete) Advance(TestPhase.ExitPlayMode);
                    break;
                case TestPhase.ExitPlayMode:
                    if (Elapsed(0.5)) StartFinish();
                    break;
            }
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex.Message}");
            StartFinish();
        }
    }

    private static void CaptureScreenshot(string label)
    {
        string path = Inkshot.Editor.AgentBridge.ScreenshotCapture.CaptureScene(label);
        if (path != null) Log($"Screenshot: {path}");
        else Log($"Screenshot FAILED for label: {label}");
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

    private static void Advance(TestPhase next)
    {
        _phase = next;
        SessionState.SetInt(PhaseKey, (int)next);
        _phaseStart = EditorApplication.timeSinceStartup;
    }

    private static bool Elapsed(double s) => EditorApplication.timeSinceStartup - _phaseStart >= s;

    private static void StartFinish()
    {
        if (_isFinishing) return;
        _isFinishing = true;
        SessionState.SetBool(FinishKey, true);
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
        SessionState.EraseBool(ActiveKey);
        SessionState.EraseBool(FinishKey);
        SessionState.EraseInt(PhaseKey);
        Log("Dart physics test complete.");
        if (Application.isBatchMode)
            EditorApplication.delayCall += () => EditorApplication.Exit(0);
    }

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

    private static void Log(string message)
    {
        string text = $"[DartPhysicsTest] {message}";
        if (Application.isBatchMode) Console.WriteLine(text);
        else UnityEngine.Debug.Log(text);
    }
}
#endif
