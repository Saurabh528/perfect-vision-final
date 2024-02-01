using UnityEngine;
using UnityEngine.UI;
using System;
using TMPro;

public class SaveShifting : MonoBehaviour
{
    public string gameName;

    public float gameTimeInSeconds;

    public float _timeStart;
    public float _timeEnd;
    public string _timestamp;

    private void Start()
    {
        _timeStart = Time.time;
        // quitButton.onClick.AddListener(OnQuitButtonClick);
        // Removed Invoke() since it will be called by CountdownTimer script
    }

    private void OnQuitButtonClick()
    {
        Application.Quit();
    }

    public void SaveGameData()
    {
        _timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        _timeEnd = Time.time;
        float durationPlayed = _timeEnd - _timeStart;


        int currentLevel = LevelDetails.levels;

        //gameNameText.text = $"Game Name: {gameName}";
        //durationText.text = $"Duration: {durationPlayed} seconds";
        //scoreText.text = $"Score: {currentScore}";
        //levelText.text = $"Level: {currentLevel}";

        //panel.SetActive(true);
    }
}


