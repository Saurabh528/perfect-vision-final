using UnityEngine;
using UnityEngine.UI;

public class RectTransformCalculator : MonoBehaviour
{
	public RectTransform targetRectTransform;

	void Start()
	{
		// Get the CanvasScaler component of the Canvas
		CanvasScaler canvasScaler = targetRectTransform.GetComponentInParent<Canvas>().GetComponent<CanvasScaler>();

		// Get the reference resolution of the Canvas Scaler
		Vector2 referenceResolution = canvasScaler.referenceResolution;

		// Get the screen resolution
		float screenWidth = Screen.width;
		float screenHeight = Screen.height;

		// Calculate the scale factor based on screen size and reference resolution
		float scaleFactor = canvasScaler.scaleFactor;

		// Get the size of the RectTransform in canvas space
		Vector2 rectSize = targetRectTransform.rect.size;

		// Convert RectTransform size to screen space size
		float screenWidthInCanvasSpace = (rectSize.x / referenceResolution.x) * screenWidth;
		float screenHeightInCanvasSpace = (rectSize.y / referenceResolution.y) * screenHeight;

		// Alternatively, applying scaleFactor directly for canvas-based calculations
		Vector2 finalSizeInScreenSpace = new Vector2(screenWidthInCanvasSpace, screenHeightInCanvasSpace) * scaleFactor;

		// Output the result
		Debug.Log($"RectTransform size on screen: {finalSizeInScreenSpace}");
	}
}
