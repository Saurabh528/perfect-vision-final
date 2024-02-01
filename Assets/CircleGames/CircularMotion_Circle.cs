using UnityEngine;
using UnityEngine.UI;

public class CircularMotion_Circle : MonoBehaviour
{
    public Button separateButton;
    public Button reverseButton;

    public float speed = 1.0f;
    public float separationSpeed = 0.5f;
    public float separationMultiplier = 0;

    public GameObject bigCirclePrefab;
    public GameObject smallCirclePrefab;

    private GameObject redBigCircle;
    private GameObject redSmallCircle;

    private GameObject blueBigCircle;
    private GameObject blueSmallCircle;

    public float radius;

    public float blinkInterval = 2.5f;
    private float blinkTime;

    private Vector3 center;
    public Vector3 blueSmallCircleOffset;

    void Start()
    {
       
    }

	public void StartGame()
	{
		center = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, Camera.main.nearClipPlane));
		transform.position = center;

		redBigCircle = Instantiate(bigCirclePrefab, transform);
		redBigCircle.GetComponent<SpriteRenderer>().color = Color.red;
		redBigCircle.transform.localPosition = Vector3.zero;

		redSmallCircle = Instantiate(smallCirclePrefab, redBigCircle.transform);
		redSmallCircle.GetComponent<SpriteRenderer>().color = Color.red;
		redSmallCircle.transform.localPosition = Vector3.zero;

		blueBigCircle = Instantiate(bigCirclePrefab, transform);
		blueBigCircle.GetComponent<SpriteRenderer>().color = Color.blue;
		blueBigCircle.transform.localPosition = Vector3.zero;

		blueSmallCircle = Instantiate(smallCirclePrefab, blueBigCircle.transform);
		blueSmallCircle.GetComponent<SpriteRenderer>().color = Color.blue;
		blueSmallCircle.transform.localPosition = blueSmallCircleOffset;

		blinkTime = 0.0f;

		/*separateButton.onClick.AddListener(Separate);
		reverseButton.onClick.AddListener(Reverse);*/
	}

    void Update()
    {
		if (!GamePlayController.Instance.IsPlaying())
			return;
        // Calculate the position of the circles based on the current time, speed, and separation
        float angle = Time.time * speed + Mathf.PI / 4;
        float x = center.x + radius * Mathf.Cos(angle) + separationMultiplier;
        float y = center.y + radius * Mathf.Sin(angle);

        // Set the position of the red circles
        redBigCircle.transform.position = new Vector3(x, y, 0);
        redSmallCircle.transform.position = redBigCircle.transform.position;

        // Set the position of the blue circles
        blueBigCircle.transform.position = new Vector3(x + separationMultiplier, y, 0);
        blueSmallCircle.transform.position = blueBigCircle.transform.position + blueSmallCircleOffset;

        // Handle blinking
        blinkTime += Time.deltaTime;
        if (blinkTime >= blinkInterval)
        {
            blueSmallCircle.SetActive(!blueSmallCircle.activeSelf);
            blinkTime = 0.0f;
        }
    }

    public void Separate()
    {
        separationMultiplier += separationSpeed;
    }

    public void Reverse()
    {
        separationMultiplier -= separationSpeed;
    }
}
