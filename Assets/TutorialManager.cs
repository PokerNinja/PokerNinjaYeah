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
    public SpriteRenderer btnMenuSprite;
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
    private string firstWin8 = "You damaged your opponent’s HP! Abit more and you win!";
    private string firstTie88 = "It's a TIE! No one loses HP.";
    private string firstLose9 = "You opponent cause you DMG! You lose when you have no HP left!";
    private string raise10 = "you can offer your opponent to raise the DMG for this round." +
        "\nYour opponent lose HP if declines. you can use that to bluff!";
    private string lastTurn11 = "The dealer has the last move, but receives only 1 energy.";
    private string emojis11 = "You can send an Emoji to your opponent By holding your finger on your avatar and drag it to the emoji you want.";
    private string rankInNumber = "Here you can check your current hand rank.";
    private string rankMenu = "Here you got all poker ranks";


    internal void SetStep(int step)
    {
        this.step = step;
        switch (step)
        {
            case 1: // Start with f1 - long click
                ResetPrefs();
                blockScreen.SetActive(true);
                InstructionsEnable(open1);
                SetPointer(pointerAnimation1, ObjectTargetEnum.nc1);
                break;
            case 2: // after long click
                InstructionsDisable(true);
                StartCoroutine(TimerForMsgAppear(noActionText2));
                break;
            case 3: // after f1 ignite
                timerForMsgEnable = false;
                StartCoroutine(ActionsWithDelay(2, () => InstructionsEnable(endYourTurn3),
                    () =>
                    {
                        SetPointer(pointerAnimation1, ObjectTargetEnum.endTurn);
                        SetObjectClickable(true, BattleSystem.Instance.Interface.turnTimer.GetComponent<SpriteRenderer>());
                    }
                    ));
                break;
            case 4: // energyCharge
                //SetObjectClickable(false, BattleSystem.Instance.Interface.turnTimer.GetComponent<SpriteRenderer>());
                InstructionsEnable(energyCharge4);
                blockScreen.SetActive(true);
                SetPointer(pointerAnimation1, ObjectTargetEnum.energies);
                break;
            case 5: // draw
                SetObjectClickable(true, BattleSystem.Instance.Interface.btnDrawRenderer);
                StartCoroutine(ActionsWithDelay(1f, () => InstructionsEnable(draw5), () => SetPointer(pointerAnimation1, ObjectTargetEnum.draw)));
                break;
            case 6: // after draw
                SetObjectClickable(false, BattleSystem.Instance.Interface.btnDrawRenderer);
                InstructionsDisable(true);
                StartCoroutine(TimerForMsgAppear(noActionText6));
                //  StartCoroutine(ActionsWithDelay(5f, () => blockScreen.SetActive(true), null));
                break;
            case 7: // getting the Dragon
                InstructionsEnable(dragonCard7);
                SetPointer(pointerAnimation1, ObjectTargetEnum.nc1);
                blockScreen.SetActive(true);
                break;
            case 8: // after Damge You win
                InstructionsEnable(firstWin8);
                blockScreen.SetActive(true);
                break;
            case 88: // after Tie
                InstructionsEnable(firstTie88);
                blockScreen.SetActive(true);
                break;
            case 9: // after Damge YOU Lose
                InstructionsEnable(firstLose9);
                blockScreen.SetActive(true);
                break;
            case 10:
                InstructionsEnable(raise10);
                SetPointer(pointerAnimation1, ObjectTargetEnum.raise);
                SetObjectClickable(true, BattleSystem.Instance.Interface.betBtn.spriteRenderer);
                blockScreen.SetActive(true);
                break;
            case 21:
                InstructionsEnable(rankMenu);
                ResetPointers();
                SetObjectClickable(true, btnMenuSprite);
                SetPointer(pointerAnimation2, ObjectTargetEnum.rankMenu);
                blockScreen.SetActive(true);
                break;
            case 29:
                InstructionsEnable(lastTurn11);
                blockScreen.SetActive(true);
                break;
            case 30:
                InstructionsEnable(emojis11);
                blockScreen.SetActive(true);
                break;
        }
    }

    private void ResetPrefs()
    {
        SavePrefsInt(Constants.TipsEnum.RankInNumber.ToString(), 0);
    }

    public void OnScreenClick()
    {
        switch (step)
        {
            case 4:
                InstructionsDisable(true);
                SetStep(5);
                break;
            case 6:
                InstructionsDisable(false);
                timerForMsgEnable = false;
                break;
            case 7:
                InstructionsDisable(false);
                break;
            case 8:
            case 9:
                InstructionsDisable(false);
                BattleSystem.Instance.StartNewRoundRoutine(true);
                break;
            case 20:
                SetStep(21);
                break;
            case 29:
                SetStep(30);
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
        raise,
        rankInNumber,
        rankMenu,
    }



    public bool DisplayTip(Constants.TipsEnum tipToDisplay, bool enableScreenBlocker)
    {
        if (LoadPrefsInt(tipToDisplay.ToString()) == 0)
        {
            Debug.LogError("Ranking!!!!");
            SavePrefsInt(tipToDisplay.ToString(), 1);
            blockScreen.SetActive(enableScreenBlocker);
            InstructionsEnable(ConvertTipEnumToString(tipToDisplay));
            SetPointer(pointerAnimation1, ConvertTipEnumToObjectWithPointer(tipToDisplay));
            return true;
        }
        return false;
    }

    private ObjectTargetEnum ConvertTipEnumToObjectWithPointer(Constants.TipsEnum tipToDisplay)
    {
        switch (tipToDisplay)
        {
            case 0: // NC
                return ObjectTargetEnum.nc1;
            case Constants.TipsEnum.RankInNumber: // NC
                return ObjectTargetEnum.rankInNumber;
        }
        return ObjectTargetEnum.nc1;
    }

    private string ConvertTipEnumToString(Constants.TipsEnum tipToDisplay)
    {
        switch (tipToDisplay)
        {
            case 0: // NC
                return open1;
            case Constants.TipsEnum.RankInNumber: // NC
                return rankInNumber;
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
            case ObjectTargetEnum.raise:
                position += new Vector3(0, 1.5f, 0);
                position += BattleSystem.Instance.Interface.betBtn.transform.position;
                break;
            case ObjectTargetEnum.rankInNumber:
                position += new Vector3(0, 1.5f, 0);
                position += BattleSystem.Instance.Interface.currentRankNumber.transform.position;
                break;
            case ObjectTargetEnum.rankMenu:
                position += new Vector3(-1.5f, 0, 0);
                position += btnMenuSprite.gameObject.transform.position;
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
    public void InstructionsDisable(bool enableBlock)
    {
        Debug.Log("disabl");
        StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(instructionsCanvas, false, 0.5f, null));
        ResetPointers();
        blockScreen.SetActive(enableBlock);
    }

    public void SetObjectClickable(bool enable, SpriteRenderer targetRenderer)
    {
        int sortingOrder = 1;
        if (enable)
            sortingOrder = 40;
        targetRenderer.sortingOrder = sortingOrder;
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
                blockScreen.SetActive(true);
            }
        }
    }



    public void ResetPointers()
    {
        pointerAnimation1.Stop();
        pointerAnimation2.Stop();
        pointerAnimation1.transform.parent.gameObject.SetActive(false);
        pointerAnimation2.transform.parent.gameObject.SetActive(false);
    }

    internal void RankInstructions(int handRank)
    {
        Debug.LogError("Ranking!???!");

        if (DisplayTip(Constants.TipsEnum.RankInNumber, true))
        {
            StartCoroutine(ActionsWithDelay(1.5f, () => BattleSystem.Instance.Interface.UpdateCardRank(handRank), null));
            step = 20;
        }

    }
}
