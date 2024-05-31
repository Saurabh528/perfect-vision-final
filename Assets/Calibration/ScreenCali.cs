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
	[SerializeField] WebCamRender webCamRender;
	Process pythonProcess;
	bool _finished = false;
	int pressCount;
	// Start is called before the first frame update

	private void Update()
	{
		if(webCamRender.IsOpen()){
			if(Input.GetKeyDown(KeyCode.P) && webCamRender.IsCaptuable()){
				string pngFilePath = $"{PatientMgr.GetPatientDataDir()}/grab_screen{pressCount}.png";
				webCamRender.CaptureAndSaveImage(pngFilePath, pressCount == 9);
				pressCount++;
				if(pressCount == 10){
					_text.text = "";
					StartPythonProcess();
				}
			}
		}
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
			_text.text = "No Web camera is installed.";
			return;
		}
		UnityEngine.Debug.Log($"Camera Index: {camindex}");
		if(!webCamRender.OpenCamera()){
			_text.text = "No Web camera is installed.";
			return;
		}
		_text.text = "Sit at 50 cms from the screen and press p 10 times in still position once comfortable.";
		
	}

	void StartPythonProcess(){
		string path =  UtilityFunc.GetFullDirFromApp("Python");

		DebugUI.LogValue("path", path);
		_text.text = "Please wait...";

#if UNITY_EDITOR
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName = UtilityFunc.GetPythonPath();
		_processStartInfo.Arguments        = $"{path}/screen_distance_callibration.py --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		
		// _processStartInfo.RedirectStandardOutput = true;
		// _processStartInfo.UseShellExecute = false;
        // _processStartInfo.RedirectStandardOutput = true;
        // _processStartInfo.RedirectStandardError = true;
		pythonProcess = Process.Start(_processStartInfo);
		
		
		// string output = pythonProcess.StandardOutput.ReadToEnd();
		// string errors = pythonProcess.StandardError.ReadToEnd();
		// pythonProcess.WaitForExit();
		// DebugUI.LogValue("Output", output);
		// DebugUI.LogValue("Errors", errors);
		
#else
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		// _processStartInfo.RedirectStandardOutput = true;
		// _processStartInfo.UseShellExecute = false;
        // _processStartInfo.RedirectStandardOutput = true;
        // _processStartInfo.RedirectStandardError = true;
		string executablePath = Path.Combine(path, $"screen_distance_callibration{UtilityFunc.GetPlatformSpecificExecutableExtension()}");
		if (!File.Exists(executablePath))
		{
			DebugUI.LogString($"Executable not found at {executablePath}");
			UtilityFunc.AppendToLog($"Executable not found at {executablePath}");
			return;  // Stop further execution if the file does not exist
		}
		_processStartInfo.FileName = executablePath;  // Use the full path

		_processStartInfo.Arguments        = $" --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		/* DebugUI.LogValue("FileName", _processStartInfo.FileName);
		DebugUI.LogValue("Arguments", _processStartInfo.Arguments); */



		pythonProcess = Process.Start(_processStartInfo);
		DebugUI.LogValue("pythonProcess", pythonProcess);
		// string output = pythonProcess.StandardOutput.ReadToEnd();
		// string errors = pythonProcess.StandardError.ReadToEnd();
		// pythonProcess.WaitForExit();
		// DebugUI.LogValue("Output", output);
		// DebugUI.LogValue("Errors", errors);
		// return;

#endif

		//UnityEngine.Debug.Log($"FinaName:{_processStartInfo.FileName}, Arguments:{_processStartInfo.Arguments}");
		if (pythonProcess != null && !pythonProcess.HasExited)
		{
			_text.text = "Processing...";
			pythonProcess.EnableRaisingEvents = true;  // Enable event raising
			pythonProcess.Exited += OnPythonProcessExited;
		}
		else{
			UnityEngine.Debug.LogError("Can not nun screen_distance process.");
			_text.text = "Checking failed. Try again.";
		}
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
