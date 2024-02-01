using UnityEngine;
using UnityEngine.UI;

public class CircleController : MonoBehaviour
{
    public float distance = 2f; // initial distance between circles
    public float speed = 1f; // initial rotation speed
    public bool isClockwise = true; // initial rotation direction
    public Button distanceButton; // reference to distance button
    public Button speedButton; // reference to speed button
    public Button directionButton; // reference to direction button

    public Button DecDistance;
         

    private Transform circle1; // reference to first circle
    private Transform circle2; // reference to second circle
    private float angle; // current rotation angle

    void Start()
    {
        // get references to circle transforms
        circle1 = transform.GetChild(0);
        circle2 = transform.GetChild(1);

        // add click listeners to buttons
        distanceButton.onClick.AddListener(IncreaseDistance);
        speedButton.onClick.AddListener(IncreaseSpeed);
        directionButton.onClick.AddListener(ToggleDirection);
        DecDistance.onClick.AddListener(DecreaseDistance);

    }

    void Update()
    {
        // calculate new rotation angle based on speed and direction
        float deltaAngle = (isClockwise ? 1 : -1) * speed * Time.deltaTime;
        angle += deltaAngle;

        // calculate new positions of circles based on distance and angle
        Vector3 position1 = new Vector3(distance * Mathf.Cos(angle), distance * Mathf.Sin(angle), 0);
        Vector3 position2 = new Vector3(-distance * Mathf.Cos(angle), -distance * Mathf.Sin(angle), 0);

        // update circle positions
        circle1.localPosition = position1;
        circle2.localPosition = position2;


        // increment angle by 1 degree per frame
       // angle += 1f * Mathf.Deg2Rad * Time.deltaTime;
    }

    void IncreaseDistance()
    {
        distance += 0.01f; // increase distance by 0.1 units
    }

    void IncreaseSpeed()
    {
        speed += 0.1f; // increase speed by 0.1 units
    }

    void DecreaseDistance()
    {
        distance -= 0.01f; // decrease distance by 0.1 units
    }

    void ToggleDirection()
    {
        isClockwise = !isClockwise; // toggle rotation direction
    }
}
