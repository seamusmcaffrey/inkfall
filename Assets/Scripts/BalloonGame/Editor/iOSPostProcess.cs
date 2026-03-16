#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

/// <summary>
/// Post-process build step for iOS: configures Xcode project settings
/// (code signing, bitcode, deployment target) and Info.plist values.
/// </summary>
public static class iOSPostProcess
{
    private const int PostProcessOrder = 100;
    private const string DeploymentTarget = "16.0";
    private const string CodeSignStyle = "Automatic";
    private const string BitcodeDisabled = "NO";

    private const string DevelopmentTeamId = "28864BA964";
    private const string XcodePropDevelopmentTeam = "DEVELOPMENT_TEAM";
    private const string XcodePropCodeSignStyle = "CODE_SIGN_STYLE";
    private const string XcodePropBitcode = "ENABLE_BITCODE";
    private const string XcodePropDeploymentTarget = "IPHONEOS_DEPLOYMENT_TARGET";
    private const string PlistKeyMinOSVersion = "MinimumOSVersion";
    private const string PlistKeyRequiresFullScreen = "UIRequiresFullScreen";

    [PostProcessBuild(PostProcessOrder)]
    public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
    {
        if (target != BuildTarget.iOS)
            return;

        ConfigureXcodeProject(pathToBuiltProject);
        ConfigureInfoPlist(pathToBuiltProject);
    }

    private static void ConfigureXcodeProject(string projectPath)
    {
        string pbxPath = PBXProject.GetPBXProjectPath(projectPath);
        var project = new PBXProject();
        project.ReadFromFile(pbxPath);

        string mainTargetGuid = project.GetUnityMainTargetGuid();
        string frameworkTargetGuid = project.GetUnityFrameworkTargetGuid();

        ApplyTargetSettings(project, mainTargetGuid);
        ApplyTargetSettings(project, frameworkTargetGuid);

        project.WriteToFile(pbxPath);
        Debug.Log("[iOSPostProcess] Xcode project settings applied.");
    }

    private static void ApplyTargetSettings(PBXProject project, string targetGuid)
    {
        project.SetBuildProperty(targetGuid, XcodePropDevelopmentTeam, DevelopmentTeamId);
        project.SetBuildProperty(targetGuid, XcodePropCodeSignStyle, CodeSignStyle);
        project.SetBuildProperty(targetGuid, XcodePropBitcode, BitcodeDisabled);
        project.SetBuildProperty(targetGuid, XcodePropDeploymentTarget, DeploymentTarget);
    }

    private static void ConfigureInfoPlist(string projectPath)
    {
        string plistPath = Path.Combine(projectPath, "Info.plist");
        var plist = new PlistDocument();
        plist.ReadFromFile(plistPath);

        PlistElementDict root = plist.root;
        root.SetString(PlistKeyMinOSVersion, DeploymentTarget);
        root.SetBoolean(PlistKeyRequiresFullScreen, true);

        plist.WriteToFile(plistPath);
        Debug.Log("[iOSPostProcess] Info.plist settings applied.");
    }
}
#endif
