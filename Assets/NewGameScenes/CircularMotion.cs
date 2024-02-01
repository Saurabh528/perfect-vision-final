using UnityEngine;

public class CircularMotion : MonoBehaviour
{

    
        public float speed = 1.0f; // Speed of the circular motion
        public float radius = 5.0f; // Radius of the circular path

        private Vector2 center;
        private float angle;

        void Start()
        {
            // Set the center of the circular path
            center = transform.position;
        }

        void Update()
        {
		if (!GamePlayController.Instance.IsPlaying())
			return;
		// Increment the angle
		float realspeed = speed * GamePlayController.GetDifficultyValue(1, 1, 10, 2);
            angle += realspeed * Time.deltaTime;

            // Calculate the new position
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            Vector2 newPosition = new Vector2(x, y);

            // Update the sprite's position
            transform.position = center + newPosition;
        }
    }    
