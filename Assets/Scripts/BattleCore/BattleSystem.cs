
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

    private bool TEST_MODE;
    public bool BOT_MODE;
    [SerializeField] public bool END_TURN_AFTER_PU;



    public BattleUI Interface => ui;

    [SerializeField] private BattleUI ui;
    [SerializeField] public PlayerInfo player;


    [SerializeField] public PlayerInfo enemy;


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
    public bool firstRound = true;
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
    private bool timedOut;
    private bool emojisWheelDisplay;



    private bool emojiCooledDown = true;
    private readonly long TURN_COUNTER_INIT = 6;
    private readonly float DELAY_BEFORE_NEW_ROUND = 6f;

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

    public bool enemyBotSkillUsed;
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

    public event Action onGameStarted;



    [Button]
    public void CheckValues()
    {
        Debug.Log("currentRound" + currentRound);
        Debug.Log("currentTurn" + currentTurn);
        Debug.Log("IsPlayerTurn" + IsPlayerTurn());
        Debug.Log("ReplaceInProgress" + ReplaceInProgress);
        Debug.Log("++++++++++++++++++");
        Debug.Log("playerPuInProcess" + playerPuInProcess);
        Debug.Log("btnReplace" + btnReplaceClickable);
        Debug.Log("energyCounter" + energyCounter);
        Debug.Log("TemproryUnclickable" + Constants.TemproryUnclickable);
        Debug.Log("replaceMode" + replaceMode);
        Debug.Log("++++++++++++++++++");
        Debug.Log("selectMode" + selectMode);
        Debug.Log("endTurnInProcess" + endTurnInProcess);
        Debug.Log("timedOut" + timedOut);
        Debug.Log("infoShow" + infoShow);
        Debug.Log("deckGenerate" + deckGenerate);
        Debug.Log("endRoutineFinished" + endRoutineFinished);
        Debug.Log("firstRound" + firstRound);

    }

    private void Start()
    {
        TEST_MODE = Values.Instance.TEST_MODE;
       // Constants.BOT_MODE = true;
        BOT_MODE = Constants.BOT_MODE;
        if (TEST_MODE || BOT_MODE)
        {
            InitTestMode();
        }
        else
        {
            gameManager = MainManager.Instance.gameManager;
            currentGameInfo = gameManager.currentGameInfo;
        }
        player = new PlayerInfo();
        enemy = new PlayerInfo();
        player.id = currentGameInfo.localPlayerId;
        enemy.id = currentGameInfo.EnemyId;
        playerLifeLeft = 2;
        enemyLifeLeft = 2;
        Interface.Initialize(player, enemy);
        startNewRound = true;
        ui.LoadNinjaBG();
        ui.InitAvatars();
        if (!TEST_MODE && !BOT_MODE)
        {
            StartCoroutine(ui.CoinFlipStartGame(currentGameInfo.playersIds[0].ToString().Equals(player.id), () => ui.SlidePuSlots()));
            LocalTurnSystem.Instance.Inito(() => StartCoroutine(InitGameListeners()));
        }
        else
        {
            DisplayStatistics();
            Constants.TemproryUnclickable = false;
            firstToPlayBotMode = GetRandomFirstTurn();
            StartCoroutine(ui.CoinFlipStartGame(firstToPlayBotMode, () =>
             {
                 ui.SlidePuSlots();
                 if (firstDeck)
                 {
                     firstDeck = false;
                     SetState(new BeginRound(this, firstToPlayBotMode, true));
                 }
             }));
        }
    }

    private bool GetRandomFirstTurn()
    {
        if (!BOT_MODE)
        {
            return true;
        }
        else
        {
            System.Random rng = new System.Random();
            return rng.Next(0, 2) > 0;
        }

    }

    private void InitTestMode()
    {
        currentGameInfo = new GameInfo();
        currentGameInfo.gameId = "zxc";
        currentGameInfo.prize = 1000;
        string playerName = PlayerPrefs.GetString("player_name", "Ninja");
        manualPuDeck = true;
        if (TEST_MODE)
        {
            playerName += "TEST";
        }
        currentGameInfo.playersIds = new String[] { playerName, "Alex" };
        currentGameInfo.localPlayerId = playerName;
        currentGameInfo.EnemyId = "Alex";
        currentGameInfo.turn = "6";
        currentTurn = 6;
        currentGameInfo.cardDeck = CreateCardsDeck(BOT_MODE);

        currentGameInfo.puDeck = CreatePuDeck(BOT_MODE);
    }

    private string[] CreatePuDeck(bool newDeck)
    {
        string[] deck;
        if (newDeck)
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
                 "fm2","wm2","fm2",
                 "fm1","fm1","fm1",
                 "fm1","fm1","fm1",
                "fm1", "fm1"};
        }
        return deck;
    }

    private string[] CreateCardsDeck(bool newDeck)
    {
        string[] deck;
        if (newDeck)
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
                "Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah","Ac", "Ah","As","Ah",
                "8c", "3h","9s",board[4],board[3], board[2],board[1],board[0],enemysHand[1], playersHand[1],enemysHand[0],playersHand[0]};
        }
        return deck;
    }

    private string[] ShuffleArray(string[] deck)
    {
        System.Random rnd = new System.Random();
        return deck.OrderBy(x => rnd.Next()).ToArray();
    }

    /*  private void LoadSoundSettings()
      {
          SoundManager.Instance.MAX_VOL_MUSIC = LoadPrefs(Constants.Instance.volumeSoundKey);
          ui.musicSlider.value = SoundManager.Instance.MAX_VOL_MUSIC;
      }*/

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
        Debug.Log("Start" + " TU " + Constants.TemproryUnclickable);

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
            ui.EnableDarkScreen(true, false, () =>
             {
                 DisableVision();
                 Debug.Log("endDS" + " " + endTurnInProcess);
                 StartCoroutine(ResetSortingOrder(false));
                 puDeckUi.EnablePusZ(true, false);
                 cardsDeckUi.DisableCardsSelection(Constants.AllCardsTag);
                 ActivatePlayerButtons(!endTurn, false);
             });
        }
        if (replaceMode && !Constants.TemproryUnclickable)
        {
            Debug.LogError("Disabling");
            EnableReplaceDialog(true, endTurn);
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
        if (!TEST_MODE && !BOT_MODE)
        {
            LocalTurnSystem.Instance.LeaveGame();
        }
        LoadMenuScene(false);
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
        if (!TEST_MODE && !BOT_MODE)
        {
            BindTurnCounter();
            BindRoundCounter();
            BindPlayersReady();
            BindCurrentPlayer();
            ListenForNewDeck();
            ListenForPowerupUse();
            ListenForEmoji();
        }
    }

    private void ListenForEmoji()
    {
        gameManager.ListenForEmoji(player.id, emojiId =>
        {
            StartCoroutine(ui.DisplayEmoji(false, emojiId, null));
        }, Debug.Log);
    }

    [Button]
    public void SoundCheck(int emojiId)
    {
        SavePrefsInt(Constants.Instance.PLAYER_WIN_BOT, 0);
        SavePrefsInt(Constants.Instance.PLAYER_LOSE_BOT, 0);
        //SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndRoundGong, true);
        //StartCoroutine(ui.DisplayEmoji(false, emojiId, null));
        //  SavePrefsInt(Constants.Instance.PLAYER_WIN_BOT,/*LoadPrefsInt(Constants.Instance.PLAYER_WIN_BOT) +*/ 1);
        ui.playerNameText.text = LoadPrefsInt(Constants.Instance.PLAYER_WIN_BOT).ToString();
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
            currentTurn = 6;
            isRoundReady = true;
            currentGameInfo.cardDeck = CreateCardsDeck(true);
            ui.winLabel.SetActive(false);
            firstToPlayBotMode = !firstToPlayBotMode;
            SetState(new BeginRound(this, firstToPlayBotMode, false));
        }
            currentRound++;
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


    private void TurnEvents(int currentTurn)
    {
        switch (currentTurn)
        {
            case 4:
                // firstDeck = false;
                //  waitForDrawerAnimationToEnd = true;
                //  yield return new WaitForSeconds(0.7f);
                //cardsDeckUi.DealCardsForBoard(true, () => waitForDrawerAnimationToEnd = false, () => UpdateHandRank(false));
                break;
            case 2:
                // cardsDeckUi.DealCardsForBoard(true, () => waitForDrawerAnimationToEnd = false, () => UpdateHandRank(false));
                break;
            case 1:
                break;
            case 0:
            case -1:
                //  isRoundReady = false;
                SetState(new EndRound(this, false));
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
        if (!TEST_MODE && !BOT_MODE)
        {
            if (!deckGenerate && LocalTurnSystem.Instance.isPlayerFirstPlayer)
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
        if (!enemyPuIsRunning && !turnInitInProgress)
        {
            Debug.Log("et ABOUT");
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndTurnGong, true);
            SetState(new PlayerTurn(this, currentTurn));
        }
        else
        {
            while (enemyPuIsRunning || turnInitInProgress)
            {
                yield return new WaitForSeconds(0.6f);
                Debug.Log("e " + enemyPuIsRunning);
                Debug.Log("t " + turnInitInProgress);
                if (!enemyPuIsRunning && !turnInitInProgress)
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
        Debug.LogError("CurrentT " + currentTurn);
        if (!endTurnInProcess && !ReplaceInProgress)
        {
            endTurnInProcess = true;
            DisableSelectMode(true);
            ui.EnableBgColor(false);
            StartCoroutine(AnimationManager.Instance.AlphaAnimation(ui.turnTextGO.GetComponent<SpriteRenderer>(), false, Values.Instance.textTurnFadeOutDuration, null));
            DisablePlayerPus();
            ui.SetTurnIndicator(false, false);
            ResetTimers();
            if (!TEST_MODE && !BOT_MODE)
            {
                gameManager.AddGameActionLog(GameManager.ActionEnum.EndTurn, "end of turn: " + currentTurn, () => { }, Debug.Log);
                LocalTurnSystem.Instance.PassTurn();
            }
            else
            {
                isPlayerBotModeTurn = false;
                // TurnEvents(--currentTurn);// SIM LEV MINUS TURN
                if (--currentTurn > 0)
                {
                    SetState(new EnemyTurn(this, currentTurn));
                }
                else if (currentTurn == 0)
                {
                    SetState(new EndRound(this, false));
                }
            }
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndTurnGong, true);
        }
        else
        {
            Debug.LogError("endingProcees");
        }
    }

    internal List<string> GetRandomAvailableCardsNames()
    {
        List<string> cardsNames = cardsDeckUi.GetAvailbeCards();
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
        Constants.cardsToSelectCounter = 0;
        /*TemproryUnclickable = true;
        yield return new WaitForSeconds(2f);
        TemproryUnclickable = false;
    */
        if (IsPlayerTurn())
        {
            yield return new WaitForSeconds(0.5f);
            if (!fm1Activated && newPowerUpName.Equals("fm1"))
            {
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
        if (playerPuInProcess || ReplaceInProgress)
        {
            playerPuInProcess = false;

            yield return new WaitForSeconds(4f);
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
            Debug.Log("s" + s);
            if (s == LocalTurnSystem.Instance.PlayerID.Value)
            {
                StartCoroutine(StartPlayerTurn(false, null));
                //  ui.playerNameText.text = "Bind " + currentTurn;

            }
            else if( s == currentGameInfo.EnemyId)
            {
                SetState(new EnemyTurn(this, currentTurn));
                // ui.enemyNameText.text = "Bind " + currentTurn;
            }
            else if (s.Contains("(#Exit#)"))
            {
                ActivatePlayerButtons(false, false);
                ui.WinPanelAfterEnemyLeaveGame(currentGameInfo.EnemyId);
                SetState(new GameOver(this, true));
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
        StartCoroutine(AnimationManager.Instance.AlphaAnimation(ui.turnTextGO.GetComponent<SpriteRenderer>(),
            false, Values.Instance.textTurnFadeOutDuration, () =>
            {
                // ui.playerNameText.text = "start " + currentTurn;
                EndAction?.Invoke();
                StartCoroutine(CheckIfEnemyPuRunningAndStartPlayerTurn());
            }));
        ;
    }


    public void FakeEnemyEndTurn()
    {
        Debug.LogError("CurrentT " + currentTurn);

        if (--currentTurn > 0)
        {
            StartCoroutine(StartPlayerTurn(true, () =>
            {
                StartCoroutine(AnimationManager.Instance.AlphaAnimation(ui.turnBtnSpriteREnderer, true, Values.Instance.turnBtnAlphaDuration, null));
            }));
        }
        else if (currentTurn == 0)
        {
            SetState(new EndRound(this, false));
        }
    }


    public async void FakeEnemyPuUse(int puIndex, string cardPlace1, string cardPlace2, bool endTurn)
    {
        Debug.LogWarning("FAking it");
        string puName = "";
        if (puIndex == -1)
        {
            puName = "sflip";
        }
        else
        {
            finishPuDissolve = false;
            puName = puDeckUi.GetPuFromList(false, puIndex).puName;
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
                SetState(new EndRound(this, false));
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
        ResetFlusherStrighter();
        isPlayerFlusher = false;
        isEnemyFlusher = false;
        isPlayerStrighter = false;
        isEnemyStrighter = false;
        playerHandIsFlusher = false;
        playerHandIsStrighter = false;
        enemyHandIsFlusher = false;
        enemyHandIsStrighter = false;
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
        // currentTurn = 1;
        if (firstRound)
        {
            currentRound = 1;
            if (!TEST_MODE && !BOT_MODE)
            {
                LocalTurnSystem.Instance.RoundCounter.Value = 1;
            }
            firstRound = false;
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
        if (!TEST_MODE && !BOT_MODE)
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
        if (!TEST_MODE && !BOT_MODE)
        {
            return LocalTurnSystem.Instance.IsPlayerTurn();
        }
        return isPlayerBotModeTurn;
    }

    public void DissolvePuAfterUse(bool isPlayer, int index)
    {
        puDeckUi.GetPuFromList(isPlayer, index).DissolvePu(2f, Values.Instance.puDissolveDuration, null, () => StartCoroutine(ResetPuUi(isPlayer, index)));
    }

    private void ListenForPowerupUse()
    {
        gameManager.ListenForPowerupUse(powerUpInfo =>
        {
            if (!powerUpInfo.playerId.Equals(player.id))
            {
                EnemyPuUse(powerUpInfo);
            }
        }, powerUpInfo =>
        {
            if (!powerUpInfo.playerId.Equals(player.id))
            {
                if (powerUpInfo.slot == -1)
                {
                    DealPu(false, null);
                }
                else
                {
                    ReplacePu(false, powerUpInfo.slot);
                }
            }
        }, Debug.Log);
    }

    private void EnemyPuUse(PowerUpInfo powerUpInfo)
    {
        isPlayerActivatePu = false;
        enemyPuIsRunning = true;
        ui.EnableDarkScreen(false, true, () => DisableVision());
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

    public IEnumerator OnCardsSelectedForPU(string cardPlace, Vector2 position)
    {
        if (Constants.cardsToSelectCounter == 0)
        {
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

    public void ShowPuInfo(Vector2 startingPosition, bool paddingRight, string puName, string puDisplayName)
    {
        infoShow = true;
        ui.ShowPuInfoDialog(startingPosition, paddingRight, puName, puDisplayName, true, false, null);
    }
    public void HideDialog()
    {
        ui.ShowPuInfoDialog(new Vector2(0, 0), false, " ", " ", false, false, () => infoShow = false);
    }

    internal IEnumerator ResetPuUi(bool isPlayer, int puIndex)
    {
        PowerUpUi pu = puDeckUi.GetPu(isPlayer, puIndex);
        puDeckUi.RemovePuFromList(isPlayer, puIndex);
        Debug.LogWarning("RESETING");
        if (newPowerUpName.Contains("m2"))
        {
            yield return new WaitForSeconds(5f);
            Debug.LogWarning("Updating");
        }
        puDeckUi.ResetPuUI(pu, () => UpdateHandRank(false));
        if (isPlayer)
        {
            StartCoroutine(AutoEndTurn());
        }
        else
        {
            finishPuDissolve = true;
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
        if (!TEST_MODE && !BOT_MODE)
            gameManager.SetNewPowerupUseDB(new PowerUpInfo(player.id, newPowerUpName, Constants.Instance.ConvertCardPlaceForEnemy(cardTarget), Constants.Instance.ConvertCardPlaceForEnemy(secondCardTargetPU), puIndex, CreateTimeStamp()), () =>
        {
            gameManager.AddGameActionLog(GameManager.ActionEnum.PuUse, "name: " + newPowerUpName + " c1: " + cardTarget + " c2: " + secondCardTargetPU, () => { }, Debug.Log);

        }, Debug.Log);
    }

    public void UpdateReplacePuInDb(int puIndex)
    {
        if (!TEST_MODE && !BOT_MODE)
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
            Constants.TemproryUnclickable = true;
            ReplaceInProgress = true;
            DisablePlayerPus();
            ReduceEnergy(Values.Instance.energyCostForDraw);
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
                Debug.Log("doneProgress");
                ReplaceInProgress = false;
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
            else if (puUi.puName.Equals("wm1") && cardsDeckUi.IsOneCardFromHandsFreeze())
            {
                puUi.EnablePu(false);
            }
            else if (puUi.puName.Equals("wm2") && (cardsDeckUi.IsOneCardFromHandsFreeze() || cardsDeckUi.IsOneCardFromBoardFreeze()))
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
            FreezePlayingCard(cardTarget2, false, false);
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
            if (lastCard)
            {
                cardsDeckUi.CloseDrawer();
            }
        }
        else
        {
            bool isFlip = IsEnemyCard(cardPlace);
            StartCoroutine(ReplaceSelectedCard(cardPlace, isFlip, delay, ResetEnable, firstCard, lastCard));
            // ReplaceSelectedCard2(cardPlace, isFlip, delay, ResetEnable, firstCard, lastCard);
        }
    }
    public async void DestroyAndDrawCard2(string cardPlace, int delay, bool ResetEnable, bool firstCard, bool lastCard)
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
        await Task.Delay(delay);
        if (puDeckUi.IsCardFreeze(cardPlace))
        {
            FreezePlayingCard(cardPlace, false, ResetEnable);
        }
        else
        {
            bool isFlip = IsEnemyCard(cardPlace);
            float ddelay = delay / 1000;
            //   StartCoroutine(ReplaceSelectedCard(cardPlace, isFlip, ddelay, ResetEnable, firstCard, lastCard));
            ReplaceSelectedCard2(cardPlace, false, delay, ResetEnable, firstCard, lastCard);
        }
    }

    private bool IsEnemyCard(string cardPlace)
    {
        return cardPlace.Contains("Enemy");
    }

    public IEnumerator ReplaceSelectedCard(string cardPlace, bool isFlip, float delay, bool ResetEnable, bool isFirstCard, bool isLastCard)
    {
        if (cardsDeckUi.GetParentByPlace(cardPlace).smokeEnable)
        {
            SmokeCardPu(false, cardPlace, false, false, false);
        }
        yield return new WaitForSeconds(delay);
        cardsDeckUi.DestroyCardObject(cardPlace, null);
        yield return new WaitForSeconds(0.4f);
        Action resetAction = null;
        if (ResetEnable)
        {
            resetAction = () => EnableDarkAndSorting(false);
        }
        cardsDeckUi.DrawAndReplaceCard(cardPlace, isFlip, resetAction, isFirstCard, isLastCard);
    }
    public async void ReplaceSelectedCard2(string cardPlace, bool isFlip, int delay, bool ResetEnable, bool isFirstCard, bool isLastCard)
    {
        if (cardsDeckUi.GetParentByPlace(cardPlace).smokeEnable)
        {
            SmokeCardPu(false, cardPlace, false, false, false);
        }

        await Task.Delay(500);
        cardsDeckUi.DestroyCardObject(cardPlace, null);
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
        }
        ui.EnableDarkScreen(isPlayerActivatePu, enable, () =>
        {
            StartCoroutine(ResetSortingOrder(enable));
            DisableVision();
        });
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

        // UpdateHandRank(false);
        //THIS ABOVE CAUSE PROB
        // yield return new WaitForFixedUpdate();
        if (!enable)
        {
            enemyPuIsRunning = false;
            playerPuInProcess = false;
            foreach (CardUi card in cardsDeckUi.GetListByTag("All"))
            {
                yield return new WaitForEndOfFrame();
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

        else if (timedOut || energyCounter == 0 /*|| energyCounter == 1 && puDeckUi.GetPuListCount(true) == 0 && !skillUsed*/)
        {
            Debug.LogError("ending");
            timedOut = false;
            yield return new WaitForSeconds(1.5f);
            if (newPowerUpName.Contains("m2") || ReplaceInProgress)
            {
                yield return new WaitForSeconds(2.5f);
            }
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
        Debug.LogError("HowM " + GenerateRandom(0, 2));

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
            if (isPlayerActivate)
            {
                StartCoroutine(AutoEndTurn());
            }
        });

    }


    internal void SwapTwoCards(string cardTarget1, string cardTarget2, bool reset)
    {
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
        if (powerUpName.Equals(nameof(PowerUpStruct.PowerUpNamesEnum.im2)))
        {
            StartCoroutine(ui.StartIcenado());
            IgnitePowerUp.Invoke();
        }
        else if (powerUpName.Equals(nameof(PowerUpStruct.PowerUpNamesEnum.fm2)))
        {
            StartCoroutine(ui.StartArmageddon());
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
            UpdateReplacePuInDb(-1);
            ReduceEnergy(Values.Instance.energyCostForDraw);
            ui.EnablePlayerButtons(false);
            DisablePlayerPus();
            DealPu(true, () =>
            {
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

                    if (replaceMode)
                    {
                        puDeckUi.EnablePusSlotZ(true, true);
                        ui.EnableDarkScreen(isPlayerActivatePu, true, () => StartCoroutine(SetClickableWithDelay(0.5f)));
                    }
                    else
                    {
                        ui.EnableDarkScreen(isPlayerActivatePu, false, () =>
                         {
                             StartCoroutine(SetClickableWithDelay(0.5f));
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
            ui.UpdateRankTextInfo(true, handRank);
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
        ui.UpdateRankTextInfo(false, 0);
    }

    private void DisableVision()
    {
        if (visionOn)
        {
            AnimationManager.Instance.VisionEffect(cardsDeckUi.GetListByTag(Constants.AllCardsTag), cardsDeckUi.GetListByTag(Constants.AllCardsTag).Count, false);
            SoundManager.Instance.PlayConstantSound(SoundManager.ConstantSoundsEnum.Vision, false);
            ui.UpdateRankTextInfo(false, 0);
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
        Debug.Log("ap " + enable + " " + endTurnInProcess);
        if (delay)
        {
            StartCoroutine(ActivePlayerButtonWithDelay());
        }
        else
        {
            if (enable && !endTurnInProcess)
            {
                Constants.TemproryUnclickable = false;
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
            UpdateEmojiDB(id);
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
        if (!TEST_MODE && !BOT_MODE)
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
        SetState(new EndRound(this, false));
    }

    internal void OnPuClick(PowerUpUi powerUpUi)
    {
        if (infoShow)
        {
            HideDialog();
        }
        else if (!ReplaceInProgress && !Constants.TemproryUnclickable && powerUpUi.isPlayer && !powerUpUi.freeze && powerUpUi.isClickable)
        {
            powerUpUi.isClickable = false;
            powerUpUi.aboutToDestroy = true;
            Interface.EnablePlayerButtons(false);
            DisablePlayerPus();
            Interface.EnableDarkScreen(powerUpUi.isPlayer, true, () => DisableVision());
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
        }
        else if (powerUpUi.isPlayer || !powerUpUi.isPlayer)
        {
            StartCoroutine(AnimationManager.Instance.Shake(powerUpUi.spriteRenderer.material, Values.Instance.disableClickShakeDuration));
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CantClick, false);
        }
    }

    private void OnGUI()
    {

        if (Event.current.type == EventType.KeyDown)
        {

            if (Input.GetKeyDown("escape"))
            {
                if (!ui.IsSliderOpen())
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
        cardsDeckUi.DealCardsForBoard(true, null /*() => waitForDrawerAnimationToEnd = false*/, () => UpdateHandRank(false));
    }


    [Button]
    public void InternetChecK()
    {
        StartCoroutine(CheckInternetConnection(result => Debug.Log("Internet: "+ result)));
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

