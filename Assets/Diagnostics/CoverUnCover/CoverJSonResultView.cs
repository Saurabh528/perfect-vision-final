using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using Newtonsoft.Json;

public class CoverStatisRowData
{
    public float mean, std, min, max, median, range;
}


public class CoverJSonResultView : MonoBehaviour
{
    [SerializeField] Text _textTitle;
    [SerializeField] CoverStatisticResultRow _rowTmpl;
	// Start is called before the first frame update
	void Start()
    {
        
    }

    public void ShowJSonContent(string title, string filepathname)
    {
		_textTitle.text = title;
        string text = File.ReadAllText(filepathname);
		Dictionary<string, CoverStatisRowData> dic = JsonConvert.DeserializeObject<Dictionary<string, CoverStatisRowData>>(text);
        foreach(KeyValuePair<string, CoverStatisRowData> pair in dic)
        {
            GameObject go = (GameObject)Instantiate(_rowTmpl.gameObject, gameObject.transform.position, _rowTmpl.transform.rotation);
            go.SetActive(true);
            go.transform.SetParent(_rowTmpl.transform.parent);
            go.transform.localScale = _rowTmpl.transform.localScale;
			CoverStatisticResultRow resultRow = go.GetComponent<CoverStatisticResultRow>();
            if (resultRow)
            {
				resultRow.ShowValue(pair.Key, pair.Value);

			}
		}
	}
}
