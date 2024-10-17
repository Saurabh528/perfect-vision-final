using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.Audio;


public class DestroyOnContact : MonoBehaviour {

	public Text Ballons;
	public int Ballonsdestroy;

	public AudioClip blastSound;
	public  AudioSource audioSource;

	public GameObject Panel;

	


	void Start()
	{
		audioSource = GetComponent<AudioSource>();
		if (audioSource == null)
		{
			gameObject.AddComponent<AudioSource>();
			audioSource = GetComponent<AudioSource>();
		}
	}


	void OnTriggerEnter2D (Collider2D other){

		if(other.gameObject.CompareTag("Fruits") && GamePlayController.Instance.IsPlaying())

        {
			audioSource.PlayOneShot(blastSound);
			Ballonsdestroy++;
			ShowMissingCount();
			if (GamePlayController.Instance.GetType() == typeof(BallonBurstController))
				GamePlayController.Instance.IncreaseScore(-1);
			if (Ballonsdestroy >= 5)
			{
				if(GameState.currentGamePlay == null)
				{
					if(GamePlayController.Instance.GetType() == typeof(BallonBurstController))
						GamePlayController.Instance.GameOver();
				}
				else
				{
					Ballonsdestroy = 0;
					ShowMissingCount();
					GamePlayController.Instance.IncreaseLevel(-1);
				}
			}
		}
		Destroy(other.gameObject);

	}

	public void ShowMissingCount()
	{
		if (Ballons)
			Ballons.text = "Ballons Missing" + " " + Ballonsdestroy;
	}

	public void ResetMissingCount()
	{
		Ballonsdestroy = 0;
		ShowMissingCount();
	}

}
