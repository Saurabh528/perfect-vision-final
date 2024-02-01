using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.ComponentModel;
using TMPro;

public class CircleAutomaticController : GamePlayController
{

	[SerializeField] TextMeshProUGUI textScore;
	[SerializeField] TextMeshProUGUI textLevel;

	public CircularMotion_Circle circularMotionController;
	public float initialSeparation = 0.0f; // Set initial separation to 0
	public float totalTimeToReduceSeparation = 120.0f; // 2 minutes

	private float timeSinceStart = 0.0f;
	private float targetSeparation = -3.0f; // Set target separation to -3
	private bool isStopped = false;
	private float originalSpeed;
	private Vector3 initialPosition;

	// Use this for initialization
	public override void Start()
	{
		base.Start();

		if (circularMotionController == null)
		{
			Debug.LogError("CircularMotion_Circle component not assigned to AutoSeparateController.");
			enabled = false;
			return;
		}

		circularMotionController.separationMultiplier = initialSeparation;
		originalSpeed = circularMotionController.speed;
		initialPosition = transform.position;
	}

	public override void StartGamePlay()
	{
		base.StartGamePlay();
		circularMotionController.StartGame();
	}

	public override void Update()
	{
		base.Update();
		if (!GamePlayController.Instance.IsPlaying())
			return;
		if (Input.GetKeyDown(KeyCode.Space))
		{
			isStopped = !isStopped;
			circularMotionController.speed = isStopped ? 0.0f : originalSpeed;

			if (!isStopped)
			{
				// Reset position, initialSeparation, and targetSeparation
				transform.position = initialPosition;
				circularMotionController.separationMultiplier = initialSeparation;
				timeSinceStart = 0.0f;
			}
		}

		if (!isStopped)
		{
			timeSinceStart += Time.deltaTime;
			float progress = Mathf.Clamp01(timeSinceStart / totalTimeToReduceSeparation);
			circularMotionController.separationMultiplier = Mathf.Lerp(initialSeparation, targetSeparation, progress);
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


}

