using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour {

    public GameObject[] obstacles;
    public Transform[] spawnPositions;
    public GameObject token;
    public float timeBetweenSpawns, timeReduce, minTimeBetweenSpawns;
    public int tokenSpawnFrequency = 8;     //The lower the frequency is, the most likely to be spawned

    private int randomObst1, randomObst2, randomObst3, randomCol1, randomCol2, randomCol3;      //We will identify the random obstacles with these variables
    private Color[] colors = new Color[3] { Color.red, Color.blue, Color.red };  // making jatin change green to red

    void Start () {
		// Spawn();        //First spawn
		colors[0] = ColorCalibration.RedColor;
		colors[1] = ColorCalibration.CyanColor;
		colors[2] = ColorCalibration.RedColor;

		float initialSpawnDelay = 4f; // Set the initial spawn delay (2-3 seconds as per your requirement)
        Invoke("Spawn", initialSpawnDelay);
    }

    public void Spawn()
    {
        ChooseObstacles();
        int[] randomObstacles = new int[3] { randomObst1, randomObst2, randomObst3 };     //Creating an array of the 3 chosen obstacles' identifier
        ChooseColors();
        int[] randomColors = new int[3] { randomCol1, randomCol2, randomCol3 };     //Creating an array of the 3 chosen obstacles' identifier

        for (int i = 0; i < spawnPositions.Length; i++)
        {
            GameObject tempObstacle = Instantiate(obstacles[randomObstacles[i]], spawnPositions[i].position, Quaternion.identity);       //Spawns a random obstacle to the temporary spawnPosition with the same rotation
            if(FindObjectOfType<ShapeChangeController>()._showObstacles)
                tempObstacle.GetComponent<Renderer>().material.color = colors[randomColors[i]];
			else
				tempObstacle.GetComponent<Renderer>().material.color = Random.value > 0.5f? Color.red: Color.blue;

		}

        if (GamePlayController.Instance.IsPlaying())        //Invokes the next spawn only if the game is not over
        {
			Invoke("Spawn", timeBetweenSpawns);     //Next spawn after 'timeBetweenSpawns' secs
			if (Random.Range(0, tokenSpawnFrequency) == 0)      //If it is time to spawn a token
                Invoke("SpawnToken", timeBetweenSpawns / 2f);     //Then calls the function to spawn token
        }

            if ((timeBetweenSpawns - timeReduce) >= minTimeBetweenSpawns)
                timeBetweenSpawns -= timeReduce;        //reduces the timeBetweenSpawns after every spawn
    }

    public void SpawnToken()
    {
        Instantiate(token, spawnPositions[Random.Range(0, spawnPositions.Length)].position, Quaternion.identity);      //Spawns token to the spawner's position with same rotation
    }

    public void ChooseObstacles()
    {
        randomObst1 = Random.Range(0, obstacles.Length);        //The first obstacle can be any one of the three obstacles
        do
        {
            randomObst2 = Random.Range(0, obstacles.Length);
        } while (randomObst1 == randomObst2);       //The second obstacle can be anything but the previously selected one
        do
        {
            randomObst3 = Random.Range(0, obstacles.Length);
        } while ((randomObst1 == randomObst3) || (randomObst2 == randomObst3));     //The third obstacle can be only the remained one
    }

    public void ChooseColors()
    {
        randomCol1 = Random.Range(0, colors.Length);        //The first obstacle's color can be any one of the three colors
        do
        {
            randomCol2 = Random.Range(0, colors.Length);
        } while (randomCol1 == randomCol2);       //The second color can be anything but the previously selected one
        do
        {
            randomCol3 = Random.Range(0, colors.Length);
        } while ((randomCol1 == randomCol3) || (randomCol2 == randomCol3));     //The third color can be only the remained one
    }
}
