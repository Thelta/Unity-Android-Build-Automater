using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Diagnostics;
using System;
using System.IO;


public class AndroidBuildOptions : EditorWindow
{
    string androidSDKPath;
    string androidProjectPath;
    int javaVersionOption = 0;

    static GUIContent[] javaVersions = null;

    static string javaPath = Path.GetFullPath(Environment.GetEnvironmentVariable("JAVA_HOME")
        + Path.DirectorySeparatorChar + "bin" + Path.DirectorySeparatorChar + "java.exe");


    [MenuItem ("File/Android Prebuild Pipeline Options")]
    public static void ShowWindow()
    {
        if(javaVersions == null)
        {
            FindJavaVersions();
        }

        EditorWindow.GetWindow(typeof(AndroidBuildOptions));
    }

    void OnGUI()
    {
        GUILayout.Label("Android Project Fields", EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();
        androidSDKPath = EditorGUILayout.TextField(new GUIContent("Android SDK File", "Path of android.jar file"), androidSDKPath);
        if (GUILayout.Button(EditorGUIUtility.FindTexture("_Popup"), EditorStyles.miniButton, GUILayout.Height(20), GUILayout.Width(20)))
        {
            androidSDKPath = EditorUtility.OpenFilePanel("Select Android SDK Path", "", "");
        }
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        androidProjectPath = EditorGUILayout.TextField(new GUIContent("Android Project Path", "Path of JAVA project"), androidProjectPath);
        if (GUILayout.Button(EditorGUIUtility.FindTexture("_Popup"), EditorStyles.miniButton, GUILayout.Height(20), GUILayout.Width(20)))
        {
            androidProjectPath = EditorUtility.OpenFolderPanel("Select Android Project Path", "", "");
        }
        GUILayout.EndHorizontal();

        if(javaVersions == null)
        {
            FindJavaVersions();
        }
        javaVersionOption = EditorGUILayout.Popup(new GUIContent("Java Version", "Select minimum requirement of Java version of your project"),
            javaVersionOption,
            javaVersions);

        if (GUILayout.Button("Save and Close", GUILayout.Height(20), GUILayout.Width(120)))
        {
            Close();
        }
    }

    void OnEnable()
    {
        if(EditorPrefs.HasKey("androidSDKPath"))
        {
            androidSDKPath = EditorPrefs.GetString("androidSDKPath");
        }

        if (EditorPrefs.HasKey("androidProjectPath"))
        {
            androidProjectPath = EditorPrefs.GetString("androidProjectPath");
        }

        if(EditorPrefs.HasKey("javaVersion"))
        {
            javaVersionOption = 18 - Mathf.RoundToInt(float.Parse(EditorPrefs.GetString("javaVersion")) * 10);
        }

    }

    void OnDisable()
    {
        EditorPrefs.SetString("androidSDKPath", androidSDKPath);
        EditorPrefs.SetString("androidProjectPath", androidProjectPath);
        EditorPrefs.SetString("javaVersion", javaVersions[javaVersionOption].text);
    }

    static void FindJavaVersions()
    {
        Process javaVerProcess = new Process();
        javaVerProcess.StartInfo.FileName = javaPath;
        javaVerProcess.StartInfo.RedirectStandardError = true;
        javaVerProcess.StartInfo.Arguments = "-version";
        javaVerProcess.StartInfo.UseShellExecute = false;
        javaVerProcess.StartInfo.CreateNoWindow = true;
        javaVerProcess.Start();
        string output = javaVerProcess.StandardError.ReadLine();
        javaVerProcess.WaitForExit();
        int ver = Mathf.RoundToInt(float.Parse(output.Substring(14, 3)) * 10);
        javaVersions = new GUIContent[ver - 15];
        for (int i = ver; i > 15; i--)
        {
            javaVersions[ver - i] = new GUIContent(((float)i / 10).ToString());
        }

    }


}
