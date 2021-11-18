using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BtnEmojiSelect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    [SerializeField]
    [Tooltip("How long must pointer be down on this object to trigger a long press")]
    private float holdTime = 1f;
    public RectTransform rectTransform;
    public Canvas canvas;
    public CanvasGroup canvasGroup;
   // public GraphicRaycast canvasGroup;

    //private bool held = false;
    //public UnityEvent onClick = new UnityEvent();

    public UnityEvent onLongPress = new UnityEvent();

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.LogError("Down");
        //held = false;
        Invoke("OnLongPress", holdTime);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.LogError("Up");
        CancelInvoke("OnLongPress");

        //if (!held)
        //    onClick.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelInvoke("OnLongPress");
    }

    void OnLongPress()
    {
        //held = true;
        Debug.LogError("LongPress");
        onLongPress.Invoke();
    }


    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.LogError("OnEndDrag");
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.LogError("OnBeginDrag");
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrop(PointerEventData eventData)
    {
        Debug.LogError("OnDrop");
        canvasGroup.blocksRaycasts = true;
        
       /* foreach(var GO in eventData.hovered)
        {
            Debug.LogError("MYMAN " + GO.name);
        }*/
        
            
    }
}
