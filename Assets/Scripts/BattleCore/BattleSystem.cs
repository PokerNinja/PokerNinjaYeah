
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

public class BattleSystem : StateMachine
{
    public static BattleSystem Instance { get; private set; }

    public bool BOT_MODE;
    public bool TUTORIAL_MODE;
    [SerializeField] public bool END_TURN_AFTER_PU;

    public TutorialManager tutoManager;

    public BattleUI Interface => ui;
    [SerializeField] private BattleUI ui;
    [SerializeField] public PlayerInfo player;
    [SerializeField] public PlayerInfo enemy;

    public string firstCardTargetPU = "";
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
    public bool firstRound = true;
    private bool gameEndByBet = false;
    public bool enemyPuIsRunning;
    internal bool readyToPlay = false;
    private bool playerPuFreeze = false;
    private bool enemyPuFreeze = false;
    public bool isPlayerActivatePu = false;



    public int replacePuLeft;
    public int skillUseLeft;

    public bool sameCardsSelection;
    private bool manualPuDeck = false;
    internal bool resetAllCardsSelectionWhenCardClicked;

    public bool endClickable;
    public bool selectMode;
    public bool playerPuInProcess;
    public bool ReplaceInProgress;
    private bool waitForDrawerAnimationToEnd = false;

    public int currentRound = 1;



    public bool endRoutineFinished = false;
    private bool playersReadyForNewRound = false;
    public bool gameOver = false;



    public bool startNewRound;
    private bool deckGenerate = false;
    public bool infoShow = false;



    public bool skillUsed = false; // IS needed?
    public bool btnReplaceClickable = false;
    public bool btnBetClickable = false;
    private bool timedOut;
    private bool emojisWheelDisplay;



    private bool emojiCooledDown = true;
    private readonly long TURN_COUNTER_INIT = 6;

    public bool isPlayerFlusher = false;
    public bool isPlayerStrighter = false;
    public bool isEnemyFlusher = false;
    public bool isEnemyStrighter = false;
    public bool playerHandIsFlusher = false;
    public bool playerHandIsStrighter = false;
    public bool enemyHandIsFlusher = false;
    public bool enemyHandIsStrighter = false;

    public bool visionUnavailable = false;
    public bool turnInitInProgress = false;
    public GameObject canvasExitDialog;

    public float fullHp;
    public float startingDamageForRound;
    public float currentDamageThisRound;
    public float playerHp;
    public float enemyHp;
    private float extraDamage;
    public bool raiseOffer;
    private bool readyToStart = false;
    private Action ResetPuAction;
    public float choosenRaise;



    public int[] raiseOptions = { 200, 500, 1000 };
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
    private bool fm1Activated;
    public string[] playersHand = { "Ac", "2d" };
    public string[] enemysHand = { "Ac", "2d" };
    public string[] board = { "3s", "4h", "5s", "6d", "7d" };
    private bool firstToPlayBotMode;
    public bool finishPuDissolve;
    public bool isPlayerBotModeTurn;

    public bool FlipTurnAfterDecline;


    public event Action onGameStarted;

    private void Start()
    {
        Constants.BOT_MODE = true;
        Constants.TUTORIAL_MODE = true;
        Debug.LogError("Alex " + Constants.BOT_MODE);
        Debug.LogError("Tu " + TUTORIAL_MODE);
        BOT_MODE = Constants.BOT_MODE;
        TUTORIAL_MODE = Constants.TUTORIAL_MODE;
        if (BOT_MODE)
        {
            InitTestMode();
        }
        else
        {
            gameManager = MainManager.Instance.gameManager;
            currentGameInfo = gameManager.currentGameInfo;
        }
        if (TUTORIAL_MODE)
        {
            fullHp = 2500;
        }
        else
        {
            fullHp = 3000;
        }
        playerHp = fullHp;
        enemyHp = fullHp;
        startingDamageForRound = 1000;
        currentDamageThisRound = startingDamageForRound;
        ui.hpForThisRoundText.text = currentDamageThisRound.ToString();

        player = new PlayerInfo();
        enemy = new PlayerInfo();
        player.id = currentGameInfo.localPlayerId;
        enemy.id = currentGameInfo.EnemyId;
        Interface.Initialize(player, enemy, fullHp);
        startNewRound = true;
        ui.LoadNinjaBG();
        ui.InitAvatars();
        ui.FillHp();
        if (!Debug.isDebugBuild)
        {
            Constants.IL2CPP_MOD = true;
        }
        if (!BOT_MODE)
        {
            LocalTurnSystem.Instance.Inito(() => StartCoroutine(InitGameListeners()));
            StartCoroutine(ui.CoinFlipStartGame(currentGameInfo.playersIds[0].ToString().Equals(player.id), () =>
            {
                StartCoroutine(ReadyToStart());
                //  ui.SlidePuSlots();
            }));
        }
        else
        {
            if (!TUTORIAL_MODE)
            {
                DisplayStatistics();
                StartBotGame();
            }
            else
            {
                tutoManager.texasDialog.SetActive(true);
            }
        }
    }

    public void StartBotGame()
    {
        Constants.TemproryUnclickable = false;
        firstToPlayBotMode = GetRandomFirstTurn();
        StartCoroutine(ui.CoinFlipStartGame(firstToPlayBotMode, () =>
        {
            if (firstDeck)
            {
                firstDeck = false;
                SetState(new BeginRound(this, firstToPlayBotMode, true));
            }
        }));
    }

    private IEnumerator ReadyToStart()
    {
        if (readyToStart)
        {
            SetState(new BeginRound(this, currentGameInfo.localPlayerId.ToString().Equals(currentGameInfo.playersIds[0]), true));
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            StartCoroutine(ReadyToStart());
        }
    }

    private bool GetRandomFirstTurn()
    {
        if (TUTORIAL_MODE)
        {
            return true;
        }
        else
        {
            return GetRandomBool();
        }

    }

    private bool GetRandomBool()
    {
        System.Random rng = new System.Random();
        return rng.Next(0, 2) > 0;
    }

    private void InitTestMode()
    {
        currentGameInfo = new GameInfo();
        currentGameInfo.gameId = CreateTimeStamp().ToString();
        currentGameInfo.prize = 1000;
        string playerName = PlayerPrefs.GetString("player_name", "Ninja");
        manualPuDeck = true;
        currentGameInfo.localPlayerId = LoadPlayerElementString() + playerName;
        currentGameInfo.EnemyId = GetRandomElement() + "Alex";
        currentGameInfo.playersIds = new String[] { currentGameInfo.localPlayerId, currentGameInfo.EnemyId };
        currentGameInfo.turn = "6";
        currentTurn = 6;
        currentGameInfo.cardDeck = CreateCardsDeck(TUTORIAL_MODE);
        currentGameInfo.puDeck = CreatePuDeck(TUTORIAL_MODE);
        SavePrefsInt(Constants.Instance.botEnergyKey.ToString(), 0);

    }

    private string GetRandomElement()
    {
        string[] elements = { "f", "i", "w" };
        string choosenEkement = elements[GenerateRandom(0, 3)];
        SavePrefsString(Constants.Instance.botEsElementKey, choosenEkement);
        return choosenEkement;
    }

    private string[] CreatePuDeck(bool tutorial)
    {
        string[] deck;
        if (!tutorial)
        {
            deck = new string[]{
              "f1","f2","f3","i1","i2","i3","w1","w2","w3",
              "f1","f2","f3","i1","i2","i3","w1","w2","w3",
              "f1","f2","f3","i1","i2","i3","w1","w2","w3",
              "f1","f2","f3","i1","i2","i3","w1","w2","w3",
              "f1","f2","f3","i1","i2","i3","w1","w2","w3",
              "fm1","fm2","im1","im2","wm1","wm2"};
            deck = ShuffleArray(deck);
        }
        else
        {
            deck = new String[] {"f2","i3","f3","f2","i3","f3",
                 "i1","f1","i2",
                 "w1","w2","w3","f2","i3","w1","w2","w3","f2","i3","f3",
                 "i1","f1","i2",
                 "w1","w2","w3",
                 "fm2","wm2","tm1",
                 "t6","t5","t4",
                 "t3","t2","t1",
            "w2","wm1","w1","wm2","wm2","wm2","w1","f3","f2",
          "w2" , "im2","f1","w2","i3","i3","f1"};
    }
        return deck;
    }

    private string[] CreateCardsDeck(bool tutorial)
    {
        string[] deck;
        if (!tutorial)
        {
            deck = new string[]{
                "Ac", "2c", "3c", "4c", "5c", "6c", "7c", "8c", "9c", "Tc", "Jc", "Qc", "Kc",
                       "Ad", "2d", "3d", "4d", "5d", "6d", "7d", "8d", "9d", "Td", "Jd", "Qd", "Kd",
                       "As", "2s", "3s", "4s", "5s", "6s", "7s", "8s", "9s", "Ts", "Js", "Qs", "Ks",
                       "Ah", "2h", "3h", "4h", "5h", "6h", "7h", "8h", "9h", "Th", "Jh", "Qh", "Kh" };
            deck = ShuffleArray(deck);
        }
        else
        {
            deck = new String[] { "Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah",
                "Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Jh","2c", "3h","4s","5h","Ac", "7s",
                "3s",
                "5c",
                "6h",
                "8d",
                "8h", "Jc","4s",
                "9d", "3c", "4c","Jh"
                /*,board[4],board[3], board[2],board[1],board[0],enemysHand[1], playersHand[1],enemysHand[0],playersHand[0]*/
            };
        }
        return deck;
    }

    private string[] ShuffleArray(string[] deck)
    {
        System.Random rnd = new System.Random();
        return deck.OrderBy(x => rnd.Next()).ToArray();
    }

    /*  public void UpdateMusicVolume()
      {
          float newVolume = ui.musicSlider.value;
          SoundManager.Instance.ChangeMusicVolume(newVolume);
          SavePrefs(Constants.Instance.volumeSoundKey, newVolume);
      }*/

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
        if (selectMode && !Constants.TemproryUnclickable)
        {
            selectMode = false;
            sameCardsSelection = false;
            isPlayerActivatePu = false;
            ui.EnableVisionClick(true);
            resetAllCardsSelectionWhenCardClicked = false;
            ui.InitLargeText(false, "");
            firstCardTargetPU = "";
            firstPosTargetPU = new Vector2(0, 0);
            puDeckUi.ResetOutstandPus();
            ui.ResetPointers();
            ui.ResetCardSelection();
            StartCoroutine(ActivatePlayerButtons(!endTurn, false));// Here or after darkEnd
            ui.EnableDarkScreen(true, false, () =>
             {
                 DisableVision();
                 ui.pEs.EnableSelecetPositionZ(false);
                 StartCoroutine(ui.pEs.EnableGlowLoop());
                 Debug.Log("endDS" + " " + endTurnInProcess);
                 StartCoroutine(ResetSortingOrder(false));
                 puDeckUi.EnablePusZ(true, false);
                 cardsDeckUi.DisableCardsSelection(Constants.AllCardsTag);
             });
        }
        if (!ReplaceInProgress && replaceMode && !Constants.TemproryUnclickable)
        {
            Debug.LogError("Disabling");
            EnableReplaceDialog(true, endTurn);
        }
        if (ui.raiseChooseDialog.activeSelf)
        {
            ui.EnableDarkScreen(true, false, () => ui.betBtn.btnBetClickable = true);
            ui.raiseChooseDialog.SetActive(false);
        }
        if (!endTurn && emojisWheelDisplay)
        {
            ShowEmojiWheel(false);
        }
    }
    public void LoadMenuScene(bool playAgain)
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

    public void ExitGame()
    {
        if (!BOT_MODE)
        {
            LocalTurnSystem.Instance.LeaveGame();
        }
        LoadMenuScene(false);
    }

    public void PlayMusic(bool enable)
    {
        SoundManager.Instance.PlayMusic();
    }




    private IEnumerator InitGameListeners()
    {
        yield return new WaitForSeconds(1f);
        if (!BOT_MODE)
        {
            BindTurnCounter();
            BindRoundCounter();
            BindPlayersReady();
            BindCurrentPlayer();
            ListenForNewDeck();
            ListenForPowerupUse();
            ListenForEmoji();
            ListenForBet();
        }
    }

    private void ListenForEmoji()
    {
        gameManager.ListenForEmoji(player.id, emojiId =>
        {
            StartCoroutine(ui.DisplayEmoji(false, emojiId, null));
        }, Debug.Log);
    }


    public IEnumerator StartNewRound()
    {
        yield return new WaitForSeconds(0.2f);
        if (!BOT_MODE)
        {
            StartCoroutine(CheckIfNewRoundReadyAndStart());
        }
        else
        {
            ShouldFlipArrow(!firstToPlayBotMode);
            currentTurn = 6;
            isRoundReady = true;
            currentGameInfo.cardDeck = CreateCardsDeck(true);
            ui.winLabel.SetActive(false);
            firstToPlayBotMode = !firstToPlayBotMode;
            SetState(new BeginRound(this, firstToPlayBotMode, false));
        }
    }

    private void ShouldFlipArrow(bool playerFirstToPlay)
    {
        if (FlipTurnAfterDecline)
        {
            ui.ApplyTurnVisual(playerFirstToPlay);
            FlipTurnAfterDecline = false;
        }
    }

    private IEnumerator CheckIfNewRoundReadyAndStart()
    {
        if (isRoundReady)
        {
            ui.winLabel.SetActive(false);
            ShouldFlipArrow(LocalTurnSystem.Instance.IsPlayerStartRound());
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
        float damage = (currentDamageThisRound + extraDamage) / fullHp;
        StartCoroutine(ui.DisplayDamageText(!isPlayerWin, currentDamageThisRound, extraDamage));
        StartCoroutine(ui.CardProjectileEffect(isPlayerWin, null, () => ui.UpdateDamage(damage, !isPlayerWin, IsPerfect())));
    }

    private bool IsPerfect()
    {
        return (playerHp <= 0 && enemyHp == fullHp) || (enemyHp <= 0 && playerHp == fullHp);
    }

    [Button]
    public void UpdateHandRank(bool reset)
    {
        if (!AreBoardCardsFlipped() && !reset)
        {
            int handRank = 7000;
            Hand bestHand = cardsDeckUi.CalculateHand(false, true, isPlayerFlusher, isPlayerStrighter);
            if (!visionUnavailable)
            {
                handRank = bestHand.Rank;
                if (playerHandIsFlusher)
                {
                    handRank = 1599;
                }
                else if (playerHandIsStrighter)
                {
                    handRank = 1609;
                }
            }
            ui.UpdateCardRank(handRank);
        }
        else
        {
            ui.ResetHandRank();
        }
    }

    #region Turn
    private void SaveAutoPlayAgain(bool playAgain)
    {
        PlayerPrefs.SetString("PlayAgain", playAgain.ToString());
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
        if (!BOT_MODE)
        {
            // if (LocalTurnSystem.Instance.IsPlayerStartRound() && !deckGenerate  /*LocalTurnSystem.Instance.isPlayerFirstPlayer*/)
            if (!LocalTurnSystem.Instance.IsPlayerTurn() && !deckGenerate  /*LocalTurnSystem.Instance.isPlayerFirstPlayer*/)
            {
                deckGenerate = true;
                DeckGeneratorDB();
            }
        }
        else
        {
            StartCoroutine(StartNewRound());
        }
    }

    private IEnumerator CheckIfEnemyPuRunningAndStartPlayerTurn()
    {
        Debug.Log("et " + enemyPuIsRunning + " " + turnInitInProgress);
        if (!enemyPuIsRunning && !turnInitInProgress && !ReplaceInProgress)
        {
            Debug.Log("et ABOUT");
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndTurnGong, true);
            SetState(new PlayerTurn(this, currentTurn));
        }
        else
        {
            while (enemyPuIsRunning || turnInitInProgress || ReplaceInProgress)
            {
                yield return new WaitForSeconds(0.6f);
                Debug.Log("e " + enemyPuIsRunning);
                Debug.Log("t " + turnInitInProgress);
                Debug.Log("r " + ReplaceInProgress);
                if (!enemyPuIsRunning && !turnInitInProgress && !ReplaceInProgress)
                {
                    StartCoroutine(CheckIfEnemyPuRunningAndStartPlayerTurn());
                }
            }
        }

    }

    public void ResetTimers()
    {
        ui.turnTimer.StopTimer();
    }

    public void EndTurn()
    {
        if (endClickable && !endTurnInProcess && !ReplaceInProgress)
        {
            if (tutoManager.step == 3)
            {
                tutoManager.InstructionsDisable();
                ui.turnTimer.GetComponent<SpriteRenderer>().sortingOrder = 0;
            }
            endTurnInProcess = true;
            StartCoroutine(ActivatePlayerButtons(false, false));
            DisableSelectMode(true);
            ui.EnableBgColor(false); //MAYBE UNECECERY
            ui.SetTurnIndicator(false, false);
            ResetTimers();
            if (!BOT_MODE)
            {
                gameManager.AddGameActionLog(GameManager.ActionEnum.EndTurn, "end of turn: " + currentTurn, () => { }, Debug.Log);
                LocalTurnSystem.Instance.PassTurn();
            }
            else
            {
                if (--currentTurn > 0)
                {
                    SetState(new EnemyTurn(this, currentTurn));
                }
                else if (currentTurn == 0)
                {
                    SetState(new EndRound(this, true, false));
                }
            }
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndTurnGong, true);
        }
        else
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CoinHit, false);
            Debug.LogError("endingProcees");
        }
    }

    internal List<string> GetRandomAvailableCardsNames(bool onlyUnfreeze)
    {
        List<string> cardsNames = cardsDeckUi.GetAvailbeCards(onlyUnfreeze);
        var rnd = new System.Random();
        List<string> shuffledcards = cardsNames.OrderBy(a => Guid.NewGuid()).ToList();
        return shuffledcards;
    }

    internal int GenerateRandom(int v1, int v2)
    {
        return UnityEngine.Random.Range(v1, v2);
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

    public IEnumerator OnTimeOut()
    {
        Debug.LogError("TIMERSDOUT");
        Constants.cardsToSelectCounter = 0;
        if (IsPlayerTurn())
        {
            yield return new WaitForSeconds(0.5f);
            if (!fm1Activated && newPowerUpName.Equals("fm1"))
            {
                ResetValuesAfterCardSelection(Constants.AllCardsTag);
                string playerCard = Constants.PlayerCard1;
                if (cardsDeckUi.GetCardUiByName(playerCard).freeze)
                {
                    playerCard = Constants.PlayerCard2;
                }
                SetState(new PowerUpState(this, true, newEnergyCost, newPowerUpName, playerCard, Constants.deckCardsNames[0], cardsDeckUi.GetCardPosition(playerCard), cardsDeckUi.GetCardPosition(Constants.deckCardsNames[0]), newPuSlotIndexUse));
                timedOut = true;
            }
            else
            {
                endClickable = true;
                StartCoroutine(WaitForPuToEndLoop());
            }
        }
    }

    private IEnumerator WaitForPuToEndLoop()
    {
        DisableSelectMode(true);
        if (playerPuInProcess || ReplaceInProgress)
        {
            //playerPuInProcess = false;
            Debug.Log("aboiut To PUINPR");
            yield return new WaitForSeconds(3f);
            Debug.Log("aboiut To PUINPR4");
            endClickable = true;
        }
        EndTurn();

        /* while (playerPuInProcess)
         {
          */
    }

    internal void NewTimerStarter()
    {
        StartCoroutine(ui.turnTimer.StartTimer(Values.Instance.turnTimerDuration));
    }
    #endregion


    #region Deck
    public void InitDecks()
    {
        cardsDeckUi = CardsDeckUi.Instance();
        cardsDeckUi.InitDeckFromServer(currentGameInfo.cardDeck);
        if (BOT_MODE)
        {
            cardsDeckUi.isPlayerFirst = firstToPlayBotMode;
        }
        else
        {
            cardsDeckUi.isPlayerFirst = IsPlayerTurn();
        }
        puDeckUi = PuDeckUi.Instance();
        if (firstDeck)
        {
            firstDeck = false;
            puDeckUi.InitDeckFromServer(currentGameInfo.puDeck);
        }
        else if (manualPuDeck)
        {
            manualPuDeck = false;
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
                readyToStart = true;
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
        if (LocalTurnSystem.Instance.IsPlayerTurn())
        {
            LocalTurnSystem.Instance.RoundCounter.Value = currentRound + 1;
            LocalTurnSystem.Instance.TurnCounter.Value = TURN_COUNTER_INIT;
        }
    }

    public void DeckGeneratorDB()
    {
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
            Debug.Log("s" + s);
            if (currentTurn != 7)
            {
                if (s == LocalTurnSystem.Instance.PlayerID.Value)
                {
                    StartCoroutine(StartPlayerTurn(false, null));
                }
                else if (s == currentGameInfo.EnemyId)
                {
                    SetState(new EnemyTurn(this, currentTurn));
                }
                else if (s.Contains("(#Exit#)"))
                {
                    StartCoroutine(ActivatePlayerButtons(false, false));
                    ui.WinPanelAfterEnemyLeaveGame(currentGameInfo.EnemyId.Substring(1));
                    SetState(new GameOver(this, true));
                }
            }
        };
    }

    private IEnumerator StartPlayerTurn(bool delay, Action EndAction)
    {
        if (delay)
        {
            yield return new WaitForSeconds(0.5f);
        }
        ui.SetTurnIndicator(false, false);
        ResetTimers();
        //mAYBE FIX IT

        EndAction?.Invoke();
        StartCoroutine(CheckIfEnemyPuRunningAndStartPlayerTurn());
    }


    public async void FakeEnemyEndTurn()
    {
        Debug.LogError("CurrentT " + currentTurn);

        if (--currentTurn > 0)
        {
            StartCoroutine(StartPlayerTurn(true, () =>
            {        //mAYBE FIX IT
                //StartCoroutine(AnimationManager.Instance.AlphaAnimation(turnTimer.turnArrowSpriteRenderer, true, Values.Instance.turnBtnAlphaDuration, null));
            }));
        }
        else if (currentTurn == 0)
        {
            await Task.Delay(1700);
            SetState(new EndRound(this, true, false));
        }
    }


    public async void FakeEnemyPuUse(int puIndex, string cardPlace1, string cardPlace2, bool endTurn)
    {
        Debug.LogWarning("FAking it");
        string puName = "";
        if (puIndex == -1)
        {
            puName = LoadPrefsString(Constants.Instance.botEsElementKey) + "p";
        }
        else
        {
            finishPuDissolve = false;
            puName = puDeckUi.GetPu(false, puIndex).puName;
        }
        PowerUpInfo puInfo = new PowerUpInfo(currentGameInfo.EnemyId, puName, cardPlace2, cardPlace1, puIndex, 12345);
        EnemyPuUse(puInfo);
        if (endTurn)
        {
            await Task.Delay(4500);
            enemyPuIsRunning = false;
            Debug.LogWarning("EndingIT");
            FakeEnemyEndTurn();
        }
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
            if (currentTurn == -1)
            {
                SetState(new EndRound(this, true, false));
            }
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
            Debug.LogError("Players Synced!");
        }
        else
        {
            Debug.LogError("Players Not Synced2");
        }
    }

    #endregion

    #region Settings
    public void ResetRoundSettings(Action FinishCallbac)
    {
        currentDamageThisRound = startingDamageForRound;
        ui.hpForThisRoundText.text = currentDamageThisRound.ToString();
        raiseOffer = false;
        ResetFlusherStrighter();
        Constants.TemproryUnclickable = false;
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
        if (firstRound)
        {
            currentRound = 1;
            if (!BOT_MODE)
            {
                firstRound = false;
                LocalTurnSystem.Instance.RoundCounter.Value = 1;
            }
        }
        UpdateHandRank(true);
        cardsDeckUi.DeleteAllCards(() => DealHands(FinishCallbac));
    }

    private void ResetFlusherStrighter()
    {
        if (isPlayerStrighter)
        {
            ui.FadeStrighterOrFlusher(true, false, false, null);
        }
        else if (isPlayerFlusher)
        {
            ui.FadeStrighterOrFlusher(true, true, false, null);
        }
        if (isEnemyStrighter)
        {
            ui.FadeStrighterOrFlusher(false, false, false, null);
        }
        else if (isEnemyFlusher)
        {
            ui.FadeStrighterOrFlusher(false, true, false, null);
        }
        isPlayerFlusher = false;
        isEnemyFlusher = false;
        isPlayerStrighter = false;
        isEnemyStrighter = false;
        playerHandIsFlusher = false;
        playerHandIsStrighter = false;
        enemyHandIsFlusher = false;
        enemyHandIsStrighter = false;
    }

    public bool DealDamage(bool isPlayer)
    {
        return DealHpDamage(isPlayer, false);
    }

    [Button]
    private void ShowEmojiBot(bool isHappy)
    {
        if (GenerateRandom(0, 11) < Values.Instance.ChanceForBotEmoji)
        {
            int emojiIndex = GenerateRandom(0, 2);
            int[] emojis = { 0, 2, 1, 3 };
            if (isHappy)
            {
                emojiIndex += 2;
            }
            StartCoroutine(ui.DisplayEmoji(false, emojis[emojiIndex], null));
        }
    }

    public void DisplayWinner(string winningText)
    {
        StartCoroutine(ui.ShowWinner(winningText));
    }

    public void UpdateWinnerDB()
    {
        if (!BOT_MODE)
        {
            gameManager.UpdateWinnerDB(player.id, () =>
                        Debug.Log("Winner updated!"),
                        Debug.LogError);
        }
    }

    #endregion


    #region PowerUp
    public void OnPowerUpPress(string newPowerUpName, int newPuSlotIndexUse, int energyCost)
    {
        if (IsPlayerTurn() && energyCounter - energyCost >= 0)
        {
            playerPuInProcess = true;
            isPlayerActivatePu = true;
            ui.EnableVisionClick(false);
            this.newPowerUpName = newPowerUpName;
            this.newPuSlotIndexUse = newPuSlotIndexUse;
            this.newEnergyCost = energyCost;

            SetState(new PowerUpState(this, true, energyCost, newPowerUpName, "", "", new Vector2(0, 0), new Vector2(0, 0), newPuSlotIndexUse));
        }
        else if (energyCounter - energyCost < 0)
        {
            Debug.LogWarning("YouDontHaveEnoughEnergy");
        }
    }

    public bool IsPlayerTurn()
    {
        if (!BOT_MODE)
        {
            return LocalTurnSystem.Instance.IsPlayerTurn();
        }
        return isPlayerBotModeTurn;
    }

    public void DissolvePuAfterUse(bool isPlayer, int index)
    {
        ResetPuAction = () => puDeckUi.GetPu(isPlayer, index).DissolvePu(0f, Values.Instance.puDissolveDuration, null, () =>
        {
            StartCoroutine(ResetPuUi(isPlayer, index));
            ResetPuAction = null;
        });
    }
    public void DissolveNcToEs(bool isPlayer, int index, Action FillEs)
    {
        ResetPuAction = () =>
        {
            /*            FillEs?.Invoke();
            */
            puDeckUi.DissolveNcToEs(isPlayer, index, FillEs, () =>
        {
            StartCoroutine(ResetPuUi(isPlayer, index));
            ResetPuAction = null;
        });
        };
    }

    public void UpdateEsAfterNcUse(bool isPlayer, string puName)
    {
        if (isPlayer)
            ui.pEs.UpdateEsAfterNcUse(puName.Contains("m"));
        else
            ui.eEs.UpdateEsAfterNcUse(puName.Contains("m"));
    }

    private void ListenForPowerupUse()
    {
        gameManager.ListenForPowerupUse(powerUpInfo =>
        {
            if (!powerUpInfo.playerId.Equals(player.id))
            {
                StartCoroutine(HandleEnemyPuUse(powerUpInfo));
            }
        }, powerUpInfo =>
        {
            if (!powerUpInfo.playerId.Equals(player.id))
            {
                StartCoroutine(HandleEnemyPuDraw(powerUpInfo.slot));

            }
        }, Debug.Log);
    }

    private IEnumerator HandleEnemyPuUse(PowerUpInfo puInfo)
    {
        if (ReplaceInProgress)
        {
            yield return new WaitForSeconds(0.6f);
            StartCoroutine(HandleEnemyPuUse(puInfo));
        }
        else
        {
            EnemyPuUse(puInfo);
        }
    }
    private IEnumerator HandleEnemyPuDraw(int slot)
    {
        if (enemyPuIsRunning)
        {
            yield return new WaitForSeconds(0.6f);
            StartCoroutine(HandleEnemyPuDraw(slot));
        }
        else
        {
            if (slot == -1)
            {
                ReplaceInProgress = true;
                DealPu(false, () => ReplaceInProgress = false);
            }
            else
            {
                ReplacePu(false, slot);
            }
        }
    }

    private void EnemyPuUse(PowerUpInfo powerUpInfo)
    {
        enemyPuIsRunning = true;
        isPlayerActivatePu = false;
        DisableVision();
        currentGameInfo.powerup = powerUpInfo;
        if (powerUpInfo.slot != -1)
        {
            puDeckUi.GetPu(false, powerUpInfo.slot).AnimatePuUse(
                      () => SetState(new PowerUpState(this, false, 0, powerUpInfo.powerupName, powerUpInfo.cardPlace1, powerUpInfo.cardPlace2,
                      cardsDeckUi.GetCardPosition(powerUpInfo.cardPlace1), cardsDeckUi.GetCardPosition(powerUpInfo.cardPlace2), powerUpInfo.slot)), null);
        }
        else // SKILL
        {
            SetState(new PowerUpState(this, false, 0, powerUpInfo.powerupName, powerUpInfo.cardPlace1, powerUpInfo.cardPlace2, cardsDeckUi.GetCardPosition(powerUpInfo.cardPlace1), cardsDeckUi.GetCardPosition(powerUpInfo.cardPlace2), powerUpInfo.slot));
        }
    }


    public IEnumerator OnCardsSelectedForPU(string cardPlace, Vector2 position)
    {
        if (Constants.cardsToSelectCounter == 0)
        {
            ui.ResetPointers();
            if (cardPlace.Length != 1) // Choice of wheel
            {
                ui.SetCardSelection(1, newPowerUpName[0].ToString(), position, IsLargeCard(cardPlace), true);
            }
            //  playerPuInProcess = true;
            if (newPowerUpName.Equals("fm1"))
            {
                fm1Activated = true;
            }
            ResetValuesAfterCardSelection(Constants.AllCardsTag);
            yield return new WaitForSeconds(0.5f);
            SetState(new PowerUpState(this, true, newEnergyCost, newPowerUpName, firstCardTargetPU, cardPlace, firstPosTargetPU, position, newPuSlotIndexUse));
            firstPosTargetPU = new Vector2(0, 0);
            firstCardTargetPU = "empty";
        }
        else if (Constants.cardsToSelectCounter == 1)
        {
            if (newPowerUpName.Equals("tp"))
            {
                ui.SetTechWheelForSelection(position, cardPlace.Contains("B"));
            }
            ui.SetCardSelection(2, newPowerUpName[0].ToString(), position, IsLargeCard(cardPlace), true);
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


    public void OnWheelSelected(int option)
    {
        --Constants.cardsToSelectCounter;
        ui.SetWheelSelectionBtn(option);
        StartCoroutine(OnCardsSelectedForPU(option.ToString(), new Vector2(0, 0)));
    }

    private bool IsLargeCard(string cardPlace)
    {
        return !cardPlace.Contains("B");
        //return cardPlace.Contains("Player") || cardPlace.Contains("Enemy");
    }

    private void ResetValuesAfterCardSelection(string cardPlace)
    {
        ui.EnableDarkScreen(true, false, () =>
        {
            ui.pEs.EnableSelecetPositionZ(false);
            StartCoroutine(ResetSortingOrder(false));
        }
        );
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

    public void ShowPuInfo(Vector2 startingPosition, bool isPu, bool paddingRight, string puName, string puDisplayName)
    {
        infoShow = true;
        ui.ShowPuInfoDialog(startingPosition, isPu, paddingRight, puName, puDisplayName, true, false, null);
        if (tutoManager.step == 1)
            tutoManager.SetStep(2);
    }
    public void HideDialog(bool isPu)
    {
        ui.ShowPuInfoDialog(new Vector2(0, 0), isPu, false, " ", " ", false, false, () => infoShow = false);
    }

    internal IEnumerator ResetPuUi(bool isPlayer, int puIndex)
    {
        if (puIndex == -1)
        {
            yield return new WaitForSeconds(2f);
            UpdateHandRank(false);
            enemyPuIsRunning = false;
        }
        else
        {
            PowerUpUi pu = puDeckUi.GetPu(isPlayer, puIndex);
            puDeckUi.RemovePuFromList(isPlayer, puIndex);
            Debug.LogWarning("RESETING");
            puDeckUi.ResetPuUI(pu, () => UpdateHandRank(false));
        }
        if (isPlayer)
        {
            StartCoroutine(AutoEndTurn());
        }
        else
        {
            finishPuDissolve = true;
            yield return new WaitForSeconds(1f);
            enemyPuIsRunning = false;
            Debug.LogError("ImresetingPU");
        }
        if (BOT_MODE)
        {
            yield return new WaitForSeconds(GenerateRandom(1, 4));
            ShowEmojiBot(!isPlayer);
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
        if (!BOT_MODE)
            gameManager.SetNewPowerupUseDB(new PowerUpInfo(player.id, newPowerUpName, Constants.Instance.ConvertCardPlaceForEnemy(cardTarget), Constants.Instance.ConvertCardPlaceForEnemy(secondCardTargetPU), puIndex, CreateTimeStamp()), () =>
        {
            gameManager.AddGameActionLog(GameManager.ActionEnum.PuUse, "name: " + newPowerUpName + " c1: " + cardTarget + " c2: " + secondCardTargetPU, () => { }, Debug.Log);

        }, Debug.Log);
    }

    public void UpdateReplacePuInDb(int puIndex)
    {
        if (!BOT_MODE)
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
        ReplaceInProgress = true;
        if (isPlayer)
        {
            Constants.TemproryUnclickable = true;
            DisablePlayerPus();
            //Maybe disable btns

            ReduceEnergy(Values.Instance.energyCostForDraw);
            UpdateReplacePuInDb(puIndex);
            ui.EnableDarkScreen(true, false, null);
            puDeckUi.EnablePusSlotZ(true, false);
        }

        puDeckUi.ReplacePu(isPlayer, puIndex, () =>
         {
             ReplaceInProgress = false;
             if (isPlayer)
             {
                 Debug.Log("doneProgress");
                 EnableReplaceDialog(true, false);
                 if (energyCounter == 0)
                 {
                     StartCoroutine(AutoEndTurn());
                 }
                 else
                 {
                     Constants.TemproryUnclickable = false;
                 }
             }
         });
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
    }

    public void CheckIfPuAvailable(PowerUpUi puUi)
    {
        if (energyCounter - puUi.energyCost < 0 || puUi.puIndex == -1 && skillUseLeft == 0)
        {
            puUi.EnablePu(false);
        }
        else
        {
            if ((puUi.puElement.Equals("t") || (puUi.puElement.Equals("w")) && !puUi.isMonster) || puUi.puName.Equals("fm1"))
            {
                string[] cardsTag = PowerUpStruct.Instance.GetReleventTagCards(puUi.name, true);
                int cardsLimit = 0;
                if (cardsDeckUi.GetListByTag(cardsTag[0]) == cardsDeckUi.GetListByTag(cardsTag[1]))
                {
                    cardsLimit = 1;
                }
                if (!CheckIfCardsAvailbleForSelect(cardsLimit, puUi.puElement.Equals("w"), cardsDeckUi.GetListByTag(cardsTag[0]))
                    || !CheckIfCardsAvailbleForSelect(cardsLimit, puUi.puElement.Equals("w"), cardsDeckUi.GetListByTag(cardsTag[1])))
                {
                    puUi.EnablePu(false);
                }
                else
                {
                    puUi.EnablePu(true);
                }
            }
            else if (puUi.puName.Equals("wm1") && cardsDeckUi.IsOneCardFromHandsFreeze())
            {
                puUi.EnablePu(false);
            }
            else if (puUi.puName.Equals("wm2") && cardsDeckUi.GetHowManyAvailableUnfrozenCards() < 2)
            {
                puUi.EnablePu(false);
            }
            else
            {
                puUi.EnablePu(true);
            }
        }
    }

    private bool CheckIfCardsAvailbleForSelect(int cardsLimit, bool glitchBlocked, List<CardUi> cards)
    {
        int frozenCards = 0;
        if (cards != null)
        {

            foreach (CardUi card in cards)
            {
                if (glitchBlocked)
                {
                    if (card.freeze || card.glitch)
                    {
                        frozenCards++;
                    }
                }
                else if (card.freeze)
                {
                    frozenCards++;
                }
                if (cards.Count - frozenCards <= cardsLimit)
                {
                    return false;
                }
            }
        }
        return true;
    }

    internal void ChangeValuePu(string cardTarget, int value)
    {
        cardsDeckUi.UpdateCardValue(cardTarget, value, () =>
         {
             EnableDarkAndSorting(false);
             UpdateHandRank(false);
         });
    }

    [Button]
    public void AddGhostCardPu(Constants.CardsOwener cardsOwener, Action Reset)
    {
        cardsDeckUi.GhostCardActivate(cardsOwener, () => UpdateHandRank(false), Reset);
    }

    [Button]
    internal void StrighterPU(bool isPlayerActivate)
    {
        if (isPlayerActivate)
        {
            if (isPlayerFlusher)
            {
                ui.FadeStrighterOrFlusher(true, true, false, null);
            }
        }
        else
        {
            if (isEnemyFlusher)
            {
                ui.FadeStrighterOrFlusher(false, true, false, null);
            }
        }
        ui.FadeStrighterOrFlusher(isPlayerActivate, false, true, () => EnableDarkAndSorting(false));

        //ui.FadeStrighterOrFlusher(isPlayerActivate, true, false,() => ui.FadeStrighterOrFlusher(isPlayerActivate, false, true, () => EnableDarkAndSorting(false)));
        isPlayerStrighter = isPlayerActivate;
        isEnemyStrighter = !isPlayerActivate;
        isPlayerFlusher = false;
        isEnemyFlusher = false;
    }

    [Button]
    internal void FlusherPU(bool isPlayerActivate)
    {
        if (isPlayerActivate)
        {
            if (isPlayerStrighter)
            {
                ui.FadeStrighterOrFlusher(true, false, false, null);
            }
        }
        else
        {
            if (isEnemyStrighter)
            {
                ui.FadeStrighterOrFlusher(false, false, false, null);
            }
        }
        ui.FadeStrighterOrFlusher(isPlayerActivate, true, true, () => EnableDarkAndSorting(false));
        isPlayerFlusher = isPlayerActivate;
        isEnemyFlusher = !isPlayerActivate;
        isPlayerStrighter = false;
        isEnemyStrighter = false;
    }

    internal void SmokeCardPu(bool enable, string cardTarget2, bool isPlayerActivate, bool reset, bool delay)
    {
        if (enable && cardsDeckUi.GetCardUiByName(cardTarget2) != null && cardsDeckUi.GetCardUiByName(cardTarget2).freeze)
        {
            FreezePlayingCard(cardTarget2, 0, false, false, false); // MAYBE BUG
        }
        Action Reset = null;
        if (reset)
        {
            Reset = () => EnableDarkAndSorting(false);
        }
        cardsDeckUi.EnableCardSmoke(enable, isPlayerActivate, cardsDeckUi.GetCardUiByName(cardTarget2));
        StartCoroutine(ui.InitSmoke(isPlayerActivate, delay, cardsDeckUi.GetParentByPlace(cardTarget2), enable, Reset));
    }

    internal void SmokeTurnRiver(bool isPlayerActivate)
    {
        SmokeCardPu(true, Constants.BTurn4, isPlayerActivate, false, false);
        SmokeCardPu(true, Constants.BRiver5, isPlayerActivate, true, true);
    }
    internal void DeactivateSmoke()
    {
        foreach (CardUi card in cardsDeckUi.GetListByTag(Constants.AllCardsTag))
        {
            if (card.underSmoke)
            {
                cardsDeckUi.EnableCardSmoke(false, false, card);
            }
        }
        foreach (CardSlot cardSlot in cardsDeckUi.allCardSlots)
        {
            if (cardSlot.smokeEnable)
            {
                StartCoroutine(ui.InitSmoke(false, false, cardSlot, false, null));
            }
        }
    }
    internal void GhostPu(bool isPlayerActivate, bool isHand)
    {
        Constants.CardsOwener cardsOwener;
        if (!isHand)
        {
            cardsOwener = Constants.CardsOwener.Board;
        }
        else if (isPlayerActivate)
        {
            cardsOwener = Constants.CardsOwener.Player;
        }
        else
        {
            cardsOwener = Constants.CardsOwener.Enemy;
        }
        AddGhostCardPu(cardsOwener, () => EnableDarkAndSorting(false));
    }
    public async void DestroyAndDrawCard(string cardPlace, float delay, bool firstCard, bool ResetEnable)
    {
        if (cardsDeckUi.GetCardUiByName(cardPlace).freeze)
        {
            FreezePlayingCard(cardPlace, 0, false, false, ResetEnable);
            if (ResetEnable)
            {
                await Task.Delay(1750);
                cardsDeckUi.CloseDrawer();
            }
        }
        else
        {
            bool isFlip = IsEnemyCard(cardPlace);
            StartCoroutine(ReplaceSelectedCard(cardPlace, isFlip, delay, firstCard, ResetEnable));
            // ReplaceSelectedCard2(cardPlace, isFlip, delay, ResetEnable, firstCard, lastCard);
        }
    }

    private bool IsEnemyCard(string cardPlace)
    {
        return cardPlace.Contains("Enemy");
    }

    public IEnumerator ReplaceSelectedCard(string cardPlace, bool isFlip, float delay, bool isFirstCard, bool ResetEnable)
    {
        if (cardsDeckUi.GetParentByPlace(cardPlace).smokeEnable)
        {
            SmokeCardPu(false, cardPlace, false, false, false);
        }
        if (isFirstCard)
            cardsDeckUi.AnimateDrawer(true, null);
        yield return new WaitForSeconds(delay);
        cardsDeckUi.DestroyCardObjectFire(cardPlace, null);
        yield return new WaitForSeconds(0.3f);
        Action resetAction = null;
        if (ResetEnable)
        {
            resetAction = () => EnableDarkAndSorting(false);
        }
        cardsDeckUi.DrawAndReplaceCard(cardPlace, isFlip, resetAction, isFirstCard, ResetEnable);
    }
    public async void ReplaceSelectedCard2(string cardPlace, bool isFlip, int delay, bool ResetEnable, bool isFirstCard, bool isLastCard)
    {
        if (cardsDeckUi.GetParentByPlace(cardPlace).smokeEnable)
        {
            SmokeCardPu(false, cardPlace, false, false, false);
        }

        await Task.Delay(500);
        cardsDeckUi.DestroyCardObjectFire(cardPlace, null);
        await Task.Delay(500);
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
            fm1Activated = false;
            //selectCardsMode = true;
            ResetPuAction?.Invoke();
            StartCoroutine(ResetSortingOrder(false));//true?
        }
        //  ui.EnableDarkScreen(isPlayerActivatePu, enable, () => {
        ResetPuAction = null;
        DisableVision();
        // });
    }

    public void UpdateZPos(bool aboveDarkScreen, string tag)
    {
        if (!tag.Equals(Constants.PoolCardTag))
        {
            foreach (CardUi card in cardsDeckUi.GetListByTag(tag))
            {
                card.EnableSelecetPositionZ(aboveDarkScreen);
            }
        }
    }



    private IEnumerator ResetSortingOrder(bool delay)
    {
        //WHEN NEED NO ENEMY
        // enemyPuIsRunning = false;
        playerPuInProcess = false;
        if (delay)
        {
            yield return new WaitForSeconds(1f);
        }
        Debug.LogError("Imreseting");
        foreach (CardUi card in cardsDeckUi.GetListByTag("All"))
        {
            card.EnableSelecetPositionZ(false);
        }
        if (tutoManager.step == 2)
        {
            tutoManager.SetStep(3);
            cardsDeckUi.playerCardsUi[0].spriteRenderer.sortingOrder = 1;
            cardsDeckUi.playerCardsUi[1].spriteRenderer.sortingOrder = 1;
        }
    }

    private IEnumerator AutoEndTurn()
    {
        Debug.LogError("AutoEnd");
        if (timedOut || (energyCounter == 0 && ui.pEs.ncCounterUse < 3 && !HaveEnoughToRaise()) /*|| energyCounter == 1 && puDeckUi.GetPuListCount(true) == 0 && !skillUsed*/)
        {
            Debug.LogError("ending");
            timedOut = false;
            ui.turnTimer.PauseTimer(true);
            yield return new WaitForSeconds(1.5f);
            if (newPowerUpName.Contains("m2") || ReplaceInProgress)
            {
                yield return new WaitForSeconds(2.5f);
            }//MakeItBetter
            endClickable = true;
            EndTurn();
        }
        else
        {
            StartCoroutine(ActivatePlayerButtons(true, false));
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
            ui.InitLargeText(true, PowerUpStruct.Instance.GetPuInstructionsByName(newPuName));
        }
        Constants.cardsToSelectCounter = cardsToSelectCounter;
    }

    internal async void FreezePlayingCard(string cardTarget, int delay, bool isToFreeze, bool isFirst, bool reset)
    {
        await Task.Delay(delay);
        CardUi cardToFreeze = cardsDeckUi.GetCardUiByName(cardTarget);
        Action resetAction = null;
        if (reset)
        {
            resetAction = () => EnableDarkAndSorting(false);
        }
        if (cardToFreeze.glitch && isToFreeze)
        {
            cardToFreeze.glitch = false;
            ui.FreezeObject(cardToFreeze.spriteRenderer, true, cardToFreeze.GetisFaceDown(), () =>
            {
                ui.FreezeObject(cardToFreeze.spriteRenderer, false, cardToFreeze.GetisFaceDown(), resetAction, true);
                cardsDeckUi.EnableGlitchValues(false, cardToFreeze.spriteRenderer.material);
            }, true);
        }
        else if (isToFreeze && cardToFreeze.freeze)
        {
            DoubleFreezeCard(cardToFreeze, cardTarget, resetAction, isFirst, reset);
        }
        else
        {
            cardToFreeze.freeze = isToFreeze;
            ui.FreezeObject(cardToFreeze.spriteRenderer, isToFreeze, cardToFreeze.GetisFaceDown(), resetAction, true);
        }
        if (reset)
        {
            await Task.Delay(1700);
            cardsDeckUi.CloseDrawer();
        }
    }

    private void DoubleFreezeCard(CardUi cardUi, string cardTarget, Action resetAction, bool isFirst, bool isLast)
    {
        //cardsDeckUi.RemoveFromList(cardUi);
        if (isFirst)
            cardsDeckUi.AnimateDrawer(true, null);
        if (cardUi.cardMark.activeSelf)
        {
            cardUi.cardMark.SetActive(false);
        }
        ui.DoubleFreezeEffect(cardUi.spriteRenderer, () => cardsDeckUi.RestAfterDestroy(cardUi, null), () => cardsDeckUi.DrawAndReplaceCard(cardTarget, IsEnemyCard(cardTarget), resetAction, isFirst, isLast));
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
            enemyPuIsRunning = false;
            if (isPlayerActivate)
            {
                StartCoroutine(AutoEndTurn());
            }
        });

    }


    internal void SwapTwoCards(string cardTarget1, string cardTarget2, bool reset)
    {
        Debug.Log(cardTarget1);
        Debug.Log(cardTarget2);
        bool noSmoke = true;
        if (cardsDeckUi.GetParentByPlace(cardTarget1).smokeEnable)
        {
            SmokeCardPu(false, cardTarget1, false, false, true);
            noSmoke = false;
        }
        if (cardsDeckUi.GetParentByPlace(cardTarget2).smokeEnable)
        {
            SmokeCardPu(false, cardTarget2, false, false, true);
            noSmoke = false;
        }
        if (noSmoke)
        {
            DestroyRandomSmoke(cardTarget1, cardTarget2);
        }
        Action ActionReset = null;
        if (reset)
        {
            ActionReset = () => EnableDarkAndSorting(false);
        }
        cardsDeckUi.SwapTwoCards(cardTarget1, cardTarget2, ActionReset);
    }

    private void DestroyRandomSmoke(string cardTarget1, string cardTarget2)
    {
        List<CardSlot> underSmokeCards = new List<CardSlot>();
        string tag1 = cardsDeckUi.CardPlaceToTag(cardTarget1);
        string tag2 = cardsDeckUi.CardPlaceToTag(cardTarget2);
        foreach (CardSlot cardSlot in cardsDeckUi.allCardSlots)
        {
            if (cardSlot.CompareTag(tag1) || cardSlot.CompareTag(tag2))
            {
                if (cardSlot.smokeEnable)
                {
                    underSmokeCards.Add(cardSlot);
                }
            }
        }
        if (underSmokeCards.Count > 0)
        {
            SmokeCardPu(false, underSmokeCards[UnityEngine.Random.Range(0, underSmokeCards.Count - 1)].childrenName, false, false, false);
        }

    }

    internal void SwapPlayersHands()
    {
        for (int i = 0; i < 4; i++)
        {
            if (cardsDeckUi.allCardSlots[i].smokeEnable)
            {
                SmokeCardPu(false, cardsDeckUi.allCardSlots[i].childrenName, false, false, true);
            }

        }
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
            yield return new WaitForSeconds(1f);
            StartCoroutine(DealPuCourotine(isPlayer, OnEnd));
        }
        else
        {
            Debug.Log("DEaling");

            puDeckUi.DealRoutine(isPlayer, OnEnd);
        }
    }

    internal void InitProjectile(bool isPlayerActivate, int puIndex, string powerUpName, Vector2 posTarget1, Vector2 posTarget2, Action IgnitePowerUp)
    {

        if (powerUpName.Equals(nameof(PowerUpStruct.PowerUpNamesEnum.im1)))
        {
            StartCoroutine(ui.IglooFx(isPlayerActivate, () => IgnitePowerUp.Invoke()));
        }
        else if (powerUpName.Equals(nameof(PowerUpStruct.PowerUpNamesEnum.im2)))
        {
            StartCoroutine(ui.StartIcenado());
            IgnitePowerUp.Invoke();
        }
        else if (powerUpName.Equals(nameof(PowerUpStruct.PowerUpNamesEnum.fm2)))
        {
            StartCoroutine(ui.StartArmageddon());
            IgnitePowerUp.Invoke();
        }
        else if (powerUpName.Equals(nameof(PowerUpStruct.PowerUpNamesEnum.tm1)))
        {
            ui.StartMatrix();
            IgnitePowerUp.Invoke();
        }
        else
        {
            if (powerUpName.Equals(nameof(PowerUpStruct.PowerUpNamesEnum.im1)))
            {
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
            }
            ui.InitProjectile(puDeckUi.GetPuPosition(isPlayerActivate, puIndex), powerUpName, isPlayerActivate, posTarget1, posTarget2, IgnitePowerUp);
        }
    }

    #endregion

    #region Buttons


    public void EnableReplaceDialog(bool disable, bool endTurn)
    {
        if (btnReplaceClickable && energyCounter > 0 && !disable && puDeckUi.GetPuListCount(true) < 2)
        {
            btnReplaceClickable = false;
            ReplaceInProgress = true;
            UpdateReplacePuInDb(-1);
            ReduceEnergy(Values.Instance.energyCostForDraw);
            StartCoroutine(ActivatePlayerButtons(false, false));
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.BtnClick, false);

            DealPu(true, () =>
            {
                ReplaceInProgress = false;
                StartCoroutine(AutoEndTurn());
            });
        }
        else if (btnReplaceClickable || endTurn || disable)
        {
            Constants.TemproryUnclickable = true;
            if (/*replaceMode ||*/ IsPlayerTurn() && energyCounter > 0)
            {
                PowerUpUi[] playerPus = puDeckUi.GetPuList(true);
                if (playerPus[0] != null || playerPus[1] != null)
                {
                    replaceMode = !replaceMode;
                    // MAKE IT DIFFERENT

                    ui.InitLargeText(replaceMode, Constants.DrawInstructions);
                    if (replaceMode)
                    {
                        puDeckUi.EnablePusSlotZ(true, true);
                        //AnimationManager.Instance.SetAlpha(ui.darkScreenRenderer, 0.56f);
                        ui.EnableDarkScreen(isPlayerActivatePu, true, () => StartCoroutine(SetClickableWithDelay(0.5f)));
                    }
                    else
                    {
                        ui.EnableDarkScreen(isPlayerActivatePu, false, () =>
                         {
                             StartCoroutine(SetClickableWithDelay(0.5f));
                             puDeckUi.EnablePusSlotZ(true, false);
                             StartCoroutine(ActivatePlayerButtons(!endTurn, false));
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
                Debug.LogError("2HI!");
                ui.EnableDarkScreen(isPlayerActivatePu, false, () =>
                {
                    puDeckUi.EnablePusSlotZ(true, false);
                    StartCoroutine(ActivatePlayerButtons(!endTurn, false));
                });
            }
        }
        else
        {
            ui.DisableClickBtnReplace();
        }
        DisableVision();
    }



    private IEnumerator SetClickableWithDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        Constants.TemproryUnclickable = false;
    }
    public void SlideRankingImg()
    {
        ui.SlideRankingImg();
    }


    bool visionOn = false;

    public void OnVisionBtnDown()
    {
        if (!AreBoardCardsFlipped() && !selectMode)
        {
            visionOn = true;
            int handRank = -1;
            //STORE LAST HAND RANK INSTEAED
            Hand bestHand = cardsDeckUi.CalculateHand(false, true, isPlayerFlusher, isPlayerStrighter);
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
                if (playerHandIsFlusher)
                {
                    handRank = 1599;
                }
                else if (playerHandIsStrighter)
                {
                    handRank = 1609;
                }
            }
            ui.UpdateCardRank(handRank);
            ui.UpdateRankTextInfo(true, handRank, ConvertWinenerRankToDamage(true));
            //TODO make it more efficient
            SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.Vision, true);

        }
    }
    public void OnVisionBtnUp()
    {
        if (!AreBoardCardsFlipped()/* && !selectMode*/)
        {
            visionOn = false;
            AnimationManager.Instance.VisionEffect(cardsDeckUi.GetBoardAndPlayerHandList(), cardsDeckUi.GetBoardAndPlayerHandList().Count, false);
        }
        SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.Vision, false);
        ui.UpdateRankTextInfo(false, 0, 0);
    }

    private void DisableVision()
    {
        if (visionOn)
        {
            AnimationManager.Instance.VisionEffect(cardsDeckUi.GetListByTag(Constants.AllCardsTag), cardsDeckUi.GetListByTag(Constants.AllCardsTag).Count, false);
            SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.Vision, false);
            ui.UpdateRankTextInfo(false, 0, 0);
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

    public void ActivatePlayerButtonsOut(bool enable, bool delay)
    {
        StartCoroutine(ActivatePlayerButtons(enable, delay));
    }

    internal IEnumerator ActivatePlayerButtons(bool enable, bool delay)
    {
        if (delay)
        {
            yield return new WaitForSeconds(Values.Instance.delayTimerStart);
        }
        if (enable && !endTurnInProcess)
        {
            Constants.TemproryUnclickable = false;
            ui.EnablePlayerButtons(true);
            ActivatePlayerPus();
            ui.betBtn.EnableBetBtn(HaveEnoughToRaise());
        }
        else if (!enable)
        {
            ui.EnablePlayerButtons(false);
            DisablePlayerPus();
            ui.betBtn.EnableBetBtn(false);
        }
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
            UpdateEmojiDB(id);
            StartCoroutine(ui.DisplayEmoji(true, id, () => emojiCooledDown = true));
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
        //ui.EnableDarkScreen(enable, null);
        emojisWheelDisplay = enable;
    }

    private void UpdateEmojiDB(int emojiId)
    {
        if (!BOT_MODE)
        {
            gameManager.UpdateEmojiDB(new EmojiInfo(currentGameInfo.localPlayerId, emojiId, CreateTimeStamp()), () =>
                                 Debug.Log("EmojiSent" + emojiId),
                                Debug.LogError);
        }
    }


    internal void WinParticleEffect()
    {

        ui.WinParticleEffect();
    }



    /* [Button]
     internal void SmokeCheck(bool enable)
     {
         ui.InitSmoke(cardsDeckUi.GetParentByPlace(Constants.PlayerCard1), enable);
         cardsDeckUi.EnableCardSmoke(enable, false, Constants.PlayerCard1);
     }*/
    [Button]
    internal void DealBoard()
    {
        cardsDeckUi.DealCardsForBoard(true, null, () => UpdateHandRank(false));
    }
    [Button]
    internal void EndRoundManual()
    {
        SetState(new EndRound(this, true, false));
    }

    internal void OnPuClick(PowerUpUi powerUpUi)
    {
        if (tutoManager.step == 2)
        {
            tutoManager.timerForMsgEnable = false;
            tutoManager.InstructionsDisable();
        }
        if (infoShow)
        {
            HideDialog(true);
        }
        else if (!ReplaceInProgress && !Constants.TemproryUnclickable && powerUpUi.isPlayer && !powerUpUi.freeze && powerUpUi.isClickable)
        {
            powerUpUi.isClickable = false;
            powerUpUi.aboutToDestroy = true;
            StartCoroutine(ActivatePlayerButtons(false, false));
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.BtnClick, false);
            if (powerUpUi.puIndex != -1)
            {
                powerUpUi.AnimatePuUse(() => OnPowerUpPress(powerUpUi.puName, powerUpUi.puIndex, powerUpUi.energyCost), null);
            }
            else
            {
                OnPowerUpPress(powerUpUi.puName, powerUpUi.puIndex, powerUpUi.energyCost);
            }
        }
        else if (!ReplaceInProgress && powerUpUi.isPlayer && powerUpUi.replaceMode)
        {
            ReplacePu(true, powerUpUi.puIndex);
            ui.InitLargeText(false, Constants.DrawInstructions);
        }
        else if (playerPuInProcess)
        {
            if (tutoManager.step != 2)
                DisableSelectMode(false);
        }
        else if (powerUpUi.isPlayer || !powerUpUi.isPlayer)
        {
            if (powerUpUi.puIndex != -1)
            {
                StartCoroutine(AnimationManager.Instance.Shake(powerUpUi.spriteRenderer.material, Values.Instance.disableClickShakeDuration));
            }
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CantClick, false);
        }
    }
    internal void OnEsClick(ElementalSkillUi es)
    {
        if (infoShow)
        {
            HideDialog(true);
        }
        else if (!ReplaceInProgress && !Constants.TemproryUnclickable)
        {
            es.isClickable = false;
            StartCoroutine(ActivatePlayerButtons(false, false));
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.BtnClick, false);
            OnPowerUpPress(es.element + "p", -1, 0);
        }
    }

    private void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown)
        {

            if (Input.GetKeyDown("escape"))
            {
                if (ui.gameOverPanel.activeSelf)
                {
                    LoadMenuScene(false);
                }
                else if (!ui.IsSliderOpen())
                {
                    EnableExitDialog(true);
                }
                else
                {
                    ui.SlideRankingImgIfOpen();
                }
            }
        }

    }

    public void EnableExitDialog(bool enable)
    {
        canvasExitDialog.SetActive(enable);
        ui.EnableDarkScreen(false, enable, () => DisableVision());
    }

    public void BtnExit()
    {
        ui.SlideRankingImgIfOpen();
        canvasExitDialog.SetActive(true);
        ui.EnableDarkScreen(false, true, () => DisableVision());
    }

    public void SavePrefsFloat(string key, float value)
    {
        PlayerPrefs.SetFloat(key, value);
    }
    public void SavePrefsString(string key, string value)
    {
        PlayerPrefs.SetString(key, value);
    }
    public string LoadPrefsString(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetString(key);
        }
        else
        {
            return "";
        }
    }
    public void SavePrefsInt(string key, int value)
    {
        PlayerPrefs.SetInt(key, value);
    }


    public float LoadPrefsFloat(string key)
    {
        if (PlayerPrefs.HasKey(key))
        {
            return PlayerPrefs.GetFloat(key);
        }
        else
        {
            return 1f;
        }
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

    private void DisplayStatistics()
    {
        ui.winLoseBot.text = "V = " + LoadPrefsInt(Constants.Instance.PLAYER_WIN_BOT) + "\n"
            + "X = " + LoadPrefsInt(Constants.Instance.PLAYER_LOSE_BOT);
    }
    internal void DealBoardCard()
    {
        //   waitForDrawerAnimationToEnd = false;
        StartCoroutine(DealBoardCardCorutine());
    }
    internal IEnumerator DealBoardCardCorutine()
    {
        //   waitForDrawerAnimationToEnd = false;
        yield return new WaitForSeconds(1f);
        cardsDeckUi.DealCardsForBoard(true, null /*() => waitForDrawerAnimationToEnd = false*/, () => UpdateHandRank(false));
    }


    [Button]
    public void InternetChecK()
    {
        StartCoroutine(CheckInternetConnection(result => Debug.Log("Internet: " + result)));
    }
    public IEnumerator CheckInternetConnection(Action<bool> syncResult)
    {
        const string echoServer = "http://google.com";

        bool result;
        using (var request = UnityEngine.Networking.UnityWebRequest.Head(echoServer))
        {
            request.timeout = 5;
            yield return request.SendWebRequest();
            result = !request.isNetworkError && !request.isHttpError && request.responseCode == 200;
        }
        syncResult(result);
    }



    private void ListenForBet()
    {
        MainManager.Instance.gameManager.ListenForOtherPlayerBet(() =>
        OtherPlayerBetResponse(true)
        ,
        () =>
        OtherPlayerBetResponse(false)
        ,
       dmg =>
            EnemyBetPopUp(dmg)
        , Debug.Log);
    }
    private void OtherPlayerBetResponse(bool isAccept)
    {
        if (LocalTurnSystem.Instance.IsPlayerTurn())
        {
            EnableWaitDialog(false);
            if (isAccept)
            {
                EnemyAcceptRaise();
            }
            else
            {
                EnemyDeclineRaise();
            }
        }
    }

    public void EnemyDeclineRaise()
    {
        ui.AnimateRaiseArrow(false);
        EnableWaitDialog(false);
        currentDamageThisRound -= GetDmgPenelty();
        SetState(new EndRound(this, false, true));
    }

    public void EnemyAcceptRaise()
    {
        EnableWaitDialog(false);
        UpdateRaise();
        ui.AnimateRaiseArrow(true);
        ui.turnTimer.PauseTimer(false);
        Debug.Log("DamageIsNow: " + currentDamageThisRound);
        StartCoroutine(AutoEndTurn());
    }

    public float GetDmgPenelty()
    {
        float dmg = 100;
        if (currentTurn == 6 || currentTurn == 5)
        {
            dmg = 500;
        }
        else if (currentTurn == 4 || currentTurn == 3)
        {
            dmg = 300;
        }
        return dmg;
    }

    private void UpdateRaise()
    {
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.BetYes, true);
        currentDamageThisRound += choosenRaise;
        ui.hpForThisRoundText.text = currentDamageThisRound.ToString();
    }

    [Button]
    public void EnemyBetPopUp(int dmg)
    {
        ui.SlideRankingImgIfOpen();
        choosenRaise = dmg;
        EnableBetDialog(dmg, true);
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.BetAsk, true);
        ui.turnTimer.PauseTimer(true);
    }

    public void AcceptBet()
    {
        UpdateRaise();
        EnableBetDialog(0, false);
        ui.AnimateRaiseArrow(true);
        ui.turnTimer.PauseTimer(false);
        if (BOT_MODE)
        {
            SetState(new BotEnemy(this, currentTurn, false, true));
        }
        else
        {
            UpdateBetDB("yes");
        }
    }

    public void RefuseBet()
    {
        isRoundReady = false;
        currentDamageThisRound -= GetDmgPenelty();
        SetState(new EndRound(this, false, false));
        EnableBetDialog(0, false);
        ui.AnimateRaiseArrow(false);
        if (!BOT_MODE)
        {
            UpdateBetDB("no");
        }
    }

    public void OfferABet()
    {
        if (!raiseOffer && ui.betBtn.btnBetClickable)
        {
            ui.betBtn.btnBetClickable = false;
            ui.SetOfferChooseRaiseDialog(true, currentDamageThisRound - GetDmgPenelty(), EnoughHpToRaise(1), EnoughHpToRaise(2));
        }
        else
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CantClick, false);
        }
    }

    public void PlayerChooseRaise(int dmg)
    {
        choosenRaise = raiseOptions[dmg];
        ui.SetOfferChooseRaiseDialog(false, 0, false, false);
        raiseOffer = true;
        ui.betBtn.EnableBetBtn(false);
        ui.turnTimer.PauseTimer(true);
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.BtnClick, false);
        EnableWaitDialog(true);
        if (BOT_MODE)
        {
            SetState(new BotEnemy(this, currentTurn, true, false));
        }
        else
        {
            UpdateBetDB(currentGameInfo.localPlayerId + choosenRaise);
        }
    }

    private void UpdateBetDB(string info)
    {
        if (!BOT_MODE)
        {
            gameManager.SetBetOffer(info, () =>
                        Debug.Log("You setbet Offer" + info)
                    ,
                   Debug.Log);
        }
    }
    public void EnableBetDialog(int dmgRaise, bool enable)
    {
        if (enable)
        {
            ui.ShowRaiseDialog(currentGameInfo.EnemyId.Substring(1), dmgRaise + currentDamageThisRound, currentDamageThisRound - GetDmgPenelty());
            ShowHpInfo();
        }
        else
        {
            ui.HideRaiseDialog();
            ui.HideHpDialog();
        }
        ui.UpdateHpZ(enable);
        ui.EnableDarkScreen(true, enable, () => DisableVision());
    }
    public void EnableWaitDialog(bool enable)
    {
        ui.ShowWaitingDialog(enable);
        ui.UpdateHpZ(enable);
        if (enable)
        {
            ShowHpInfo();
        }
        else
        {
            ui.HideHpDialog();
        }
    }

    [Button]
    internal bool DealHpDamage(bool dealToPlayer, bool endByBet)
    {
        extraDamage = 0;
        if (!endByBet)
        {
            extraDamage = ConvertWinenerRankToDamage(!dealToPlayer);
        }
        if (dealToPlayer)
        {
            playerHp -= currentDamageThisRound;
            playerHp -= extraDamage;
            if (playerHp <= 0)
            {
                return true;
            }
        }
        else
        {
            enemyHp -= currentDamageThisRound;
            enemyHp -= extraDamage;
            if (enemyHp <= 0)
            {
                return true;
            }
        }
        return false;
    }

    public float ConvertWinenerRankToDamage(bool playerWin)
    {
        int rank = ui.ConvertHandRankToTextNumber(cardsDeckUi.CalculateHand(true, playerWin, false, false).Rank);
        switch (rank)
        {
            case 1:
                return 1600f;
            case 2:
                return 1400f;
            case 3:
                return 1200f;
            case 4:
                return 1000f;
            case 5:
                return 700f;
            case 6:
                return 500f;
            case 7:
                return 300f;
            case 8:
                return 200f;
            case 9:
                return 100f;
            case 10:
                return 0f;
        }
        return 0f;
    }

    public void ShowHpInfo()
    {
        ui.ShowHpDialog(playerHp, true);
        ui.ShowHpDialog(enemyHp, false);
    }
    internal bool HaveEnoughToRaise()
    {
        return EnoughHpToRaise(0) && !raiseOffer;
    }

    public bool EnoughHpToRaise(int option)
    {
        return (playerHp >= (currentDamageThisRound + raiseOptions[option]))
              && (enemyHp >= (currentDamageThisRound + raiseOptions[option]));
    }

    public void ResetEs(bool isPlayer)
    {
        ui.ResetEs(isPlayer);
        ResetPuAction = () => StartCoroutine(ResetPuUi(isPlayer, -1));
    }
    internal bool ShouldFlipTurnAfterBetDecline()
    {
        if (!BOT_MODE)
        {
            return LocalTurnSystem.Instance.ShouldFlipArrowAfterRaiseDeclined();
        }
        else
        {
            return (firstToPlayBotMode && IsPlayerTurn()) || (!firstToPlayBotMode && !IsPlayerTurn());
        }

    }

    internal void ResetNC()
    {
        ui.SetCardSelection(2, "", new Vector2(0, 0), false, false);
        SetState(new PowerUpState(this, false, 0, newPowerUpName, "reset", "", new Vector2(0, 0), new Vector2(0, 0), newPuSlotIndexUse));
        Constants.cardsToSelectCounter = 2;
    }
    private string LoadPlayerElementString()
    {
        return PlayerPrefs.GetString("player_element_string", "f");
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
    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            Debug.Log("Pause");
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }
    [Button]
    public void PauseGame()
    {
        Time.timeScale = 0;
    }
    [Button]
    public void ResumeGame()
    {
        Time.timeScale = 1;
    }


}

