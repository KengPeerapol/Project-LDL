using System.Collections ;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
public class DragDropUi : MonoBehaviour, IDragHandler
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private RectTransform rectTransform;
    
    void Start()
    {
        rectTransform = GetComponent<RectTransform>();
    }
    
    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta ;
    }
}
