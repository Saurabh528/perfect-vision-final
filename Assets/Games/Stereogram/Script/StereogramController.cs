using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class StereogramController : DiagnosticController
{
    public enum PatternDirection{
        Left,
        Right,
        Up,
        Down
    };
    public enum SymbolType{
        Triangle,
        Moon,
        Lightning,
        Heart,
        Star,
        Circle,
        Rectangle,
        X
    };

    //[SerializeField] Text textLevel, textScore;
    [SerializeField] Image imageCyan, imageRed, markCyan, markRed, imageTimePercent;
    [SerializeField] GameObject playPanel, resultPanel, symbolSection;
    [SerializeField] StereogramSettingUI settingUI;
    [SerializeField] TextMeshProUGUI textCorrectStatus, textWrongStatus;
    [SerializeField] TweenAppear tweenArrowKeys, tweenTapSymbol;
    [SerializeField] Sprite[] symbolIcons;

    PatternDirection curPatDir = PatternDirection.Left;
    SymbolType curSymbol = SymbolType.Triangle;
    DepthMode                 depthMode = DepthMode.Depth10;
    int                 customEyesIn = 10;

    float               jumpTime = 6;
    StereoOverlapMode   overlapMode = StereoOverlapMode.EyesIn;
    StereoTestMode      testMode = StereoTestMode.VisualSymbol;
    SizeMode            sizeMode = SizeMode.Normal;
    LevelMode           levelMode = LevelMode.Level1;
    float               patternDelay;
    int                 wrongCount = 0;
    int                 distance, longDist, correct;
    int score;
    float remainTime, totalTime, patternStartTime;
    List<float> guessTimeList;
    // Start is called before the first frame update
    public void Start()
    {
	}

    public void Update(){
        if(remainTime > 0){
            if(testMode != StereoTestMode.VisualSymbol){
                if(Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A))
                    CheckPatternByArrowKey(PatternDirection.Left);
                else if(Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D))
                    CheckPatternByArrowKey(PatternDirection.Right);
                else if(Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
                    CheckPatternByArrowKey(PatternDirection.Up);
                else if(Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S))
                    CheckPatternByArrowKey(PatternDirection.Down);
            }
            
            patternDelay -= Time.deltaTime;
            if(patternDelay <= 0 && testMode != StereoTestMode.VisualSymbol){
                if (depthMode == DepthMode.DepthIncrease){
                    distance--;
                    if(distance == -1)
                        distance = 0;
                }
                    
                ShowNewPattern();
            }
            imageTimePercent.fillAmount = remainTime / totalTime;
            remainTime -= Time.deltaTime;
            if(remainTime < 0){
                remainTime = 0;
                ShowResult();
            }
        }
        
    }

    public void OnSymbolButton(Button button){
        if(remainTime <= 0)
            return;
        CheckPatternBySymbol(Enum.Parse<SymbolType>(button.name));
    }

    void CheckPatternByArrowKey(PatternDirection dir){
        if(dir == curPatDir)
            OnCheckOK();
        else
            OnCheckFail();
        ShowNewPattern();
    }

    void CheckPatternBySymbol(SymbolType type){
        if(type == curSymbol)
            OnCheckOK();
        else
            OnCheckFail();
        ShowNewPattern();
    }

    void OnCheckOK(){
        if (depthMode == DepthMode.DepthIncrease)
            distance++;
        longDist = distance;
        guessTimeList.Add(Time.time - patternStartTime);
        correct++;
        if(correct == 1){
            if(testMode == StereoTestMode.VisualSymbol)
                tweenTapSymbol.Disappear();
            else
                tweenArrowKeys.Disappear();
        }
        textCorrectStatus.text = correct.ToString();
        score++;
    }

    void OnCheckFail(){
        if (depthMode == DepthMode.DepthIncrease){
            distance--;
            if(distance == -1)
                distance = 0;
        }
        
        score--;
        if(score < 0)
            score = 0;
        wrongCount++;
        textWrongStatus.text = wrongCount.ToString();
        /* if(wrongCount == 3 && GameState.currentGamePlay == null)
            GameOver(); */
    }




    /* public override void ShowScore() {
        textScore.text = $"Score: {_score}";
    }

    public override void ShowLevel() {
        textLevel.text = $"Level {_level}";
    } */

    public void OnBtnStart(){
        settingUI.SaveSetting();
        depthMode = settingUI.GetDepthMode();
        if(depthMode == DepthMode.DepthCustom)
            customEyesIn = settingUI.GetCustomEyesIn();
        if(depthMode == DepthMode.Depth10)
            distance = 3;
        else if(depthMode == DepthMode.Depth20)
            distance = 6;
        else if(depthMode == DepthMode.Depth30)
            distance = 9;
        else if(depthMode == DepthMode.DepthCustom)
            distance = customEyesIn;
        else //DepthMode.DepthIncrease
            distance = 0;
        sizeMode = settingUI.GetSizeMode();
        levelMode = settingUI.GetLevelMode();
        //jumpTime = settingUI.GetJumpTime();
        overlapMode = settingUI.GetOverlapMode();
        //testMode = settingUI.GetTestMode();
        symbolSection.SetActive(testMode == StereoTestMode.VisualSymbol);
        if(testMode == StereoTestMode.VisualSymbol && levelMode == LevelMode.Level1){
            SymbolType[] types = new SymbolType[5];
            for(int i = 0; i < 5; i++){
                types[i] = (SymbolType)i;
            }
            SetSymbolButtons(types);
        }
        totalTime = remainTime = settingUI.GetPlayTime();
        Debug.Log($"Depth:{depthMode}, JumpTime: {jumpTime}, Overlap: {overlapMode}, Test: {testMode}, PlayTime: {remainTime}");
        settingUI.gameObject.SetActive(false);
        StartGamePlay();
    }

    public void OnBtnReStart(){
        if(depthMode == DepthMode.DepthIncrease)
            distance = 0;
        remainTime = totalTime;
        resultPanel.SetActive(false);
        StartGamePlay();
    }

    void StartGamePlay()
	{
        correct = wrongCount = score = 0;
        textCorrectStatus.text = correct.ToString();
        textWrongStatus.text = wrongCount.ToString();
        guessTimeList = new List<float>();
        playPanel.SetActive(true);
		ShowNewPattern();
        if(testMode == StereoTestMode.VisualSymbol)
            tweenTapSymbol.Appear();
        else
            tweenArrowKeys.Appear();
	}

    void ShowNewPattern(){
        patternDelay = jumpTime;
        float scale = sizeMode == SizeMode.Normal? 1: 0.5f;
        imageCyan.transform.localScale = imageRed.transform.localScale = new Vector3(scale, scale, 1);
        string redpath = "", cyanpath = "";
        if(testMode == StereoTestMode.VisualSymbol){
            SelectVisualSymbolPatterns(out redpath, out cyanpath);
        }
        else{
            PatternDirection randomDirection = curPatDir;
            // Create an instance of the Random class
            System.Random random = new System.Random();
            while(randomDirection == curPatDir){
                // Get the number of values in the PatternDirection enum
                int enumLength = Enum.GetNames(typeof(PatternDirection)).Length;
                
                // Generate a random number within the range of enum values
                int randomIndex = random.Next(enumLength);
                
                // Cast the random number to the PatternDirection enum type
                randomDirection = (PatternDirection)randomIndex;
            }
            curPatDir = randomDirection;
            GetArrowPatternPath(out redpath, out cyanpath);
        }
        
        float move = 0;
        if(overlapMode == StereoOverlapMode.EyesIn)
            move = distance * 2;
        else if(overlapMode == StereoOverlapMode.EyesOut)
            move = -distance * 2;
        else
            move = distance * 2 * (imageCyan.transform.localPosition.x > 0? -1: 1);
        imageCyan.transform.localPosition = markCyan.transform.localPosition = new Vector3(move, 0, 0);
        imageRed.transform.localPosition = markRed.transform.localPosition = new Vector3(-move, 0, 0);
        
        // Load the sprite from the Resources folder
        Sprite sprite = Resources.Load<Sprite>(redpath);
        if (sprite == null)
        {
            Debug.LogError("Sprite not found in Resources folder: redpath");
            return;
        }
        // Set the sprite to the Image component
        imageRed.sprite = sprite;

        sprite = Resources.Load<Sprite>(cyanpath);
        if (sprite == null)
        {
            Debug.LogError("Sprite not found in Resources folder: cyanpath");
            return;
        }
        imageCyan.sprite = sprite;
        patternStartTime = Time.time;
        
    }

    void SelectVisualSymbolPatterns(out string redpath, out string cyanpath){
        SymbolType symboltype = curSymbol;
        // Create an instance of the Random class
        System.Random random = new System.Random();
        while(symboltype == curSymbol){
            int enumLength = levelMode == LevelMode.Level1? 5: Enum.GetNames(typeof(SymbolType)).Length;
            
            // Generate a random number within the range of enum values
            int randomIndex = random.Next(enumLength);
            
            // Cast the random number to the PatternDirection enum type
            symboltype = (SymbolType)randomIndex;
        }
        curSymbol = symboltype;
        if(levelMode == LevelMode.Level2){
            SymbolType[] types = new SymbolType[5];
            int pos = random.Next(5);
            for(int i = 0; i < 5; i++)
                types[i] = curSymbol;
            for(int i = 0; i < 5; i++){
                if(i != pos){
                    SymbolType othertype = curSymbol;
                    while(othertype == curSymbol || types.Contains(othertype)){
                        int enumLength = Enum.GetNames(typeof(SymbolType)).Length;
                        int randomIndex = random.Next(enumLength);
                        othertype = (SymbolType)randomIndex;
                    }
                    types[i] = othertype;
                }
            }
            SetSymbolButtons(types);
        }
        StereoOverlapMode mode = overlapMode;
        if(overlapMode == StereoOverlapMode.EyesMixed)
            mode = imageCyan.transform.localPosition.x > 0? StereoOverlapMode.EyesOut: StereoOverlapMode.EyesIn;
        GetSymbolPatternPath(out redpath, out cyanpath, mode);
    }

    void GetArrowPatternPath(out string redpath, out string cyanpath){
        if(testMode == StereoTestMode.VisualJump){
            redpath = $"StereogramPattern/Hexagon_{curPatDir}_Depth{depthMode}_{ColorChannel.CC_Red}";
            cyanpath = $"StereogramPattern/Hexagon_{curPatDir}_Depth{depthMode}_{ColorChannel.CC_Cyan}";
        }
        else{//VisualPower
            redpath = $"StereogramPattern/Diamond_{curPatDir}_{ColorChannel.CC_Red}";
            cyanpath = $"StereogramPattern/Diamond_{curPatDir}_{ColorChannel.CC_Cyan}";
        }
    }

    void GetSymbolPatternPath(out string redpath, out string cyanpath, StereoOverlapMode mode){
        if(testMode != StereoTestMode.VisualSymbol){
            Debug.LogError("Can't generate path in {testMode} mode");
            redpath = "";
            cyanpath = "";
            return;
        }
        if(mode == StereoOverlapMode.EyesIn){
            //cyanpath = $"StereogramPattern/Symbols_{mode}_{ColorChannel.CC_Cyan}";
            cyanpath = $"StereogramPattern/Symbols_{mode}_CC_Blue";
            redpath = $"StereogramPattern/Symbols_{curSymbol}_{mode}_{ColorChannel.CC_Red}";
        }
        else if(mode == StereoOverlapMode.EyesOut){
            redpath = $"StereogramPattern/Symbols_{mode}_{ColorChannel.CC_Red}";
            //cyanpath = $"StereogramPattern/Symbols_{curSymbol}_{mode}_{ColorChannel.CC_Cyan}";
            cyanpath = $"StereogramPattern/Symbols_{curSymbol}_{mode}_CC_Blue";
        }
        else{
            Debug.LogError("Can't generate path in {mode} mode");
            redpath = "";
            cyanpath = "";
            return;
        }
        
    }

    /* public override void OnScoreChange(int levelstartscore, int score)
	{
		base.OnScoreChange(levelstartscore, score);
		if (score >= levelstartscore + 3)
		{
			IncreaseLevel();
		}
	} */



    void ShowResult(){
        playPanel.SetActive(false);
        resultPanel.SetActive(true);
       /*  if(testMode == StereoTestMode.VisualJump)
            resultPanel.transform.Find("Title").GetComponent<Text>().text = "Visual Jump";
        else if(testMode == StereoTestMode.VisualPower)
            resultPanel.transform.Find("Title").GetComponent<Text>().text = "Visual Power";
        else
            resultPanel.transform.Find("Title").GetComponent<Text>().text = "Visual Symbols"; */
        /* resultPanel.transform.Find("Score").GetComponent<Text>().text = score.ToString();
        resultPanel.transform.Find("LongDist").GetComponent<Text>().text = longDist.ToString();
        resultPanel.transform.Find("Correct").GetComponent<Text>().text = correct.ToString();
        resultPanel.transform.Find("Errors").GetComponent<Text>().text = wrongCount.ToString();
        resultPanel.transform.Find("Time").GetComponent<Text>().text = UtilityFunc.ConvertSec2MMSS(totalTime); */
        resultPanel.transform.Find("Score").GetComponent<Text>().text = score.ToString();
        //resultPanel.transform.Find("LongDist").GetComponent<Text>().text = longDist.ToString();
        resultPanel.transform.Find("Correct").GetComponent<Text>().text = correct.ToString();
        resultPanel.transform.Find("Errors").GetComponent<Text>().text = wrongCount.ToString();
        float maxTime = 0, minTime = 0, averTime = 0;
        if(guessTimeList.Count > 0){
            maxTime = guessTimeList.Max();
            minTime = guessTimeList.Min();
            averTime = guessTimeList.Average();
        }
        resultPanel.transform.Find("Fastest").GetComponent<Text>().text = minTime.ToString("F2") + " sec";
        resultPanel.transform.Find("Slowest").GetComponent<Text>().text = maxTime.ToString("F2") + " sec";
        resultPanel.transform.Find("Average").GetComponent<Text>().text = averTime.ToString("F2") + " sec";
        resultPanel.transform.Find("TotalTime").GetComponent<Text>().text = UtilityFunc.ConvertSec2MMSS(totalTime);
    }

    public override void AddResults(){
        PatientRecord pr = PatientDataMgr.GetPatientRecord();
        DiagnoseTestItem dti = new DiagnoseTestItem();
        dti.AddValue(resultPanel.transform.Find("LongDist").GetComponent<Text>().text);
        dti.AddValue(resultPanel.transform.Find("Correct").GetComponent<Text>().text);
        dti.AddValue(resultPanel.transform.Find("Errors").GetComponent<Text>().text);
        pr.AddDiagnosRecord("Stereography", dti);
    }

    public override bool ResultExist(){
        if(!base.ResultExist())
            return false;
        return remainTime == 0;
    }

    public void SetSymbolButtons(SymbolType[] types){
        if(types.Length != 5){
            Debug.LogError("Can have only 5 symbols.");
            return;
        }
        for(int i = 0; i < 5; i++){
            symbolSection.transform.GetChild(i).GetComponent<Image>().sprite = symbolIcons[(int)types[i]];
            symbolSection.transform.GetChild(i).gameObject.name = types[i].ToString();
        }
    }
}
