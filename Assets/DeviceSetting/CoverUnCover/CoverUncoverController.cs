using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using System;

public class CoverUncoverController : MonoBehaviour
{
	[SerializeField] TCPListener _tcp;
	[SerializeField] GameObject _redPoint, _btnPrint, _btnHelp;
	[SerializeField] Text _textHint;
	[SerializeField] CoverResultView _resultView;
	Process pythonProcess;
	bool _finished = false;

	// Start is called before the first frame update
	void Start()
    {
		
	}

	private void Update()
	{
		if (_finished)
		{
			StartCoroutine(Routine_Finish());
			_finished = false;
		}
	}

	public void OnBtnStart()
	{
		int camindex = GlobalSettingUI.GetCurrentCameraIndex();
		if(camindex == -1){
			_textHint.text = "No Web camera is instaled.";
			return;
		}
		UnityEngine.Debug.Log($"Camera Index: {camindex}");

		_tcp.InitTCP();
#if UNITY_EDITOR
		string path = Application.dataPath + "/..";
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName         = "python.exe";
		_processStartInfo.Arguments        = $"{path}/Python/CoverUnCover/cover_uncover.py --connect --{GameConst.PYARG_CAMERAINDEX}={camindex}";
		//_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
#else
		string path = Application.dataPath + "/..";
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName         = "cover_uncover.exe";
		_processStartInfo.Arguments        = $" --connect --quiet --{GameConst.PYARG_CAMERAINDEX}={camindex}";
		_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
#endif
		if (pythonProcess != null)
		{
			pythonProcess.EnableRaisingEvents = true;
			pythonProcess.Exited += OnPythonProcessExited;
			_textHint.text = "Please wait...";
		}
		else
			_textHint.text = "Checking failed. Try again.";
	}

	// Update is called once per frame
	public void OnRecvString(System.String message)
	{
		if (message.StartsWith("CMD:"))
		{
			string cmdstr = message.Substring(4);
			if (cmdstr == "SHOWPOINT")
				_redPoint.SetActive(true);
			else if (cmdstr == "EXIT")
			{
				_redPoint.SetActive(false);
			}
		}
		else if (message.StartsWith("MSG:")) {
			if (_textHint)
				_textHint.text = message.Substring(4);
		}
	}

	private void OnDestroy()
	{
		if (pythonProcess != null)
		{
			pythonProcess.Dispose();
		}
		_tcp.StopTCP();
	}

	private void OnPythonProcessExited(object sender, EventArgs e)
	{
		_finished = true;
	}


	IEnumerator Routine_Finish()
	{
		_textHint.text = "Checking successful.";
		yield return new WaitForSeconds(2);
		_textHint.text = "";
		_resultView.gameObject.SetActive(true);
		_resultView.ShowResult(PatientMgr.GetCurrentPatientName());
		_btnPrint.SetActive(true);
		_btnHelp.SetActive(true);
	}

	public void OnBtnPrintPDF()
	{
		_resultView.PrintAndShowPDF(PatientMgr.GetCurrentPatientName());
	}
}
