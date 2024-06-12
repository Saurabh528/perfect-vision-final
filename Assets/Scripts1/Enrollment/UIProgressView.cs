using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIProgressView : MonoBehaviour
{
	[SerializeField] ProgressionGraph _graphMaxScore, _graphMaxLevel, _graphAvgTime;
	[SerializeField] Text _title;
	public void ViewProgression(string gamename)
	{
		_title.text = gamename + " - Progression Analysis";
		PatientRecord record = PatientDataMgr.GetPatientRecord();
		List<SessionRecord> sessionlist = record.GetSessionRecordList();
		Dictionary<float, float> scoreValueList = new Dictionary<float, float>();
		Dictionary<float, float> levelValueList = new Dictionary<float, float>();
		Dictionary<float, float> avgTimeList = new Dictionary<float, float>();
		float time = 0;
		foreach(SessionRecord ssrecord in sessionlist)
		{
			for(int i = 0; i < ssrecord.games.Count; i++)
			{
				GamePlay gp = ssrecord.games[i];
				if (gp.name == gamename)
				{
					time += gp.duration;
					scoreValueList[time] = GamePlay.GetConvertedScore(gp.eScr, gp.name);
					levelValueList[time] = gp.eLvl;
					avgTimeList[time] = gp.eLvl == gp.sLvl? gp.duration: ((float)gp.duration / (gp.eLvl - gp.sLvl));
				}
			}
		}

		_graphMaxScore.DrawAnylysData(scoreValueList, Color.red);
		_graphMaxLevel.DrawAnylysData(levelValueList, Color.yellow);
		_graphAvgTime.DrawAnylysData(avgTimeList, Color.blue);
	}

	public void ViewDateScoreProgression(string gamename)
	{
		_title.text = gamename;
		Dictionary<DateTime, float> scoreValueList = UISessionRecordView.GetMeanDateScoreList(gamename);
		_graphMaxScore.DrawAnylysData(scoreValueList, Color.red);
	}


}
