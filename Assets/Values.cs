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
    public float turnTimerDuration = 40f;
    public float sfxVolume = 0.9f;
    public float musicVolume = 0.6f;
    public float lowPitchRange = 0.95f;
    public float highPitchRange = 1.05f;
    public float delayBeforeStartNewRound =2f;
    public float delayBeforeStartFirstRound = 1f;
    public int delayTimerStart = 3;
    public int skillUseLimit = 1;
    public GamePhase resetSkillEvery = GamePhase.Round;
    // public int replaceUseLimit = 1;
    //    public GamePhase resetReplaceEvery = GamePhase.Turn;

    //[GUIColor(0, 1, 0)]
    [Title("Effects")]
    public float puProjectileMoveDuration = 2f;
    public float puProjectileFadeOutDuration = 0.4f;
    public float windMoveDuration = 0.3f;
    public float windFadeOutDuration =1f;

    public float winningCardProjectileMoveDuration = 1f;

    public Transform winCardRightStart;
    public Transform winCardLeftStart;
    public Transform winCardRightEnd;
    public Transform winCardLeftEnd;

    public Color[] visionColorsByRank;
    public Color currentVisionColor;


    //[GUIColor(1, 1, 0)]
    [Title("Animation")]
    [Title("General", bold: false)]
    public float objectMoveDuration = 2f;
    public float pulseDuration = 1f;
    public float rankInfoMoveDuration = 1f;
    public float darkScreenAlphaDuration = 0.4f;
    public float shineDuration =0.3f;
    public int coinShineEvery = 3;
    public float infoDialogFadeOutDuration = 0.2f;
    public float showDialogMoveDuration = 0.5f;

    [Title("CardUi", bold: false)]
    public Color cardSelectionColor ;
    public float FreezeDuration =3f ;
    public float cardDrawMoveDuration = 2f;
    public float cardSwapMoveDuration = 2f;
    public float markOnCardScaleAlphaDuration = 3f;
    public float cardFlipDuration = 0.45f;
    public float cardBurnDuration = 1f;
    public float delayBetweenHandsAndFlopDeal = 0.6f;
    public float delayBetweenDealPlayersCards = 0.18f;
    public float delayBetweenDealBoardCards = 0.18f;

    [Title("PowerUpUi", bold: false)]
    public float puPushNewSlotMoveDuration = 2f;
    public float puDrawMoveDuration = 2f;
    public float puDissolveDuration = 4f;
    public float enrgyDissolveDuration = 1.2f;
    public float puChangeColorDisableDuration =1f;
    public int puShineEvery = 3;
    public float floatingShakeAnimationSpeed = 0.5f;
    public float floatingShakeAnimationX = 0.55f;
    public float floatingShakeAnimationY = 0.12f;

    [Title("Turn", bold: false)]
    public float turnBtnAlphaDuration = 1f;
    public float turnTextMoveDuration = 1.9f;
    public float turnTextScaleDuration = 1.3f;
    public float drawerMoveDuration = 0.9f;
    public float textTurnFadeOutDuration = 1f;
    public float turnIndicatorFadeDuration = 1.2f;

    [Title("End", bold: false)]
    public float winningCardsMoveDuration =4f;
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





    public enum GamePhase
    {
        Turn,
        Round,
        Game,
    }
}
