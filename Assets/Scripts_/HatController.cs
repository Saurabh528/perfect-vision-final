using UnityEngine;
using System.Collections;

public class HatController : MonoBehaviour {

	public Camera cam;
	private float maxWidth, maxHeight;
	public AudioSource audioSource;
	public AudioClip blastSound, bombSound;
	int missCount;
	// Use this for initialization
	void Start () {
	
		if (cam == null) {
			cam = Camera.main;
		}
		Vector3 upperCorner = new Vector3 (Screen.width, Screen.height, 0.0f);
		Vector3 targetWidth = cam.ScreenToWorldPoint(upperCorner);
		Bounds bound = GetComponent<Collider2D>().bounds;
		float hatWidth = bound.extents.x;
		maxWidth = targetWidth.x-hatWidth;
		maxHeight = targetWidth.y - bound.extents.y;
	}
	
	// Update is called once per physics timestep
	void FixedUpdate () {
		if (GamePlayController.Instance.IsPlaying()) {
			Vector3 rawPosition = cam.ScreenToWorldPoint (Input.mousePosition);
			Vector3 targetPosition = new Vector3 (Mathf.Clamp(rawPosition.x, -maxWidth, maxWidth), Mathf.Clamp(rawPosition.y, -maxHeight, maxHeight), 0); 
			GetComponent<Rigidbody2D> ().MovePosition (targetPosition);
		}
	}

	void OnTriggerEnter2D(Collider2D other)
	{
		Destroy(other.gameObject);
		if (!GamePlayController.Instance.IsPlaying())
			return;
		
		if (other.gameObject.CompareTag("Fruits"))
		{
			audioSource.PlayOneShot(blastSound, 5);
			GamePlayController.Instance.IncreaseScore();
		}
		else if (other.gameObject.CompareTag("Bomb"))
		{
			audioSource.PlayOneShot(bombSound, 3);
			GamePlayController.Instance.IncreaseScore(-1);
		}
	}
}
