using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CoverStatisticResultRow : MonoBehaviour
{
    [SerializeField] Text _factor, _mean, _std, _min, _max, _median, _range;

    public void ShowValue(string factor, CoverStatisRowData data)
    {
        _factor.text = factor;
        _mean.text = string.Format("{0,12:F3}", data.mean);
		_std.text = string.Format("{0,12:F3}", data.std);
		_min.text = string.Format("{0,12:F3}", data.min);
		_max.text = string.Format("{0,12:F3}", data.max);
		_median.text = string.Format("{0,12:F3}", data.median);
		_range.text = string.Format("{0,12:F3}", data.range);
	}
}
