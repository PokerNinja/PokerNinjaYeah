using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BtnClicker : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();
    public float holdTime = 0.3f;
    public SpriteRenderer spriteRenderer;
    public string btnName;
    public bool isClickable;
    private bool held = false;



    public void OnClick()
    {
        if (isClickable)
        {
            Debug.LogError("Clicker;");
            onClick.Invoke();
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        LoadSpriteBtn(false);
        CancelInvoke("OnLongPress");

        if (!held)
        {
            onClick.Invoke();
        }
        else
        {
            BattleSystem.Instance.HideDialog(false);
        }
        held = false;
    }

    public void OND()
    {
        Debug.LogError("OND;");

        BattleSystem.Instance.ShowPuInfo(transform.position, false, false, btnName, Constants.ReplacePuInfo);
    }




    public void OnPointerDown(PointerEventData eventData)
    {
        LoadSpriteBtn(true);
        held = false;
        Invoke("OnLongPress", holdTime);

    }

    private void LoadSpriteBtn(bool pressed)
    {
        string path = btnName;
        if (pressed)
        {
            path += "_p";
        }
        spriteRenderer.sprite = Resources.Load("Sprites/GameScene/" + path, typeof(Sprite)) as Sprite;
    }
    private void OnLongPress()
    {
        held = true;
        onLongPress.Invoke();
    }
}
