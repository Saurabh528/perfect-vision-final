using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FixationDisparityController : DiagnosticController
{
    bool _topMoving, _rightMoving;
    [SerializeField] Transform _transTop, _transRight;
    [SerializeField] TweenAppear _tweenArrowKeys;
    [SerializeField] float _moveSpeed = 30;
    [SerializeField] Text _textHorDisparity, _textHorPrism, _textHorTag, _textVerDisparity, _textVerPrism, _textVerTag;
    bool _topBlinking, _rightBlinking;
    Color _originColor;
    Color _blinkColor = new Color(0.2f, 0.2f, 0.2f, 1);
    bool _isBlinked;
    float _blinkTime;
    const float _blinkTime_Color = 1;
    const float _blinkTime_Black = 0.2f;
  
    void OnEnable()
	{
		ExitUI.EnableShutdownButton(false);
	}

	void OnDestroy()
	{
		ExitUI.EnableShutdownButton(true);
	}

    // Start is called before the first frame update
    void Start()
    {
        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
        if(_topMoving){
            if(_topBlinking){
                _blinkTime -= Time.deltaTime;
                if(_blinkTime < 0){
                    if(_isBlinked){
                        _blinkTime += _blinkTime_Color;
                        _transTop.Find("Sprite").GetComponent<Image>().color = _originColor;
                        _isBlinked = false;
                    }
                    else{
                        _blinkTime += _blinkTime_Black;
                        _transTop.Find("Sprite").GetComponent<Image>().color = _blinkColor;
                        _isBlinked = true;
                    }
                }
            }

            float horValue = Input.GetAxis("Horizontal") * _moveSpeed * Time.deltaTime;
            if(horValue != 0){
                _transTop.transform.Translate(new Vector3(horValue, 0, 0));
            }
            if(horValue != 0){
                ShowValuesAndCheck();
            }
        }

        if(_rightMoving){
            if(_rightBlinking){
                _blinkTime -= Time.deltaTime;
                if(_blinkTime < 0){
                    if(_isBlinked){
                        _blinkTime += _blinkTime_Color;
                        _transRight.Find("Sprite").GetComponent<Image>().color = _originColor;
                        _isBlinked = false;
                    }
                    else{
                        _blinkTime += _blinkTime_Black;
                        _transRight.Find("Sprite").GetComponent<Image>().color = _blinkColor;
                        _isBlinked = true;
                    }
                }
            }

            float verValue = Input.GetAxis("Vertical") * _moveSpeed * Time.deltaTime;
            if(verValue != 0){
                _transRight.transform.Translate(new Vector3(0, verValue, 0));
            }
            if(verValue != 0){
                ShowValuesAndCheck();
            }
        }
    }


    public void OnBtnReset(){
        StartGame();
    }

    public void OnTopBarClicked(){
        StartCoroutine(Routine_TopClick());
    }

    public void OnRightBarClicked(){
        StartCoroutine(Routine_RightClick());
    }

    public void ShowValuesAndCheck(){
        _textHorDisparity.text = Mathf.Abs((_transTop.localPosition.x - 0.1364f) / 28.6578f).ToString("F2");
        _textHorPrism.text = Mathf.Abs((_transTop.localPosition.x + 0.006f) / 18.58f).ToString("F2");
        _textHorTag.text = _transTop.localPosition.x > 0 ? "eso": "exo";

        _textVerDisparity.text = Mathf.Abs((_transRight.localPosition.y - 0.1364f) / 28.6578f).ToString("F2");
        _textVerPrism.text = Mathf.Abs((_transRight.localPosition.y + 0.006f) / 18.58f).ToString("F2");
        _textVerTag.text = _transRight.localPosition.y > 0 ? "hypo": "hyper";
        if(_topBlinking && Math.Abs(_transTop.localPosition.x) < 2){
            _topBlinking = false;
            _transTop.Find("Sprite").GetComponent<Image>().color = _originColor;
        }
        else if(Math.Abs(_transRight.localPosition.y) < 2){
            _rightBlinking = false;
            _transRight.Find("Sprite").GetComponent<Image>().color = _originColor;
        }
    }

    void StartGame(){
        _transTop.localPosition = new Vector3(UnityEngine.Random.Range(20, 50), _transTop.localPosition.y, 0);
        _transRight.localPosition = new Vector3(_transRight.localPosition.x, UnityEngine.Random.Range(-50, -20), 0);
        _topMoving = _rightMoving = false;
        _tweenArrowKeys.Disappear();
        _originColor = _transTop.Find("Sprite").GetComponent<Image>().color;
        _topBlinking = _rightBlinking = false;
        _isBlinked = false;
        _blinkTime = 0;
        ShowValuesAndCheck();
    }

    IEnumerator Routine_TopClick(){
        Transform spriteTrans = _transTop.Find("Sprite");
        spriteTrans.localScale = new Vector3(8, 1, 1);
        yield return new WaitForSeconds(0.5f);
        spriteTrans.localScale = new Vector3(1, 1, 1);
        _rightMoving = false;
        _topMoving = true;
        _rightBlinking = false;
        _transRight.Find("Sprite").GetComponent<Image>().color = _originColor;
        _topBlinking = true;
        _blinkTime = _blinkTime_Color;
        spriteTrans.GetComponent<Image>().color = _originColor;
        ShowValuesAndCheck();
        _tweenArrowKeys.Appear();
    }

    IEnumerator Routine_RightClick(){
        Transform spriteTrans = _transRight.Find("Sprite");
        spriteTrans.localScale = new Vector3(1, 8, 1);
        yield return new WaitForSeconds(0.5f);
        spriteTrans.localScale = new Vector3(1, 1, 1);
        _rightMoving = true;
        _topMoving = false;
        _topBlinking = false;
        _transTop.Find("Sprite").GetComponent<Image>().color = _originColor;
        _rightBlinking = true;
        _blinkTime = _blinkTime_Color;
        spriteTrans.GetComponent<Image>().color = _originColor;
        ShowValuesAndCheck();
        _tweenArrowKeys.Appear();
    }

    public override void AddResults(){
        PatientRecord pr = PatientDataMgr.GetPatientRecord();
        DiagnoseTestItem dti = new DiagnoseTestItem();
        dti.AddValue(_textHorDisparity.text);
        dti.AddValue(_textHorTag.text);
        dti.AddValue(_textHorPrism.text);
        dti.AddValue(_textVerDisparity.text);
        dti.AddValue(_textVerTag.text);
        dti.AddValue(_textVerPrism.text);
        pr.AddDiagnosRecord("Fixation Disparity", dti) ;
    }

    public override bool ResultExist(){
        if(!base.ResultExist())
            return false;
        return true;
    }
}
