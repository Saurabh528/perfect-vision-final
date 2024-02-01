using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class CalibrationSliderProgressView : MonoBehaviour
{
	[SerializeField] CreateGraph graph;
	[SerializeField] Text _title;
	float width, height;
	float heightPerV;
	const float rulerSize = 20;
	double period;
	DateTime startTime;
	// Update is called once per frame
	void Update()
    {
        
    }

	public void Clear()
	{
		UtilityFunc.DeleteAllChildTransforms(graph.transform);
	}
	public void Draw(ColorChannel channel)
    {
        Dictionary<DateTime, uint> valuelist = UISessionRecordView.GetSessionColorList(channel);

		_title.text = UtilityFunc.ColorChannelToName(channel) + " Slider RGB Values Over Time";
		if (valuelist.Count !=  0)
			DrawAnylysData(valuelist);
    }

	public void DrawAnylysData(Dictionary<DateTime, uint> timeColorlist)
	{
		startTime = timeColorlist.First().Key;
		RectTransform rt = graph.GetComponent<RectTransform>();
		width = rt.rect.width;
		height = rt.rect.height;
		UtilityFunc.DeleteAllChildTransforms(graph.transform);
		graph.SetWidth(5);
		graph.SetColor(Color.black);
		DrawBoundRect();
		graph.SetWidth(2);
		period = (timeColorlist.Last().Key - timeColorlist.First().Key).TotalSeconds;
		DrawAxis(timeColorlist);
		graph.SetWidth(5);
		DrawGraph(timeColorlist);
	}

	void DrawAxis(Dictionary<DateTime, uint> timeValuelist)
	{
		graph.SetFontSize(30);
		graph.TextOut("0", -rulerSize, -rulerSize, TextAnchor.LowerRight, FontStyle.Bold);
		DrawHorScale(timeValuelist);

		DrawVerticalScale(true);

	}

	void DrawHorScale(Dictionary<DateTime, uint> timeValuelist)
	{

		int maxCount = 8;
		Dictionary<DateTime, uint> reducedList = new Dictionary<DateTime, uint>();
		KeyValuePair<DateTime, uint> firstpair = timeValuelist.First();
		DateTime prevTime = firstpair.Key;
		reducedList.Add(firstpair.Key, firstpair.Value);
		foreach(KeyValuePair<DateTime, uint> pair in timeValuelist)
		{
			if((pair.Key - prevTime).TotalSeconds > period / (maxCount - 1))
			{
				reducedList.Add(pair.Key, pair.Value);
				prevTime = pair.Key;
			}
		}

		foreach(KeyValuePair<DateTime, uint> pair in reducedList)
		{
			float x = period == 0?0: (float)(width * (pair.Key - startTime).TotalSeconds / period);
			if(pair.Key != timeValuelist.First().Key || pair.Key != timeValuelist.Last().Key)
			{
				graph.MoveTo(x, height);
				graph.LineTo(x, -rulerSize);
			}
			
			graph.SetColor(Color.black);
			graph.TextOut(pair.Key.ToString("MMM d yy"), x, -rulerSize - 5, TextAnchor.UpperCenter, FontStyle.Bold);
		}
	}

	void DrawBoundRect()
	{
		graph.MoveTo(0, 0);
		graph.LineTo(width, 0);
		graph.LineTo(width, height);
		graph.LineTo(0, height);
		graph.LineTo(0, 0);
	}

	void DrawVerticalScale(bool showgrid = false)
	{
		int verstepCount = 5;
		float valueStep = 50;
		heightPerV = height / 270;
		for (int i = 1; i <= verstepCount; i++)
		{
			if (showgrid)
			{
				if (i != verstepCount)
				{
					graph.MoveTo(width, heightPerV * i * valueStep);
					graph.LineTo(-rulerSize, heightPerV * i * valueStep);
				}
				else
				{
					graph.MoveTo(width, heightPerV * 255);
					graph.LineTo(-rulerSize, heightPerV * 255);
				}
			}
			else
			{
				if (i != verstepCount)
				{
					graph.MoveTo(0, heightPerV * i * valueStep);
					graph.LineTo(-rulerSize, heightPerV * i * valueStep);
				}
				else
				{
					graph.MoveTo(0, heightPerV * 255);
					graph.LineTo(-rulerSize, heightPerV * 255);
				}
					
			}

		}
		for (int i = 1; i < verstepCount; i++)
		{
			graph.TextOut(((int)(valueStep * i)).ToString(), -rulerSize - 5, heightPerV * i * valueStep, TextAnchor.MiddleRight, FontStyle.Bold);
		}
		graph.TextOut("255", -rulerSize - 5, heightPerV * 255, TextAnchor.MiddleRight, FontStyle.Bold);
	}

	void DrawGraph(Dictionary<DateTime, uint> timeValuelist)
	{
		int count = 0;
		graph.SetColor(Color.red);
		foreach (KeyValuePair<DateTime, uint> pair in timeValuelist)
		{
			float value = (pair.Value >> 24);
			float x = (float)(width * (pair.Key - startTime).TotalSeconds / period);
			if (count == 0)
				graph.MoveTo(x, heightPerV * value);
			else
				graph.LineTo(x, heightPerV * value);
			count++;
		}

		count = 0;
		graph.SetColor(Color.green);
		foreach (KeyValuePair<DateTime, uint> pair in timeValuelist)
		{
			float value = (pair.Value >> 16) & 0xff;
			float x = (float)(width * (pair.Key - startTime).TotalSeconds / period);
			if (count == 0)
				graph.MoveTo(x, heightPerV * value);
			else
				graph.LineTo(x, heightPerV * value);
			count++;
		}

		count = 0;
		graph.SetColor(Color.blue);
		foreach (KeyValuePair<DateTime, uint> pair in timeValuelist)
		{
			float value = (pair.Value >> 8) & 0xff;
			float x = (float)(width * (pair.Key - startTime).TotalSeconds / period);
			if (count == 0)
				graph.MoveTo(x, heightPerV * value);
			else
				graph.LineTo(x, heightPerV * value);
			count++;
		}
	}

	public void OnCalibrationProgressionChannelChange(Int32 index)
	{
		if (index == 0)
			Draw(ColorChannel.CC_Red);
		else if (index == 1)
			Draw(ColorChannel.CC_Cyan);
		else if (index == 2)
			Draw(ColorChannel.CC_Background);
	}
}
