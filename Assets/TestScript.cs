using System.Collections;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class TestScript : MonoBehaviour
{
	public RectTransform targetRectTransform;
	void Start(){
		// Get the size of the RectTransform in canvas space (local size)
		Vector2 rectSize = targetRectTransform.rect.size;

		// Output the result in pixels (since ConstantPixelSize doesn't scale based on screen size)
		Debug.Log($"RectTransform size in pixels: {rectSize}");
	}
   
}


