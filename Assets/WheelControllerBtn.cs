using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class WheelControllerBtn : MonoBehaviour, IDropHandler
{
    public int id;
    public string itemName;
    public Sprite icon;

  

  /*  
    public void HoverEnter()
    {
        StartCoroutine(AnimationManager.Instance.ScaleObjectWheel(transform, 1.15f, 0.2f));
    }
    public void HoverExit()
    {
        StartCoroutine(AnimationManager.Instance.ScaleObjectWheel(transform, 1f, 0.2f));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        HoverExit();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        HoverEnter();
    }

    public void OnPointerClick(PointerEventData eventData)
    {
       // BattleSystem.Instance.EmojiSelected(id);
    }*/

    public void OnDrop(PointerEventData eventData)
    {
        Debug.LogError("WOEKING");
        if (eventData.pointerDrag != null)
        {
        BattleSystem.Instance.EmojiSelected(id);
        Debug.LogError("WOEKING!!!!!!!");
        }
    }
}
