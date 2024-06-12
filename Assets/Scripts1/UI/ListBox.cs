using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ListBox : MonoBehaviour
{
	[SerializeField] GameObject _itemTmpl;
	public UnityEngine.Events.UnityEvent<string> OnItemActivated;
	[SerializeField] bool DisableOnMouseClick;
	List<Toggle> items = new List<Toggle>();
	int _highlightIndex;
	public void Add(string text)
	{
		GameObject newobj = (GameObject)Instantiate(_itemTmpl, _itemTmpl.transform.position, _itemTmpl.transform.rotation);
		newobj.name = text;
		newobj.transform.SetParent(_itemTmpl.transform.parent);
		newobj.transform.localScale = _itemTmpl.transform.localScale;
		newobj.SetActive(true);
		TextMeshProUGUI textMeshProUGUI = newobj.GetComponentInChildren<TextMeshProUGUI>();
		textMeshProUGUI.text = text;
		items.Add(newobj.GetComponent<Toggle>());
	}

	public void Clear()
	{
		UtilityFunc.DeleteAllSideTransforms(_itemTmpl.transform, false);
		items.Clear();
	}

	public void OnItemChanged(bool value)
	{
		if (value)
		{
			Toggle[] toggles = _itemTmpl.transform.parent.GetComponentsInChildren<Toggle>();
			foreach (Toggle toggle in toggles)
			{
				if (toggle.isOn)
				{
					if (OnItemActivated != null)
						OnItemActivated.Invoke(toggle.name);

				}
			}
		}
	}

	public void SetHighlight(int index)
	{
		if (index >= items.Count)
			return;
		_highlightIndex = index;
		for(int i = 0; i < items.Count; i++)
		{
			if (i == index)
				items[i].targetGraphic.color = items[i].colors.highlightedColor;
			else
				items[i].targetGraphic.color = items[i].colors.normalColor;
		}
	}

	private void Update()
	{
		if(DisableOnMouseClick && Input.GetMouseButtonUp(0))
			gameObject.SetActive(false);
		else if(items.Count > 0)
		{
			if (Input.GetKeyDown(KeyCode.UpArrow))
			{
				_highlightIndex = (_highlightIndex - 1) % items.Count;
				SetHighlight(_highlightIndex);
				
			}
			else if (Input.GetKeyDown(KeyCode.DownArrow))
			{
				_highlightIndex = (_highlightIndex + 1) % items.Count;
				SetHighlight(_highlightIndex);
			}
			else if (OnItemActivated != null && Input.GetKeyDown(KeyCode.Return) && _highlightIndex < items.Count)
			{
				Input.ResetInputAxes();
				OnItemActivated.Invoke(items[_highlightIndex].name);
			}
		}
		
		
	}

	public List<Toggle> GetItems() { return items; }
}
