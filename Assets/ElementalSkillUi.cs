using Sirenix.OdinInspector;
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

    public Animation shineAnimation;

    public Animation liquidAnimation;
    public Color glowColor;
    public SpriteRenderer glowRenderer;




    [Button]
    public IEnumerator FullEffect(bool enableLoop)
    {
        yield return new WaitForSeconds(1f);
        WhiteGlow();
        if (enableLoop && isPlayer)
        {
            StartCoroutine(EnableGlowLoop());
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EsFull, true);
        }
    }

    public void WhiteGlow()
    {
        glowRenderer.color = new Color(1f, 1f, 1f);
        liquidAnimation.Play();
        shineAnimation.Play("es_full_glow");
    }

    public IEnumerator EnableGlowLoop()
    {
        if (ncCounterUse == 3)
        {
            yield return new WaitForSeconds(2.2f);
            glowRenderer.color = glowColor;
            shineAnimation.Play("es_shine_loop");
        }
    }

    public void DisableGlow()
    {
        shineAnimation.Rewind();
        shineAnimation.Play();
        shineAnimation.Sample();
        shineAnimation.Stop();
        AnimationManager.Instance.SetAlpha(glowRenderer, 0f);
    }

    [Button]
    public void FillElemental(int amount)
    {
        Debug.LogError("FILLINH");
        float targetY = 0f;
        switch (amount)
        {
            case 0:
                targetY = -5.54f;
                if (!isPlayer)
                    targetY = 5.62f;
                break;
            case 1:
                targetY = -5.14f;
                if (!isPlayer)
                    targetY = 5.22f;
                break;
            case 2:
                targetY = -4.65f;
                if (!isPlayer)
                    targetY = 4.72f;
                break;
            case 3:
                targetY = -4.22f;
                if (!isPlayer)
                    targetY = 4.3f;
                break;
        }

        if (amount != 0 && isPlayer)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EsFill, true);
        }
        StartCoroutine(AnimationManager.Instance.SimpleSmoothMove(pElementSkillLiquid.transform, 0f,
            new Vector3(pElementSkillLiquid.transform.position.x, targetY, pElementSkillLiquid.transform.position.z), Values.Instance.elementalSkillFillDuration, () =>
            {
                if (ncCounterUse == 3)
                {
                    StartCoroutine(FullEffect(true));
                }
            }, null));
    }

    public bool IsNcEqualsPesElement(string element)
    {
        Debug.LogError("E" + element);
        Debug.LogError("TE" + this.element);

        return this.element == element;
    }

    [Button]
    public void UpdateEsAfterNcUse(bool isMonster)
    {
        if (isMonster)
        {
            ncCounterUse++;
        }
        if (++ncCounterUse >= 3)
        {
            ncCounterUse = 3;
        }
        if (isPlayer)
            EnableSelecetPositionZ(false);
        FillElemental(ncCounterUse);
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
            case "t": //tech
                {
                    liquidColor = Values.Instance.techElement;
                    iconPath = "tech_icon";
                    break;
                }
        }
        pElementalSkillLiquid.color = liquidColor;
        glowColor = liquidColor;
        pElementalSkillIcon.sprite = Resources.Load("Sprites/GameScene/ElementalSkill/" + iconPath, typeof(Sprite)) as Sprite;
    }

    public void Enable(bool enable)
    {
        isClickable = enable;
        /*Action btnEnable = () => isClickable = enable;
        if (enable)
        {
        }
        else
        {
            btnEnable.Invoke();
            btnEnable = null;
        }*/
        // Make IT more stable
        //StartCoroutine(AnimationManager.Instance.UpdateValue(enable, "_GradBlend", Values.Instance.puChangeColorDisableDuration, btnReplaceRenderer.material, value, btnEnable));
        // StartCoroutine(AnimationManager.Instance.DarkerAnimation(pElementalSkillIcon, !enable, Values.Instance.puChangeColorDisableDuration, btnEnable));
    }
    public void OnClick()
    {

        if (Constants.TUTORIAL_MODE)
        {
            // BattleSystemTuto.Instance.OnPuClick(this);
        }
        else
        {
            if (isPlayer && ncCounterUse == 3 && isClickable)
            {
                DisableGlow();
                WhiteGlow();
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
        else if ( BattleSystem.Instance.infoShow)
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
        if ( !BattleSystem.Instance.infoShow)
        {
            Vector3 pos = transform.position;
            if (!isPlayer)
            {
                pos += new Vector3(0, -5, 0);
            }
            BattleSystem.Instance.ShowPuInfo(pos, true, false, element + "p", "");
        }
    }

    [Button]
    public void EnableSelecetPositionZ(bool aboveDarkScreen)
    {
        Debug.LogError("What");
        float interval = 88.5f;
        if (aboveDarkScreen)
        {
            interval = 28f;
        }
        transform.position = new Vector3(transform.position.x, transform.position.y, interval);
    }
    private void OnLongPress()
    {
        held = true;
        onLongPress.Invoke();
    }

    internal void ResetEs()
    {
        if (isPlayer)
            DisableGlow();
        ncCounterUse = 0;
        FillElemental(0);
    }
}
