using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class PingPongController : GamePlayController {
	public Ball ball;

	public Paddle playerPaddle;
	public Paddle computerPaddle;
	public int computerScore { get; private set; }
	public Text computerScoreText;
	public float StoreSpeed;
	public SpriteRenderer ballRender;
	static Dictionary<string, int> savedcomputerScore = new Dictionary<string, int>();
	public float barColorTime = 8;
	bool colorSwitch;
	float remainColorTime;
	public Text textScore;
	[SerializeField] Text textLevel;
	// Use this for initialization
	public override void Start () {
		base.Start();
	}

	public override void StartGamePlay()
	{
		base.StartGamePlay();
		NewGame();
	}

	public void NewGame()
	{
		StartRound();
		colorSwitch = false;
		remainColorTime = barColorTime;
		SwitchBarColors();
	}

	public void SwitchBarColors()
	{
		colorSwitch = !colorSwitch;
		if (colorSwitch)
		{
			playerPaddle.GetComponent<SpriteRenderer>().color = ColorCalibration.RedColor;
			computerPaddle.GetComponent<SpriteRenderer>().color = ColorCalibration.CyanColor;
		}
		else
		{
			computerPaddle.GetComponent<SpriteRenderer>().color = ColorCalibration.RedColor;
			playerPaddle.GetComponent<SpriteRenderer>().color = ColorCalibration.CyanColor;
		}
	}

	public void StartRound()
	{
		playerPaddle.ResetPosition();
		computerPaddle.ResetPosition();
		ball.ResetPosition();
		ball.AddStartingForce();
		StoreSpeed = Paddle.speed;

	}


	public override void  Update () {
		base.Update();
		ColorChange();
		if (IsPlaying())
		{
			remainColorTime -= Time.deltaTime;
			if(remainColorTime < 0)
			{
				remainColorTime = barColorTime + Random.Range(-1f, 1f);
				SwitchBarColors();
			}
		}
	}

	public void ColorChange()
	{

		if (ball.transform.position.x > 0)
		{

			ballRender.color = ColorCalibration.CyanColor;
		}
		else
		{

			ballRender.color = ColorCalibration.RedColor;
		}
	}

	public override void ShowScore()
	{
		textScore.text = $"{_score}";
	}
	public override void OnScoreChange(int levelstartscore, int score)
	{
		base.OnScoreChange(levelstartscore, score);
		if (score >= levelstartscore + 3)
		{
			IncreaseLevel();
		}
		StartRound();
	}

	public override void IncreaseLevel(int delta = 1)
	{
		base.IncreaseLevel(delta);
		StoreSpeed = LevelDetails.SpeedAI[_level - 1];
		Debug.Log("Check Level wise Speed" + StoreSpeed);
	}

	public override void SetInitialLevelAndScore(string keyname, SavedGameData sgd)
	{
		base.SetInitialLevelAndScore(keyname, sgd);
		StoreSpeed = LevelDetails.SpeedAI[sgd.level - 1];
		if (savedcomputerScore.ContainsKey(keyname))
			SetComputerScore(savedcomputerScore[keyname]);
	}

	public override void SetPlayingState(bool state)
	{
		base.SetPlayingState(state);
		ball.SetPlaingState(state);
	}

	public void ComputerScores()
	{
		SetComputerScore(computerScore + 1);
		StartRound();
	}

	private void SetComputerScore(int score)
	{
		computerScore = score;
		computerScoreText.text = score.ToString();
	}

	public override string RecordSessionGameState()
	{
		string keyname = base.RecordSessionGameState();
		if (!string.IsNullOrEmpty(keyname))
			savedcomputerScore[keyname] = computerScore;
		return keyname;
	}

	public override void ShowLevel()
	{
		textLevel.text = $"Level {_level}";
	}

	public override float GetDifficultyValue(){
		return GamePlayController.GetDifficultyValue(1, 1f, 10, 2);
	}
}

