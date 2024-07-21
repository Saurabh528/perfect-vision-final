using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ColorCalibrationProgressView : MonoBehaviour
{
    [SerializeField] Text _textTitle;
    [SerializeField] CalibrationGraph _graph;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void CreateView(ColorChannel channel)
    {
        if(channel == ColorChannel.CC_Red)
        {
            _textTitle.text = "Red";
			_textTitle.color = Color.red;

		}
		else if (channel == ColorChannel.CC_Cyan)
		{
			_textTitle.text = "Blue";
			_textTitle.color = Color.blue;
		}
		else if (channel == ColorChannel.CC_Background)
		{
			_textTitle.text = "Background";
			_textTitle.color = Color.white;
		}
		Dictionary<DateTime, UInt32> colorlist = UISessionRecordView.GetSessionDiagnosticsColorList(channel);
        _graph.CreateItems(colorlist);
    }
}
