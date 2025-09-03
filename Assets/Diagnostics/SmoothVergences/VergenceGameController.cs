using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class VergenceGameController : GamePlayController
{
    const string KeyName_MaxIn = "Vergence_MaxIn";
    const string KeyName_MaxOut = "Vergence_MaxOut";
    //Deviation (cm) = Prism diopters (Δ) * Distance to screen (m) where deviation is the distance between images, prism dioptres is the BI BO value and distance is the distance from the screen
	float _bio;
    float _BI, _BO;//BI < 0, BO > 0
    float speed = 2;
    float _screendis = 0.3f;
    float _cm2pix = 45;
    [SerializeField] Image _blueImage, _redImage;
    [SerializeField] GameObject btnStop;
    [SerializeField] List<VergencePatternInfo> _patternSet = new List<VergencePatternInfo>();
    [SerializeField] Text textLevel, textScore;
    [SerializeField] TMP_InputField textMaxIn, textMaxOut;
    int _patternIndex;
    // Start is called before the first frame update
    public override void Start()
    {
        
        textMaxIn.text = (-PlayerPrefs.GetFloat(KeyName_MaxIn, -10)).ToString();
        textMaxOut.text = PlayerPrefs.GetFloat(KeyName_MaxOut, 10).ToString();
        _bio = 0;
        float dpi = 72;
        _cm2pix = dpi / 2.54f;
	}

    public void OnClickOptionOK(){
        _BI = -float.Parse(textMaxIn.text);
        _BO = float.Parse(textMaxOut.text);
        PlayerPrefs.SetFloat(KeyName_MaxIn, _BI);
        PlayerPrefs.SetFloat(KeyName_MaxOut, _BO);
        base.Start();
    }

    public void StartGame()
    {
		_bio = 0;
        speed = Mathf.Abs(speed);
	}

    // Update is called once per frame
    public override void Update()
    {
        base.Update();
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
	}



    public void OnBtnStart(){
        Time.timeScale = 1;
        _level++;
        ShowLevel();
        _patternIndex++;
        if(_patternIndex == _patternSet.Count)
            _patternIndex = 0;
        _blueImage.sprite = _patternSet[_patternIndex]._blueSprite;
        _redImage.sprite = _patternSet[_patternIndex]._redSprite;
    }

    public void OnBtnStop(){
        Debug.Log($"_bio:{_bio}");
        _score = (int)(_bio * 10);
        ShowScore();
        Time.timeScale = 0;
    }

    public override void ShowScore() {
        float abs = Mathf.Abs((float)_score) / 10f;
        string absstr = abs.ToString("F1");
        string direction = _score > 0? "BO": "BI";
        textScore.text = $"Score {absstr} {direction}";
    }

    public override void ShowLevel() {
        textLevel.text = $"Level {_level}";
    }


}
