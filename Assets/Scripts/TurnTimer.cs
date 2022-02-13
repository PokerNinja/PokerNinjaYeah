using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class TurnTimer : MonoBehaviour, IPointerDownHandler, IPointerUpHandler

{

    public UnityEvent onClick = new UnityEvent();
    public UnityEvent onLongPress = new UnityEvent();
    [SerializeField] private Image countdownCircleTimer;
    [SerializeField] private float countTimer;
    private bool endTurnByPlayer;
    public TurnArrowUi turnArrowUi;
    // public SpriteRenderer turnArrowSpriteRenderer;
    // public Animation turnArrowAnimation;

    //  [SerializeField] private float countTimer;
    private bool updateTime;
    private bool pause;
    private float totalTime;
    private Coroutine thereCanBeOnlyOne;

    private float holdTime = 0.3f;
    private bool held = false;

    // public ITimeOut iTimeOut { get; set; }


    private void Start()
    {
        
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.LogWarning("cus UP");
        CancelInvoke("OnLongPress");

        if (!held)
        {
            onClick.Invoke();
        }
        else if (!Constants.TUTORIAL_MODE && BattleSystem.Instance.infoShow)
        {
            BattleSystem.Instance.HideDialog(false);
        }
        else if (Constants.TUTORIAL_MODE && BattleSystemTuto.Instance.infoShow)
        {
            BattleSystemTuto.Instance.HideDialog();
        }

    }


    public void OnPointerDown(PointerEventData eventData)
    {
        held = false;
        Invoke("OnLongPress", holdTime);
    }

    
  

    public void OND()
    {
        if (!Constants.TUTORIAL_MODE && !BattleSystem.Instance.infoShow)
        {
            BattleSystem.Instance.ShowPuInfo(transform.position, false, false, "end", Constants.ReplacePuInfo);
        }
        else if (Constants.TUTORIAL_MODE && !BattleSystemTuto.Instance.infoShow)
        {
            BattleSystemTuto.Instance.ShowPuInfo(transform.position, false, "end", Constants.ReplacePuInfo);
        }
    }

    public void StopTimer()
    {

        turnArrowUi.changeToRed = false;
        //turnArrowSpriteRenderer.material.SetFloat("_GradBoostX", 0.1f);
        SetCounterColor(false);
        endTurnByPlayer = true;
        updateTime = false;
        countTimer = -10f;
        countdownCircleTimer.fillAmount = 0f;
        thereCanBeOnlyOne = null;
        turnArrowUi.ApplyIndicatorArrow(false);

    }

    /*    private void ApplyIndicatorArrow(bool enable, bool isPlayerTurn)
        {
            //turnArrowSpriteRenderer.gameObject.SetActive(enable);
            turnArrowAnimation.Rewind();
            turnArrowAnimation.Play();
            turnArrowAnimation.Sample();
            turnArrowAnimation.Stop();
            // turnArrowSpriteRenderer.material.SetFloat("_GradBlend", 0.1f);
            if (enable)
            {
                //  FlipImage(isPlayerTurn);
                SetArrowColor(false);
                turnArrowAnimation.Play();
              //  turnArrowSpriteRenderer.material.SetFloat("_GradBlend", 1f);
            }
            else
            {
                turnArrowAnimation.Stop();
            }
        }*/




    public IEnumerator StartTimer(float timerDuration)
    {
        StartCoroutine(AnimateFillCounter());
        yield return new WaitForSeconds(Values.Instance.delayTimerStart);
        pause = false;
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

    private IEnumerator AnimateFillCounter()
    {
        float startTime = Time.time;
        float movementDuration = 1f;
        float t;

        while (countdownCircleTimer.fillAmount < 1f)
        {
            t = (Time.time - startTime) / movementDuration;
            countdownCircleTimer.fillAmount = Mathf.Lerp(0, 1, t);
            yield return null;
            if(countdownCircleTimer.fillAmount >= 1f)
            {
                break;
            }
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
    /*if (isPlayerTurn && !isLastSeconds && countTimer< 10)
           {
               isLastSeconds = true;
               SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.LastSeconds, true);
               SetCounterColor(true);
   float currentR = turnArrowSpriteRenderer.material.GetFloat("_GradBoostX");

               while (currentR >= 0.05f)
               {
                   yield return null;
                   currentR = turnArrowSpriteRenderer.material.GetFloat("_GradBoostX");
                   Debug.LogError("a:  " + currentR);
                   if (currentR <= 0.15f)
                   {
                       SetArrowColor(true);
                       break;
                   }
               }*/

 
    private IEnumerator CountDown()
    {
        bool isPlayerTurn = BattleSystem.Instance.IsPlayerTurn();
        turnArrowUi.ApplyIndicatorArrow(true);
        bool isLastSeconds = false;
        while (countTimer > 0 && updateTime)
        {

            if (isPlayerTurn && !isLastSeconds && countTimer < 10)
            {
                isLastSeconds = true;
                SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.LastSeconds, true);
                SetCounterColor(true);

            }
            if (!pause)
            {
                countTimer -= Time.deltaTime;
                float normalizedValue = Mathf.Clamp(
                countTimer / totalTime, 0.0f, 1.0f);
                countdownCircleTimer.fillAmount = normalizedValue;
            }
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

    /* private void SetArrowColor(bool toRed)
     {
         Color color = new Color(1f, 1f, 1f);
         if (toRed)
         {
             color = Values.Instance.brightRed;
         }
         turnArrowSpriteRenderer.material.SetColor("_GradBotLeftCol", color);
     }*/

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
        turnArrowUi.changeToRed = true;
    }

    internal void PauseTimer(bool enable)
    {
        pause = enable;
    }

    internal void Activate(bool enable)
    {
        gameObject.SetActive(enable);
    }
    private void OnLongPress()
    {
        held = true;
        onLongPress.Invoke();
    }

}
