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
    //private bool held = false;
    //public UnityEvent onClick = new UnityEvent();

    public UnityEvent onLongPress = new UnityEvent();

    private void Start()
    {
        holdTime = Values.Instance.holdTimeForEmojiSelector;
        smallBox = new Vector2(0.1f, 0.1f);
        largeBox = new Vector2(1.6f, 2.5f);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
      //  var position = Input.mousePosition/ canvas.scaleFactor;
      //  Debug.LogWarning("" + position);
        currentEmojiId = -1;
        Debug.LogError("Down");
        //held = false;
        // btnTransform.position = eventData.pointerPress.transform.position;
        //Vector3 s = (Vector3)(rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor);
       // s.z = 0;
        //btnTransform.position = new Vector3(10f,-2f,0f);
       // Debug.LogError("ds " + PointerDataToRelativePos(eventData));
        //Debug.LogError("d " + eventData.position /canvas.scaleFactor);

        Invoke("OnLongPress", holdTime);
        //  transform.position = eventData.pointerPress.transform.position;

    }

    private Vector2 PointerDataToRelativePos(PointerEventData eventData)
    {
        Vector2 result;
        Vector2 clickPosition = eventData.position;
        RectTransform thisRect = GetComponent<RectTransform>();

        RectTransformUtility.ScreenPointToLocalPointInRectangle(thisRect, clickPosition, null, out result);
        result += thisRect.sizeDelta / 2;

        return result;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.LogError("Up");
        transform.position = startingPosition.position;
        boxCollider.size = largeBox;
        BattleSystem.Instance.EmojiSelected(currentEmojiId);

        CancelInvoke("OnLongPress");
        //if (!held)
        //    onClick.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.LogError("Exit");

        CancelInvoke("OnLongPress");
    }

    void OnLongPress()
    {
        //held = true;
        //Debug.LogError("LongPress");
        onLongPress.Invoke();
        boxCollider.size = smallBox;
    }


    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.LogError("EndDrag");
        canvasGroup.blocksRaycasts = true;
      //  BattleSystem.Instance.EmojiSelected(currentEmojiId);

    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.LogError("d " + eventData.delta / canvas.scaleFactor);
        // rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
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
