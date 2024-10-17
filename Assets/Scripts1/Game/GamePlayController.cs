using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using TMPro;
using UnityEngine.UI;
using PlayFab.ClientModels;

using UnityEngine.SceneManagement;

public class GamePlayController : MonoBehaviour
{
	public static GamePlayController Instance;
	[SerializeField] float _timeleft = 360;
	[SerializeField] GameObject helpPanel;
	[SerializeField] GameObject gameOverPanel;
	[SerializeField] GameObject nextGamePanel, pausePanel;
	[SerializeField] GameObject sessionFinishPanel;
	[SerializeField] AudioClip correctClip;
	[SerializeField] AudioClip wrongClip;
	[SerializeField] AudioSource effectSndSource;
	[SerializeField] Image imageBackButton;
	[SerializeField] Sprite spritePause, spriteBack;
	public TextMeshProUGUI textTime;

	private string currentSceneName;


	bool _isPlaying = false;
	protected int _score = 0;
	protected int _level = 1;
	protected int _levelStartScore;
	DateTime _startTime;
	float _duration;
	static Dictionary<string, SavedGameData> savedGameData = new Dictionary<string, SavedGameData>();
	public AudioSource _backAudio;
	public virtual void Awake()
	{
#if UNITY_EDITOR
		//_timeleft = 30;
#endif
        //UISignIn.StartFromSignInDebugMode();
        Instance = this;
		if (GameState.currentGameMode == GAMEMODE.SessionGame)
			_timeleft = SessionMgr._timeSecond;
	}
	// Start is called before the first frame update
	public virtual void Start()
    {
		if(helpPanel){
			helpPanel.SetActive(true);
			if(imageBackButton)
				imageBackButton.sprite = spriteBack;
		}
		else if(imageBackButton)
			imageBackButton.sprite = spritePause;

		
	}

    // Update is called once per frame
    public virtual void Update()
    {
		if (_isPlaying && _timeleft > 0)
		{
			_duration += Time.deltaTime;
			_timeleft -= Time.deltaTime;
			if (_timeleft < 0 && !IsDiagnosticMode())
			{
				_timeleft = 0;
				if(GameState.currentGamePlay != null)
				{
					StopPlay();
					RecordSessionGameState();
					if(GameState.currentSessionPlayIndex == SessionMgr.GetGameList().Count - 1)
					{
						sessionFinishPanel.SetActive(true);
						Cursor.visible = true;
						if (_backAudio)
							_backAudio.Stop();
					}
					else
					{
						nextGamePanel.SetActive(true);
						Cursor.visible = true;
						if (_backAudio)
							_backAudio.Stop();
					}
				}
				else{
					OnTimeisUp();
				}
				
			}
			textTime.text = UtilityFunc.ConvertSec2MMSS(_timeleft);
		}
    }

	public virtual void OnTimeisUp(){

	}

	public virtual void StartGamePlay()
	{
		SetPlayingState(true);
		if (imageBackButton && !IsDiagnosticMode())
			imageBackButton.sprite = spritePause;
		_startTime = DateTime.Now;
		helpPanel.SetActive(false);
		if (_backAudio)
			_backAudio.Play();

		//StartCoroutine(DelayedLoadScene(3)); // Load scene after a delay
	}

	// Coroutine for delayed scene loading
	private IEnumerator DelayedLoadScene(float delay)
	{
		yield return new WaitForSeconds(delay); // Wait for the specified delay
		LoadNewScene(); // Load the scene after the delay
	}

	public void LoadNewScene()
	{
		SceneManager.LoadScene("Snake");
	}



	public bool IsPlaying()
	{
		return _isPlaying;
	}

	public void IncreaseScore(int inc = 1)
	{
		_score += inc;
		if (_score < 0)
		{
			_score = 0;
			if(GameState.currentGamePlay == null)
				GameOver();
		}
		OnScoreChange(_levelStartScore, _score);
	}

	public virtual void OnScoreChange(int levelstartscore, int score)
	{
		ShowScore();
	}

	public virtual void GameOver()
	{
		SetPlayingState(false);
		gameOverPanel.SetActive(true);
		Cursor.visible = true;
		if (_backAudio)
			_backAudio.Stop();
	}

	public void ClearScoreAndLevel()
	{
		_levelStartScore = _score = 0;
		ShowScore();
		_level = 1;
		ShowLevel();
	}

	public virtual void IncreaseLevel(int delta = 1)
	{
		_levelStartScore = _score;
		_level += delta;
		if(_level <= 0)
			_level = 1;
		ShowLevel();
	}

	public virtual void ShowLevel() { }

	public virtual void RestartGame()
	{
		_levelStartScore = _score;
		SetPlayingState(true);
		ChangeScene.RestartScene();
	}

	public virtual void SetInitialLevelAndScore(string keyname, SavedGameData sgd)
	{
		_score = _levelStartScore = sgd.score;
		_level = sgd.level;
		if(GameState.currentGamePlay != null)
		{
			GameState.currentGamePlay.sScr = sgd.score;
			GameState.currentGamePlay.sLvl = sgd.score;
		}
		ShowScore();
		ShowLevel();
		GameState.currentGamePlay.sScr = sgd.score;
		GameState.currentGamePlay.sLvl = sgd.level;
	}

	public virtual void ShowScore() { }

	

	public virtual void OnBtnPause()
	{
		if(imageBackButton && imageBackButton.sprite == spriteBack)
			EndGame();
		else{
			if(pausePanel)
				pausePanel.SetActive(true);
			Time.timeScale = 0;
		}

	}

	void OnApplicationFocus(bool hasFocus)
    {
        if(!hasFocus && imageBackButton && imageBackButton.sprite == spritePause && pausePanel && !pausePanel.activeSelf)
			OnBtnPause();
    }

	public virtual void OnBtnResume()
	{
		Time.timeScale = 1;
	}

	public virtual void EndGame()
	{
		Cursor.visible = true;
		Time.timeScale = 1;
		if (GameState.currentGamePlay == null){
			if(IsDiagnosticMode())
				ChangeScene.LoadScene("Diagnostic");
			else
				ChangeScene.LoadScene("GamePanel");
		}
		else
		{
			if (_startTime.Ticks == 0)
				SessionMgr.CancelCurrentSessionGame();
			else
			{
				RecordSessionGameState();
			}

			ChangeScene.LoadScene(GameState.IsDoctor()? "Enrollment": "HomeTherapy");
		}
	}

	public virtual string RecordSessionGameState()
	{
		if (GameState.currentGamePlay == null)
			return "";
		//GameState.currentGamePlay.time = _startTime;
		GameState.currentGamePlay.duration = (int)_duration;
		GameState.currentGamePlay.eScr = _score;
		GameState.currentGamePlay.eLvl = _level;
		string key = GetPatientGameDataKey();
		savedGameData[key] = new SavedGameData(_score, _level);
		return key;
	}

	public void StartNextGame()
	{
		Time.timeScale = 1;
		SessionMgr.StartSessionGame(++GameState.currentSessionPlayIndex);
	}

	public void EndSession()
	{
		ChangeScene.LoadScene(GameState.IsDoctor()? "Enrollment": "HomeTherapy");
	}

	string GetPatientGameDataKey()
	{
		if (GameState.currentPatient == null || GameState.currentGamePlay == null)
			return "";
		return GameState.currentPatient.name;
	}


	public virtual void StopPlay()
	{
		SetPlayingState(false);
	}

	public virtual void SetPlayingState(bool state)
	{
		_isPlaying = state;
	}

	public GameObject GetGameOverPanel()
	{
		return gameOverPanel;
	}

	public int GetScore()
	{
		return _score;
	}

	public void EnableTime(bool enable)
	{
		textTime.enabled = enable;
	}

	public float GetRemainTime()
	{
		return _timeleft;
	}

	public static float GetDifficultyValue(float l0, float d0, float l1, float d1, float level = 0)
	{
		float a = (d0 - d1) / (l0 - l1);
		float b = d0 - a * l0;
		if (level == 0 && GamePlayController.Instance)
			level = GamePlayController.Instance._level;
		return a * level + b;
	}

	public void PlayCorrectSound()
	{
		if (effectSndSource && correctClip)
			effectSndSource.PlayOneShot(correctClip);
	}

	public void PlayWrongSound()
	{
		if (effectSndSource && wrongClip)
			effectSndSource.PlayOneShot(wrongClip);
	}

	public int GetLevel()
	{
		return _level;
	}

	public static bool IsDiagnosticMode()
	{
		return GameConst.MODE_DOCTORTEST || GameState.currentGameMode == GAMEMODE.DeviceSetting;
	}

	public virtual float GetDifficultyValue(){
		return 1;
	}

	/// <summary>
	/// This function is called when the object becomes enabled and active.
	/// </summary>
	void OnEnable()
	{
		ExitUI.EnableShutdownButton(false);
	}

	/// <summary>
	/// This function is called when the behaviour becomes disabled or inactive.
	/// </summary>
	void OnDisable()
	{
		ExitUI.EnableShutdownButton(true);
	}
}
