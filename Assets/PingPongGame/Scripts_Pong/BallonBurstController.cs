using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
public class BallonBurstController : GamePlayController {

	public Camera cam;
	public GameObject[] balls;
	public HatController hat_Controller;
	[SerializeField] Sprite[] backSprites;
	[SerializeField] SpriteRenderer backSpriteRender;
	[SerializeField] DestroyOnContact destroypool;
	public Text textScore;
	[SerializeField] Text textLevel;
	private float maxWidth;
	int backspriteIndex;
	public float speed;
	public float spawnperiod = 1;
	float spawnremainTime;
	[SerializeField] Transform _spawnLT, _spawnLB, _spawnRT, _spawnRB;
	// Use this for initialization
	public override void Start () {
		base.Start();
		spawnremainTime = spawnperiod;
		if (cam == null) {
			cam = Camera.main;
		}
		Vector3 upperCorner = new Vector3 (Screen.width, Screen.height, 0.0f);
		Vector3 targetWidth = cam.ScreenToWorldPoint(upperCorner);
		float ballWidth = balls[0].GetComponent<Renderer>().bounds.extents.x;
		maxWidth = targetWidth.x-ballWidth;
	}





	public override void  Update () {
		base.Update();
		if (GamePlayController.Instance.IsPlaying()) {
			spawnremainTime -= Time.deltaTime;
			if(spawnremainTime < 0)
			{
				spawnremainTime += spawnperiod - spawnperiod * Mathf.Clamp01(GamePlayController.GetDifficultyValue(1, 0.5f, 15, 1)) + Random.Range(0.4f, 0.8f);
				SpawnBall();
			}
		
		}
	}

	public void SpawnBall()
	{
		Vector3 startpos, endpos;
		/* if(Random.value > 0.5f)
		{
			startpos = Vector3.Lerp(_spawnLT.position, _spawnLB.position, Random.Range(0.1f, 0.9f));
			endpos = Vector3.Lerp(_spawnRT.position, _spawnRB.position, Random.Range(0.1f, 0.9f));
		}
		else
		{

			startpos = Vector3.Lerp(_spawnLT.position, _spawnRT.position, Random.Range(0.1f, 0.9f));
			endpos = Vector3.Lerp(_spawnLB.position, _spawnRB.position, Random.Range(0.1f, 0.9f));
		}
		if(Random.value > 0.5f)
		{
			Vector3 tmps = startpos;
			startpos = endpos;
			endpos = tmps;
		} */
		startpos = Vector3.Lerp(_spawnLT.position, _spawnRT.position, Random.Range(0.1f, 0.9f));
		endpos = Vector3.Lerp(_spawnLB.position, _spawnRB.position, Random.Range(0.1f, 0.9f));
		GameObject ball = balls[Random.Range(0, balls.Length)];

		
		Quaternion spawnRotation = Quaternion.identity;
		GameObject ballobj = (GameObject)Instantiate(ball, startpos, spawnRotation);
		ballobj.GetComponent<Rigidbody2D>().velocity = (endpos - startpos).normalized * speed * GamePlayController.GetDifficultyValue(1, 1, 20, 4);
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
		//ChangeBackground();
		destroypool.ResetMissingCount();
	}

	public override void SetInitialLevelAndScore(string keyname, SavedGameData sgd)
	{
		base.SetInitialLevelAndScore(keyname, sgd);
	}

	void ChangeBackground()
	{
		if (backSprites.Length < 2)
			return;
		backspriteIndex++;
		backspriteIndex %= backSprites.Length;
		backSpriteRender.sprite = backSprites[backspriteIndex];
	}


	public override void RestartGame()
	{
		base.RestartGame();
		GameObject[] ballons = GameObject.FindGameObjectsWithTag("Fruits");
		foreach (GameObject obj in ballons){
			GameObject.Destroy(obj);
		}
		GameObject[] bombs = GameObject.FindGameObjectsWithTag("Bomb");
		foreach (GameObject obj in bombs)
		{
			GameObject.Destroy(obj);
		}
		destroypool.ResetMissingCount();
	}

	public override void ShowScore()
	{
		textScore.text = $"Score {_score}";
	}

	public override void ShowLevel() {
		textLevel.text = $"Level {_level}";
	}

	public override void StartGamePlay()
	{
		base.StartGamePlay();
		Cursor.visible = false;
	}

	public override void OnBtnPause()
	{
		base.OnBtnPause();
		Cursor.visible = true;
	}

	public override void OnBtnResume()
	{
		base.OnBtnResume();
		Cursor.visible = false;
	}
}

