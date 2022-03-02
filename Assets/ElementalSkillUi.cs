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
    public void FullEffect(bool enableLoop)
    {
        glowRenderer.color = new Color(1f, 1f, 1f);
        liquidAnimation.Play();
        shineAnimation.Play("es_full_glow");
        if (enableLoop)
        {
            StartCoroutine(EnableGlowLoop());
        }
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
                targetY = -4.23f;
                break;
        }
        if (amount != 0)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EsFill, true);
        }
        StartCoroutine(AnimationManager.Instance.SimpleSmoothMove(pElementSkillLiquid.transform, 0.6f,
            new Vector3(pElementSkillLiquid.transform.position.x, targetY, 88.8f), Values.Instance.elementalSkillFillDuration, null, () =>
            {
                if (ncCounterUse == 3)
                {
                    FullEffect(true);
                    SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EsFull, true);
                }
            }));
    }

    public bool IsNcEqualsPesElement(string element)
    {
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
        {
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
        if (isPlayer)
        {
            pElementalSkillLiquid.color = liquidColor;
            glowColor = liquidColor;
        }
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
                BattleSystem.Instance.OnEsClick(this);
                FullEffect(false);
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
            BattleSystem.Instance.ShowPuInfo(transform.position, true, false, element + "p", "");
        }
        else if (Constants.TUTORIAL_MODE && !BattleSystemTuto.Instance.infoShow)
        {
            // BattleSystemTuto.Instance.ShowPuInfo(transform.position, false, element + "p", "");
        }
    }

    [Button]
    public void EnableSelecetPositionZ(bool aboveDarkScreen)
    {
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
        FillElemental(0);
        ncCounterUse = 0;
    }
}
