using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RaiseTimer : MonoBehaviour
{

    [SerializeField] private Image countdownCircleTimer;
    [SerializeField] private float countTimer;

    private bool updateTime;
    private bool isPlayer;
    private float totalTime;
    private int starting;
    private Coroutine thereCanBeOnlyOne;
    private bool isLastSeconds;


    public void StopTimer()
    {
        SetCounterColor(false);
        updateTime = false;
        countTimer = -10f;
        countdownCircleTimer.fillAmount = 1.0f;
        thereCanBeOnlyOne = null;
        if (isLastSeconds)
        {
            Debug.LogError("coubter" + countTimer);
            SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.LastSeconds, false);
        }
    }


    public IEnumerator StartTimer(float timerDuration)
    {
        yield return new WaitForSeconds(Values.Instance.delayTimerStart);
        updateTime = true;
        totalTime = timerDuration;
        countTimer = totalTime;
        if (thereCanBeOnlyOne == null)
        {
            thereCanBeOnlyOne = StartCoroutine(CountDown());
        }
    }


    private IEnumerator CountDown()
    {
        bool isEnemyTurn = !BattleSystem.Instance.IsPlayerTurn();
        isLastSeconds = false;
        while (countTimer > 0 && updateTime)
        {
            if (isEnemyTurn && !isLastSeconds && countTimer < 5)
            {
                isLastSeconds = true;
                SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.LastSeconds, true);
                SetCounterColor(true);
            }
            countTimer -= Time.deltaTime;
            float normalizedValue = Mathf.Clamp(
            countTimer / totalTime, 0.0f, 1.0f);
            countdownCircleTimer.fillAmount = normalizedValue;

          
            yield return null; //Don't freeze Unity
        }
        if (countTimer <= 0 || !updateTime)
        {
            Debug.LogError("about toStop");
            if (isEnemyTurn)
            {
            Debug.LogError("StopTimer");
                SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.LastSeconds, false);
                BattleSystem.Instance.RefuseBet();
            }

        }
        yield break;
    }

    private void SetCounterColor(bool enable)
    {
        Color targetColor;
        if (enable)
        {
            targetColor = new Color(0.921f, 0.235f, 0.219f);
        }
        else
        {
            targetColor = new Color(1f, 1f, 1f);
        }
        countdownCircleTimer.color = targetColor;
    }

}
