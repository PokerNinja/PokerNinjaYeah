using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonLongPress : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField]
    [Tooltip("How long must pointer be down on this object to trigger a long press")]
    private float holdTime = 0.3f;

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

    public void OND()
    {
        BattleSystem.Instance.ShowPuInfo(transform.position, false, false,"dealer","");
    }

    public void ONU()
    {
        BattleSystem.Instance.HideDialog(false);

    }

    private void OnLongPress()
    {
        Debug.LogError("P");

        held = true;
        onLongPress.Invoke();
    }
}