using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BtnEmojiSelect : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler, IBeginDragHandler, IEndDragHandler, IDragHandler
{
    
    //[Tooltip("How long must pointer be down on this object to trigger a long press")]
    private float holdTime;
    public RectTransform rectTransform;
    public Canvas canvas;
    public CanvasGroup canvasGroup;
    public Transform startingPosition;
    private int currentEmojiId = 0;

    //private bool held = false;
    //public UnityEvent onClick = new UnityEvent();

    public UnityEvent onLongPress = new UnityEvent();

    private void Start()
    {
        holdTime = Values.Instance.holdTimeForEmojiSelector;
    }

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
        canvasGroup.blocksRaycasts = true;
        transform.position = startingPosition.position;
        BattleSystem.Instance.EmojiSelected(currentEmojiId);
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.LogError("OnBeginDrag");
        canvasGroup.blocksRaycasts = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        currentEmojiId = ConvertNameToId(collision.name);
        Debug.LogError("E" + currentEmojiId);
        Debug.LogError("E" + collision.name);

    }

    private int ConvertNameToId(string name)
    {
        switch (name)
        {
            case "Eangry":
                return 0;
            case "Esmug":
                return 1;
            case "Esurprised":
                return 2;
            case "EthumbUp":
                return 3;
            default:
                break;
        }
        return -1;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        currentEmojiId = -1;
    }

    /*    public void OnDrop(PointerEventData eventData)
        {
            Debug.LogError("OnDrop");

           *//* foreach(var GO in eventData.hovered)
            {
                Debug.LogError("MYMAN " + GO.name);
            }*//*


        }*/
}
