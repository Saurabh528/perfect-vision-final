using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System.IO;
using System;
using System.Linq.Expressions;

public class RecordWebcamVideo : MonoBehaviour
{
    [SerializeField] GameObject btnSave;

    static Process pythonProcess;
    public static RecordWebcamVideo Instance;
    void Awake()
    {
        Instance = this;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDestroy(){
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            StreamWriter writer = pythonProcess.StandardInput;
            writer.WriteLine("q");
            writer.Flush();
            pythonProcess = null;
        }
    }

    public static void StartRecord(){
        if(!Instance)
            return;
        int camindex = GlobalSettingUI.GetCurrentCameraIndex();
		if(camindex == -1){
			UnityEngine.Debug.Log("No Web camera is instaled.");
			return;
		}

        string path =  UtilityFunc.GetFullDirFromApp("Python");
#if UNITY_EDITOR
        ProcessStartInfo _processStartInfo = new ProcessStartInfo();
        _processStartInfo.WorkingDirectory = path;
        _processStartInfo.FileName = UtilityFunc.GetPythonPath();
        _processStartInfo.Arguments        = $"{path}/record_camera.py --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
        _processStartInfo.UseShellExecute = false;
        _processStartInfo.RedirectStandardInput = true;
        _processStartInfo.RedirectStandardOutput = true;
        _processStartInfo.RedirectStandardError = true;
        _processStartInfo.CreateNoWindow = false;
        pythonProcess = Process.Start(_processStartInfo);
#else
        ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName = UtilityFunc.GetPythonExecutablePath(path, "record_camera");  // Use the full path
		if(Application.platform == RuntimePlatform.WindowsPlayer)
			_processStartInfo.Arguments        = $" --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		else if(Application.platform == RuntimePlatform.OSXPlayer)
			_processStartInfo.Arguments        = $"{path}/record_camera.py --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
        _processStartInfo.UseShellExecute = false;
        _processStartInfo.RedirectStandardInput = true;
        _processStartInfo.RedirectStandardOutput = true;
        _processStartInfo.RedirectStandardError = true;
        _processStartInfo.CreateNoWindow = true;
		_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
#endif
        if (pythonProcess != null)
		{
			pythonProcess.EnableRaisingEvents = true;
		}
    }


    public void SaveVideo(string testname){
        if (pythonProcess != null && !pythonProcess.HasExited)
        {
            StreamWriter writer = pythonProcess.StandardInput;
            writer.WriteLine($"save {testname}");
            writer.Flush();
            pythonProcess = null;
            btnSave.SetActive(false);
        }
        else{
            Diagnosis.SaveVideo(PatientMgr.GetPatientDataDir(), testname);
            btnSave.SetActive(false);
        }
       
    }

    public static void ActivateSaveButton(){
        if(Instance)
            Instance.btnSave.SetActive(true);
    }

}
