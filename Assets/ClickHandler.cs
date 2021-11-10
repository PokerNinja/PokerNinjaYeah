using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickHandler : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();
    private float holdTime = 0.3f;


    private bool held = false;
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
        Debug.LogError("Clicking");
        if (BattleSystem.Instance.infoShow)
        {
            BattleSystem.Instance.HideDialog();
        }
      
            BattleSystem.Instance.EnableReplaceDialog();
        
    }
    public void OnPointerUp(PointerEventData eventData)
    {

        CancelInvoke("OnLongPress");

        if (!held)
        {
            onClick.Invoke();
        }
        else if (BattleSystem.Instance.infoShow)
        {
            BattleSystem.Instance.HideDialog();
        }

    }






    public void OnPointerDown(PointerEventData eventData)
    {

        held = false;
        Invoke("OnLongPress", holdTime);

    }
    public void OND()
    {

        if (!BattleSystem.Instance.infoShow)
        {
            BattleSystem.Instance.ShowPuInfo(transform.position, "replace", Constants.ReplacePuInfo);
        }
    }


    private void OnLongPress()
    {

        held = true;
        onLongPress.Invoke();
    }


}
