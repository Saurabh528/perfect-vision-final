using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using System;
using System.IO;
using UnityEngine.UI;
using TMPro;

public class CardCali : MonoBehaviour
{
	[SerializeField] Text _text;
	[SerializeField] Button _btnStart;
	[SerializeField] WebCamRender webCamRender;
	[SerializeField] GameObject pointTip;
	[SerializeField] GameObject[] cornerPoints;
	[SerializeField] TCPListener _tcp;
	[SerializeField] TextMeshProUGUI textStatus;
	Process pythonProcess;
	bool _finished = false;
	int turnIndex;//0,1,2,3
	float[] distances = new float[]{40, 45, 50, 55};
	int actIndex;//0,1,2,3,4--p, LT, RT, LB, RB
	// Start is called before the first frame update
	Vector2 [,] points = new Vector2[4, 4];
	private void Update()
	{
		if(webCamRender.IsOpen() && pythonProcess != null){
			if(Input.GetKeyDown(KeyCode.P)){
				webCamRender.Pause();
				actIndex = 1;
				ShowTip();
				if(turnIndex == 0)
					pointTip.SetActive(true);
			}
			else if(actIndex > 0 && Input.GetMouseButtonDown(0)){
				points[turnIndex, actIndex - 1] = Input.mousePosition;
				cornerPoints[actIndex - 1].transform.position = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
				cornerPoints[actIndex - 1].SetActive(true);
				actIndex++;
				if(actIndex == 5){
					StartCoroutine(NextDistanceTurn());
				}
				else{
					ShowTip();
				}
			}
		}
		else if (_finished)
		{
			StartCoroutine(Routine_Finish());
			_finished = false;
		}
	}

	IEnumerator NextDistanceTurn(){
		yield return new WaitForSeconds(1);
		actIndex = 0;
		turnIndex++;
		foreach(GameObject gameObjectgo in cornerPoints)
			gameObjectgo.SetActive(false);
		if(turnIndex == 4){
			turnIndex = 0;
			webCamRender.StopRecord();
			string pointparam = "";
			for(int i = 0; i < 4; i++){
				for(int j = 0; j < 4; j++){
					Vector2 imagepoint = Screen2Image(points[i, j]);
					if(i == 0 && j == 0)
						pointparam = $"({imagepoint.x},{imagepoint.y})";
					else
						pointparam += $",({imagepoint.x},{imagepoint.y})";
				}
			}
			File.WriteAllText($"{PatientMgr.GetPatientDataDir()}/points.txt", pointparam);
			webCamRender.CloseCamera();
			textStatus.text = "";
		}
		else{
			webCamRender.Resume();
			ShowTip();
		}
	}

	public void StartCali()
    {
		turnIndex = actIndex = 0;
		int camindex = GlobalSettingUI.GetCurrentCameraIndex();
		if(camindex == -1){
			_text.text = "No Web camera is instaled.";
			return;
		}

		UnityEngine.Debug.Log($"Camera Index: {camindex}");
		
		if(!webCamRender.OpenCamera()){
			_text.text = "No Web camera is installed.";
			return;
		}
		_btnStart.gameObject.SetActive(false);
		_tcp.InitTCP();
		if(StartPythonProcess())
			webCamRender.StartRecord($"{PatientMgr.GetPatientDataDir()}/record.png");
	}

	void ShowTip(){
		if(actIndex == 0)
			_text.text = $"Sit at {distances[turnIndex]}cm from the screen and press P.";
		else if(actIndex == 1)
			_text.text = "Click on the left-top corner of the card.";
		else if(actIndex == 2)
			_text.text = "Click on the right-top corner of the card.";
		else if(actIndex == 3)
			_text.text = "Click on the left-bottom corner of the card.";
		else if(actIndex == 4)
			_text.text = "Click on the right-bottom corner of the card.";
	}

	bool StartPythonProcess(){
		
		if(File.Exists($"{PatientMgr.GetPatientDataDir()}/points.txt"))
			File.Delete($"{PatientMgr.GetPatientDataDir()}/points.txt");
		string path =  UtilityFunc.GetFullDirFromApp("Python");
		_text.text = "Please wait...";
#if UNITY_EDITOR
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName = UtilityFunc.GetPythonPath();
		_processStartInfo.Arguments        = $"{path}/card_callib_final.py --connect --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
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
		_processStartInfo.Arguments        = $"--connect --{GameConst.PYARG_DATADIR}=\"{PatientMgr.GetPatientDataDir()}\"";
		_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
#endif
		if (pythonProcess != null)
		{
			ShowTip();
			pythonProcess.EnableRaisingEvents = true;
			pythonProcess.Exited += OnPythonProcessExited;
			return true;
		}
		else{
			_btnStart.gameObject.SetActive(true);
			_text.text = "Checking failed. Try again.";
			return false;
		}
	}

	Vector2 Screen2Image(Vector2 screenPoint){//convert point from screen space to video image space
		Vector2 imagePoint = Vector2.zero;
		RawImage rawImage = webCamRender.GetDisplayImage();
		if(rawImage == null)
			return imagePoint;
		RectTransform rectTransform = rawImage.GetComponent<RectTransform>();
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(rectTransform, screenPoint, Camera.main, out Vector2 localPoint))
        {
            // Convert local point to normalized coordinates (0 to 1)
            Rect rect = rectTransform.rect;
            float x = (localPoint.x - rect.x) / rect.width;
            float y = (localPoint.y - rect.y) / rect.height;

            imagePoint = new Vector2(x, y);
        }
		imagePoint.x = Mathf.Clamp01(imagePoint.x);
		imagePoint.y = 1 - Mathf.Clamp01(imagePoint.y);
		imagePoint.x *= rawImage.texture.width;
		imagePoint.y *= rawImage.texture.height;
		imagePoint.x = (int)Mathf.Clamp(imagePoint.x , 0, rawImage.texture.width - 1);
		imagePoint.y = (int)Mathf.Clamp(imagePoint.y , 0, rawImage.texture.height - 1);
        return imagePoint;
	}

	private void OnPythonProcessExited(object sender, EventArgs e)
	{
		_finished = true;
		_btnStart.gameObject.SetActive(true);
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
			textStatus.text = message.Substring(4);
		}
	}


	
}
