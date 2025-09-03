using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StereoDepthController : DiagnosticController
{
    enum DepthMode
    {
        ThreeDepth,
        FiveDepth
    };

    enum ObjectCount
    {
        FewObjects,
        ManyObjects
    };

    enum RoundCount
    {
        OneRound,
        FiveRound,
        TenRound,
        TwentyRound
    };

    enum DistanceMode
    {
        ShortDistance,
        LongDistance
    };


    const string KeyName_Depth = "StereoDepth_Depth";
    const string KeyName_ObjectCount = "StereoDepth_ObjCount";
    const string KeyName_RoundCount = "StereoDepth_RoundCount";
    const string KeyName_DistanceMode = "StereoDepth_DistanceMode";
    [SerializeField]
    GameObject resultPanel, scorePanel, nextRoundButton, seeResultButton, confirmButton, totalRight, totalWrong;
    [SerializeField]
    Toggle[] togglesDepth, togglesObjCount, togglesRoundCount, togglesDistanceMode;
    [SerializeField]
    private GameObject playPanel, settingPanel;
    [SerializeField] RawImage imageGivenPattern, imagePatternTmpl, imageGivenRoundFrame;
    [SerializeField]
    Texture2D[] patterns;
    [SerializeField]
    Texture2D patternRoundRect, patternCircle;
    [SerializeField]
    RectTransform transCorrectMark, transWrongMark;
    [SerializeField]
    private TextMeshProUGUI textCorrectStatus, textWrongStatus;
    int correctPatternCount;
    DepthMode depthMode;
    ObjectCount objectCount;
    RoundCount roundCountMode;
    DistanceMode distanceMode;
    int roundCount;
    int currentRoundIndex;
    float currentArcsec, arcsecStep;
    int givenDepthPosition;
    int[] correctIndices;
    int givenPixelOffset;
    float stepX = 120, stepY = 100;
    private int score, correct, wrong;
    float startTime;
    // Start is called before the first frame update
    void Start()
    {
        LoadSetting();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void SaveSetting()
    {
        PlayerPrefs.SetInt(KeyName_Depth, (int)GetDepthMode());
        PlayerPrefs.SetInt(KeyName_ObjectCount, (int)GetObjectCount());
        PlayerPrefs.SetInt(KeyName_RoundCount, (int)GetRoundCount());
        PlayerPrefs.SetInt(KeyName_DistanceMode, (int)GetDistanceMode());
    }

    void LoadSetting()
    {
        SetDepthMode((DepthMode)PlayerPrefs.GetInt(KeyName_Depth, 0));
        SetObjectCount((ObjectCount)PlayerPrefs.GetInt(KeyName_ObjectCount, 0));
        SetRoundCount((RoundCount)PlayerPrefs.GetInt(KeyName_RoundCount, 0));
        SetDistanceMode((DistanceMode)PlayerPrefs.GetInt(KeyName_DistanceMode, 0));
    }

    DepthMode GetDepthMode()
    {
        for (int i = 0; i < togglesDepth.Length; i++)
        {
            if (togglesDepth[i].isOn)
                return (DepthMode)i;
        }
        return DepthMode.ThreeDepth;
    }

    void SetDepthMode(DepthMode mode)
    {
        int i = (int)mode;
        if (i >= togglesDepth.Length)
        {
            UnityEngine.Debug.LogError("Depth value exceeds limit.");
            return;
        }
        togglesDepth[i].isOn = true;
    }

    ObjectCount GetObjectCount()
    {
        for (int i = 0; i < togglesObjCount.Length; i++)
        {
            if (togglesObjCount[i].isOn)
                return (ObjectCount)i;
        }
        return ObjectCount.FewObjects;
    }

    void SetObjectCount(ObjectCount count)
    {
        int i = (int)count;
        if (i >= togglesObjCount.Length)
        {
            UnityEngine.Debug.LogError("Depth value exceeds limit.");
            return;
        }
        togglesObjCount[i].isOn = true;
    }

    RoundCount GetRoundCount()
    {
        for (int i = 0; i < togglesRoundCount.Length; i++)
        {
            if (togglesRoundCount[i].isOn)
                return (RoundCount)i;
        }
        return RoundCount.OneRound;
    }

    void SetRoundCount(RoundCount count)
    {
        int i = (int)count;
        if (i >= togglesRoundCount.Length)
        {
            UnityEngine.Debug.LogError("Depth value exceeds limit.");
            return;
        }
        togglesRoundCount[i].isOn = true;
    }

    DistanceMode GetDistanceMode()
    {
        for (int i = 0; i < togglesDistanceMode.Length; i++)
        {
            if (togglesDistanceMode[i].isOn)
                return (DistanceMode)i;
        }
        return DistanceMode.ShortDistance;
    }

    void SetDistanceMode(DistanceMode mode)
    {
        int i = (int)mode;
        if (i >= togglesDistanceMode.Length)
        {
            UnityEngine.Debug.LogError("Depth value exceeds limit.");
            return;
        }
        togglesDistanceMode[i].isOn = true;
    }

    public void OnBtnStart()
    {
        depthMode = GetDepthMode();
        objectCount = GetObjectCount();
        roundCountMode = GetRoundCount();
        distanceMode = GetDistanceMode();
        SaveSetting();
        settingPanel.SetActive(false);
        if (roundCountMode == RoundCount.OneRound)
            roundCount = 1;
        else if (roundCountMode == RoundCount.FiveRound)
            roundCount = 5;
        else if (roundCountMode == RoundCount.TenRound)
            roundCount = 10;
        else
            roundCount = 20;
        StartGamePlay();
    }

    public void OnBtnReStart()
    {
        this.resultPanel.SetActive(false);
        this.StartGamePlay();
    }

    void StartGamePlay()
    {
        playPanel.SetActive(true);
        scorePanel.SetActive(true);
        nextRoundButton.SetActive(false);
        seeResultButton.SetActive(false);
        confirmButton.SetActive(true);
        resultPanel.SetActive(false);
        currentRoundIndex = score = correct = wrong = 0;
        textCorrectStatus.text = correct.ToString();
        textWrongStatus.text = wrong.ToString();
        startTime = Time.time;
		currentArcsec = 1000;
		/*if (roundCountMode == RoundCount.OneRound)
        {
            currentArcsec = PixelToArcSec(Random.Range(1.5f, 2.5f));
            arcsecStep = 0;
        }
        else if (roundCountMode == RoundCount.FiveRound)
        {
            currentArcsec = PixelToArcSec(Random.Range(1.5f, 2.5f)) + 4000;
            arcsecStep = 1000;
        }
        else if (roundCountMode == RoundCount.TenRound)
        {
            currentArcsec = PixelToArcSec(Random.Range(1.5f, 2.5f)) + 4500;
            arcsecStep = 500;
        }
        else
        {
            currentArcsec = PixelToArcSec(Random.Range(1.5f, 2.5f)) + 4750;
            arcsecStep = 250;
        }*/
		this.ShowNewPattern();
    }

    void ShowNewPattern()
    {
        transWrongMark.gameObject.SetActive(false);
        transCorrectMark.gameObject.SetActive(false);
		totalRight.SetActive(false);
		totalWrong.SetActive(false);
		correctPatternCount = Random.Range(1, 10);
        int slotCount = GetSlotCount();
        List<Texture2D> slotpatterns = new List<Texture2D>();
        int []indices = new int[patterns.Length];
        for (int i = 0; i < patterns.Length; i++)
            indices[i] = i;
        for (int i = 0; i < patterns.Length; i++)
        {
            int j = i + Random.Range(1, patterns.Length - 2);
            j %= patterns.Length;
            int tempIndex = indices[i];
            indices[i] = indices[j];
            indices[j] = tempIndex;
        }
        for (int i = 0; i < slotCount; i++)
            slotpatterns.Add(patterns[indices[i]]);
        correctIndices = new int[correctPatternCount];
        for (int i = 0;i < correctPatternCount; i++)
            correctIndices[i] = -1;
        for (int i = 0; i < correctPatternCount; i++)
        {
            int correctIndex = Random.Range(0, slotCount);
            while(correctIndices.Contains(correctIndex))
                correctIndex = Random.Range(0, slotCount);
            correctIndices[i] = correctIndex;
        }
        int previewIndex = correctIndices[Random.Range(0, correctPatternCount)];
        UtilityFunc.DeleteAllSideTransforms(imagePatternTmpl.transform, false);

        
        givenDepthPosition = depthMode == DepthMode.ThreeDepth?Random.Range(-1, 2): Random.Range(-2, 3);
        if (depthMode == DepthMode.ThreeDepth)
            givenPixelOffset = ArcSecondsToPixel (currentArcsec * givenDepthPosition);
        else
            givenPixelOffset = ArcSecondsToPixel(currentArcsec * givenDepthPosition / 2);
        if (distanceMode == DistanceMode.LongDistance)
            givenPixelOffset *= 2;
        int columnCount = 4;

        
        Texture2D mergedTexture = DepthMerger.GenerateDepthImageFromWhiteImage(patternRoundRect, givenPixelOffset);
        imageGivenRoundFrame.gameObject.SetActive(true);
        imageGivenRoundFrame.GetComponent<RectTransform>().sizeDelta = new Vector2(mergedTexture.width, mergedTexture.height);
        imageGivenRoundFrame.texture = mergedTexture;

        mergedTexture = DepthMerger.GenerateDepthImageFromWhiteImage(slotpatterns[previewIndex], givenPixelOffset);
        imageGivenPattern.GetComponent<RectTransform>().sizeDelta = new Vector2(mergedTexture.width, mergedTexture.height);
        imageGivenPattern.texture = mergedTexture;
        Vector2 tmplPosition = imagePatternTmpl.GetComponent<RectTransform>().anchoredPosition;
        for (int i = 0; i < slotCount; i++)
        {
            int pixelOffset;
            if (correctIndices.Contains(i))
            {
                pixelOffset = givenPixelOffset;
            }
            else
            {
                int depthPosition = 0;
                float arcSec;

                if (depthMode == DepthMode.ThreeDepth)
                {
                    depthPosition = Random.Range(-1, 2);
                    while(depthPosition == givenDepthPosition)
                        depthPosition = Random.Range(-1, 2);
                    arcSec = currentArcsec * depthPosition;
                }
                else
                {
                    depthPosition = Random.Range(-2, 3);
                    while (depthPosition == givenDepthPosition)
                        depthPosition = Random.Range(-2, 3);
                    arcSec = currentArcsec * depthPosition / 2;
                }
                pixelOffset = ArcSecondsToPixel(arcSec);
                if (distanceMode == DistanceMode.LongDistance)
                    pixelOffset *= 2;

            }
            mergedTexture = DepthMerger.GenerateDepthImageFromWhiteImage(slotpatterns[i], pixelOffset);
            GameObject newObj = Instantiate(imagePatternTmpl.gameObject, imagePatternTmpl.transform.parent);
			newObj.name = i.ToString();

			newObj.SetActive(true);
            newObj.transform.localScale = imagePatternTmpl.transform.localScale;
            if (slotCount == 20)
            {
                columnCount = 5;
            }
            int row = i / columnCount;
            int col = i % columnCount;
            newObj.GetComponent<RectTransform>().anchoredPosition = tmplPosition + new Vector2(stepX * col, stepY * row);
            newObj.GetComponent<RectTransform>().sizeDelta = new Vector2(mergedTexture.width, mergedTexture.height);
            newObj.GetComponent<RawImage>().texture = mergedTexture;

            
        }
    }

    public void OnClickCountButton(GameObject button)
    {
        int guessCount = int.Parse(button.name);
        int countDeviation = Mathf.Abs(guessCount - correctPatternCount);

        int slotCount = GetSlotCount();

		Vector2 tmplPosition = imagePatternTmpl.GetComponent<RectTransform>().anchoredPosition;
        for (int i = 0; i < slotCount; i++)
        {
            if (correctIndices.Contains(i))
            {
                Texture2D mergedTexture = DepthMerger.GenerateDepthImageFromWhiteImage(patternCircle, givenPixelOffset);
                GameObject newObj = Instantiate(imagePatternTmpl.gameObject, imagePatternTmpl.transform.parent);
                newObj.SetActive(true);
                newObj.transform.localScale = imagePatternTmpl.transform.localScale;
                int columnCount = 4;
                if (slotCount == 20)
                {
                    columnCount = 5;
                }
                int row = i / columnCount;
                int col = i % columnCount;
                newObj.GetComponent<RectTransform>().anchoredPosition = tmplPosition + new Vector2(stepX * col, stepY * row);
                newObj.GetComponent<RectTransform>().sizeDelta = new Vector2(mergedTexture.width, mergedTexture.height);
                newObj.GetComponent<RawImage>().texture = mergedTexture;

            }
        }
        if (currentRoundIndex == roundCount - 1)
            seeResultButton.SetActive(true);
        else
        {
            nextRoundButton.SetActive(true);
            if (countDeviation < 2)
                currentArcsec -= arcsecStep;
        }

        transCorrectMark.SetParent(transCorrectMark.parent.parent.GetChild(correctPatternCount - 1), false);
        transCorrectMark.gameObject.SetActive(true);
        if (countDeviation == 0)
        {
            correct++;
            textCorrectStatus.text = correct.ToString();
        }
        else
        {
            wrong++;
			textWrongStatus.text = wrong.ToString();
            transWrongMark.SetParent(transWrongMark.parent.parent.GetChild(guessCount - 1), false);
            transWrongMark.gameObject.SetActive(true);
        }
		score += (int)(100f / roundCount * (slotCount - countDeviation) / slotCount);
        currentRoundIndex++;
    }

    int ArcSecondsToPixel(float arcsec)
    {
        return (int)DPICaculator.ConvertArcSecondToPixelOffset(imagePatternTmpl.canvas, arcsec, imagePatternTmpl.GetComponent<RectTransform>().localScale.x);
    }

    float PixelToArcSec(float pixeloffset)
    {
        return DPICaculator.ConvertCanvasSizeToArcSeconds(imagePatternTmpl.canvas, pixeloffset, imagePatternTmpl.GetComponent<RectTransform>().localScale.x);
    }

    public void OnBtnNextRound()
    {
        
        nextRoundButton.SetActive(false);
        confirmButton.SetActive(true);
        ShowNewPattern();
    }

    public void OnBtnSeeResult()
    {
        resultPanel.SetActive(true);
        this.playPanel.SetActive(false);
        this.resultPanel.SetActive(true);
        this.resultPanel.transform.Find("Score").GetComponent<Text>().text = this.score.ToString();
        this.resultPanel.transform.Find("Correct").GetComponent<Text>().text = this.correct.ToString();
        this.resultPanel.transform.Find("Incorrect").GetComponent<Text>().text = this.wrong.ToString();
        /*int resultSecArc = ((int)currentArcsec);
        if (distanceMode == DistanceMode.LongDistance)
            resultSecArc /= 2;
        this.resultPanel.transform.Find("SecArc").GetComponent<Text>().text = resultSecArc.ToString();*/
        this.resultPanel.transform.Find("TotalTime").GetComponent<Text>().text = UtilityFunc.ConvertSec2MMSS(Time.time - startTime);

    }

    public override void AddResults()
    {
        PatientRecord patientRecord = PatientDataMgr.GetPatientRecord();
        DiagnoseTestItem diagnoseTestItem = new DiagnoseTestItem();
        diagnoseTestItem.AddValue(roundCount.ToString());
        diagnoseTestItem.AddValue(score.ToString());
        diagnoseTestItem.AddValue(correct.ToString()); 
        diagnoseTestItem.AddValue(wrong.ToString());
        //diagnoseTestItem.AddValue(resultPanel.transform.Find("SecArc").GetComponent<Text>().text);
        patientRecord.AddDiagnosRecord("StereoDepth", diagnoseTestItem);
    }

    public override bool ResultExist()
    {
        return resultPanel.activeSelf && currentRoundIndex == roundCount;
    }

    public void OnClickedPattern(GameObject button)
    {
        if (!confirmButton.activeSelf)
            return;
		GameObject checkmark = button.transform.GetChild(0).gameObject;
        checkmark.SetActive(!checkmark.activeSelf);
    }

    int GetSlotCount()
    {
        return objectCount == ObjectCount.FewObjects ? 16 : 20;
	}

    public void OnBtnConfirm()
    {
		int countDeviation = 0;
		confirmButton.SetActive(false);
		int slotCount = GetSlotCount();
		Vector2 tmplPosition = imagePatternTmpl.GetComponent<RectTransform>().anchoredPosition;
		for (int i = 0; i < slotCount; i++)
		{
			/*if (correctIndices.Contains(i))
			{
				Texture2D mergedTexture = DepthMerger.GenerateDepthImageFromWhiteImage(patternCircle, givenPixelOffset);
				GameObject newObj = Instantiate(imagePatternTmpl.gameObject, imagePatternTmpl.transform.parent);
				newObj.SetActive(true);
				newObj.transform.localScale = imagePatternTmpl.transform.localScale;
				int columnCount = 4;
				if (slotCount == 20)
				{
					columnCount = 5;
				}
				int row = i / columnCount;
				int col = i % columnCount;
				newObj.GetComponent<RectTransform>().anchoredPosition = tmplPosition + new Vector2(stepX * col, stepY * row);
				newObj.GetComponent<RectTransform>().sizeDelta = new Vector2(mergedTexture.width, mergedTexture.height);
				newObj.GetComponent<RawImage>().texture = mergedTexture;

			}*/
            Transform patternTrans = imagePatternTmpl.transform.parent.Find($"{i}");
            if (correctIndices.Contains(i))
            {
                if (patternTrans.GetChild(0).gameObject.activeSelf)
                    patternTrans.GetChild(0).GetComponent<Image>().color = Color.green;
                else
                {
					patternTrans.GetChild(0).gameObject.SetActive(true);
					patternTrans.GetChild(0).GetComponent<Image>().color = Color.red;
                    countDeviation++;
				}
			}
            else
            {
				if (patternTrans.GetChild(0).gameObject.activeSelf)
                {
                    patternTrans.GetChild(0).gameObject.SetActive(false);
					patternTrans.GetChild(1).gameObject.SetActive(true);
                    countDeviation++;
				}
			}
		}
		if (currentRoundIndex == roundCount - 1)
			seeResultButton.SetActive(true);
		else
		{
			nextRoundButton.SetActive(true);
			/*if (countDeviation < 2)
				currentArcsec -= arcsecStep;*/
		}

		/*transCorrectMark.SetParent(transCorrectMark.parent.parent.GetChild(correctPatternCount - 1), false);
		transCorrectMark.gameObject.SetActive(true);*/
		if (countDeviation == 0)
		{
			correct++;
			textCorrectStatus.text = correct.ToString();
            totalRight.SetActive(true);
		}
		else
		{
            totalWrong.SetActive(true);
			wrong++;
			textWrongStatus.text = wrong.ToString();
			/*transWrongMark.SetParent(transWrongMark.parent.parent.GetChild(guessCount - 1), false);
			transWrongMark.gameObject.SetActive(true);*/
		}
		score += (int)(100f / roundCount * (slotCount - countDeviation) / slotCount);
		currentRoundIndex++;
	}
}
