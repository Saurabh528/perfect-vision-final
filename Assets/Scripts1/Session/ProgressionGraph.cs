using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(CreateGraph))]
public class ProgressionGraph : MonoBehaviour
{
	CreateGraph graph;
	float width, height;
	float widthPerS, heightPerV;
	const float rulerSize = 20;
  

	public void DrawAnylysData(Dictionary<float, float> timeValuelist, Color color)
	{
		if (timeValuelist.Count < 1)
			return;
		RectTransform rt = GetComponent<RectTransform>();
		width = rt.rect.width;
		height = rt.rect.height;
		graph = GetComponent<CreateGraph>();
		float maxTime = -1, maxValue = -1;
		foreach(KeyValuePair<float, float> pair in timeValuelist)
		{
			if (maxTime < pair.Key)
				maxTime = pair.Key;
			if (maxValue < pair.Value)
				maxValue = pair.Value;
		}
		graph.SetWidth(5);
		graph.SetColor(Color.black);
		DrawBoundRect();
		DrawAxis(maxTime, maxValue);
		graph.SetWidth(10);
		graph.SetColor(color);
		DrawGraph(timeValuelist);
	}

	public void DrawAnylysData(Dictionary<DateTime, float> timeValuelist, Color color)
	{
		RectTransform rt = GetComponent<RectTransform>();
		width = rt.rect.width;
		height = rt.rect.height;
		graph = GetComponent<CreateGraph>();
		graph.SetWidth(5);
		graph.SetColor(Color.black);
		DrawBoundRect();
		/*if (timeValuelist.Count < 2)
			return;*/
		graph.SetWidth(2);
		DrawAxis(timeValuelist);
		graph.SetWidth(5);
		graph.SetColor(color);
		DrawGraph(timeValuelist);
	}

	void DrawAxis(Dictionary<DateTime, float> timeValuelist)
	{
		graph.SetFontSize(30);
		graph.TextOut("0", -rulerSize, -rulerSize, TextAnchor.LowerRight, FontStyle.Bold);
		int horstepCount = Mathf.Min(8, timeValuelist.Count - 1);
        widthPerS = horstepCount == 0 ? width : width / horstepCount;
        for (int i = 0; i <= horstepCount; i++)
		{
			graph.SetColor(new Color(0.7f, 0.7f, 0.7f));
			if(i != 0 && i != horstepCount)
			{
				graph.MoveTo(widthPerS * i, height);
				graph.LineTo(widthPerS * i, -rulerSize);
			}
			graph.SetColor(Color.black);
			KeyValuePair<DateTime, float> pair = timeValuelist.ElementAt(timeValuelist.Count - 1 - horstepCount + i);
			graph.TextOut(pair.Key.ToString("MMM d yy"), widthPerS * i, -rulerSize - 5, TextAnchor.UpperCenter, FontStyle.Bold);
		}

		float maxValue = -1;
		foreach(KeyValuePair<DateTime, float> pair in timeValuelist)
		{
			if (maxValue < pair.Value)
				maxValue = pair.Value;
		}
		DrawVerticalScale(maxValue, true);

	}

	void DrawBoundRect()
	{
		graph.MoveTo(0, 0);
		graph.LineTo(width, 0);
		graph.LineTo(width, height);
		graph.LineTo(0, height);
		graph.LineTo(0, 0);
	}

	void DrawVerticalScale(float maxValue, bool showgrid = false)
	{
		int verstepCount = 0;
		float valueStep = 0;
		if (maxValue <= 1)
		{
			verstepCount = 1;
			valueStep = 1;
		}
		else if (maxValue <= 5)
		{
			valueStep = 1;
			verstepCount = (int)Mathf.Ceil(maxValue) + 1;
		}
		else
		{
			verstepCount = 5;
			valueStep = (int)(maxValue / 5) + 1;
		}
		heightPerV = height / (verstepCount * valueStep);
		for (int i = 1; i <= verstepCount; i++)
		{
			if (showgrid)
			{
				if(i != verstepCount)
				{
					graph.MoveTo(width, heightPerV * i * valueStep);
					graph.LineTo(-rulerSize, heightPerV * i * valueStep);
				}
			}
			else
			{
				graph.MoveTo(0, heightPerV * i * valueStep);
				graph.LineTo(-rulerSize, heightPerV * i * valueStep);
			}
			
		}
		for (int i = 1; i <= verstepCount; i++)
		{
			graph.TextOut(((int)(valueStep * i)).ToString(), -rulerSize - 5, heightPerV * i * valueStep, TextAnchor.MiddleRight, FontStyle.Bold);
		}
	}
	void DrawAxis(float maxTimeS, float maxValue)
	{
		graph.SetFontSize(30);
		graph.TextOut("0", -rulerSize, -rulerSize, TextAnchor.UpperRight, FontStyle.Bold);
		int horstepCount = 0;
		float timeStep = 0;
		if (maxTimeS <= 60)
		{
			horstepCount = 1;
			timeStep = 60;
		}
		else if (maxTimeS <= 720)
		{
			timeStep = 60;
			horstepCount = (int)((maxTimeS - 1) / timeStep) + 1;
		}
		else if(maxTimeS <= 3600)
		{
			timeStep = 360;
			horstepCount = (int)((maxTimeS - 1) / timeStep) + 1;
		}
		else
		{
			horstepCount = 6;
			timeStep = (int)(maxTimeS / 1800) * 360;
		}

		widthPerS = width / (horstepCount * timeStep);
		for (int i = 1; i <= horstepCount; i++){
			graph.MoveTo(widthPerS * i * timeStep, 0);
			graph.LineTo(widthPerS * i * timeStep, -rulerSize);
		}

		
		for (int i = 1; i <= horstepCount; i++)
		{
			graph.TextOut(((int)(timeStep * i / 60)).ToString(), widthPerS * i * timeStep, -rulerSize - 5, TextAnchor.UpperCenter, FontStyle.Bold);
		}

		DrawVerticalScale(maxValue);
	}

	void DrawGraph(Dictionary<float, float> timeValuelist)
	{
		graph.MoveTo(0, 0);
		foreach (KeyValuePair<float, float> pair in timeValuelist){
			graph.LineTo(widthPerS * pair.Key, heightPerV * pair.Value);
		}
	}

	void DrawGraph(Dictionary<DateTime, float> timeValuelist)
	{
		int count = 0;
		foreach (KeyValuePair<DateTime, float> pair in timeValuelist)
		{
			graph.SetColor(Color.red);
			if(count == 0)
				graph.MoveTo(widthPerS * count, heightPerV * pair.Value);
			else
				graph.LineTo(widthPerS * count, heightPerV * pair.Value);
			graph.SetColor(Color.cyan);
			graph.DrawCircle(widthPerS * count, heightPerV * pair.Value, 13);
			graph.SetColor(Color.red);
			graph.DrawCircle(widthPerS * count, heightPerV * pair.Value, 10);
			count++;
		}
	}
}
