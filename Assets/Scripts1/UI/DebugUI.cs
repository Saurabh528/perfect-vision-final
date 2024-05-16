using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DebugUI : MonoBehaviour
{
    public static DebugUI Instance;
    [SerializeField] TextMeshProUGUI LogText;

    void Awake(){
        Instance = this;
        GameObject.DontDestroyOnLoad(gameObject);
    }

    public static void LogValue(string name, System.Object value){
        if(!Instance)
            return;
        Instance.LogText.text += $"\n{name}: {value.ToString()}";
    }

    public static void LogString(string text){
        if(!Instance)
            return;
        Instance.LogText.text += $"\n{text}";
    }
    
}
