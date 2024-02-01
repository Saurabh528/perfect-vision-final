using UnityEngine;

public class Rotator : MonoBehaviour {
	enum RotDirection
	{
		CW,
		CCW,
		ANY,
	}
	public float speed = 100f;
	[SerializeField] RotDirection direction;
	float dir;
	private void Start()
	{
		if (direction == RotDirection.CCW)
			dir = 1;
		else if (direction == RotDirection.CW)
			dir = -1;
		else
			dir = Random.value > 0.5f ? 1 : -1;
	}

	// Update is called once per frame
	void Update () {
		float realspeed = speed;
		if (GamePlayController.Instance && GamePlayController.Instance.IsPlaying())
			realspeed = speed * GamePlayController.GetDifficultyValue(1, 1, 20, 4);
		transform.Rotate(0f, 0f, realspeed * Time.deltaTime * dir);
		
	}
}
