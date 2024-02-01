using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour {

    public TextMeshProUGUI tokenText;

    [HideInInspector]
    public int score = 0;

    void Start()
    {
        tokenText.text = PlayerPrefs.GetInt("Token", 0).ToString();     //Writes out the number of tokens to the screen
    }

    public void IncrementScore()
    {
		if (GamePlayController.Instance.IsPlaying())       //If the game is not over
			GamePlayController.Instance.IncreaseScore();
    }

    public void IncrementToken()
    {
        if (GamePlayController.Instance.IsPlaying())       //If the game is not over
        {
            PlayerPrefs.SetInt("Token", PlayerPrefs.GetInt("Token", 0) + 1);        //Increases the number of tokens
            tokenText.text = PlayerPrefs.GetInt("Token", 0).ToString();     //Writes out the number of tokens to the screen
        }
    }

    public void IncrementToken(int countOfToken)
    {
        if (GamePlayController.Instance.IsPlaying())       //If the game is not over
        {
            PlayerPrefs.SetInt("Token", PlayerPrefs.GetInt("Token", 0) + countOfToken);        //Increases the number of tokens
            tokenText.text = PlayerPrefs.GetInt("Token", 0).ToString();     //Writes out the number of tokens to the screen
            FindObjectOfType<AudioManager>().TokenSound();      //Plays tokenSound
        }
    }

    public void DecrementToken(int decreaseValue)
    {
        PlayerPrefs.SetInt("Token", PlayerPrefs.GetInt("Token", 0) - decreaseValue);        //Decreases the number of tokens by decreaseValue
        tokenText.text = PlayerPrefs.GetInt("Token", 0).ToString();     //Writes out the number of tokens to the screen
    }
}
