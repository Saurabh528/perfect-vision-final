using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class PatientItem : MonoBehaviour
{
	public Text _name;
	[HideInInspector]
	public PatientData _pdata;

	public void SetPatientData(PatientData pdata)
	{
		_pdata = pdata;
		_name.text = pdata.name;
	}
}
