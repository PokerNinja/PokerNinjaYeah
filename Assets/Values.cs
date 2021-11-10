using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Values : Singleton<Values>
{
    public bool TEST_MODE = false;
    public float objectMoveDuration;
    public float FreezeDuration =3 ;
    public float LoseLifeDuration = 1f;
    public float turnTextMoveDuration = 1.9f;
    public float turnTextScaleDuration = 1.3f;
    public float winningCardsMoveDuration =4f;
    public float cardDrawMoveDuration = 2f;
    public float cardSwapMoveDuration = 2f;
    public float puPushNewSlotMoveDuration = 2f;
    public float puDrawMoveDuration = 2f;
    public float drawerMoveDuration = 0.9f;
    public float rankInfoMoveDuration = 1f;
    public float darkScreenAlphaDuration = 0.4f;
    public float winningCardProjectileMoveDuration = 1f;
    public float disableClickShakeSpeed = 1.8f;
    public float disableClickShakeX = 1f;
    public float disableClickShakeY = 0.05f;
    public float disableClickShakeDuration = 0.3f;


    public float floatingShakeAnimationSpeed = 0.5f;
    public float floatingShakeAnimationX = 0.55f;
    public float floatingShakeAnimationY = 0.12f;

    public Transform winCardRightStart;
    public Transform winCardLeftStart;
    public Transform winCardRightEnd;
    public Transform winCardLeftEnd;
    
}
