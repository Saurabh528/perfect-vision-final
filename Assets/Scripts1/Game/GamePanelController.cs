using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class GamePanelController : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI _textHelp;
    [SerializeField] GameObject _objHelpPanel;
    // Start is called before the first frame update
    void Start()
    {
		GameState.currentGameMode = GAMEMODE.SingleGame;
	}

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBtnGameHelp(string str)
    {

    }

    public void OnBtnHelpClose() { 
    }
}
