using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;


public class VergenceGameController : GamePlayController
{
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
    int _patternIndex;
    // Start is called before the first frame update
    public override void Start()
    {
        base.Start();
        _BI = -10;
        _BO = 10;
        _bio = 0;
        float dpi = 72;
        /* string dpipath = Application.dataPath + "/../Python/DPI.txt";
        if (File.Exists(dpipath))
        {
            string[] lines = File.ReadAllLines(dpipath);
            if(lines.Length >= 2 ) {
                dpi = float.Parse(lines[1]);
            }
        } */
        _cm2pix = dpi / 2.54f;
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
