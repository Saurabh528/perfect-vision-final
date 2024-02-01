using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class UIDisplacementProgressView : MonoBehaviour
{
	[SerializeField] Text _textVAxisTitle;
	public DisplacementProgressionGraph graph;

	public void DrawGraph(string axisname, List<DisplacementRecord> recordList, Color color, EYESIDE side)
	{
		_textVAxisTitle.text = axisname;
		graph.DrawAnylysData(axisname, recordList, color, side);
	}

}
