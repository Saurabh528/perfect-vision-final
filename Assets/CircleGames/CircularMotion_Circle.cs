using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class CircularMotion_Circle : MonoBehaviour
{
    public Transform object1;
    public Transform object2;
    public float moveSpeed = 1f;
    public float maxDistance = 5f;
    public Slider speedSlider;
    private bool movingApart = true;
    public GameObject pauseButton;
    public GameObject resumeButton;
    public Transform circle;
    public Transform square;
    public TextMeshProUGUI distanceText;
    private void Awake()
    {
        pauseButton.SetActive(true);
        resumeButton.SetActive(false);
    }
    void Update()
    {
        float currentDistance = Vector3.Distance(object1.position, object2.position);
        moveSpeed = speedSlider.value;
        if (movingApart)
        {
            // Move objects apart
            object1.position += object1.right * moveSpeed * Time.deltaTime;
            object2.position -= object2.right * moveSpeed * Time.deltaTime;

            // Check if max distance reached
            if (currentDistance > maxDistance)
            {
                movingApart = false; // Start moving them back together
            }
        }
        else
        {
            // Move objects back together
            object1.position -= object1.right * moveSpeed * Time.deltaTime;
            object2.position += object2.right * moveSpeed * Time.deltaTime;

            // Optional: Check if objects are back at starting distance to reverse again
            // if (currentDistance <= maxDistance) // Using half the maxDistance for illustration
            // {
            //     movingApart = true; // Start moving them apart again
            // }
        }

        float distance = Vector3.Distance(circle.position, square.position);
        distanceText.text = "Distance: " + distance.ToString("F2");
    }

    public void PauseButton()
    {
        pauseButton.SetActive(false);
        resumeButton.SetActive(true);
        Time.timeScale = 0;
    }
    public void ResumeButton()
    {

        resumeButton.SetActive(false);
        pauseButton.SetActive(true);
        Time.timeScale = 1;
    }
}

// public class CircularMotion_Circle : MonoBehaviour
// {
//     public Transform object1;
//     public Transform object2;
//     public float separationSpeed = 1f;
//     public float maxDistance = 5f;
//     private float startTime;
//     private Vector3 object1StartPos;
//     private Vector3 object2StartPos;
//     private float journeyLength;

//     void Start()
//     {
//         startTime = Time.time;
//         object1StartPos = object1.position;
//         object2StartPos = object2.position;
//         // Assuming the objects are at their closest at the start
//         journeyLength = Vector3.Distance(object1StartPos, object2StartPos);
//     }

//     void Update()
//     {
//         float distCovered = (Time.time - startTime) * separationSpeed;
//         float fractionOfJourney = distCovered / journeyLength;

//         // Use PingPong to move back and forth between the start position and maximum separation distance
//         float pingPong = Mathf.PingPong(distCovered, maxDistance);

//         // Move objects
//         object1.position = Vector3.Lerp(object1StartPos, object1StartPos + (object1.right * maxDistance), pingPong / maxDistance);
//         object2.position = Vector3.Lerp(object2StartPos, object2StartPos - (object2.right * maxDistance), pingPong / maxDistance);

//         // Check if objects are at max distance to start crossing over
//         if (Vector3.Distance(object1.position, object2.position) >= maxDistance)
//         {
//             // Logic to make them cross over
//             // This example will just swap their start positions to demonstrate the crossover
//             Vector3 tempPos = object1StartPos;
//             object1StartPos = object2StartPos;
//             object2StartPos = tempPos;
//             startTime = Time.time; // Reset timing for smooth transition
//         }
//     }
// }



// using UnityEngine;
// using UnityEngine.UI;

// public class CircularMotion_Circle : MonoBehaviour
// {
//     // public Button separateButton;
//     // public Button reverseButton;

//     public float speed = 1.0f;
//     public float separationSpeed = 0.5f;
//     public float separationMultiplier = 0;

//     public GameObject bigCirclePrefab;
//     public GameObject smallCirclePrefab;

//     private GameObject redBigCircle;
//     private GameObject redSmallCircle;

//     private GameObject blueBigCircle;
//     private GameObject blueSmallCircle;

//     public float radius;

//     public float blinkInterval = 2.5f;
//     private float blinkTime;

//     private Vector3 center;
//     public Vector3 blueSmallCircleOffset;

//     void Start()
//     {
//        Reverse();
//     }

// 	public void StartGame()
// 	{
// 		center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane));
// 		transform.position = center;

// 		redBigCircle = Instantiate(bigCirclePrefab, transform);
// 		redBigCircle.GetComponent<SpriteRenderer>().color = Color.red;
// 		redBigCircle.transform.localPosition = Vector3.zero;

// 		redSmallCircle = Instantiate(smallCirclePrefab, redBigCircle.transform);
// 		redSmallCircle.GetComponent<SpriteRenderer>().color = Color.red;
// 		redSmallCircle.transform.localPosition = Vector3.zero;

// 		blueBigCircle = Instantiate(bigCirclePrefab, transform);
// 		blueBigCircle.GetComponent<SpriteRenderer>().color = Color.blue;
// 		blueBigCircle.transform.localPosition = Vector3.zero;

// 		blueSmallCircle = Instantiate(smallCirclePrefab, blueBigCircle.transform);
// 		blueSmallCircle.GetComponent<SpriteRenderer>().color = Color.blue;
// 		blueSmallCircle.transform.localPosition = blueSmallCircleOffset;

// 		blinkTime = 0.0f;

// 		/*separateButton.onClick.AddListener(Separate);
// 		reverseButton.onClick.AddListener(Reverse);*/
// 	}

//     void Update()
//     {
// 		// if (!GamePlayController.Instance.IsPlaying())
// 		// 	return;
//         // Calculate the position of the circles based on the current time, speed, and separation
//         float angle = Time.time * speed + Mathf.PI / 4;
//         float x = center.x + radius * Mathf.Cos(angle) + separationMultiplier;
//         float y = center.y + radius * Mathf.Sin(angle);

//         // Set the position of the red circles
//         redBigCircle.transform.position = new Vector3(x, y, 0);
//         redSmallCircle.transform.position = redBigCircle.transform.position;

//         // Set the position of the blue circles
//         blueBigCircle.transform.position = new Vector3(x + separationMultiplier, y, 0);
//         blueSmallCircle.transform.position = blueBigCircle.transform.position + blueSmallCircleOffset;

//         // Handle blinking
//         blinkTime += Time.deltaTime;
//         if (blinkTime >= blinkInterval)
//         {
//             blueSmallCircle.SetActive(!blueSmallCircle.activeSelf);
//             blinkTime = 0.0f;
//         }
//     }

//     public void Separate()
//     {
//         separationMultiplier += separationSpeed;
//     }

//     public void Reverse()
//     {
//         separationMultiplier -= separationSpeed;
//     }
// }
