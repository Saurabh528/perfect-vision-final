using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using System;
using System.IO;
using System.Text.RegularExpressions;


public class CoverUncoverResultData{
	public bool isValid = false;
	public float leftMean, leftStd, rightMean, rightStd;
	public CoverUncoverResultData(string filenmae){
		if (!File.Exists(filenmae))
			return;
		string[] lines = File.ReadAllLines(filenmae);
		if(lines.Length != 2)
			return;
		string pattern = @"Mean Value = (\d+\.\d+), Standard Deviation = (\d+\.\d+)";
		// Match the pattern in the input string
        Match matchLeft = Regex.Match(lines[0], pattern);
        Match matchRight = Regex.Match(lines[1], pattern);
        if (matchLeft.Success && matchRight.Success){
			leftMean = float.Parse(matchLeft.Groups[1].Value);
			leftStd = float.Parse(matchLeft.Groups[2].Value);
			rightMean = float.Parse(matchRight.Groups[1].Value);
			rightStd = float.Parse(matchRight.Groups[2].Value);
		}
		isValid = true;
	}
}

public class CoverUncoverController : MonoBehaviour
{
	[SerializeField] TCPListener _tcp;
	[SerializeField] GameObject _redPoint, _btnPrint, _btnHelp;
	[SerializeField] Text _textHint, _txtStatus;
	[SerializeField] CoverResultView _resultView;
	Process pythonProcess;
	bool _finished = false;
	CoverUncoverResultData _resultData;
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
		
		string path =  UtilityFunc.GetFullDirFromApp("Python");
		_textHint.text = "Please wait...";
#if UNITY_EDITOR
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName = UtilityFunc.GetPythonPath();
		_processStartInfo.Arguments        = $"{path}/cover_uncover.py --connect --quiet --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		pythonProcess = Process.Start(_processStartInfo);
#else
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		string executablePath = Path.Combine(path, $"cover_uncover{UtilityFunc.GetPlatformSpecificExecutableExtension()}");
		if (!File.Exists(executablePath))
		{
			DebugUI.LogString($"Executable not found at {executablePath}");
			return;  // Stop further execution if the file does not exist
		}
		_processStartInfo.FileName = executablePath;  // Use the full path
		_processStartInfo.Arguments        = $" --connect --quiet --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
		UtilityFunc.AppendToLog("Cover/Uncover python process started.");
#endif
		if (pythonProcess != null)
		{
			pythonProcess.EnableRaisingEvents = true;
			pythonProcess.Exited += OnPythonProcessExited;
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
		else if (message.StartsWith("STS:")) {
			if (_txtStatus)
				_txtStatus.text = message.Substring(4);
		}
	}

	private void OnDestroy()
	{
		if(pythonProcess != null && !pythonProcess.HasExited)
		{
			pythonProcess.Kill();
		}
		_tcp.StopTCP();
	}

	private void OnPythonProcessExited(object sender, EventArgs e)
	{
		
		_finished = true;
	}


	IEnumerator Routine_Finish()
	{
		_redPoint.SetActive(false);
		_textHint.text = "Checking successful.";
		yield return new WaitForSeconds(2);
		_textHint.text = "";
		_resultView.gameObject.SetActive(true);
		_resultData = null;
		_resultData = new CoverUncoverResultData(PatientMgr.GetPatientDataDir() + "/eye_statistics.txt");
		_resultView.ShowResult(_resultData);
		_btnPrint.SetActive(true);
		//_btnHelp.SetActive(true);
	}

	public void OnBtnPrintPDF()
	{
		_resultView.PrintAndShowPDF(_resultData);
	}

}
