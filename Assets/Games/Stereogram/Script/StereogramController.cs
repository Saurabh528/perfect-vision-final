using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StereogramController : DiagnosticController
{
    
	[SerializeField]
	private Image imageTimePercent;

	[SerializeField]
	private StereogramViewer stereogramViewer;

	[SerializeField]
	private GameObject playPanel;

	[SerializeField]
	private GameObject resultPanel, scorePanel, timePanel;

	[SerializeField]
	private GameObject symbolSection;

	[SerializeField]
	private StereogramSettingUI settingUI;

	[SerializeField]
	private TextMeshProUGUI textCorrectStatus;

	[SerializeField]
	private TextMeshProUGUI textWrongStatus;

	[SerializeField]
	private TweenAppear tweenArrowKeys;

	[SerializeField]
	private TweenAppear tweenTapSymbol;

	[SerializeField]
	private Sprite[] symbolIcons;

	private StereogramController.PatternDirection curPatDir;

	private StereogramController.SymbolType curSymbol;

	private DepthMode depthMode;

	private int customEyesIn = 10;

	private float jumpTime = 6f;

	private StereoOverlapMode overlapMode;
	ZDepth zDepth;
	TimeMode timeMode;

	private StereoTestMode testMode;

	private SizeMode sizeMode;

	private LevelMode levelMode;

	private float patternDelay;

	private int wrongCount, continuousWrongCount;

	private int distance;

	private int longDist;

	private int correct;

	private int score;

	private float remainTime;

	private float totalTime;

	private float patternStartTime;

	private StereoOverlapMode currentOverlapMode;

	private List<float> guessTimeList;
	bool isPlaying;

	public enum PatternDirection
	{
		Left,
		Right,
		Up,
		Down
	}

	public enum SymbolType
	{
		Triangle,
		Moon,
		Lightning,
		Heart,
		Star,
		Circle,
		Rectangle,
		X
	}
	public void Start()
	{
	}

	public void Update()
	{
		if(!isPlaying)
			return;
		if ((testMode == StereoTestMode.VisualPower && timeMode == TimeMode.MaxDistance) || this.remainTime > 0f)
		{
			if (this.testMode != StereoTestMode.VisualSymbol)
			{
				if (UnityEngine.Input.GetKeyDown(KeyCode.LeftArrow) || UnityEngine.Input.GetKeyDown(KeyCode.A))
				{
					this.CheckPatternByArrowKey(StereogramController.PatternDirection.Left);
				}
				else if (UnityEngine.Input.GetKeyDown(KeyCode.RightArrow) || UnityEngine.Input.GetKeyDown(KeyCode.D))
				{
					this.CheckPatternByArrowKey(StereogramController.PatternDirection.Right);
				}
				else if (UnityEngine.Input.GetKeyDown(KeyCode.UpArrow) || UnityEngine.Input.GetKeyDown(KeyCode.W))
				{
					this.CheckPatternByArrowKey(StereogramController.PatternDirection.Up);
				}
				else if (UnityEngine.Input.GetKeyDown(KeyCode.DownArrow) || UnityEngine.Input.GetKeyDown(KeyCode.S))
				{
					this.CheckPatternByArrowKey(StereogramController.PatternDirection.Down);
				}
			}
			/* this.patternDelay -= Time.deltaTime;
			if (this.patternDelay <= 0f && this.testMode != StereoTestMode.VisualSymbol)
			{
				if (this.depthMode == DepthMode.DepthIncrease)
				{
					this.distance--;
					if (this.distance == -1)
					{
						this.distance = 0;
					}
				}
				this.ShowNewPattern();
			} */
			this.imageTimePercent.fillAmount = this.remainTime / this.totalTime;
			this.remainTime -= Time.deltaTime;
			if (testMode != StereoTestMode.VisualPower || timeMode == TimeMode.Timed){
				if((testMode != StereoTestMode.VisualPower || timeMode == TimeMode.Timed) && this.remainTime < 0f)
				{
					this.remainTime = 0f;
					this.ShowResult();
				}
			}
		}
	}

	public void OnSymbolButton(Button button)
	{
		if (this.remainTime <= 0f)
		{
			return;
		}
		this.CheckPatternBySymbol(Enum.Parse<StereogramController.SymbolType>(button.name));
	}

	private void CheckPatternByArrowKey(StereogramController.PatternDirection dir)
	{
		if (dir == this.curPatDir)
		{
			this.OnCheckOK();
		}
		else
		{
			this.OnCheckFail();
		}
		this.ShowNewPattern();
	}

	private void CheckPatternBySymbol(StereogramController.SymbolType type)
	{
		if (type == this.curSymbol)
		{
			this.OnCheckOK();
		}
		else
		{
			this.OnCheckFail();
		}
		this.ShowNewPattern();
	}

	private void OnCheckOK()
	{
		continuousWrongCount = 0;
		if (this.depthMode == DepthMode.DepthIncrease)
		{
			this.distance++;
		}
		this.longDist = this.distance;
		this.guessTimeList.Add(Time.time - this.patternStartTime);
		this.correct++;
		if (this.correct == 1)
		{
			if (this.testMode == StereoTestMode.VisualSymbol)
				this.tweenTapSymbol.Disappear();
			else
				this.tweenArrowKeys.Disappear();
		}
		this.textCorrectStatus.text = this.correct.ToString();
		this.score++;
	}

	private void OnCheckFail()
	{
		if (this.depthMode == DepthMode.DepthIncrease)
		{
			this.distance--;
			if (this.distance == -1)
			{
				this.distance = 0;
			}
		}
		this.score--;
		if (this.score < 0)
		{
			this.score = 0;
		}
		this.wrongCount++;
		continuousWrongCount++;
		if(testMode == StereoTestMode.VisualPower && timeMode == TimeMode.MaxDistance && continuousWrongCount == 3)
			ShowResult();
		this.textWrongStatus.text = this.wrongCount.ToString();
	}

	public void OnBtnStart()
	{
		this.settingUI.SaveSetting();
        scorePanel.SetActive(true);
		testMode = settingUI.GetTestMode();
		this.depthMode = this.settingUI.GetDepthMode();
		if (this.depthMode == DepthMode.DepthCustom)
		{
			this.customEyesIn = this.settingUI.GetCustomEyesIn();
		}
		if (this.depthMode == DepthMode.Depth10)
		{
			this.distance = 3;
		}
		else if (this.depthMode == DepthMode.Depth20)
		{
			this.distance = 6;
		}
		else if (this.depthMode == DepthMode.Depth30)
		{
			this.distance = 9;
		}
		else if (this.depthMode == DepthMode.DepthCustom)
		{
			this.distance = this.customEyesIn;
		}
		else
		{
			this.distance = 0;
		}
		this.sizeMode = this.settingUI.GetSizeMode();
		this.levelMode = this.settingUI.GetLevelMode();
		this.overlapMode = this.settingUI.GetOverlapMode();
		if(testMode != StereoTestMode.VisualSymbol)
			overlapMode = StereoOverlapMode.EyesIn;
		this.currentOverlapMode = ((this.overlapMode == StereoOverlapMode.EyesOut) ? StereoOverlapMode.EyesOut : StereoOverlapMode.EyesIn);
		this.symbolSection.SetActive(this.testMode == StereoTestMode.VisualSymbol);
		zDepth = settingUI.GetZDepthMode();
		timeMode = settingUI.GetTimeMode();
		timePanel.SetActive (testMode != StereoTestMode.VisualPower || timeMode == TimeMode.Timed);
		if (this.testMode == StereoTestMode.VisualSymbol && this.levelMode == LevelMode.Level1)
		{
			StereogramController.SymbolType[] array = new StereogramController.SymbolType[5];
			for (int i = 0; i < 5; i++)
			{
				array[i] = (StereogramController.SymbolType)i;
			}
			this.SetSymbolButtons(array);
		}
		this.totalTime = (this.remainTime = this.settingUI.GetPlayTime());
		UnityEngine.Debug.Log(string.Format("Depth:{0}, JumpTime: {1}, Overlap: {2}, Test: {3}, PlayTime: {4}", new object[]
		{
			this.depthMode,
			this.jumpTime,
			this.overlapMode,
			this.testMode,
			this.remainTime
		}));
		this.settingUI.gameObject.SetActive(false);
		this.StartGamePlay();
	}

	public void OnBtnReStart()
	{
		if (this.depthMode == DepthMode.DepthIncrease)
		{
			this.distance = 0;
		}
		this.remainTime = this.totalTime;
		this.resultPanel.SetActive(false);
		this.StartGamePlay();
	}

	private void StartGamePlay()
	{
		isPlaying = true;
		this.correct = (this.wrongCount = (this.score = 0));
		this.textCorrectStatus.text = this.correct.ToString();
		this.textWrongStatus.text = this.wrongCount.ToString();
		this.guessTimeList = new List<float>();
		this.playPanel.SetActive(true);
		this.ShowNewPattern();
		if (this.testMode == StereoTestMode.VisualSymbol)
			this.tweenTapSymbol.Appear();
		else
			this.tweenArrowKeys.Appear();
	}

	private void ShowNewPattern()
	{
		this.patternDelay = this.jumpTime;
		float num = (this.sizeMode == SizeMode.Normal) ? 1f : 0.5f;
		this.stereogramViewer.transform.parent.localScale = new Vector3(num, num, 1f);
		string path = "";
		string path2 = "";
		if (this.testMode == StereoTestMode.VisualSymbol)
		{
			this.SelectVisualSymbolPatterns(out path, out path2);
		}
		else
		{
			StereogramController.PatternDirection patternDirection = this.curPatDir;
			System.Random random = new System.Random();
			int maxValue = Enum.GetNames(typeof(StereogramController.PatternDirection)).Length;
			patternDirection = (StereogramController.PatternDirection)random.Next(maxValue);
			/*while (patternDirection == this.curPatDir)
			{
				int maxValue = Enum.GetNames(typeof(StereogramController.PatternDirection)).Length;
				patternDirection = (StereogramController.PatternDirection)random.Next(maxValue);
			} */
			this.curPatDir = patternDirection;
			this.GetArrowPatternPath(out path, out path2);
		}
		Texture2D texture2D = Resources.Load<Texture2D>(path);
		if (texture2D == null)
		{
			UnityEngine.Debug.LogError("Sprite not found in Resources folder: redpath");
			return;
		}
		Texture2D texture2D2 = Resources.Load<Texture2D>(path2);
		if (texture2D2 == null)
		{
			UnityEngine.Debug.LogError("Sprite not found in Resources folder: cyanpath");
			return;
		}
		this.patternStartTime = Time.time;
		this.stereogramViewer.MergePatterns(texture2D, texture2D2, (this.currentOverlapMode == StereoOverlapMode.EyesIn) ? (this.distance * 8) : (-this.distance * 8));
	}

	private void SelectVisualSymbolPatterns(out string redpath, out string cyanpath)
	{
		StereogramController.SymbolType symbolType = this.curSymbol;
		System.Random random = new System.Random();
		while (symbolType == this.curSymbol)
		{
			int maxValue = (this.levelMode == LevelMode.Level1) ? 5 : Enum.GetNames(typeof(StereogramController.SymbolType)).Length;
			symbolType = (StereogramController.SymbolType)random.Next(maxValue);
		}
		this.curSymbol = symbolType;
		if (this.levelMode == LevelMode.Level2)
		{
			StereogramController.SymbolType[] array = new StereogramController.SymbolType[5];
			int num = random.Next(5);
			for (int i = 0; i < 5; i++)
			{
				array[i] = this.curSymbol;
			}
			for (int j = 0; j < 5; j++)
			{
				if (j != num)
				{
					StereogramController.SymbolType symbolType2 = this.curSymbol;
					while (symbolType2 == this.curSymbol || array.Contains(symbolType2))
					{
						int maxValue2 = Enum.GetNames(typeof(StereogramController.SymbolType)).Length;
						symbolType2 = (StereogramController.SymbolType)random.Next(maxValue2);
					}
					array[j] = symbolType2;
				}
			}
			this.SetSymbolButtons(array);
		}
		if (this.overlapMode == StereoOverlapMode.EyesMixed)
		{
			this.currentOverlapMode = ((this.currentOverlapMode == StereoOverlapMode.EyesIn) ? StereoOverlapMode.EyesOut : StereoOverlapMode.EyesIn);
		}
		this.GetSymbolPatternPath(out redpath, out cyanpath, this.currentOverlapMode);
	}

	private void GetArrowPatternPath(out string redpath, out string cyanpath)
	{
		if (this.testMode == StereoTestMode.VisualJump)
		{
			redpath = string.Format("StereogramPattern/Jump_{0}_{1}_EyesIn_{2}", zDepth, curPatDir, ColorChannel.CC_Red);
			cyanpath = string.Format("StereogramPattern/Jump_{0}_{1}_EyesIn_CC_Blue", zDepth, curPatDir);
		}
		else{
			redpath = string.Format("StereogramPattern/Power_{0}_EyesIn_{1}", curPatDir, ColorChannel.CC_Red);
			cyanpath = string.Format("StereogramPattern/Power_{0}_EyesIn_CC_Blue", curPatDir);
		}
		
	}

	private void GetSymbolPatternPath(out string redpath, out string cyanpath, StereoOverlapMode mode)
	{
		if (this.testMode != StereoTestMode.VisualSymbol)
		{
			UnityEngine.Debug.LogError("Can't generate path in {testMode} mode");
			redpath = "";
			cyanpath = "";
			return;
		}
		if (mode == StereoOverlapMode.EyesIn)
		{
			cyanpath = string.Format("StereogramPattern/Symbols_{0}_CC_Blue", mode);
			redpath = string.Format("StereogramPattern/Symbols_{0}_{1}_{2}", this.curSymbol, mode, ColorChannel.CC_Red);
			return;
		}
		if (mode == StereoOverlapMode.EyesOut)
		{
			redpath = string.Format("StereogramPattern/Symbols_{0}_{1}", mode, ColorChannel.CC_Red);
			cyanpath = string.Format("StereogramPattern/Symbols_{0}_{1}_CC_Blue", this.curSymbol, mode);
			return;
		}
		UnityEngine.Debug.LogError("Can't generate path in {mode} mode");
		redpath = "";
		cyanpath = "";
	}

	private void ShowResult()
	{
		isPlaying = false;
		this.playPanel.SetActive(false);
		this.resultPanel.SetActive(true);
		this.resultPanel.transform.Find("Score").GetComponent<Text>().text = this.score.ToString();
		this.resultPanel.transform.Find("Correct").GetComponent<Text>().text = this.correct.ToString();
		this.resultPanel.transform.Find("Errors").GetComponent<Text>().text = this.wrongCount.ToString();
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		if (this.guessTimeList.Count > 0)
		{
			num = this.guessTimeList.Max();
			num2 = this.guessTimeList.Min();
			num3 = this.guessTimeList.Average();
		}
		this.resultPanel.transform.Find("Fastest").GetComponent<Text>().text = num2.ToString("F2") + " sec";
		this.resultPanel.transform.Find("Slowest").GetComponent<Text>().text = num.ToString("F2") + " sec";
		this.resultPanel.transform.Find("Average").GetComponent<Text>().text = num3.ToString("F2") + " sec";
		this.resultPanel.transform.Find("TotalTime").GetComponent<Text>().text = UtilityFunc.ConvertSec2MMSS(this.totalTime - this.remainTime);
	}

	public override void AddResults()
	{
		PatientRecord patientRecord = PatientDataMgr.GetPatientRecord();
		DiagnoseTestItem diagnoseTestItem = new DiagnoseTestItem();
		diagnoseTestItem.AddValue(this.resultPanel.transform.Find("Correct").GetComponent<Text>().text);
		diagnoseTestItem.AddValue(this.resultPanel.transform.Find("Errors").GetComponent<Text>().text);
		patientRecord.AddDiagnosRecord("Stereography", diagnoseTestItem);
	}

	public override bool ResultExist()
	{
		return base.ResultExist() && this.remainTime == 0f;
	}

	public void SetSymbolButtons(StereogramController.SymbolType[] types)
	{
		if (types.Length != 5)
		{
			UnityEngine.Debug.LogError("Can have only 5 symbols.");
			return;
		}
		for (int i = 0; i < 5; i++)
		{
			this.symbolSection.transform.GetChild(i).GetComponent<Image>().sprite = this.symbolIcons[(int)types[i]];
			this.symbolSection.transform.GetChild(i).gameObject.name = types[i].ToString();
		}
	}

}
