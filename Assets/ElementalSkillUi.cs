using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class ElementalSkillUi : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{

    public bool isPlayer;
    public bool isClickable;
    public string element;
    public int ncCounterUse;

    public GameObject pElementSkillLiquid;
    public SpriteRenderer pElementalSkillIcon;
    public SpriteRenderer pElementalSkillLiquid;
    private float holdTime = 0.3f;
    private bool held = false;
    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();


    public void FillElemental(int amount)
    {
        float targetY = 0f;
        switch (amount)
        {
            case 0:
                targetY = -5.54f;
                break;
            case 1:
                targetY = -5f;
                break;
            case 2:
                targetY = -4.6f;
                break;
            case 3:
                targetY = -4.2f;
                break;
        }
        StartCoroutine(AnimationManager.Instance.SimpleSmoothMove(pElementSkillLiquid.transform, 0,
            new Vector3(pElementSkillLiquid.transform.position.x, targetY, pElementSkillLiquid.transform.position.z), Values.Instance.elementalSkillFillDuration, null, null));
    }

    public void UpdateEsAfterNcUse(string element)
    {
        if (this.element == element)
        {
            if (++ncCounterUse >= 3)
            {
                ncCounterUse = 3;
            }
            FillElemental(ncCounterUse);
        }
    }

    public void InitializeES(string elementalSkillType)
    {
        element = elementalSkillType;
        Color liquidColor = new Color(1, 1, 1);
        string iconPath = "";
        switch (elementalSkillType)
        {
            case "f": //fire
                {
                    liquidColor = Values.Instance.fireElement;
                    iconPath = "fire_icon";
                    break;
                }
            case "i": //ice
                {
                    liquidColor = Values.Instance.iceElement;
                    iconPath = "ice_icon";
                    break;
                }
            case "w": //wind
                {
                    liquidColor = Values.Instance.windElement;
                    iconPath = "wind_icon";
                    break;
                }
        }
        pElementalSkillLiquid.color = liquidColor;
        pElementalSkillIcon.sprite = Resources.Load("Sprites/GameScene/ElementalSkill/" + iconPath, typeof(Sprite)) as Sprite;
    }

    public void Enable(bool enable)
    {
        Action btnEnable = () => isClickable = enable; ;
        if (enable)
        {
        }
        else
        {
            btnEnable.Invoke();
            btnEnable = null;
        }
        // Make IT more stable
        //StartCoroutine(AnimationManager.Instance.UpdateValue(enable, "_GradBlend", Values.Instance.puChangeColorDisableDuration, btnReplaceRenderer.material, value, btnEnable));
        StartCoroutine(AnimationManager.Instance.DarkerAnimation(pElementalSkillIcon, !enable, Values.Instance.puChangeColorDisableDuration, btnEnable));
    }
    public void OnClick()
    {

        if (Constants.TUTORIAL_MODE)
        {
           // BattleSystemTuto.Instance.OnPuClick(this);
        }
        else
        {
            if(isPlayer && ncCounterUse == 3 && isClickable)
            {
            BattleSystem.Instance.OnEsClick(this);
            }
            else
            {
                SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CantClick, false);
            }
        }
    }
    public void OnPointerUp(PointerEventData eventData)
    {

        CancelInvoke("OnLongPress");
        
            if (!held)
            {
                onClick.Invoke();
            }
            else if (Constants.TUTORIAL_MODE && BattleSystemTuto.Instance.infoShow)
            {
                BattleSystemTuto.Instance.HideDialog();
            }
            else if (!Constants.TUTORIAL_MODE && BattleSystem.Instance.infoShow)
            {
                BattleSystem.Instance.HideDialog(true);
            }
        
    }

    public void OnPointerDown(PointerEventData eventData)
    {
            held = false;
            Invoke("OnLongPress", holdTime);
    }

   /* private void PressSprite(bool pressed)
    {
        if (puIndex == -1 && isPlayer)
        {
            string spritePath = "skill_white";
            if (pressed)
            {
                spritePath = "skill_press";
            }
            spriteRenderer.sprite = Resources.Load("Sprites/GameScene/Buttons/" + spritePath, typeof(Sprite)) as Sprite;
        }
    }
*/
    public void OND()
    {
        if (!Constants.TUTORIAL_MODE && !BattleSystem.Instance.infoShow)
        {
            BattleSystem.Instance.ShowPuInfo(transform.position,true,false, "es"+element, "");
        }
        else if (Constants.TUTORIAL_MODE && !BattleSystemTuto.Instance.infoShow)
        {
            BattleSystemTuto.Instance.ShowPuInfo(transform.position, false, "es" + element, "");
        }
    }


    private void OnLongPress()
    {
        held = true;
        onLongPress.Invoke();
    }
}
