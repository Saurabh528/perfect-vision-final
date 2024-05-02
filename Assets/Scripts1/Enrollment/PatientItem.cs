using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PatientItem : MonoBehaviour
{
	public Text _name;
	[HideInInspector]
	public string _pname;

	public void SetPatientName(string name)
	{
		_pname = name;
		_name.text = name;
	}
}
