using PlayFab.MultiplayerModels;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class DictionaryModel
{
	static Dictionary<int, List<string>> dictionary;
	public static void LoadDictionary(TextAsset tasset)
	{
		if (dictionary != null)
			return;
		dictionary = new Dictionary<int, List<string>>();
		string text = tasset.text;
		string[] strings = text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
		List<string> words3 = new List<string>();
		List<string> words4 = new List<string>();
		List<string> words5 = new List<string>();
		List<string> words6 = new List<string>();
		List<string> words7 = new List<string>();
		foreach (string str in strings)
		{
			string trimstr = str.TrimEnd('\r');
			if (trimstr.Length == 3)
				words3.Add(trimstr);
			else if (trimstr.Length == 4)
				words4.Add(trimstr);
			else if (trimstr.Length == 5)
				words5.Add(trimstr);
			else if (trimstr.Length == 6)
				words6.Add(trimstr);
			else if (trimstr.Length == 7)
				words7.Add(trimstr);
		}
		dictionary[3] = words3;
		dictionary[4] = words4;
		dictionary[5] = words5;
		dictionary[6] = words6;
		dictionary[7] = words7;
	}

	public static string GetRandomWord(int length)
	{
		if (!dictionary.ContainsKey(length))
			return "";
		List<string> strings = dictionary[length];
		if (strings.Count == 0)
			return "";
		return strings[Random.Range(0, strings.Count)];
	}

	public static List<string> GetSimiliarStringList(string origin, int count)
	{
		List<string> strings = dictionary[origin.Length];
		if (strings.Count == 0)
			return null;
		List<string> candidatestrs = new List<string>();
		for (int errs = 1; errs <= origin.Length; errs++)
		{
			foreach (string str in strings)
			{
				if (GetErrorCount(str, origin) == errs)
				{
					candidatestrs.Add(str);
					if (candidatestrs.Count == 50)
					{
						return GetRandomStrings(candidatestrs, count);
					}
				}
			}
		}

		return GetRandomStrings(candidatestrs, count);
	}

	public static List<string> GetRandomStrings(List<string> candidatestrs, int count)
	{
		if (candidatestrs.Count < count)
			return null;
		List<int> indexs = new List<int>();
		for (int i = 0; i < count; i++)
		{
			int index = Random.Range(0, candidatestrs.Count);
			while (indexs.Contains(index))
				index = Random.Range(0, candidatestrs.Count);
		}
		List<string> retstrs = new List<string>();
		foreach (int i in indexs)
			retstrs.Add(candidatestrs[i]);
		return candidatestrs;
	}

	public static int GetErrorCount(string s1, string s2)
	{
		if (s1.Length != s2.Length)
			return Mathf.Min(s1.Length, s2.Length);
		int count = 0;
		for(int i = 0; i < s1.Length; i++)
		{
			if (s1[i] != s2[i])
				count++;
		}
		return count;
	}
}
