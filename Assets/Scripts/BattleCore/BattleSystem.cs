
using Firebase.Functions;
using Managers;
using Serializables;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UniRx;

using StandardPokerHandEvaluator;
using Sirenix.OdinInspector;

public class BattleSystem : StateMachine, ITimeOut
{
    public static BattleSystem Instance { get; private set; }

    private bool TEST_MODE;
    [SerializeField] public bool END_TURN_AFTER_PU;

    public BattleUI Interface => ui;


    public bool TemproryUnclickable = false;
    [SerializeField] private BattleUI ui;
    [SerializeField] public PlayerInfo player;
    [SerializeField] public PlayerInfo enemy;


    public int cardsToSelectCounter = 0;
    private string firstCardTargetPU = "";
    private Vector2 firstPosTargetPU;

    public CardsDeckUi cardsDeckUi;
    public PuDeckUi puDeckUi;
    private GameManager gameManager;
    private GameInfo currentGameInfo;
    public PowerUpUi skillBtn;

    public string newPowerUpName;
    private string puDisplayName;
    private int newPuSlotIndexUse;
    private int newEnergyCost;

    //Game Settings
    private bool firstDeck = true;



    public int prizeAmount = 1000;
    public int currentTurn = 6;
    private bool firstRound = true;
    private bool gameEndByBet = false;
    public Timer turnTimer;
    public bool enemyPuIsRunning;
    internal bool readyToPlay = false;
    private bool playerPuFreeze = false;
    private bool enemyPuFreeze = false;
    public bool isPlayerActivatePu = false;
    public int playerLifeLeft;
    public int enemyLifeLeft;

    public int replacePuLeft;
    public int skillUseLeft;

    public bool sameCardsSelection;
    private bool manualPuDeck = false;
    internal bool resetAllCardsSelectionWhenCardClicked;

    public bool selectMode;
    public bool playerPuInProcess;
    private bool waitForDrawerAnimationToEnd = false;

    private int currentRound = 1;
    public bool endRoutineFinished = false;
    private bool playersReadyForNewRound = false;
    public bool gameOver = false;
    public bool startNewRound;
    private bool deckGenerate = false;
    public bool infoShow = false;
    public bool skillUsed = false;
    public bool btnReplaceClickable = false;
    private bool timedOut;
    private bool emojisWheelDisplay;
    private bool emojiCooledDown = true;
    private readonly long TURN_COUNTER_INIT = 6;
    private readonly float DELAY_BEFORE_NEW_ROUND = 6f;



    [SerializeField]
    public bool isRoundReady
    {
        get => playersReadyForNewRound && endRoutineFinished && !gameOver;
        set
        {
            playersReadyForNewRound = false;
            endRoutineFinished = false;
        }
    }
    //   public int _energyCounter;
    public int energyCounter;

    /*   get => _energyCounter;
       set
       {
           int additionalEnergy = value - _energyCounter;
           Debug.LogError("was " + _energyCounter);
           Debug.LogError("now " + value);
           Debug.LogError("add " + additionalEnergy);
           _energyCounter = value;
       }*/




    public bool replaceMode = false;
    public bool endTurnInProcess = false;

    public event Action onGameStarted;
    private void Start()
    {
        TEST_MODE = Values.Instance.TEST_MODE;
        //Debug.LogWarning("S:" + PowerUpStruct.PowerUpNamesEnum.f1.ToString());
        Debug.LogWarning("Start LRA");

        if (TEST_MODE)
        {
            manualPuDeck = true;
            currentGameInfo = new GameInfo();
            currentGameInfo.gameId = "zxc";
            currentGameInfo.prize = 1000;
            currentGameInfo.playersIds = new String[] { "1", "2" };
            currentGameInfo.localPlayerId = "1";
            currentGameInfo.EnemyId = "2";
            currentGameInfo.cardDeck = new String[] { "Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah",
                "Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah",
                "Ac", "Ah","4s","2h","6c", "5h","Ts","Jh","Qc", "Ks","As","Kh"};
            currentGameInfo.puDeck = new String[] {"f2","i3","f3","f2","i3","f3",
                 "i1","f1","i2",
                 "w1","w2","w3","f2","i3","w1","w2","w3","f2","i3","f3",
                 "i1","f1","i2",
                 "w1","w2","w3",
                    "f2","i3","f3",
                 "i1","f1","i2",
                 "w1","w2","w3",
                 "fm1","w3"};
            currentGameInfo.turn = "1";
        }
        else
        {
            gameManager = MainManager.Instance.gameManager;
            currentGameInfo = gameManager.currentGameInfo; // Current Gameinfo or battleSettings??

        }
        player = new PlayerInfo();
        enemy = new PlayerInfo();
        player.id = currentGameInfo.localPlayerId;
        enemy.id = currentGameInfo.EnemyId;
        playerLifeLeft = 2;
        enemyLifeLeft = 2;

        /*replacePuLeft = Values.Instance.replaceUseLimit;
        skillUseLeft = Values.Instance.skillUseLimit;*/
        //FirstToPlay(true);


        Interface.Initialize(player, enemy);
        startNewRound = true;
        // StartCoroutine(InitGameListeners());

        ui.InitAvatars();
        if (!TEST_MODE)
        {
            LocalTurnSystem.Instance.Inito(() => StartCoroutine(InitGameListeners()));
        }
        else
        {
            if (firstDeck)
            {
                firstDeck = false;
                SetState(new BeginRound(this, true, true));
            }
        }
    }


    public void Awake()
    {
        // Time.fixedDeltaTime = 0.01333333f;
        // Application.targetFrameRate = 60;
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

    public void DisableSelectMode(bool endTurn)
    {
        if (selectMode && !TemproryUnclickable)
        {
            TemproryUnclickable = true;
            selectMode = false;
            sameCardsSelection = false;
            isPlayerActivatePu = false;
            ui.EnableVisionClick(true);
            resetAllCardsSelectionWhenCardClicked = false;
            ui.InitLargeText(false, "");
            firstCardTargetPU = "";
            firstPosTargetPU = new Vector2(0, 0);
            ui.EnableDarkScreen(false, () =>
            {

                StartCoroutine(ResetSortingOrder(false));
                puDeckUi.EnablePusZ(true, false);
                cardsDeckUi.DisableCardsSelection(Constants.AllCardsTag);
                ActivatePlayerButtons(!endTurn, false);
                TemproryUnclickable = false;
            });
        }
        if (replaceMode && !TemproryUnclickable)
        {
            EnableReplaceDialog(true, endTurn);
        }
        if (!endTurn && emojisWheelDisplay)
        {
            ShowEmojiWheel(false);
        }
    }
    public void LoadMenuScene(bool playAgain)
    {
        if (TEST_MODE)
        {
            SceneManager.LoadScene("GameScene2");
        }
        else
        {
            //SoundManager.Instance.StopMusic();
            SaveAutoPlayAgain(playAgain);
            SceneManager.LoadScene("GameMenuScene");
            Destroy(GameObject.Find("AnimationManager"));
            Destroy(GameObject.Find("ObjectPooler"));
            Destroy(GameObject.Find("LocalTurnSystem"));
            Destroy(GameObject.Find("SoundManager"));
            Destroy(GameObject.Find("BattleSystem"));
        }
    }

    public void PlayMusic(bool enable)
    {
        SoundManager.Instance.PlayMusic();
        /*if (enable)
        {
        }
        else
        {
            SoundManager.Instance.StopMusic();
        }*/
    }




    private IEnumerator InitGameListeners()
    {
        yield return new WaitForSeconds(1f);

        BindTurnCounter();
        BindRoundCounter();
        BindPlayersReady();
        BindCurrentPlayer();
        ListenForNewDeck();
        ListenForPowerupUse();
        ListenForEmoji();


    }

    private void ListenForEmoji()
    {
        gameManager.ListenForEmoji(player.id, emojiId =>
        {
            StartCoroutine(ui.DisplayEmoji(emojiId,null));
        }, Debug.Log) ;
    }

    [Button]
    public void SoundCheck()
    {
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndRoundGong);
    }

    public IEnumerator StartNewRound()
    {

        yield return new WaitForSeconds(0.2f);
        StartCoroutine(CheckIfNewRoundReadyAndStart());
    }

    private IEnumerator CheckIfNewRoundReadyAndStart()
    {
        if (isRoundReady)
        {
            ui.winLabel.SetActive(false);
            SetState(new BeginRound(this, LocalTurnSystem.Instance.IsPlayerStartRound(), false));
        }
        else
        {
            yield return new WaitForSeconds(0.2f);
            Debug.LogError("checkagain!");
            StartCoroutine(CheckIfNewRoundReadyAndStart());

        }
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


    public void UpdateHandRank(bool reset)
    {
        if (!AreBoardCardsFlipped() && !reset)
        {
            Hand bestHand = cardsDeckUi.CalculateHand(false);
            ui.UpdateCardRank(bestHand.Rank);
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
                SetState(new EndRound(this, false, false));
                break;
        }
    }

    public void StartNewRoundRoutine(bool delay)
    {
        StartCoroutine(StartNewRoundRoutineWithDelay(delay));
    }
    private IEnumerator StartNewRoundRoutineWithDelay(bool delay)
    {
        if (delay)
        {
            yield return new WaitForSeconds(3.5f);
        }
        if (!deckGenerate && LocalTurnSystem.Instance.isPlayerFirstPlayer)
        {
            deckGenerate = true;
            DeckGeneratorDB();
        }
    }

    private IEnumerator CheckIfEnemyPuRunningAndStartPlayerTurn()
    {
        if (!enemyPuIsRunning)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndTurnGong);
            SetState(new PlayerTurn(this, currentTurn));
        }
        else
        {
            while (enemyPuIsRunning)
            {
                yield return new WaitForSeconds(0.6f);
                if (!enemyPuIsRunning)
                {
                    StartCoroutine(CheckIfEnemyPuRunningAndStartPlayerTurn());
                }
            }
        }

    }

    public void ResetTimers()
    {
        turnTimer.StopTimer();
    }


    public void EndTurn()
    {
        if (!endTurnInProcess)
        {
            endTurnInProcess = true;
            DisableSelectMode(true);
            ui.EnableBgColor(false);
            StartCoroutine(AnimationManager.Instance.AlphaAnimation(ui.turnTextGO.GetComponent<SpriteRenderer>(), false, Values.Instance.textTurnFadeOutDuration, null));
            DisablePlayerPus();
            ResetTimers();
            gameManager.AddGameActionLog(GameManager.ActionEnum.EndTurn, "end of turn: " + currentTurn, () => { }, Debug.Log);
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndTurnGong);
            LocalTurnSystem.Instance.PassTurn();
        }
    }


    public void OnEndTurnButton()
    {
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.BtnClick1);
        EndTurn();
    }
    private void SaveTurnOf(string whatPlayer)
    {
        PlayerPrefs.SetString("turn", whatPlayer);
    }

    public void OnTimeOut()
    {
        if (IsPlayerTurn())
        {
            if (newPowerUpName.Equals("fm1"))
            {
                Debug.LogError("howmany");
                ResetValuesAfterCardSelection(Constants.AllCardsTag);
                SetState(new PowerUpState(this, true, newEnergyCost, newPowerUpName, Constants.PlayerCard1, Constants.deckCardsNames[0], cardsDeckUi.GetCardPosition(Constants.PlayerCard1), cardsDeckUi.GetCardPosition(Constants.deckCardsNames[0]), newPuSlotIndexUse));
                timedOut = true;
            }
            else
            {
                StartCoroutine(WaitForPuToEndLoop());
            }

        }
    }

    private IEnumerator WaitForPuToEndLoop()
    {
        DisableSelectMode(true);
        if (playerPuInProcess)
        {
            playerPuInProcess = false;

            yield return new WaitForSeconds(5f);
        }
        EndTurn();

        /* while (playerPuInProcess)
         {
             yield return new WaitForSeconds(0.3f);
             if (playerPuInProcess)
             {
                 StartCoroutine(WaitForPuToEndLoop());
                 break;
             }
             else
             {
                 EndTurn();
             }
         }
         if (!playerPuInProcess)
         {
             EndTurn();
         }*/
    }

    internal void NewTimerStarter(bool isPlayer)
    {
        //  Interface.FlipTurnIndicator(isPlayer);
        StartCoroutine(turnTimer.StartTimer(Values.Instance.turnTimerDuration));
    }
    #endregion


    #region Deck
    public void InitDecks()
    {
        cardsDeckUi = CardsDeckUi.Instance();
        cardsDeckUi.InitDeckFromServer(currentGameInfo.cardDeck);

        cardsDeckUi.isPlayerFirst = IsPlayerTurn();


        puDeckUi = PuDeckUi.Instance();
        if (firstDeck)
        {
            puDeckUi.InitDeckFromServer(currentGameInfo.puDeck);
        }
        else if (manualPuDeck)
        {
            puDeckUi.InitDeckFromServer(currentGameInfo.puDeck);
        }
    }
    private void DealHands(Action FinishCallback)
    {
        waitForDrawerAnimationToEnd = true;
        StartCoroutine(cardsDeckUi.CreateHands(() => waitForDrawerAnimationToEnd = false, FinishCallback));

    }

    private void ListenForNewDeck()
    {

        gameManager.ListenForNewDeck(() =>
        {
            deckGenerate = false;
            currentGameInfo = gameManager.currentGameInfo;
            if (firstDeck)
            {
                //  SetState(new BeginRound(this, LocalTurnSystem.Instance.IsPlayerStartRound(), true));
                // LocalTurnSystem.Instance.InitBind("");
                SetState(new BeginRound(this, currentGameInfo.localPlayerId.ToString().Equals(currentGameInfo.playersIds[0]), true));
                //  LocalTurnSystem.Instance.StartGame(currentGameInfo.playersIds[0]);

            }
            else
            {
                ResetRoundAndTurnCounter();

            }
            Debug.Log("New Deck is here!" + gameManager.currentGameInfo.cardDeck[51]);
        }, Debug.Log);
    }

    private void ResetRoundAndTurnCounter()
    {
        if (LocalTurnSystem.Instance.isPlayerFirstPlayer)
        {
            LocalTurnSystem.Instance.RoundCounter.Value = currentRound + 1;
            LocalTurnSystem.Instance.TurnCounter.Value = TURN_COUNTER_INIT;
        }
    }

    public void DeckGeneratorDB()
    {

        //  ui.playerNameText.text = " generate";
        //ui.playerNameText.text = "I Generate";
        var data = new Dictionary<string, object>();
        data["gameId"] = currentGameInfo.gameId;
        //Call the function and extract the operation from the result.
        var function = FirebaseFunctions.DefaultInstance.GetHttpsCallable("generateNewDeck");
        function.CallAsync(data).ContinueWith((task) =>
        {
            if (task.IsFaulted)
            {
                foreach (var inner in task.Exception.InnerExceptions)
                {
                    if (inner is FunctionsException)
                    {
                        Debug.Log(inner.Message);
                    }
                }
            }
            else
            {
                gameManager.AddGameActionLog(GameManager.ActionEnum.GenerateDeck, "Player generate deck", () => { }, Debug.Log);
            }
        });
    }
    private void BindRoundCounter()
    {

        LocalTurnSystem.Instance.RoundCounter.onValueChanged += i =>
        {
            if ((int)i != 1)
            {

                if (!gameOver && currentRound + 1 == (int)i)
                {
                    currentRound = (int)i;
                    LocalTurnSystem.Instance.PlayerReady.Value = currentRound;
                    StartCoroutine(StartNewRound());
                }
                else
                {
                    Debug.LogError("Round not Sync");
                }
            }

        };
    }

    private void BindCurrentPlayer()
    {

        LocalTurnSystem.Instance.CurrentPlayerID.onValueChanged += s =>
        {
            if (s == LocalTurnSystem.Instance.PlayerID.Value)
            {
                ResetTimers();
                StartCoroutine(AnimationManager.Instance.AlphaAnimation(ui.turnTextGO.GetComponent<SpriteRenderer>(), false, Values.Instance.textTurnFadeOutDuration, null));
                StartCoroutine(CheckIfEnemyPuRunningAndStartPlayerTurn());
            }
            else
            {
                SetState(new EnemyTurn(this, currentTurn));
            }


        };
    }


    private void BindTurnCounter()
    {
        LocalTurnSystem.Instance.onGameStarted += () =>
        {
            onGameStarted?.Invoke();
        };

        onGameStarted += () => { Debug.Log("Aviad here it is!"); };

        LocalTurnSystem.Instance.TurnCounter.onValueChanged += i =>
        {
            currentTurn = (int)i;
            TurnEvents(currentTurn);
        };
    }
    private void BindPlayersReady()
    {

        LocalTurnSystem.Instance.OtherPlayerReady.onValueChanged += i =>
        {
            SetPlayersReady((int)i);
        };
        LocalTurnSystem.Instance.PlayerReady.onValueChanged += i =>
         {
             SetPlayersReady((int)i);
         };
    }

    private void SetPlayersReady(int i)
    {
        if (currentRound == i && LocalTurnSystem.Instance.PlayerReady.Value == i)
        {
            playersReadyForNewRound = true;
        }
        else
        {
            Debug.LogError("Players Not Synces2");
        }
    }

    #endregion

    #region Settings
    public void ResetRoundSettings(Action FinishCallbac)
    {
        TemproryUnclickable = false;
        ui.tieTitle.SetActive(false);
        ui.EnableBgColor(false);
        isRoundReady = false;
        if (playerPuFreeze || enemyPuFreeze)
        {
            selectMode = false;
            playerPuFreeze = false;
            enemyPuFreeze = false;
        }

        ui.ResetTurnPanels();
        // currentTurn = 1;
        if (firstRound)
        {
            currentRound = 1;
            if (!TEST_MODE)
            {
                LocalTurnSystem.Instance.RoundCounter.Value = 1;
            }
            firstRound = false;
        }
        UpdateHandRank(true);
        cardsDeckUi.DeleteAllCards(() => DealHands(FinishCallbac));
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

    public void UpdateWinnerDB()
    {
        gameManager.UpdateWinnerDB(player.id, () =>
                    Debug.Log("Winner updated!"),
                    Debug.LogError);
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

            SetState(new PowerUpState(this, true, energyCost, newPowerUpName, "", "", new Vector2(0, 0), new Vector2(0, 0), newPuSlotIndexUse));
        }
        else if (energyCounter - energyCost < 0)
        {
            Debug.LogWarning("YouDontHaveEnoughEnergy");
        }
    }

    public void ReduceSkillUse()
    {

        if (--skillUseLeft == 0)
        {
            puDeckUi.EnablePlayerSkill(false);
        }
    }

    public bool IsPlayerTurn()
    {
        if (!TEST_MODE)
        {
            return LocalTurnSystem.Instance.IsPlayerTurn();
        }
        return true;
    }

    public void DissolvePuAfterUse(bool isPlayer, int index)
    {
        puDeckUi.GetPuFromList(isPlayer, index).DissolvePu(2f, Values.Instance.puDissolveDuration, null, () => ResetPuUi(isPlayer, index));
    }

    private void ListenForPowerupUse()
    {
        gameManager.ListenForPowerupUse(powerUpInfo =>
        {
            if (!powerUpInfo.playerId.Equals(player.id))
            {
                isPlayerActivatePu = false;
                enemyPuIsRunning = true;
                ui.EnableDarkScreen(true, null);
                currentGameInfo.powerup = powerUpInfo;
                if (powerUpInfo.slot != -1)
                {
                    puDeckUi.GetPu(false, powerUpInfo.slot).AnimatePuUse(
                              () => SetState(new PowerUpState(this, false, 0, powerUpInfo.powerupName, powerUpInfo.cardPlace1, powerUpInfo.cardPlace2,
                              cardsDeckUi.GetCardPosition(powerUpInfo.cardPlace1), cardsDeckUi.GetCardPosition(powerUpInfo.cardPlace2), powerUpInfo.slot)), null);
                }
                else // SKILL
                {
                    SetState(new PowerUpState(this, false, 0, powerUpInfo.powerupName, powerUpInfo.cardPlace1, powerUpInfo.cardPlace2, new Vector2(0, 0), new Vector2(0, 0), powerUpInfo.slot));
                }
            }
        }, powerUpInfo =>
        {
            if (!powerUpInfo.playerId.Equals(player.id))
            {
                ReplacePu(false, powerUpInfo.slot);
            }
        }, Debug.Log);
    }



    public IEnumerator OnCardsSelectedForPU(string cardPlace, Vector2 position)
    {
        if (cardsToSelectCounter == 0)
        {
            //  playerPuInProcess = true;

            ResetValuesAfterCardSelection(cardPlace);

            yield return new WaitForSeconds(0.5f);
            //   selectMode = false;
            /* if (newPowerUpName.Equals("f2"))
             {
             SetState(new PowerUpState(this, true, newPowerUpName, cardPlace, firstCardTargetPU,  firstPosTargetPU, position, newPuSlotIndexUse));
             }
             else
             {*/
            SetState(new PowerUpState(this, true, newEnergyCost, newPowerUpName, firstCardTargetPU, cardPlace, firstPosTargetPU, position, newPuSlotIndexUse));

            firstPosTargetPU = new Vector2(0, 0);
            firstCardTargetPU = "empty";
        }
        else if (cardsToSelectCounter == 1)
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

    public void ShowPuInfo(Vector2 startingPosition, string puName, string puDisplayName)
    {
        infoShow = true;
        ui.ShowPuInfoDialog(startingPosition, puName, puDisplayName, true, false, null);
    }
    public void HideDialog()
    {
        ui.ShowPuInfoDialog(new Vector2(0, 0), " ", " ", false, false, () => infoShow = false);
    }

    internal void ResetPuUi(bool isPlayer, int puIndex)
    {
        PowerUpUi pu = puDeckUi.GetPu(isPlayer, puIndex);
        puDeckUi.RemovePuFromList(isPlayer, puIndex);
        puDeckUi.ResetPuUI(pu, null);
        if (isPlayer)
        {
            StartCoroutine(AutoEndTurn());
        }
    }
    internal void ResetPuUi(PowerUpUi pu)
    {
        puDeckUi.RemovePuFromList(pu.isPlayer, pu.puIndex);
        puDeckUi.ResetPuUI(pu, null);
    }


    public void UpdatePuInDb(string firstCardTargetPUstring, string secondCardTargetPU, int puIndex)
    {
        string cardTarget;
        if (firstCardTargetPUstring == string.Empty)
        {
            cardTarget = firstCardTargetPU;
        }
        else
        {
            cardTarget = firstCardTargetPUstring;
        }
        if (!TEST_MODE)
            gameManager.SetNewPowerupUseDB(new PowerUpInfo(player.id, newPowerUpName, Constants.Instance.ConvertCardPlaceForEnemy(cardTarget), Constants.Instance.ConvertCardPlaceForEnemy(secondCardTargetPU), puIndex, CreateTimeStamp()), () =>
        {
            gameManager.AddGameActionLog(GameManager.ActionEnum.PuUse, "name: " + newPowerUpName + " c1: " + cardTarget + " c2: " + secondCardTargetPU, () => { }, Debug.Log);

        }, Debug.Log);
    }

    public void UpdateReplacePuInDb(int puIndex)
    {
        if (!TEST_MODE)
            gameManager.SetNewPowerupUseDB(new PowerUpInfo(player.id, "replace", "empty", "empty", puIndex, CreateTimeStamp()), () =>
            {
                gameManager.AddGameActionLog(GameManager.ActionEnum.ReplacePu, "index: " + puIndex, () => { }, Debug.Log);

            }, Debug.Log);
    }

    private long CreateTimeStamp()
    {
        return TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.Local).Ticks;
    }
    internal void ReplacePu(bool isPlayer, int puIndex)
    {
        if (isPlayer)
        {
            TemproryUnclickable = true;
            DisablePlayerPus();
            ReduceEnergy(1);
            /* if (--replacePuLeft == 0)
             {
                 EnableBtnReplace(false);
             }*/
            //  energyCounter--;
            UpdateReplacePuInDb(puIndex);
            // EnableReplaceDialog();
            //  ActivatePlayerPus();
        }
        puDeckUi.ReplacePu(isPlayer, puIndex, () =>
        {
            if (isPlayer)
            {

                EnableReplaceDialog(true, false);
                if (energyCounter == 0)
                {
                    EndTurn();
                }
                else
                {
                    TemproryUnclickable = false;
                }
            }
            /*  if (isPlayer && energyCounter == 0)
              {
                  EndTurn();
              }
              else if (isPlayer)
              {
                  ActivatePlayerPus();
              }*/
        });
    }

    private void EnableBtnReplace(bool enable)
    {
        //btnReplaceClickable = enable;
        ui.EnableBtnReplace(enable);
    }

    public void ActivatePlayerPus()
    {
        foreach (PowerUpUi puUi in puDeckUi.GetPuList(true))
        {
            if (puUi != null)
            {
                CheckIfPuAvailable(puUi);
            }
        }
        CheckIfPuAvailable(skillBtn);
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
        if (energyCounter - puUi.energyCost < 0 || puUi.puIndex == -1 && skillUseLeft == 0)
        {
            puUi.EnablePu(false);
        }
        else
        {
            if (puUi.puElement.Equals("i") || puUi.puElement.Equals("w") && !puUi.puElement.Contains("wm"))
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
        /*if (currentTurn < 3 && cardPlace.Contains("River"))
        {
            Debug.LogError("card not excist");

        }
        else if ((currentTurn < 3 && cardPlace.Contains("Turn")))
        {
            Debug.LogError("card not excist");
        }
        else
        {
            if (currentTurn < 3 && cardPlace.Contains("Flop3"))
            {
                ResetEnable = true;
            }
        }*/
        if (puDeckUi.IsCardFreeze(cardPlace))
        {
            FreezePlayingCard(cardPlace, false, ResetEnable);
        }
        else
        {
            bool isFlip = IsEnemyCard(cardPlace);
            StartCoroutine(ReplaceSelectedCard(cardPlace, isFlip, delay, ResetEnable, firstCard, lastCard));
        }
    }

    private bool IsEnemyCard(string cardPlace)
    {
        return cardPlace.Contains("Enemy");
    }

    public IEnumerator ReplaceSelectedCard(string cardPlace, bool isFlip, float delay, bool ResetEnable, bool isFirstCard, bool isLastCard)
    {
        yield return new WaitForSecondsRealtime(delay);
        cardsDeckUi.DestroyCardObject(cardPlace, null);
        yield return new WaitForSeconds(0.4f);
        Action resetAction = null;
        if (ResetEnable)
        {
            resetAction = () => EnableDarkAndSorting(false);
        }
        cardsDeckUi.DrawAndReplaceCard(cardPlace, isFlip, resetAction, isFirstCard, isLastCard);
    }

    private void EnableDarkAndSorting(bool enable)
    {
        if (!enable)
        {
            ui.EnableVisionClick(true);
            newPowerUpName = "x";
            //selectCardsMode = true;
        }
        ui.EnableDarkScreen(enable, () => StartCoroutine(ResetSortingOrder(enable)));
    }

    public void UpdateZPos(bool aboveDarkScreen, string tag)
    {
        foreach (CardUi card in cardsDeckUi.GetListByTag(tag))
        {
            card.EnableSelecetPositionZ(aboveDarkScreen);
        }
    }

    private IEnumerator ResetSortingOrder(bool enable)
    {
        UpdateHandRank(false);
        yield return new WaitForFixedUpdate();
        if (!enable)
        {
            enemyPuIsRunning = false;
            playerPuInProcess = false;
            foreach (CardUi card in cardsDeckUi.GetListByTag("All"))
            {
                card.EnableSelecetPositionZ(false);
            }
        }
        /*if (!enable && isPlayerActivatePu)
        {
            StartCoroutine(AutoEndTurn());
        }*/
    }

    private IEnumerator AutoEndTurn()
    {
        if (END_TURN_AFTER_PU)
        {
            yield return new WaitForSeconds(0.5f);
            EndTurn();
        }

        else if (timedOut || energyCounter == 0 || energyCounter == 1 && puDeckUi.GetPuListCount(true) == 0 && !skillUsed)
        {
            timedOut = false;
            yield return new WaitForSeconds(1.5f);
            EndTurn();
        }
        else
        {
            ui.EnablePlayerButtons(true);
            ActivatePlayerPus();
        }
    }

    [Button]
    public void ClickForInfo()
    {
        Debug.LogError("HowM " + puDeckUi.GetPuListCount(true));

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
        this.cardsToSelectCounter = cardsToSelectCounter;
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
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.PuSee);
        cardsDeckUi.FlipCardPu(cardTarget2, isPlayerActivate, () =>
        {
            EnableDarkAndSorting(false);
            if (isPlayerActivate)
            {
                StartCoroutine(AutoEndTurn());
            }
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

        StartCoroutine(DealPuCourotine(isPlayer, OnEnd));

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

        ui.InitProjectile(puDeckUi.GetPuPosition(isPlayerActivate, puIndex), inlargeProjectile, powerUpName[0].ToString(), posTarget1, posTarget2, IgnitePowerUp);
    }

    #endregion

    #region Buttons


    public void EnableReplaceDialog(bool disable, bool endTurn)
    {
        if (btnReplaceClickable && energyCounter > 0 && !disable && puDeckUi.GetPuListCount(true) < 2)
        {
            ReduceEnergy(1);
            ui.EnablePlayerButtons(false);
            DisablePlayerPus();
            DealPu(true, () =>
            {
                ui.EnablePlayerButtons(true);
                ActivatePlayerPus();
            });
        }
        else if (btnReplaceClickable || endTurn || disable)
        {

            if (/*replaceMode ||*/ IsPlayerTurn() && energyCounter > 0)
            {
                PowerUpUi[] playerPus = puDeckUi.GetPuList(true);
                if (playerPus[0] != null || playerPus[1] != null)
                {
                    replaceMode = !replaceMode;
                    if (replaceMode)
                    {
                        puDeckUi.EnablePusSlotZ(true, true);
                        ui.EnableDarkScreen(true, null);
                    }
                    else
                    {
                        ui.EnableDarkScreen(false, () =>
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
        }
        else
        {
            ui.DisableClickBtnReplace();
        }
    }



    public void SlideRankingImg()
    {
        ui.SlideRankingImg();
    }



    public void OnVisionBtnDown()
    {
        if (!AreBoardCardsFlipped() && !selectMode)
        {
            Hand bestHand = cardsDeckUi.CalculateHand(false);
            ui.VisionEffect(bestHand.getCards(), cardsDeckUi.boardCardsUi, cardsDeckUi.playerCardsUi);
            ui.UpdateCardRank(bestHand.Rank);
            ui.UpdateRankTextInfo(true, bestHand.Rank);
            SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.Vision, true);

        }
    }
    public void OnVisionBtnUp()
    {
        if (!AreBoardCardsFlipped() && !selectMode)
        {
            AnimationManager.Instance.VisionEffect(cardsDeckUi.GetBoardAndPlayerHandList(), false);
        }
        SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.Vision, false);
        ui.UpdateRankTextInfo(false, 0);

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
                ActivatePlayerPus();
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
        ActivatePlayerPus();
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
                        ui.UpdateEnergy(true, 2);
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
        ShowEmojiWheel(false);
        if (emojiCooledDown && id != -1)
        {
            emojiCooledDown = false;
            StartCoroutine(ui.DisplayEmoji(id,() => emojiCooledDown = true));
            UpdateEmojiDB(id);
        }
        else if(!emojiCooledDown)
        {
            Debug.LogError("Wait A Sec");
        }
    }
    public void ShowEmojiWheel(bool enable)
    {
        ui.ShowEmojiWheel(enable);
        //ui.EnableDarkScreen(enable, null);
        emojisWheelDisplay = enable;
    }

    private void UpdateEmojiDB(int emojiId)
    {
        gameManager.UpdateEmojiDB(new EmojiInfo(currentGameInfo.localPlayerId, emojiId, CreateTimeStamp()), () =>
                             Debug.Log("EmojiSent" + emojiId),
                            Debug.LogError);
    }

    [Button]
    internal void WinParticleEffect()
    {
        ui.WinParticleEffect();
    }
    /* internal void FreezePu(string puTarget, bool isToFreeze)
     {
         PowerUpUi cardToFreeze = GameObject.Find(puTarget).GetComponent<PowerUpUi>();
         cardToFreeze.freeze = isToFreeze;
         ui.FreezeObject(cardToFreeze.spriteRenderer, isToFreeze, () => EnableDarkAndSorting(false));
     }*/
    /*    private string[] Get2RandomDigitsForBoardCards()
        {
            int boardCound = cardsDeckUi.boardCards.Count;
            int firstToBlind = UnityEngine.Random.Range(1, boardCound);
            int secondToBlind = UnityEngine.Random.Range(1, boardCound);

            while (firstToBlind == secondToBlind)
            {
                secondToBlind = UnityEngine.Random.Range(1, boardCound);
            }
            return new string[] { cardsDeckUi.GetBoardNameByIndex(firstToBlind), cardsDeckUi.GetBoardNameByIndex(secondToBlind) };

        }*/
    /*public void UpdateSmokePuInDb(string firstTarget, string secondTarget, int slotNumber)
    {
        gameManager.SetNewPowerupUseDB(new PowerUpInfo(player.id, newPowerUpName, firstTarget, secondTarget, slotNumber, createTimeStamp()), () =>
        {
            Debug.Log("update po use! " + newPowerUpName);
        }, Debug.Log);
    }*/
}

