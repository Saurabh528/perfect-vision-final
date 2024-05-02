using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipesMovement : MonoBehaviour
{
    public float speed = 5f;
    public float leftEdge;
    // Start is called before the first frame update
    void Start()
    {
        leftEdge = Camera.main.ScreenToWorldPoint(Vector3.zero).x - 20f;
    }

    // Update is called once per frame
    void Update()
    {
        float levelSpeed = GamePlayController.Instance == null?speed:(speed * GamePlayController.GetDifficultyValue(1, 1, 30, 3));
        transform.position += Vector3.left * levelSpeed * Time.deltaTime;
        if (transform.position.x < leftEdge)
        {
            Destroy(gameObject);
        }
    }
}
