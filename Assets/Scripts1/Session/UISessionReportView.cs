using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
public class UISessionReportView : MonoBehaviour
{
	[SerializeField] Text _title;
	[SerializeField] Text[] _gamenames;
	[SerializeField] Text[] _maxScores;
	[SerializeField] Text[] _maxLevels;
	[SerializeField] Text[] _maxavgTimes;

	public void ViewRecord(SessionRecord record)
	{
		_title.text = $"Session Report ({record.time.ToString("f")})";
		for (int i = 0; i < _gamenames.Length; i++){
			if(i < record.games.Count)
			{
				_gamenames[i].text = record.games[i].name;
				_maxScores[i].text = record.games[i].eScr.ToString();
				_maxLevels[i].text = record.games[i].eLvl.ToString();
				float avgtime = record.games[i].sLvl == record.games[i].eLvl ? 0 : ((float)record.games[i].duration / (record.games[i].eLvl - record.games[i].sLvl));
				_maxavgTimes[i].text = avgtime == 0? "-": ((int)avgtime).ToString();
			}
			else
			{
				_gamenames[i].text = _maxScores[i].text = _maxLevels[i].text = _maxavgTimes[i].text = "-";
			}
		}
	}

	public void ViewHighscore()
	{
		_title.text = $"Session HighScore";
		Dictionary<string, StatisData> datas = UISessionRecordView.GetHighscoreData();
		for(int i = 0; i < datas.Count; i++)
		{
			KeyValuePair<string, StatisData> pair = datas.ElementAt(i);
			_gamenames[i].text = pair.Key;
			_maxScores[i].text = pair.Value.maxScore == -1? "-": pair.Value.maxScore.ToString();
			_maxLevels[i].text = pair.Value.maxLevel == -1? "-": pair.Value.maxLevel.ToString();
			_maxavgTimes[i].text = pair.Value.maxAvgTime == -1? "-": ((int)pair.Value.maxAvgTime).ToString();
		}
		gameObject.SetActive(true);
	}

}
