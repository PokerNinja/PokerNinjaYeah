using UnityEngine.UI;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class EmojiItem : MonoBehaviour
{
    public int id;
    /*public string itemName;
    public Sprite icon;*/




    /*  public void HoverEnter()
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
      }
  */


    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name.Equals("EmojiSelector"))
        {

            StartCoroutine(AnimationManager.Instance.ScaleObjectWheel(transform, 1f, 0.2f));
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.BtnClick, false);
        }

    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        StartCoroutine(AnimationManager.Instance.ScaleObjectWheel(transform, 0.8f, 0.2f));

    }
    /* public void OnDrop(PointerEventData eventData)
     {
         Debug.LogError("WOEKING");
         if (eventData.pointerDrag != null)
         {
         Debug.LogError("WOEKING!!!!!!!");
         BattleSystem.Instance.EmojiSelected(id);
         }
     }*/
}
