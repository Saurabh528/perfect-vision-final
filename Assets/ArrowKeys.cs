using UnityEngine;
using UnityEngine.UI;

public class ArrowKeys : MonoBehaviour
{
    public Text upText;
    public Text downText;
    public Text leftText;
    public Text rightText;
    private EyesightTester eyesightTester;
    private Vector2[] directions = new Vector2[]
    {
        Vector2.up,
        Vector2.down,
        Vector2.left,
        Vector2.right,
    };

    void Start()
    {
        eyesightTester = FindObjectOfType<EyesightTester>();
    }
    void Update()
    {
        Debug.Log("Update called");

        if (eyesightTester && eyesightTester.GetComponent<Image>().color == Color.white)
        {
            // Get the direction of the highlighted square
            Vector2 squareDirection = (eyesightTester.transform.position - transform.position).normalized;
            Debug.Log("Highlighted square direction: " + squareDirection);

            // Check if the correct arrow key was pressed
            for (int i = 0; i < directions.Length; i++)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow) && Vector2.Dot(directions[i], squareDirection) > 0.9f)
                {
                    upText.color = Color.green;
                    break;
                }
                else
                {
                    upText.color = Color.white;
                }

                if (Input.GetKeyDown(KeyCode.DownArrow) && Vector2.Dot(directions[i], squareDirection) > 0.9f)
                {
                    downText.color = Color.green;
                    break;
                }
                else
                {
                    downText.color = Color.white;
                }

                if (Input.GetKeyDown(KeyCode.LeftArrow) && Vector2.Dot(directions[i], squareDirection) < -0.9f)
                {
                    leftText.color = Color.green;
                    break;
                }
                else
                {
                    leftText.color = Color.white;
                }

                if (Input.GetKeyDown(KeyCode.RightArrow) && Vector2.Dot(directions[i], squareDirection) > 0.9f)
                {
                    rightText.color = Color.green;
                    break;
                }
                else
                {
                    rightText.color = Color.white;
                }
            }
        }
        else
        {
            // Turn off all the arrow key highlights
            upText.color = Color.white;
            downText.color = Color.white;
            leftText.color = Color.white;
            rightText.color = Color.white;
        }
    }



}
