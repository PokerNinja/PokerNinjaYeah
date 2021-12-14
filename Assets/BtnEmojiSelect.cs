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
    public Transform btnTransform;
    private int currentEmojiId = 0;
    public BoxCollider2D boxCollider;
    private Vector2 largeBox, smallBox;
    private Vector2 largeOffset, smallOffset;
    //private bool held = false;
    //public UnityEvent onClick = new UnityEvent();

    public UnityEvent onLongPress = new UnityEvent();

    private void Start()
    {
        holdTime = Values.Instance.holdTimeForEmojiSelector;
        smallBox = new Vector2(0.1f, 0.1f);
        largeBox = new Vector2(1.6f, 2.5f);
        smallOffset = new Vector2(0f, 0f);
        largeOffset = new Vector2(0.1004553f, -0.7235291f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        currentEmojiId = -1;
        transform.position = eventData.pointerCurrentRaycast.worldPosition;
        Invoke("OnLongPress", holdTime);

    }


    public void OnPointerUp(PointerEventData eventData)
    {
        transform.position = startingPosition.position;
        boxCollider.size = largeBox;
        boxCollider.offset = largeOffset;
        BattleSystem.Instance.EmojiSelected(currentEmojiId);

        CancelInvoke("OnLongPress");
        //if (!held)
        //    onClick.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        //Debug.LogError("Exit");

        CancelInvoke("OnLongPress");
    }

    void OnLongPress()
    {
        //held = true;
        //Debug.LogError("LongPress");
        onLongPress.Invoke();
        boxCollider.size = smallBox;
        boxCollider.offset = smallOffset;
    }


    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.position = eventData.pointerCurrentRaycast.worldPosition;
        canvasGroup.blocksRaycasts = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        currentEmojiId = ConvertNameToId(collision.name);
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

}
