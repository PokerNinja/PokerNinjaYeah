using MyBox;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Values : Singleton<Values>
{
    [GUIColor(0.3f, 0.8f, 0.8f)]
    [Title("Settings")]
    public bool TEST_MODE = false;
    public bool TUTORIAL_MODE = false;
    public bool flusherOn = false;
    public bool strighterOn = false;
    public float turnTimerDuration = 40f;
    public float sfxVolume = 0.9f;
    public float musicVolume = 0.6f;
    public float lowPitchRange = 0.95f;
    public float highPitchRange = 1.05f;
    public float delayBeforeStartNewRound = 2f;
    public float delayBeforeStartFirstRound = 1f;
    public int delayTimerStart = 3;
    public int skillUseLimit = 1;
    public GamePhase resetSkillEvery = GamePhase.Round;
    [GUIColor(0.176f, 1f, 0.130f)]
    public int energyCostForDraw = 1;
    public float tutoObjsFadeDuration = 0.7f;
    public float fontAppearance = 0.7f;
    public int ChanceForBotEmoji = 3;
    public float raiseTimerDuration = 30f;

    // public int replaceUseLimit = 1;
    //    public GamePhase resetReplaceEvery = GamePhase.Turn;

    //[GUIColor(0, 1, 0)]
    [Title("Effects")]
    public float puProjectileMoveDuration = 2f;
    public float puProjectileFadeOutDuration = 0.4f;
    public float windMoveDuration = 0.3f;
    public float windFadeOutDuration = 1f;
    public float windRorationSpeed = 2f;
    public float circularRadiusMove = 1f;
    public float circularMoveDuration = 1f;
    public float circualScaleMultiplication = 1.25f;

    public float winningCardProjectileMoveDuration = 1f;

    public Transform winCardRightStart;
    public Transform winCardLeftStart;
    public Transform winCardRightEnd;
    public Transform winCardLeftEnd;

    public Color[] visionColorsByRank;
    public Color currentVisionColor;
    public Color ghostOutlineColor;
    public float fadeFlusherDuration = 5f;
    public float durationGlitchBeforeChange = 2f;
    public float durationGlitchAfterChange = 2f;
    public float coinFlipEndMoveDuration = 1f;
    public float tutoMaskFadeLoopDuration = 0.75f;
    public float delayBetweenProjectiles = 0.25f;
    public float defaultFadeD = 1f;
    public float inAndOutAnimation = 2f;
    public Color brightRed ;
    public Color darkRed ;
    public Color yellow;


    [Title("Ninja")]
    [Title("Frames", bold: false)]
    public Color woodFrameColor;
    public Color greenFrameColor;
    public Color dojoFrameColor;
    public Color spaceFrameColor;


    [Title("NCvision")]
    [Title("Elements", bold: false)]
    public Color fireVision;
    public Color iceVision;
    public Color windVision;
    public Color shadowVision;
    public Color electricVision;


    [Title("Text")]
    [Title("Colors", bold: false)]
    public string redText = "#F03B37";
    public string blueText = "#02C8FF";
    public string yellowText = "#FFC35E";



    [GUIColor(1, 1, 0)]
    [Title("Animation")]
    [Title("General", bold: false)]
    public float objectMoveDuration = 2f;
    public float pulseDuration = 1f;
    public float rankInfoMoveDuration = 1f;
    public float darkScreenAlphaDuration = 0.4f;
    public float shineDuration = 0.3f;
    public int coinShineEvery = 3;
    public float infoDialogFadeOutDuration = 0.2f;
    public float showDialogMoveDuration = 0.5f;
    public float hpScaleDuration = 1f;

    [Title("Emojis", bold: false)]
    public float holdTimeForEmojiSelector = 0.25f;
    public float emojiMenuFadeDuration = 0.5f;
    public float emojiDisplayFadeDuration = 0.15f;
    public float emojiStay = 3f;
    public float emojiCoolDown = 5f;

    [Title("CardUi", bold: false)]
    public Color cardSelectionColor;
    public float drawerMoveDuration = 0.9f;
    public float FreezeDuration = 3f;
    public float cardDrawMoveDuration = 2f;
    public float cardSwapMoveDuration = 2f;
    public float markOnCardScaleAlphaDuration = 3f;
    public float cardFlipDuration = 0.45f;
    public float cardBurnDuration = 1f;
    public float delayBetweenHandsAndFlopDeal = 0.6f;
    public float delayBetweenDealPlayersCards = 0.18f;
    public float delayBetweenDealBoardCards = 0.18f;
    public float freezeMaterialFadeBurnWidth = 0.56f;

    [Title("PowerUpUi", bold: false)]
    public float pusDrawerMoveDuration = 2f;
    public float puPushNewSlotMoveDuration = 2f;
    public float puDrawMoveDuration = 2f;
    public float puDissolveDuration = 4f;
    public float enrgyDissolveDuration = 1.2f;
    public float puChangeColorDisableDuration = 1f;
    public int puShineEvery = 3;
    public float floatingShakeAnimationSpeed = 0.5f;
    public float floatingShakeAnimationX = 0.55f;
    public float floatingShakeAnimationY = 0.12f;

    [Title("Turn", bold: false)]
    public float turnBtnAlphaDuration = 1f;
    public float turnTextMoveDuration = 1.9f;
    public float turnTextScaleDuration = 1.3f;
    public float textTurnFadeOutDuration = 1f;
    public float turnIndicatorFadeDuration = 1.2f;

    [Title("End", bold: false)]
    public int delayBetweenCardWinArrangeInMilli = 50;
    public float winningCardsMoveDuration = 1f;
    public float LoseLifeDuration = 1f;
    public float bgPulseColorSwapDuration = 1f;
    public float bgMaxValueSwapColor = 0.67f;
    public float hitTextScaleDuration = 2f;
    public float hitTextFDuration = 0.5f;

    [Title("ShakeDisableAnimation", bold: false)]
    public float disableClickShakeSpeed = 1.8f;
    public float disableClickShakeX = 1f;
    public float disableClickShakeY = 0.05f;
    public float disableClickShakeDuration = 0.3f;
    public float disableClickShakeDurationForEmoji = 1f;




    public float test1 = 0f;
    public float test2 = 0.5f;
    public float test3 = 0f;
    public enum GamePhase
    {
        Turn,
        Round,
        Game,
    }
}
