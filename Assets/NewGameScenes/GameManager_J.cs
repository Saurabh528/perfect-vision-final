using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager_J : MonoBehaviour
{
   

    public GameObject cubePrefab;
    public List<Vector3> possiblePositions;
    private GameObject cube;
    public float changeDelay;
    public string correctDirectionMessage;
    public float initialDelay;
    private Vector3 currentDirection;

    public AudioClip correctGuessSound;


    void Start()
    {
        SpawnCube();
        StartCoroutine(AutomaticPositionChanges());

    }

    void SpawnCube()
    {
        //Vector3 randomPosition = possiblePositions[Random.Range(0, possiblePositions.Count)];
        //Color randomColor = new Color(Random.value, Random.value, Random.value);

        //cube = Instantiate(cubePrefab, randomPosition, Quaternion.identity);
        //CubeScript cubeScript = cube.GetComponent<CubeScript>();
        //cubeScript.position = randomPosition;
        //cubeScript.color = randomColor;

        Vector3 randomPosition = possiblePositions[Random.Range(0, possiblePositions.Count)];
        randomPosition.y = transform.position.y; // Keep y-coordinate constant

        Debug.Log("Spawn" + randomPosition.y);
        Color randomColor = new Color(Random.value, Random.value, Random.value);

        cube = Instantiate(cubePrefab, randomPosition, Quaternion.identity);
        CubeScript cubeScript = cube.GetComponent<CubeScript>();
        cubeScript.position = randomPosition;
       // cubeScript.color = randomColor;

    }


   

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            CheckPosition(Vector3.up);
        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            CheckPosition(Vector3.down);
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            CheckPosition(Vector3.left);
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            CheckPosition(Vector3.right);
        }
    }



 void CheckPosition(Vector3 direction)
    {
        Vector3 cubePosition = cube.GetComponent<CubeScript>().position;
        Debug.Log("cubePosition" + cubePosition);
        Vector3 targetPosition = cubePosition + direction;
        Debug.Log("targetPosition" + targetPosition);

        // Round positions to nearest integer
        cubePosition = new Vector3(Mathf.Round(cubePosition.x), Mathf.Round(cubePosition.y), Mathf.Round(cubePosition.z));
        targetPosition = new Vector3(Mathf.Round(targetPosition.x), Mathf.Round(targetPosition.y), Mathf.Round(targetPosition.z));

        if (possiblePositions.Contains(targetPosition))
        {
            if (targetPosition == cubePosition)
            {
                Debug.Log(correctDirectionMessage + " (direction: " + direction + ")");
                currentDirection = direction;
            }
            else
            {
                Debug.Log("You identified the cube's position correctly!");
                GetComponent<AudioSource>().PlayOneShot(correctGuessSound);
            }
        }
        else
        {
            Debug.Log("Sorry, that's not the cube's position.");
            GetComponent<AudioSource>().PlayOneShot(correctGuessSound);
        }
    }


    IEnumerator ChangePosition()
    {
        yield return new WaitForSeconds(changeDelay);
        Destroy(cube);
        SpawnCube();
    }

    IEnumerator AutomaticPositionChanges()
    {
        yield return new WaitForSeconds(initialDelay);

        while (true)
        {
            Vector3 randomPosition = possiblePositions[Random.Range(0, possiblePositions.Count)];
            cube.GetComponent<CubeScript>().position = randomPosition;

            yield return new WaitForSeconds(changeDelay);
        }
    }



}
