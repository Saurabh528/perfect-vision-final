using UnityEngine;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using System.IO;

public class PostBuildActions : IPostprocessBuildWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnPostprocessBuild(BuildReport report)
    {
        if (report.summary.platform == BuildTarget.StandaloneOSX)
        {
            string buildPath = report.summary.outputPath;
            string frameworksPath = Path.Combine(buildPath, "Contents/Frameworks");

            // Ensure the Frameworks directory exists
            if (!Directory.Exists(frameworksPath))
            {
                Directory.CreateDirectory(frameworksPath);
            }

            // Path to the libgdiplus library on your build machine (adjust as necessary)
            string sourcePath = "/usr/local/lib/libgdiplus.dylib";

            // Destination path within the app bundle
            string destinationPath = Path.Combine(frameworksPath, "libgdiplus.dylib");

            // Copy the libgdiplus.dylib to the Frameworks directory
            File.Copy(sourcePath, destinationPath, overwrite: true);

            // Set RPath to look in the Frameworks directory
            string executablePath = Path.Combine(buildPath, "Contents/MacOS/", Path.GetFileNameWithoutExtension(buildPath));
            string installNameToolArgs = $"-add_rpath @executable_path/../Frameworks {executablePath}";
            
            // Execute the install_name_tool command
            System.Diagnostics.Process process = new System.Diagnostics.Process();
            process.StartInfo.FileName = "install_name_tool";
            process.StartInfo.Arguments = installNameToolArgs;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.Start();
            
            string output = process.StandardOutput.ReadToEnd();
            process.WaitForExit();

            Debug.Log("Post-build actions completed: " + output);
        }
    }
}
