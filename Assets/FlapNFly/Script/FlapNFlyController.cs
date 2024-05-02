using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class FlapNFlyController : GamePlayController
{
    [SerializeField] BirdFlap birdFlap;
    [SerializeField] PipeSpawnerScript pipeSpawnerScript;
    [SerializeField] TextMeshProUGUI textLevel, textScore;

    public override void StartGamePlay()
	{
		base.StartGamePlay();
		textScore.enabled = true;
		textLevel.enabled = true;
		EnableTime(true);
	}

    public override void Update()
	{
		base.Update();
		if (GamePlayController.Instance.IsPlaying())
		{
			if (Input.GetButtonDown("Jump")
			|| Input.GetMouseButtonDown(0)
			|| Input.GetKeyDown(KeyCode.UpArrow)
				|| Input.GetKeyDown(KeyCode.W))
			{
				if (!birdFlap.enabled){
                    birdFlap.enabled = pipeSpawnerScript.enabled = true;
                    birdFlap.Fly();
                }
						
			}
		}
	}

    public override void ShowLevel()
	{
		textLevel.text = $"Level {_level}";
	}

    public override void ShowScore()
	{
		textScore.text = $"{_score}";
	}

    public override void OnScoreChange(int levelstartscore, int score)
	{
		base.OnScoreChange(levelstartscore, score);
		if (score >= levelstartscore + 3)
			IncreaseLevel();
	}

    public override void GameOver(){
        base.GameOver();
        Time.timeScale = 0;
    }

    public override void RestartGame(){
        base.RestartGame();
        Time.timeScale = 1;
    }

    public override void StopPlay(){
        base.StopPlay();
        Time.timeScale = 0;
    }
    /* void Awake()
    {
    }

     public void GameOver()
    {
        Debug.Log("HITTTT");
        gameOver.SetActive(true);
        Time.timeScale = 0;

    } */
    
    /* public void IncreaseScore()
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
    } */
}

