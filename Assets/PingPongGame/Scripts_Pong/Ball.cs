using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class Ball : MonoBehaviour
{
    public float speed = 180;
    [HideInInspector]
    public Rigidbody2D rigid;

  

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }

    public void ResetPosition()
    {
        rigid.velocity = Vector2.zero;
        rigid.position = Vector2.zero;
    }

    public void AddStartingForce()
    {
        // Flip a coin to determine if the ball starts left or right
        float x = Random.value < 0.5f ? -1f : 1f;

        // Flip a coin to determine if the ball goes up or down. Set the range
        // between 0.5 -> 1.0 to ensure it does not move completely horizontal.
        float y = Random.value < 0.5f ? Random.Range(-1f, -0.5f)
                                      : Random.Range(0.5f, 1f);

        Vector2 direction = new Vector2(x, y);
        rigid.AddForce(direction * speed * GamePlayController.Instance.GetDifficultyValue());
    }

	public void SetPlaingState(bool state)
	{
		Rigidbody2D rigid = GetComponent<Rigidbody2D>();
		rigid.constraints = RigidbodyConstraints2D.FreezeRotation | (state ? RigidbodyConstraints2D.None : RigidbodyConstraints2D.FreezePosition);
	}



}
