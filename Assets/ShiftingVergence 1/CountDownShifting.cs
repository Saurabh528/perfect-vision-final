using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class CountDownShifting : MonoBehaviour
{
    public float timeLeft = 300f; // 5 minutes * 60 seconds
    public TextMeshProUGUI countdownText;
    public GameObject endPanel; // Reference to the panel that shows the results
    
    // public TextMeshProUGUI scoreText;
    public TextMeshProUGUI timeStampText;
    public TextMeshProUGUI startTimeText;
    public TextMeshProUGUI endTimeText;

    private SaveShifting SaveShifting;
    private bool gameDataSaved = false; // Add a flag to ensure we only save game data once

    private void Start()
    {
        SaveShifting = FindObjectOfType<SaveShifting>(); // Find the GameDataSaver script in the scene
    }

    private void Update()
    {
        if (timeLeft > 0)
        {
            timeLeft -= Time.deltaTime;
            int minutes = (int)(timeLeft / 60);
            int seconds = (int)(timeLeft % 60);
            countdownText.text = $"{minutes:00}:{seconds:00}";
        }
        else if (!gameDataSaved)
        {
            countdownText.text = "00:00";
            SaveGameData();
            ShowEndPanel();
            Invoke("SceneCHange", 10);
        }
    }

    private void SaveGameData()
    {
        SaveShifting.SaveGameData();
        gameDataSaved = true;
    }

    private void ShowEndPanel()
    {

        int currentLevel = LevelDetails.levels;
       

        timeStampText.text = $"Timestamp: {SaveShifting._timestamp}";
        startTimeText.text = $"Start Time: {SaveShifting._timeStart}";
        endTimeText.text = $"End Time: {SaveShifting._timeEnd}";

        endPanel.SetActive(true); // Show the panel
    }

    public void SceneCHange()
    {
        SceneManager.LoadScene("GamePanel");
    }
}



//CountDownShifting