using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class UIDisplaceView : MonoBehaviour
{
    [SerializeField] DisplacementGraph _graph;
	[SerializeField] Text _txtTitle;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowWithTwoColumn()
    {
		Debug.Log("ShowWithTwoColumn called");
        if(GameState.currentPatient == null)
        {
            
			gameObject.SetActive(false);
            return;
        }
		string path = PatientMgr.GetPatientDataDir() + "/displacement.csv";
        if (!File.Exists(path))
        {
			EnrollmentManager.Instance.ShowMessage("Displacement record does not exist.");
			gameObject.SetActive(false);
			return;
		}
        string[] str = File.ReadAllLines(path);
        if(str.Length < 10) {
			gameObject.SetActive(false);
			return;
		}
		gameObject.SetActive(true);
        List<float> valuelist = new List<float>();
        for(int i = 1; i < str.Length; i++)
        {
            string[] splitstrs = str[i].Split(new char[] { ',' });
            valuelist.Add(float.Parse(splitstrs[2]) / float.Parse(splitstrs[1]));
        }
        _graph.DrawDisplacementData(valuelist, Color.black);
	}

	public void ShowWithOneColumnCSV(string title, string pathname)
	{
		Debug.Log("ShowWithOneColumnCSV called");
		if (!File.Exists(pathname))
		{
			gameObject.SetActive(false);
			return;
		}
		string[] str = File.ReadAllLines(pathname);
		if (str.Length < 10)
		{
			gameObject.SetActive(false);
			return;
		}

		_txtTitle.text = title;
		gameObject.SetActive(true);
		List<float> valuelist = new List<float>();
		for (int i = 1; i < str.Length; i++)
		{
			string[] splitstrs = str[i].Split(new char[] { ',' });
			valuelist.Add(float.Parse(splitstrs[1]));
		}
		_graph.DrawDisplacementData(valuelist, Color.black);
	}
}
