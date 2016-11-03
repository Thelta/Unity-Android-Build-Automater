using UnityEngine;
using System.Collections.Generic;
using UnityEditor;
using System.Diagnostics;
using System;
using System.IO;
using System.Linq;
using System.Xml;


public class AndroidBuildStart
{
    static string androidSDKPath;
    static string androidProjectPath;
    static string javaVersion;

    static string javaHome =  Path.Combine(Environment.GetEnvironmentVariable("JAVA_HOME"), "bin");
    static string javacPath = Path.Combine(javaHome, "javac");
    static string jarPath = Path.Combine(javaHome, "jar");

    [MenuItem("File/Android Prebuild Start")]
    static void StartBuild()
    {
        EditorUtility.DisplayProgressBar("Android Prebuild", "We are starting.", 0);
        GetStartingValues();

        string tempPath = Path.Combine(Path.GetTempPath(),
            "UnityPrebuilder" + Path.DirectorySeparatorChar + GetProjectName());

        if (Directory.Exists(tempPath))
        {
            Directory.Delete(tempPath, true);
        }
        Directory.CreateDirectory(tempPath);


        //builds .class files from android project
        EditorUtility.DisplayProgressBar("Android Prebuild", "Building .class files.", 0.11f);

        Process javac = new Process();
        javac.StartInfo.FileName = javacPath;
        javac.StartInfo.RedirectStandardError = true;
        javac.StartInfo.UseShellExecute = false;
        javac.StartInfo.CreateNoWindow = true;
        javac.StartInfo.Arguments = BuildJavacArguments(tempPath);
        javac.Start();
        string output = javac.StandardError.ReadToEnd();
        javac.WaitForExit();

        if (output.IndexOf(" error" + Environment.NewLine) >= 0 || output.IndexOf(" errors" + Environment.NewLine) >= 0)
        {
            UnityEngine.Debug.LogException(new JavacCompileErrorException(output));
            throw new JavacCompileErrorException("This shouldn't have happened.javac compiling failed.See output of javac.");
        }

        string pluginFolder = Path.Combine(Path.Combine(Application.dataPath, "Plugins"), "Android");
        if(Directory.Exists(pluginFolder))
        {
            string oldPluginFolder = pluginFolder + "_old";
            if(Directory.Exists(oldPluginFolder))
            {
                Directory.Delete(oldPluginFolder, true);
            }
            Directory.Move(pluginFolder, oldPluginFolder);
        }
        Directory.CreateDirectory(pluginFolder);

        //creates jar file from .class files
        EditorUtility.DisplayProgressBar("Android Prebuild", "Creating jar files.", 0.33f);

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
        EditorUtility.DisplayProgressBar("Android Prebuild", "Populating plugins folder with jars.", 0.44f);

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
        EditorUtility.DisplayProgressBar("Android Prebuild", "Controlling jar files.", 0.48f);

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
                if (duplicate.Length > 0)
                {
                    for(int k = 0; k < duplicate.Length; k++)
                    {
                        string extension = duplicate[k].Substring(duplicate[k].LastIndexOf('.'));
                        if(extension == ".class")
                        {
                            haveDuplicate = true;
                            UnityEngine.Debug.LogError(new HaveDuplicateClassFileException(
                                "Have duplicate files in ") + classNames[i].Key + " and " + classNames[j].Key + " named " + duplicate[k]);
                        }
                    }
                }
            }
        }
        if(haveDuplicate)
        {
            throw new HaveDuplicateClassFileException();
        }

        //Control package name from AndroidManifest.xml
        EditorUtility.DisplayProgressBar("Android Prebuild", "Controlling AndroidManifest.xml.", 0.74f);

        string androidManifestPath = Path.Combine(appPath, Path.Combine("src", Path.Combine("main", "AndroidManifest.xml")));
        XmlDocument androidManifest = new XmlDocument();
        androidManifest.Load(androidManifestPath);
        androidManifest.GetElementsByTagName("manifest");
        string package = androidManifest.DocumentElement.Attributes["package"].Value;
        if(package != PlayerSettings.bundleIdentifier)
        {
            throw new PackageNameNotSameException("Please check that bundle identifier is same as package name in AndroidManifest.xml");
        }

        EditorUtility.DisplayProgressBar("Android Prebuild", "Finished.", 1);
        EditorUtility.ClearProgressBar();
        UnityEngine.Debug.Log("Prebuild is completed.You can start android build.");
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
