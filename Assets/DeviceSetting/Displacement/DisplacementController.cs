using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Diagnostics;
using UnityEngine.UI;
using System;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using TMPro;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
public class DisplacementController : MonoBehaviour
{
	[SerializeField] TCPListener _tcp;
	[SerializeField] GameObject _redPoint, _btnPrint, _btnResultHelp;
	[SerializeField] Text _textHint, _textAverDist;
	[SerializeField] UIDisplaceView _displaceviewTmpl;
	[SerializeField] ScrollRect _scrollRect;
	[SerializeField] TextMeshProUGUI _textStatus;
	Process pythonProcess;
	bool _finished = false;
    public TextMeshProUGUI textDisplay;  // Reference to your TextMeshProUGUI component
    string filePath = @"D:\PROJECTS\perfect-vision-aman2\Python\displacement.txt";  // Name of your text file

    void Start()
    {
#if UNITY_EDITOR
		//UISignIn.StartFromSignInDebugMode();
#endif

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
		_processStartInfo.Arguments        = $"{path}/Python/Displacement/dev_prism.py --connect  --{GameConst.PYARG_CAMERAINDEX}={camindex}";
		pythonProcess = Process.Start(_processStartInfo);
#else
		string path = Application.dataPath + "/..";
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName         = "dev_prism.exe";
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
				_tcp.StopTCP();
			}
		}
		else if (message.StartsWith("MSG:")) {
			if (_textHint)
				_textHint.text = message.Substring(4);
		}
		else if (message.StartsWith("STS:"))
		{
			if (_textStatus)
				_textStatus.text = message.Substring(4);

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
		_textHint.text = "Checking finished.";
		yield return new WaitForSeconds(2);
		_textHint.text = "";
		ShowResult();
		float targetpos = 1;
		float startpos = _scrollRect.verticalNormalizedPosition;
		for (int i = 1; i < 10; i++)
		{
			_scrollRect.verticalNormalizedPosition = Mathf.Lerp(startpos, targetpos, 0.1f * i);
			yield return new WaitForSeconds(0.05f);
		}
	}



	void ShowResult()
	{
		//activate the textDisplay object
		textDisplay.gameObject.SetActive(true);
        if (System.IO.File.Exists(filePath))
        {
            // Read all text from the file
            string contents = System.IO.File.ReadAllText(filePath);
            // Set the text of your TextMeshProUGUI component
            textDisplay.text = contents;
        }
        else
        {
            UnityEngine.Debug.LogError("File not found: " + filePath);
        }
        MakeResultView("Iris Displacement (LEFT EYE)", PatientMgr.GetPatientDataDir() + "/data_left_displacement.csv");
		MakeResultView("Iris Displacement (RIGHT EYE)", PatientMgr.GetPatientDataDir() + "/data_right_displacement.csv");
		//string alternatepath = PatientMgr.GetPatientDataDir() + "/alternate_test.csv";
        string alternatepath = "D:\\PROJECTS\\perfect-vision-aman2\\Python\\Displacement\\RELD.txt";
		UnityEngine.Debug.Log(alternatepath);
        if (File.Exists(alternatepath)){
			UnityEngine.Debug.Log("Alternate path ke andar");
			string[] strs = File.ReadAllLines(alternatepath);
			UnityEngine.Debug.Log(strs);
			if(strs.Length > 1) {
				string[] valuestrs = strs[1].Split(new char[] {','});
				UnityEngine.Debug.Log(valuestrs);
				float rightAver = float.Parse(valuestrs[1]);
				float leftAver = float.Parse(valuestrs[2]);
				if(valuestrs.Length > 2) {
					_textAverDist.text = $"Average Displacement:   LEFT: {leftAver.ToString("G3")}   RIGHT: {rightAver.ToString("G3")}";
					_textAverDist.transform.parent.parent.gameObject.SetActive(true);
					_textAverDist.transform.parent.parent.SetAsLastSibling();
					if(GameState.currentPatient != null)
					{
						PatientDataMgr.AddDisplacementRecord(new DisplacementRecord(leftAver, rightAver));
						PatientDataMgr.SavePatientData(SaveDisplacementDataSuccess, SaveDisplacementDataFailed);
					}
					
				}
				
			}
			
		}
		_btnPrint.SetActive(true );
		_btnResultHelp.SetActive(true);
		_textStatus.gameObject.SetActive(false);
	}

	void SaveDisplacementDataSuccess()
	{
		StartCoroutine(Routine_ShowHit("Displacement record saved.", 2f));
	}

	void SaveDisplacementDataFailed(string error)
	{
		StartCoroutine(Routine_ShowHit(error, 2f));
	}

	IEnumerator Routine_ShowHit(string str, float delay)
	{
		_textHint.text = str;
		yield return new WaitForSeconds(delay);
		_textHint.text = "";
	}

	

	void MakeResultView(string title, string pathname)
	{
		UIDisplaceView view = Instantiate(_displaceviewTmpl, _displaceviewTmpl.transform.position, _displaceviewTmpl.transform.rotation);
		view.name = title;
		view.transform.SetParent(_displaceviewTmpl.transform.parent);
		view.transform.localScale = _displaceviewTmpl.transform.localScale;
		view.gameObject.SetActive(true);
		view.ShowWithOneColumnCSV(title, pathname);
	}

	public void OnBtnPrint()
	{
		string dir = PatientMgr.GetTherapyResultDir(); // If directory does not exist, create it.
		if (!Directory.Exists(dir))
		{
			Directory.CreateDirectory(dir);
		}

		string path = dir + PatientMgr.GetCurrentPatientName() + ".pdf";

		PdfWriter writer;
		Document document;
		if (!PDFUtil.CreatePDFHeader(path, out document, out writer))
			return;
		PDFUtil.AddSectionBar("Displacement Result", document);
		PrintOneColumnCSV("Left Eye", PatientMgr.GetPatientDataDir() + "/data_left_displacement.csv", document, writer);
		document.Add(new Paragraph("\n"));
		PrintOneColumnCSV("Right Eye", PatientMgr.GetPatientDataDir() + "/data_right_displacement.csv", document, writer);
		document.Add(new Paragraph("\n"));

		//add Average Result
		string alternatepath = PatientMgr.GetPatientDataDir() + "/alternate_test.csv";
		if (File.Exists(alternatepath))
		{
			string[] strs = File.ReadAllLines(alternatepath);
			if (strs.Length > 1)
			{
				string[] valuestrs = strs[1].Split(new char[] { ',' });
				float rightAver = float.Parse(valuestrs[1]);
				float leftAver = float.Parse(valuestrs[2]);
				if (valuestrs.Length > 2)
				{
					iTextSharp.text.Font fntSubTitle = new iTextSharp.text.Font(PDFUtil.bfntHead, 14, 1, iTextSharp.text.BaseColor.BLUE);
					Paragraph prgSubTitle = new Paragraph();
					prgSubTitle.Alignment = Element.ALIGN_LEFT;

					Chunk chkSubTitle = new Chunk($"Average Displacement:   LEFT: {leftAver.ToString("G3")}   RIGHT: {rightAver.ToString("G3")}\n", fntSubTitle);
					prgSubTitle.Add(chkSubTitle);
					prgSubTitle.SpacingAfter = 40;
					document.Add(prgSubTitle);
				}
			}
		}
		
		document.Close();
		UtilityFunc.StartProcessByFile(path);
	}

	void PrintOneColumnCSV(string title, string pathname, Document document, PdfWriter writer)
	{
		PDFUtil.AddSubSection(title, document);
		if (!File.Exists(pathname))
		{
			return;
		}
		string[] str = File.ReadAllLines(pathname);
		if (str.Length < 10)
		{
			return;
		}
		List<float> valuelist = new List<float>();
		for (int i = 1; i < str.Length; i++)
		{
			string[] splitstrs = str[i].Split(new char[] { ',' });
			valuelist.Add(float.Parse(splitstrs[1]));
		}

		float width = 500;
		float height = 130;
		float rulerSize = 5;
		float Xpos = (document.PageSize.Width - width) / 2;
		float Ypos = writer.GetVerticalPosition(true) - 10;
		PdfContentByte contentByte = writer.DirectContent;
		contentByte.SetLineWidth(2);
		contentByte.SetColorStroke(iTextSharp.text.BaseColor.BLACK);
		contentByte.SetLineDash(0);

		//rect
		contentByte.Rectangle(Xpos, Ypos - height, width, height);
		contentByte.Stroke();

		//horline
		contentByte.SetLineWidth(1);
		contentByte.SetColorStroke(iTextSharp.text.BaseColor.GRAY);
		contentByte.SetLineDash(5, 2, 2);
		int horstepCount = 10;
		float widthPerS = width / horstepCount;
		for (int i = 1; i <= horstepCount; i++)
		{
			if (i != horstepCount)
			{
				contentByte.MoveTo(Xpos + widthPerS * i, Ypos);
				contentByte.LineTo(Xpos + widthPerS * i, Ypos - height - rulerSize);
				contentByte.Stroke();
			}
			contentByte.BeginText();
			contentByte.SetFontAndSize(PDFUtil.bfntHead, 12);
			contentByte.ShowTextAligned(Element.ALIGN_CENTER, i.ToString(), Xpos + widthPerS * i, Ypos - height - rulerSize - 10, 0);
			contentByte.EndText();
		}

		//verline
		float maxValue = -1;
		foreach (float value in valuelist)
		{
			maxValue = Math.Max(maxValue, value);
		}
		int verstepCount = 0;
		float valueStep = 0;
		int power = 0;
		if (maxValue <= 1)
		{
			verstepCount = 10;
			power = (int)(-Mathf.Log10(maxValue) + 1);
			valueStep = 1;
			//_textPower.text = (-power).ToString();
		}
		else if (maxValue <= 5)
		{
			valueStep = 1;
			verstepCount = (int)Mathf.Ceil(maxValue) + 1;
		}
		else
		{
			verstepCount = 5;
			valueStep = (int)(maxValue / 5) + 1;
		}
		if (power != 0)
			PDFUtil.ShowPower10((int)power, contentByte, Xpos, Ypos);
		float heightPerV = height / (verstepCount * valueStep);
		for (int i = 1; i <= verstepCount; i++)
		{
			
			if (i != verstepCount)
			{
				contentByte.MoveTo(Xpos + width, Ypos - height + heightPerV * i * valueStep);
				contentByte.LineTo(Xpos - rulerSize, Ypos - height + heightPerV * i * valueStep);
				contentByte.Stroke();
			}
		}
		for (int i = 1; i <= verstepCount; i++)
		{
			contentByte.BeginText();
			contentByte.SetFontAndSize(PDFUtil.bfntHead, 12);
			contentByte.ShowTextAligned(Element.ALIGN_CENTER, ((int)(valueStep * i)).ToString(), Xpos -rulerSize - 10, Ypos - height + heightPerV * i * valueStep - 4, 0);
			contentByte.EndText();
		}

		//draw graph
		int count = valuelist.Count;
		contentByte.SetColorStroke(iTextSharp.text.BaseColor.BLUE);
		contentByte.SetLineWidth(2);
		contentByte.SetLineDash(0);
		contentByte.SetLineJoin(PdfContentByte.LINE_JOIN_ROUND);
		int prevIndex = 0;
		contentByte.MoveTo(Xpos, Ypos - height + valuelist[prevIndex] * heightPerV * Mathf.Pow(10, power));
		for (int i = 1; i < width; i++)
		{
			int newindex = count * i / (int)width;
			if (newindex >= count)
				break;
			if (newindex != prevIndex)
			{
				contentByte.LineTo(Xpos + i, Ypos - height + valuelist[newindex] * heightPerV * Mathf.Pow(10, power));
				prevIndex = newindex;
			}
		}
		contentByte.Stroke();
		document.Add(new Paragraph("\n\n\n\n\n\n\n\n\n"));
		Paragraph prgTime = new Paragraph();
		prgTime.Alignment = Element.ALIGN_CENTER;
		prgTime.SpacingAfter = 10;
		Chunk chk = new Chunk("Time (s)\n");
		prgTime.Add(chk);
		document.Add(prgTime);
	}
	void SaveData(int x , int y)
	{
        PlayFabClientAPI.GetUserData(new GetUserDataRequest() { },
            result =>
            {
                 
                var prevJson = result.Data["Displacement"].Value;
                int count = Int32.Parse(result.Data["DiagnosticCount"].Value);
                //count++;

                DateTime now = DateTime.Now;
                string dateCurrent = now.ToShortDateString();

                UnityEngine.Debug.Log("DiagnosticCount VARIABLE IS" + count);
                JObject prevJObject = JObject.Parse(prevJson);
                JObject newSessionData = new JObject();
                newSessionData["x"] = x.ToString();
                newSessionData["y"] = y.ToString();
                newSessionData["Date"] = dateCurrent;
                
				string sessions = "Session" + count.ToString();
                prevJObject[sessions] = newSessionData;
                string updatedJson = prevJObject.ToString(Newtonsoft.Json.Formatting.Indented);

                var request = new UpdateUserDataRequest()
                {
                    Data = new Dictionary<string, string> { { "Displacement", updatedJson } },
                    Permission = UserDataPermission.Public
                };
                PlayFabClientAPI.UpdateUserData(request,
                 result =>
                 {
                     UnityEngine.Debug.Log("Successfully added Displacement data");
                 },
                 error =>
                 {
                     UnityEngine.Debug.Log("Not added Displacement data");

                 });
            },// Success callback
            error =>
            {
                UnityEngine.Debug.Log("Displacement data GetUserData api called error");

            });// Error callback
    }
}
