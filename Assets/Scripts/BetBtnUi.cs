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

    private void Start()
    {
        anim.Stop();
    }

    /*public void OnClick()
    {
        // TODO move this to battleSystem
        // BattleSystem battleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
        //Maybe Better One
       
            BattleSystem.Instance.EnableReplaceDialog(false);
        Debug.LogError("OND aa" + infoShow);


    }*/


    /*  public void OnPointerUp(PointerEventData eventData)
      {

          Debug.LogError("OnUp " + infoShow);

         // CancelInvoke("OnLongPress");
          BattleSystem.Instance.HideDialog(() => infoShow = false);

      }*/

    /* public void OnPointerDown(PointerEventData eventData)
     {
       //  BattleSystem.Instance.ShowPuInfo(transform.position, " ", Constants.ReplacePuInfo);
         Debug.LogError("OnDown " + infoShow);
         held = false;
            Invoke("OnLongPress", holdTime);
 *//*
         if (!infoShow)
         {
             infoShow = true;
             BattleSystem.Instance.ShowPuInfo(transform.position, " ", Constants.ReplacePuInfo);
         }*//*

     }*/
    /*   public void OND()
       {
           Debug.LogError("OND " + infoShow);

           if (!infoShow)
           {
               infoShow = true;
               BattleSystem.Instance.ShowPuInfo(transform.position, " ", Constants.ReplacePuInfo);
           }

       }*/




    public void OnClick()
    {
        // TODO move this to battleSystem
        // BattleSystem battleSystem = GameObject.Find("BattleSystem").GetComponent<BattleSystem>();
        //Maybe Better One
        /*  if (!Constants.TUTORIAL_MODE)
          {
              if (BattleSystem.Instance.infoShow)
              {
                  BattleSystem.Instance.HideDialog();
              }
              BattleSystem.Instance.EnableReplaceDialog(false, false);
          }
          else
          {*/
        if (BattleSystem.Instance.infoShow)
        {
            BattleSystem.Instance.HideDialog();
        }

        onClick.Invoke();

        //BattleSystem.Instance.EnableBetDialog(true);
        /*}*/
    }
    public void OnPointerUp(PointerEventData eventData)
    {
        LoadSpriteBtn(false);
        CancelInvoke("OnLongPress");

        if (!held)
        {
            onClick.Invoke();
        }
        BattleSystem.Instance.HideDialog();
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
    }

    public void OND()
    {
        if (!Constants.TUTORIAL_MODE && !BattleSystem.Instance.infoShow)
        {
            BattleSystem.Instance.ShowPuInfo(transform.position, false, "bet", Constants.ReplacePuInfo);
        }
        else if (Constants.TUTORIAL_MODE && !BattleSystemTuto.Instance.infoShow)
        {
            BattleSystemTuto.Instance.ShowPuInfo(transform.position, false, "bet", Constants.ReplacePuInfo);
        }
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
