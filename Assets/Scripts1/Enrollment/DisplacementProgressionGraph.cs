using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CreateGraph))]
public class DisplacementProgressionGraph : MonoBehaviour
{
	[SerializeField] Text _textPower;
	float width, height;
	float widthPerS, heightPerV;
	const float rulerSize = 20;
	CreateGraph graph;
	int power = 0;


	public void DrawAnylysData(string axisname, List<DisplacementRecord> recordList, Color color, EYESIDE side)
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
		DrawAxis(recordList, side);
		graph.SetWidth(5);
		graph.SetColor(color);
		DrawGraph(recordList, side);
	}

	void DrawAxis(List<DisplacementRecord> recordList, EYESIDE side)
	{
		graph.SetFontSize(30);
		graph.TextOut("0", -rulerSize, -rulerSize, TextAnchor.LowerRight, FontStyle.Bold);
		DrawHorizontalscale(recordList);

		float maxValue = -1;
		foreach (DisplacementRecord record in recordList)
		{
			if (side == EYESIDE.LEFT)
			{
				if (maxValue < record.aver_displace_left)
					maxValue = record.aver_displace_left;
			}
			else if (side == EYESIDE.RIGHT)
			{
				if (maxValue < record.aver_displace_right)
					maxValue = record.aver_displace_right;
			}

		}
		DrawVerticalScale(maxValue, true);

	}

	void DrawHorizontalscale(List<DisplacementRecord> recordList)
	{
		int horstepCount = Mathf.Min(8, recordList.Count - 1);
		widthPerS = horstepCount == 0 ? width : width / horstepCount;
		for (int i = 0; i <= horstepCount; i++)
		{
			graph.SetColor(new Color(0.7f, 0.7f, 0.7f));
			if (i != 0 && i != horstepCount)
			{
				graph.MoveTo(widthPerS * i, height);
				graph.LineTo(widthPerS * i, -rulerSize);
			}
			graph.SetColor(Color.black);
			DisplacementRecord record = recordList[recordList.Count - 1 - horstepCount + i];
			graph.TextOut(record.datetime.ToString("MMM d yy"), widthPerS * i, -rulerSize - 5, TextAnchor.UpperCenter, FontStyle.Bold);
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

	void DrawVerticalScale(float maxValue, bool showgrid = false)
	{
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
		for (int i = 1; i <= verstepCount; i++)
		{
			if (showgrid)
			{
				if (i != verstepCount)
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




	void DrawGraph(List<DisplacementRecord> recordList, EYESIDE side)
	{
		int count = 0;
		foreach (DisplacementRecord record in recordList)
		{
			graph.SetColor(Color.red);
			float realY = heightPerV * (side == EYESIDE.LEFT ? record.aver_displace_left : record.aver_displace_right) * Mathf.Pow(10, power);
			if (count == 0)
				graph.MoveTo(widthPerS * count, realY);
			else
				graph.LineTo(widthPerS * count, realY);
			graph.SetColor(Color.cyan);
			graph.DrawCircle(widthPerS * count, realY, 13);
			graph.SetColor(Color.red);
			graph.DrawCircle(widthPerS * count, realY, 10);
			count++;
		}
	}
}
