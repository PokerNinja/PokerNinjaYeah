using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickHandlerTuto : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();
    private float holdTime = 0.3f;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
    private bool held = false;
 


    public void OnClick()
    {
        // TODO move this to battleSystem
        // BattleSystem battleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
        //Maybe Better One
        if (BattleSystemTuto.Instance.infoShow)
        {
            BattleSystemTuto.Instance.HideDialog();
        }
        BattleSystemTuto.Instance.EnableReplaceDialog(false,false);
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        LoadSpriteBtn(false);
        Debug.LogError("UUp");

        CancelInvoke("OnLongPress");

        if (!held)
        {
            onClick.Invoke();
        }
        else if (BattleSystemTuto.Instance.infoShow)
        {
            BattleSystemTuto.Instance.HideDialog();
        }

    }






    public void OnPointerDown(PointerEventData eventData)
    {
        
       LoadSpriteBtn(true);
        held = false;
        Invoke("OnLongPress", holdTime);
        Debug.LogError("DDown");

    }
    private void LoadSpriteBtn(bool press)
    {
        animator.SetBool("enable", !press);
       /* string imgName;
        if (press)
        {
            imgName = "draw_press";
        }
        else
        {
            imgName = "draw1";
        }
        spriteRenderer.sprite = Resources.Load("Sprites/GameScene/Buttons/" + imgName, typeof(Sprite)) as Sprite;*/
    }

    public void OND()
    {
        if (!BattleSystemTuto.Instance.infoShow)
        {
            BattleSystemTuto.Instance.ShowPuInfo(transform.position,false, "replace", Constants.ReplacePuInfo);
        }
    }


    private void OnLongPress()
    {
        held = true;
        onLongPress.Invoke();
    }


}
