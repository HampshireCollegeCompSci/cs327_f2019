using UnityEngine;
using UnityEditor;
using System.IO;
using UnityEditor.Build.Reporting;

public class ComboBuild
{
    //This creates a menu item to trigger the dual builds https://docs.unity3d.com/ScriptReference/MenuItem.html
    // Uncheck the "Clear on Build" and "Clear on Recompile" option in the Console's Clear tab in order to see all the debug logs

    [MenuItem("Game Build Menu/Dual Build")]
    public static void BuildGame()
    {
        //This builds the player twice: a build with desktop-specific texture settings (WebGL_Build) as well as mobile-specific texture settings (WebGL_Mobile), and combines the necessary files into one directory (WebGL_Build)

        string dualBuildPath = "WebGLBuilds";
        string desktopBuildName = "WebGL_Build";
        string mobileBuildName = "WebGL_Mobile";

        string desktopPath = Path.Combine(dualBuildPath, desktopBuildName);
        string mobilePath = Path.Combine(dualBuildPath, mobileBuildName);
        string[] scenes = new string[] {
            "Assets/Scenes/MainMenuScene.unity",
            "Assets/Scenes/GameplayScene.unity",
            "Assets/Scenes/SummaryScene.unity",
            "Assets/Scenes/PauseScene.unity",
            "Assets/Scenes/SettingsScene.unity",
            "Assets/Scenes/AboutScene.unity",
            "Assets/Scenes/AchievementsScene.unity",
            "Assets/Scenes/StatsScene.unity"
        };

        BuildPlayerOptions buildPlayerOptionsDesktop = new()
        {
            scenes = scenes,
            locationPathName = desktopPath,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            subtarget = (int)WebGLTextureSubtarget.DXT,
            options = BuildOptions.None
        };
        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.DXT;

        Debug.Log("Building WebGL Desktop");
        BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptionsDesktop);
        BuildSummary summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
            return;
        }

        BuildPlayerOptions buildPlayerOptionsMobile = new()
        {
            scenes = scenes,
            locationPathName = mobilePath,
            target = BuildTarget.WebGL,
            targetGroup = BuildTargetGroup.WebGL,
            subtarget = (int)WebGLTextureSubtarget.ASTC,
            options = BuildOptions.None
        };
        EditorUserBuildSettings.webGLBuildSubtarget = WebGLTextureSubtarget.ASTC;

        Debug.Log("Building WebGL Mobile");
        report = BuildPipeline.BuildPlayer(buildPlayerOptionsMobile);
        summary = report.summary;
        if (summary.result == BuildResult.Succeeded)
        {
            Debug.Log("Build succeeded: " + summary.totalSize + " bytes");
        }
        else if (summary.result == BuildResult.Failed)
        {
            Debug.Log("Build failed");
            return;
        }

        string newMobileDataPath = Path.Combine(desktopPath, "Build", mobileBuildName + ".data");
        if (File.Exists(newMobileDataPath))
        {
            Debug.Log("Deleting old mobile data");
            File.Delete(newMobileDataPath);
        }

        // Copy the mobile.data file to the desktop build directory to consolidate them both
        FileUtil.CopyFileOrDirectory(Path.Combine(mobilePath, "Build", mobileBuildName + ".data"), newMobileDataPath);
    }
}
