using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class FusionalRangesController : DiagnosticController
{
	public enum FusionStage
	{
		None,
		BIStarted,
		BIBreak,
		BOStarted,
		BOBreak
	}
	[SerializeField]
	Texture2D[] rawTextures, patternTextures;
	[SerializeField]
	RawImage outputImage;
	[SerializeField]
	int depthPixel = 5;
	[SerializeField]
	float disparityMMStep = 3;
	[SerializeField]
	AudioClip audioSuccess, audioFail;
	[SerializeField]
	private GameObject resultPanel;
	[SerializeField]
	Texture2D[] characterMarks;
	int currentPatternIndex;
	int roundNumber;
	FusionStage currentStage;
	float disparityMM, BIBreakMM, BIRecoverMM, BOBreakMM, BORecoverMM;
	int successCount, wrongCount;
    // Start is called before the first frame update
    void Start()
    {
		StartGamePlay();

	}

    // Update is called once per frame
    void Update()
    {
		if (currentStage != FusionStage.None)
		{
			if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				if (currentPatternIndex == 0)
					OnGuessSuccess();
				else
					OnGuessWrong();
			}
			else if (Input.GetKeyDown(KeyCode.LeftArrow))
			{
				if (currentPatternIndex == 1)
					OnGuessSuccess();
				else
					OnGuessWrong();
			}
			else if (Input.GetKeyDown(KeyCode.RightArrow))
			{
				if (currentPatternIndex == 2)
					OnGuessSuccess();
				else
					OnGuessWrong();
			}
			else if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				if (currentPatternIndex == 3)
					OnGuessSuccess();
				else
					OnGuessWrong();
			}
		}
    }

	void OnGuessSuccess()
	{
		AudioSource.PlayClipAtPoint(audioSuccess, Camera.main.transform.position);
		successCount++;
		wrongCount = 0;
		if(currentStage != FusionStage.BIBreak && currentStage != FusionStage.BOBreak)
			disparityMM += disparityMMStep;
		
		if(successCount == 3)
		{
			successCount = 0;
			if (currentStage == FusionStage.BIBreak)
			{
				BIRecoverMM = disparityMM;
				disparityMM = 0;
				StartCoroutine(ChangeState(2, FusionStage.BOStarted));
			}
			else if (currentStage == FusionStage.BOBreak)
			{
				BORecoverMM = disparityMM;
				StartCoroutine(ShowResult(2));
			}
			else
				ShowNewPattern();
		}
		else
			ShowNewPattern();
	}	

	void OnGuessWrong()
	{
		AudioSource.PlayClipAtPoint(audioFail, Camera.main.transform.position);
		wrongCount++;
		successCount = 0;
		if(currentStage != FusionStage.BIStarted && currentStage != FusionStage.BOStarted)
			disparityMM -= disparityMMStep;

		if (disparityMM < 0)
			disparityMM = 0;
		if (wrongCount == 2)
		{
			wrongCount = 0;
			if (currentStage == FusionStage.BIStarted && disparityMM > 10)
			{
				BIBreakMM = disparityMM;
				StartCoroutine(ChangeState(5, FusionStage.BIBreak));
			}
			else if (currentStage == FusionStage.BOStarted && disparityMM > 10)
			{
				BOBreakMM = disparityMM;
				StartCoroutine(ChangeState(5, FusionStage.BOBreak));
			}
			else
				ShowNewPattern();
		}
		else
			ShowNewPattern();
	}

	IEnumerator ShowResult(float delay)
	{
		currentStage = FusionStage.None;
		yield return new WaitForSeconds(delay);
		outputImage.gameObject.SetActive(false);
		resultPanel.SetActive(true);
		Debug.Log($"BIBreakMM: {BIBreakMM}, BOBreakMM: {BOBreakMM}");
		Transform transImage = resultPanel.transform.Find("Image");
		transImage.Find("BIBreakValue").GetComponent<Text>().text = $"{DiopterUtil.ConvertDisparityMMToDiopter(BIBreakMM, 400).ToString("F2")} BI Break";
		transImage.Find("BIRecoveryValue").GetComponent<Text>().text = $"{DiopterUtil.ConvertDisparityMMToDiopter(BIRecoverMM, 400).ToString("F2")} BI Recovery";
		transImage.Find("BOBreakValue").GetComponent<Text>().text = $"{DiopterUtil.ConvertDisparityMMToDiopter(BOBreakMM, 400).ToString("F2")} BO Break";
		transImage.Find("BORecoveryValue").GetComponent<Text>().text = $"{DiopterUtil.ConvertDisparityMMToDiopter(BORecoverMM, 400).ToString("F2")} BO Recovery";
		/*transImage.Find("BIBreakValue").GetComponent<Text>().text = $"{(DiopterUtil.ConvertDisparityMMToDiopter(BIBreakMM, 500) * 2.3f).ToString("F2")} BI Break";
		transImage.Find("BIRecoveryValue").GetComponent<Text>().text = $"{(DiopterUtil.ConvertDisparityMMToDiopter(BIRecoverMM, 500) * 1.6f).ToString("F2")} BI Recovery";
		transImage.Find("BOBreakValue").GetComponent<Text>().text = $"{(DiopterUtil.ConvertDisparityMMToDiopter(BOBreakMM, 500) * 1.82f).ToString("F2")} BO Break";
		transImage.Find("BORecoveryValue").GetComponent<Text>().text = $"{(DiopterUtil.ConvertDisparityMMToDiopter(BORecoverMM, 500) * 1.53f).ToString("F2")} BO Recovery";*/
	}

	

	IEnumerator ChangeState(float delay, FusionStage nextState)
	{
		Debug.Log($"Change state to {nextState}");
		currentStage = FusionStage.None;
		yield return new WaitForSeconds(delay);
		successCount = wrongCount = 0;
		currentStage = nextState;
		ShowNewPattern();
	}

	void StartGamePlay()
	{
		resultPanel.SetActive(false);
		currentStage = FusionStage.BIStarted;
		disparityMM = 0;
		roundNumber = 0;
		ShowNewPattern();
	}

	void ShowNewPattern()
	{
		currentPatternIndex = Random.Range(0, patternTextures.Length);
		roundNumber++;
		Texture2D leftImage, rightImage;
		DepthMerger.GenerateRandomDotChannelFromShape(rawTextures[roundNumber % rawTextures.Length], patternTextures[currentPatternIndex], depthPixel * ((currentStage == FusionStage.BIStarted || currentStage == FusionStage.BIBreak)?-1:1),
			(currentStage == FusionStage.BIStarted || currentStage == FusionStage.BIBreak), out leftImage, out rightImage);
		Texture2D mark = characterMarks[Random.Range(0, characterMarks.Length)];
		DepthMerger.InsertMarkIntoTexture2D(leftImage, 251, 251, mark, (currentStage == FusionStage.BIStarted || currentStage == FusionStage.BIBreak) ? ColorChannel.CC_Cyan : ColorChannel.CC_Red, out leftImage);
		DepthMerger.InsertMarkIntoTexture2D(rightImage, 251, 251, mark, (currentStage == FusionStage.BIStarted || currentStage == FusionStage.BIBreak) ? ColorChannel.CC_Red : ColorChannel.CC_Cyan, out rightImage);
		int disparityPixel = DPICaculator.ConvertScreenMMToPixel(outputImage.canvas, disparityMM, outputImage.transform.localScale.x);
		Texture2D outTexture = DepthMerger.GenerateDepthImageFromLeftRightImage(leftImage, rightImage, disparityPixel);
		outputImage.texture = outTexture;
		outputImage.GetComponent<RectTransform>().sizeDelta = new Vector2(outTexture.width, outTexture.height);
	}

	public override bool ResultExist()
	{
		return resultPanel.activeSelf;
	}

	public override void AddResults()
	{
		PatientRecord patientRecord = PatientDataMgr.GetPatientRecord();
		DiagnoseTestItem diagnoseTestItem = new DiagnoseTestItem();
		Transform transImage = resultPanel.transform.Find("Image");
		diagnoseTestItem.AddValue(transImage.Find("BIBreakValue").GetComponent<Text>().text);
		diagnoseTestItem.AddValue(transImage.Find("BIRecoveryValue").GetComponent<Text>().text);
		diagnoseTestItem.AddValue(transImage.Find("BOBreakValue").GetComponent<Text>().text);
		diagnoseTestItem.AddValue(transImage.Find("BORecoveryValue").GetComponent<Text>().text);
		patientRecord.AddDiagnosRecord("FusionalRanges", diagnoseTestItem);
	}
}
