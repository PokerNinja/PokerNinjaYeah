using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
{
    [SerializeField]
    [Tooltip("How long must pointer be down on this object to trigger a long press")]
    private float holdTime = 1f;

    // Remove all comment tags (except this one) to handle the onClick event!
    private bool held = false;
    public UnityEvent onPointerUp = new UnityEvent();

    public UnityEvent onLongPress = new UnityEvent();

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.LogError("D");
        held = false;
        Invoke("OnLongPress", holdTime);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.LogError("U");
        CancelInvoke("OnLongPress");

        onPointerUp.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.LogError("E");

        CancelInvoke("OnLongPress");
    }

    private void OnLongPress()
    {
        Debug.LogError("P");

        held = true;
        onLongPress.Invoke();
    }
}