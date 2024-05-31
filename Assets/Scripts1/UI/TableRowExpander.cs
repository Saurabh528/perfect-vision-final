using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UI.Tables;
using System.Collections.Generic;
[RequireComponent(typeof(TableRow))]
public class TableRowExpander : MonoBehaviour, IPointerClickHandler
{
    static TableRowExpander previousExpander;
    float preferredHeight; // Set your preferred height here
    public float animationDuration = 0.5f; // Duration for the height expansion

    private RectTransform rectTransform;
    private ScrollRect parentScrollRect;
    private bool isDragging = false;
    TableRow tableRow;

    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
        tableRow = GetComponent<TableRow>();
        preferredHeight = tableRow.preferredHeight;
        parentScrollRect = GetComponentInParent<ScrollRect>();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!isDragging) // Only handle click if not dragging
        {
            ExpandToFitContent();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        if (parentScrollRect != null)
        {
            parentScrollRect.OnBeginDrag(eventData);
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (parentScrollRect != null)
        {
            parentScrollRect.OnDrag(eventData);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        if (parentScrollRect != null)
        {
            parentScrollRect.OnEndDrag(eventData);
        }
    }

    

    private void ExpandToFitContent()
    {
        float contentHeight = CalculateContentHeight();
        StartCoroutine(AnimateHeight(contentHeight));
    }

    private float CalculateContentHeight()
    {
        
        float contentHeight = preferredHeight;

        // Iterate over child elements (cells)
        foreach (RectTransform cell in rectTransform)
        {
            // Find the Text component in the child objects
            Text textComponent = cell.GetComponentInChildren<Text>();
            if (textComponent != null)
            {
                // Calculate the preferred height of the text content
                float textHeight = LayoutUtility.GetPreferredHeight(textComponent.rectTransform);

                // Add the height of the RectTransform itself
                textHeight += cell.rect.height;

                // Update the content height
                contentHeight = Mathf.Max(contentHeight, textHeight);
            }
        }

        return contentHeight;
    }

    private IEnumerator AnimateHeight(float targetHeight)
    {
        //CollapsePreviousRow();
        float startHeight = rectTransform.sizeDelta.y;
        float elapsedTime = 0f;
        tableRow.preferredHeight = 0;
        while (elapsedTime < animationDuration)
        {
            float newHeight = Mathf.Lerp(startHeight, targetHeight, elapsedTime / animationDuration);
            rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, newHeight);
            tableRow.preferredHeight = newHeight;

            if(previousExpander != null){
                TableRow prevRow = previousExpander.GetComponent<TableRow>();
                newHeight = Mathf.Lerp(prevRow.preferredHeight, previousExpander.preferredHeight, elapsedTime / animationDuration);
                RectTransform prevRT = previousExpander.GetComponent<RectTransform>();
                prevRT.sizeDelta = new Vector2(prevRT.sizeDelta.x, newHeight);
                prevRow.preferredHeight = newHeight;
            }
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        tableRow.preferredHeight = targetHeight;
        rectTransform.sizeDelta = new Vector2(rectTransform.sizeDelta.x, targetHeight);
        if(previousExpander != null){
            previousExpander.GetComponent<TableRow>().preferredHeight = previousExpander.preferredHeight;
            previousExpander.GetComponent<RectTransform>().sizeDelta = new Vector2(previousExpander.GetComponent<RectTransform>().sizeDelta.x, previousExpander.preferredHeight);
        }
        previousExpander = this;
    }

    void CollapsePreviousRow(){
        if(previousExpander != null)
            previousExpander.GetComponent<TableRow>().preferredHeight = previousExpander.preferredHeight;
        previousExpander = null;
    }
}
