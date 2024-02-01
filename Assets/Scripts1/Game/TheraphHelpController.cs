using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class TheraphHelpController : MonoBehaviour
{

    [SerializeField] GameObject _imageGallery;
    [SerializeField] GameObject _objPlayBtn;
	[SerializeField] GameObject _objShowHint;
    public static string PREFNAME_DONTSHOWHINT = "ShowHint_";

	private void Awake()
	{
        string hintstr = PlayerPrefs.GetString(PREFNAME_DONTSHOWHINT + SceneManager.GetActiveScene().name, "true");
        if(hintstr == "true")
            StartCountDown();
	}
	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void StartCountDown()
    {
		_imageGallery.SetActive(false);
		_objPlayBtn.SetActive(false);
		_objShowHint.SetActive(false);
		GetComponent<Animator>().Play("TherapyCountDown");
	}

    public void OnCountEnd()
    {
        gameObject.SetActive(false);
        GamePlayController.Instance.StartGamePlay();
    }

    public void OnToggleHintShow(bool showHint)
    {
        string hintstr = PlayerPrefs.GetString(PREFNAME_DONTSHOWHINT + SceneManager.GetActiveScene().name, "true");
        hintstr = hintstr == "true"? "false": "true";
        PlayerPrefs.SetString(PREFNAME_DONTSHOWHINT + SceneManager.GetActiveScene().name, hintstr);
    }
}
