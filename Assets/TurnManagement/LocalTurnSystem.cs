using System;
using System.Collections;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using MyBox;
using Sirenix.OdinInspector;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;
using Firebase;
using Firebase.Database;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;

public class LocalTurnSystem : Singleton<LocalTurnSystem>
{




    [Required] [SerializeField] private FirebaseTurnMangementNetworkBinder FirebaseTurnMangementNetworkBinder;
    public int MaxTurns = 6;
    public int MaxRounds = 3;
    public bool isPlayerFirstPlayer;
    // public ReactiveProperty<long> RoundCounter;

    public SynchronyzedFirebaseProperty<long> TurnCounter = new SynchronyzedFirebaseProperty<long>(-1);
    public SynchronyzedFirebaseProperty<long> RoundCounter = new SynchronyzedFirebaseProperty<long>(1);

    public bool GameInitlized = false;
    [ShowInInspector]
    private SynchronyzedFirebaseProperty<string> _PlayerID = new SynchronyzedFirebaseProperty<string>("-1");

    [ShowInInspector]
    public SynchronyzedFirebaseProperty<string> PlayerID
    {
        get => _PlayerID;

    }
    [ShowInInspector]



    // public SynchronyzedFirebaseProperty<string> OtherPlayerID = new SynchronyzedFirebaseProperty<string>("-1");


    public SynchronyzedFirebaseProperty<long> PlayerReady = new SynchronyzedFirebaseProperty<long>(-1);
    public SynchronyzedFirebaseProperty<long> OtherPlayerReady = new SynchronyzedFirebaseProperty<long>(-1);
    /*
    public SynchronyzedFirebaseProperty<bool> PlayerReady = new SynchronyzedFirebaseProperty<bool>(false);
    public SynchronyzedFirebaseProperty<bool> OtherPlayerReady = new SynchronyzedFirebaseProperty<bool>(false);*/

    /*  [Required]
      public SynchronyzedFirebaseProperty<string> FirstPlayer = new SynchronyzedFirebaseProperty<string>("a");
      [Required]
      public SynchronyzedFirebaseProperty<string> SecondPlayer = new SynchronyzedFirebaseProperty<string>("b");*/

    [ShowInInspector]
    [GUIColor(1f, 1f, 0f)]
    public SynchronyzedFirebaseProperty<string> CurrentPlayerID = new SynchronyzedFirebaseProperty<string>("-1");

    [Button]
    public void SetPlayerReady(long roundNumber)
    {
        PlayerReady.Value = roundNumber;
    }

    public event Action<string> onTurnStartedForPlayer;

    public void DecrementTurnCounter()
    {
        TurnCounter.Value -= 1;
        TurnStartedNotificationTrigger.Value += 1;
    }

    private void Awake()
    {
        Debug.LogWarning("awwake LRA");
        // InitializeSingleton(true);
        DontDestroyOnLoad(this.gameObject);
    }


    [Button]
    public void PassTurn()
    {
        if (CurrentPlayerID.Value == PlayerID.Value)
        {

            if (TurnCounter.Value == 1)
            {
                TurnCounter.Value = -1;
            }
            else if (TurnCounter.Value > 1)
            {
                TurnCounter.Value -= 1;
                CurrentPlayerID.Value = OtherPlayerID;
                // TurnStartedNotificationTrigger.Value += 1;
            }/*else if(TurnCounter.Value == -1)
            {
                //CurrentPlayerID.Value = OtherPlayerID;
                Debug.LogError("WHARME");
            }*/


        }
    }

    public bool IsPlayerStartRound()
    {
        if (RoundCounter.Value % 2 == 0)
        {
            return PlayerID.Value.Equals(SecondPlayerStr);
        }
        else
        {
            return PlayerID.Value.Equals(FirstPlayerStr);
        }
    }

    public SynchronyzedFirebaseProperty<long> TurnStartedNotificationTrigger = new SynchronyzedFirebaseProperty<long>(-1);
    public SynchronyzedFirebaseProperty<long> GameStartedNotificationTrigger = new SynchronyzedFirebaseProperty<long>(-1);
    [ShowInInspector]
    public string FirstPlayerStr;
    [ShowInInspector]
    public string SecondPlayerStr;
    [ShowInInspector]
    public string OtherPlayerID;

    public void TriggerGameStarted()
    {
        GameStartedNotificationTrigger.Value += 1;
    }

    public void TriggerTurn()
    {
        TurnStartedNotificationTrigger.Value += 1;
    }

    /* public void SwitchPlayer()
     {
         CurrentPlayerID.Value = OtherPlayerID.Value;
     }*/

    public event Action onTurnStarted;
    public event Action onGameStarted;
    public DatabaseReference dbRef;
    public string[] playerIDs;
    public string MyPlayerID;
    internal bool BindCheck = false;

    public void Init(DatabaseReference dbRef, string[] playerIDs, string MyPlayerID)
    {

        this.dbRef = dbRef;
        this.playerIDs = playerIDs;
        this.MyPlayerID = MyPlayerID;

    }
    [Button]
    public async void Inito(Action startRound)
    {

        await Task.Delay(1000);
        TurnCounter.Value = MaxTurns;
        isPlayerFirstPlayer = playerIDs[0].ToString().Equals(MyPlayerID);
        FirebaseTurnMangementNetworkBinder.Initialize(dbRef, playerIDs, MyPlayerID);
        await Task.Delay(1000);
        if (BindCheck)
        {
            startRound.Invoke();
        }
        while (!BindCheck)
        {
            await Task.Delay(1000);
            if (BindCheck)
            {
                startRound.Invoke();
            }
        }
       
        // SceneManager.LoadScene("GameScene2");

        /* TurnCounter.onValueChanged += x =>
         {
             if (x <= 0)
             {
                 RoundEnded();
             }
         };*/
        //   GameStartedNotificationTrigger.onValueChanged += b => { onGameStarted?.Invoke();};
        /*onGameStarted += () => { Debug.LogWarning($"Game Started! {PlayerID.Value}"); };

        //  TurnStartedNotificationTrigger.onValueChanged += b => onTurnStarted?.Invoke();
        onTurnStarted += () =>
        {
            if (CurrentPlayerID == PlayerID)
            {
                onTurnStartedForPlayer?.Invoke(CurrentPlayerID.Value);
            }
        };

        onTurnStarted += () =>
        {
            Debug.LogWarning($"Turn Started! {PlayerID.Value} ");
        };
        onTurnStartedForPlayer += s =>
        {
            Debug.LogWarning($"Turn started for {s}");
        };
        GameInitlized = true;*/
    }

    [Button]
    public void InitBind(string FirstPlayer)
    {
        FirebaseTurnMangementNetworkBinder.BindPlayerToTurnManagement("");

    }

    public event Action onRoundEnded;
    public event Action onRoundStarted;

    public void RoundEnded()
    {
        Debug.LogWarning("Round Ended");
        CurrentPlayerID.Value = "-1";
        onRoundEnded?.Invoke();
    }

    public void RoundStarted()
    {
        Debug.LogWarning("Round Started");
        onRoundStarted?.Invoke();
    }
    [Button]
    public bool IsPlayerTurn()
    {
        return CurrentPlayerID.Value.ToString().Equals(PlayerID.Value.ToString());
    }

    public void LeaveGame()
    {
        CurrentPlayerID.Value = PlayerID.Value.ToString() + "(#Exit#)";
    }
 
}
