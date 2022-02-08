using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    [SerializeField] private Image countdownCircleTimer;
    [SerializeField] private float countTimer ;
    private bool endTurnByPlayer;
    public SpriteRenderer turnArrowSpriteRenderer;
    public Animation turnArrowAnimation;

    //  [SerializeField] private float countTimer;
    private bool updateTime;
    private bool countDownRunning;
    private bool isPlayer;
    private float totalTime;
    private int starting;
    private Coroutine thereCanBeOnlyOne;

    // public ITimeOut iTimeOut { get; set; }


    public void StopTimer()
    {
        endTurnByPlayer = true;
        updateTime = false;
        countTimer = -10f;
        countdownCircleTimer.fillAmount = 1.0f;
        thereCanBeOnlyOne = null;
        ApplyIndicatorArrow(false, false);

    }

    private void ApplyIndicatorArrow(bool enable, bool isPlayerTurn)
    {
        turnArrowSpriteRenderer.gameObject.SetActive(enable);
        turnArrowSpriteRenderer.gameObject.SetActive(enable);
        if (enable)
        {
            FlipImage(isPlayerTurn);
            turnArrowAnimation.Play();
        }
        else
        {
            turnArrowAnimation.Stop();
        }
    }

    private void FlipImage(bool isPlayer)
    {
        Vector3 parentArrowScale = turnArrowSpriteRenderer.transform.parent.transform.localScale;
        float addition = 1; // enemyTurn
        if (isPlayer)
        {
            addition = -1;
        }
        Vector3 newScale = new Vector3(parentArrowScale.x,  addition);
        turnArrowSpriteRenderer.transform.parent.transform.localScale = newScale;
    }

    public IEnumerator StartTimer(float timerDuration)
    {
        SetCounterColor(false);
        yield return new WaitForSeconds(Values.Instance.delayTimerStart);
        endTurnByPlayer = false;
        updateTime = true;
        totalTime = timerDuration;
        countTimer = totalTime;
        if (thereCanBeOnlyOne == null)
        {
            thereCanBeOnlyOne = StartCoroutine(CountDown());
        }
        else
        {

        }

    }
    /*public IEnumerator ActiveTimer(bool enable, bool isPlayer)
    {
        this.isPlayer = isPlayer;
        if (enable)
        {
            if (updateTime)
            {
                updateTime = false;
            }
            countTimer = totalTime;
            yield return new WaitForSeconds(2);
            if (thereCanBeOnlyOne == null)
            {
                StartCoroutine(CountDown());
            }
        }
        else
        {
            updateTime = false;
            countTimer = 0;
            countdownCircleTimer.fillAmount = 1.0f;
            thereCanBeOnlyOne = null;
            StopCoroutine(CountDown());
        }

    }*/

    private IEnumerator CountDown()
    {
        bool isPlayerTurn = BattleSystem.Instance.IsPlayerTurn();
        ApplyIndicatorArrow(true, isPlayerTurn);
        bool isLastSeconds = false;
        while (countTimer > 0 && updateTime)
        {

            if (isPlayerTurn && !isLastSeconds && countTimer < 10)
            {
                isLastSeconds = true;
                SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.LastSeconds, true);
                SetCounterColor(true);
            }
            countTimer -= Time.deltaTime;
            float normalizedValue = Mathf.Clamp(
            countTimer / totalTime, 0.0f, 1.0f);
            countdownCircleTimer.fillAmount = normalizedValue;
            /*if (countTimer <= 0 || !updateTime)
            {
                if (isLastSeconds)
                {
                    SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.LastSeconds, false);
                }
                if (!endTurnByPlayer)
                {
                    BattleSystem.Instance.OnTimeOut();
                }
                break;
            }*/
            yield return null; //Don't freeze Unity
        }
        if (countTimer <= 0 || !updateTime)
        {
            if (isLastSeconds)
            {
                SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.LastSeconds, false);
            }
            if (!endTurnByPlayer)
            {
               StartCoroutine(BattleSystem.Instance.OnTimeOut());
            }
        }
        yield break;
    }

    private void SetCounterColor(bool enable)
    {
        Color targetColor ;
        if (enable)
        {
            targetColor = new Color(0.921f, 0.235f, 0.219f);
        }
        else
        {
            targetColor = new Color(1f, 1f, 1f);
        }
        countdownCircleTimer.color = targetColor;
        turnArrowSpriteRenderer.color = targetColor;
    }
}
