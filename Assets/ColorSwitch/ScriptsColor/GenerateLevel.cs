using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;

public class GenerateLevel : MonoBehaviour {
	public static GenerateLevel Instance;
    public GameObject[] obstacles;
    public GameObject star;
	float previousHeight;
	float lastCreatepos;
	int obstaclesTostar = 1;
	public static List<string> nextnames = new List<string>();

	private void Awake()
	{
		Instance = this;
	}
	void Start () {
        StartLevel();
	}

	public static void CreateObstacle()
	{
		Instance.MakeNewObstacle();
	}
    //Make new obsctacle whenever a new point is scored
    public void MakeNewObstacle() {
		
		GameObject obj;
		if (obstaclesTostar++ % 3 == 0)
		{
			obj = star;
			nextnames.Clear();
		}
		else
		{
			int index = Random.Range(0, obstacles.Length);
			obj = obstacles[index];
			while (true)
			{
				ObstacleBehaviour ob = obj.GetComponent<ObstacleBehaviour>();
				if (ob.minLevelNum <= GamePlayController.Instance.GetLevel())
					break;
				index = Random.Range(0, obstacles.Length);
				obj = obstacles[index];
			}
			
			string pattersstr = "";
			SpriteColorCalibrate[] components = obj.transform.GetComponentsInChildren<SpriteColorCalibrate>();
			foreach(SpriteColorCalibrate scc in components)
			{
				if (!pattersstr.Contains(scc.name))
					pattersstr += ", " + scc.name;
			}
			nextnames.Add(pattersstr);
		}
		float space = obj.GetComponent<ObstacleBehaviour>().space;
		lastCreatepos += (previousHeight + space) / 2;
		previousHeight = space;
		GameObject newobj = Instantiate(obj, new Vector3(0, lastCreatepos, 0), Quaternion.identity);

		
    }
    //Start level by creating two random obstacles
    private void StartLevel() {
		MakeNewObstacle();
		MakeNewObstacle();
		MakeNewObstacle();

	}
}
