using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using UnityEngine;

public class TestScript : MonoBehaviour
{
    // Start is called before the first frame update
    public void RunMainPythonScript()
    {
        RunPython("D:\\PROJECTS\\perfect-vision-aman2\\Python\\main.py");
    }

    public void RunCvPythonScript()
    {
        RunPython("D:\\PROJECTS\\perfect-vision-aman2\\Python\\cv.py");
    }

    private void RunPython(string scriptPath)
    {
        ProcessStartInfo start = new ProcessStartInfo();
        start.FileName = "python"; // Make sure 'python' is in PATH or provide the full path to python.exe
        start.Arguments = $"\"{scriptPath}\""; // Pass the full path to the Python script
        start.UseShellExecute = false;
        start.RedirectStandardOutput = true;
        start.RedirectStandardError = true;

        using (Process process = Process.Start(start))
        {
            using (StreamReader reader = process.StandardOutput)
            {
                string result = reader.ReadToEnd();
                UnityEngine.Debug.Log(result);
            }
            using (StreamReader reader = process.StandardError)
            {
                string stderr = reader.ReadToEnd();
                if (!string.IsNullOrEmpty(stderr))
                {
                    UnityEngine.Debug.LogError(stderr);
                }
            }
        }
    }
}


