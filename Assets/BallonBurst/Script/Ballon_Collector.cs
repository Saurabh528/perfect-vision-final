using UnityEngine;
using System.Collections;

public class Ballon_Collector : MonoBehaviour
{

    void OnTriggerEnter2D(Collider2D target)
    {
        if (target.tag == "Bomb" || target.tag == "Fruits")
        {
            target.gameObject.SetActive(false);
        }
        FruitSpawner.check = true;
    }

} // class
