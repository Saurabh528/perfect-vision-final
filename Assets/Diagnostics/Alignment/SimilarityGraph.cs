using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class SimilarityGraph : MonoBehaviour
{
	public CreateGraph graph;
	float width, height;
	float widthPerS, heightPerV, widthPerValue;
	const float rulerSize = 20;
	[SerializeField] Text _txtTitle;
	// Start is called before the first frame update
	public void Draw(List<IrisState> irisList, IRISSIMCLASS isclass)
	{
		RectTransform rt = graph.GetComponent<RectTransform>();
		width = rt.rect.width;
		height = rt.rect.height;
		graph.SetWidth(5);
		graph.SetColor(Color.black);
		DrawBoundRect();
		graph.SetWidth(2);
		DrawAxis(irisList);
		graph.SetWidth(5);
		graph.SetColor(Color.black);
		DrawGraph(irisList, isclass);
		_txtTitle.text = isclass.ToString() + " Plot";
	}

	void DrawAxis(List<IrisState> irisList)
	{
		graph.SetFontSize(30);
		graph.TextOut("0", -rulerSize, -rulerSize, TextAnchor.LowerRight, FontStyle.Bold);
		DrawHorizontalScale(irisList);

		DrawVerticalScale(true);

	}

	void DrawHorizontalScale(List<IrisState> irisList)
	{
		int horstepCount = 0;
		int valueStep = 0;
		if (irisList.Count <= 5)
		{
			valueStep = 1;
			horstepCount = irisList.Count;
		}
		else
		{
			horstepCount = 6;
			valueStep = (int)(irisList.Count / 6) + 1;
		}
		widthPerS = width / horstepCount;
		widthPerValue = width / (horstepCount * valueStep);
		for (int i = 1; i <= horstepCount; i++)
		{
			if (i != horstepCount)
			{
				graph.SetColor(new Color(0.7f, 0.7f, 0.7f));
				graph.MoveTo(widthPerS * i, height);
				graph.LineTo(widthPerS * i, -rulerSize);
			}

			graph.SetColor(Color.black);
			graph.TextOut((i * valueStep).ToString(), widthPerS * i, -rulerSize - 5, TextAnchor.UpperCenter, FontStyle.Bold);
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
		float valueStep = 10;
		heightPerV = height / (verstepCount * valueStep);
		graph.SetColor(new Color(0.7f, 0.7f, 0.7f));
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
		graph.SetColor(Color.black);
		for (int i = 1; i <= verstepCount; i++)
		{
			graph.TextOut(((int)(valueStep * i)).ToString(), -rulerSize - 5, heightPerV * i * valueStep, TextAnchor.MiddleRight, FontStyle.Bold);
		}
	}


	void DrawGraph(List<IrisState> irisList, IRISSIMCLASS isclass)
	{
		if (irisList.Count == 0)
			return;
		graph.SetColor(Color.blue);
		graph.SetWidth(3);
		float value = 0;
		if (isclass == IRISSIMCLASS.LELD)
			value = irisList.First().LeLd;
		else if (isclass == IRISSIMCLASS.LERD)
			value = irisList.First().LeRd;
		else if (isclass == IRISSIMCLASS.RELD)
			value = irisList.First().ReLd;
		else
			value = irisList.First().ReRd;
		graph.MoveTo(0, value * heightPerV);
		for (int i = 1; i < irisList.Count; i++)
		{
			if (isclass == IRISSIMCLASS.LELD)
				value = irisList[i].LeLd;
			else if (isclass == IRISSIMCLASS.LERD)
				value = irisList[i].LeRd;
			else if (isclass == IRISSIMCLASS.RELD)
				value = irisList[i].ReLd;
			else
				value = irisList[i].ReRd;
			graph.LineTo(widthPerValue * (i + 1), value * heightPerV);
		}
	}
}
