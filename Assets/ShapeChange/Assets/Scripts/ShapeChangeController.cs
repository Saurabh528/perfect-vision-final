using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class ShapeChangeController : GamePlayController
{

	[SerializeField] TextMeshProUGUI textScore;
	[SerializeField] TextMeshProUGUI textLevel;
	[SerializeField] Toggle _toggleShowObstacles;

	public Animation[] groundAnimations;
	public bool _showObstacles = true;
	public GameObject background, muteImage, spawner, jumpLeftButton, jumpRightButton;
	private Color[] colors = new Color[3] { Color.red, Color.blue, Color.red }; // make change here from green to red
	public TextMeshProUGUI highScoreText, endScoreText, endHighScoreText;
	// Use this for initialization
	public override void Start()
	{
		colors[0] = ColorCalibration.RedColor;
		colors[1] = ColorCalibration.CyanColor;
		colors[2] = ColorCalibration.RedColor;
		base.Start();
		_showObstacles = PlayerPrefs.GetInt("ShapeChange_ShowObstacle", 1) == 1;
		_toggleShowObstacles.isOn = _showObstacles;
		SetShowObstacles();
		HighScoreCheck();
	}


	public void HighScoreCheck()
	{
		/*if (FindObjectOfType<ScoreManager>().score > PlayerPrefs.GetInt("HighScore", 0))
		{
			PlayerPrefs.SetInt("HighScore", FindObjectOfType<ScoreManager>().score);
		}
		highScoreText.text = "BEST " + PlayerPrefs.GetInt("HighScore", 0).ToString();
		endHighScoreText.text = "BEST " + PlayerPrefs.GetInt("HighScore", 0).ToString();*/
	}


	public void AudioCheck()
	{
		FindObjectOfType<AudioManager>().soundIsOn = true;
		FindObjectOfType<AudioManager>().PlayBackgroundMusic();
		/*if (PlayerPrefs.GetInt("Audio", 0) == 0)
		{
			muteImage.SetActive(false);
			FindObjectOfType<AudioManager>().soundIsOn = true;
			FindObjectOfType<AudioManager>().PlayBackgroundMusic();
		}
		else
		{
			muteImage.SetActive(true);
			FindObjectOfType<AudioManager>().soundIsOn = false;
			FindObjectOfType<AudioManager>().StopBackgroundMusic();
		}*/
	}

	public override void StartGamePlay()
	{
		AudioCheck();
		base.StartGamePlay();
		spawner.SetActive(true);

		FindObjectOfType<JumpManager>().enabled = true;
		FindObjectOfType<Spawner>().enabled = true;
		textScore.enabled = true;
		textLevel.enabled = true;
		textTime.enabled = true;
		jumpLeftButton.SetActive(true);
		jumpRightButton.SetActive(true);

	}

	public override void OnScoreChange(int levelstartscore, int score)
	{
		base.OnScoreChange(levelstartscore, score);
		if (score >= levelstartscore + 5)
			IncreaseLevel();
	}


	


	public override void ShowScore() {
		textScore.text = $"{_score}";
	}

	public override void ShowLevel()
	{
		textLevel.text = $"Level {_level}";
	}

	public override void GameOver()
	{
		base.GameOver();
		textScore.enabled = false;
		textLevel.enabled = false;
		textTime.enabled = false;

	}


	public void AudioButton()
	{
		if (PlayerPrefs.GetInt("Audio", 0) == 0)
			PlayerPrefs.SetInt("Audio", 1);
		else
			PlayerPrefs.SetInt("Audio", 0);
		AudioCheck();
	}


	public void OnToggleShowObstacles(bool value)
	{
		PlayerPrefs.SetInt("ShapeChange_ShowObstacle", value? 1:0);
		_showObstacles = value;
		SetShowObstacles();
	}

	void SetShowObstacles()
	{
		if (_showObstacles)
		{
			for (int i = 0; i < background.transform.childCount; i++)
			{
				Transform child = background.transform.GetChild(i);
				child.gameObject.SetActive(true);
				child.GetComponent<Renderer>().material.color = colors[Random.Range(0, colors.Length)];
			}
		}
		else
		{
			for (int i = 0; i < background.transform.childCount; i++)
			{
				Transform child = background.transform.GetChild(i);
				child.gameObject.SetActive(false);
			}
		}
	}
}

