using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Diagnostics;
using System;
using System.IO;
using System.Text;


public class AndroidBuildStart
{
    static string androidSDKPath;
    static string androidProjectPath;
    static string javaVersion;

    static string javaHome =  Path.Combine(Environment.GetEnvironmentVariable("JAVA_HOME"), "bin");
    static string javacPath = Path.Combine(javaHome, "javac.exe");
    static string jarPath = Path.Combine(javaHome, "jar.exe");

    [MenuItem("File/Android Prebuild Start")]
    static void StartBuild()
    {
        GetStartingValues();

        string tempPath = Path.Combine(Path.GetTempPath(),
            "UnityPrebuilder" + Path.DirectorySeparatorChar + GetProjectName());

        Directory.CreateDirectory(tempPath);

        UnityEngine.Debug.Log(BuildJavacArguments(tempPath));


        Process javac = new Process();
        javac.StartInfo.FileName = javacPath;
        javac.StartInfo.RedirectStandardError = true;
        javac.StartInfo.UseShellExecute = false;
        javac.StartInfo.CreateNoWindow = true;
        javac.StartInfo.Arguments = BuildJavacArguments(tempPath);
        javac.Start();
        string output = javac.StandardError.ReadToEnd();
        javac.WaitForExit();

        if(output.IndexOf(" error\n") < 0)
        {
            UnityEngine.Debug.Log(output);
            throw new Exception("I am Error");
        }
    }

    static string BuildJavacArguments(string tempPath)
    {
        ArgumentBuilder builder = new ArgumentBuilder();
        builder.AddArgument("-source", javaVersion);
        builder.AddArgument("-target", javaVersion);
        builder.AddArgument("-verbose");

        builder.AddArgument("-d", tempPath);

        string appPath = Path.Combine(androidProjectPath, "app");
        string[] allJars = Directory.GetFiles(Path.Combine(appPath, "libs"), "*.*", SearchOption.AllDirectories);
        Array.Resize<string>(ref allJars, allJars.Length + 1);
        allJars[allJars.Length - 1] = androidSDKPath;
        builder.AddArgument("-classpath", allJars);

        string sourcePath = appPath + Path.DirectorySeparatorChar + "src"
            + Path.DirectorySeparatorChar + "main"
            + Path.DirectorySeparatorChar + "java";
        string[] allJavas = Directory.GetFiles(sourcePath, "*.java", SearchOption.AllDirectories);
        builder.AddArgument("", allJavas);


        return builder.ToString();
    }

    static string GetProjectName()
    {
        string[] s = Application.dataPath.Split('/');
        UnityEngine.Debug.Log(Application.dataPath);

        string projectName = s[s.Length - 2];
        return projectName;
    }

    static void GetStartingValues()
    {
        if (EditorPrefs.HasKey("androidSDKPath"))
        {
            androidSDKPath = EditorPrefs.GetString("androidSDKPath");
        }
        else
        {
            throw new OptionsNotFoundException("Android SDK PAth not found.");
        }

        if (EditorPrefs.HasKey("androidProjectPath"))
        {
            androidProjectPath = EditorPrefs.GetString("androidProjectPath");
        }
        else
        {
            throw new OptionsNotFoundException("Android Project Path not found.");
        }

        if (EditorPrefs.HasKey("javaVersion"))
        {
            javaVersion = EditorPrefs.GetString("javaVersion");
        }
        else
        {
            throw new OptionsNotFoundException("Selected Java version not found.");
        }
    }
}
