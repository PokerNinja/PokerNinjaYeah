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
    public Animation anim;
    public CanvasGroup raiseInfoDialog;
    public TextMeshProUGUI currentDmgText;
    public TextMeshProUGUI dmgPenelty;

    private void Start()
    {
        anim.Stop();
    }





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
        if (!txtMultiplayer.gameObject.activeSelf)
        {
            string path = "bet";
            if (press)
            {
                path += "_p";
            }
            spriteRenderer.sprite = Resources.Load("Sprites/GameScene/" + path, typeof(Sprite)) as Sprite;
        }
    }


    public void DisplayDoubleDamage(float damageMultiplayer)
    {
        spriteRenderer.sprite = Resources.Load("Sprites/GameScene/bet_empty", typeof(Sprite)) as Sprite;
        txtMultiplayer.gameObject.SetActive(true);
        txtMultiplayer.text = "x" + damageMultiplayer;
        anim.Play();
    }

    public void ResetBtn()
    {
        txtMultiplayer.gameObject.SetActive(false);
        LoadSpriteBtn(false);
        anim.Stop();
        anim.Rewind();
    }

    public void OND()
    {
        if (!Constants.TUTORIAL_MODE && !BattleSystem.Instance.infoShow)
        {
            ShowRaiseInfo();
        }
        else if (Constants.TUTORIAL_MODE && !BattleSystemTuto.Instance.infoShow)
        {
            //BattleSystemTuto.Instance.ShowPuInfo(transform.position, false, "bet", Constants.ReplacePuInfo);
        }
    }

    private void ShowRaiseInfo()
    {
        float currentDmg = BattleSystem.Instance.currentDamageThisRound; // Store it differently
        raiseInfoDialog.gameObject.SetActive(true);
        StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(raiseInfoDialog, true, Values.Instance.infoDialogFadeOutDuration, null));
        currentDmgText.text = currentDmg + " to " + (currentDmg * 1.5);
        dmgPenelty.text = "-"+ (currentDmg/2) + " HP ";
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
        btnEnable.Invoke();

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
          StartCoroutine(AnimationManager.Instance.UpdateValue(enable, "_GradBlend", Values.Instance.puChangeColorDisableDuration, spriteRenderer.material, value, btnEnable));
  */
    }
}
