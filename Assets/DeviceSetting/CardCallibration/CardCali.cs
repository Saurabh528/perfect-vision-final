using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
using System.IO;
using UnityEngine.UI;
using UnityEngine.Events;
using Org.BouncyCastle.Asn1.Misc;
using System.util;

public class CardCali : MonoBehaviour
{
	[SerializeField] Text _text;
	[SerializeField] Button _btnStart;
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

	public void StartCali()
    {
		int camindex = GlobalSettingUI.GetCurrentCameraIndex();
		if(camindex == -1){
			_text.text = "No Web camera is instaled.";
			return;
		}
		_btnStart.enabled = false;

		UnityEngine.Debug.Log($"Camera Index: {camindex}");
		string path =  UtilityFunc.GetFullDirFromApp("Python");
		_text.text = "Please wait...";
#if UNITY_EDITOR
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName = UtilityFunc.GetPythonPath();
		_processStartInfo.Arguments        = $"{path}/card_callib_final.py --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		pythonProcess = Process.Start(_processStartInfo);
#else
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		string executablePath = Path.Combine(path, $"card_callib_final{UtilityFunc.GetPlatformSpecificExecutableExtension()}");
		if (!File.Exists(executablePath))
		{
			DebugUI.LogString($"Executable not found at {executablePath}");
			return;  // Stop further execution if the file does not exist
		}
		_processStartInfo.FileName = executablePath;  // Use the full path
		_processStartInfo.Arguments        = $" --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
#endif
		if (pythonProcess != null)
		{
			pythonProcess.EnableRaisingEvents = true;
			pythonProcess.Exited += OnPythonProcessExited;
		}
		else{
			_btnStart.enabled = true;
			_text.text = "Checking failed. Try again.";
		}
	}

	private void OnPythonProcessExited(object sender, EventArgs e)
	{
		_finished = true;
		_btnStart.enabled = true;
	}


	IEnumerator Routine_Finish()
	{
		_text.text = "Checking successful.";
		yield return new WaitForSeconds(2);
		ChangeScene.LoadScene(GameState.MODE_DOCTORTEST? "Diag_ForTest" : "Diagnostic");
	}

	private void OnDestroy()
	{
		if(pythonProcess != null && !pythonProcess.HasExited)
		{
			pythonProcess.Kill();
		}
	}

	public void OnRecvString(System.String message)
	{
		if (message.StartsWith("CMD:"))
		{
			
		}
		else if (message.StartsWith("MSG:"))
		{
			if (_text)
				_text.text = message.Substring(4);
		}
		else if (message.StartsWith("STS:"))
		{
			
		}
	}


	
}
