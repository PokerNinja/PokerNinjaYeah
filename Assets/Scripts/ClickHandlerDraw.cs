
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ClickHandlerDraw : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();
    private float holdTime = 0.3f;
    public SpriteRenderer spriteRenderer;
    public Animator animator;
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
        if (BattleSystem.Instance.infoShow)
        {
            BattleSystem.Instance.HideDialog(false);
        }
        BattleSystem.Instance.EnableReplaceDialog(false, false);
        if(BattleSystem.Instance.tutoManager.step == 5)
        {
            spriteRenderer.sortingOrder = 0;
            BattleSystem.Instance.tutoManager.SetStep(6);
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
        BattleSystem.Instance.HideDialog(false);
    }


    public void OnPointerDown(PointerEventData eventData)
    {

        LoadSpriteBtn(true);
        held = false;
        Invoke("OnLongPress", holdTime);

    }
    private void LoadSpriteBtn(bool press)
    {
        animator.SetBool("enable", !press);
        //string path = btnName;

        /*  }
          else if (press)
          {
              path += "_p";
          }
          spriteRenderer.sprite = Resources.Load("Sprites/GameScene/Buttons/" + path, typeof(Sprite)) as Sprite;*/
    }

    public void OND()
    {
        BattleSystem.Instance.ShowPuInfo(transform.position, false, false, "replace", Constants.ReplacePuInfo);
    }


    private void OnLongPress()
    {
        held = true;
        onLongPress.Invoke();
    }


}
