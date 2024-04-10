using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public TextMeshProUGUI scoreText;//for storing scores
    public GameObject gameOver;
    public GameObject reload;
    public GameObject menu;
    public GameObject PausePanel;
    //public GameObject stopScript;

    int score;
     void Awake()
    {
        gameOver.SetActive(false);
    }

    public void GameOver()
    {
        Debug.Log("HITTTT");
        gameOver.SetActive(true);
        Time.timeScale = 0;

    }
    
    public void IncreaseScore()
    {
       Debug.Log("Score got increased");
       score++;
       scoreText.text = score.ToString();//converting our integer to string.
    }
    
    public void PauseGame()
    {
        PausePanel.SetActive(true);
        Time.timeScale = 0;
    }
    //Function to restart a scene
    public void RestartGame()
    {
        SceneManager.LoadScene("FlapNFly");
        Time.timeScale = 1;
    }

    public void LoadScene()
    {
        SceneManager.LoadScene("GamePanel");
    }
    public void ResumeGame()
    {
        PausePanel.SetActive(false);
        Time.timeScale = 1;
    }
}

