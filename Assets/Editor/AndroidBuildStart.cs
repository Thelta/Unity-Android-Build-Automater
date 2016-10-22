using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;


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

        //TODO : Wipe all .class files from temp.
        Directory.CreateDirectory(tempPath);

        //builds .class files from android project
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

        //TODO : Manage old plugin folder.
        string pluginFolder = Path.Combine(Path.Combine(Application.dataPath, "Plugins"), "Android");
        Directory.CreateDirectory(pluginFolder);

        //creates jar file from .class files
        Process jar = new Process();
        jar.StartInfo.FileName = jarPath;
        jar.StartInfo.RedirectStandardError = true;
        jar.StartInfo.UseShellExecute = false;
        jar.StartInfo.CreateNoWindow = true;
        jar.StartInfo.Arguments = BuildJarBuildArgument(tempPath, pluginFolder);
        jar.Start();
        output = jar.StandardError.ReadToEnd();
        jar.WaitForExit();

        if (output.IndexOf("Exception") > 0 || output.IndexOf("no such file or directory") > 0)
        {
            UnityEngine.Debug.LogException(new JarFailedException(output));
            throw new JarFailedException("This shouldn't have happened.jar building failed.See output");
        }

        //copy libs to plugins folder except class.jar
        string appPath = Path.Combine(androidProjectPath, "app");
        string[] allJars = Directory.GetFiles(Path.Combine(appPath, "libs"), "*.jar", SearchOption.AllDirectories);
        List<string> jarFilenames = new List<string>(allJars.Length + 1);
        
        for(int i = 0; i < allJars.Length; i++)
        {
            string filename = allJars[i].Substring(allJars[i].LastIndexOf('\\') + 1);
            if(filename != "classes.jar")
            {
                File.Copy(allJars[i], Path.Combine(pluginFolder, filename));
                jarFilenames.Add(filename);
            }
        }

        jarFilenames.Add("AndroidPlugin.jar");

        //control .class files in .jar files, so it doesn't create dex error.
        List<KeyValuePair<string, List<string>>> classNames = new List<KeyValuePair<string, List<string>>>();
        for(int i = 0; i < jarFilenames.Count; i++)
        {
            classNames.Add(new KeyValuePair<string, List<string>>(
                jarFilenames[i], ZipFilenameExtractor.GetNames(Path.Combine(pluginFolder, jarFilenames[i]))));
        }

        bool haveDuplicate = false;

        for(int i = 0; i < classNames.Count - 1; i++)
        {
            for(int j = i + 1; j < classNames.Count; j++)
            {
                string[] duplicate = classNames[i].Value.Intersect(classNames[j].Value).ToArray();
                if(duplicate.Length > 0)
                {
                    haveDuplicate = true;
                    for(int k = 0; i < duplicate.Length; i++)
                    {
                        UnityEngine.Debug.LogError(new HaveDuplicateClassFile(
                            "Have duplicate files in") + classNames[i].Key + " and " + classNames[j] + " named " + duplicate[k]);
                    }
                }
            }
        }
        if(haveDuplicate)
        {
            throw new HaveDuplicateClassFile();
        }


    }

    static string BuildJarBuildArgument(string tempPath, string pluginFolder)
    {
        ArgumentBuilder builder = new ArgumentBuilder();
        builder.AddArgument("cf");

        builder.AddArgument(Path.Combine(pluginFolder, "AndroidPlugin.jar"));

        builder.AddArgument("-C", tempPath);

        builder.AddArgument(".");

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
