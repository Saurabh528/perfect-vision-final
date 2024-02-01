using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Standard : MonoBehaviour
{
    public static string theDate;
    public static int Trial;

    private void Awake()
    {
        Trial = 1;
    }
    private void Start()
    {
        theDate = System.DateTime.Now.ToString("dd-MM-yyyy_hh-mm-ss");
        
        WriteLevelToDataManager();
    }
    public void SceneSwitcher(int SceneNumber)

    {
       SceneManager.LoadScene(SceneNumber);
    }
    public void WriteLevelToDataManager()//(int level)
    {
       
        // DataManagement.currentLevel = level;
        DataManagement.recordDistance = true; // starts recording distance
        DataManagement.distance = 0;
//        DataManagement.previousPosition = tinker_u_sound.final_position;
        Debug.Log(DataManagement.recordDistance);
    }
}
