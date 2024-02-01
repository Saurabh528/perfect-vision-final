using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CalibInfoUI : MonoBehaviour
{
    [SerializeField] Text _textColor, _textTime;
    [SerializeField] Image _imageSlot;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetInfo(DateTime datetime, UInt32 color)
    {
        byte[] bytes = BitConverter.GetBytes(color);
        _textColor.text = $"<color=red>{bytes[3]}</color>\r\n<color=green>{bytes[2]}</color>\r\n<color=blue>{bytes[1]}</color>\r\n{bytes[0]}";//$"{bytes[3]}\n{bytes[2]}\n{bytes[1]}\n{bytes[0]}";
        _textTime.text = datetime.ToString("MMM d yy");
        _imageSlot.color = new Color32(bytes[3], bytes[2], bytes[1], bytes[0]);
	}
}
