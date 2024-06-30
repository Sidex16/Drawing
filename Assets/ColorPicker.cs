using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ColorPicker : MonoBehaviour, IPointerClickHandler
{
    public  Color color; 

    public Image image;

    public void OnPointerClick(PointerEventData eventData)
    {
        color = GetColor(Camera.main.WorldToScreenPoint(eventData.position), GetComponent<Image>());
        image.color = color;
        DrawOnObject.SetDrawColor(color);
    }
    
    Color GetColor(Vector2 screenPoint, Image imageToPick)
    {
        Vector2 point;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(imageToPick.rectTransform, screenPoint, Camera.main, out point);
        point += imageToPick.rectTransform.sizeDelta / 2;
        Texture2D t = GetComponent<Image>().sprite.texture;
        Vector2Int m_point = new Vector2Int((int)(point.x * t.width / imageToPick.rectTransform.sizeDelta.x), (int)(point.y * t.height / imageToPick.rectTransform.sizeDelta.y));
        return t.GetPixel(m_point.x, m_point.y);
    }  
    
    
}

