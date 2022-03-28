using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class BetBtnUi : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();
    private float holdTime = 0.3f;
    public SpriteRenderer spriteRenderer;
    public TextMeshProUGUI txtMultiplayer;
    private bool held = false;
    public bool btnBetClickable = false;
    public CanvasGroup raiseInfoDialog;
   






    public void OnClick()
    {

        onClick.Invoke();

    }
    public void OnPointerUp(PointerEventData eventData)
    {
        LoadSpriteBtn(false);
        CancelInvoke("OnLongPress");

        if (!held)
        {
            onClick.Invoke();
        }
        HideRaiseInfo();
    }

   

    public void OnPointerDown(PointerEventData eventData)
    {

        LoadSpriteBtn(true);
        held = false;
        Invoke("OnLongPress", holdTime);

    }
    private void LoadSpriteBtn(bool press)
    {
        
            string path = "bet";
            if (press)
            {
                path += "_p";
            }
            spriteRenderer.sprite = Resources.Load("Sprites/GameScene/" + path, typeof(Sprite)) as Sprite;
        
    }


 /*   public void DisplayDoubleDamage(string currentDmg)
    {
        spriteRenderer.sprite = Resources.Load("Sprites/GameScene/bet_empty", typeof(Sprite)) as Sprite;
        txtMultiplayer.gameObject.SetActive(true);
        txtMultiplayer.text =  currentDmg;
        anim.Play();
    }*/

   /* [Button]
    public void ResetBtn()
    {
        txtMultiplayer.gameObject.SetActive(false);
        LoadSpriteBtn(false);
        anim.Rewind();
        anim.Play();
        anim.Sample();
        anim.Stop();
        spriteRenderer.material.SetFloat("_Glow", 0f);
    }*/

    public void OND()
    {
        if (!BattleSystem.Instance.infoShow)
        {
            ShowRaiseInfo();
        }
    }

    private void ShowRaiseInfo()
    {
        float currentDmg = BattleSystem.Instance.currentDamageThisRound; // Store it differently
        raiseInfoDialog.gameObject.SetActive(true);
        StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(raiseInfoDialog, true, Values.Instance.infoDialogFadeOutDuration, null));
        
    }
    private void HideRaiseInfo()
    {
        StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(raiseInfoDialog,
            false, Values.Instance.infoDialogFadeOutDuration, ()=> raiseInfoDialog.gameObject.SetActive(false)));
    }

    private void OnLongPress()
    {
        held = true;
        onLongPress.Invoke();
    }

    public void EnableBetBtn(bool enable)
    {
        Action btnEnable = () => btnBetClickable = enable;
        //btnEnable.Invoke();
        if (!enable)
        {
            btnEnable = null;
            btnBetClickable = false;
        }
          StartCoroutine(AnimationManager.Instance.DarkerAnimation(spriteRenderer, !enable, Values.Instance.puChangeColorDisableDuration, btnEnable));
        /*  float value = 0.65f;
          if (enable)
          {
              value = 0;
          }
          else
          {
              btnEnable.Invoke();
              btnEnable = null;
          }
          // Make IT more stable
  */
    }
}
