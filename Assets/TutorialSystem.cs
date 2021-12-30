using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TutorialSystem : State
{
    /*public GameObject tutorialMaskRing;
    public GameObject tutorialMaskRect;
    public SpriteRenderer darkScreenRenderer;
    public SpriteRenderer btnTutorial;
    public SpriteRenderer tutorialBgText;
    public TextMeshProUGUI tutorialText;
    public int lastObjectToFocus;
    public SpriteRenderer coinsFocus;
    public SpriteRenderer energyCostTuto;*/


    private TutorialUi ui;

    public const string tuto1 = "GET a better hand ranking than your opponents!";
    public const string tuto2 = "Each player has 2 coins. When you win, the other player lose 1 coin";
    public const string tuto3 = "Your energy. bla bla bli blo bla bla bla bolo";
    public const string tuto4 = "Every turn you get 1 Ninja card. Hold down your finger on the Ninja Card";
    public const string tuto5 = "This is the energy cost of this Ninja Card.\nYou can activate your ninja card on your turn, depends on your energy";
    public const string tuto6 = "You got pair of seven! Freeze your '7' using your ninja card";
    public const string tuto7 = "Now - Click on your card '7'";
    public const string tuto8 = "Great job!";
    public const string tuto9 = "Now you have left 1 energy left to use. You can use it but maybe better save it." +
                                 "\nEnd your turn";
    public static readonly string[] tutoInfo = { tuto1, tuto2, tuto3, tuto4, tuto5, tuto6, tuto7, tuto8, tuto9 };

    public TutorialSystem(BattleSystem battleSystem, bool enableFocus, int maskShape, int objectNumber, bool endByBtn) : base(battleSystem)
    {
        ui = battleSystem.tutorialUi;
        Action EndFocus = null;
        switch (objectNumber)
        {
            case 2: //energy
                EndFocus = () => BattleSystem.Instance.ChargeEnergyCounter(2);
                break;
            case 3: //puInfo
                EndFocus = () => BattleSystem.Instance.ActivateButtonForTutorial(4);
                break;
            case 5: // 
                EndFocus = () => BattleSystem.Instance.ActivateButtonForTutorial(5);
                // ui.ScaleDownMask(2);
                break;
            case 6: // 
                GetSpriteTargetForTutorial(5).sortingOrder = 1;
                // ui.ScaleDownMask(2);
                break;
            case 7: // after cardselect
                GetSpriteTargetForTutorial(6).sortingOrder = 1;
                break;
            case 8: //
                ui.ScaleDownMask(2);
                ui.MoveCardsMask(false, null);
                battleSystem.Interface.energyLeft[0].spriteRenderer.sortingOrder = 34;
                EndFocus = () => BattleSystem.Instance.ActivateButtonForTutorial(8);
                break;
        }
        FocusOnObjectWithText(enableFocus, maskShape, objectNumber, endByBtn, EndFocus);
    }

    internal void FocusOnObjectWithText(bool enable, int maskShape, int objectNumber, bool endByBtn, Action EndAction)
    {
        Debug.LogWarning("Focus " + objectNumber);
        SpriteRenderer spriteTarget;
        spriteTarget = GetSpriteTargetForTutorial(objectNumber);
        GameObject mask = null;

        if (enable)
        {
            ui.lastObjectToFocus = objectNumber;
            if (maskShape != MaskShape.noMask.GetHashCode())
            {
                mask = InitMask(maskShape, spriteTarget.transform.position);
            }
            if (!endByBtn)
            {
                EndAction += () => spriteTarget.sortingOrder = 34;
            }
            ui.tutorialText.text = tutoInfo[objectNumber];
            AnimateTutoObjects(true, maskShape, endByBtn, EndAction);
        }
        else
        {
            AnimateTutoObjects(false, maskShape, endByBtn, () =>
           {
               if (!endByBtn)
               {
                   if (spriteTarget != null)
                   {
                       spriteTarget.sortingOrder = 0;
                   }
               }
           });

        }
    }

    private GameObject InitMask(int maskShape, Vector3 position)
    {
        GameObject mask;
        if (maskShape == MaskShape.rectMask.GetHashCode())
        {
            mask = ui.tutorialMaskRect;
        }
        else
        {
            mask = ui.tutorialMaskRing;
        }
        mask.transform.position = position;
        mask.transform.localScale = new Vector2(0.001f, 0.001f);
        return mask;
    }
    private void AnimateTutoObjects(bool toVisible, int maskShape, bool endByBtn, Action EndAction)
    {
        GameObject mask = ui.tutorialMaskRing;
        float targetMaskScaleLarge = 2000f;
        float targetMaskScaleSmall = 0.001f;
        float duration = Values.Instance.tutoObjsFadeDuration;
        Action scaleMask = null;
        Action fadeFont = null;
        Action fadeDarkScreen = null;
        Action fadeBtn = null;
        if (maskShape != MaskShape.noMask.GetHashCode())
        {
            if (maskShape == MaskShape.rectMask.GetHashCode())
            {
                mask = ui.tutorialMaskRect;
                targetMaskScaleLarge = 300f;
                targetMaskScaleSmall = 0.0033333f;
            }
            float targetMaskScale = targetMaskScaleSmall;
            if (toVisible)
            {
                targetMaskScale = targetMaskScaleLarge;
            }
            scaleMask = () => BattleSystem.Instance.StartCoroutine(AnimationManager.Instance.ScaleObject(targetMaskScale, duration, mask.transform, null, null));
        }
        fadeFont = () => BattleSystem.Instance.StartCoroutine(AnimationManager.Instance.AlphaFontAnimation(ui.tutorialText, toVisible, duration, null));
        fadeBtn = () => AnimationManager.Instance.AlphaFade(endByBtn, ui.btnTutorial, duration, null);
        fadeDarkScreen = () => AnimationManager.Instance.FadeBurnDarkScreen(ui.darkScreenRenderer.material, toVisible, duration / 2, null);


        if (toVisible)
        {
            fadeFont += fadeBtn += scaleMask += EndAction;
            AnimationManager.Instance.FadeBurnDarkScreen(ui.darkScreenRenderer.material, toVisible, duration, fadeFont);
        }
        else
        {
            fadeBtn?.Invoke();
            scaleMask?.Invoke();
            fadeDarkScreen += EndAction;
            BattleSystem.Instance.StartCoroutine(AnimationManager.Instance.AlphaFontAnimation(ui.tutorialText, toVisible, duration / 2, fadeDarkScreen));
        }

    }
    /* internal void FocusOnObjectWithText2(bool dontFadeViews, bool enable, int maskShape, int objectNumber, bool endByBtn)
     {
         SpriteRenderer spriteTarget;
         spriteTarget = GetSpriteTargetForTutorial(objectNumber);
         GameObject mask = null;
         if (maskShape != MaskShape.noMask.GetHashCode())
         {
             if (maskShape == MaskShape.rectMask.GetHashCode())
             {
                 mask = ui.tutorialMaskRect;
             }
             else
             {
                 mask = ui.tutorialMaskRing;
             }

         }
         if (enable)
         {
             ui.lastObjectToFocus = objectNumber;
             BattleSystem.Instance.continueTutorial = false;

             TutorialObjectsVisible(true, true, maskShape, endByBtn);
             AnimateTutoObjects(true, maskShape, endByBtn, dontFadeViews, null);

             if (maskShape != MaskShape.noMask.GetHashCode())
             {
                 mask.transform.position = spriteTarget.transform.position;
                 mask.transform.localScale = new Vector2(0.01f, 0.01f);
             }
             if (!endByBtn)
             {
                 // spriteTarget.sortingOrder = 10;
             }
             ui.tutorialText.text = tutoInfo[objectNumber];
         }
         else
         {
             if (objectNumber == Constants.TutorialObjectEnum.pu.GetHashCode())
             {
                 ui.tutorialMaskRect.SetActive(false);
                 FocusOnObjectWithText(false, true, maskShape, Constants.TutorialObjectEnum.puCost.GetHashCode(), true);
             }
             else
             {
                 AnimateTutoObjects(false, maskShape, endByBtn, false, () =>
                 {
                     TutorialObjectsVisible(false, false, maskShape, false);
                     if (!endByBtn)
                     {
                         spriteTarget.sortingOrder = 0;
                     }
                     BattleSystem.Instance.continueTutorial = true;
                 });
             }
         }
     }
 */

    /*private void AnimateTutoObjects2(bool toVisible, int maskShape, bool endByBtn, bool jusjAddBtn, Action EndAction)
    {
        GameObject mask = ui.tutorialMaskRing;
        float targetMaskScaleLarge = 200f;
        float targetMaskScaleSmall = 0.01f;
        float duration = Values.Instance.tutoObjsFadeDuration;
        Action scaleMask = null;
        if (maskShape != MaskShape.noMask.GetHashCode())
        {
            if (maskShape == MaskShape.rectMask.GetHashCode())
            {
                mask = ui.tutorialMaskRect;
                targetMaskScaleLarge = 30f;
                targetMaskScaleSmall = 0.033333f;
            }
            scaleMask = () => BattleSystem.Instance.StartCoroutine(AnimationManager.Instance.ScaleObject(targetMaskScaleLarge, duration, mask.transform, null, null));
        }

        scaleMask += () => BattleSystem.Instance.StartCoroutine(AnimationManager.Instance.AlphaFontAnimation(ui.tutorialText, toVisible, duration, null));
        Action FadeAllObjects = () => AnimationManager.Instance.FadeBurnDarkScreen(ui.darkScreenRenderer.material, toVisible, duration , () => scaleMask?.Invoke());
        //FadeAllObjects += () => BattleSystem.Instance.StartCoroutine(AnimationManager.Instance.AlphaFontAnimation(ui.tutorialText, toVisible, duration, null));
        if (endByBtn || (!toVisible && ui.btnTutorial.color.a == 1))
        {
            FadeAllObjects += () => AnimationManager.Instance.AlphaFade(toVisible, ui.btnTutorial, duration, EndAction);
        }

        if (toVisible)
        {
            if (!jusjAddBtn)
            {
                FadeAllObjects.Invoke();
            }
            else
            {
                //HOW To HANDLE
                mask.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                BattleSystem.Instance.StartCoroutine(AnimationManager.Instance.ScaleObject(0.033333f, duration, ui.tutorialMaskRect.transform, null, scaleMask));
            }
        }
        else
        {
            //HOW To HANDLE
            scaleMask = null;
            BattleSystem.Instance.StartCoroutine(AnimationManager.Instance.ScaleObject(targetMaskScaleSmall, duration, mask.transform, null, FadeAllObjects));
        }

    }*/

    private void TutorialObjectsVisible(bool enable, bool effectText, int maskShape, bool endByBtn)
    {
        if (effectText)
        {
            ui.tutorialText.enabled = enable;
            ui.tutorialText.gameObject.SetActive(enable);
        }
        if (maskShape != MaskShape.noMask.GetHashCode())
        {
            if (maskShape == MaskShape.rectMask.GetHashCode())
            {
                ui.tutorialMaskRect.SetActive(enable);
            }
            else
            {
                ui.tutorialMaskRing.SetActive(enable);
            }
        }

        ui.btnTutorial.transform.gameObject.SetActive(endByBtn);
    }

    private SpriteRenderer GetSpriteTargetForTutorial(int objectNumber)
    {
        switch (objectNumber)
        {

            case 1:
                return ui.coinsFocus;
            case 2:
                return battleSystem.Interface.energyLeft[1].spriteRenderer;
            case 3:
            case 5:
                return battleSystem.puDeckUi.playerPusUi[0].spriteRenderer;
            case 4:
                return ui.energyCostTuto;
            case 6:
            case 7:
                return battleSystem.cardsDeckUi.playerCardsUi[0].spriteRenderer;
            case 8:
                return battleSystem.Interface.turnBtnSpriteREnderer;
        }
        return null;
    }

    /*   public void ContinueWithTutorial()
       {
           FocusOnObjectWithText(false, false, false, ui.lastObjectToFocus, false);
       }*/

    private enum MaskShape
    {
        noMask = 0,
        roundMask = 1,
        rectMask = 2,
    }
}
