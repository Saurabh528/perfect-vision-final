using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Ballon_PlayerMovement : MonoBehaviour
{

    private Rigidbody2D myBody;
    public static Vector3 pos;

    private void Awake()   // call before the start 
    {
        myBody = GetComponent<Rigidbody2D>();
      // tinker_u_sound.play_state = 1;
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // FixedUpdate is called every 2-3 seconds frame
    void FixedUpdate()
    {

        
        //Vector2 vel = myBody.velocity;
        //vel.x = Input.GetAxis("Horizontal") * speed;
        //myBody.velocity = vel;       
        


        //pos.x = (tinker_u_sound.pos2) - 38.5f;
      //  pos.y = (tinker_u_sound.pos1) - 16f;
        pos.z = 0;
      //  transform.position = tinker_u_sound.final_position; //Debug.Log(pos); 

    }
}
