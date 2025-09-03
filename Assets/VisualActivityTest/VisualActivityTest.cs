using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using PlayFab.ClientModels;
using PlayFab;
public class VisualActivityTest : DiagnosticController
{
    public enum SYMBOLTYPE{
        RING = 0,
        SPRITE
    }

    public enum VATSTATE{
        WAIT_SYMBOLTYPE,
        GUIDE_TUTORIAL,
        GUIDE_COVERLEFT,
        GUIDE_SCREENDIST,
        PLAY_RIGHTEYE,
        GUIDE_COVERRIGHT,
        PLAY_LEFTEYE,
        SHOW_RESULT
    }

    SYMBOLTYPE SymbolType;
    VATSTATE State = VATSTATE.WAIT_SYMBOLTYPE;
    [SerializeField] TextMeshProUGUI NextBtnText, GuideText, CountText, ScoreText;
    [SerializeField] GameObject Obj_SymbolTypeBtns, Img_MatchBelowLine, Img_RingTuto, Img_CoverLeft, Img_CoverRight, Img_KeepDist,
        Panel_Test, Panel_Result, Obj_Spritebuttons, Obj_RingTestGuide;
    [SerializeField] Image RandomImg;
    [SerializeField] RectTransform rtRandomImg;
    [SerializeField] Sprite[] SpritePatterns;
    [SerializeField] Sprite SpriteRing;
    [SerializeField] float sizeMM = 10;
    const int MAX_TRYCOUNT = 45;
    int TryCount;
    int WrongCount, RightCycleCount, RightCount;
    int[] scores = new int[]{200, 200, 100, 80, 63, 50, 40, 32, 26, 20, 16, 12, 10, 8, 6, 4, 2};
    int RightScore, LeftScore;
    Vector3 initialPosition;
    // Start is called before the first frame update
    void Start()
    {
         CountText.gameObject.SetActive(false);
         ScoreText.gameObject.SetActive(false);
         Panel_Test.SetActive(false);
        Debug.Log($"Screen DPI: {ScreenDPI.GetPPI()}");

        int desiredPixelSize = DPICaculator.ConvertScreenMMToPixel(RandomImg.canvas, sizeMM, rtRandomImg.localScale.x);
		rtRandomImg.sizeDelta = new Vector2 (desiredPixelSize, desiredPixelSize);
    }

    // Update is called once per frame
    void Update()
    {
        if(SymbolType == SYMBOLTYPE.RING && (State == VATSTATE.PLAY_RIGHTEYE || State == VATSTATE.PLAY_LEFTEYE) && !EventSystem.current.IsPointerOverGameObject()){
            
            if(Input.GetMouseButtonDown(0)){
                initialPosition = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0);
            }
            else if(Input.GetMouseButtonUp(0)){
                Vector3 direction = new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0) - initialPosition;
                if(direction.magnitude < 30)
                    return;
                float angle = Mathf.Atan2(direction.y, direction.x) * 180 / Mathf.PI;
                Quaternion mouseRot = Quaternion.Euler(0, 0, angle);
                if(Quaternion.Angle(mouseRot, RandomImg.transform.rotation) < 22.05f)
                    OnGuessRight();
                else
                    OnGuessWrong();
            }
        }
    }

    public void OnSelectSymbolType(int type){
        SymbolType = (SYMBOLTYPE)type;
        State = VATSTATE.GUIDE_TUTORIAL;
        NextBtnText.transform.parent.gameObject.SetActive(true);
        Obj_SymbolTypeBtns.SetActive(false);
        if(SymbolType == SYMBOLTYPE.SPRITE){
            Img_MatchBelowLine.SetActive(true);
            GuideText.text = "Select the matching shape below the line!";
        }
        else{
            Img_RingTuto.SetActive(true);
            GuideText.text = "Swipe in the direction of emptiness in the circle!";
        }
    }

    public void OnBtnNext(){
        if(State == VATSTATE.WAIT_SYMBOLTYPE){

        }
        else if(State == VATSTATE.GUIDE_TUTORIAL){
            State = VATSTATE.GUIDE_COVERLEFT;
            GuideText.text = "First cover your child's left eye.";
            if(SymbolType == SYMBOLTYPE.SPRITE)
                Img_MatchBelowLine.SetActive(false);
            else
                Img_RingTuto.SetActive(false);
            Img_CoverLeft.SetActive(true);
        }
        else if(State == VATSTATE.GUIDE_COVERLEFT){
            State = VATSTATE.GUIDE_SCREENDIST;
            GuideText.text = "Keep your head at a distance you set in settings.";
            NextBtnText.text = "Start";
            Img_CoverLeft.SetActive(false);
            Img_KeepDist.SetActive(true);
        }
        else if(State == VATSTATE.GUIDE_SCREENDIST){
            State = RightScore == 0? VATSTATE.PLAY_RIGHTEYE: VATSTATE.PLAY_LEFTEYE;
            GuideText.gameObject.SetActive(false);
            NextBtnText.transform.parent.gameObject.SetActive(false);
            Img_KeepDist.SetActive(false);
            StartTest();
        }
        else if(State == VATSTATE.GUIDE_COVERRIGHT){
            State = VATSTATE.GUIDE_SCREENDIST;
            GuideText.text = "Keep your head at a distance you set in settings.";
            NextBtnText.text = "Start";
            Img_CoverRight.SetActive(false);
            Img_KeepDist.SetActive(true);
        }
    }

    void StartTest(){
        WrongCount = RightCount = RightCycleCount = TryCount = 0;
        Panel_Test.SetActive(true);
        if(SymbolType == SYMBOLTYPE.SPRITE)
            Obj_Spritebuttons.SetActive(true);
        else
            Obj_RingTestGuide.SetActive(true);
        CountText.gameObject.SetActive(true);
        ScoreText.gameObject.SetActive(true);
        StartCoroutine(Routine_SpawnRandomSymbol());
    }

    public void OnClickPattern(GameObject button){
        if(SymbolType == SYMBOLTYPE.SPRITE){
            if(button.name == RandomImg.sprite.name)
                OnGuessRight();
            else
                OnGuessWrong();
        }
        if(TryCount == MAX_TRYCOUNT){
            EndEyeTest();
        }
    }

    void OnGuessRight(){
        RightCount++;
        if(RightCount == 3){
            RightCycleCount++;
            RightCount = 0;
        }
        if(TryCount < MAX_TRYCOUNT)
            StartCoroutine(Routine_SpawnRandomSymbol());
    }

    void EndEyeTest(){
        if(State == VATSTATE.PLAY_RIGHTEYE){
            State = VATSTATE.GUIDE_COVERRIGHT;
            Panel_Test.SetActive(false);
            CountText.gameObject.SetActive(false);
            ScoreText.gameObject.SetActive(false);
            GuideText.text = "And now cover your child's right eye";
            GuideText.gameObject.SetActive(true);
            NextBtnText.text = "Next";
            NextBtnText.transform.parent.gameObject.SetActive(true);
            Img_CoverRight.SetActive(true);
        }
        else if(State == VATSTATE.PLAY_LEFTEYE)
            ShowTotalResult();

    }

    void OnGuessWrong(){
        WrongCount++;
        int score = ShowScore();
        if(WrongCount == 3){
            if(State == VATSTATE.PLAY_RIGHTEYE)
                RightScore = score;
            else
                LeftScore = score;
            EndEyeTest();
        }
        else if(TryCount < MAX_TRYCOUNT)
            StartCoroutine(Routine_SpawnRandomSymbol());
    }

    void ShowTotalResult(){
        State = VATSTATE.SHOW_RESULT;
        Panel_Test.SetActive(false);
        RandomImg.gameObject.SetActive(false);
        Panel_Result.SetActive(true);
        Panel_Result.transform.Find("LeftEye").GetComponent<TextMeshProUGUI>().text = $"Left eye: \t~20/{LeftScore}";
        Panel_Result.transform.Find("RightEye").GetComponent<TextMeshProUGUI>().text = $"Right eye:\t~20/{RightScore}";
        

        CountText.gameObject.SetActive(false);
        ScoreText.gameObject.SetActive(false);
    }

    
    IEnumerator Routine_SpawnRandomSymbol(){
        TryCount++;
        RandomImg.gameObject.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        if(SymbolType == SYMBOLTYPE.SPRITE){
            RandomImg.sprite = SpritePatterns[UnityEngine.Random.Range(0, SpritePatterns.Length)];
        }
        else{
            RandomImg.transform.rotation = Quaternion.Euler(0, 0, 45 * UnityEngine.Random.Range(0, 8));
            RandomImg.sprite = SpriteRing;
        }
        float zoom = (float)(MAX_TRYCOUNT - TryCount + 1) / MAX_TRYCOUNT;
        zoom *= zoom;
        RandomImg.transform.localScale = new Vector3(zoom, zoom, 1);
        RandomImg.gameObject.SetActive(true);
        ShowScore();
    }

    int ShowScore(){
        CountText.text = $"{TryCount}/{MAX_TRYCOUNT}";
        ScoreText.text = $"~20/{scores[RightCycleCount + 1]}";
        return scores[RightCycleCount];
    }

    public void OnClickRestart(){
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }


	public override void AddResults(){
        PatientRecord pr = PatientDataMgr.GetPatientRecord();
        DiagnoseTestItem dti = new DiagnoseTestItem();
        dti.AddValue($"~20/{LeftScore}");
        dti.AddValue($"~20/{RightScore}");
        pr.AddDiagnosRecord("Visual Acuity", dti) ;
    }

	public override bool ResultExist(){
        if(!base.ResultExist())
            return false;
        return State == VATSTATE.SHOW_RESULT;;
    }

    
}
