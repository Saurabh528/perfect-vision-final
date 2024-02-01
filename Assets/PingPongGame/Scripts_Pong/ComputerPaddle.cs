using UnityEngine;

public class ComputerPaddle : Paddle
{
    public Rigidbody2D ball;



    private void FixedUpdate()
    {
		if (!GamePlayController.Instance.IsPlaying())
			return;
        float realspeed = speed * GamePlayController.Instance.GetDifficultyValue();
        // Check if the ball is moving towards the paddle (positive x velocity)
        // or away from the paddle (negative x velocity)
        if (ball.velocity.x > 0f)
        {
            // Move the paddle in the direction of the ball to track it
            if (ball.position.y > rigidbody.position.y) {
                rigidbody.AddForce(Vector2.up * realspeed);
            } else if (ball.position.y < rigidbody.position.y) {
                rigidbody.AddForce(Vector2.down * realspeed);
            }
        }
        else
        {
            // Move towards the center of the field and idle there until the
            // ball starts coming towards the paddle again
            if (rigidbody.position.y > 0f) {
                rigidbody.AddForce(Vector2.down * realspeed);
            } else if (rigidbody.position.y < 0f) {
                rigidbody.AddForce(Vector2.up * realspeed);
            }
        }
    }


   

}
