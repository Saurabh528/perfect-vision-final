using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using System.Diagnostics;
using System.IO;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Org.BouncyCastle.Asn1.Mozilla;
using PlayFab;
using PlayFab.ClientModels;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
public class DotAlignmentController : GamePlayController {

	public Text textScore;
	[SerializeField] Text textLevel, _textInstruction;
	[SerializeField] TextMeshProUGUI _textStatus;
	[SerializeField] float spawnperiod = 3;
	float spawnremainTime;
	TCPListener tcp;
	bool _finished = false;
	bool _failed = false;
	Process pythonProcess;
	[SerializeField] GameObject _objOutput, _btnPrint, _btnHelp;
	[SerializeField] SimilarityTableRow _simTblRowTmpl;
	[SerializeField] SimilarityGraph _simGraphTmpl;
	[SerializeField] Transform _dotParent;
	int _curDotIndex = -1;
	List<SimilarityResult> similarityResults = new List<SimilarityResult>();
	List<IrisState> irisList = new List<IrisState>();
	List<float> similarValueList = new List<float>();
	
	// Use this for initialization
	public override void Start () {
		tcp = GetComponent<TCPListener>();
		spawnremainTime = spawnperiod;
	}

	

	



	public override void  Update () {
		base.Update();
		if (GamePlayController.Instance.IsPlaying()) {
			if(_curDotIndex != -1)
			{
				spawnremainTime -= Time.deltaTime;
				if (spawnremainTime < 0)
				{
					spawnremainTime += spawnperiod;
					MoveDot();
				}
			}
			
		}
		if (_finished)
		{
			StopPlay();
			if(_failed)
				_textInstruction.text = "Checking failed.";
			else
				StartCoroutine(Routine_Finish());
			_finished = false;
		}
	}

	void MoveDot()
	{
		int newIndex = _curDotIndex;
		while (newIndex == _curDotIndex)
			newIndex = UnityEngine.Random.Range(0, 9);
		_curDotIndex = newIndex;
		ShowCurDot();
	}

	void ShowCurDot()
	{
		foreach(Transform child in _dotParent)
		{
			child.gameObject.SetActive(false);
		}
		_dotParent.GetChild(_curDotIndex).gameObject.SetActive(true);
	}

	IEnumerator Routine_Finish()
	{
		
		_textInstruction.text = "Checking finished.";
		yield return new WaitForSeconds(2);
		_textInstruction.text = "";
		ShowResult();
		
	}

	void ShowResult()
	{
		foreach (Transform child in _dotParent)
		{
			child.gameObject.SetActive(false);
		}
		//read similar values
		string outputPath = PatientMgr.GetPatientDataDir() + "/out_data.csv";
		if (File.Exists(outputPath))
		{
			string[] lines = File.ReadAllLines(outputPath);
			for (int i = 1; i < lines.Length; i++)
			{
				string[] words = lines[i].Split(new char[] { ',' });
				similarValueList.Add(float.Parse(words[2]));
			}
		}

		textLevel.gameObject.SetActive(false);
		textScore.gameObject.SetActive(false);
		textTime.gameObject.SetActive(false);
		_objOutput.SetActive(true);
		Canvas.ForceUpdateCanvases();
		similarityResults.Clear();
		irisList.Clear();
		string groupPath = PatientMgr.GetPatientDataDir() + "/grouped_output.csv";
		if (File.Exists(groupPath))
		{
			int count = 0;
			string[] lines = File.ReadAllLines(groupPath);
			for(int i = 1; i < lines.Length; i++)
			{
				string[] words = lines[i].Split(new char[] { ',' });
				if (words[2] != "0.0") {
					string[] periods = words[1].Split(new char[] { '-' });
					float minVal = float.Parse(periods[0]);
					float maxVal = float.Parse(periods[1]);
					float sum = 0, countValue = 0;
					foreach(float simvalue in similarValueList)
					{
						if(minVal <= simvalue && simvalue <= maxVal)
						{
							sum += simvalue;
							countValue++;
						}
					}

					similarityResults.Add(new SimilarityResult(words[1], words[2]));
					SimilarityTableRow row = Instantiate(_simTblRowTmpl, _simTblRowTmpl.transform.position, _simTblRowTmpl.transform.rotation);
					row.transform.SetParent(_simTblRowTmpl.transform.parent);
					row.transform.localScale = _simTblRowTmpl.transform.localScale;
					row.gameObject.SetActive(true);
					row.SetData(++count, words[1], words[2], countValue == 0? 0: (sum / countValue));
				}
			}
		}

		string similarityPath = PatientMgr.GetPatientDataDir() + "/out_data.csv";
		if (File.Exists(similarityPath))
		{
			string[] lines = File.ReadAllLines(similarityPath);
			for (int i = 1; i < lines.Length; i++)
			{
				string[] words = lines[i].Split(new char[] { ',' });
				IrisState sim = new IrisState();
				sim.LeLd = float.Parse(words[3]);
				sim.LeRd = float.Parse(words[4]);
				sim.ReLd = float.Parse(words[5]);
				sim.ReRd = float.Parse(words[6]);
				irisList.Add(sim);
			}

			CreateSimilarityGraph(irisList, IRISSIMCLASS.LELD);
			CreateSimilarityGraph(irisList, IRISSIMCLASS.LERD);
			CreateSimilarityGraph(irisList, IRISSIMCLASS.RELD);
			CreateSimilarityGraph(irisList, IRISSIMCLASS.RERD);
			_btnPrint.SetActive(true);
			_btnHelp.SetActive(true);
			_textStatus.gameObject.SetActive(false);
		}
	}

	void CreateSimilarityGraph(List<IrisState> irisList, IRISSIMCLASS isclass)
	{
		SimilarityGraph graph = Instantiate(_simGraphTmpl, _simGraphTmpl.transform.position, _simGraphTmpl.transform.rotation);
		Canvas.ForceUpdateCanvases();
		graph.transform.SetParent(_simGraphTmpl.transform.parent);
		graph.transform.localScale = _simGraphTmpl.transform.localScale;
		graph.gameObject.SetActive(true);
		graph.Draw(irisList, isclass);
		if(isclass == IRISSIMCLASS.LERD)
		{
			graph.transform.localPosition = new Vector3(graph.transform.localPosition.x, -graph.transform.localPosition.y, graph.transform.localPosition.z);
		}
		else if (isclass == IRISSIMCLASS.RELD)
		{
			graph.transform.localPosition = new Vector3(-graph.transform.localPosition.x, graph.transform.localPosition.y, graph.transform.localPosition.z);
		}
		else if (isclass == IRISSIMCLASS.RERD)
		{
			graph.transform.localPosition = new Vector3(-graph.transform.localPosition.x, -graph.transform.localPosition.y, graph.transform.localPosition.z);
		}
	}


	public override void StartGamePlay()
	{
		base.StartGamePlay();
		_textInstruction.text = "Connecting to camera...";
		tcp.InitTCP();
		StartPythonAlignment();
	}

	public void OnTCPConnected()
	{
		_textInstruction.text = "";
	}

	void StartPythonAlignment()
	{
		int camindex = GlobalSettingUI.GetCurrentCameraIndex();
		if(camindex == -1){
			_textInstruction.text = "No Web camera is instaled.";
			return;
		}
		UnityEngine.Debug.Log($"Cameraindex: {camindex}");

#if UNITY_EDITOR
		string path = Application.dataPath + "/..";
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName         = "python.exe";
		_processStartInfo.Arguments        = $"{path}/Python/Alignment/final_pos_sim.py --connect --{GameConst.PYARG_CAMERAINDEX}={camindex}";
		//_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
#else
		string path = Application.dataPath + "/..";
		ProcessStartInfo _processStartInfo = new ProcessStartInfo();
		_processStartInfo.WorkingDirectory = path;
		_processStartInfo.FileName         = "final_pos_sim.exe";
		_processStartInfo.Arguments        = $" --connect --quiet --{GameConst.PYARG_CAMERAINDEX}={camindex}";
		_processStartInfo.WindowStyle   = ProcessWindowStyle.Hidden;
		pythonProcess = Process.Start(_processStartInfo);
#endif
		if (pythonProcess != null)
		{
			pythonProcess.EnableRaisingEvents = true;
			pythonProcess.Exited += OnPythonProcessExited;
		}
		else
			_textInstruction.text = "Checking failed. Try again.";
	}


	private void OnPythonProcessExited(object sender, System.EventArgs e)
	{
		_finished = true;
	}


	public void OnBtnHome()
	{
		Cursor.visible = true;
		Time.timeScale = 1;
		ChangeScene.LoadScene("DeviceSetting");
		
	}

	public void OnRecvString(System.String message)
	{
		if (message.StartsWith("CMD:"))
		{
			string cmdstr = message.Substring(4);
			if (cmdstr == "FAIL")
			{
				_failed = true;
			}
			else if (cmdstr == "EXIT")
			{
				//tcp.StopTCP();
			}
			else if (cmdstr == "START")
			{
				spawnremainTime = spawnperiod;
				_curDotIndex = 4;
				ShowCurDot();
			}
		}
		else if (message.StartsWith("MSG:"))
		{
			if (_textInstruction)
				_textInstruction.text = message.Substring(4);
		}
		else if (message.StartsWith("STS:"))
		{
			if (_textStatus)
				_textStatus.text = message.Substring(4);
		}
	}

	public override void StopPlay()
	{
		base.StopPlay();
		tcp.StopTCP();
		GameObject[] eggs = GameObject.FindGameObjectsWithTag("Fruits");
		foreach (GameObject go in eggs)
			GameObject.Destroy(go);
	}

	private void OnDestroy()
	{
		if(pythonProcess != null)
		{
			pythonProcess.Dispose();
		}
		tcp.StopTCP();
	}

	public void OnBtnPrintPDF()
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
		PDFUtil.AddSectionBar("Alignment Result", document);

		//Similarity table
		PDFUtil.AddSubSection("Similarity Table", document);
		PdfPTable table = new PdfPTable(3);
		table.WidthPercentage = 50000f / (document.PageSize.Width - document.RightMargin - document.LeftMargin);
		iTextSharp.text.Font cellFont = new iTextSharp.text.Font(PDFUtil.bfntHead, 11, 1, iTextSharp.text.BaseColor.BLACK);
		BaseColor cellcolor1 = new BaseColor(175, 161, 253);
		BaseColor cellcolor2 = new BaseColor(189, 178, 253);

		PdfPCell cell = new PdfPCell(new Phrase("No", cellFont));
		cell.HorizontalAlignment = Element.ALIGN_CENTER;
		cell.FixedHeight = 30;
		cell.VerticalAlignment = Element.ALIGN_MIDDLE;
		cell.BackgroundColor = new BaseColor(158, 104, 168);
		table.AddCell(cell);

		cell = new PdfPCell(new Phrase("Similarity", cellFont));
		cell.HorizontalAlignment = Element.ALIGN_CENTER;
		cell.FixedHeight = 30;
		cell.VerticalAlignment = Element.ALIGN_MIDDLE;
		cell.BackgroundColor = new BaseColor(223, 170, 41);
		table.AddCell(cell);

		cell = new PdfPCell(new Phrase("Duration (s)", cellFont));
		cell.HorizontalAlignment = Element.ALIGN_CENTER;
		cell.FixedHeight = 30;
		cell.VerticalAlignment = Element.ALIGN_MIDDLE;
		cell.BackgroundColor = new BaseColor(223, 111, 107);
		table.AddCell(cell);
		int count = 0;
		foreach(SimilarityResult similarityResult in similarityResults)
		{
			count++;
			cell = new PdfPCell(new Phrase(count.ToString(), cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = 30;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0? cellcolor1: cellcolor2;
			table.AddCell(cell);

			cell = new PdfPCell(new Phrase(similarityResult.similarity, cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = 30;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0 ? cellcolor1 : cellcolor2;
			table.AddCell(cell);

			cell = new PdfPCell(new Phrase(similarityResult.duration.ToString(), cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = 30;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0 ? cellcolor1 : cellcolor2;
			table.AddCell(cell);
		}

		document.Add(table);
		document.Add(new Paragraph("\n\n"));
		//Position Similarity
		float sizeX = 250;
		float sizeY = 150;
		if (writer.GetVerticalPosition(true) < sizeY * 2 + 70)
			document.NewPage();
		PDFUtil.AddSubSection("Position Similarity", document);
		document.Add(new Paragraph("\n"));
		float Xpos = (document.PageSize.Width - sizeX * 2) / 2 + 20;
		float Ypos = writer.GetVerticalPosition(true) - 10;
		DrawSimilarityGraph(writer, irisList, Xpos, Ypos, sizeX, sizeY, IRISSIMCLASS.LELD);
		DrawSimilarityGraph(writer, irisList, Xpos, Ypos, sizeX, sizeY, IRISSIMCLASS.LERD);
		DrawSimilarityGraph(writer, irisList, Xpos, Ypos, sizeX, sizeY, IRISSIMCLASS.RELD);
		DrawSimilarityGraph(writer, irisList, Xpos, Ypos, sizeX, sizeY, IRISSIMCLASS.RERD);

		document.Close();
		UtilityFunc.StartProcessByFile(path);
	}

	void DrawSimilarityGraph(PdfWriter writer, List<IrisState> irisList, float Xpos, float Ypos, float sizeX, float sizeY, IRISSIMCLASS isclass)
	{
		float width = sizeX - 40;
		float height = sizeY - 40;
		float rulerSize = 5;
		if (isclass == IRISSIMCLASS.RELD)
			Xpos += sizeX;
		else if (isclass == IRISSIMCLASS.LERD)
			Ypos -= sizeY;
		else if (isclass == IRISSIMCLASS.RERD)
		{
			Xpos += sizeX;
			Ypos -= sizeY;
		}
		PdfContentByte contentByte = writer.DirectContent;
		contentByte.SetLineWidth(2);
		contentByte.SetColorStroke(iTextSharp.text.BaseColor.BLACK);
		contentByte.SetLineDash(0);

		//rect
		contentByte.Rectangle(Xpos, Ypos - height, width, height);
		contentByte.Stroke();

		//Draw Axis
		contentByte.BeginText();
		contentByte.SetFontAndSize(PDFUtil.bfntHead, 14);
		contentByte.ShowTextAligned(Element.ALIGN_LEFT, isclass.ToString(), Xpos, Ypos + 5, 0);
		contentByte.EndText();

		//horline
		contentByte.SetLineWidth(1);
		contentByte.SetColorStroke(iTextSharp.text.BaseColor.GRAY);
		contentByte.SetLineDash(5, 2, 2);
		int horstepCount = 0;
		float valueStep = 0;
		if (irisList.Count <= 5)
		{
			valueStep = 1;
			horstepCount = irisList.Count;
		}
		else
		{
			horstepCount = 6;
			valueStep = (int)(irisList.Count / 6) + 1;
		}
		float widthPerS = width / horstepCount;
		float widthPerValue = width / (horstepCount * valueStep);
		contentByte.SetColorStroke(iTextSharp.text.BaseColor.GRAY);
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
			contentByte.ShowTextAligned(Element.ALIGN_CENTER, (i * valueStep).ToString(), Xpos + widthPerS * i, Ypos - height - rulerSize - 10, 0);
			contentByte.EndText();
		}

		//verline
		int verstepCount = 5;
		valueStep = 10;
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
			contentByte.ShowTextAligned(Element.ALIGN_CENTER, ((int)(valueStep * i)).ToString(), Xpos  - rulerSize - 10, Ypos - height + heightPerV * i * valueStep - 4, 0);
			contentByte.EndText();
		}

		//DRAW graph
		contentByte.SetColorStroke(iTextSharp.text.BaseColor.BLUE);
		contentByte.SetLineWidth(2);
		contentByte.SetLineDash(0);
		contentByte.SetLineJoin(PdfContentByte.LINE_JOIN_ROUND);
		float value = 0;
		if (isclass == IRISSIMCLASS.LELD)
			value = irisList.First().LeLd;
		else if (isclass == IRISSIMCLASS.LERD)
			value = irisList.First().LeRd;
		else if (isclass == IRISSIMCLASS.RELD)
			value = irisList.First().ReLd;
		else
			value = irisList.First().ReRd;
		contentByte.MoveTo(Xpos, Ypos - height + value * heightPerV);
		for (int i = 1; i < irisList.Count; i++)
		{
			if (isclass == IRISSIMCLASS.LELD)
				value = irisList[i].LeLd;
			else if (isclass == IRISSIMCLASS.LERD)
				value = irisList[i].LeRd;
			else if (isclass == IRISSIMCLASS.RELD)
				value = irisList[i].ReLd;
			else
				value = irisList[i].ReRd;
			contentByte.LineTo(Xpos + widthPerValue * (i + 1), Ypos - height + value * heightPerV);
		}
		contentByte.Stroke();
	}
	void SaveData(int x , int y)
	{
        PlayFabClientAPI.GetUserData(new GetUserDataRequest() { },
            result =>
            {
                 
                var prevJson = result.Data["Alignment"].Value;
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
                    Data = new Dictionary<string, string> { { "Alignment", updatedJson } },
                    Permission = UserDataPermission.Public
                };
                PlayFabClientAPI.UpdateUserData(request,
                 result =>
                 {
                     UnityEngine.Debug.Log("Successfully added Alignment data");
                 },
                 error =>
                 {
                     UnityEngine.Debug.Log("Not added Alignment data");

                 });
            },// Success callback
            error =>
            {
                UnityEngine.Debug.Log("Alignment data GetUserData api called error");

            });// Error callback
    }
}

