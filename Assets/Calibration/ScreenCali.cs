using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Events;

public class ScreenCali : MonoBehaviour
{
	[SerializeField] Text _text;
	Process pythonProcess;
	bool _finished = false;
	// Start is called before the first frame update

	private void Update()
	{
		if (_finished)
		{
			StartCoroutine(Routine_Finish());
			_finished = false;
		}
	}

	public void StartDistanceCali()
    {
		DebugUI.LogString("StartDistanceCali");
		int camindex = GlobalSettingUI.GetCurrentCameraIndex();
		DebugUI.LogValue("cameraidx", camindex);
		if(camindex == -1){
			_text.text = "No Web camera is instaled.";
			return;
		}
		UnityEngine.Debug.Log($"Camera Index: {camindex}");
		string path =  UtilityFunc.GetFullDirFromApp("Python");

		DebugUI.LogValue("path", path);
		_text.text = "Please wait...";

#if UNITY_EDITOR
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName = UtilityFunc.GetPythonPath();
		_processStartInfo.Arguments        = $"{path}/screen_distance_callibration.py --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		
		/* _processStartInfo.RedirectStandardOutput = true;
		_processStartInfo.UseShellExecute = false;
        _processStartInfo.RedirectStandardOutput = true;
        _processStartInfo.RedirectStandardError = true; */
		pythonProcess = Process.Start(_processStartInfo);
		
		/*
		
		string output = pythonProcess.StandardOutput.ReadToEnd();
		string errors = pythonProcess.StandardError.ReadToEnd();
		pythonProcess.WaitForExit();
		DebugUI.LogValue("Output", output);
		DebugUI.LogValue("Errors", errors);
		return;/**/
#else
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		/* _processStartInfo.RedirectStandardOutput = true;
		_processStartInfo.UseShellExecute = false;
        _processStartInfo.RedirectStandardOutput = true;
        _processStartInfo.RedirectStandardError = true; */
		string executablePath = Path.Combine(path, $"screen_distance_callibration{UtilityFunc.GetPlatformSpecificExecutableExtension()}");
		if (!File.Exists(executablePath))
		{
			DebugUI.LogString($"Executable not found at {executablePath}");
			return;  // Stop further execution if the file does not exist
		}
		_processStartInfo.FileName = executablePath;  // Use the full path

		_processStartInfo.Arguments        = $"--{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		DebugUI.LogValue("FileName", _processStartInfo.FileName);
		DebugUI.LogValue("Arguments", _processStartInfo.Arguments);



		pythonProcess = Process.Start(_processStartInfo);
		DebugUI.LogValue("pythonProcess", pythonProcess);
		/*string output = pythonProcess.StandardOutput.ReadToEnd();
		string errors = pythonProcess.StandardError.ReadToEnd();
		pythonProcess.WaitForExit();
		DebugUI.LogValue("Output", output);
		DebugUI.LogValue("Errors", errors);
		return;/**/

#endif

		//UnityEngine.Debug.Log($"FinaName:{_processStartInfo.FileName}, Arguments:{_processStartInfo.Arguments}");
		if (pythonProcess != null && !pythonProcess.HasExited)
		{
			pythonProcess.EnableRaisingEvents = true;  // Enable event raising
			pythonProcess.Exited += OnPythonProcessExited;
		}
		else
			_text.text = "Checking failed. Try again.";
	}

	private void OnPythonProcessExited(object sender, EventArgs e)
	{
		//UnityEngine.Debug.Log(pythonProcess.StandardOutput.ReadToEnd());
		_finished = true;
	}

	private void OnDestroy()
	{
		if(pythonProcess != null && !pythonProcess.HasExited)
		{
			pythonProcess.Kill();
		}
	}


	IEnumerator Routine_Finish()
	{
		_text.text = "Checking successful.";
		yield return new WaitForSeconds(2);
		ChangeScene.LoadScene("CardCalli");
	}


	
}
