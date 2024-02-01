using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EyesightTester : MonoBehaviour
{
    public float highlightTime = 1.0f;
    public float waitTime = 0.5f;
    private Image image;
    private RectTransform rectTransform;
    private Vector2[] positions;

    void Start()
    {
        image = GetComponent<Image>();
        rectTransform = GetComponent<RectTransform>();
        positions = new Vector2[]
        {
            new Vector2(-200, 200), // top left
            new Vector2(0, 200),    // top center
            new Vector2(200, 200),  // top right
            new Vector2(-200, 0),   // middle left
            new Vector2(0, 0),      // middle center
            new Vector2(200, 0),    // middle right
            new Vector2(-200, -200),// bottom left
            new Vector2(0, -200),   // bottom center
            new Vector2(200, -200), // bottom right
        };
        StartCoroutine(HighlightSquare());
    }

    IEnumerator HighlightSquare()
    {
        while (true)
        {
            // Choose a random position
            int index = Random.Range(0, positions.Length);
            Vector2 position = positions[index];

            // Set the square position and highlight it
            rectTransform.anchoredPosition = position;
            image.color = Color.white;

            // Wait for the highlight time
            yield return new WaitForSeconds(highlightTime);

            // Turn off the highlight and wait for the wait time
            image.color = Color.black;
            yield return new WaitForSeconds(waitTime);
        }
    }
}
