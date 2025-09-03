using System;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RawImage))]
public class StereogramViewer : MonoBehaviour
{
	public void MergePatterns(Texture2D redPattern, Texture2D bluePattern, int patternDistance)
	{
		int width = redPattern.width;
		int height = redPattern.height;
		int num = width + Math.Abs(patternDistance);
		if (this.mergedTexture != null)
		{
			UnityEngine.Object.Destroy(this.mergedTexture);
		}
		this.mergedTexture = new Texture2D(num, height);
		Color[] array = new Color[num * height];
		Color[] pixels = redPattern.GetPixels();
		Color[] pixels2 = bluePattern.GetPixels();
		for (int i = 0; i < height; i++)
		{
			for (int j = 0; j < num; j++)
			{
				int num2;
				int num3;
				if (patternDistance >= 0)
				{
					num2 = j;
					num3 = j - patternDistance;
				}
				else
				{
					num2 = j + patternDistance;
					num3 = j;
				}
				Color color;
				if (num2 >= 0 && num2 < width)
				{
					color = pixels[i * width + num2];
				}
				else
				{
					color = new Color(0f, 0f, 0f, 0f);
				}
				Color color2;
				if (num3 >= 0 && num3 < width)
				{
					color2 = pixels2[i * width + num3];
				}
				else
				{
					color2 = new Color(0f, 0f, 0f, 0f);
				}
				float r = Mathf.Clamp01(color.r * color.a + color2.r * color2.a);
				float g = Mathf.Clamp01(color.g * color.a + color2.g * color2.a);
				float b = Mathf.Clamp01(color.b * color.a + color2.b * color2.a);
				Mathf.Max(color.a, color2.a);
				//array[i * num + j] = new Color(r, g, b, 1f);//for Red/Cyan pair
				array[i * num + j] = new Color(r, 0, b, 1f);//for Red/Blue pair
			}
		}
		this.mergedTexture.SetPixels(array);
		this.mergedTexture.Apply();
		base.GetComponent<RectTransform>().sizeDelta = new Vector2((float)num, (float)height);
		this.rawImage.texture = this.mergedTexture;
		if (this.markRed)
		{
			this.markRed.localPosition = new Vector3((float)(-(float)patternDistance / 2), 0f, 0f);
		}
		if (this.markCyan)
		{
			this.markCyan.localPosition = new Vector3((float)(patternDistance / 2), 0f, 0f);
		}
	}

	private void OnDestroy()
	{
		if (this.mergedTexture != null)
		{
			UnityEngine.Object.Destroy(this.mergedTexture);
		}
	}

	[SerializeField]
	private RawImage rawImage;

	[SerializeField]
	private Transform markRed;

	[SerializeField]
	private Transform markCyan;

	private Texture2D mergedTexture;
}
