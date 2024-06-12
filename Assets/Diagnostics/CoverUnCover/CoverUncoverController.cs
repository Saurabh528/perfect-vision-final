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

public class CoverUncoverController : DiagnosticController
{
	[SerializeField] TCPListener _tcp;
	[SerializeField] GameObject _redPoint, _btnPrint, _btnHelp, _btnStart;
	[SerializeField] Text _textHint, _txtStatus;
	[SerializeField] CoverResultView _resultView;
	Process pythonProcess;
	bool _finished = false;
	bool hint_P;
	CoverUncoverResultData _resultData;
	// Start is called before the first frame update
	void Start()
    {
		if(!File.Exists(PatientMgr.GetPatientDataDir() + "/conversion_rates.txt")){
			_btnStart.SetActive(false);
			_textHint.text = "Please pass card callibration first on Device Setting.";
		}
	}

	

	private void Update()
	{
		if (_finished)
		{
			StartCoroutine(Routine_Finish());
			_finished = false;
		}
		else if(pythonProcess != null && _txtStatus.text.StartsWith("Distance:") && Input.GetKeyDown(KeyCode.P)){
			_textHint.text = "";
			_tcp.SendString("p");
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
		
		//if(Application.platform == RuntimePlatform.OSXPlayer)
			//path = UtilityFunc.GetFullDirFromApp("Deploy/MacBuild/Python");
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
		_processStartInfo.FileName = UtilityFunc.GetPythonExecutablePath(path, "cover_uncover");  // Use the full path
		if(Application.platform == RuntimePlatform.WindowsPlayer)
			_processStartInfo.Arguments        = $" --connect --quiet --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		else if(Application.platform == RuntimePlatform.OSXPlayer)
			_processStartInfo.Arguments        = $"{path}/cover_uncover.py --connect --quiet --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		
		_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
#endif
		if (pythonProcess != null)
		{
			_textHint.text = "Loading...";
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
			if (_txtStatus){
				_txtStatus.text = message.Substring(4);
				if(_txtStatus.text.StartsWith("Distance:") && !hint_P){
					_textHint.text = "Sit at a distance of 40 cms and press p";
					hint_P = true;
				}
			}
		}
	}

	private void OnDestroy()
	{
		if(pythonProcess != null && !pythonProcess.HasExited)
		{
			pythonProcess.Kill();
		}
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

	public override void AddResults(){
        PatientRecord pr = PatientDataMgr.GetPatientRecord();
        DiagnoseTestItem dti = new DiagnoseTestItem();
        dti.AddValue(_resultView._leftMean.text);
        dti.AddValue(_resultView._leftStd.text);
        dti.AddValue(_resultView._rightMean.text);
        dti.AddValue(_resultView._rightStd.text);
        pr.AddDiagnosRecord("Cover Uncover", dti) ;
    }

	public override bool ResultExist(){
        if(!base.ResultExist())
            return false;
        return _resultData != null;
    }
}
