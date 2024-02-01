//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class CubeScript : MonoBehaviour
//{
//    public Vector3 position;
//    public Color color;


//    // Start is called before the first frame update
//    void Start()
//    {

//    }

//    void Update()
//    {
//        transform.position = position;
//        GetComponent<Renderer>().material.color = new Color(0,0.17f,1,0);
//    }
//}


using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeScript : MonoBehaviour
{
    public Vector3 position;

    private float colorChangeInterval = 3f; // Time in seconds before color changes
    private float colorChangeTimer = 0f;

    // Start is called before the first frame update
    void Start()
    {
        // Set the initial color of the cube to blue
        GetComponent<Renderer>().material.color = Color.blue;
    }

    void Update()
    {
        // Move the cube to its assigned position
        transform.position = position;

        // Check if it's time to change color
        colorChangeTimer += Time.deltaTime;
        if (colorChangeTimer >= colorChangeInterval)
        {
            // Change the color randomly between red and blue
            Color newColor = Random.Range(0, 2) == 0 ? Color.red : Color.blue;
            GetComponent<Renderer>().material.color = newColor;

            // Reset the timer
            colorChangeTimer = 0f;
        }
    }
}
