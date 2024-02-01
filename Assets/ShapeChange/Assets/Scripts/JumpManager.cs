using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JumpManager : MonoBehaviour
{
    private Animation anim;
    private Transform playerTransform;

    void Start()
    {
        anim = GameObject.FindGameObjectWithTag("Player").GetComponent<Animation>(); //Initializes animation
        playerTransform = GameObject.FindGameObjectWithTag("Player").GetComponent<Transform>(); //Initializes transform
    }

    void Update()
    {
		
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            JumpLeft();
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            JumpRight();
        }
    }

    public void JumpRight()
    {
		if (!GamePlayController.Instance.IsPlaying())
			return;
		if (!anim.isPlaying) //If the animation is not playing at the moment
        {
            if (playerTransform.position.x == -3f) //If the player is on the first ground
                anim.Play("SecondRight"); //Then it jumps to the second ground

            else if (playerTransform.position.x == 0f) //If the player is on the second ground
                anim.Play("ThirdRight"); //Then it jumps to the third ground
        }
    }

    public void JumpLeft()
    {
		if (!GamePlayController.Instance.IsPlaying())
			return;
		if (!anim.isPlaying) //If the animation is not playing at the moment
        {
            if (playerTransform.position.x == 0f) //If the player is on the second ground
                anim.Play("FirstLeft"); //Then it jumps to the first ground

            else if (playerTransform.position.x == 3f) //If the player is on the third ground
                anim.Play("SecondLeft"); //Then it jumps to the second ground
        }
    }
}
