using System.Collections;
using System.Collections.Generic;
using Org.BouncyCastle.Crypto.Parameters;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PopupUI : MonoBehaviour
{
    public static PopupUI Instance;
    [SerializeField] Button YesButton;
    [SerializeField] TextMeshProUGUI txtExplain;
    [SerializeField] GameObject objDialog;
    void Awake(){
        Instance = this;
        GameObject.DontDestroyOnLoad(gameObject);
    }

    public static void ShowQuestionBox(string question, UnityAction yesaction){
        Instance.txtExplain.text = question;
        Instance.YesButton.onClick.RemoveAllListeners();
        Instance.YesButton.onClick.AddListener(yesaction);
        Instance.YesButton.onClick.AddListener(()=>Instance.objDialog.SetActive(false));
        Instance.objDialog.SetActive(true);
    }

   
}
