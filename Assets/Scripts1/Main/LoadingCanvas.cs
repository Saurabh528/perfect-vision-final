using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoadingCanvas : MonoBehaviour
{
    public static LoadingCanvas instance;
	private void Awake()
	{
		DontDestroyOnLoad(gameObject);
		instance = this;
        gameObject.SetActive(false);
	}
	
	public static void Show()
	{
		if (!instance)
			return;
		instance.gameObject.SetActive(true);
	}

	public static void Hide()
	{
		if (!instance)
			return;
		instance.gameObject.SetActive(false);
	}
}
