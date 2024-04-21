using UnityEngine;
using UnityEngine.UI;
using System.IO;

public class CSVGridDisplay : MonoBehaviour
{
    public GameObject gridPanel;
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
                Text cellText = newText.GetComponent<Text>();
                float value;
                // Check if the cell content is a number
                if (float.TryParse(cell.Trim(), out value))
                {
                    // If it's a number, format it to two decimal places
                    cellText.text = value.ToString("F2");
                }
                else
                {
                    // If it's not a number, display as is
                    cellText.text = cell.Trim();
                }
            }
        }
    }
}
