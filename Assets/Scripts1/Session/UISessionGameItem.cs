using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
public class UISessionGameItem : MonoBehaviour
{
	public TextMeshProUGUI nameGUI;
	public Button btnCancel;

	void Start(){
		if(GameState.IsPatient() && btnCancel)
			btnCancel.enabled = false;
	}

	public void SetGaneName(string name)
	{
		nameGUI.text = name;
	}

	public string GetGameName()
	{
		return nameGUI.text;
	}
}
