using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

public static class BuildMobile
{
    static string[] GetEnabledScenes()
    {
        return EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
    }

    [MenuItem("Build/Mobile/Build Android APK")]
    public static void BuildAndroidAPK()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = false;

        string outDir = "Builds/Android";
        Directory.CreateDirectory(outDir);
        string outPath = Path.Combine(outDir, "game.apk");

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = outPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log($"Android APK built: {outPath}");
        else
            Debug.LogError($"Android build failed: {report.summary.result}");
    }

    [MenuItem("Build/Mobile/Build Android AAB")]
    public static void BuildAndroidAAB()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        EditorUserBuildSettings.buildAppBundle = true;

        string outDir = "Builds/Android";
        Directory.CreateDirectory(outDir);
        string outPath = Path.Combine(outDir, "game.aab");

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = outPath,
            target = BuildTarget.Android,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log($"Android AAB built: {outPath}");
        else
            Debug.LogError($"Android build failed: {report.summary.result}");
    }

    [MenuItem("Build/Mobile/Build iOS Xcode Project")]
    public static void BuildiOS()
    {
        EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

        string outDir = "Builds/iOS";
        Directory.CreateDirectory(outDir);

        BuildPlayerOptions opts = new BuildPlayerOptions
        {
            scenes = GetEnabledScenes(),
            locationPathName = outDir,
            target = BuildTarget.iOS,
            options = BuildOptions.None
        };

        BuildReport report = BuildPipeline.BuildPlayer(opts);
        if (report.summary.result == BuildResult.Succeeded)
            Debug.Log($"iOS Xcode project exported to: {outDir}");
        else
            Debug.LogError($"iOS build failed: {report.summary.result}");
    }
}
