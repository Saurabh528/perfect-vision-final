using System.Collections;
using System.Collections.Generic;
using Org.BouncyCastle.Asn1;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MuscleBalanceResultData{
    /* public Vector2 LT_L, CT_L, RT_L, LM_L, CM_L, RM_L, LB_L, CB_L, RB_L, 
            LT_R, CT_R, RT_R, LM_R, CM_R, RM_R, LB_R, CB_R, RB_R; */
    public Dictionary<string, Vector2> _dicDeviation = new Dictionary<string, Vector2>();
}


public class MascleBalanceController : DiagnosticController
{
    public const string GameName = "Muscle Balance";
    [SerializeField] TweenAppear _tweenUseArrowKeys, _tweenOnceAligned, _tweenRightFixating;
    [SerializeField] TipUI _topPopUp;
    [SerializeField] MuscleResultViewer _resultViewer_L, _resultViewer_R;
    [SerializeField] Transform _transRing, _transCross;
    [SerializeField] Transform[] _basePts;
    [SerializeField] float _moveSpeed = 30;
    [SerializeField] PhoriaObserveUI _observerL, _observerR;
    [SerializeField] Toggle _toggleRightFixation, _toggleViewResult;
    [SerializeField] GameObject _resultView;
    [SerializeField] Button _btnNext, _btnPrev;
    int _curIndex;
    EYESIDE _curSide;
    MuscleBalanceResultData _resultData_L = new MuscleBalanceResultData(), _resultData_R = new MuscleBalanceResultData();
    SingleEyeAlignmentData[,] resultValues = new SingleEyeAlignmentData[2, 9];//0:left, 1:right
    // Start is called before the first frame update
    void Start()
    {
        _curSide = EYESIDE.LEFT;
        StartCoroutine(Routine_StartGame());
        
        RecordWebcamVideo.StartRecord();
    }

    // Update is called once per frame
    void Update()
    {
        if(_curSide != EYESIDE.INVALID && !_resultView.activeSelf){
            float horValue = Input.GetAxis("Horizontal") * _moveSpeed * Time.deltaTime;
            float verValue = Input.GetAxis("Vertical") * _moveSpeed * Time.deltaTime;
            if(horValue != 0 || verValue != 0){
                Transform trans = _curSide == EYESIDE.LEFT? _transRing : _transCross;
                trans.transform.Translate(new Vector3(horValue, verValue, 0));
                ShowValues();
            }
        }
    }

    void OnEnable(){
        ExitUI.EnableShutdownButton(false);
    }

    void OnDestroy(){
        ExitUI.EnableShutdownButton(true);
    }


    public void OnToggleViewResult(bool value){
        if(value){
            _resultView.SetActive(true);
            bool isOK_L = _resultViewer_L.ShowPoints(_resultData_L, _basePts);
            bool isOK_R = _resultViewer_R.ShowPoints(_resultData_R, _basePts);
            if(!isOK_L && !isOK_R)
                _topPopUp.Show("Some points are missing in both fixation", 3);
            else if(!isOK_L)
                _topPopUp.Show("Some points are missing in Left fixation", 3);
            else if(!isOK_R)
                _topPopUp.Show("Some points are missing in Right fixation", 3);
        }
        else{
            _resultView.SetActive(false);
        }
        _btnNext.enabled = _btnPrev.enabled = !value;
    }

    public void OnToggleRightFixating(bool value){
        if(!value)
            return;
        if(_resultView.activeSelf){
            _resultView.SetActive(false);
            _toggleViewResult.isOn = false;
        }
        _tweenRightFixating.Disappear();
        _curSide = EYESIDE.RIGHT;
        StartCoroutine(Routine_StartGame());
    }

    public void OnBtnNextPoint(){
        if(_curSide == EYESIDE.INVALID)
            return;
        if(_curSide == EYESIDE.LEFT){
            _resultData_L._dicDeviation[_basePts[_curIndex].name] = _transRing.position - _basePts[_curIndex].position;
            resultValues[0, _curIndex] = new SingleEyeAlignmentData(
                _observerL._textHorVal.text,
                _observerL._textHorTag.text,
                _observerL._textVerVal.text,
                _observerL._textVerTag.text,
                _observerL._textCombineVal.text,
                _observerL._textCombineTag.text,
                _observerL._textDegreeVal.text
            );
        }
        else if(_curSide == EYESIDE.RIGHT){
            _resultData_R._dicDeviation[_basePts[_curIndex].name] = _transCross.position - _basePts[_curIndex].position;
            resultValues[1, _curIndex] = new SingleEyeAlignmentData(
                _observerR._textHorVal.text,
                _observerR._textHorTag.text,
                _observerR._textVerVal.text,
                _observerR._textVerTag.text,
                _observerR._textCombineVal.text,
                _observerR._textCombineTag.text,
                _observerR._textDegreeVal.text
            );
        }
        _curIndex++;
        if(_curIndex == _basePts.Length){
            _curIndex = 0;
            if(_curSide == EYESIDE.LEFT){
                _tweenRightFixating.Appear();
                _topPopUp.Show("Left Fixating completed", 3);
                _toggleRightFixation.interactable = true;
            }
            else{
                _topPopUp.Show("Right Fixating completed", 3);
                RecordWebcamVideo.ActivateSaveButton();
            }
            
        }
        else{
            if(_curIndex == 6)
                _tweenUseArrowKeys.Disappear();
            else if(_curIndex == 7)
                _tweenOnceAligned.Disappear();
            PlaceRingOrCross();
            ShowValues();
        }
        
    }

    public void OnBtnRestart(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void OnBtnPrevPoint(){
        if(_curSide == EYESIDE.INVALID)
            return;
        if(_curSide == EYESIDE.LEFT){
            _resultData_L._dicDeviation[_basePts[_curIndex].name] = _transRing.position - _basePts[_curIndex].position;
            resultValues[0, _curIndex] = new SingleEyeAlignmentData(
                _observerL._textHorVal.text,
                _observerL._textHorTag.text,
                _observerL._textVerVal.text,
                _observerL._textVerTag.text,
                _observerL._textCombineVal.text,
                _observerL._textCombineTag.text,
                _observerL._textDegreeVal.text
            );
        }
        else if(_curSide == EYESIDE.RIGHT){
            _resultData_R._dicDeviation[_basePts[_curIndex].name] = _transCross.position - _basePts[_curIndex].position;
            resultValues[1, _curIndex] = new SingleEyeAlignmentData(
                _observerR._textHorVal.text,
                _observerR._textHorTag.text,
                _observerR._textVerVal.text,
                _observerR._textVerTag.text,
                _observerR._textCombineVal.text,
                _observerR._textCombineTag.text,
                _observerR._textDegreeVal.text
            );
        }
        _curIndex--;
        if(_curIndex == -1){
            _curIndex = 0;
            return;
        }
        else{
            PlaceRingOrCross();
            ShowValues();
        }
    }

    IEnumerator Routine_StartGame(){
        _curIndex = 0;
        PlaceRingOrCross();
        ShowValues();
        yield return new WaitForSeconds(2);
        _tweenUseArrowKeys.Appear();
        yield return new WaitForSeconds(2);
        _topPopUp.Show("Put the Red Circle at the center of Blue Plus", 3);
        yield return new WaitForSeconds(2);
        _tweenOnceAligned.Appear();
    }

    void PlaceRingOrCross(){
        if(_curSide == EYESIDE.LEFT){
            _transCross.position = _basePts[_curIndex].position;
        }
        else if(_curSide == EYESIDE.RIGHT){
            _transRing.position = _basePts[_curIndex].position;
        }
    }

    void ShowValues(){
        if(_curSide == EYESIDE.LEFT)
            _observerL.ShowValues(-_transCross.position.x + _transRing.position.x, -_transCross.position.y + _transRing.position.y, _curSide);
        else
            _observerR.ShowValues(_transCross.position.x - _transRing.position.x, _transCross.position.y - _transRing.position.y, _curSide);
            
    }

    public override void AddResults(){
        PatientRecord pr = PatientDataMgr.GetPatientRecord();
        DiagnoseTestItem dti = new DiagnoseTestItem();
        for(int ptindex = 0; ptindex < 9; ptindex++){
            for(int eye = 0; eye < 2; eye++){
                dti.AddValue($"{resultValues[eye, ptindex].horVal}:{resultValues[eye, ptindex].horTag}");
                dti.AddValue($"{resultValues[eye, ptindex].verVal}:{resultValues[eye, ptindex].verTag}");
                dti.AddValue($"{resultValues[eye, ptindex].combineVal}:{resultValues[eye, ptindex].combineTag}");
            }
        }
        pr.AddDiagnosRecord(GameName, dti);
    }

    public override bool ResultExist(){
        if(!base.ResultExist())
            return false;
        for(int i = 0; i < 9; i++){
            if(resultValues[0, i] == null)
                return false;
             if(resultValues[1, i] == null)
                return false;
        }
        return true;
    }
}
