using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CreateGraph))]
public class DisplacementGraph : MonoBehaviour
{
	[SerializeField] Text _textPower;
	CreateGraph graph;
	float width, height;
	float widthPerS, heightPerV;
	int power = 0;
	const float rulerSize = 20;
	

	

	public void DrawDisplacementData(List<float> Valuelist, Color color)
	{
		Debug.Log("DrawDisplacementData called");
		UtilityFunc.DeleteAllChildTransforms(transform);
		RectTransform rt = GetComponent<RectTransform>();
		width = rt.rect.width;
		height = rt.rect.height;
		graph = GetComponent<CreateGraph>();
		graph.SetWidth(5);
		graph.SetColor(Color.black);
		DrawBoundRect();
		graph.SetWidth(2);
		DrawAxis(Valuelist);
		graph.SetWidth(5);
		graph.SetColor(color);
		DrawGraph(Valuelist);
	}

	void DrawAxis(List<float> Valuelist)
	{
		Debug.Log("DrawAxis called");
		graph.SetFontSize(30);
		graph.TextOut("0", -rulerSize, -rulerSize, TextAnchor.LowerRight, FontStyle.Bold);
		DrawHorizontalScale();
		float maxValue = -1;
		foreach (float value in Valuelist)
		{
			maxValue = Math.Max(maxValue, value);
		}
		
		DrawVerticalScale(maxValue, true);

	}

	void DrawHorizontalScale()
	{
		Debug.Log("DrawHorizontalScale called");
		int horstepCount = 10;
		widthPerS = width / horstepCount;
		for (int i = 1; i <= horstepCount; i++)
		{
			if (i != horstepCount)
			{
				graph.SetColor(new Color(0.7f, 0.7f, 0.7f));
				graph.MoveTo(widthPerS * i, height);
				graph.LineTo(widthPerS * i, -rulerSize);
			}

			graph.SetColor(Color.black);
			graph.TextOut(i.ToString(), widthPerS * i, -rulerSize - 5, TextAnchor.UpperCenter, FontStyle.Bold);
		}
	}

	void DrawBoundRect()
	{
		Debug.Log("DrawBoundRect called");
		graph.MoveTo(0, 0);
		graph.LineTo(width, 0);
		graph.LineTo(width, height);
		graph.LineTo(0, height);
		graph.LineTo(0, 0);
	}

	void DrawVerticalScale(float maxValue, bool showgrid = false)
	{
		Debug.Log("DrawVerticalScale called");
		int verstepCount = 0;
		float valueStep = 0;
		if (maxValue <= 1)
		{
			verstepCount = 10;
			power = (int)(-Mathf.Log10(maxValue) + 1);
			valueStep = 1;
			_textPower.transform.parent.gameObject.SetActive(true);
			_textPower.text = (-power).ToString();
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
		graph.SetColor(new Color(0.7f, 0.7f, 0.7f));
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
		graph.SetColor(Color.black);
		for (int i = 1; i <= verstepCount; i++)
		{
			graph.TextOut(((int)(valueStep * i)).ToString(), -rulerSize - 5, heightPerV * i * valueStep, TextAnchor.MiddleRight, FontStyle.Bold);
		}
	}
	

	void DrawGraph(List<float> Valuelist)
	{
		Debug.Log("Draw function called");
		if (Valuelist.Count == 0)
			return;
		int count = Valuelist.Count;
		graph.SetColor(Color.blue);
		graph.SetWidth(3);
		int prevIndex = 0;
		graph.MoveTo(0, Valuelist[prevIndex] * heightPerV * Mathf.Pow(10, power));
		for(int i = 1; i < width; i++)
		{
			int newindex = count * i / (int)width;
			if (newindex >= count)
				break;
			if(newindex != prevIndex)
			{
				graph.LineTo(i, Valuelist[newindex] * heightPerV * Mathf.Pow(10, power));
				prevIndex = newindex;
			}
		}
	}

	
}
