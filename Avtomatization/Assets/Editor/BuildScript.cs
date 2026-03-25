using UnityEditor;
using UnityEngine;
using System.IO;

public class BuildScript
{
    private const string BUILD_FOLDER = "Builds";
    private const string PC_BUILD_NAME = "MyGame.exe";
    private const string ANDROID_BUILD_NAME = "MyGame.apk";

    [MenuItem("Build/Build All")]
    public static void BuildAll()
    {
        if (!Directory.Exists(BUILD_FOLDER))
        {
            Directory.CreateDirectory(BUILD_FOLDER);
        }

        BuildPlayerOptions pcOptions = new BuildPlayerOptions();
        pcOptions.scenes = GetScenes();
        pcOptions.locationPathName = Path.Combine(BUILD_FOLDER, PC_BUILD_NAME);
        pcOptions.target = BuildTarget.StandaloneWindows64;
        pcOptions.options = BuildOptions.None;

        Debug.Log("Начинается сборка для Windows...");
        BuildPipeline.BuildPlayer(pcOptions);
        Debug.Log("Сборка для Windows завершена!");

        BuildPlayerOptions androidOptions = new BuildPlayerOptions();
        androidOptions.scenes = GetScenes();
        androidOptions.locationPathName = Path.Combine(BUILD_FOLDER, ANDROID_BUILD_NAME);
        androidOptions.target = BuildTarget.Android;
        androidOptions.options = BuildOptions.None;

        Debug.Log("Начинается сборка для Android...");
        BuildPipeline.BuildPlayer(androidOptions);
        Debug.Log("Сборка для Android завершена!");

        EditorUtility.RevealInFinder(BUILD_FOLDER);
    }

    [MenuItem("Build/Build PC Only")]
    public static void BuildPC()
    {
        if (!Directory.Exists(BUILD_FOLDER))
        {
            Directory.CreateDirectory(BUILD_FOLDER);
        }

        BuildPlayerOptions pcOptions = new BuildPlayerOptions();
        pcOptions.scenes = GetScenes();
        pcOptions.locationPathName = Path.Combine(BUILD_FOLDER, PC_BUILD_NAME);
        pcOptions.target = BuildTarget.StandaloneWindows64;
        pcOptions.options = BuildOptions.None;

        Debug.Log("Начинается сборка для Windows...");
        BuildPipeline.BuildPlayer(pcOptions);
        Debug.Log("Сборка для Windows завершена!");
        EditorUtility.RevealInFinder(BUILD_FOLDER);
    }

    [MenuItem("Build/Build Android Only")]
    public static void BuildAndroid()
    {
        if (!Directory.Exists(BUILD_FOLDER))
        {
            Directory.CreateDirectory(BUILD_FOLDER);
        }

        BuildPlayerOptions androidOptions = new BuildPlayerOptions();
        androidOptions.scenes = GetScenes();
        androidOptions.locationPathName = Path.Combine(BUILD_FOLDER, ANDROID_BUILD_NAME);
        androidOptions.target = BuildTarget.Android;
        androidOptions.options = BuildOptions.None;

        Debug.Log("Начинается сборка для Android...");
        BuildPipeline.BuildPlayer(androidOptions);
        Debug.Log("Сборка для Android завершена!");
        EditorUtility.RevealInFinder(BUILD_FOLDER);
    }

    private static string[] GetScenes()
    {
        EditorBuildSettingsScene[] scenes = EditorBuildSettings.scenes;
        string[] scenePaths = new string[scenes.Length];

        for (int i = 0; i < scenes.Length; i++)
        {
            scenePaths[i] = scenes[i].path;
        }

        return scenePaths;
    }

#if UNITY_WEBGL
using UnityEditor;
using UnityEngine;
using System.IO;

public partial class BuildScript
{
    private const string WEBGL_BUILD_FOLDER = "Builds/WebGL";
    
    [MenuItem("Build/Build WebGL")]
    public static void BuildWebGL()
    {
        if (!Directory.Exists(WEBGL_BUILD_FOLDER))
        {
            Directory.CreateDirectory(WEBGL_BUILD_FOLDER);
        }
        
        BuildPlayerOptions webglOptions = new BuildPlayerOptions();
        webglOptions.scenes = GetScenes();
        webglOptions.locationPathName = WEBGL_BUILD_FOLDER;
        webglOptions.target = BuildTarget.WebGL;
        webglOptions.options = BuildOptions.None;
        
        Debug.Log("Начинается WebGL сборка с кастомным шаблоном...");
        BuildPipeline.BuildPlayer(webglOptions);
        Debug.Log("WebGL сборка завершена! Сборка сохранена в: " + WEBGL_BUILD_FOLDER);
        
        EditorUtility.RevealInFinder(WEBGL_BUILD_FOLDER);
    }
    
    [MenuItem("Build/Build All Platforms")]
    public static void BuildAllPlatforms()
    {
        BuildPC();
        BuildAndroid();
        BuildWebGL();
        Debug.Log("Все сборки завершены!");
    }
}
#endif
}