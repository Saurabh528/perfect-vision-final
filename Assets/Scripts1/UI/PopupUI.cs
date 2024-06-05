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
    [SerializeField] Button YesButton, NoButton;
    [SerializeField] TextMeshProUGUI txtExplain;
    [SerializeField] GameObject objDialog;
    void Awake(){
        Instance = this;
        GameObject.DontDestroyOnLoad(gameObject);
    }

    public static void ShowQuestionBox(string question, UnityAction yesaction, UnityAction noaction = null){
        if(Instance == null){
            GameObject canvasPopupPrefab = Resources.Load<GameObject>("Prefab/CanvasPopup");
            if (canvasPopupPrefab != null)
            {
                // Instantiate the prefab in the scene
                GameObject canvasPopupInstance = Instantiate(canvasPopupPrefab);
                Instance = canvasPopupInstance.GetComponent<PopupUI>();
                // Optionally, set the parent of the instantiated prefab if needed
                // For example, to make it a child of the current GameObject:
                // canvasPopupInstance.transform.SetParent(transform, false);
            }
        }
        if(Instance == null)
            return;
        Instance.txtExplain.text = question;
        Instance.YesButton.onClick.RemoveAllListeners();
        Instance.YesButton.onClick.AddListener(yesaction);
        Instance.YesButton.onClick.AddListener(()=>Instance.objDialog.SetActive(false));
        Instance.NoButton.onClick.RemoveAllListeners();
        if(noaction != null)
            Instance.NoButton.onClick.AddListener(noaction);
        Instance.NoButton.onClick.AddListener(()=>Instance.objDialog.SetActive(false));
        Instance.objDialog.SetActive(true);
    }

   
}
