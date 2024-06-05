using UnityEngine;
using UnityEngine.UI;
public class AlignmentTableRow : MonoBehaviour
{
    [SerializeField] Text _dotPosition, _metric1, _metric1_, _metric2, _metric2_, _metric3, _metric3_, _metric4, _metric4_, _ipd, _ipd_;
	public static string[] _posLabel = { "Dextroelevation", "Elevation", "Levoelevation", "Dextroversion", "Primary Position", "Levoversion", "Dextrodepression", "Depression", "Levodepression"};
    public void SetData(float[] rowValues)
    {
		_dotPosition.text = _posLabel[(int)rowValues[0]];
		_metric1.text = rowValues[2].ToString("F2");
		_metric1_.text = rowValues[3].ToString("F2");
		_metric2.text = rowValues[4].ToString("F2");
		_metric2_.text = rowValues[5].ToString("F2");
		_metric3.text = rowValues[6].ToString("F2");
		_metric3_.text = rowValues[7].ToString("F2");
		_metric4.text = rowValues[8].ToString("F2");
		_metric4_.text = rowValues[9].ToString("F2");
		_ipd.text = rowValues[10].ToString("F2");
		_ipd_.text = rowValues[11].ToString("F2");
	}

	public static string GetDotPositionLabel(int index){
		if(index < 0 || index >= _posLabel.Length)
			return "";
		return _posLabel[index];
	}
}
