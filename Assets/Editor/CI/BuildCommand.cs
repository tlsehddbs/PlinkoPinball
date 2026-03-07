using UnityEditor;
using UnityEngine;

public static class BuildCommand
{
    public static void BuildWindows()
    {
        var outDir = System.Environment.GetEnvironmentVariable("CI_OUT_DIR");
        if (string.IsNullOrEmpty(outDir)) outDir = "BuildOutput";

        // TODO: 씬 목록을 프로젝트에 맞게 수정 관리할 것
        string[] scenes = { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/MainTable.unity" };

        var options = new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = System.IO.Path.Combine(outDir, "Game.exe"),
            target = BuildTarget.StandaloneWindows64,
            options = BuildOptions.None
        };

        var report = BuildPipeline.BuildPlayer(options);
        if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
        {
            Debug.LogError("Build Failed: " + report.summary.result);
            EditorApplication.Exit(1);
        }
        EditorApplication.Exit(0);
    }
}
