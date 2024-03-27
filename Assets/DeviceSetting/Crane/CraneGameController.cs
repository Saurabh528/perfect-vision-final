using iTextSharp.text.pdf;
using iTextSharp.text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Diagnostics;
using System.IO;
using System.Drawing;
using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayFab.ClientModels;
using PlayFab;
public class CraneGameController : GamePlayController
{
	

    [SerializeField] RectTransform _rtArmblue, _rtArmred, _rtBoxblue, _rtBoxred, _rtArmPivot, _rtBoxPivot;
    [SerializeField] Slider _sliderPosition, _sliderDepth;
	[SerializeField] GameObject _btnCheck, _btnNext, _objExplain, _btnPrint;
	[SerializeField] Text _textResult;
	[SerializeField] Texture2D _imageForPDF;
	[SerializeField] Text _txtLevel, _txtScore;
	[SerializeField] CanvasScaler _canvasScaler;
    float _armBaseX, _armDistHalf, _boxDistHalf;
    float _orgArmPivotY;
    public float _controlSpeed = 20f, _fallingSpeed = 200;
    bool _dragPositionSlider, _dragDepthSlider, checking;
	
	List<CraneCheckResult>	_checkResult = new List<CraneCheckResult>();
    // Start is called before the first frame update
    public override void Start()
    {
        _orgArmPivotY = _rtArmPivot.localPosition.y;
		if (IsDiagnosticMode())
		{
			_txtLevel.gameObject.SetActive(false);
			_txtScore.gameObject.SetActive(false);
			textTime.gameObject.SetActive(false);
			StartGamePlay();
		}
		else
			base.Start();
	}

	// Update is called once per frame
	public override void Update()
    {
		base.Update();
		if (checking)
		{
			_rtArmPivot.anchoredPosition = new Vector3(_armBaseX, _rtArmPivot.anchoredPosition.y - _fallingSpeed * Time.deltaTime, 0);
			if(_rtArmPivot.anchoredPosition.y <= _rtBoxPivot.anchoredPosition.y)
			{
				checking = false;
				_rtArmPivot.anchoredPosition = new Vector3(_armBaseX, _rtBoxPivot.anchoredPosition.y, 0);
				ShowCheckResult();
			}
		}
		else if(_btnCheck.activeSelf && IsPlaying())
		{
			float hor = Input.GetAxis("Horizontal");
			if (hor != 0)
			{
				_armBaseX += hor * _controlSpeed * Time.deltaTime;
				UpdateArmPibot();
			}
			else if (_dragPositionSlider)
			{
				_armBaseX += _sliderPosition.value * _controlSpeed * Time.deltaTime;
				UpdateArmPibot();
			}
			else
				_sliderPosition.value = 0;

			float ver = Input.GetAxis("Vertical");
			if (ver != 0)
			{
				_armDistHalf += ver * _controlSpeed * Time.deltaTime;
				_armDistHalf = Mathf.Clamp(_armDistHalf, -100f, 100f);
				UpdateArms();
			}
			else if (_dragDepthSlider)
			{
				_armDistHalf += _sliderDepth.value * _controlSpeed * Time.deltaTime;
				UpdateArms();
			}
			else
				_sliderDepth.value = 0;
		}
       
	}

	public override void ShowScore()
	{
		_txtScore.text = $"Score {_score}";
	}

	public override void ShowLevel()
	{
		_txtLevel.text = $"Level {_level}";
	}

	public override void StartGamePlay()
	{
		base.StartGamePlay();
		StartNewGame();
	}

	void StartNewGame()
    {
		float maxBaseX = 200;
		if (!IsDiagnosticMode())
			maxBaseX = GamePlayController.GetDifficultyValue(1, maxBaseX / 5, 10, maxBaseX, _checkResult.Count + 1);
		_armBaseX = UnityEngine.Random.Range(-maxBaseX, maxBaseX);
        UpdateArmPibot();

		float maxDistHalf = 50;
		if (!IsDiagnosticMode())
			maxDistHalf = GamePlayController.GetDifficultyValue(1, maxDistHalf / 5, 10, maxDistHalf, _checkResult.Count + 1);
		_armDistHalf = UnityEngine.Random.Range(maxDistHalf, maxDistHalf * 2) * (UnityEngine.Random.value > 0.5f ? -1 : 1);
        UpdateArms();

		_boxDistHalf = UnityEngine.Random.Range(-20f, 20f);
		_rtBoxblue.anchoredPosition = new Vector3(_boxDistHalf, 0, 0);
		_rtBoxred.anchoredPosition = new Vector3(-_boxDistHalf, 0, 0);
		_textResult.gameObject.SetActive(false);
		_objExplain.SetActive(false);
		_btnNext.SetActive(false);
		_btnCheck.SetActive(true);
	}

    void UpdateArms()
    {
		_rtArmblue.anchoredPosition = new Vector3(_armDistHalf, 0, 0);
		_rtArmred.anchoredPosition = new Vector3(-_armDistHalf, 0, 0);
	}

    void UpdateArmPibot()
    {
		_rtArmPivot.anchoredPosition = new Vector3(_armBaseX, _orgArmPivotY, 0);
	}

	public void OnPositionPointerDown(PointerEventData eventData)
	{
        _dragPositionSlider = true;
	}

	public void OnPositionPointerUp(PointerEventData eventData)
	{
		_dragPositionSlider = false;
	}

	public void OnDepthPointerDown(PointerEventData eventData)
	{
		_dragDepthSlider = true;
	}

	public void OnDepthPointerUp(PointerEventData eventData)
	{
		_dragDepthSlider = false;
	}

	public void OnBtnCheck()
	{
		checking = true;
		_btnCheck.SetActive(false);
	}

	void ShowCheckResult()
	{
		/*
		//for arc seconds
		float depthCM = VisualFactor.CanvasToCM(_boxDistHalf - _armDistHalf, _canvasScaler);
		int depthDist = (int)VisualFactor.ScreenCMToArcSecond(depthCM);
		float positionCM = VisualFactor.CanvasToCM(_armBaseX, _canvasScaler);
		int positionDist = (int)VisualFactor.ScreenCMToArcSecond(positionCM);*/
		int depthDist = (int)(_boxDistHalf - _armDistHalf);
		_textResult.text = $"Depth distance: {depthDist}\r\nPosition distance: {(int)_armBaseX}";
        SaveData(depthDist, _armBaseX);
        _textResult.gameObject.SetActive(true);
		_objExplain.SetActive(true);

		if (!IsDiagnosticMode())
		{
			_btnNext.SetActive(true);
			_btnPrint.SetActive(true);
			_checkResult.Add(new CraneCheckResult(depthDist, (int)_armBaseX));
			IncreaseScore(GetScoreIncrease(depthDist, (int)_armBaseX));
		}
	}

	void SaveData(int x,float y)
	{
        //string filePath = Directory.GetCurrentDirectory() + "\\Python\\crane2d.txt";
        //UnityEngine.Debug.Log("Path is " + filePath);
        //y = (int)y;
        //try
        //{
        //    // Convert the integer to a string since WriteAllText expects string data.
        //    File.WriteAllText(filePath, "");
        //    File.AppendAllText(filePath, x.ToString());
        //    File.AppendAllText(filePath, " ");
        //    File.AppendAllText(filePath, y.ToString());         
        //    UnityEngine.Debug.Log("This is x" + x);
        //    UnityEngine.Debug.Log("This is y" + y);

        //}
        //catch (Exception ex)
        //{
        //    // If something goes wrong, this will print the error message.
        //    UnityEngine.Debug.Log("An error occurred: " + ex.Message);
        //}

        PlayFabClientAPI.GetUserData(new GetUserDataRequest() { },
            result =>
            {
                var prevJson = result.Data["Crane2D"].Value;
                int count = Int32.Parse(result.Data["COUNT"].Value); 
                count++;
                UnityEngine.Debug.Log("COUNT VARIABLE IS" + count);
                JObject prevJObject = JObject.Parse(prevJson);
                JObject newSessionData = new JObject();
                newSessionData["x"] = x.ToString();
                newSessionData["y"] = y.ToString();
                string sessions = "Sessions" + count.ToString();
                prevJObject[sessions] = newSessionData;
                string updatedJson = prevJObject.ToString(Newtonsoft.Json.Formatting.Indented);

                var request = new UpdateUserDataRequest()
                {
                    Data = new Dictionary<string, string> { { "Crane2D", updatedJson } },
                    Permission = UserDataPermission.Public
                };
                PlayFabClientAPI.UpdateUserData(request,
                 result =>
                 {
                     UnityEngine.Debug.Log("Successfully added crane2D data");
                 },
                 error =>
                 {
                     UnityEngine.Debug.Log("Not added crane2D data");

                 });
            },// Success callback
            error =>
            {
                UnityEngine.Debug.Log("Crane2D data GetUserData api called error");

            });// Error callback
    }
	int GetScoreIncrease(int depDist, int posDist, int intDest = 0)
	{
		if (Mathf.Abs(depDist) <= 5 && Mathf.Abs(posDist) <= 5 && Mathf.Abs(intDest) <= 5)
			return 2;
		else if (Mathf.Abs(depDist) <= 10 && Mathf.Abs(posDist) <= 10 && Mathf.Abs(intDest) <= 10)
			return 1;
		else return 0;
	}


	public void OnBtnNext()
	{
		StartNewGame();
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
		PDFUtil.AddSectionBar("CraneGame Report", document);
		System.Drawing.Image image = UtilityFunc.Texture2Image(_imageForPDF);
		if(image != null)
		{
			iTextSharp.text.Image img = iTextSharp.text.Image.GetInstance(image, BaseColor.WHITE);
			img.Alignment = iTextSharp.text.Image.ALIGN_CENTER;
			img.ScaleToFit(300, 300);
			document.Add(img);
		}

		//Similarity table
		PDFUtil.AddSubSection("Distance Result", document);
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

		cell = new PdfPCell(new Phrase("Depth distance", cellFont));
		cell.HorizontalAlignment = Element.ALIGN_CENTER;
		cell.FixedHeight = 30;
		cell.VerticalAlignment = Element.ALIGN_MIDDLE;
		cell.BackgroundColor = new BaseColor(223, 170, 41);
		table.AddCell(cell);

		cell = new PdfPCell(new Phrase("Position distance", cellFont));
		cell.HorizontalAlignment = Element.ALIGN_CENTER;
		cell.FixedHeight = 30;
		cell.VerticalAlignment = Element.ALIGN_MIDDLE;
		cell.BackgroundColor = new BaseColor(223, 111, 107);
		table.AddCell(cell);


		int count = 0;
		foreach(CraneCheckResult result in _checkResult)
		{
			count++;
			cell = new PdfPCell(new Phrase(count.ToString(), cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = 30;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0 ? cellcolor1 : cellcolor2;
			table.AddCell(cell);

			cell = new PdfPCell(new Phrase(result.depthDist.ToString(), cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = 30;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0 ? cellcolor1 : cellcolor2;
			table.AddCell(cell);

			cell = new PdfPCell(new Phrase(result.positionDist.ToString(), cellFont));
			cell.HorizontalAlignment = Element.ALIGN_CENTER;
			cell.FixedHeight = 30;
			cell.VerticalAlignment = Element.ALIGN_MIDDLE;
			cell.BackgroundColor = count % 2 == 0 ? cellcolor1 : cellcolor2;
			table.AddCell(cell);

		}

		document.Add(table);
		document.Add(new Paragraph("\n\n"));
		document.Close();
		UtilityFunc.StartProcessByFile(path);

	}

	public void OnBtnHome()
	{
		Cursor.visible = true;
		Time.timeScale = 1;
		if (GameState.MODE_DOCTORTEST)
			ChangeScene.LoadScene("Diag_ForTest");
		else if (GameState.currentGameMode == GAMEMODE.DeviceSetting)
			ChangeScene.LoadScene("Diagnostic");
		else
			EndGame();
	}
}
