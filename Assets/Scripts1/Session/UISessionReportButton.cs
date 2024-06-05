using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UISessionReportButton : MonoBehaviour
{
	[SerializeField] Text _textDate;
	public void SetDateStr(String datestr)
	{
		_textDate.text =datestr;
	}
}
