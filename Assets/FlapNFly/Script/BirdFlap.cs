using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BirdFlap : MonoBehaviour
{
    private Vector3 direction;
    public float gravity = -9.8f;//will be helping for the downfall of the bird
    public float strength = 5f;//by how much strength the bird would jump
    SpriteRenderer sr;//getting access to the sprite rendere component.
    public Sprite[] sprites;//collection of sprites
    private int spriteIndex;//will keep track of which sprite in sprites array will be used in current frame
    [SerializeField] CameraShake cameraShake;

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }
    void Start()
    {
        InvokeRepeating(nameof(Animate), 0.15f, 0.15f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W))
        {
            Fly();
        }

        direction.y += gravity * Time.deltaTime; //we wnat our bird to go down as we had made bird kinematic thus no gravity works on it.
        transform.position += direction * Time.deltaTime;//changing the direction of bird frame wise.
    }

    public void Fly(){
        direction = Vector3.up * strength;
    }

    public void Animate()
    {
        spriteIndex++;
        if (spriteIndex >= sprites.Length) // to make sure if it loops back
        {
            spriteIndex = 0;
        }
        sr.sprite = sprites[spriteIndex];
    }

    void OnTriggerEnter2D(Collider2D col)
    {
        if (col.gameObject.tag == "Obstacle" || col.gameObject.tag == "DeadZone")
        {
            if (GameState.currentGamePlay == null){
                GamePlayController.Instance.GameOver();
            }
            else
            {
                if(cameraShake != null)
                    cameraShake.Shake();
                GamePlayController.Instance.IncreaseScore(-3);
                if(col.gameObject.tag == "DeadZone")
                    Fly();
            }
        }
        else if (col.gameObject.tag == "Scoring")
        {
            GamePlayController.Instance.IncreaseScore();
        }
    }
}
