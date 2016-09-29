using UnityEngine;
using System.Collections;
using UnityEditor;


public class AndroidBuildOptions : EditorWindow
{
    string androidSDKPath;
    string androidProjectPath;


    [MenuItem ("File/Android Prebuild Pipeline Options")]
    public static void ShowWindow()
    {
        EditorWindow.GetWindow(typeof(AndroidBuildOptions));
    }

    void OnGUI()
    {
        GUILayout.Label("Android Project Fields", EditorStyles.boldLabel);

        androidSDKPath = EditorGUILayout.TextField(new GUIContent("Android SDK Path", "Path of android.jar file"), androidSDKPath);
        androidProjectPath = EditorGUILayout.TextField(new GUIContent("Android Project Path", "Path of JAVA project"), androidProjectPath);
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

    }

    void OnDisable()
    {
        EditorPrefs.SetString("androidSDKPath", androidSDKPath);
        EditorPrefs.SetString("androidProjectPath", androidProjectPath);
    }


}
