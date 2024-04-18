using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CSVGridDisplay : MonoBehaviour
{
    // The panel that will hold the grid of text
    public GameObject gridPanel;

    // The text prefab that will be used for each cell
    public GameObject textPrefab;

    private string filePath;

    void Start()
    {
        filePath = Path.Combine(Application.dataPath, "final_result.csv");
        DisplayCsvData(filePath);
    }

    void DisplayCsvData(string file)
    {
        if (!File.Exists(file))
        {
            Debug.LogError("File not found: " + file);
            return;
        }

        string[] lines = File.ReadAllLines(file);

        // Calculate the number of columns by looking at the first row
        int columns = lines[0].Split(',').Length;

        GridLayoutGroup gridLayout = gridPanel.GetComponent<GridLayoutGroup>();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = columns;

        foreach (string line in lines)
        {
            string[] cells = line.Split(',');

            foreach (string cell in cells)
            {
                GameObject newText = Instantiate(textPrefab, gridPanel.transform);
                newText.GetComponent<Text>().text = cell.Trim();
            }
        }
    }
}
