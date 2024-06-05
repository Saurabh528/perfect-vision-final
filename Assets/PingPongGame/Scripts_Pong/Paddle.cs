using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Paddle : MonoBehaviour
{
    public static float speed = 4f;
    [HideInInspector]
    public Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void ResetPosition()
    {
        rigid.velocity = Vector2.zero;
        rigid.position = new Vector2(rigid.position.x, 0f);
    }

}
