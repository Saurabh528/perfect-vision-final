using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class PhoriaGameController : DiagnosticController
{

    [SerializeField] TipUI _tipUI;
    [SerializeField] TweenAppear _tweenTop, _tweenUseLeftArrow, _tweenUseRightArrow, _tweenClickRight, _tweenBottom, _tweenWheel;
    [SerializeField] RectTransform _horLine, _horDot, _verLine, _verDot;
    [SerializeField] Text _topTweenText;
    [SerializeField] PhoriaObserveUI _observerLeft, _observerRight;
    [SerializeField] Toggle _toggleLeft, _toggleRight;

    const string TIPTEXT_60cm = "Viewing Distance set to Near = 60cm";
    const string TIPTEXT_RightFixating = "Put the Red Circle on top of Blue Circle.";
    public const string GameName = "Phoria";
    const float DOTPLACE_RANGE = 0.15f;
    bool _movable;
    EYESIDE _sideFixating = EYESIDE.LEFT;
    float _moveSpeed = 30;
    float _zoomFactor = 1;
    float _verlineX, _verDotY;
    bool resultExist;


    // Start is called before the first frame update
    void Start()
    {

        StartGame();
    }

    // Update is called once per frame
    void Update()
    {
       

        if(!_movable){
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (scroll != 0){
                _zoomFactor *= (1f + scroll);
                if(_zoomFactor < 1)
                    _zoomFactor = 1;
                else if(_zoomFactor > 100)
                    _zoomFactor = 100;
                ApplyZoom();
            }
            return;
        }
		float horValue = Input.GetAxis("Horizontal") * _moveSpeed * Time.deltaTime;
        if(horValue != 0){
            _verlineX += horValue;
            _verLine.transform.localPosition = new Vector3(_verlineX * _zoomFactor, 0, 0);
        }
        float verValue = Input.GetAxis("Vertical") * _moveSpeed * Time.deltaTime;
        if(verValue != 0){
            _verDotY += verValue;
            _verDot.transform.localPosition = new Vector3(0, _verDotY * _zoomFactor, 0);
        }
        if(horValue != 0 || verValue != 0){
            ShowValues();
        }

        
    }

    void ApplyZoom(){
        _verLine.localPosition = new Vector3(_verlineX * _zoomFactor, 0, 0);
        _verLine.localScale = _horDot.localScale = new Vector3(_zoomFactor, 1, 1);
        _verDot.localPosition = new Vector3(0, _verDotY * _zoomFactor, 0);
        _verDot.localScale = _horLine.localScale = new Vector3(1, _zoomFactor, 1);
    }

    void ShowValues(){
        if(_sideFixating == EYESIDE.LEFT){
            bool passed = _observerLeft.ShowValues(_verLine.transform.localPosition.x, _verDot.transform.localPosition.y, _sideFixating);
            if(passed && _movable)
                StartCoroutine(StartClickhereRoutine());
        }
        else{
            bool passed = _observerRight.ShowValues(_verLine.transform.localPosition.x, _verDot.transform.localPosition.y, _sideFixating);
            if(passed && _movable)
                StartCoroutine(ShowResult());
        }
    }

    void StartGame(){
        resultExist = false;
        ApplyColorCalliToImage(_verLine, ColorChannel.CC_Red);
        ApplyColorCalliToImage(_verDot, ColorChannel.CC_Red);
        ApplyColorCalliToImage(_horLine, ColorChannel.CC_Cyan);
        ApplyColorCalliToImage(_horDot, ColorChannel.CC_Cyan);
        PlaceDotsRandom();
        _tipUI.Show(TIPTEXT_60cm);
        _toggleLeft.enabled = false;
        _toggleRight.enabled = false;
    }

    public void OnToggleRightFixating(bool value){
        if(!value)
            return;
        _toggleLeft.enabled = false;
        _toggleRight.enabled = false;
        StartCoroutine(StartRightFixating());
    }

    public void OnToggleLeftFixating(bool value){
        if(!value)
            return;
    }

    public void OnClickedTipUIButton(Text tipText){
        if(tipText.text == TIPTEXT_60cm)
            StartCoroutine(StartLeftFixating());
        else if(tipText.text == TIPTEXT_RightFixating){
            _movable = true;
            _zoomFactor = 1;
            ApplyZoom();
            _topTweenText.text = "Align the blue dot over the red dot on the screen.";
            _tweenTop.Appear();
        }
    }

    void PlaceDotsRandom(){
        _zoomFactor = 1;
        _verlineX = Random.Range(-Screen.width * DOTPLACE_RANGE, Screen.width * DOTPLACE_RANGE);
        while(Mathf.Abs(_verlineX) < 20)
            _verlineX = Random.Range(-Screen.width * DOTPLACE_RANGE, Screen.width * DOTPLACE_RANGE);
        _verDotY = Random.Range(20, Screen.height * DOTPLACE_RANGE);
        ApplyZoom();
    }


    void ApplyColorCalliToImage(RectTransform trans, ColorChannel channel){
        SpriteColorCalibrate scc = trans.gameObject.AddComponent<SpriteColorCalibrate>();
        scc._ColorChannel = channel;
    }

    IEnumerator StartLeftFixating(){
        ShowValues();
        _topTweenText.text = "Align the red dot over the blue dot on the screen.";
        _tweenTop.Appear();
        yield return new WaitForSeconds(1);
        _tweenUseLeftArrow.Appear();
        _movable = true;
        _zoomFactor = 1;
        ApplyZoom();
    }

    IEnumerator StartClickhereRoutine(){
        _movable = false;
        _tweenTop.Disappear();
        yield return new WaitForSeconds(1);
        _tweenUseLeftArrow.Disappear();
        yield return new WaitForSeconds(1);
        _tweenWheel.Appear();
        yield return new WaitForSeconds(2);
        _tweenClickRight.Appear();
        _toggleLeft.enabled = true;
        _toggleRight.enabled = true;
    }

    IEnumerator StartRightFixating(){
        _tweenClickRight.Disappear();
        _tweenWheel.Disappear();
        yield return new WaitForSeconds(1);
        _tweenUseRightArrow.Appear();
        yield return new WaitForSeconds(1);
        ApplyColorCalliToImage(_verLine, ColorChannel.CC_Cyan);
        ApplyColorCalliToImage(_verDot, ColorChannel.CC_Cyan);
        ApplyColorCalliToImage(_horLine, ColorChannel.CC_Red);
        ApplyColorCalliToImage(_horDot, ColorChannel.CC_Red);
        PlaceDotsRandom();
        _tipUI.Show(TIPTEXT_RightFixating);
        _sideFixating = EYESIDE.RIGHT;
        
    }

    IEnumerator ShowResult(){
        resultExist = true;
        _movable = false;
        _tweenUseRightArrow.Disappear();
        yield return new WaitForSeconds(1);
        _tweenWheel.Appear();
        yield return new WaitForSeconds(1);
        _tweenTop.Disappear();
        yield return new WaitForSeconds(3);
        _tweenBottom.Appear();
        yield return new WaitForSeconds(3);
        _tweenBottom.Disappear();
    }

    public override void AddResults(){
        PatientRecord pr = PatientDataMgr.GetPatientRecord();
        DiagnoseTestItem dti = new DiagnoseTestItem();
        dti.AddValue($"{_observerLeft._textHorVal.text}:{_observerLeft._textHorTag.text}");
        dti.AddValue($"{_observerLeft._textVerVal.text}:{_observerLeft._textVerTag.text}");
        dti.AddValue($"{_observerLeft._textCombineVal.text}:{_observerLeft._textCombineTag.text}");
        dti.AddValue($"{_observerRight._textHorVal.text}:{_observerRight._textHorTag.text}");
        dti.AddValue($"{_observerRight._textVerVal.text}:{_observerRight._textVerTag.text}");
        dti.AddValue($"{_observerRight._textCombineVal.text}:{_observerRight._textCombineTag.text}");
        pr.AddDiagnosRecord(GameName, dti) ;
    }

    public override bool ResultExist(){
        if(!base.ResultExist())
            return false;
        return resultExist;
    }
}
