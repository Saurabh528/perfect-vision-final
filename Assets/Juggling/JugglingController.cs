using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json.Serialization;
using System.ComponentModel;
using TMPro;
public class JugglingController : GamePlayController
{

	[SerializeField] TextMeshProUGUI textScore;
	[SerializeField] TextMeshProUGUI textLevel;
	public TextMeshProUGUI randomText;
	public Button[] optionButtons;
	private string generatedString;
	private int correctAnswerIndex;
	private string selectedOption = null;
	bool missed = false;
	// Use this for initialization
	string redcolorStr, cyancolorStr;
	const string suffixStr = "UDPQC";

	public TextAsset wordasset;

	public override void Awake()
	{
		base.Awake();
		DictionaryModel.LoadDictionary(wordasset);
	}
	public override void StartGamePlay()
	{
		base.StartGamePlay();
		redcolorStr = $"#{ColorUtility.ToHtmlStringRGB(ColorCalibration.RedColor)}";
		cyancolorStr = $"#{ColorUtility.ToHtmlStringRGB(ColorCalibration.CyanColor)}";
		StartCoroutine(PlayGame());
	}

	private IEnumerator PlayGame()
	{
		generatedString = "";
		yield return new WaitForSeconds(2);
		while (GamePlayController.Instance.IsPlaying())
		{
			
			missed = false;
			GenerateRandomString();
			yield return StartCoroutine(DisplayGeneratedString());
			SetupOptions();

			bool isAnswerCorrect = false;
			while (!isAnswerCorrect)
			{
				yield return new WaitUntil(() => selectedOption != null);
				isAnswerCorrect = CheckAnswer(selectedOption);

				if (isAnswerCorrect)
				{
					Debug.Log("Correct!");
					if(!missed)
						GamePlayController.Instance.IncreaseScore();
				}
				else
				{
					Debug.Log("Incorrect!");
					selectedOption = null; // Reset the selected option
					if (!missed)
					{
						GamePlayController.Instance.IncreaseScore(-1);
						missed = true;
					}
				}
			}

			yield return new WaitForSeconds(1); // Add a 1-second delay before resetting options
			ResetOptions();
			yield return new WaitForSeconds(4); // Add a 2-second interval between each round
		}
	}

	public override void Update()
	{
		base.Update();
		
	}
	public override void OnScoreChange(int levelstartscore, int score)
	{
		base.OnScoreChange(levelstartscore, score);
		if (score >= levelstartscore + 3)
			IncreaseLevel();
	}


	public override void ShowLevel()
	{
		textLevel.text = $"Level {_level}";
	}

	public override void ShowScore()
	{
		textScore.text = $"{_score}";
	}

	private void GenerateRandomString()
	{


		/*generatedString = "PP";
		for (int i = 2; i < 7; i++)
		{
			char randomChar = suffixStr[Random.Range(0, suffixStr.Length)];
			generatedString += randomChar;
		}*/
		int length = Mathf.Clamp(_level + 2, 3, 7);
		string orgword = DictionaryModel.GetRandomWord(length);
		int letterpos = Random.Range(0, length);
		generatedString = "";
		if (letterpos > 0)
			generatedString = orgword.Substring(0, letterpos);
		generatedString  += (char)Random.Range(65, 91);
		if (letterpos < length - 1)
			generatedString += orgword.Substring(letterpos + 1);
	}

	private IEnumerator DisplayGeneratedString()
	{
		// randomText.text = generatedString; // Working Code

		SetCharacterColors(generatedString);
		yield return new WaitForSeconds(2 * GamePlayController.GetDifficultyValue(1, 1, 10, 0.5f));
		randomText.text = "";
		SetupOptions();
	}

	private void SetCharacterColors(string text)
	{
		string coloredText = "";
		for (int i = 0; i < text.Length; i++)
		{
			if(i % 2 == 0)
				coloredText += $"<color={cyancolorStr}>{text[i]}</color>";
			else
				coloredText += $"<color={redcolorStr}>{text[i]}</color>";
			
		}
		randomText.text = coloredText;
	}

	private void SetupOptions()
	{
		List<string> strings = DictionaryModel.GetSimiliarStringList(generatedString, optionButtons.Length);
		correctAnswerIndex = Random.Range(0, optionButtons.Length);
		for (int i = 0; i < optionButtons.Length; i++)
		{
			string option;
			if (i == correctAnswerIndex)
			{
				option = generatedString;
			}
			else
			{
				option = strings[i];
			}

			optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text = option;
			optionButtons[i].onClick.AddListener(() => SelectOption(option));

		}
	}

	private void SelectOption(string option)
	{
		selectedOption = option;
	}

	/*private string GenerateIncorrectOption()
	{
		string incorrectOption = "";
		for (int i = 0; i < 6; i++)
		{
			char randomChar = suffixStr[Random.Range(0, suffixStr.Length)];
			incorrectOption += randomChar;
		}
		return incorrectOption;
		return "";
	}*/

	//private bool CheckAnswer(string selectedOption)
	//{
	//    bool isCorrect = selectedOption == generatedString;
	//    this.selectedOption = null;
	//    return isCorrect;
	//}


	private bool CheckAnswer(string selectedOption)
	{
		bool isCorrect = selectedOption == generatedString;
		int selectedIndex = -1;

		// Find the index of the selected option
		for (int i = 0; i < optionButtons.Length; i++)
		{
			if (optionButtons[i].GetComponentInChildren<TextMeshProUGUI>().text == selectedOption)
			{
				selectedIndex = i;
				break;
			}
		}

		// Change the button color based on the correctness
		if (isCorrect)
		{
			optionButtons[selectedIndex].GetComponent<Image>().color = Color.blue;
			PlayCorrectSound();
		}
		else
		{
			optionButtons[selectedIndex].GetComponent<Image>().color = Color.red;
			PlayWrongSound();
		}

		this.selectedOption = null;
		return isCorrect;
	}

	private void ResetOptions()
	{
		foreach (Button button in optionButtons)
		{
			button.onClick.RemoveAllListeners();
			button.GetComponent<Image>().color = Color.white; // Reset the button color
		}
	}

}

