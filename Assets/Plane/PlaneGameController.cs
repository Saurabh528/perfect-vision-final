using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
public class PlaneGameController : GamePlayController
{

	[SerializeField] TextMeshProUGUI textScore;
	[SerializeField] TextMeshProUGUI textLevel;
	public GameObject planePrefab;
	public int numberOfPlanes = 6;
	public float spawnRadius = 5f;
	//public float speed = 2f;

	private List<GameObject> planes = new List<GameObject>();
	private int hintIndex1, hintIndex2;
	private int clickedPlanes;
	//private List<GameObject> selectionIndicators = new List<GameObject>();

	public float minPlaneDistance = 2.0f;
	public GameObject _wndReady, _wndProceedSelection, _wndSelectObjs;
	bool _clickable, _planeLocked;
	GameObject selectedObj1, selectedObj2;
	float _startTime, _selectTime;
	public float _watchTime = 3;
	public override void StartGamePlay()
	{
		base.StartGamePlay();
		//SetPlayingState(false);
		StartCoroutine(StartNewLevel());
	}

	public override void Update()
	{
		base.Update();
		if (Input.GetMouseButtonDown(0) && _clickable)
		{
			CheckPlaneClick();
		}
		else if (_planeLocked)
		{
			if(Time.time - _startTime > _watchTime + GamePlayController.GetDifficultyValue(1, 0, 20, 20))
			{
				StartCoroutine(Routine_ShowScreen_SelectObjects());
			}
		}

	}

	IEnumerator Routine_ShowScreen_SelectObjects()
	{
		/*foreach (GameObject obj in planes)
			obj.GetComponent<FlyScript>().enabled = false;*/
		_wndSelectObjs.SetActive(true);
		_planeLocked = false;
		_clickable = true;
		yield return new WaitForSeconds(1);
		_wndSelectObjs.SetActive(false);
	}

	private void AddSelectionIndicator(GameObject plane)
	{
		/*GameObject indicator = Instantiate(selectionIndicatorPrefab, plane.transform.position, Quaternion.identity);
		indicator.transform.SetParent(plane.transform);
		indicator.transform.localScale = new Vector3(1, 1, 1);

		// Set the sorting layer and order so the circle appears on top of the plane
		SpriteRenderer indicatorRenderer = indicator.GetComponent<SpriteRenderer>();
		SpriteRenderer planeRenderer = plane.GetComponent<SpriteRenderer>();
		indicatorRenderer.sortingLayerID = planeRenderer.sortingLayerID;
		indicatorRenderer.sortingOrder = planeRenderer.sortingOrder + 1;

		selectionIndicators.Add(indicator);*/
		plane.GetComponent<FlyScript>().ShowHighlight(true);
	}

	public override void OnScoreChange(int levelstartscore, int score)
	{
		base.OnScoreChange(levelstartscore, score);
		if (score >= levelstartscore + 3)
			IncreaseLevel();
	}

	public override void IncreaseLevel()
	{
		base.IncreaseLevel();
	}

	public override void ShowLevel()
	{
		textLevel.text = $"Level {_level}";
	}

	public override void ShowScore()
	{
		textScore.text = $"{_score}";
	}

	void SpawnPlanes()
	{
		for (int i = 0; i < numberOfPlanes + GamePlayController.GetDifficultyValue(1, 0, 20, 10); i++)
		{
			Vector3 spawnPosition;
			bool validPosition;
			int spawnAttempts = 0;

			do
			{
				spawnPosition = new Vector3(Random.Range(-spawnRadius - 2, spawnRadius - 2), Random.Range(-spawnRadius / 2, spawnRadius / 2), 0);
				validPosition = true;
				spawnAttempts++;

				foreach (GameObject plane in planes)
				{
					if (Vector3.Distance(spawnPosition, plane.transform.position) < minPlaneDistance)
					{
						validPosition = false;
						break;
					}
				}
			} while (!validPosition && spawnAttempts < 100);

			if (validPosition)
			{
				GameObject newPlane = Instantiate(planePrefab, spawnPosition, Quaternion.identity);
				SpriteRenderer sr = newPlane.GetComponent<SpriteRenderer>();
				planes.Add(newPlane);
			}
		}

		// Set hint planes
		hintIndex1 = Random.Range(0, planes.Count);
		hintIndex2 = hintIndex1;

		while (hintIndex2 == hintIndex1)
		{
			hintIndex2 = Random.Range(0, planes.Count);
		}

		Debug.Log("Hint plane 1 index: " + hintIndex1); // Debug message
		Debug.Log("Hint plane 2 index: " + hintIndex2); // Debug message

		/*// Assign the same sprite to both hint planes
		Sprite hintSprite = planes[hintIndex1].GetComponent<SpriteRenderer>().sprite;
		planes[hintIndex2].GetComponent<SpriteRenderer>().sprite = hintSprite;*/
	}




	void ShowHint()
	{
		StartCoroutine(HintCoroutine());
	}

	IEnumerator HintCoroutine()
	{
		planes[hintIndex1].GetComponent<FlyScript>().ShowHighlight(true);
		planes[hintIndex2].GetComponent<FlyScript>().ShowHighlight(true);
		yield return new WaitForSeconds(3/* GamePlayController.GetDifficultyValue(1, 1, 10, 0.5f) */);
		//_wndReady.SetActive(true);
		OnBtnReady();
	}

	public void OnBtnReady()
	{
		_wndReady.SetActive(false);
		clickedPlanes = 0;
		planes[hintIndex1].GetComponent<FlyScript>().ShowHighlight(false);
		planes[hintIndex2].GetComponent<FlyScript>().ShowHighlight(false);
		foreach (GameObject obj in planes)
			obj.GetComponent<FlyScript>().enabled = true;
		_startTime = Time.time;
		_planeLocked = true;
		//SetPlayingState(true);
	}

	//IEnumerator HintCoroutine()
	//{
	//    // Get the hint sprite from the first hint plane
	//    Sprite hintSprite = planes[hintIndex1].GetComponent<SpriteRenderer>().sprite;

	//    // Change the color of the hint sprite to blue
	//    hintSprite = ChangeSpriteColor(hintSprite, Color.blue);

	//    // Assign the blue hint sprite to the first hint plane
	//    planes[hintIndex1].GetComponent<SpriteRenderer>().sprite = hintSprite;

	//    // Change the color of the hint sprite to red
	//    hintSprite = ChangeSpriteColor(hintSprite, Color.red);

	//    // Assign the red hint sprite to the second hint plane
	//    planes[hintIndex2].GetComponent<SpriteRenderer>().sprite = hintSprite;

	//    yield return new WaitForSeconds(1);

	//    // Reset the hint planes to their original sprites
	//        planes[hintIndex1].GetComponent<SpriteRenderer>().color = Color.white;
	//        planes[hintIndex2].GetComponent<SpriteRenderer>().color = Color.white;
	//}

	Sprite ChangeSpriteColor(Sprite sprite, Color color)
	{
		// Create a new texture with the same dimensions as the sprite
		Texture2D texture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

		// Get the pixels from the original sprite and set them on the new texture
		Color[] pixels = sprite.texture.GetPixels((int)sprite.textureRect.x,
												   (int)sprite.textureRect.y,
												   (int)sprite.textureRect.width,
												   (int)sprite.textureRect.height);
		texture.SetPixels(pixels);

		// Change the color of the pixels to the desired color
		for (int i = 0; i < pixels.Length; i++)
		{
			if (pixels[i].a > 0)
			{
				pixels[i] = color;
			}
		}
		texture.SetPixels(pixels);
		texture.Apply();

		// Create a new sprite with the modified texture
		Sprite newSprite = Sprite.Create(texture,
										 new Rect(0, 0, sprite.rect.width, sprite.rect.height),
										 new Vector2(0.5f, 0.5f),
										 sprite.pixelsPerUnit);

		return newSprite;
	}


	/*void MovePlanes()
	{
		if (!GamePlayController.Instance.IsPlaying())
			return;
		//foreach (GameObject plane in planes)
		//{
		//    Vector2 randomDirection = new Vector2(Random.Range(-speed, speed), Random.Range(-speed, speed));
		//    plane.transform.position += (Vector3)randomDirection * Time.deltaTime;
		//}

		foreach (GameObject plane in planes)
		{
			// Get the plane's current direction (normalized)
			Vector2 planeDirection = plane.transform.right.normalized;

			// Add a fixed amount to the plane's position based on its direction
			float planeSpeed = Random.Range(0.5f, speed);
			plane.transform.position += (Vector3)(planeDirection * planeSpeed * Time.deltaTime);

			// Check if the plane has moved out of bounds
			if (plane.transform.position.magnitude > spawnRadius)
			{
				// Reverse the plane's direction if it has moved out of bounds
				plane.transform.right = -planeDirection;
			}
		}
	}*/





	void CheckPlaneClick()
	{
		Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
		Vector2 mouseWorldPos2D = new Vector2(mouseWorldPos.x, mouseWorldPos.y);
		Collider2D hitCollider = Physics2D.OverlapPoint(mouseWorldPos2D);

		if (hitCollider != null)
		{
			GameObject clickedPlane = hitCollider.gameObject;
			if (clickedPlane.GetComponent<FlyScript>() && clickedPlane != selectedObj1)
			{
				clickedPlanes++;
				if (clickedPlanes == 1)
					selectedObj1 = clickedPlane;
				else if (clickedPlanes == 2)
					selectedObj2 = clickedPlane;
				AddSelectionIndicator(clickedPlane);
			}
			else
			{
				Debug.Log("Wrong plane clicked!");
			}

			if (clickedPlanes == 2)
			{
				/*GamePlayController.Instance.IncreaseScore();
				StartCoroutine(WaitBeforeReset());*/
				_clickable = false;
				foreach(GameObject obj in planes)
				{
					/*obj.GetComponent<FlyScript>().enabled = false;*/
					_wndProceedSelection.SetActive(true);
				}
				_selectTime = Time.time;
			}
		}
		else
		{
			Debug.Log("No plane clicked.");
		}
	}

	IEnumerator WaitBeforeReset()
	{
		yield return new WaitForSeconds(1);
		ResetLevel();
	}

	void ResetLevel()
	{
		foreach (GameObject plane in planes)
		{
			Destroy(plane);
		}
		planes.Clear();
/*

		foreach (GameObject indicator in selectionIndicators)
		{
			Destroy(indicator);
		}
		selectionIndicators.Clear();*/

		StartCoroutine(StartNewLevel());


	}

	IEnumerator StartNewLevel()
	{
		yield return new WaitForSeconds(0.5f);
		selectedObj1 = selectedObj2 = null;
		_clickable = false;
		clickedPlanes = 0;
		SpawnPlanes();
		ShowHint();
	}

	public void OnBtnProceedSelectionYes() {
		StartCoroutine(CheckResult());		
	}
	public void OnBtnProceedSelectionNo() {
		clickedPlanes = 0;
		if (selectedObj1)
			selectedObj1.GetComponent<FlyScript>().ShowHighlight(false);
		if (selectedObj2)
			selectedObj2.GetComponent<FlyScript>().ShowHighlight(false);
		selectedObj1 = selectedObj2 = null;
		_wndProceedSelection.SetActive(false);
		_clickable = true;
	}

	IEnumerator CheckResult()
	{
		_wndProceedSelection.SetActive(false);
		int correctcount = 0;
		if (selectedObj1 == planes[hintIndex1] || selectedObj1 == planes[hintIndex2])
		{
			correctcount++;
			selectedObj1.GetComponent<FlyScript>().ShowHighlight(true, FlyScript.HighlightMode.Right);
		}
		else
			selectedObj1.GetComponent<FlyScript>().ShowHighlight(true, FlyScript.HighlightMode.Wrong);

		if (selectedObj2 == planes[hintIndex1] || selectedObj2 == planes[hintIndex2])
		{
			correctcount++;
			selectedObj2.GetComponent<FlyScript>().ShowHighlight(true, FlyScript.HighlightMode.Right);
		}
		else
			selectedObj2.GetComponent<FlyScript>().ShowHighlight(true, FlyScript.HighlightMode.Wrong);
		IncreaseScore(correctcount - 1);
		yield return new WaitForSeconds(2);
		ResetLevel();
	}

	public void OnBtnSelectObjsOK()
	{
		_wndSelectObjs.SetActive(false);
		_clickable = true;
	}
}

