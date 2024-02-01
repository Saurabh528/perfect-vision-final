using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.
using UnityEngine.Events;

public class CustomSlider : MonoBehaviour, IPointerDownHandler, IPointerUpHandler// required interface when using the OnPointerDown method.
{
	public UnityEvent<PointerEventData> onPointerDown, onPointerUp;
	//, onPointerExit;
	//Do this when the mouse is clicked over the selectable object this script is attached to.
	public void OnPointerDown(PointerEventData eventData)
	{
		if(onPointerDown != null)
			onPointerDown.Invoke(eventData);
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		if (onPointerUp != null)
			onPointerUp.Invoke(eventData);
	}
}