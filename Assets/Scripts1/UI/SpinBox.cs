using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class SpinBox : MonoBehaviour
{
    TMP_InputField _inputTMP;
    [SerializeField] int _min, _max;
    int value;
    // Start is called before the first frame update
    void Start()
    {
		_inputTMP = GetComponent<TMP_InputField>();
		if (_inputTMP)
			value = int.Parse(_inputTMP.text);

	}

    public void Increase(int delta)
    {
		value = Mathf.Clamp(value + delta, _min, _max);
        if(_inputTMP)
            _inputTMP.text = value.ToString();
	}

    public void OnValueChanged(string str)
    {
        try
        {
            value = int.Parse(str);
        }
        catch { }
    }
}
