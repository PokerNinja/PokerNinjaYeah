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
    }
    public IEnumerator StartTimer(float timerDuration)
    {
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
    public IEnumerator ActiveTimer(bool enable, bool isPlayer)
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

    }

    private IEnumerator CountDown()
    {
        bool isPlayerTurn = BattleSystem.Instance.IsPlayerTurn();
        bool isLastSeconds = false;
        while (countTimer > 0 && updateTime)
        {

            if (isPlayerTurn && !isLastSeconds && countTimer < 10)
            {
                isLastSeconds = true;
                SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.LastSeconds, true);
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
                BattleSystem.Instance.OnTimeOut();
            }
        }
        yield break;
    }


 
}
