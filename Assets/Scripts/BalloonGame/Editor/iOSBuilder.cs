#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

/// <summary>
/// iOS build automation for device and simulator targets.
/// Menu items under Build/ and a CLI entry point via -executeMethod.
/// </summary>
public static class iOSBuilder
{
    private const string BundleId = "com.seamuslawless.inkshot";
    private const string MainScenePath = "Assets/Scenes/InkshotScene.unity";
    private const string OutputDirectory = "Builds/iOS";
    private const string MinimumIOSVersion = "16.0";
    private const string DevelopmentTeamId = "28864BA964";
    private const int CliFailureExitCode = 1;

    [MenuItem("Build/iOS Development")]
    public static void BuildDevelopment()
    {
        var options = BuildOptions.Development
                    | BuildOptions.ConnectWithProfiler
                    | BuildOptions.AllowDebugging;

        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;

        string outputPath = Path.Combine(OutputDirectory, "InkshotDev");
        BuildiOS(options, outputPath);
    }

    [MenuItem("Build/iOS Simulator")]
    public static void BuildSimulator()
    {
        PlayerSettings.iOS.sdkVersion = iOSSdkVersion.SimulatorSDK;

        string outputPath = Path.Combine(OutputDirectory, "InkshotSimulator");
        BuildiOS(BuildOptions.Development, outputPath);
    }

    /// <summary>
    /// CLI entry point: invoke via Unity -executeMethod iOSBuilder.CommandLineBuild.
    /// Pass -simulatorBuild to target the simulator instead of a device.
    /// </summary>
    public static void CommandLineBuild()
    {
        string[] args = Environment.GetCommandLineArgs();
        bool isSimulator = args.Contains("-simulatorBuild");

        if (isSimulator)
        {
            Debug.Log("[iOSBuilder] CLI: building for simulator");
            BuildSimulator();
        }
        else
        {
            Debug.Log("[iOSBuilder] CLI: building for device (development)");
            BuildDevelopment();
        }
    }

    private static void ConfigurePlayerSettings()
    {
        PlayerSettings.SetApplicationIdentifier(
            NamedBuildTarget.iOS, BundleId);

        PlayerSettings.SetScriptingBackend(
            NamedBuildTarget.iOS, ScriptingImplementation.IL2CPP);

        PlayerSettings.iOS.appleEnableAutomaticSigning = true;
        PlayerSettings.iOS.appleDeveloperTeamID = DevelopmentTeamId;
        PlayerSettings.iOS.targetOSVersionString = MinimumIOSVersion;
    }

    private static void BuildiOS(BuildOptions options, string outputPath)
    {
        ConfigurePlayerSettings();
        EnsureSceneInBuildSettings();
        EnsureOutputDirectory(outputPath);

        var buildPlayerOptions = new BuildPlayerOptions
        {
            scenes = new[] { MainScenePath },
            locationPathName = outputPath,
            target = BuildTarget.iOS,
            options = options,
        };

        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
        BuildSummary summary = report.summary;

        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log($"[iOSBuilder] Build succeeded: {summary.totalSize} bytes, "
                    + $"output at {outputPath}");
        }
        else
        {
            string message = $"Build failed with {summary.totalErrors} error(s). "
                           + $"Result: {summary.result}";

            Debug.LogError($"[iOSBuilder] {message}");

            if (Application.isBatchMode)
            {
                EditorApplication.Exit(CliFailureExitCode);
            }
            else
            {
                EditorUtility.DisplayDialog("iOS Build Failed", message, "OK");
            }
        }
    }

    private static void EnsureSceneInBuildSettings()
    {
        var currentScenes = EditorBuildSettings.scenes;
        bool scenePresent = currentScenes.Any(
            s => s.path == MainScenePath && s.enabled);

        if (scenePresent) return;

        var sceneEntry = new EditorBuildSettingsScene(MainScenePath, true);
        var updatedScenes = currentScenes
            .Where(s => s.path != MainScenePath)
            .Prepend(sceneEntry)
            .ToArray();

        EditorBuildSettings.scenes = updatedScenes;
        Debug.Log($"[iOSBuilder] Added {MainScenePath} to build settings");
    }

    private static void EnsureOutputDirectory(string path)
    {
        string directory = Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
    }
}
#endif
