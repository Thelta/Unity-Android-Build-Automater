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

        Process javac = new Process();
        javac.StartInfo.FileName = javacPath;
        javac.StartInfo.RedirectStandardError = true;
        javac.StartInfo.UseShellExecute = false;
        javac.StartInfo.CreateNoWindow = true;
        javac.StartInfo.Arguments = BuildJavacArguments(tempPath);
        javac.Start();
        string output = javac.StandardError.ReadToEnd();
        javac.WaitForExit();    

        if(output.IndexOf(" error\n") >= 0 || output.IndexOf(" error\0") >= 0)
        {
            UnityEngine.Debug.LogException(new JavacCompileErrorException(output));
            throw new JavacCompileErrorException("This shouldn't have happened.javac compiling failed.See output of javac.");
        }

        string pluginFolder = Path.Combine(Path.Combine(Application.dataPath, "Plugins"), "Android");
        Directory.CreateDirectory(pluginFolder);

        Process jar = new Process();
        jar.StartInfo.FileName = jarPath;
        jar.StartInfo.RedirectStandardError = true;
        jar.StartInfo.UseShellExecute = false;
        jar.StartInfo.CreateNoWindow = true;
        jar.StartInfo.Arguments = BuildJarArguments(tempPath, pluginFolder);
        jar.Start();
        output = jar.StandardError.ReadToEnd();
        jar.WaitForExit();

        UnityEngine.Debug.Log(BuildJarArguments(tempPath, pluginFolder));

        if (output.IndexOf("Exception") > 0 || output.IndexOf("no such file or directory") > 0)
        {
            UnityEngine.Debug.LogException(new JarFailedException(output));
            throw new JarFailedException("This shouldn't have happened.jar building failed.See output");
        }



    }

    static string BuildJarArguments(string tempPath, string pluginFolder)
    {
        ArgumentBuilder builder = new ArgumentBuilder();
        builder.AddArgument("cf");

        builder.AddArgument(Path.Combine(pluginFolder, "AndroidPlugin.jar"));

        string[] allClass = Directory.GetFiles(tempPath, "*.class", SearchOption.AllDirectories);
        for (int i = 0; i < allClass.Length; i++)
        {
            builder.AddArgument(allClass[i]);
        }

        return builder.ToString();
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
