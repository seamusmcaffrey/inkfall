#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>
/// Headless play-mode smoke test for the Inkshot bootstrap flow.
/// </summary>
[InitializeOnLoad]
public static partial class BalloonSmokeRunner
{
    private const string ScenePath = "Assets/Scenes/InkshotScene.unity";
    private const string ActiveKey = "Inkshot.Smoke.Active";
    private const string PhaseKey = "Inkshot.Smoke.Phase";
    private const string FinishingKey = "Inkshot.Smoke.Finishing";
    private const string ExitCodeKey = "Inkshot.Smoke.ExitCode";
    private const string ErrorsKey = "Inkshot.Smoke.Errors";

    private enum SmokePhase
    {
        EnterPlayMode,
        WaitForBootstrap,
        OpenSettings,
        AdjustSettings,
        TriggerLaunchFeel,
        StartRun,
        WaitForRunBootstrap,
        ExitPlayMode,
    }

    private static readonly List<string> Errors = new();

    private static SmokePhase _phase;
    private static double _phaseStartedAt;
    private static bool _isFinishing;
    private static int _exitCode;

    static BalloonSmokeRunner()
    {
        RestoreStateIfNeeded();
    }

    public static void Run()
    {
        Errors.Clear();
        SessionState.SetBool(ActiveKey, true);
        SessionState.SetBool(FinishingKey, false);
        SessionState.SetInt(ExitCodeKey, 0);
        SessionState.SetString(ErrorsKey, string.Empty);
        SetPhase(SmokePhase.EnterPlayMode);
        _phaseStartedAt = EditorApplication.timeSinceStartup;
        _isFinishing = false;
        _exitCode = 0;

        AttachCallbacks();

        EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
    }

    private static bool HasElapsed(double seconds)
    {
        return EditorApplication.timeSinceStartup - _phaseStartedAt >= seconds;
    }

    private static void StartFinish(int exitCode)
    {
        if (_isFinishing)
        {
            return;
        }

        _isFinishing = true;
        SessionState.SetBool(FinishingKey, true);
        _exitCode = exitCode;
        SessionState.SetInt(ExitCodeKey, exitCode);
        SetPhase(SmokePhase.ExitPlayMode);

        if (EditorApplication.isPlaying)
        {
            EditorApplication.isPlaying = false;
        }
        else
        {
            FinishAndExit();
        }
    }

    private static void RecordFailure(string message)
    {
        AppendError(message);
        StartFinish(1);
    }

    private static void FinishAndExit()
    {
        DetachCallbacks();

        LoadErrors();
        SessionState.EraseBool(ActiveKey);
        SessionState.EraseBool(FinishingKey);
        SessionState.EraseInt(ExitCodeKey);
        SessionState.EraseInt(PhaseKey);
        SessionState.EraseString(ErrorsKey);

        if (Errors.Count > 0)
        {
            foreach (string error in Errors)
            {
                Debug.LogError(error);
            }
        }

        EditorApplication.delayCall += () => EditorApplication.Exit(_exitCode);
    }

    private static void RestoreStateIfNeeded()
    {
        if (!SessionState.GetBool(ActiveKey, false))
        {
            return;
        }

        _phase = (SmokePhase)SessionState.GetInt(PhaseKey, 0);
        _isFinishing = SessionState.GetBool(FinishingKey, false);
        _exitCode = SessionState.GetInt(ExitCodeKey, 0);
        _phaseStartedAt = EditorApplication.timeSinceStartup;

        AttachCallbacks();

        if (_isFinishing && !EditorApplication.isPlaying && !EditorApplication.isPlayingOrWillChangePlaymode)
        {
            FinishAndExit();
        }
    }

    private static void AttachCallbacks()
    {
        DetachCallbacks();
        Application.logMessageReceived += HandleLog;
        EditorApplication.playModeStateChanged += HandlePlayModeChanged;
        EditorApplication.update += Update;
    }

    private static void DetachCallbacks()
    {
        Application.logMessageReceived -= HandleLog;
        EditorApplication.playModeStateChanged -= HandlePlayModeChanged;
        EditorApplication.update -= Update;
    }

    private static void SetPhase(SmokePhase phase)
    {
        _phase = phase;
        SessionState.SetInt(PhaseKey, (int)phase);
    }

    private static void AppendError(string message)
    {
        LoadErrors();
        Errors.Add(message);
        SessionState.SetString(ErrorsKey, string.Join("\n---\n", Errors));
    }

    private static void LoadErrors()
    {
        if (Errors.Count > 0)
        {
            return;
        }

        string stored = SessionState.GetString(ErrorsKey, string.Empty);
        if (string.IsNullOrWhiteSpace(stored))
        {
            return;
        }

        string[] messages = stored.Split(new[] { "\n---\n" }, StringSplitOptions.RemoveEmptyEntries);
        foreach (string message in messages)
        {
            Errors.Add(message);
        }
    }
}
#endif
