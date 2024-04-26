using System;
using System.Diagnostics;
using System.IO;
using UnityEngine;


public class PythonRunner : MonoBehaviour
{
    void Start()
    {
        RunPythonScript();
    }

    void RunPythonScript()
    {
        ProcessStartInfo startInfo = new ProcessStartInfo();
        startInfo.FileName = "/Users/jatingoel/Downloads/python_executable/dist/jatin"; // Path to the executable
        startInfo.UseShellExecute = false;
        startInfo.RedirectStandardOutput = true;
        startInfo.RedirectStandardError = true;

        try
        {
            using (Process process = Process.Start(startInfo))
            {
                using (StreamReader reader = process.StandardOutput)
                {
                    string result = reader.ReadToEnd();
                    UnityEngine.Debug.Log("Output: " + result);
                }
                using (StreamReader reader = process.StandardError)
                {
                    string stderr = reader.ReadToEnd();
                    if (!string.IsNullOrEmpty(stderr))
                    {
                        UnityEngine.Debug.LogError("Error: " + stderr);
                    }
                }
            }
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogError("Failed to start process: " + e.Message);
        }
    }
}
