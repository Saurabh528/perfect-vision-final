using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SocialPlatforms.Impl;
using UnityEngine.UI;

public class FusionalRangesGameController : GamePlayController
{
    enum BaseMode{
        BaseIn,
        BaseOut
    }
	[SerializeField]
	Texture2D[] rawTextures, patternTextures;
	[SerializeField]
	RawImage outputImage;
	[SerializeField]
	int depthPixel = 5;
	[SerializeField]
	float disparityMMStep = 3;
	/* [SerializeField]
	private GameObject resultPanel; */
    [SerializeField]
    Text _scoreText, _levelText;
	[SerializeField]
	Texture2D[] characterMarks;
	int currentPatternIndex;
	int roundNumber;
    [SerializeField]
	BaseMode mode;
	float disparityMM, BreakMM, RecoverMM;
	int successCount, wrongCount;
    bool waitingInput;
    bool breakState = false;//normal or break state
    // Start is called before the first frame update

    
    public override void StartGamePlay()
	{
		base.StartGamePlay();
        ShowNewPattern();
        waitingInput = true;
    }
    // Update is called once per frame
    public override void Update()
    {
        base.Update();
		if (waitingInput)
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
		PlayCorrectSound();
		successCount++;
		wrongCount = 0;
		if(!breakState){
			disparityMM += disparityMMStep;
            if(_score < disparityMM / disparityMMStep){
                _score = (int)(disparityMM / disparityMMStep);
                _level = 1 + _score / 3;
                ShowScore();
                ShowLevel();
            }
        }
		else if(successCount == 3)
		{
			successCount = 0;
			breakState = false;
		}
		
        ShowNewPattern();
	}	

    public override void ShowScore() {
        _scoreText.text = $"Score {_score}";
    }
    public override void ShowLevel() { 
        _levelText.text = $"Level {_level}";
    }
	void OnGuessWrong()
	{
		PlayWrongSound();
		wrongCount++;
		successCount = 0;
		if(breakState){
			disparityMM -= disparityMMStep;
            if (disparityMM < 0)
                disparityMM = 0;
            ShowNewPattern();
        }
		else if (wrongCount == 2)
		{
			wrongCount = 0;
            breakState = true;
            disparityMM -= disparityMMStep;
            if (disparityMM < 0)
                disparityMM = 0;
            StartCoroutine (Routine_DelayAndShowPattern(3));
		}
		else
            ShowNewPattern();
	}

	/* IEnumerator ShowResult(float delay)
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
		
	} */

	IEnumerator Routine_DelayAndShowPattern(float time){
        waitingInput = false;
        yield return new WaitForSeconds(time);
        waitingInput = true;
        ShowNewPattern();
    }

	/* IEnumerator ChangeState(float delay, FusionStage nextState)
	{
		Debug.Log($"Change state to {nextState}");
		currentStage = FusionStage.None;
		yield return new WaitForSeconds(delay);
		successCount = wrongCount = 0;
		currentStage = nextState;
		ShowNewPattern();
	} */

	/* void StartGamePlay()
	{
		resultPanel.SetActive(false);
		currentStage = FusionStage.BIStarted;
		disparityMM = 0;
		roundNumber = 0;
		ShowNewPattern();
	} */

	void ShowNewPattern()
	{
		currentPatternIndex = Random.Range(0, patternTextures.Length);
		roundNumber++;
		Texture2D leftImage, rightImage;
		DepthMerger.GenerateRandomDotChannelFromShape(rawTextures[roundNumber % rawTextures.Length], patternTextures[currentPatternIndex], depthPixel * (mode == BaseMode.BaseIn?-1:1),
			mode == BaseMode.BaseIn, out leftImage, out rightImage);
		Texture2D mark = characterMarks[Random.Range(0, characterMarks.Length)];
		DepthMerger.InsertMarkIntoTexture2D(leftImage, 251, 251, mark, mode == BaseMode.BaseIn ? ColorChannel.CC_Cyan : ColorChannel.CC_Red, out leftImage);
		DepthMerger.InsertMarkIntoTexture2D(rightImage, 251, 251, mark, mode == BaseMode.BaseIn ? ColorChannel.CC_Red : ColorChannel.CC_Cyan, out rightImage);
		int disparityPixel = DPICaculator.ConvertScreenMMToPixel(outputImage.canvas, disparityMM, outputImage.transform.localScale.x);
		Texture2D outTexture = DepthMerger.GenerateDepthImageFromLeftRightImage(leftImage, rightImage, disparityPixel);
		outputImage.texture = outTexture;
		outputImage.GetComponent<RectTransform>().sizeDelta = new Vector2(outTexture.width, outTexture.height);
	}

	/* public override bool ResultExist()
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
	} */
}
