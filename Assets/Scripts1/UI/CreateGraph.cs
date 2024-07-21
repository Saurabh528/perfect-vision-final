using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CreateGraph : MonoBehaviour
{
	public Sprite lineImage, circleImage;
	float width;
	Color color;
	float curX, curY;
	int fontSize;
	[SerializeField] Font font;

	private void Start()
	{
	}

	public void SetWidth(float w)
	{
		width = w;
	}

	public void SetColor(Color c)
	{
		color = c;
	}

	public void MoveTo(float x, float y)
	{
		curX = x;
		curY = y;
	}

	public void LineTo(float x, float y)
	{
		MakeLine(curX, curY, x, y);
		curX = x;
		curY = y;
	}

	void MakeLine(float ax, float ay, float bx, float by)
	{
		GameObject NewObj = new GameObject();
		NewObj.name = $"({ax}, {ay})-({bx}, {by})";
		Image NewImage = NewObj.AddComponent<Image>();
		NewImage.sprite = lineImage;
		NewImage.type = Image.Type.Sliced;
		NewImage.color = color;
		RectTransform straight = NewObj.GetComponent<RectTransform>();
		straight.SetParent(transform);
		straight.localScale = Vector3.one;
		Vector3 a = new Vector3(ax, ay, 0);
		Vector3 b = new Vector3(bx, by, 0);
		straight.localPosition = (a + b) / 2;
		Vector3 dif = a - b;
		straight.sizeDelta = new Vector2(dif.magnitude + width, width);
		straight.rotation = Quaternion.Euler(new Vector3(0, 0, 180 * Mathf.Atan2(dif.y , dif.x) / Mathf.PI));
	}

	public void DrawCircle(float x, float y, float radius)
	{
		GameObject NewObj = new GameObject();
		NewObj.name = $"Circle({x}, {y})-{radius}";
		Image NewImage = NewObj.AddComponent<Image>();
		NewImage.sprite = circleImage;
		NewImage.type = Image.Type.Simple;
		NewImage.color = color;
		RectTransform circle = NewObj.GetComponent<RectTransform>();
		circle.SetParent(transform);
		circle.localPosition = new Vector3(x, y, 0);
		circle.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, radius * 2);
		circle.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, radius * 2);
		circle.localScale = Vector3.one;
	}

	public void SetFontSize(int size)
	{
		fontSize = size;
	}

	public void TextOut(string str, float x, float y, TextAnchor anchor, FontStyle style)
	{
		GameObject NewObj = new GameObject();
		NewObj.name = $"text-{str}";
		Text text = NewObj.AddComponent<Text>();
		text.text = str;
		text.color = color;
		text.font = font;
		text.fontSize = fontSize;
		text.horizontalOverflow = HorizontalWrapMode.Overflow;
		text.verticalOverflow = VerticalWrapMode.Overflow;
		text.fontStyle = style;
		text.alignment = anchor;
		RectTransform rt = NewObj.GetComponent<RectTransform>();
		rt.SetParent(transform);
		rt.localScale = Vector3.one;
		rt.sizeDelta = Vector2.zero;
		rt.localPosition = new Vector3(x, y, 0);
	}

}
