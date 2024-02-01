using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using PlayFab;
using PlayFab.Json;
using Newtonsoft.Json;
public class test : MonoBehaviour
{
   void Start()
	{
		GameState.playfabID = "3883FA43353";
		GameState.username = "Akuete";
		string value1 = StringEncrypter.Crypt("2024-12-31");
		string decValue = StringEncrypter.Decrypt(value1);
		Debug.Log(value1);
		Debug.Log(decValue);
	}
}
