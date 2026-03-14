#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// Headless play-mode smoke test for the Inkshot bootstrap flow.
/// </summary>
[InitializeOnLoad]
public static class BalloonSmokeRunner
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

    private static void Update()
    {
        try
        {
            switch (_phase)
            {
                case SmokePhase.EnterPlayMode:
                    if (!EditorApplication.isPlayingOrWillChangePlaymode)
                    {
                        SetPhase(SmokePhase.WaitForBootstrap);
                        EditorApplication.isPlaying = true;
                        _phaseStartedAt = EditorApplication.timeSinceStartup;
                    }
                    break;

                case SmokePhase.WaitForBootstrap:
                    if (EditorApplication.isPlaying && HasElapsed(0.75d))
                    {
                        RequireComponent<TitleScreen>("TitleScreen");
                        RequireComponent<SettingsPanel>("SettingsPanel");
                        SetPhase(SmokePhase.OpenSettings);
                        _phaseStartedAt = EditorApplication.timeSinceStartup;
                    }
                    break;

                case SmokePhase.OpenSettings:
                    ClickButton("TitleScreen", "SETTINGS");
                    SetPhase(SmokePhase.AdjustSettings);
                    _phaseStartedAt = EditorApplication.timeSinceStartup;
                    break;

                case SmokePhase.AdjustSettings:
                    if (HasElapsed(0.25d))
                    {
                        SettingsPanel settingsPanel = RequireComponent<SettingsPanel>("SettingsPanel");
                        Slider[] sliders = settingsPanel.GetComponentsInChildren<Slider>(true);
                        Toggle toggle = settingsPanel.GetComponentInChildren<Toggle>(true);

                        if (sliders.Length == 0 || toggle == null)
                        {
                            throw new InvalidOperationException("SettingsPanel did not build its controls.");
                        }

                        sliders[0].value = Mathf.Clamp01(sliders[0].value - 0.1f);
                        toggle.isOn = !toggle.isOn;
                        ClickButton("SettingsPanel", "Close");

                        SetPhase(SmokePhase.TriggerLaunchFeel);
                        _phaseStartedAt = EditorApplication.timeSinceStartup;
                    }
                    break;

                case SmokePhase.TriggerLaunchFeel:
                    EventBus.Publish(new DartLaunchedEvent
                    {
                        LaunchVelocity = Vector3.up * 12f,
                        PullStrength = 0.5f,
                    });

                    SetPhase(SmokePhase.StartRun);
                    _phaseStartedAt = EditorApplication.timeSinceStartup;
                    break;

                case SmokePhase.StartRun:
                    if (HasElapsed(0.2d))
                    {
                        ClickButton("TitleScreen", "START RUN");
                        SetPhase(SmokePhase.WaitForRunBootstrap);
                        _phaseStartedAt = EditorApplication.timeSinceStartup;
                    }
                    break;

                case SmokePhase.WaitForRunBootstrap:
                    if (HasElapsed(3.6d))
                    {
                        RunManager runManager = RequireComponent<RunManager>("GameManager");
                        BalloonGameManager balloonGameManager = RequireComponent<BalloonGameManager>("GameManager");

                        if (runManager.CurrentState != RunState.InRoom)
                        {
                            throw new InvalidOperationException($"RunManager did not reach InRoom. Current state: {runManager.CurrentState}");
                        }

                        if (balloonGameManager.CurrentState != BalloonGameManager.GameState.Ready)
                        {
                            throw new InvalidOperationException($"BalloonGameManager did not reach Ready. Current state: {balloonGameManager.CurrentState}");
                        }

                        StartFinish(0);
                    }
                    break;

                case SmokePhase.ExitPlayMode:
                    break;
            }
        }
        catch (Exception exception)
        {
            RecordFailure(exception.ToString());
        }
    }

    private static bool HasElapsed(double seconds)
    {
        return EditorApplication.timeSinceStartup - _phaseStartedAt >= seconds;
    }

    private static void ClickButton(string rootObjectName, string buttonName)
    {
        GameObject root = FindNamedObject(rootObjectName);
        if (root == null)
        {
            throw new InvalidOperationException($"Could not find root object '{rootObjectName}'.");
        }

        foreach (Button button in root.GetComponentsInChildren<Button>(true))
        {
            if (button.name == buttonName)
            {
                button.onClick.Invoke();
                return;
            }
        }

        throw new InvalidOperationException($"Could not find button '{buttonName}' under '{rootObjectName}'.");
    }

    private static T RequireComponent<T>(string objectName) where T : Component
    {
        GameObject root = FindNamedObject(objectName);
        if (root == null)
        {
            throw new InvalidOperationException($"Could not find object '{objectName}' in play mode.");
        }

        T component = root.GetComponent<T>();
        if (component == null)
        {
            throw new InvalidOperationException($"Object '{objectName}' is missing component '{typeof(T).Name}'.");
        }

        return component;
    }

    private static void HandleLog(string condition, string stackTrace, LogType type)
    {
        if (!EditorApplication.isPlaying)
        {
            return;
        }

        if (type != LogType.Error && type != LogType.Assert && type != LogType.Exception)
        {
            return;
        }

        AppendError($"{type}: {condition}\n{stackTrace}".Trim());
    }

    private static void HandlePlayModeChanged(PlayModeStateChange change)
    {
        if (change == PlayModeStateChange.EnteredEditMode && _isFinishing)
        {
            FinishAndExit();
        }
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

    private static GameObject FindNamedObject(string objectName)
    {
        Scene activeScene = SceneManager.GetActiveScene();
        if (!activeScene.IsValid())
        {
            return null;
        }

        foreach (GameObject root in activeScene.GetRootGameObjects())
        {
            GameObject match = FindNamedObjectRecursive(root.transform, objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }

    private static GameObject FindNamedObjectRecursive(Transform current, string objectName)
    {
        if (current.name == objectName)
        {
            return current.gameObject;
        }

        for (int childIndex = 0; childIndex < current.childCount; childIndex++)
        {
            GameObject match = FindNamedObjectRecursive(current.GetChild(childIndex), objectName);
            if (match != null)
            {
                return match;
            }
        }

        return null;
    }
}
#endif
