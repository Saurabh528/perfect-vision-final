using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class Fruit_Bomb : MonoBehaviour
{

    // public GameObject Apple, Grapes, Mango, Orange, Banana;
    //In place of above line use below line, it will save your time
    public GameObject[] Fruits;
    public float spawnRate = 3f;
    float nextSpawn = 0f;
    private BoxCollider2D col;
    float x1, x2;

    int whatToSpawn;
    private void Awake()
    {
        col = GetComponent<BoxCollider2D>();

        x1 = transform.position.x - col.bounds.size.x / 2f;
        x2 = transform.position.x + col.bounds.size.x / 2f;
    }

    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > nextSpawn)
        {

            // Vector3 temp = transform.position;
            Vector3 temp = transform.position;

            temp.x = Random.Range(x1, x2);
            //temp.x = Random.Range(-10, 10);
            //  temp.y = Random.Range(-5, 5);
            Instantiate(Fruits[Random.Range(0, Fruits.Length)], temp, Quaternion.identity);
            //  PlayerMovement.check = false;
            nextSpawn = Time.time + spawnRate;

        }


    }




}