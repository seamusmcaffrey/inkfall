#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public static partial class BalloonSmokeRunner
{
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
}
#endif
