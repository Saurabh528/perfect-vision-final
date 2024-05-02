using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using System.ComponentModel;

public class ColorSwitchController : GamePlayController
{
	//public static ColorSwitchController Instance;
	[SerializeField] Text textScore;
	[SerializeField] Text textLevel;
	public static int highScore;
	public Text highScoreBox;
	public Text scoreBoxPrompt;
	public GameObject highScoreCongratulate;
	public SpriteRenderer sr;
	public Rigidbody2D rb;
	public float jumpForce = 10f;
	public GameObject particle;
	public GenerateLevel generateLevel;
	public CameraShake cameraShake;
	string[] patternkindstrs = {"Solid", "Dot", "Wave", "Stripe"};
	[SerializeField] Sprite[] sprites;
	int curPatternIndex = -1;
	int fallcount;



	public override void StartGamePlay()
	{
		base.StartGamePlay();
		textScore.enabled = true;
		textLevel.enabled = true;
		EnableTime(true);
		highScore = PlayerPrefs.GetInt(DataKey.GetPrefKeyName ("score"), 0);
		generateLevel.enabled = true;
		GenerateLevel.nextnames.Add("Stripe, Wave");
		SetRandomPattern();		
	}

	public void SetRandomPattern()
	{
		int index = Random.Range(0, patternkindstrs.Length);
		int repeat = 0;
		while (true)
		{
			repeat++;
			if (index == curPatternIndex && repeat < 15)
			{
				index = Random.Range(0, patternkindstrs.Length);
			}
			else
			{
				List<string> patternnames = GenerateLevel.nextnames;
				bool include = true;
				foreach (string str in patternnames)
				{
					if (!str.Contains(patternkindstrs[index]))
					{
						include = false;
						break;
					}
				}
				if (include)
					break;
				index = Random.Range(0, patternkindstrs.Length);
			}
			
		}
		curPatternIndex = index;
		sr.sprite = sprites[curPatternIndex];
		sr.color = (curPatternIndex == 0 || curPatternIndex == 2) ? ColorCalibration.RedColor : ColorCalibration.CyanColor;
		
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
				if (IsStoped())
						MakeStopOrMovable(false);
				rb.velocity = Vector2.up * jumpForce;
			}
		}
		
	}
	public override void OnScoreChange(int levelstartscore, int score)
	{
		base.OnScoreChange(levelstartscore, score);
		if (score >= levelstartscore + 3)
			IncreaseLevel();
	}

	public override void IncreaseLevel()
	{
		base.IncreaseLevel();
	}

	public override void SetInitialLevelAndScore(string keyname, SavedGameData sgd)
	{
		base.SetInitialLevelAndScore(keyname, sgd);
	}

	public override void ShowLevel()
	{
		textLevel.text = $"Level {_level}";
	}

	public override void ShowScore()
	{
		textScore.text = $"{_score}";
	}

	bool IsStoped()
	{
		return (rb.constraints & RigidbodyConstraints2D.FreezePositionY) != 0;
	}
	void MakeStopOrMovable(bool stop)
	{
		if (stop)
			rb.constraints |= RigidbodyConstraints2D.FreezePositionY;
		else
			rb.constraints &= ~(RigidbodyConstraints2D.FreezePositionY);
	}
	public override void RestartGame()
	{
		if (GameState.currentGamePlay != null)
		{
			transform.position = new Vector3(0, Camera.main.transform.position.y - 2.75f, 0);
			rb.velocity = Vector2.zero;
			MakeStopOrMovable(true);
			GetComponent<SpriteRenderer>().enabled = true;
			_levelStartScore = _score;
			SetPlayingState(true);

		}
		else
			base.RestartGame();
		
	}

	void OnTriggerEnter2D(Collider2D col)
	{
		//If player goes below screen, game over
		if (col.tag == "MainCamera" && GamePlayController.Instance.IsPlaying())
		{
			fallcount++;
			if(fallcount == 3)
			{
				fallcount = 0;
				_level = 1;
				_levelStartScore = _score;
				ShowLevel();
			}
			Lost();
			return;
		}
		else if (col.tag == "ColorSwitch")
		{
			
			col.enabled = false;
			if(col.name.Contains("Star"))
			{
				
				Destroy(col.gameObject);
				GenerateLevel.CreateObstacle();
				SetRandomPattern();
			}
			else if (col.name != patternkindstrs[curPatternIndex] && GamePlayController.Instance.IsPlaying())
			{
				/* if(col.GetComponent<SpriteRenderer>() != null) for pattern testing
					col.gameObject.SetActive(false);
				else
					col.transform.parent.gameObject.SetActive(false); */
				col.enabled = false;
				Lost();
				return;
			}
		}
		
		
	}

	public void Lost()
	{
		highScoreCongratulate.SetActive(false);
		
		if (_score > highScore)
		{
			highScore = _score;
			PlayerPrefs.SetInt(DataKey.GetPrefKeyName ("score"), _score);
			highScoreCongratulate.SetActive(true);
		}
		highScoreBox.text = "High Score : " + highScore;
		scoreBoxPrompt.text = "Your score: " + _score;
		GameObject ps = Instantiate(particle, gameObject.transform.transform.position, Quaternion.identity);
		ParticleSystem.MainModule main = ps.GetComponent<ParticleSystem>().main;
		main.startColor = sr.color;
		if (GameState.currentGamePlay != null)
		{
			IncreaseScore(-3);
		}
		cameraShake.Shake();
		gameObject.GetComponent<SpriteRenderer>().enabled = false;
		MakeStopOrMovable(true);
		GameOver();
	}


	public override void GameOver()
	{
		StartCoroutine(Routine_GameOver());
	}

	IEnumerator Routine_GameOver()
	{
		SetPlayingState(false);
		yield return new WaitForSeconds(.7f);
		GetGameOverPanel().SetActive(true);
	}

	public override void StopPlay()
	{
		base.StopPlay();
		Rotator[] rotators = GameObject.FindObjectsByType<Rotator>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
		foreach (Rotator rotator in rotators)
			rotator.enabled = false;
		rb.velocity = Vector2.zero;
		rb.constraints = RigidbodyConstraints2D.FreezeAll;
	}

}

