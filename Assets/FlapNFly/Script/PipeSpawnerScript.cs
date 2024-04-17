using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PipeSpawnerScript : MonoBehaviour
{
    public GameObject prefab;
    public float spawnRate = 1f;//rate of spawning the pipes
    public float minHeight = -1f;//where the top pipe will be placed
    public float maxHeight = 1f;//where the bottom pipe will be placed
                                // Start is called before the first frame update

    private void OnEnable()
    {
        InvokeRepeating(nameof(Spawn), spawnRate, spawnRate);
    }
    private void OnDisable()
    {
        CancelInvoke(nameof(Spawn));
    }

    void Spawn()
    {
        GameObject pipes = Instantiate(prefab, transform.position, Quaternion.identity);
        pipes.transform.position += Vector3.up * Random.Range(minHeight, maxHeight);//gives the position of new gameobject by adding vector3 to the offset.
                                                                                    // Define your colors with their hex values
        Color[] colors = new Color[]
        {
        HexToColor("FE8080"), // Convert hex to Color
        HexToColor("00FFFF")  // Convert hex to Color
        };

        // Access the PipeTop and PipeBottom children
        Transform pipeTop = pipes.transform.Find("PipeTop");
        Transform pipeBottom = pipes.transform.Find("PipeBottom");

        // Randomly select a color for PipeTop
        if (pipeTop != null)
        {
            SpriteRenderer topRenderer = pipeTop.GetComponent<SpriteRenderer>();
            if (topRenderer != null)
            {
                topRenderer.color = colors[Random.Range(0, colors.Length)];
            }
        }

        // Randomly select a color for PipeBottom (can be the same or different)
        if (pipeBottom != null)
        {
            SpriteRenderer bottomRenderer = pipeBottom.GetComponent<SpriteRenderer>();
            if (bottomRenderer != null)
            {
                bottomRenderer.color = colors[Random.Range(0, colors.Length)];
            }
        }
    }

    Color HexToColor(string hex)
    {
        byte r = byte.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber);
        byte g = byte.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber);
        byte b = byte.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber);
        return new Color32(r, g, b, 255); // Alpha set to 255 (fully opaque)
    }
}
