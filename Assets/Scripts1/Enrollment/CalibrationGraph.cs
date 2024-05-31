using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CalibrationGraph : MonoBehaviour
{
    [SerializeField] CalibInfoUI _infoSlotTmpl;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateItems(Dictionary<DateTime, UInt32> colorlist)
    {
        Canvas.ForceUpdateCanvases();
		float width = transform.parent.GetComponent<RectTransform>().rect.width;
        float slotwidth = width / colorlist.Count;
        int count = 0;
        foreach(KeyValuePair<DateTime, UInt32> pair in colorlist)
        {
			GameObject newobj = Instantiate(_infoSlotTmpl.gameObject, _infoSlotTmpl.transform.position, _infoSlotTmpl.transform.rotation);
            newobj.SetActive(true);
            CalibInfoUI calibInfoUI = newobj.GetComponent<CalibInfoUI>();
            RectTransform rt = calibInfoUI.GetComponent<RectTransform>();
			RectTransform rtsrc = _infoSlotTmpl.GetComponent<RectTransform>();
			rt.SetParent(rtsrc.parent);
            UtilityFunc.CopyRectTransform(rtsrc, rt);
            rt.Translate(new Vector3(slotwidth * count, 0, 0));
            //rt.localPosition = rtsrc.localPosition + new Vector3(slotwidth * count, 0, 0);
			//rt.offsetMax = new Vector3(rtsrc.offsetMin.x + slotwidth * (count + 1), rtsrc.offsetMax.y);
			calibInfoUI.name = pair.Key.ToString();
            calibInfoUI.SetInfo(pair.Key, pair.Value);
            count++;
		}
    }
}
