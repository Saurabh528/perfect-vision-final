using UnityEngine;

public class PlaneMovement : MonoBehaviour
{
    public float speed = 2f; // Speed of plane movement
    public float changeDirectionInterval = 2f; // Time interval to change plane direction
    public float maxRotationAngle = 45f; // Maximum angle of rotation when changing direction

    private Vector2 moveDirection; // Current direction of plane movement
    private float timeSinceDirectionChange = 0f; // Time elapsed since last direction change

    // Start is called before the first frame update
    void Start()
    {
        // Set a random initial direction for the plane to move towards
        moveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;
        transform.right = moveDirection; // Set initial rotation of plane towards move direction
    }

    // Update is called once per frame
    void Update()
    {
        // Move the plane in its current direction
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        // Check if it's time to change the plane's direction
        timeSinceDirectionChange += Time.deltaTime;
        if (timeSinceDirectionChange >= changeDirectionInterval)
        {
            // Set a new random direction for the plane to move towards
            moveDirection = new Vector2(Random.Range(-1f, 1f), Random.Range(-1f, 1f)).normalized;

            // Calculate the angle between the current direction and the new direction
            float angle = Vector2.Angle(transform.right, moveDirection);

            // Clamp the angle to the maximum rotation angle
            angle = Mathf.Clamp(angle, 0f, maxRotationAngle);

            // Determine the sign of the angle using the cross product
            float sign = Mathf.Sign(Vector3.Cross(transform.right, moveDirection).z);

            // Rotate the plane towards the new direction with the clamped angle and sign
            transform.Rotate(0f, 0f, angle * sign);

            // Reset the direction change timer
            timeSinceDirectionChange = 0f;
        }
    }
}
