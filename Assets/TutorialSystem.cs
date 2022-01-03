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

    public const string tuto0 = "GET a better hand ranking than your opponents!";
    public const string tuto1 = "Each player has 2 coins. When you win, the other player lose 1 coin";
    public const string tuto2 = "Your energy. bla bla bli blo bla bla bla bolo";
    public const string tuto3 = "Every turn you get 1 Ninja card. Hold down your finger on the Ninja Card";
    public const string tuto4 = "This is the energy cost of this Ninja Card.\nYou can activate your ninja card on your turn, depends on your energy";
    public const string tuto30 = "You got pair of seven!";
    public const string tuto5 = "Freeze your '7' using your Ninja card.";
    public const string tuto6 = "Now - Click on your card '7'";
    public const string tuto7 = "Great job!";
    public const string tuto8 = "Now you have left 1 energy left to use. You can use it but maybe better save it." +
                                 "\nEnd your turn";
    /// <summary>
    ////////////////////////Starts Enemy First Turn 
    /// </summary>
    public const string tuto9 = "You can hold your finger on your hand to see your current hand and rank";
    public const string tuto10 = "Here you can see all hand rankings";
    /// <summary>
    /// ////////////////////Starts Player Second Turn
    /// </summary>
    public const string tuto11 = "With the cost of 2 energy, You can use flip to see the opponents card once per round.";
    public const string tuto12 = "Choose a card to flip";
    public const string tuto13 = "It’s Frozen. Choose the other card";
    public const string tuto14 = "A card with this symbol means its revealed";
    public const string tuto15 = "With the cost of 1 energy you can draw another ninja card";
    public const string tuto16 = "Got no energy left! The turn will pass automatically";
    public const string tuto17 = "When you finish your turn with 2 Ninja cards, the one from the left will be destroyd before you next turn.";
    /// <summary>
    /// ////////////////////Starts Enemy Second Turn
    /// </summary>
    public const string tuto18 = "Hold your finger on your ninja and drag it to select an Emoji";
    /// <summary>
    /// /////////////////// Player Last Turn
    /// </summary>
    public const string tuto19 = "In the final turn of the round, you get only 1 energy";
    public const string tuto20 = "Hold to read";
    public const string tuto21 = "Use the card to swap the enemy’s card.";
    public const string tuto22 = " Choose his '7' card and from the board - other than '7'.";
    public const string tuto23 = "You just UP’d your rank!";
    public const string tuto24 = "GOOD LUCK!";
    public static readonly string[] tutoInfo = { tuto0, tuto1, tuto2, tuto3, tuto4, tuto5, tuto6, tuto7, tuto8,
        tuto9,tuto10, tuto11, tuto12, tuto13, tuto14, tuto15, tuto16, tuto17, tuto18,tuto19,tuto20,tuto21,tuto22,tuto23,tuto24,
    "","","","","",tuto30};

    public TutorialSystem(BattleSystem battleSystem, bool enableFocus, int maskShape, int objectNumber, bool endByBtn) : base(battleSystem)
    {
        ui = battleSystem.tutorialUi;
        Action EndFocus = null;
        switch (objectNumber)
        {
            case 0:
                battleSystem.ActivatePlayerButtons(false, false);
                break;
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
            case 8: // End First Turn
               // ui.ScaleDownMask(2);
                ui.MoveCardsMaskPlayer(false, null);
                
                EndFocus = () => BattleSystem.Instance.ActivateButtonForTutorial(8);
                EndFocus += () => ui.ScaleMaskForEndFirstTurn();
                break;
            case 11: //flip skill
                ui.tutorialText.transform.position = new Vector3(-0.843f, -0.4f, 10f);
                //  ui.tutorialText.transform.position = new Vector3(-0.843f,2.45f,-80f);
                battleSystem.cardsDeckUi.enemyCardsUi[1].freeze = false;
                EndFocus = () => BattleSystem.Instance.ActivateButtonForTutorial(3);
                break;
            case 12: //flip skill chooose 
                ui.MoveCardsMaskEnemy(true, null);
                battleSystem.puDeckUi.playerSkillUi.spriteRenderer.sortingOrder = 1;
                battleSystem.cardsDeckUi.enemyCardsUi[1].spriteRenderer.sortingOrder = 35;
                battleSystem.cardsDeckUi.enemyCardsUi[0].spriteRenderer.sortingOrder = 1;
                break;
            case 13: //flip skill frozen pick
               BattleSystem.Instance.StartCoroutine(AnimationManager.Instance.Shake(battleSystem.cardsDeckUi.enemyCardsUi[1].spriteRenderer.material, Values.Instance.disableClickShakeDuration));
                ui.ScaleDownMask(1);
                battleSystem.cardsDeckUi.enemyCardsUi[1].freeze = true;
                battleSystem.cardsDeckUi.enemyCardsUi[1].spriteRenderer.sortingOrder = 1;
                battleSystem.cardsDeckUi.enemyCardsUi[0].spriteRenderer.sortingOrder = 35;
                battleSystem.cardsDeckUi.enemyCardsUi[0].SetSelection(true, "f", "f");
                battleSystem.cardsDeckUi.enemyCardsUi[0].cardMark.GetComponent<SpriteRenderer>().sortingOrder = 36;
                battleSystem.cardsDeckUi.enemyCardsUi[0].clickbleForPU = true;
                battleSystem.cardsToSelectCounter = 1;
                break;
            case 14: //flip skill choose normal
                ui.MoveCardsMaskEnemy(false, null);
                battleSystem.cardsDeckUi.enemyCardsUi[0].spriteRenderer.sortingOrder = 1;
                break;
            case 15: //noEnergyLeft
                break;
            case 16:// auto end
                battleSystem.Interface.btnReplaceRenderer.sortingOrder = 1;
                break;
            case 17:// auto end
                if (!enableFocus)
                {
                    battleSystem.continueTutorial = true;
                    EndFocus = () => BattleSystem.Instance.FakePlayerEndTurn();
                }
                break;
            case 18:
                if (enableFocus)
                {
                    EndFocus = () => ui.ScaleMaskForEmojis();
                }
                else
                {
                    battleSystem.Interface.updateTutorial = true;
                    EndFocus = () => battleSystem.FakeEnemyEndTurn();
                    EndFocus += () => battleSystem.enemyPuIsRunning = false;
                }
                break;
            case 19:
                EndFocus = () => BattleSystem.Instance.ChargeEnergyCounter(1);
                break;
            case 20:
                EndFocus = () => BattleSystem.Instance.ActivateButtonForTutorial(4);
                break;
            case 21:
                EndFocus = () => BattleSystem.Instance.ActivateButtonForTutorial(5);
                break;

        }
        FocusOnObjectWithText(enableFocus, maskShape, objectNumber, endByBtn, EndFocus);
    }

    internal void FocusOnObjectWithText(bool enable, int maskShape, int objectNumber, bool endByBtn, Action EndAction)
    {
        Debug.LogWarning("Focus " + objectNumber);
        SpriteRenderer spriteTarget;
        ui.btnTutorial.interactable = endByBtn;
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
                if(spriteTarget != null)
                {
                EndAction += () => spriteTarget.sortingOrder = 34;
                }
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
               EndAction?.Invoke();
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
            scaleMask = () => BattleSystem.Instance.StartCoroutine(AnimationManager.Instance.ScaleObjectRatio(targetMaskScale, duration, mask.transform, null, null));
        }
        fadeFont = () => BattleSystem.Instance.StartCoroutine(AnimationManager.Instance.AlphaFontAnimation(ui.tutorialText, toVisible, duration, null));
        fadeBtn = () => AnimationManager.Instance.AlphaFade(endByBtn, ui.btnTutorialSprite, duration, null);
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


    private SpriteRenderer GetSpriteTargetForTutorial(int objectNumber)
    {
        switch (objectNumber)
        {

            case 1:
                return ui.coinsFocus;
            case 2:
            case 16:
            case 19:
                return battleSystem.Interface.energyLeft[1].spriteRenderer;
            case 3:
            case 5:
            case 20:
            case 21:
                return battleSystem.puDeckUi.playerPusUi[0].spriteRenderer;
            case 4:
                return ui.energyCostTuto;
            case 6:
            case 7:
                return battleSystem.cardsDeckUi.playerCardsUi[0].spriteRenderer;
            case 8:
                return battleSystem.Interface.turnBtnSpriteREnderer;
            case 9:
                return ui.visionBtn;
            case 10:
                return ui.handRankMenu;
            case 11:
                return battleSystem.puDeckUi.playerSkillUi.spriteRenderer;
            case 12:
                return battleSystem.cardsDeckUi.enemyCardsUi[1].spriteRenderer;
            case 13:
                return battleSystem.cardsDeckUi.enemyCardsUi[0].spriteRenderer;
            case 14:
                return ui.eyeRenderer;
            case 15:
                return battleSystem.Interface.btnReplaceRenderer;
            case 17:
                return battleSystem.puDeckUi.playerPusUi[1].spriteRenderer;
            case 18:
                return battleSystem.Interface.emojiSelector;
        }
        return null;
    }

    private enum MaskShape
    {
        noMask = 0,
        roundMask = 1,
        rectMask = 2,
    }
}
