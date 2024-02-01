using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBounds : MonoBehaviour
{
    private float minX, maxX;


    // Start is called before the first frame update
    void Start()
    {
        Vector3 coor = Camera.main.ScreenToWorldPoint(new Vector3(Screen.width, Screen.height, 0)); // it convert the unity Screen corrdinate to the unity world points coordinate

        //minX = - 2.1f; // so it will bound at edges of screen 
        //maxX = 2.1f;   // We hardcore also the sma estuff after looking into axis

        minX = -coor.x + 0.3f;
        maxX = coor.x - 0.3f;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 temp = transform.position;

        if (temp.x > maxX)

            temp.x = maxX;

        if (temp.x < minX)

            temp.x = minX;
        transform.position = temp;

    }
}
