using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class UISessionReportButton : MonoBehaviour
{
	[SerializeField] Text _textDate;
	[HideInInspector]public SessionRecord _ssRecord;
	public void SetSessionRecord(SessionRecord record)
	{
		if (record == null)
			return;
		_ssRecord = record;
		_textDate.text = _ssRecord.time.ToString("yyyy-MM-dd hh:mm");
	}
}
