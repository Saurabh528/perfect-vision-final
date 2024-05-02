using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
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
		int camindex = GlobalSettingUI.GetCurrentCameraIndex();
		if(camindex == -1){
			_text.text = "No Web camera is instaled.";
			return;
		}
		UnityEngine.Debug.Log($"Camera Index: {camindex}");
		string path = Application.dataPath + "/../Python";
#if UNITY_EDITOR
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName         = "python.exe";
		_processStartInfo.Arguments        = $"{path}/screen_distance_callibration.py --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_PATIENTNAME}={PatientMgr.GetCurrentPatientName()}";
		pythonProcess = Process.Start(_processStartInfo);
#else
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName         = "screen_distance_callibration.exe";
		_processStartInfo.Arguments        = $"--{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_PATIENTNAME}={PatientMgr.GetCurrentPatientName()}";
		_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
#endif
		if (pythonProcess != null)
		{
			pythonProcess.EnableRaisingEvents = true;
			pythonProcess.Exited += OnPythonProcessExited;
			_text.text = "Please wait...";
		}
		else
			_text.text = "Checking failed. Try again.";
	}

	private void OnPythonProcessExited(object sender, EventArgs e)
	{
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
