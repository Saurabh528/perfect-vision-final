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
using TouchControlsKit;
using System;

public class CraneCheckResult
{
	public int depthDist, positionDist, intervalDist;
	public CraneCheckResult(int depDist, int posDist, int intDist = 0)
	{
		depthDist = depDist;
		positionDist = posDist;
		intervalDist = intDist;
	}
}
public class Crane3DGameController : GamePlayController
{
	

    [SerializeField] Transform _rtArmblue, _rtArmred, _rtBoxblue, _rtBoxred, _rtArmPivot, _rtBoxPivot;
    [SerializeField] Slider _sliderPosition;
	[SerializeField] GameObject _btnCheck, _btnNext, _objExplain, _btnPrint;
	[SerializeField] Text _textResult;
	[SerializeField] Texture2D _imageForPDF;
	[SerializeField] Text _txtLevel, _txtScore;
    float _armBaseX, _armDistHalf, _boxDistHalf, _armIntervalHalf, _boxIntervalHalf;
    float _orgArmPivotY;
    public float _controlSpeed = 20f, _fallingSpeed = 200;
    bool _dragPositionSlider, checking;
	
	List<CraneCheckResult>	_checkResult = new List<CraneCheckResult>();
	// Start is called before the first frame update
	public override void Start()
    {
		_orgArmPivotY = _rtArmPivot.localPosition.y;
		if(IsDiagnosticMode())
		{
			_txtLevel.gameObject.SetActive(false);
			_txtScore.gameObject.SetActive(false);
			textTime.gameObject.SetActive(false) ;
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
			_rtArmPivot.localPosition = new Vector3(_armBaseX, _rtArmPivot.localPosition.y - _fallingSpeed * Time.deltaTime, _rtArmPivot.localPosition.z);
			if(_rtArmPivot.localPosition.y <= _rtBoxPivot.localPosition.y)
			{
				checking = false;
				_rtArmPivot.localPosition = new Vector3(_armBaseX, _rtBoxPivot.localPosition.y, _rtArmPivot.localPosition.z);
				ShowCheckResult();
			}
		}
		else if(_btnCheck.activeSelf && IsPlaying())
		{
			float hor = Input.GetAxis("Horizontal");
			if (hor != 0)
			{
				_armDistHalf += hor * _controlSpeed * Time.deltaTime;
				UpdateArms();
			}
			float ver = Input.GetAxis("Vertical");
			if (ver != 0)
			{
				_armIntervalHalf += ver * _controlSpeed * Time.deltaTime;
				UpdateArms();
			}

			if (_dragPositionSlider)
			{
				_armBaseX += _sliderPosition.value * _controlSpeed * Time.deltaTime;
				UpdateArmPibot();
			}
			else
				_sliderPosition.value = 0;

			
			Vector2 touchInput = TCKInput.GetAxis("Joystick");
			if(touchInput != Vector2.zero)
			{
				_armDistHalf += touchInput.x * _controlSpeed * Time.deltaTime;
				_armIntervalHalf += touchInput.y * _controlSpeed * Time.deltaTime;
				UpdateArms();
			}
			
			
		}

	}

	public override void OnScoreChange(int levelstartscore, int score)
	{
		base.OnScoreChange(levelstartscore, score);
		if (score >= levelstartscore + 3)
			IncreaseLevel();
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
		float maxBaseX = 0.5f;
		if (!IsDiagnosticMode())
			maxBaseX = GamePlayController.GetDifficultyValue(1, maxBaseX / 5, 10, maxBaseX, _checkResult.Count + 1);
		_armBaseX = UnityEngine.Random.Range(-maxBaseX, maxBaseX);
        UpdateArmPibot();

		float maxDistHalf = 0.02f;
		if (!IsDiagnosticMode())
			maxDistHalf = GamePlayController.GetDifficultyValue(1, maxDistHalf / 5, 10, maxDistHalf, _checkResult.Count + 1);
		_armDistHalf = UnityEngine.Random.Range(maxDistHalf, maxDistHalf * 2) * (UnityEngine.Random.value > 0.5f ? -1 : 1);
		_armIntervalHalf = UnityEngine.Random.Range(-0.05f, 0.05f);
		UpdateArms();

		_boxDistHalf = UnityEngine.Random.Range(-0.05f, 0.05f);
		_boxIntervalHalf = UnityEngine.Random.Range(-0.05f, 0.05f);
		
		_rtBoxblue.localPosition = new Vector3(_boxDistHalf, 0.0001f, _boxIntervalHalf);
		_rtBoxred.localPosition = new Vector3(-_boxDistHalf, 0, -_boxIntervalHalf);
		_textResult.gameObject.SetActive(false);
		_objExplain.SetActive(false);
		_btnNext.SetActive(false);
		_btnCheck.SetActive(true);
	}

    void UpdateArms()
    {
		_rtArmblue.localPosition = new Vector3(_armDistHalf, _rtArmblue.localPosition.y, _armIntervalHalf);
		_rtArmred.localPosition = new Vector3(-_armDistHalf, _rtArmred.localPosition.y, -_armIntervalHalf);
	}

    void UpdateArmPibot()
    {
		_rtArmPivot.localPosition = new Vector3(_armBaseX, _orgArmPivotY, _rtArmPivot.localPosition.z);
	}

	public void OnPositionPointerDown(PointerEventData eventData)
	{
        _dragPositionSlider = true;
	}

	public void OnPositionPointerUp(PointerEventData eventData)
	{
		_dragPositionSlider = false;
	}

	public void OnBtnCheck()
	{
		checking = true;
		_btnCheck.SetActive(false);
	}

	void ShowCheckResult()
	{
		float showRate = 500;
		int depthDist = (int)((_boxDistHalf - _armDistHalf) * showRate);
		int intervalDist = (int)((_boxIntervalHalf - _armIntervalHalf) * showRate);
		int positionDist = (int)(_armBaseX * showRate);
		/*//for arc seconds
		Vector3 posArmPivot = Camera.main.WorldToScreenPoint(_rtArmPivot.position);
		Vector3 posBoxPivot = Camera.main.WorldToScreenPoint(_rtBoxPivot.position);
		int positionDist = (int)VisualFactor.ScreenCMToArcSecond(VisualFactor.CanvasToCM(posBoxPivot.x - posArmPivot.x));
		Vector3 posRedCrane = Camera.main.WorldToScreenPoint(_rtArmred.position);
		Vector3 posCyanCrane = Camera.main.WorldToScreenPoint(_rtArmblue.position);
		Vector3 posRedBox = Camera.main.WorldToScreenPoint(_rtBoxred.position);
		Vector3 posCyanBox = Camera.main.WorldToScreenPoint(_rtBoxblue.position);
		int depthDist = (int)VisualFactor.ScreenCMToArcSecond(VisualFactor.CanvasToCM(posRedCrane.x - posCyanCrane.x - (posRedBox.x - posCyanBox.x)));
		int intervalDist = (int)VisualFactor.ScreenCMToArcSecond(VisualFactor.CanvasToCM(posRedCrane.y - posCyanCrane.y - (posRedBox.y - posCyanBox.y)));*/
		_textResult.text = $"Depth distance: {depthDist}\r\nInterval distance: {intervalDist}\r\nPosition distance: {positionDist}";

		SaveData(depthDist,intervalDist,positionDist);

		_textResult.gameObject.SetActive(true);
		_objExplain.SetActive(true);
		if (!IsDiagnosticMode())
		{
			_btnNext.SetActive(true);
			_btnPrint.SetActive(true);
			_checkResult.Add(new CraneCheckResult(depthDist, positionDist, intervalDist));
			IncreaseScore(GetScoreIncrease(depthDist, positionDist, intervalDist));
		}
		
	}

	void SaveData(int x,int y,int z)
	{
        //int numberToSave = 123; // This is the integer you want to save.
        //string filePath = "D:\\PROJECTS\\perfect-vision-aman2\\Python\\crane3d.txt"; // The path to the file where you want to save the integer.
        string filePath = Directory.GetCurrentDirectory() + "\\Python\\crane3d.txt";
        UnityEngine.Debug.Log("Path is "+filePath);
        try
        {
			// Convert the integer to a string since WriteAllText expects string data.
			File.WriteAllText(filePath, "");
            File.AppendAllText(filePath, x.ToString());
            File.AppendAllText(filePath, " ");
            File.AppendAllText(filePath, y.ToString());
            File.AppendAllText(filePath, " ");
            File.AppendAllText(filePath, z.ToString());
            File.AppendAllText(filePath, " ");
            UnityEngine.Debug.Log("This is x" + x);
            UnityEngine.Debug.Log("This is y" + y);
            UnityEngine.Debug.Log("This is z" + z);
        }
        catch (Exception ex)
        {
            // If something goes wrong, this will print the error message.
            UnityEngine.Debug.Log("An error occurred: " + ex.Message);
        }
    }
	int GetScoreIncrease(int depDist, int posDist, int intDest)
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
		PdfPTable table = new PdfPTable(4);
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

		cell = new PdfPCell(new Phrase("Interval distance", cellFont));
		cell.HorizontalAlignment = Element.ALIGN_CENTER;
		cell.FixedHeight = 30;
		cell.VerticalAlignment = Element.ALIGN_MIDDLE;
		cell.BackgroundColor = new BaseColor(91, 231, 170);
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

			cell = new PdfPCell(new Phrase(result.intervalDist.ToString(), cellFont));
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
		else if(GameState.currentGameMode == GAMEMODE.DeviceSetting)
			ChangeScene.LoadScene("Diagnostic");
		else
			EndGame();
	}
}
