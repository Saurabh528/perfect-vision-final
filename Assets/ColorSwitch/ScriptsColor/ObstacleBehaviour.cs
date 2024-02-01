using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleBehaviour : MonoBehaviour
{
	public float space = 6;
	public int minLevelNum = 1;
	bool passed = false;

	// Update is called once per frame
	void OnTriggerEnter2D(Collider2D col)
	{
		if (col.tag == "DeadZone")
		{
			Destroy(gameObject);
			GenerateLevel.CreateObstacle();
		}
		else if (!gameObject.name.Contains("Star") && col.tag == "Player" && !passed)
		{
			passed = true;
			GamePlayController.Instance.IncreaseScore();
		}
	}
}
