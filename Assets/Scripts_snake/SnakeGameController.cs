using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;

public class SnakeGameController : MonoBehaviour
{
    [SerializeField] Text textLevel, _textInstruction;
    [SerializeField] GameObject[] zoneObjects;
    [SerializeField] GameObject _objOutput, _btnDownload;
    [SerializeField] AlignmentTableRow _alignTblRowTmpl;
	[SerializeField] RawImage _image4Plot, _imageIPDPlot;
    Process pythonProcess;
    bool _finished = false, _failed = false;
    public Snake snake;
    // Start is called before the first frame update
    void Start()
    {
        StartPythonProcess();
    }

    // Update is called once per frame
    void Update()
    {
       if (_finished)
		{
			snake.EndGame();
			if(_failed)
				_textInstruction.text = "Checking failed.";
			else
				StartCoroutine(Routine_Finish());
			_finished = false;
		}
    }

    void StartPythonProcess(){
        int camindex = GlobalSettingUI.GetCurrentCameraIndex();
		if(camindex == -1){
			_textInstruction.text = "No Web camera is instaled.";
			return;
		}
		UnityEngine.Debug.Log($"Cameraindex: {camindex}");

		string path = Application.dataPath + "/../Python";
#if UNITY_EDITOR
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName         = "python.exe";
		_processStartInfo.Arguments        = $"{path}/final_snake_game.py --quiet --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_PATIENTNAME}={PatientMgr.GetCurrentPatientName()}";
		//_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
#else
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName         = "final_snake_game.exe";
		_processStartInfo.Arguments        = $" --quiet --{GameConst.PYARG_CAMERAINDEX}={camindex} --{GameConst.PYARG_PATIENTNAME}={PatientMgr.GetCurrentPatientName()}";
		_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
#endif
		if (pythonProcess != null)
		{
			pythonProcess.EnableRaisingEvents = true;
			pythonProcess.Exited += OnPythonProcessExited;
			UnityEngine.Debug.Log("PYTHON PROCESS IS NOT NULLLLLLl");
            
        }
		else{
			_textInstruction.text = "Checking failed. Try again.";
            snake.EndGame();
			//_btnStart.SetActive(true);
		}
    }

    private void OnPythonProcessExited(object sender, System.EventArgs e)
	{
        _finished = true;
        
	}

    IEnumerator Routine_Finish()
	{
		
		_textInstruction.text = "Checking finished.";
		snake.EndGame();
		yield return new WaitForSeconds(3);
        foreach(GameObject obj in zoneObjects)
            obj.SetActive(false);
		_textInstruction.text = "";	
        ShowResult();
		_btnDownload.SetActive(true);
	}

    private void OnDestroy()
	{
		if(pythonProcess != null && !pythonProcess.HasExited)
		{
			pythonProcess.Kill();
		}
	}

   

    void ShowResult()
	{
		_objOutput.SetActive(true);
		UtilityFunc.SetRawImageFromFile(_image4Plot, PatientMgr.GetPatientDataDir() + "/four_plots.png");
		UtilityFunc.SetRawImageFromFile(_imageIPDPlot, PatientMgr.GetPatientDataDir() + "/ipd_plot.png");
    }

    public void OnBtnPrintPDF()
	{
		
		PdfWriter writer;
		Document document;
		string dir = PatientMgr.GetDiagnoseResultDir(); // If directory does not exist, create it.
		if (!Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}

		string path = dir + PatientMgr.GetCurrentPatientName() + "_Alignment.pdf";
		if (!PDFUtil.CreatePDFHeader(path, out document, out writer))
			return;
		PDFUtil.AddSectionBar("Alignment Result", document);

		//collcted metrics table
		PDFUtil.AddSubSection("Snake Game Test", document);
		System.Drawing.Image image = UtilityFunc.Texture2Image((Texture2D)_image4Plot.texture);
		if(image != null)
		{
			iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(image, BaseColor.WHITE);
			img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
			img.ScaleToFit(500, 500);
			document.Add(img);
		}
		image = UtilityFunc.Texture2Image((Texture2D)_imageIPDPlot.texture);
		if(image != null)
		{
			iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(image, BaseColor.WHITE);
			img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
			img.ScaleToFit(500, 500);
			document.Add(img);
		}
        document.Close();
		UtilityFunc.StartProcessByFile(path);
	}
}
