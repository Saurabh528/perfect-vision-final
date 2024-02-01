using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObstacleMovement : MonoBehaviour {

    public float movementSpeed;

    private Rigidbody rb;

	void Start () {
        rb = GetComponent<Rigidbody>();
		float realspeed = movementSpeed * GamePlayController.GetDifficultyValue(1, 1, 10, 4);
        rb.AddForce(transform.forward * -realspeed);        //Moves the obstacle towards the player

        if (CompareTag("Cube Instance"))        //If the obstacle is cube
            transform.Rotate(Vector3.right, 90f);       //Then rotates it
        else if(CompareTag("Prism Instance"))       //If the obstacle is Prism
            transform.Rotate(Vector3.right, -90f);      //Then rotates it as well
	}
	
	void Update () {
        if (!GamePlayController.Instance.IsPlaying())       //Checks if the player collided with a collider which has different shape
            rb.Sleep();     //If it did, then stops the obstacle
	}
}
