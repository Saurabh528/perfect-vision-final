using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimilarityTableRow : MonoBehaviour
{
    [SerializeField] Text _txtNo, _txtSim, _txtDuration, _txtMean;
    // Start is called before the first frame update
    public void SetData(int index, string sim, string duration, float meanVal = 0)
    {
		_txtNo.text = index.ToString();
        _txtSim.text = sim;
        _txtDuration.text = duration;
		_txtMean.text = string.Format("{0, 5:F2}", meanVal);
	}
}
