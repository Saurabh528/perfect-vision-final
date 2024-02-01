using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FruitSpawner : MonoBehaviour
{
    public GameObject[] RespawningObject;    //Array to store random generating objects
    public float spawnRate = 1f;
    float nextSpawn = 0f;
    public static bool check;               //Remember - It will not be displayed in inspector window as static. It has been trigger to "true" from different script
    public float MinimumX, MinimumY, MaximumX, MaximumY;   //Spawn Area

    //private GameObject[] fruits;  // We made array of fruits 
    private BoxCollider2D col;
    
    float x1, x2;

    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();

        x1 = transform.position.x - col.bounds.size.x / 2f;
        x2 = transform.position.x + col.bounds.size.x / 2f;
    }


        



    // Use this for initialization
    void Start()
    {//Setting true so that condition get true one time and display random object
        //if not set to true then it will not generate random object as check value will be false
        check = true;

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextSpawn && check == true)
        {

            Vector3 temp = new Vector3();



            temp.x = Random.Range(MinimumX, MaximumX);
            temp.y = Random.Range(MinimumY, MaximumY);
            //Checking for prev generated random point with new, so that new is not generated in the area of 10 unit from previous one
            while (((temp.x <= GameObject.FindGameObjectWithTag("Player").transform.position.x + 20) && (temp.x >= GameObject.FindGameObjectWithTag("Player").transform.position.x - 20)))
            {
                temp.x = Random.Range(MinimumX, MaximumX);
                temp.y = Random.Range(MinimumY, MaximumY);

            }



            //Instantiating prefab
            Instantiate(RespawningObject[Random.Range(0, RespawningObject.Length)], temp, Quaternion.identity);

            check = true;   //triggering check to false so that next object will only be genrated once someone enable it from other script
            nextSpawn = Time.time + spawnRate;

        }
    }


}

