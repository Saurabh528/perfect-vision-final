using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExitUI : MonoBehaviour
{
    public static ExitUI Instance;
    public static bool HideCursorOnResume = false;
    float showtime;

    static bool forceexit;
    [SerializeField] GameObject ExitPanel, btnShutdown;

    void Awake(){
        if(Instance){
            Destroy(gameObject);
            return;
        }
        Instance = this;
        GameObject.DontDestroyOnLoad(gameObject);
    }

    void OnApplicationQuit(){
        if(!IsShowingExitPanel() && !forceexit){
            
            Application.CancelQuit();
            HideCursorOnResume = !Cursor.visible;
            Cursor.visible = true;
            showtime = Time.time;
            Instance.ExitPanel.SetActive(true);
        }
    }

    static bool IsShowingExitPanel(){
        return Instance.ExitPanel.activeSelf;
    }

    public void OnBtnExitCancel(){
        
        Time.timeScale = 1;
        Cursor.visible = !HideCursorOnResume;
        Instance.ExitPanel.SetActive(false);
    }

    public void OnBtnExitOK(){
        Debug.Log("ClOSED POPUP");
        Time.timeScale = 0;
        Instance.ExitPanel.SetActive(true);
       
    }

    public void OnBtnLoginExitOk()
    {
        Debug.Log("Application will quit");
        Application.Quit();
    }
    void Update(){
        if(IsShowingExitPanel()){
            if(Time.time > showtime + 1)
                Time.timeScale = 0;
        }
        else
            return;
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            OnBtnExitOK();
        else if(Input.GetKeyDown(KeyCode.Escape))
            OnBtnExitCancel();
    }

    public static void EnableShutdownButton(bool enable){
        if(Instance == null)
            return;
        Instance.btnShutdown.SetActive(enable);
    }

    public static void ForceExit(){
        forceexit = true;
        Application.Quit();
    }
}
