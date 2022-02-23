using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class Swipper : MonoBehaviour, IDragHandler, IEndDragHandler
{

    public UnityEvent SwipeAction;
    public DraggedDirection dragDirection ;

    public void OnEndDrag(PointerEventData eventData)
    {
        Vector3 dragVectorDirection = (eventData.position - eventData.pressPosition).normalized;
       if(GetDragDirection(dragVectorDirection) == dragDirection)
        {
            SwipeAction?.Invoke();
        }
    }
    public enum DraggedDirection
    {
        Up,
        Down,
        Right,
        Left
    }
    private DraggedDirection GetDragDirection(Vector3 dragVector)
    {
        float positiveX = Mathf.Abs(dragVector.x);
        float positiveY = Mathf.Abs(dragVector.y);
        DraggedDirection draggedDir;
        if (positiveX > positiveY)
        {
            draggedDir = (dragVector.x > 0) ? DraggedDirection.Right : DraggedDirection.Left;
        }
        else
        {
            draggedDir = (dragVector.y > 0) ? DraggedDirection.Up : DraggedDirection.Down;
        }
        Debug.Log("swipe: " + draggedDir);
        return draggedDir;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("swip1e: " );
    }
}
