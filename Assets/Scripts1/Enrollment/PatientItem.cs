using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class PatientItem : MonoBehaviour
{
	public Text _name;
	public TextMeshProUGUI _number;
	[HideInInspector]
	public string _pname;

	public void SetPatientInfo(string name, int number)
	{
		_pname = name;
		_name.text = name;
		_number.text = number.ToString();
	}
}
