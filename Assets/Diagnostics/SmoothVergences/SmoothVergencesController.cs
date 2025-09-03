using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[Serializable]
class VergencePatternInfo
{
    public Sprite _blueSprite, _redSprite;
    public string _name;
}
public class SmoothVergencesController : MonoBehaviour
{
    //Deviation (cm) = Prism diopters (Δ) * Distance to screen (m) where deviation is the distance between images, prism dioptres is the BI BO value and distance is the distance from the screen
	float _bio;
    float _BI, _BO;//BI < 0, BO > 0
    float _practiceTime = 2;
    float speed = 2;
    float _screendis = 0.3f;
    float _cm2pix = 45;
    float _startTime;
    [SerializeField] Image _blueImage, _redImage, _blueImagePreview, _redImagePreview;
    [SerializeField] Text _textBIO, _textSpeed;
    [SerializeField] GameObject _objOption, _btnPrevPattern, _btnNextPattern;
    [SerializeField] TMP_InputField _inputBO, _inputBI, _inputTime;
    [SerializeField] GameObject btnStart, btnStop;
    [SerializeField] TextMeshProUGUI _textPatternName;
    [SerializeField] List<VergencePatternInfo> _patternSet = new List<VergencePatternInfo>();
    int _patternIndex;
    // Start is called before the first frame update
    void Start()
    {
        _BI = -10;
        _BO = 10;
        _bio = 0;
        float dpi = 100;
        string dpipath = DPICaculator.GetDPIPath();
        if (File.Exists(dpipath))
        {
            string[] lines = File.ReadAllLines(dpipath);
            if(lines.Length >= 2 ) {
                dpi = float.Parse(lines[1]);
            }
        }
        _cm2pix = dpi / 2.54f;
        Time.timeScale = 0;
        ShowSpeed();
        SetPattern(_patternIndex);
		_objOption.SetActive(true);
	}

    public void StartGame()
    {
        _startTime = Time.time;
		_bio = 0;
        speed = Mathf.Abs(speed);
		ShowSpeed();
	}

    // Update is called once per frame
    void Update()
    {
        if(Time.time - _startTime > _practiceTime * 60) {
            Time.timeScale = 0;
            btnStart.SetActive(true);
            btnStop.SetActive(false);
            _startTime = Time.time;
        }
        _bio += speed * Time.deltaTime;
        if(_bio > _BO)
        {
            speed = -speed;
            _bio = _BO;
        }
        else if(_bio < _BI) {
            _bio = _BI;
            speed = -speed;
        }
        float deviation = _bio * _screendis;
        float devinPix = deviation * _cm2pix;
		_blueImage.transform.localPosition = new Vector3(devinPix / 2, _blueImage.transform.localPosition.y, 0);
		_redImage.transform.localPosition = new Vector3(-devinPix / 2, _redImage.transform.localPosition.y, 0);
        _textBIO.text = string.Format("{0,4:F1}", Mathf.Abs(_bio)) + " " + (_bio < 0? "BI": "BO");
		//Debug.Log(_bio.ToString() + " " + (_bio > 0? "BO": "BI"));
	}

    public void OnBtnSpeed(float delta)
    {
        speed  = Mathf.Sign(speed) * Mathf.Clamp(Mathf.Abs(speed) + delta, 1, 3);
        ShowSpeed();
	}

    public void OnBtnStartOrStop(float timeScale)
    {
		Time.timeScale = timeScale;
	}



    public void OnBtnBack() {
		Time.timeScale = 1;
	}

    void ShowSpeed()
    {
		_textSpeed.text = string.Format("{0,1:F1}", Mathf.Abs(speed)) + " SPEED";
	}

    public void OnBtnOptionOK()
    {
        _BO = float.Parse(_inputBO.text);
        _BI = -float.Parse(_inputBI.text);
        _practiceTime = float.Parse(_inputTime.text);
        speed = Mathf.Abs(speed);
        _bio = 0;
	}

    public void OnBtnOption()
    {
        if (Time.timeScale == 1)
            return;
        _objOption.SetActive(true);

	}

    void SetPattern(int index)
    {
        if (index >= _patternSet.Count)
            return;
        _patternIndex = index;
		_blueImagePreview.sprite = _blueImage.sprite = _patternSet[index]._blueSprite;
		_redImagePreview.sprite = _redImage.sprite = _patternSet[index]._redSprite;
        _textPatternName.text =  _patternSet[index]._name;
		_btnNextPattern.SetActive(_patternIndex < _patternSet.Count - 1);
		_btnPrevPattern.SetActive(_patternIndex > 0);
	}

    public void OnChangePatternButton(int delta)
    {
        SetPattern(_patternIndex + delta);
    }

    public void OnSliderSpeed(float value)
    {
        speed = Mathf.Sign(speed) * value;
        ShowSpeed();

	}
}
