using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ButtonEvnets : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
   
    public UnityEvent onPointerDown = new UnityEvent();
    public UnityEvent onPointerUp = new UnityEvent();
 
   
    public void OnPointerDown(PointerEventData eventData)
    {
        onPointerDown.Invoke();
    }
   
    
    public void OnPointerUp(PointerEventData eventData)
    {
        onPointerUp.Invoke();
    }
}