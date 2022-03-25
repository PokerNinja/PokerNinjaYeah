
using Firebase.Functions;
using Managers;
using Serializables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

using StandardPokerHandEvaluator;
using Sirenix.OdinInspector;
using System.Threading.Tasks;
using System.Linq;
using UnityEngine.EventSystems;

public class BattleSystemTuto : StateMachineTuto, ICancelHandler, IPointerDownHandler
{
    public static BattleSystemTuto Instance { get; private set; }
    public TutorialUi tutorialUi;

    public bool continueTutorial = false;
    private bool toOpen = true;

    public BattleUITuto Interface => ui;


    [SerializeField] private BattleUITuto ui;

    private string firstCardTargetPU = "";
    private Vector2 firstPosTargetPU;

    public CardsDeckUi cardsDeckUi;
    public PuDeckUi puDeckUi;

    public PowerUpUi skillBtn;

    public string newPowerUpName;
    private string puDisplayName;
    private int newPuSlotIndexUse;
    private int newEnergyCost;

    //Game Settings
    private bool firstDeck = true;

    public int currentTurn = 6;
    private bool firstRound = true;
    private bool gameEndByBet = false;
    public bool enemyPuIsRunning;

    internal bool readyToPlay = false;////

    public bool isPlayerActivatePu = false;

    public int playerLifeLeft;
    public int enemyLifeLeft;

    public bool sameCardsSelection;
    internal bool resetAllCardsSelectionWhenCardClicked;

    public bool selectMode;
    public bool playerPuInProcess;
    public bool ReplaceInProgress;
    private bool waitForDrawerAnimationToEnd = false;

    public bool endRoutineFinished = false;
    public bool infoShow = false;

    public bool btnReplaceClickable = false;
    private bool emojisWheelDisplay;
    private bool emojiCooledDown = true;
    private readonly long TURN_COUNTER_INIT = 6;
    private readonly float DELAY_BEFORE_NEW_ROUND = 6f;

    public bool visionUnavailable = false;
    public bool turnInitInProgress = false;
    private bool firstDealTuto = true;


    public int energyCounter;


    public bool replaceMode = false;
    public bool endTurnInProcess = false;
    public string[] playersHand = { "7s", "Ac" };
    public string[] enemysHand = { "7d", "Js" };
    public string[] board = { "7h", "Qs", "5c", "6d", "Ks" };
    public event Action onGameStarted;

    private string[] cardDeck, puDeck;
    private int pokerTutoPhase;
    public GameObject pokerTutorial;
    public GameObject dialogInfo;
    public GameObject tutoDialog;

    private void Start()
    {
        pokerTutoPhase = 1;
        playerLifeLeft = 2;
        enemyLifeLeft = 2;
        Constants.TUTORIAL_MODE = true;
    }


    public void StartPokerTutorial()
    {
        tutoDialog.SetActive(false);
        pokerTutorial.SetActive(true);
    }

    public void StartNinjaTuto()
    {
        tutoDialog.SetActive(false);
        cardDeck = new String[] { "Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah",
                "Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah",
                "8c", "3h","9s",board[4],board[3], board[2],board[1],board[0],enemysHand[1], playersHand[1],enemysHand[0],playersHand[0]};
        puDeck = new String[] {"f2","i3","f3","f2","i3","f3",
                 "i1","f1","i2",
                 "w1","w2","w3","f2","i3","w1","w2","w3","f2","i3","f3",
                 "i1","f1","i2",
                 "w1","w2","w3",
                    "f2","i3","f3",
                 "i1","f1","w3",
                 "f2","f3","f2",
                "i3", "i3"};
        currentTurn = 5;
        Interface.Initialize("Ninja", "Alex");
        ui.InitAvatars();
        if (firstDeck)
        {
            firstDeck = false;
            StartCoroutine(StarRound());
        }
    }

    private IEnumerator StarRound()
    {
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.StartRound, true);
        InitDecks();
        yield return new WaitForSeconds(0.5f);
        ResetRoundSettings(() => StartTurn());
    }

    private void StartTurn()
    {
        PlayMusic(true);
        Interface.EnableBgColor(false);
        isPlayerActivatePu = false;
        readyToPlay = true;
        Interface.EnableVisionClick(true);
        PlayerTurnTuto(5);
    }


    bool yourLastTurn, finalTurn;

    public void PlayerTurnTuto(int turnCounter)
    {
        yourLastTurn = false;
        finalTurn = false;
        Debug.LogError("Notice turn counter " + turnCounter);
        if (turnCounter == 2)
        {
            yourLastTurn = true;
        }
        else if (turnCounter == 1)
        {
            finalTurn = true;
        }
        StartPlayerTurn(turnCounter);
    }

    private void StartPlayerTurn(int turnCounter)
    {
        endTurnInProcess = false;
        newPowerUpName = "x";

        DealPu(true, () =>
        {
            Action tutorialAction = null;
            if (turnCounter == 5)
            {
                tutorialAction = () => FocusOnObjectWithText(true, 0, Constants.TutorialObjectEnum.startGame.GetHashCode(), true);
            }
            else if (turnCounter == 3)
            {
                ChargeEnergyCounter(2);
                tutorialAction = () => FocusOnObjectWithText(true, 1, Constants.TutorialObjectEnum.flipSkill.GetHashCode(), false);
            }
            else if (turnCounter == 1)
            {
                tutorialAction = () => FocusOnObjectWithText(true, 1, Constants.TutorialObjectEnum.lastTurnEnergy.GetHashCode(), true);
            }
            tutorialAction?.Invoke();
        }
        );
    }



    public void Awake()
    {
        QualitySettings.vSyncCount = 0;
        Screen.sleepTimeout = SleepTimeout.NeverSleep;

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }

    }


    public void LoadMenuScene(bool playAgain)
    {
        Constants.TUTORIAL_MODE = false;
        //SoundManager.Instance.StopMusic();
        SceneManager.LoadScene("GameMenuScene");
        Destroy(GameObject.Find("AnimationManager"));
        Destroy(GameObject.Find("ObjectPooler"));
        Destroy(GameObject.Find("SoundManager"));
        Destroy(GameObject.Find("BattleSystemTuto"));
    }

    public void PlayMusic(bool enable)
    {
        SoundManager.Instance.PlayMusic();
        /////
    }


    [Button]
    internal void EndRoundVisual(bool isPlayerWin)
    {
        int lifeLeft = playerLifeLeft;
        if (isPlayerWin)
        {
            lifeLeft = enemyLifeLeft;
        }
        StartCoroutine(ui.CardProjectileEffect(isPlayerWin, () => HitEffectAnimation(isPlayerWin), () => ui.LoseLifeUi(!isPlayerWin, lifeLeft)));
    }



    internal void HitEffectAnimation(bool isPlayerHit)
    {
        ui.HitEffect(isPlayerHit);
    }

    [Button]
    public void UpdateHandRank(bool reset)
    {
        Debug.LogError("hereIm");
        if (!AreBoardCardsFlipped() && !reset)
        {
            int handRank = 7000;
            Hand bestHand = cardsDeckUi.CalculateHand(false, true, false, false);

            handRank = bestHand.Rank;

            ui.UpdateCardRank(handRank);
        }
        else
        {
            ui.UpdateCardRank(7000);
        }
    }

    #region Turn
    private void SaveAutoPlayAgain(bool playAgain)
    {
        PlayerPrefs.SetString("PlayAgain", playAgain.ToString());
    }


    private void TurnEvents(int currentTurn)
    {
        switch (currentTurn)
        {
            case 4:
                firstDeck = false;
                waitForDrawerAnimationToEnd = true;
                //  yield return new WaitForSeconds(0.7f);
                cardsDeckUi.DealCardsForBoard(true, () => waitForDrawerAnimationToEnd = false, () => UpdateHandRank(false));
                break;
            case 2:
                cardsDeckUi.DealCardsForBoard(true, () => waitForDrawerAnimationToEnd = false, () => UpdateHandRank(false));
                break;
            case 1:
                break;
            case -1:
                //  isRoundReady = false;
                StartCoroutine(EndRoundTuto());
                break;
        }
    }


    private IEnumerator CheckIfEnemyPuRunningAndStartPlayerTurn()
    {
        Debug.LogError("et " + enemyPuIsRunning + " " + turnInitInProgress);
        if (!enemyPuIsRunning && !turnInitInProgress)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndTurnGong, true);
            PlayerTurnTuto(currentTurn);
        }
        else
        {
            while (enemyPuIsRunning || turnInitInProgress)
            {
                if (!enemyPuIsRunning && !turnInitInProgress)
                {
                    StartCoroutine(CheckIfEnemyPuRunningAndStartPlayerTurn());
                }
                yield return new WaitForSeconds(0.6f);
            }
        }

    }



    public void EndTurn()
    {
        ui.turnBtnSpriteREnderer.sortingOrder = 1;
        StartCoroutine(FakePlayerEndTurnCoro());
        FocusOnObjectWithText(false, 2, 0, false);
    }


    public void OnEndTurnButton()
    {
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.BtnClick1, false);
        EndTurn();
    }

    private void SaveTurnOf(string whatPlayer)
    {
        PlayerPrefs.SetString("turn", whatPlayer);
    }



    private IEnumerator WaitForPuToEndLoop()
    {
        if (playerPuInProcess || ReplaceInProgress)
        {
            playerPuInProcess = false;

            yield return new WaitForSeconds(4f);
        }
        EndTurn();

    }

    #endregion


    #region Deck
    public void InitDecks()
    {
        cardsDeckUi = CardsDeckUi.Instance();
        cardsDeckUi.InitDeckFromServer(cardDeck);
        cardsDeckUi.isPlayerFirst = IsPlayerTurn();
        puDeckUi = PuDeckUi.Instance();
        puDeckUi.InitDeckFromServer(puDeck);
    }
    private void DealHands(Action FinishCallback)
    {
        waitForDrawerAnimationToEnd = true;
        StartCoroutine(cardsDeckUi.CreateHands(() => waitForDrawerAnimationToEnd = false, FinishCallback));
    }



    private void ResetRoundAndTurnCounter()
    {
        if (LocalTurnSystem.Instance.isPlayerFirstPlayer)
        {
            LocalTurnSystem.Instance.RoundCounter.Value = 0 + 1;
            LocalTurnSystem.Instance.TurnCounter.Value = TURN_COUNTER_INIT;
        }
    }



    private IEnumerator StartPlayerTurn(bool delay, Action EndAction)
    {
        if (delay)
        {
            yield return new WaitForSeconds(0.5f);
        }
        ui.SetTurnIndicator(false, false);
        EndAction?.Invoke();
        StartCoroutine(CheckIfEnemyPuRunningAndStartPlayerTurn());
        /*  StartCoroutine(AnimationManager.Instance.AlphaAnimation(ui.turnTextGO.GetComponent<SpriteRenderer>(),
              false, Values.Instance.textTurnFadeOutDuration, () =>
              {
                  EndAction?.Invoke();
                  StartCoroutine(CheckIfEnemyPuRunningAndStartPlayerTurn());
              }));*/
    }

    internal void ActivateButtonForTutorial(int turnCounter)
    {
        switch (turnCounter)
        {
            case 4:
                //  puDeckUi.playerPusUi[0].EnablePu(true);
                StartCoroutine(AnimationManager.Instance.UpdateValue(true, "_GradBlend", Values.Instance.puChangeColorDisableDuration, puDeckUi.playerPusUi[0].spriteRenderer.material, 0, null));
                break;
            case 5:
                puDeckUi.playerPusUi[0].isClickable = true;
                break;
            case 3:
                skillBtn.EnablePu(true);
                break;
            case 1:
                puDeckUi.playerPusUi[1].EnablePu(true);
                break;
            case 8:
                ui.EnableEndTurnBtn(true);
                break;
            case 15:
                ui.EnableBtnReplace(true);
                break;

        }
    }

    public void FakeEnemyEndTurn()
    {
        StartCoroutine(StartPlayerTurn(true, () =>
        {
            TurnEvents(--currentTurn);
            StartCoroutine(AnimationManager.Instance.AlphaAnimation(ui.turnBtnSpriteREnderer, true, Values.Instance.turnBtnAlphaDuration, null));
        }));
    }

    public void FakePlayerEndTurn()
    {
        StartCoroutine(FakePlayerEndTurnCoro());
    }
    private IEnumerator FakePlayerEndTurnCoro()
    {
        ui.energyLeft[0].spriteRenderer.sortingOrder = 0;
        ui.EnableEndTurnBtn(false);
        StartCoroutine(AnimationManager.Instance.AlphaAnimation(ui.turnTextGO.GetComponent<SpriteRenderer>(), false, Values.Instance.textTurnFadeOutDuration, null));
        ui.SetTurnIndicator(false, false);
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndTurnGong, true);
        TurnEvents(--currentTurn);
        yield return new WaitForSeconds(3f);
        if (currentTurn > 1)
        {
            EnemyTurnTuto(currentTurn);
        }
        else
        {
            StartCoroutine(EndRoundTuto());
        }
    }

    private void EnemyTurnTuto(int currentTurn)
    {
        turnInitInProgress = true;
        Interface.EnableBgColor(false);
        Interface.EnablePlayerButtons(false);
        DisablePlayerPus();
        DealPu(false, () =>
        {
            Interface.SetTurnIndicator(false, true);
            SetStateTuto(new TutorialEnemy(this, currentTurn));
            turnInitInProgress = false;
        }
        );
    }

    public async void FakeEnemyPuUse(int puIndex, string cardPlace1, string cardPlace2, bool endTurn)
    {
        Debug.LogWarning("FAking it");
        PowerUpInfo puInfo = new PowerUpInfo("Alex", puDeckUi.GetPu(false, puIndex).puName, cardPlace2, cardPlace1, puIndex, 12345);
        EnemyPuUse(puInfo);
        if (endTurn)
        {
            await Task.Delay(4500);
            enemyPuIsRunning = false;
            Debug.LogWarning("EndingIT");
            FakeEnemyEndTurn();
        }
    }
    [Button]
    public void FakeEnemyEmoji(int index)
    {
        Debug.LogError("ind" + index);
        StartCoroutine(ui.DisplayEmoji(false, index, null));
    }




    #endregion

    #region Settings
    public void ResetRoundSettings(Action FinishCallbac)
    {
        /* Constants.TemproryUnclickable = false;
         ui.tieTitle.SetActive(false);
         ui.EnableBgColor(false);

         ui.ResetTurnPanels();
         // currentTurn = 1;
         if (firstRound)
         {
             firstRound = false;////
         }*/
        UpdateHandRank(true);
        //  cardsDeckUi.DeleteAllCards(() => DealHands(FinishCallbac));
        DealHands(FinishCallbac);
    }



    public bool DealDamage(bool isPlayer)
    {
        bool isGameOver = false;
        if (isPlayer)
        {
            // ui.LoseLifeUi(isPlayer, --playerLifeLeft);
            --playerLifeLeft;
        }
        else
        {
            --enemyLifeLeft;
        }
        if (playerLifeLeft == 0 || enemyLifeLeft == 0)
        {
            isGameOver = true;
        }
        return isGameOver;
    }


    public void DisplayWinner(string winningText)
    {
        StartCoroutine(ui.ShowWinner(winningText));
    }


    #endregion


    #region PowerUp
    public void OnPowerUpPress(string newPowerUpName, int newPuSlotIndexUse, int energyCost)
    {
        if (IsPlayerTurn() && energyCounter - energyCost >= 0)
        {
            //  ResetTimers();
            playerPuInProcess = true;
            isPlayerActivatePu = true;
            ui.EnableVisionClick(false);
            this.newPowerUpName = newPowerUpName;
            //  puDisplayName = PowerUpStruct.Instance.GetPowerUpDisplayName(newPowerUpName);
            this.newPuSlotIndexUse = newPuSlotIndexUse;
            this.newEnergyCost = energyCost;

            SetStateTuto(new PowerUpStateTuto(this, true, energyCost, newPowerUpName, "", "", new Vector2(0, 0), new Vector2(0, 0), newPuSlotIndexUse));
            if (newPowerUpName.Equals("i3"))
            {
                FocusOnObjectWithText(true, 0, 6, false);
            }
            if (newPowerUpName.Equals("sflip"))
            {
                FocusOnObjectWithText(true, 0, Constants.TutorialObjectEnum.cardToFlipFreeze.GetHashCode(), false);
            }
            if (newPowerUpName.Equals("w3"))
            {
                FocusOnObjectWithText(true, 0, 22, false);
            }

        }
        else if (energyCounter - energyCost < 0)
        {
            Debug.LogWarning("YouDontHaveEnoughEnergy");
        }
    }



    public bool IsPlayerTurn()
    {
        return true;
    }

    public void DissolvePuAfterUse(bool isPlayer, int index)
    {
        puDeckUi.GetPu(isPlayer, index).DissolvePu(2f, Values.Instance.puDissolveDuration, null, () => ResetPuUi(isPlayer, index));
    }


    private void EnemyPuUse(PowerUpInfo powerUpInfo)
    {
        isPlayerActivatePu = false;
        enemyPuIsRunning = true;
        ui.EnableDarkScreen(false, true, null);
        if (powerUpInfo.slot != -1)
        {
            puDeckUi.GetPu(false, powerUpInfo.slot).AnimatePuUse(
                      () => SetStateTuto(new PowerUpStateTuto(this, false, 0, powerUpInfo.powerupName, powerUpInfo.cardPlace1, powerUpInfo.cardPlace2,
                      cardsDeckUi.GetCardPosition(powerUpInfo.cardPlace1), cardsDeckUi.GetCardPosition(powerUpInfo.cardPlace2), powerUpInfo.slot)), null);
        }
        else // SKILL
        {
            SetStateTuto(new PowerUpStateTuto(this, false, 0, powerUpInfo.powerupName, powerUpInfo.cardPlace1, powerUpInfo.cardPlace2, new Vector2(0, 0), new Vector2(0, 0), powerUpInfo.slot));
        }
    }

    public IEnumerator OnCardsSelectedForPU(string cardPlace, Vector2 position)
    {
        if (Constants.cardsToSelectCounter == 0)
        {

            if (newPowerUpName.Equals("sflip"))
            {
                if (Constants.EnemyCard1.Equals(cardPlace))
                {
                    FocusOnObjectWithText(true, 1, 14, true);
                }
            }
            else if (newPowerUpName.Equals("i3"))
            {
                FocusOnObjectWithText(true, 0, 7, true);
            }
            else if (newPowerUpName.Equals("w3"))
            {
                FocusOnObjectWithText(false, 2, 22, false);
                StartCoroutine(FocusOnObjectWithDelasy(4, true, 1, 23, true));
            }


            ResetValuesAfterCardSelection(Constants.AllCardsTag);
            yield return new WaitForSeconds(0.5f);
            if (cardPlace.Equals(Constants.EnemyCard2) && newPowerUpName.Equals("sflip"))
            {
                FocusOnObjectWithText(true, 0, 13, false);
            }
            else
            {
                SetStateTuto(new PowerUpStateTuto(this, true, newEnergyCost, newPowerUpName, firstCardTargetPU, cardPlace, firstPosTargetPU, position, newPuSlotIndexUse));
            }

            firstPosTargetPU = new Vector2(0, 0);
            firstCardTargetPU = "empty";
        }
        else if (Constants.cardsToSelectCounter == 1)
        {
            firstCardTargetPU = cardPlace;
            firstPosTargetPU = position;
            yield return new WaitForSeconds(0.1f);
            if (!sameCardsSelection)
            {
                cardsDeckUi.DisableCardsSelection(cardPlace);
            }
            else
            {
                sameCardsSelection = false;
            }
        }
    }

    private void ResetValuesAfterCardSelection(string cardPlace)
    {
        selectMode = false;
        if (resetAllCardsSelectionWhenCardClicked)
        {
            cardsDeckUi.DisableCardsSelection("All");
            resetAllCardsSelectionWhenCardClicked = false;
        }
        else
        {
            cardsDeckUi.DisableCardsSelection(cardPlace);
        }
        ui.InitLargeText(false, puDisplayName);
    }

    /// <summary>
    /// ///////
    public void ShowPuInfo(Vector2 startingPosition, bool paddingRight, string puName, string puDisplayName)
    {
        infoShow = true;
        ui.ShowPuInfoDialog(startingPosition, paddingRight, puName, puDisplayName, true, false, null);
    }
    public void HideDialog()
    {
        ui.ShowPuInfoDialog(new Vector2(0, 0), false, " ", " ", false, false, () =>
        {
            infoShow = false;
            //ui.dialogContentUi.color = new Color(1, 1, 1, 0);
        });
    }

    internal void ResetPuUi(bool isPlayer, int puIndex)
    {
        PowerUpUi pu = puDeckUi.GetPu(isPlayer, puIndex);
        puDeckUi.RemovePuFromList(isPlayer, puIndex);
        puDeckUi.ResetPuUI(pu, null);
    }
    internal void ResetPuUi(PowerUpUi pu)
    {
        puDeckUi.RemovePuFromList(pu.isPlayer, pu.puIndex);
        puDeckUi.ResetPuUI(pu, null);
    }





    private long CreateTimeStamp()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local).Ticks;
    }
    internal void ReplacePu(bool isPlayer, int puIndex)
    {
        if (isPlayer)
        {
            Constants.TemproryUnclickable = true;
            ReplaceInProgress = true;
            DisablePlayerPus();
            ReduceEnergy(Values.Instance.energyCostForDraw);
        }
        puDeckUi.ReplacePu(isPlayer, puIndex, () =>
        {
            if (isPlayer)
            {
                ReplaceInProgress = false;
                EnableReplaceDialog(true, false);
                if (energyCounter == 0)
                {
                    // StartCoroutine(AutoEndTurn());
                }
                else
                {
                    Constants.TemproryUnclickable = false;
                }
            }
        });
    }





    public void DisablePlayerPus()
    {
        foreach (PowerUpUi puUi in puDeckUi.GetPuList(true))
        {
            if (puUi != null)
            {
                if (puUi.aboutToDestroy)
                {
                    puUi.EnableShake(false);
                    puUi.aboutToDestroy = false;
                }
                else
                {
                    puUi.EnablePu(false);
                }
            }
        }
        skillBtn.EnablePu(false);
    }
    public void CheckIfPuAvailable(PowerUpUi puUi)
    {
        if (energyCounter - puUi.energyCost < 0 || puUi.puIndex == -1)
        {
            puUi.EnablePu(false);
        }
        else
        {
            if (puUi.puElement.Equals("i") || (puUi.puElement.Equals("w") && !puUi.isMonster) || puUi.puName.Equals("fm1"))
            {
                string[] cardsTag = PowerUpStruct.Instance.GetReleventTagCards(puUi.name, true);
                int cardsLimit = 0;
                if (cardsDeckUi.GetListByTag(cardsTag[0]) == cardsDeckUi.GetListByTag(cardsTag[1]))
                {
                    cardsLimit = 1;
                }
                if (!CheckIfCardsAvailbleForSelect(cardsLimit, cardsDeckUi.GetListByTag(cardsTag[0]))
                    || !CheckIfCardsAvailbleForSelect(cardsLimit, cardsDeckUi.GetListByTag(cardsTag[1])))
                {
                    puUi.EnablePu(false);
                }
                else
                {
                    puUi.EnablePu(true);
                }
            }
            else if (puUi.puName.Equals("im1") && cardsDeckUi.IsPlayerHandUnderSmoke())
            {
                puUi.EnablePu(false);
            }
            else if (puUi.puName.Equals("wm1") && cardsDeckUi.IsOneCardFromHandsFreeze(true))
            {
                puUi.EnablePu(false);
            }
            else
            {
                puUi.EnablePu(true);
            }
        }
    }

    private bool CheckIfCardsAvailbleForSelect(int cardsLimit, List<CardUi> cards)
    {
        int frozenCards = 0;
        if (cards != null)
        {

            foreach (CardUi card in cards)
            {
                if (card.freeze)
                {
                    frozenCards++;
                    if (cards.Count - frozenCards <= cardsLimit)
                    {
                        return false;
                    }
                }
            }
        }
        return true;
    }


    public void DestroyAndDrawCard(string cardPlace, float delay, bool ResetEnable, bool firstCard, bool lastCard)
    {
        if (cardsDeckUi.GetCardUiByName(cardPlace).freeze)
        {
            FreezePlayingCard(cardPlace, false, ResetEnable);
        }
        else
        {
            bool isFlip = IsEnemyCard(cardPlace);
            StartCoroutine(ReplaceSelectedCard(cardPlace, isFlip, delay, ResetEnable, firstCard, lastCard));
        }
        if (cardPlace.Equals(Constants.BFlop3))
        {
            StartCoroutine(FocusOnObjectWithDelasy(3, true, 0, 18, false));
        }
    }

    private bool IsEnemyCard(string cardPlace)
    {
        return cardPlace.Contains("Enemy");
    }

    public IEnumerator ReplaceSelectedCard(string cardPlace, bool isFlip, float delay, bool ResetEnable, bool isFirstCard, bool isLastCard)
    {

        yield return new WaitForSecondsRealtime(delay);
        cardsDeckUi.DestroyCardObjectFire(cardPlace, false, null);
        yield return new WaitForSeconds(delay + 0.4f);
        Action resetAction = null;
        if (ResetEnable)
        {
            resetAction = () => EnableDarkAndSorting(false);
        }
        cardsDeckUi.DrawAndReplaceCard(cardPlace, isFlip, resetAction, isFirstCard, isLastCard);
    }

    private void EnableDarkAndSorting(bool enable)
    {
        /* if (TUTORIAL_MODE && newPowerUpName.Equals("i3"))
         { }
         else
         {
             if (!enable)
             {
                 ui.EnableVisionClick(true);
                 newPowerUpName = "x";
                 fm1Activated = false;
                 //selectCardsMode = true;
             }
             ui.EnableDarkScreen(isPlayerActivatePu, enable, () => StartCoroutine(ResetSortingOrder(enable)));
         }*/
    }

    public void UpdateZPos(bool aboveDarkScreen, string tag)
    {
        foreach (CardUi card in cardsDeckUi.GetListByTag(tag))
        {
            card.EnableSelecetPositionZ(aboveDarkScreen);
        }
    }




    public void SetCardsSelectionAndDisplayInfo(int cardsToSelectCounter, string newPuName)
    {
        if (!newPuName.Equals("fm1"))
        {
            selectMode = true;
        }
        if (cardsToSelectCounter > 0)
        {
            ui.InitLargeText(true, PowerUpStruct.Instance.GetPuInfoByName(newPuName));
        }
        Constants.cardsToSelectCounter = cardsToSelectCounter;
    }

    internal void FreezePlayingCard(string cardTarget, bool isToFreeze, bool reset)
    {
        CardUi cardToFreeze = GameObject.Find(cardTarget).GetComponent<CardUi>();
        cardToFreeze.freeze = isToFreeze;
        if (!reset)
        {
            ui.FreezeObject(cardToFreeze.spriteRenderer, isToFreeze, cardToFreeze.GetisFaceDown(), null, true);
        }
        else
        {
            ui.FreezeObject(cardToFreeze.spriteRenderer, isToFreeze, cardToFreeze.GetisFaceDown(), () => EnableDarkAndSorting(false), true);
        }
    }


    internal void SwapAndDestroy(string cardTarget1, string cardTarget2)
    {
        if (cardTarget2.Contains("Deck"))
        {
            cardsDeckUi.SwapAndDestroy(cardTarget2, cardTarget1, () => EnableDarkAndSorting(false));
        }
        else
        {
            cardsDeckUi.SwapAndDestroy(cardTarget1, cardTarget2, () => EnableDarkAndSorting(false));
        }
    }


    internal void Draw2Cards(bool isEnemy, Action EndAction)
    {
        cardsDeckUi.Draw2CardsWithDrawer(isEnemy, EndAction);
    }

    internal void FlipCard(string cardTarget2, bool isPlayerActivate)
    {
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.PuSee, false);
        cardsDeckUi.FlipCardPu(cardTarget2, isPlayerActivate, () =>
        {
            EnableDarkAndSorting(false);
        });

    }


    internal void SwapTwoCards(string cardTarget1, string cardTarget2)
    {
        cardsDeckUi.SwapTwoCards(cardTarget1, cardTarget2, () => EnableDarkAndSorting(false));
    }


    internal void SwapPlayersHands()
    {
        cardsDeckUi.SwapPlayersHands(() => EnableDarkAndSorting(false));
    }
    internal void DealPu(bool isPlayer, Action OnEnd)
    {

        if (firstDealTuto)
        {
            firstDealTuto = false;
            OnEnd.Invoke();
        }
        else
        {
            StartCoroutine(DealPuCourotine(isPlayer, OnEnd));
        }
    }
    internal IEnumerator DealPuCourotine(bool isPlayer, Action OnEnd)
    {
        if (waitForDrawerAnimationToEnd)
        {
            yield return new WaitForSeconds(0.1f);
            StartCoroutine(DealPuCourotine(isPlayer, OnEnd));
        }
        else
        {
            puDeckUi.DealRoutine(isPlayer, OnEnd);
        }
    }

    internal void InitProjectile(bool isPlayerActivate, int puIndex, string powerUpName, Vector2 posTarget1, Vector2 posTarget2, Action IgnitePowerUp)
    {
        bool inlargeProjectile = false;
        switch (powerUpName)
        {
            case nameof(PowerUpStruct.PowerUpNamesEnum.im1):
                if (isPlayerActivate)
                {
                    posTarget1 = cardsDeckUi.playerCardsUi[0].transform.position;
                    posTarget2 = cardsDeckUi.playerCardsUi[1].transform.position;
                }
                else
                {
                    posTarget1 = cardsDeckUi.enemyCardsUi[0].transform.position;
                    posTarget2 = cardsDeckUi.enemyCardsUi[1].transform.position;
                }
                break;
            case nameof(PowerUpStruct.PowerUpNamesEnum.fm2):
                inlargeProjectile = true;
                break;
        }

        ui.InitProjectile(puDeckUi.GetPuPosition(isPlayerActivate, puIndex), inlargeProjectile, powerUpName, posTarget1, posTarget2, IgnitePowerUp);
    }

    #endregion

    #region Buttons


    public void EnableReplaceDialog(bool disable, bool endTurn)
    {
        if (btnReplaceClickable && energyCounter > 0 && !disable && puDeckUi.GetPuListCount(true) < 2)
        {
            /*if (TUTORIAL_MODE)
            {
                FocusOnObjectWithText(false, 1, 14, false);
            }*/
            btnReplaceClickable = false;
            ReduceEnergy(Values.Instance.energyCostForDraw);
            ui.EnablePlayerButtons(false);
            DisablePlayerPus();
            DealPu(true, () =>
            {
                FocusOnObjectWithText(true, 1, 16, true);
            });
        }
        else if (btnReplaceClickable || endTurn || disable)
        {
            Debug.LogError("1");
            if (/*replaceMode ||*/ IsPlayerTurn() && energyCounter > 0)
            {
                Debug.LogError("2");
                PowerUpUi[] playerPus = puDeckUi.GetPuList(true);
                if (playerPus[0] != null || playerPus[1] != null)
                {
                    replaceMode = !replaceMode;
                    if (replaceMode)
                    {
                        puDeckUi.EnablePusSlotZ(true, true);
                        ui.EnableDarkScreen(isPlayerActivatePu, true, null);
                    }
                    else
                    {
                        ui.EnableDarkScreen(isPlayerActivatePu, false, () =>
                         {
                             puDeckUi.EnablePusSlotZ(true, false);
                             ActivatePlayerButtons(!endTurn, false);
                         });
                    }
                    if (playerPus[0] != null)
                    {
                        playerPus[0].SetReplaceMode(replaceMode);
                    }
                    if (playerPus[1] != null)
                    {
                        playerPus[1].SetReplaceMode(replaceMode);
                    }
                }
            }
            else
            {
                Debug.LogError("2");
                ui.EnableDarkScreen(isPlayerActivatePu, false, () =>
                {
                    puDeckUi.EnablePusSlotZ(true, false);
                    ActivatePlayerButtons(!endTurn, false);
                });
            }
        }
        else
        {
            ui.DisableClickBtnReplace();
        }
    }



    public void SlideRankingImg()
    {
        if (currentTurn == 4 && toOpen)
        {
            FocusOnObjectWithText(false, 1, 9, false);
            toOpen = false;
        }
        else if (currentTurn == 4 && !toOpen)
        {
            continueTutorial = true;
        }
        ui.SlideRankingImg();
    }



    public void OnVisionBtnDown()
    {
        if (!AreBoardCardsFlipped() && !selectMode)
        {
            int handRank = -1;
            //STORE LAST HAND RANK INSTEAED
            Hand bestHand = cardsDeckUi.CalculateHand(false, true, false, false);
            if (!visionUnavailable)
            {
                List<CardUi> winningPlayersCards = new List<CardUi>();
                winningPlayersCards.AddRange(cardsDeckUi.boardCardsUi);
                winningPlayersCards.AddRange(cardsDeckUi.playerCardsUi);
                if (cardsDeckUi.ghostCardUi != null && !cardsDeckUi.ghostCardUi.cardPlace.Contains("Enemy"))
                {
                    winningPlayersCards.Add(cardsDeckUi.ghostCardUi);
                }
                ui.VisionEffect(bestHand.getCards(), winningPlayersCards);
                handRank = bestHand.Rank;
            }
            ui.UpdateCardRank(handRank);
            ui.UpdateRankTextInfo(true, handRank);
            //TODO make it more efficient
            SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.Vision, true);

        }
    }
    public void OnVisionBtnUp()
    {
        if (!AreBoardCardsFlipped()/* && !selectMode*/)
        {
            AnimationManager.Instance.VisionEffect(cardsDeckUi.GetBoardAndPlayerHandList(), cardsDeckUi.GetBoardAndPlayerHandList().Count, false);
        }
        SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.Vision, false);
        ui.UpdateRankTextInfo(false, 0);
        if (currentTurn == 4)
        {
            tutorialUi.btnTutorial.interactable = true;
            AnimationManager.Instance.AlphaFade(true, tutorialUi.btnTutorialSprite, 1f, null/* ()=>infoDialog.gameObject.SetActive(false)*/);
        }
    }




    private bool AreBoardCardsFlipped()
    {
        if (cardsDeckUi.boardCardsUi.Count < 3)
        {
            return true;
        }
        foreach (CardUi cardUi in cardsDeckUi.boardCardsUi)
        {
            if (cardUi.GetisFaceDown())
            {
                return true;
            }
        }

        return false;
    }
    internal void RevealEnemyCards()
    {
        ui.RevealCards(cardsDeckUi.enemyCardsUi);

    }

    internal void RevealBoardCards()
    {
        ui.RevealCards(cardsDeckUi.boardCardsUi);
    }


    internal void ActivatePlayerButtons(bool enable, bool delay)
    {
        if (delay)
        {
            StartCoroutine(ActivePlayerButtonWithDelay());
        }
        else
        {
            if (enable && !endTurnInProcess)
            {
                ui.EnablePlayerButtons(true);

            }
            else if (!enable)
            {
                ui.EnablePlayerButtons(false);
                DisablePlayerPus();
            }
        }
    }

    private IEnumerator ActivePlayerButtonWithDelay()
    {
        yield return new WaitForSeconds(Values.Instance.delayTimerStart);
        ui.EnablePlayerButtons(true);
    }

    #endregion

    internal void ChargeEnergyCounter(int amountToAdd)
    {
        if (energyCounter != 3)
        {
            switch (energyCounter)
            {
                case 0:
                case 1:
                    {
                        ui.UpdateEnergy(true, amountToAdd);
                        break;
                    }
                case 2:
                    {
                        ui.UpdateEnergy(true, 1);
                        break;
                    }
            }
        }
        energyCounter += amountToAdd;
        if (energyCounter > 3)
        {
            energyCounter = 3;
        }

    }
    internal void ReduceEnergy(int amountToSub)
    {
        if (energyCounter != 0)
        {
            energyCounter -= amountToSub;
            ui.UpdateEnergy(false, amountToSub);
        }
    }
    internal void EmojiSelected(int id)
    {
        if (emojiCooledDown && id != -1)
        {
            ShowEmojiWheel(false);
            emojiCooledDown = false;
            StartCoroutine(ui.DisplayEmoji(true, id, () => emojiCooledDown = true));
            FocusOnObjectWithText(false, 1, 18, false);
        }
        else if (id == -1)
        {
            ShowEmojiWheel(false);
        }
        else if (!emojiCooledDown)
        {
            StartCoroutine(ui.ShakeEmoji(id, () => ShowEmojiWheel(false)));
        }
    }
    public void ShowEmojiWheel(bool enable)
    {
        ui.ShowEmojiWheel(enable);
        emojisWheelDisplay = enable;
    }



    internal void WinParticleEffect()
    {

        ui.WinParticleEffect();
    }

    internal void FocusOnObjectWithText(bool enable, int maskShape, int objectNumber, bool endByBtn)
    {
        SetStateTuto(new TutorialSystem(this, enable, maskShape, objectNumber, endByBtn));
    }
    internal IEnumerator FocusOnObjectWithDelasy(int delay, bool enable, int maskShape, int objectNumber, bool endByBtn)
    {
        yield return new WaitForSeconds(delay);
        SetStateTuto(new TutorialSystem(this, enable, maskShape, objectNumber, endByBtn));
    }

    [Button]
    internal void DealBoard()
    {
        cardsDeckUi.DealCardsForBoard(true, null, () => UpdateHandRank(false));
    }
    [Button]
    internal void EndRoundManual()
    {
        StartCoroutine(EndRoundTuto());
    }

    public void EndRoundExternal()
    {
        StartCoroutine(EndRoundTuto());
    }

    private IEnumerator EndRoundTuto()
    {
        PlayMusic(false);
        Interface.EnablePlayerButtons(false);
        Interface.SetTurnIndicator(false, false);
        Interface.EnableVisionClick(false);

        RevealEnemyCards();
        RevealBoardCards();
        foreach (CardUi card in cardsDeckUi.boardCardsUi)
        {
            card.cardMark.SetActive(false);
        }
        yield return new WaitForSeconds(1.5f);

        DisplayWinner(WinnerCalculator());
        yield return new WaitForSeconds(5f);
        ui.winBtn.SetActive(true);
    }


    public string WinnerCalculator()
    {
        //MAKE IT BETTER
        Hand bestOpponentHand = cardsDeckUi.CalculateHand(true, false, false, false);


        Hand bestPlayerHand = cardsDeckUi.CalculateHand(true, true, false, false);

        // battleSystem.Interface.UpdateCardRank(bestPlayerHand.Rank);
        string winnerMsg = "";
        // Opponent win
        // Make it better

        if (bestPlayerHand.Rank.CompareTo(bestOpponentHand.Rank) == -1)
        {
            // SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Win, true);
            if (DealDamage(false))
            {
            }
            else
            {
                winnerMsg = "Ninja Wins With ";
            }
            winnerMsg += Interface.ConvertHandRankToTextDescription(bestPlayerHand.Rank);
            AnimateWinWithHand(bestPlayerHand, true);
        }
        return winnerMsg;
    }



    private void AnimateWinWithHand(Hand winningHand, bool isPlayerWin)
    {
        //MAKE IT BETTER
        List<Card> winningCards = winningHand.getCards();
        List<CardUi> winningPlayersCards = new List<CardUi>();
        List<CardUi> winningBoardCards = new List<CardUi>();
        List<CardUi> losingBoardCards = new List<CardUi>();
        List<CardUi> boardCardsUi = new List<CardUi>();
        List<CardUi> playerCardsUi = new List<CardUi>();
        List<CardUi> EnemyCardsUi = new List<CardUi>();
        boardCardsUi.AddRange(cardsDeckUi.GetListByTag("CardB"));
        losingBoardCards.AddRange(boardCardsUi);
        playerCardsUi.AddRange(cardsDeckUi.GetListByTag("CardP"));
        EnemyCardsUi.AddRange(cardsDeckUi.GetListByTag("CardE"));
        CardUi card1, card2;
        if (isPlayerWin)
        {
            card1 = playerCardsUi[0];
            card2 = playerCardsUi[1];
        }
        else
        {
            card1 = EnemyCardsUi[0];
            card2 = EnemyCardsUi[1];
        }

        string card1Str = card1.cardDescription.ToString();// Cant catch properly. catch it first place 
        string card2Str = card2.cardDescription.ToString();
        string winningCardDesc;
        for (int i = 0; i < 5; i++)
        {
            winningCardDesc = winningCards[i].ToString(CardToStringFormatEnum.ShortCardName);
            if (card1Str.Equals(winningCardDesc))
            {
                if (card1.freeze)
                {
                    Interface.FreezeObject(card1.spriteRenderer, false, card1.GetisFaceDown(), null, false);
                }
                winningPlayersCards.Add(card1);
            }
            if (card2Str.Equals(winningCardDesc))
            {
                if (card2.freeze)
                {
                    Interface.FreezeObject(card2.spriteRenderer, false, card2.GetisFaceDown(), null, false);
                    //TODO make it better
                }
                winningPlayersCards.Add(card2);
            }

            for (int j = 0; j < 5; j++)
            {
                if (boardCardsUi[j].cardDescription.ToString().Equals(winningCardDesc))
                {
                    winningBoardCards.Add(boardCardsUi[j]);
                    j = 5;
                }
            }
        }
        losingBoardCards.RemoveAll(item => winningBoardCards.Contains(item));
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndRoundGong, true);

        winningPlayersCards = winningPlayersCards.Concat(winningBoardCards).ToList();
        winningPlayersCards = RearangeWinningCards(winningPlayersCards, false);
        AnimationManager.Instance.AnimateWinningHandToBoard2(winningPlayersCards,
           ConvertRankToCardToGlow(Interface.ConvertHandRankToTextNumber(winningHand.Rank)),
           losingBoardCards, cardsDeckUi.boardTransform,
           () =>
           {
               EndRoundVisual(isPlayerWin);
           });
    }

    private int ConvertRankToCardToGlow(int rank)
    {
        switch (rank)
        {
            case 10:
                return 1;
            case 9:
                return 2;
            case 8:
            case 3:
                return 4;
            case 7:
                return 3;/*
            case 6:
            case 5:
            case 4:
            case 2:
            case 1:
                return 5;*/
        }
        return 5;
    }

    private bool IsStrightOrFlush(int rank)
    {
        return (rank <= 1609 && rank >= 323) || rank <= 166;
    }

    private List<CardUi> RearangeWinningCards(List<CardUi> winningHand, bool isStrightOrFlush)
    {
        List<CardUi> aragnedCards = new List<CardUi>();

        winningHand = winningHand.OrderByDescending(h => Card.StringValueToInt(h.cardDescription[0].ToString())).ToList<CardUi>();
        if (isStrightOrFlush)
        {
            return winningHand;
        }
        else
        {
            List<CardUi> repetitive = new List<CardUi>();
            string lastRepeatValue = "";
            for (int i = 4; i > 0; i--)
            {
                if (lastRepeatValue == winningHand[i].cardDescription[0].ToString())
                {
                    repetitive.Add(winningHand[i]);
                    winningHand.RemoveAt(i);
                }
                else if (winningHand[i].cardDescription[0].ToString() == winningHand[i - 1].cardDescription[0].ToString())
                {
                    lastRepeatValue = winningHand[i].cardDescription[0].ToString();
                    repetitive.Add(winningHand[i]);
                    repetitive.Add(winningHand[i - 1]);
                    winningHand.RemoveAt(i);
                    winningHand.RemoveAt(i - 1);
                    i--;
                }
            }
            aragnedCards = aragnedCards.Concat(repetitive).ToList();
            aragnedCards = aragnedCards.OrderByDescending(h => Card.StringValueToInt(h.cardDescription[0].ToString())).ToList<CardUi>();
        }
        aragnedCards = aragnedCards.Concat(winningHand).ToList();
        return aragnedCards;
    }
    internal void OnPuClick(PowerUpUi powerUpUi)
    {
        if (infoShow)
        {
            HideDialog();
        }
        else if (powerUpUi.isPlayer && !powerUpUi.freeze && powerUpUi.isClickable)
        {
            powerUpUi.isClickable = false;
            powerUpUi.aboutToDestroy = true;
            Interface.EnablePlayerButtons(false);
            DisablePlayerPus();
            Interface.EnableDarkScreen(powerUpUi.isPlayer, true, null);
            if (powerUpUi.puIndex != -1)
            {
                powerUpUi.AnimatePuUse(() => OnPowerUpPress(powerUpUi.puName, powerUpUi.puIndex, powerUpUi.energyCost), null);
            }
            else
            {
                OnPowerUpPress(powerUpUi.puName, powerUpUi.puIndex, powerUpUi.energyCost);
            }
        }
        else if (powerUpUi.isPlayer && powerUpUi.replaceMode)
        {
            ReplacePu(true, powerUpUi.puIndex);
        }
        else if (powerUpUi.isPlayer || !powerUpUi.isPlayer)
        {
            StartCoroutine(AnimationManager.Instance.Shake(powerUpUi.spriteRenderer.material, Values.Instance.disableClickShakeDuration));
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CantClick, false);
        }
    }

    public void OnCancel(BaseEventData eventData)
    {
        Debug.LogWarning("cancel " + eventData.ToString());
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.LogWarning("pointerD " + eventData.button.ToString());
        ui.playerNameText.text = eventData.button.ToString();
        ui.enemyNameText.text = eventData.GetHashCode().ToString();
    }
    public void NextPokerTutorial()
    {
        if (pokerTutoPhase == 4)
        {
            pokerTutorial.SetActive(false);
            StartNinjaTuto();
        }
        else
        {
            ui.LoadNextPokerTutoImage(++pokerTutoPhase);
        }
    }

    public void ShowHandRanking()
    {
        if (pokerTutoPhase == 4)
        {
            ui.LoadNextPokerTutoImage(3);
            pokerTutoPhase = 3;
        }
    }

    public void ShowExitDialog(bool enable)
    {
        dialogInfo.SetActive(enable);
    }

    private void OnGUI()
    {
        if (Input.GetKeyUp("escape"))
        {
            ShowExitDialog(true);
        }
    }


}

