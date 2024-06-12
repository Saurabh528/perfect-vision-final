using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using UnityEngine;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using System;
using iTextSharp.text.pdf.parser;
using System.Diagnostics;
using System.Linq;
using System.Drawing;
using UnityEngine.UI;
using System.Drawing.Imaging;
using PlayFab;
using PlayFab.ClientModels;
using TMPro;

public class MyPdfPageEventHandler : PdfPageEventHelper
{
	const float horizontalPosition = 0.9f;  
	const float verticalPosition = 0.02f;    
    public static MyPdfPageEventHandler Instance = new MyPdfPageEventHandler();
	public override void OnEndPage(PdfWriter writer, Document document)
	{
		var footerText = new Phrase(writer.CurrentPageNumber.ToString());

		float posX = writer.PageSize.Width * horizontalPosition;
		float posY = writer.PageSize.Height * verticalPosition;
		float rotation = 0;
		writer.DirectContent.SetColorFill(iTextSharp.text.BaseColor.BLACK);
		ColumnText.ShowTextAligned(writer.DirectContent, Element.PHRASE, footerText, posX, posY, rotation);
	}
}

public class UISessionRecordView : MonoBehaviour
{
	public static UISessionRecordView Instance;
	public UISessionReportView _reportViewTmpl;
	public UISessionReportButton _ssReportButtonTmpl;
	[SerializeField] UIProgressView _progressViewTmpl;
	List<UIProgressView> _progressViews = new List<UIProgressView>();
	List<UIDisplacementProgressView> _DispprogressViews = new List<UIDisplacementProgressView>();
	List<UISessionReportButton> _ssReportButtons = new List<UISessionReportButton>();
	List<ColorCalibrationProgressView> _colorCaliViews = new List<ColorCalibrationProgressView> ();
	[SerializeField] UISessionReportView _highcoreView;
	[SerializeField] ScrollRect _scrollRect;
	[SerializeField] UIDisplacementProgressView _disProgressViewTmpl;
	[SerializeField] ColorCalibrationProgressView _colorCalibProgViewTmpl;
	[SerializeField] ColorCalibrationProgressView _colorCaliProgView;
	[SerializeField] GameObject _disProgressSection, _sessionProgressSection, _colorProgressSection, _btnExportPDF, diagGraphSection, diagGraphRow,
	objSpectacles_Exercises, obj_Summary_Suggestion, obj_DiagAnalysTable;
	[SerializeField] CalibrationSliderProgressView _calisliderView;
	[SerializeField] Text textDiagDate;
	[SerializeField] UIDiagnosticReportView diagnosticReportView;
	public TextMeshProUGUI textCrane2D;
    public TextMeshProUGUI textDisplacement;
    public TextMeshProUGUI textVAT;
    public TextMeshProUGUI textAlignment;
    public TextMeshProUGUI textWorth4DOT;
    private ScrollRect parentScrollRect;
    private Vector2 initialPointerPosition;
    private Vector2 initialScrollPosition;


    private void Awake()
	{
		Instance = this;
		parentScrollRect = GetComponentInParent<ScrollRect>();
        //textCrane2D make it disable
        /* textCrane2D.enabled = false;
		textDisplacement.enabled = false;
		textVAT.enabled = false;
		textAlignment.enabled = false;
		textWorth4DOT.enabled = false; */

    }

	void Update(){
		if(Input.GetKeyDown(KeyCode.D))
			UnityEngine.Debug.Log($"ScrollPos position:{_scrollRect.verticalNormalizedPosition}");
	}

	void LateUpdate(){
		if ( Input.touchCount > 1 ) {

			Vector2 totalMove = Vector2.zero;

			foreach (Touch thisTouch in Input.touches ) {
				totalMove += thisTouch.deltaPosition;
			}

			/* calculate average movement for each touch */

			Vector2 panMove    =  totalMove / Input.touchCount;
			if(panMove.y != 0){
				float position = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition + panMove.y / Screen.height * Time.deltaTime);
				_scrollRect.verticalNormalizedPosition = position;
			}
			
		}
		else{
			float Vertical = Input.GetAxis("Vertical");
			if(Vertical != 0){
				float position = Mathf.Clamp01(_scrollRect.verticalNormalizedPosition + Vertical * 0.1f * Time.deltaTime);
				_scrollRect.verticalNormalizedPosition = position;
			}
				
		}
	}
	public void LoadSessionData()
	{
		UnityEngine.Debug.Log("Load Session Data called");
		if (GameState.currentPatient == null)
		{
			Clear();
			return;
		}
		PatientDataMgr.LoadPatientData(GameState.currentPatient.name, OnLoadPatientSessionDataSuccess, ShowError);

	}

	void OnLoadPatientSessionDataSuccess()
	{
		UnityEngine.Debug.Log("On Load Patient Session Data Success called");
		ShowPatientSessionData();
	}

	void ShowError(string err)
	{
		EnrollmentManager.Instance.ShowMessage(err);
	}


	public void Clear()
	{
		foreach (UISessionReportButton btn in _ssReportButtons)
		{
			if(btn.gameObject != null)
				GameObject.Destroy(btn.gameObject);
		}
		_ssReportButtons.Clear();
		if(_reportViewTmpl)
			UtilityFunc.DeleteAllSideTransforms(_reportViewTmpl.transform);
		if(diagnosticReportView)
			diagnosticReportView.gameObject.SetActive(false);
		ClearProgressViews();
	}



	public void ShowPatientSessionData()
	{
		
		//1st called,3rd time called
		PatientRecord record = PatientDataMgr.GetPatientRecord();
		Clear();
		List<string> datelist = new List<string>();
		List<SessionRecord> sessionlist = record.GetSessionRecordList();
		foreach(SessionRecord session in sessionlist){
			DateTime dt = session.time;
			string datestr = dt.ToString(GameConst.STRFORMAT_DATETIME);
			if(!datelist.Contains(datestr))
				datelist.Add(datestr);
		}

		Dictionary<string, DiagnoseRecord> diagnoslist = record.GetDiagnoseRecords();
		foreach(KeyValuePair<string, DiagnoseRecord> pair in diagnoslist){
			if(!datelist.Contains(pair.Key))
				datelist.Add(pair.Key);
		}

		int siblingIndex = _ssReportButtonTmpl.transform.GetSiblingIndex();
		foreach (string datestr in datelist){
			UISessionReportButton btn = Instantiate(_ssReportButtonTmpl, _ssReportButtonTmpl.transform.position, _ssReportButtonTmpl.transform.rotation);
			btn.transform.SetParent(_ssReportButtonTmpl.transform.parent);
			btn.transform.localScale = _ssReportButtonTmpl.transform.localScale;
			btn.name = datestr;
			btn.gameObject.SetActive(true);
			btn.transform.SetSiblingIndex(++siblingIndex);
			btn.SetDateStr(datestr);
			_ssReportButtons.Add(btn);
		}
        
    }

	
	public void ShowSessionReport(UISessionReportButton button)
	{
		if(_reportViewTmpl != null)
			UtilityFunc.DeleteAllSideTransforms(_reportViewTmpl.transform);
		if(diagnosticReportView)
			diagnosticReportView.gameObject.SetActive(false);
		PatientRecord record = PatientDataMgr.GetPatientRecord();
		List<SessionRecord> sessionlist = record.GetSessionRecordList();
		int siblingIndex = button.transform.GetSiblingIndex() + 1;
		foreach(SessionRecord session in sessionlist){
			DateTime dt = session.time;
			string datestr = dt.ToString(GameConst.STRFORMAT_DATETIME);
			if(datestr == button.name){
				UISessionReportView newview = Instantiate(_reportViewTmpl, _reportViewTmpl.transform.position, _reportViewTmpl.transform.rotation);
				newview.transform.SetParent(_reportViewTmpl.transform.parent);
				newview.transform.localScale = _reportViewTmpl.transform.localScale;
				newview.ViewRecord(session);
				newview.transform.SetSiblingIndex(siblingIndex++);
				newview.gameObject.SetActive(true);
			}
		}
        
		Dictionary<string, DiagnoseRecord> diagnoRecords = record.GetDiagnoseRecords();
		foreach(KeyValuePair<string, DiagnoseRecord> pair in diagnoRecords){
			if(pair.Key == button.name && diagnosticReportView != null){
				diagnosticReportView.gameObject.SetActive(true);
				diagnosticReportView.transform.SetSiblingIndex(siblingIndex);
				diagnosticReportView.ShowRecord(pair.Value, $"Diagnostic Report - {pair.Key}");
				break;
			}
		}
		StartCoroutine(ScrollToTarget(button.GetComponent<RectTransform>()));
	}


	ColorCalibrationProgressView CreateColorCalibrationView(ColorChannel channel)
	{
        UnityEngine.Debug.Log("Color Callibiration VIew called");
        ColorCalibrationProgressView view = Instantiate(_colorCalibProgViewTmpl, _colorCalibProgViewTmpl.transform.position, _colorCalibProgViewTmpl.transform.rotation);
		view.transform.SetParent(_colorCalibProgViewTmpl.transform.parent);
		view.transform.localScale = _colorCalibProgViewTmpl.transform.localScale;
		view.name = channel.ToString();
		view.gameObject.SetActive(true);
		view.transform.SetSiblingIndex(_colorCalibProgViewTmpl.transform.GetSiblingIndex() + 1);
		view.CreateView(channel);
		return view;
	}
	public void OnBtnProgressionAnalysis()
	{
		
		UnityEngine.Debug.Log("On Btn Progression Analysis called");
		ClearProgressViews();
		List<DisplacementRecord> dispplaceRecords = GetMeanDisplacementList();
		if(dispplaceRecords !=  null &&  dispplaceRecords.Count != 0)
		{
			_disProgressSection.SetActive(true);
			CreateDisplacementProgressView("Right Eye", dispplaceRecords, UnityEngine.Color.black, EYESIDE.RIGHT);
			CreateDisplacementProgressView("Left Eye", dispplaceRecords, UnityEngine.Color.black, EYESIDE.LEFT);
		}

		if (PatientDataMgr.GetPatientRecord().GetSessionRecordList().Count == 0)
		{
			_colorProgressSection.SetActive(false);
		}
		else
		{
			_colorProgressSection.SetActive(true);
			_colorCaliViews.Add(CreateColorCalibrationView(ColorChannel.CC_Background));
			_colorCaliViews.Add(CreateColorCalibrationView(ColorChannel.CC_Cyan));
			_colorCaliViews.Add(CreateColorCalibrationView(ColorChannel.CC_Red));
		}

		_calisliderView.gameObject.SetActive(true);
		_calisliderView.Draw(ColorChannel.CC_Red);

		List<string> gamenames = GetRecodedGames();
		_sessionProgressSection.SetActive(gamenames.Count > 0);
		int siblingIndex = _progressViewTmpl.transform.GetSiblingIndex();
		foreach (string gamename in gamenames){
			UIProgressView view = Instantiate(_progressViewTmpl, _progressViewTmpl.transform.position, _progressViewTmpl.transform.rotation);
			view.transform.SetParent(_progressViewTmpl.transform.parent);
			view.transform.SetSiblingIndex(++siblingIndex);
			view.transform.localScale = _progressViewTmpl.transform.localScale;
			view.name = gamename;
			view.gameObject.SetActive(true);
			//view.ViewProgression(gamename);//show max score, max level, avg time
			view.ViewDateScoreProgression(gamename);//show score/date
			_progressViews.Add(view);
		}
		if(_btnExportPDF)
			_btnExportPDF.SetActive(true);
		StartCoroutine(ScrollToTarget(_highcoreView.GetComponent<RectTransform>()));
        //GetDataFunction();
    }

	public void OnBtnDiagnosticReport(){
		/*diagGraphSection.SetActive(true);
		textDiagDate.text = DateTime.Now.ToString("dd/MM/yyyy");
		DiagGraphItemData[] datas = new DiagGraphItemData[]{
			new DiagGraphItemData("Visual Acuity", 1),
			new DiagGraphItemData("Binocular Acuity", 0),
			new DiagGraphItemData("Stereo Acuity", 1),
			new DiagGraphItemData("Subjective Refraction", 1),
			new DiagGraphItemData("Positive Accommodation", 5),
			new DiagGraphItemData("Negative Accommodation", 3),
			new DiagGraphItemData("Accommodation Reserve", 4),
			new DiagGraphItemData("Accommodation Lag", 2),
			new DiagGraphItemData("Accommodation Flexibility", 1),
			new DiagGraphItemData("Phoria (H)", 4),
			new DiagGraphItemData("Phoria (V)", 5),
			new DiagGraphItemData("Fixation Disparity (H)", 5),
			new DiagGraphItemData("Fixation Disparity (V)", 5),
			new DiagGraphItemData("Fusional Reserves", 1)
		};
		UtilityFunc.DeleteAllSideTransforms(diagGraphRow.transform, false);
		RectTransform rt = diagGraphSection.GetComponent<RectTransform>();
		Vector2 sizeDelta = rt.sizeDelta;
		int rowheight = 107;
        sizeDelta.y = datas.Length * rowheight + 258;
        rt.sizeDelta = sizeDelta;
		for(int i = 0; i < datas.Length; i++){
			GameObject newrow = Instantiate(diagGraphRow, diagGraphRow.transform.position, diagGraphRow.transform.rotation);
			newrow.name = datas[i].name;
			newrow.transform.SetParent(diagGraphRow.transform.parent);
			newrow.transform.localScale = diagGraphRow.transform.localScale;
			newrow.GetComponent<RectTransform>().anchoredPosition = diagGraphRow.GetComponent<RectTransform>().anchoredPosition - new Vector2(0, i * rowheight);
			newrow.SetActive(true);
			newrow.transform.Find("Name").GetComponent<Text>().text = datas[i].name;
			newrow.transform.Find("Bar").GetComponent<UnityEngine.UI.Image>().color = DiagGraphItemData.colorByValue[datas[i].value];
			rt = newrow.transform.Find("Bar").GetComponent<RectTransform>();
			sizeDelta = rt.sizeDelta;
			sizeDelta.x = 250 * datas[i].value;
			rt.sizeDelta = sizeDelta;
		}*/
		//objSpectacles_Exercises.SetActive(true);
		//obj_Summary_Suggestion.SetActive(true);
		obj_DiagAnalysTable.SetActive(true);
		StartCoroutine(ScrollToTarget(obj_DiagAnalysTable.GetComponent<RectTransform>()));
	}
    void GetDataFunction()
    {
        PlayFabClientAPI.GetUserData(new GetUserDataRequest() { },
    result =>
    {

        //var prevJson = result.Data["Crane2D"].Value;
        //int count = Int32.Parse(result.Data["COUNT"].Value);
        // check if the session already exists.  If it exists, do not manipulate Json string. 
        /*var prevJson = result.Data["Crane2D"].Value;
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
        string updatedJson = prevJObject.ToString(Newtonsoft.Json.Formatting.Indented);*/

        /* UnityEngine.Debug.Log("Get User Data called");
        textCrane2D.enabled = true;
		textDisplacement.enabled = true;
		textVAT.enabled = true;
		textAlignment.enabled = true;
		textWorth4DOT.enabled = true;
        UnityEngine.Debug.Log(result.Data["Crane2D"].Value);
		textCrane2D.text = "CRANE2D: " + result.Data["Crane2D"].Value;
		textDisplacement.text = "Displacement: " + result.Data["Displacement"].Value;
        textVAT.text = "VAT: " + result.Data["VAT"].Value;
		textAlignment.text = "Alignment: " + result.Data["Alignment"].Value;
		textWorth4DOT.text = "Worth 4 DOT: " + result.Data["Worth4Dot"].Value; */
    },// Success callback
    error =>
    {
        UnityEngine.Debug.Log("Crane2D data GetUserData api called error");

    });// Error callback
    }


	public void ClearProgressViews()
	{
		_disProgressSection.SetActive(false);
		_sessionProgressSection.SetActive(false);
		_colorProgressSection.SetActive(false);
		foreach (UIProgressView pview in _progressViews)
			GameObject.Destroy(pview.gameObject);
		_progressViews.Clear();
		foreach (UIDisplacementProgressView pview in _DispprogressViews)
			GameObject.Destroy(pview.gameObject);
		_DispprogressViews.Clear();
		foreach (ColorCalibrationProgressView pview in _colorCaliViews)
			GameObject.Destroy(pview.gameObject);
		_colorCaliViews.Clear();
		_highcoreView.gameObject.SetActive(false);
		_calisliderView.Clear();
		_calisliderView.gameObject.SetActive(false);
		if(diagGraphSection)
			diagGraphSection.SetActive(false);
		/* objSpectacles_Exercises.SetActive(false);
		obj_Summary_Suggestion.SetActive(false); */
		if(obj_DiagAnalysTable)
			obj_DiagAnalysTable.SetActive(false);
		if(_btnExportPDF)
			_btnExportPDF.SetActive(false);
	}

	public void OnBtnExportPDF()
	{
		if (GameState.currentPatient == null)
			return;
        string dir = PatientMgr.GetTherapyResultDir(); // If directory does not exist, create it.
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }

        string path = dir + GameState.currentPatient.name + ".pdf";

        PdfWriter writer;
		Document document;
		if (!PDFUtil.CreatePDFHeader(path, out document, out writer))
			return;
		iTextSharp.text.pdf.BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
		//Add Progression Anylysis
		
		PDFUtil.AddSectionBar("Progression Anaylysis", document);

		//Add highscore
		PDFUtil.AddSubSection("Session HighScore", document);
		Dictionary<string, StatisData> highdatas = GetHighscoreData();
		PdfPTable table = new PdfPTable(highdatas.Count + 1);
		table.WidthPercentage = 50000f / (document.PageSize.Width - document.RightMargin - document.LeftMargin);
		iTextSharp.text.Font cellFont = new iTextSharp.text.Font(bfntHead, 11, 1, iTextSharp.text.BaseColor.BLACK);
		BaseColor cellcolor1 = new BaseColor(175, 161, 253);
		BaseColor cellcolor2 = new BaseColor(189, 178, 253);
		table.AddCell("");
		Dictionary<string, BaseColor> gameColors = new Dictionary<string, BaseColor>();
		gameColors[SessionMgr.GetGameName(0)] = new BaseColor(237, 161, 145);
		gameColors[SessionMgr.GetGameName(1)] = new BaseColor(237, 95, 171);
		gameColors[SessionMgr.GetGameName(2)] = new BaseColor(237, 95, 254);
		gameColors[SessionMgr.GetGameName(3)] = new BaseColor(103, 103, 254);
		gameColors[SessionMgr.GetGameName(4)] = new BaseColor(103, 235, 184);
		gameColors[SessionMgr.GetGameName(5)] = new BaseColor(181, 235, 145);
		gameColors[SessionMgr.GetGameName(6)] = new BaseColor(255, 134, 47);
		gameColors[SessionMgr.GetGameName(7)] = new BaseColor(255, 134, 47);
		foreach (KeyValuePair<string, StatisData> pair in highdatas)
        {
			PdfPCell cell = new PdfPCell(new Phrase(pair.Key, cellFont));
            cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = 30;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = gameColors[pair.Key];
			table.AddCell(cell);
        }
		PdfPCell scorecell = new PdfPCell(new Phrase("Max Score", cellFont));
		scorecell.HorizontalAlignment = Element.ALIGN_CENTER;
		scorecell.FixedHeight = 20;
		scorecell.VerticalAlignment = Element.ALIGN_MIDDLE;
		scorecell.BackgroundColor = cellcolor1;
		table.AddCell(scorecell);
		foreach (KeyValuePair<string, StatisData> pair in highdatas)
        {
			PdfPCell valuecell = new PdfPCell(new Phrase(pair.Value.maxScore == -1 ? "-" : pair.Value.maxScore.ToString()));
			valuecell.HorizontalAlignment = Element.ALIGN_CENTER;
			valuecell.VerticalAlignment = Element.ALIGN_MIDDLE;
			valuecell.BackgroundColor = cellcolor1;
			table.AddCell(valuecell);
		}
		PdfPCell levelcell = new PdfPCell(new Phrase("Max Level", cellFont));
		levelcell.HorizontalAlignment = Element.ALIGN_CENTER;
		levelcell.FixedHeight = 20;
		levelcell.VerticalAlignment = Element.ALIGN_MIDDLE;
		levelcell.BackgroundColor = cellcolor2;
		table.AddCell(levelcell);
		foreach (KeyValuePair<string, StatisData> pair in highdatas)
        {
			PdfPCell valuecell = new PdfPCell(new Phrase(pair.Value.maxLevel == -1 ? "-" : pair.Value.maxLevel.ToString()));
			valuecell.HorizontalAlignment = Element.ALIGN_CENTER;
			valuecell.VerticalAlignment = Element.ALIGN_MIDDLE;
			valuecell.BackgroundColor = cellcolor2;
			table.AddCell(valuecell);
		}
		PdfPCell timecell = new PdfPCell(new Phrase("Time / Level", cellFont));
		timecell.HorizontalAlignment = Element.ALIGN_CENTER;
		timecell.FixedHeight = 20;
		timecell.VerticalAlignment = Element.ALIGN_MIDDLE;
		timecell.BackgroundColor = cellcolor1;
		table.AddCell(timecell);
		foreach (KeyValuePair<string, StatisData> pair in highdatas)
        {
			PdfPCell valuecell = new PdfPCell(new Phrase(pair.Value.maxAvgTime == -1 ? "-" : ((int)pair.Value.maxAvgTime).ToString()));
			valuecell.HorizontalAlignment = Element.ALIGN_CENTER;
			valuecell.VerticalAlignment = Element.ALIGN_MIDDLE;
			valuecell.BackgroundColor = cellcolor1;
			table.AddCell(valuecell);
		}
		document.Add(table);
		document.Add(new Paragraph("\n"));

		//Add graphs
		List<string> gamenames = GetRecodedGames();
        foreach (string gamename in gamenames)
        {
            if (writer.GetVerticalPosition(true) < 190)
                document.NewPage();
            DrawProgressionGraphPDF(document, writer, gamename);
        }

		//Add Color Calibration
		document.NewPage();
		PDFUtil.AddSectionBar("Color Calibration Anaylysis", document);
		DrawColorCalibrationGraph(document, writer, ColorChannel.CC_Red);
		DrawColorCalibrationGraph(document, writer, ColorChannel.CC_Cyan);
		DrawColorCalibrationGraph(document, writer, ColorChannel.CC_Background);

		document.Close();

        

        //view pdf
		UtilityFunc.StartProcessByFile(path);


	}

	void DrawProgressionGraphPDF(Document document, PdfWriter writer, string gamename)
	{
        //Add gamename
        Dictionary<DateTime, float> timeValuelist = GetMeanDateScoreList(gamename);
		PDFUtil.AddSubSection(gamename, document);
		iTextSharp.text.pdf.BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

		PdfContentByte contentByte = writer.DirectContent;
        contentByte.SetLineWidth(2);
		contentByte.SetColorStroke(iTextSharp.text.BaseColor.BLACK);
		contentByte.SetLineDash(0);
		//Add graph
		float Ypos = writer.GetVerticalPosition(true);
        
        int horstepCount = Mathf.Min(8, timeValuelist.Count - 1);
        float width = 500;
        float height = 130;
        float rulerSize = 5;
        float Xpos = (document.PageSize.Width - width) / 2;
        float widthPerS = horstepCount == 0? width: width / horstepCount;

        //rect
        contentByte.Rectangle(Xpos, Ypos - height, width, height);
        contentByte.Stroke();

		//ver line
		contentByte.SetLineWidth(1);
        contentByte.SetColorStroke(iTextSharp.text.BaseColor.GRAY);
		contentByte.SetLineDash(5, 2, 2);
		contentByte.SetColorFill(iTextSharp.text.BaseColor.BLACK);
		
		for (int i = 0; i <= horstepCount; i++)
        {
            if (i != 0 && i != horstepCount)
            {   
                contentByte.MoveTo(Xpos + widthPerS * i, Ypos);
                contentByte.LineTo(Xpos + widthPerS * i, Ypos - height - rulerSize);
                contentByte.Stroke();
            }
            KeyValuePair<DateTime, float> pair = timeValuelist.ElementAt(timeValuelist.Count - 1 - horstepCount + i);
            contentByte.BeginText();
            contentByte.SetFontAndSize(bfntHead, 12);
            contentByte.ShowTextAligned(Element.ALIGN_CENTER, pair.Key.ToString("MMM d yy"), Xpos + widthPerS * i, Ypos - height - rulerSize - 10, 0);
            contentByte.EndText();
        }

        //hor line

		float maxValue = -1;
        foreach (KeyValuePair<DateTime, float> pair in timeValuelist)
        {
            if (maxValue < pair.Value)
                maxValue = pair.Value;
        }
        int verstepCount = 0;
        float valueStep = 0;
        if (maxValue <= 1)
        {
            verstepCount = 1;
            valueStep = 1;
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
        float heightPerV = height / (verstepCount * valueStep);
		for (int i = 1; i <= verstepCount; i++)
        {
            if (i != verstepCount)
            {
				contentByte.MoveTo(Xpos + width, Ypos - height + heightPerV * i * valueStep);
				contentByte.LineTo(Xpos - rulerSize, Ypos - height + heightPerV * i * valueStep);
				contentByte.Stroke();
			}
			contentByte.BeginText();
			//contentByte.SetFontAndSize(bfntHead, 12);
			contentByte.ShowTextAligned(Element.ALIGN_CENTER, ((int)(valueStep * i)).ToString(), Xpos - rulerSize - 10, Ypos - height + heightPerV * i  * valueStep - 4, 0);
			contentByte.EndText();
		}

		//draw graph
		contentByte.SetLineWidth(1);
		contentByte.SetColorStroke(iTextSharp.text.BaseColor.RED);
		contentByte.SetLineDash(0);
		int count = 0;
        PointF prevPos = new PointF(0, 0);
		foreach (KeyValuePair<DateTime, float> pair in timeValuelist)
        {
            /*if(count == 0)
				prevPos = new PointF(Xpos + widthPerS * count, Ypos - height + heightPerV * pair.Value * valueStep);
			contentByte.MoveTo(prevPos.X, prevPos.Y);
			prevPos = new PointF(Xpos + widthPerS * count, Ypos - height + heightPerV * pair.Value * valueStep);
			contentByte.LineTo(prevPos.X, prevPos.Y);
			contentByte.Stroke();		
			count++;*/
            if (count == 0)
                contentByte.MoveTo(Xpos + widthPerS * count, Ypos - height + heightPerV * pair.Value);
            else
				contentByte.LineTo(Xpos + widthPerS * count, Ypos - height + heightPerV * pair.Value);
            
			count++;
		}
		contentByte.Stroke();

		//draw points
		count = 0;
        float radius = 3;
        contentByte.SetColorFill(iTextSharp.text.BaseColor.GREEN);
        contentByte.SetColorStroke(iTextSharp.text.BaseColor.BLUE);
		foreach (KeyValuePair<DateTime, float> pair in timeValuelist)
        {
            float x = Xpos + widthPerS * count;
            float y = Ypos - height + heightPerV * pair.Value;
            contentByte.Ellipse(x - radius, y - radius, x + radius, y + radius);
			contentByte.FillStroke();
			count++;
		}
		document.Add(new Paragraph("\n\n\n\n\n\n\n\n\n"));



    }

	void DrawColorCalibrationGraph(Document document, PdfWriter writer, ColorChannel channel)
	{
		UnityEngine.Debug.Log("DrawColorCalibrationGraph called");
		Dictionary<DateTime, uint> timeValuelist = UISessionRecordView.GetSessionColorList(channel);
		PDFUtil.AddSubSection(UtilityFunc.ColorChannelToName(channel) + " Slider RGB Values Over Time", document);
		iTextSharp.text.pdf.BaseFont bfntHead = BaseFont.CreateFont(BaseFont.TIMES_ROMAN, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);

		PdfContentByte contentByte = writer.DirectContent;
		contentByte.SetLineWidth(2);
		contentByte.SetColorStroke(iTextSharp.text.BaseColor.BLACK);
		contentByte.SetLineDash(0);
		//Add graph
		float Ypos = writer.GetVerticalPosition(true);

		int horstepCount = Mathf.Min(8, timeValuelist.Count - 1);
		float width = 500;
		float height = 130;
		float rulerSize = 5;
		float Xpos = (document.PageSize.Width - width) / 2;
		float widthPerS = horstepCount == 0 ? width : width / horstepCount;

		//rect
		contentByte.Rectangle(Xpos, Ypos - height, width, height);
		contentByte.Stroke();

		//ver line
		contentByte.SetLineWidth(1);
		contentByte.SetColorStroke(iTextSharp.text.BaseColor.GRAY);
		contentByte.SetLineDash(5, 2, 2);
		contentByte.SetColorFill(iTextSharp.text.BaseColor.BLACK);

		for (int i = 0; i <= horstepCount; i++)
		{
			if (i != 0 && i != horstepCount)
			{
				contentByte.MoveTo(Xpos + widthPerS * i, Ypos);
				contentByte.LineTo(Xpos + widthPerS * i, Ypos - height - rulerSize);
				contentByte.Stroke();
			}
			KeyValuePair<DateTime, uint> pair = timeValuelist.ElementAt(timeValuelist.Count - 1 - horstepCount + i);
			contentByte.BeginText();
			contentByte.SetFontAndSize(bfntHead, 12);
			contentByte.ShowTextAligned(Element.ALIGN_CENTER, pair.Key.ToString("MMM d yy"), Xpos + widthPerS * i, Ypos - height - rulerSize - 10, 0);
			contentByte.EndText();
		}

		//hor line

		int verstepCount = 5;
		float valueStep = 50;
		float heightPerV = height / 270;
		for (int i = 1; i <= verstepCount; i++)
		{
			if(i != verstepCount)
			{
				contentByte.MoveTo(Xpos + width, Ypos - height + heightPerV * i * valueStep);
				contentByte.LineTo(Xpos - rulerSize, Ypos - height + heightPerV * i * valueStep);
				contentByte.Stroke();
			}
			else
			{
				contentByte.MoveTo(Xpos + width, Ypos - height + heightPerV * 255);
				contentByte.LineTo(Xpos - rulerSize, Ypos - height + heightPerV * 255);
				contentByte.Stroke();
			}

				
			contentByte.BeginText();
			if (i != verstepCount)
				contentByte.ShowTextAligned(Element.ALIGN_CENTER, ((int)(valueStep * i)).ToString(), Xpos - rulerSize - 10, Ypos - height + heightPerV * i  * valueStep - 4, 0);
			else
				contentByte.ShowTextAligned(Element.ALIGN_CENTER, "255", Xpos - rulerSize - 10, Ypos - height + heightPerV * 255 - 4, 0);
			contentByte.EndText();
		}

		//draw graph
		contentByte.SetLineWidth(1);
		BaseColor []graphcolor = { iTextSharp.text.BaseColor.RED, iTextSharp.text.BaseColor.GREEN, iTextSharp.text.BaseColor.BLUE };
		contentByte.SetLineDash(0);

		for(int channelIndex = 0; channelIndex < 3; channelIndex++)
		{
			contentByte.SetColorStroke(graphcolor[channelIndex]);
			int count = 0;
			foreach (KeyValuePair<DateTime, uint> pair in timeValuelist)
			{
				byte value = (byte)((pair.Value >> ((3 - channelIndex) * 8)) & 0xff);
				if (count == 0)
					contentByte.MoveTo(Xpos + widthPerS * count, Ypos - height + heightPerV * value);
				else
					contentByte.LineTo(Xpos + widthPerS * count, Ypos - height + heightPerV * value);
				count++;
			}
			contentByte.Stroke();
		}
		

		
		document.Add(new Paragraph("\n\n\n\n\n\n\n\n\n"));
	}

	public static List<string> GetRecodedGames()
    {
        List<string> gamenames = new List<string>();
        PatientRecord record = PatientDataMgr.GetPatientRecord();
        List<SessionRecord> sessionlist = record.GetSessionRecordList();
        foreach (SessionRecord ssrecord in sessionlist)
        {
            for (int i = 0; i < ssrecord.games.Count; i++)
            {
                if (!gamenames.Contains(ssrecord.games[i].name))
                    gamenames.Add(ssrecord.games[i].name);
            }
        }
        return gamenames;
    }

	public List<DisplacementRecord> GetDisplacementRecords()
	{
		PatientRecord record = PatientDataMgr.GetPatientRecord();
		return record.displacementRecords;
	}

	public static List<DisplacementRecord> GetMeanDisplacementList()
	{
		PatientRecord record = PatientDataMgr.GetPatientRecord();
		List<DisplacementRecord> recordlist = record.GetDisplacementRecordList();
		Dictionary<DateTime, DisplacementRecord> dicRecords = new Dictionary<DateTime, DisplacementRecord>();
		Dictionary<DateTime, int> countList = new Dictionary<DateTime, int>();
		foreach (DisplacementRecord disRecord in recordlist)
		{

			DateTime dt = new DateTime(disRecord.datetime.Year, disRecord.datetime.Month, disRecord.datetime.Day);
			if (dicRecords.ContainsKey(dt))
			{
				countList[dt]++;
				dicRecords[dt].aver_displace_left += disRecord.aver_displace_left;
				dicRecords[dt].aver_displace_right += disRecord.aver_displace_right;
			}
			else
			{
				DisplacementRecord newRecord = new DisplacementRecord(disRecord.aver_displace_left, disRecord.aver_displace_right);
				newRecord.datetime = disRecord.datetime;
				dicRecords[dt] = newRecord;
				countList[dt] = 1;
			}
		}

		foreach (KeyValuePair<DateTime, DisplacementRecord> pair in dicRecords)
		{
			pair.Value.aver_displace_left /= countList[pair.Key];
			pair.Value.aver_displace_right /= countList[pair.Key];
		}

		int maxCount = 8;
		if (dicRecords.Count > maxCount)
		{
			List<DisplacementRecord> reducedList = new List<DisplacementRecord>();
			KeyValuePair<DateTime, DisplacementRecord> itempair = dicRecords.ElementAt(0);
			reducedList.Add(itempair.Value);
			for (int i = 1; i <= maxCount - 2; i++)
			{
				itempair = dicRecords.ElementAt((int)((float)i * (dicRecords.Count - 1) / (maxCount - 1)));
				reducedList.Add(itempair.Value);
			}
			itempair = dicRecords.ElementAt(dicRecords.Count - 1);
			reducedList.Add(itempair.Value);
			return reducedList;
		}
		else
		{
			List<DisplacementRecord> List = new List<DisplacementRecord>();
			foreach (KeyValuePair<DateTime, DisplacementRecord> pair in dicRecords)
				List.Add(pair.Value);
			return List;
		}
	}

	public static Dictionary<DateTime, UInt32> GetSessionColorList(ColorChannel channel)
	{
		PatientRecord record = PatientDataMgr.GetPatientRecord();
		List<SessionRecord> sessionlist = record.GetSessionRecordList();
		Dictionary<DateTime, UInt32> colorList = new Dictionary<DateTime, UInt32>();
		UInt32 curColor = 0;
		foreach (SessionRecord ssrecord in sessionlist)
		{

			DateTime dt = new DateTime(ssrecord.time.Year, ssrecord.time.Month, ssrecord.time.Day);
			UInt32 color = 0;
			if (channel == ColorChannel.CC_Red)
				color = ssrecord.cali.red;
			else if (channel == ColorChannel.CC_Cyan)
				color = ssrecord.cali.cyan;
			else if (channel == ColorChannel.CC_Background)
				color = ssrecord.cali.back;
			if (curColor != color && color != 0)
			{
				if (colorList.Count >= 2 && color == colorList.ElementAt(colorList.Count - 2).Value)
					colorList.Remove(colorList.Last().Key);
				else if(colorList.Count == 0 || colorList.Last().Value != color)
				{
					colorList[dt] = color;
					curColor = color;
				}
				
			}
			

		}


		int maxCount = 8;
		if (colorList.Count > maxCount)
		{
			Dictionary<DateTime, UInt32> reducedList = new Dictionary<DateTime, UInt32>();
			KeyValuePair<DateTime, UInt32> itempair = colorList.ElementAt(0);
			reducedList.Add(itempair.Key, itempair.Value);
			for (int i = 1; i <= maxCount - 2; i++)
			{
				itempair = colorList.ElementAt((int)((float)i * (colorList.Count - 1) / (maxCount - 1)));
				reducedList.Add(itempair.Key, itempair.Value);
			}
			itempair = colorList.ElementAt(colorList.Count - 1);
			reducedList.Add(itempair.Key, itempair.Value);
			return reducedList;
		}
		else
			return colorList;
	}

	public static Dictionary<DateTime, UInt32> GetCalibSliderValueList()
	{
		PatientRecord record = PatientDataMgr.GetPatientRecord();
		List<SessionRecord> sessionlist = record.GetSessionRecordList();
		Dictionary<DateTime, UInt32> valueList = new Dictionary<DateTime, UInt32>();
		foreach (SessionRecord ssrecord in sessionlist)
		{

			valueList[ssrecord.time] = ssrecord.cali.slider;
		}

		return valueList;
	}

	public static Dictionary<DateTime, float>  GetMeanDateScoreList(string gamename)
    {
        PatientRecord record = PatientDataMgr.GetPatientRecord();
        List<SessionRecord> sessionlist = record.GetSessionRecordList();
        Dictionary<DateTime, float> scoreValueList = new Dictionary<DateTime, float>();
        Dictionary<DateTime, float> scoreSumList = new Dictionary<DateTime, float>();
        Dictionary<DateTime, int> countList = new Dictionary<DateTime, int>();
        foreach (SessionRecord ssrecord in sessionlist)
        {
            for (int i = 0; i < ssrecord.games.Count; i++)
            {
                GamePlay gp = ssrecord.games[i];
                if (gp.name == gamename)
                {
                    DateTime dt = new DateTime(ssrecord.time.Year, ssrecord.time.Month, ssrecord.time.Day);
                    if (scoreSumList.ContainsKey(dt))
                    {
                        countList[dt]++;
                        scoreSumList[dt] += GamePlay.GetConvertedScore(gp.eScr, gp.name);
                    }
                    else
                    {
                        scoreSumList[dt] = GamePlay.GetConvertedScore(gp.eScr, gp.name);
                        countList[dt] = 1;
                    }
                }
            }
        }

        foreach (KeyValuePair<DateTime, float> pair in scoreSumList)
        {
            scoreValueList[pair.Key] = scoreSumList[pair.Key] / countList[pair.Key];
        }

        int maxCount = 8;
		if (scoreValueList.Count > maxCount)
        {
			Dictionary<DateTime, float> reducedList = new Dictionary<DateTime, float>();
            KeyValuePair<DateTime, float> itempair = scoreValueList.ElementAt(0);
            reducedList.Add(itempair.Key, itempair.Value);
            for(int i = 1; i <= maxCount - 2; i++)
            {
                itempair = scoreValueList.ElementAt((int)((float)i * (scoreValueList.Count - 1) / (maxCount - 1)));
				reducedList.Add(itempair.Key, itempair.Value);
			}
			itempair = scoreValueList.ElementAt(scoreValueList.Count - 1);
			reducedList.Add(itempair.Key, itempair.Value);
            return reducedList;
		}
        else
		    return scoreValueList;
    }

    public static Dictionary<string, StatisData> GetHighscoreData()
    {
        string[] gamenames = SessionMgr.GetGameNames();
        Dictionary<string, StatisData> datas = new Dictionary<string, StatisData>();
		PatientRecord record = PatientDataMgr.GetPatientRecord();
		List<SessionRecord> sessionlist = record.GetSessionRecordList();
		for (int i = 0; i < gamenames.Length; i++)
		{
			float maxscore = -1, maxLevel = -1, maxAvgTime = -1;
            StatisData data = new StatisData();
			foreach (SessionRecord ssrecord in sessionlist)
			{
				for (int j = 0; j < ssrecord.games.Count; j++)
				{
					GamePlay gp = ssrecord.games[j];
					if (gp.name == gamenames[i])
					{
						if (GamePlay.GetConvertedScore(gp.eScr, gp.name) > maxscore)
							maxscore = GamePlay.GetConvertedScore(gp.eScr, gp.name);
						if (gp.eLvl > maxLevel)
							maxLevel = gp.eLvl;
						float avgtime = gp.sLvl == gp.eLvl ? 0 : ((float)gp.duration / (gp.eLvl - gp.sLvl));
						if (avgtime > maxAvgTime)
							maxAvgTime = avgtime;
					}
				}
			}
            data.maxScore = maxscore;
            data.maxLevel = maxLevel;
            data.maxAvgTime = maxAvgTime;
            datas[gamenames[i]] = data;
		}
        return datas;
	}

	void CreateDisplacementProgressView(string axisName, List<DisplacementRecord> records, UnityEngine.Color color, EYESIDE side)
	{
		UIDisplacementProgressView view = Instantiate(_disProgressViewTmpl, _disProgressViewTmpl.transform.position, _disProgressViewTmpl.transform.rotation);
		view.name = axisName;
		view.transform.SetParent(_disProgressViewTmpl.transform.parent);
		view.transform.localScale = _disProgressViewTmpl.transform.localScale;
		view.gameObject.SetActive(true);
		view.transform.transform.SetSiblingIndex(_disProgressViewTmpl.transform.GetSiblingIndex() + 1);
		view.DrawGraph(axisName, records, color, side);
		_DispprogressViews.Add(view);
	}

	IEnumerator ScrollToTarget(RectTransform target)
    {
		Canvas.ForceUpdateCanvases();
		float curPos = _scrollRect.verticalNormalizedPosition;
		float pivotAdjustment = target.rect.height * target.pivot.y;
		float localYTopLeft = target.localPosition.y + (target.rect.height - pivotAdjustment);

		float tgtPos = Mathf.Clamp01(1 - (-localYTopLeft) / (_scrollRect.content.rect.height - _scrollRect.viewport.rect.height));
		int count = 10;
		for (int i = 1; i <= count; i++)
		{
			yield return new WaitForSeconds(0.05f);
			_scrollRect.verticalNormalizedPosition = Mathf.Lerp(curPos, tgtPos, (float)i / count);
		}
    }

}
