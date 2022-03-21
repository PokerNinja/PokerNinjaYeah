using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject texasDialog;
    public GameObject texasTutorial;
    public CanvasGroup instructionsCanvas;
    public TextMeshProUGUI instructionsText;
    public GameObject blockScreen;
    public SpriteRenderer pokerTutoSpriteRenderer;
    private int pokerTutoPhase;
    public int step = 0;
    public Animation pointerAnimation1;
    public Animation pointerAnimation2;
    private const string colorNinja = "#1C8857";

    private string open1 = "Use your <color=" + colorNinja + ">Ninja-Card</color> to defeat your opponent!" +
        "\n\n*Hold your finger on your Ninja-Cards to see what they do.";
    private string noActionText2 = "Use your Ninja-Card to burn a card from your hand!";
    private string endYourTurn3 = "Press here to end your turn.";

    private string energyCharge4 = "You get 2 Energy per turn. This is your resource for actions, Use it wisely.";
    private string draw5 = "Here you can draw another NC - with the cost of 1 energy.";
    private string noActionText6 = "You still got energy left! You can use another NC, Draw or Pass the turn." +
        "\nRemember you can hold you finger on anything for more information.";
    private string dragonCard7 = "You got a rare dragon card! They cost 2 energy.";
    private string firstWin8 = "You damaged your opponent’s HP! abit more and you win!";
    private string raise9 = "you can offer your opponent to raise the DMG for this round." +
        "\nYour opponent lose HP if declines. you can use that to bluff!";
    private string lastTurn10 = "The dealer has the last move, but receives only 1 energy.";
    private string emojis11 = "You can send an Emoji to your opponent By holding your finger on your avatar and drag it to the emoji you want.";


    internal void SetStep(int step)
    {
        this.step = step;
        switch (step)
        {
            case 1: // Start with f1 - long click
                blockScreen.SetActive(true);
                InstructionsEnable(open1);
                SetPointer(pointerAnimation1, ObjectTargetEnum.nc1);
                break;
            case 2: // after long click
                InstructionsDisable();
                StartCoroutine(TimerForMsgAppear(noActionText2));
                break;
            case 3: // after f1 ignite
                timerForMsgEnable = false;
                SetObjectClickable(BattleSystem.Instance.Interface.turnTimer.GetComponent<SpriteRenderer>());
                StartCoroutine(ActionsWithDelay(4, () => InstructionsEnable(endYourTurn3), () => SetPointer(pointerAnimation1, ObjectTargetEnum.endTurn)));
                break;
            case 4: // energyCharge
                InstructionsEnable(energyCharge4);
                SetPointer(pointerAnimation1, ObjectTargetEnum.energies);
                break;
            case 5: // draw
                SetObjectClickable(BattleSystem.Instance.Interface.btnDrawRenderer);
                StartCoroutine(ActionsWithDelay(1.5f, () => InstructionsEnable(draw5), () => SetPointer(pointerAnimation1, ObjectTargetEnum.draw)));
                break;
            case 6: // after draw
                InstructionsDisable();
                StartCoroutine(TimerForMsgAppear(noActionText6));
                StartCoroutine(ActionsWithDelay(3f, () => blockScreen.SetActive(true), null));
                break;
            case 7: // getting the Dragon
                InstructionsEnable(dragonCard7);
                SetPointer(pointerAnimation1, ObjectTargetEnum.nc1);
                blockScreen.SetActive(true);
                break;
            case 8: // after Damge
                InstructionsEnable(firstWin8);
                blockScreen.SetActive(true);
                break;
            case 9:
                //VISION RANK Menu
                break;
        }
    }

    public void OnScreenClick()
    {
        switch (step)
        {
            case 4:
                InstructionsDisable();
                SetStep(5);
                break;
            case 6:
                InstructionsDisable();
                timerForMsgEnable = false;
                break;
            case 7:
                InstructionsDisable();
                blockScreen.SetActive(false);
                break;
            case 8:
                InstructionsDisable();
                blockScreen.SetActive(false);
                BattleSystem.Instance.StartNewRoundRoutine(true);
                break;
        }
    }
    private IEnumerator ActionsWithDelay(float seconds, Action action1, Action action2)
    {
        yield return new WaitForSeconds(seconds);
        action1?.Invoke();
        action2?.Invoke();
    }

    private enum ObjectTargetEnum
    {
        nc1,
        endTurn,
        draw,
        energies,
    }



    public void DisplayTip(Constants.TipsEnum tipToDisplay, bool enableScreenBlocker)
    {
        if(LoadPrefsInt(tipToDisplay.ToString()) == 0)
        {
            SavePrefsInt(tipToDisplay.ToString(), 1);
            blockScreen.SetActive(enableScreenBlocker);
            InstructionsEnable(ConvertTipEnumToString(tipToDisplay));
            SetPointer(pointerAnimation1, ConvertTipEnumToObjectWithPointer(tipToDisplay));
        }
    }

    private ObjectTargetEnum ConvertTipEnumToObjectWithPointer(Constants.TipsEnum tipToDisplay)
    {
        switch (tipToDisplay)
        {
            case 0: // NC
                return ObjectTargetEnum.nc1;
        }
        return ObjectTargetEnum.nc1;
    }

    private string ConvertTipEnumToString(Constants.TipsEnum tipToDisplay)
    {
        switch (tipToDisplay)
        {
            case 0: // NC
                return open1;
        }
        return "";
    }

    public void SavePrefsInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }

    public int LoadPrefsInt(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetInt(key);
        }
        else
        {
            return 0;
        }
    }

    private void SetPointer(Animation pointerAnim, ObjectTargetEnum objectTarget)
    {
        Vector3 position = new Vector3(0, 0, 0);

        switch (objectTarget)
        {
            case ObjectTargetEnum.nc1:
                position += new Vector3(0, 2.2f, 0);
                position += BattleSystem.Instance.puDeckUi.playerPusUi[0].transform.position;
                break;
            case ObjectTargetEnum.endTurn:
                position += new Vector3(0, 1.5f, 0);
                position += BattleSystem.Instance.Interface.turnTimer.transform.position;
                break;
            case ObjectTargetEnum.draw:
                position += new Vector3(0, 1.5f, 0);
                position += BattleSystem.Instance.Interface.btnDrawRenderer.transform.position;
                break;
            case ObjectTargetEnum.energies:
                position += new Vector3(0, 1.5f, 0);
                position += BattleSystem.Instance.Interface.energyLeft[2].transform.position;
                break;

        }
        pointerAnim.transform.parent.transform.position = position;
        pointerAnim.Play();
        pointerAnim.transform.parent.gameObject.SetActive(true);
    }
    public void StartPokerTutorial()
    {
        texasDialog.SetActive(false);
        texasTutorial.SetActive(true);
    }

    public void StartPnTutorial()
    {
        texasDialog.SetActive(false);
        BattleSystem.Instance.StartBotGame();
    }
    public void NextPokerTutorial()
    {
        if (pokerTutoPhase == 4)
        {
            texasTutorial.SetActive(false);
            StartPnTutorial();
        }
        else
        {
            LoadNextPokerTutoImage(++pokerTutoPhase);
        }
    }

    public void ShowHandRanking()
    {
        if (pokerTutoPhase == 4)
        {
            LoadNextPokerTutoImage(3);
            pokerTutoPhase = 3;
        }
    }
    private void LoadNextPokerTutoImage(int imageNumber)
    {
        pokerTutoSpriteRenderer.sprite = Resources.Load("Sprites/Tuto/poker_tuto_" + imageNumber, typeof(Sprite)) as Sprite;
    }

    private void InstructionsEnable(string text)
    {
        instructionsText.text = text;
        StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(instructionsCanvas, true, 0.5f, null));
    }
    public void InstructionsDisable()
    {
        Debug.Log("disabl");
        StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(instructionsCanvas, false, 0.5f, null));
        ResetPointers();
    }

    public void SetObjectClickable(SpriteRenderer targetRenderer)
    {
        targetRenderer.sortingOrder = 40;
    }


    public bool timerForMsgEnable;
    int timer;
    private IEnumerator TimerForMsgAppear(string text)
    {
        timer = 0;
        timerForMsgEnable = true;
        while (timer <= 5)
        {
            Debug.LogError("timer " + timer);
            if (!timerForMsgEnable)
            {
                break;
            }
            timer++;
            yield return new WaitForSeconds(1);
            if (timer == 4)
            {
                Debug.LogError("Boom");
                InstructionsEnable(text);
            }
        }
    }



    public void ResetPointers()
    {
        pointerAnimation1.Stop();
        //pointerAnimation2.Stop();
        pointerAnimation1.transform.parent.gameObject.SetActive(false);
        // pointerAnimation2.transform.parent.gameObject.SetActive(false);
    }
}
