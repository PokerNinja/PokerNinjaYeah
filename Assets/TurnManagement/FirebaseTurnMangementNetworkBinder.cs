using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Firebase.Database;
using Firebase.Extensions;
using Sirenix.OdinInspector;
using UniRx;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using Random = UnityEngine.Random;

public class FirebaseTurnMangementNetworkBinder : MonoBehaviour
{
    [Required]
    public LocalTurnSystem LocalTurnSystem;

    public DatabaseReference DataBaseReference;

    private string _databaseReferenceToPath = string.Empty;

    [ShowInInspector]
    public string DatabaseReferencePath
    {
        get
        {
            if (_databaseReferenceToPath == string.Empty)
            {
                if (DataBaseReference != null)
                {
                    _databaseReferenceToPath = DataBaseReference.Reference.ToString();
                }
            }
            else
            {
                return _databaseReferenceToPath;
            }
            return "Empty String";

        }
    }

    public string pathToGame = string.Empty;

    private FirebaseDatabase Database => DataBaseReference.Database;

    //public DatabaseReference.

    public static string TurnDataCollectionName = "TurnManager";

    public async Task<bool> Async_InitlizedDatabaseCheck()
    {
        bool b = false;
        await DataBaseReference.Child(TurnDataCollectionName).Child("Initlized").GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Fault in access to Database");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        if (snapshot.Value is bool rb)
                            b = rb;
                    }
                }
            });

        return b;
    }

    public bool CheckIfOnlineDatabaseWasInitlized()
    {
        DataBaseAPI.DatabaseAPI.InitializeDatabase();
        DataBaseReference = FirebaseDatabase.DefaultInstance.RootReference;

        return false;
    }

    [ShowInInspector]
    public DataSnapshot returned;

    [Button]
    public void getBoolTest()
    {
        returned = DataBaseReference.Child(TurnDataCollectionName).Child("Initlized").GetValueAsync().Result;
    }

    public async void BindPlayerToTurnManagement(string PlayerID)
    {
        Debug.LogWarning("bind");
        await Task.Delay(200);
         LocalTurnSystem.CurrentPlayerID.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("CurrentPlayer"));
        /*
                LocalTurnSystem.PlayerReady.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player1_Ready"));
                LocalTurnSystem.OtherPlayerReady.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player2_Ready"));*/
        //  LocalTurnSystem.PlayerID.Value = PlayerID;

        // LocalTurnSystem.FirstPlayer.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("FirstPlayer"));
        // LocalTurnSystem.OtherPlayerID.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("SecondPlayer"));
        LocalTurnSystem.RoundCounter.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("RoundCounter"));
        LocalTurnSystem.TurnCounter.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("TurnCounter"));
        //  LocalTurnSystem.Instance.FirstPlayer.Value = await GetFirstPlayerID();
    }
    /*
        public async void InitlizeFirebaseTurnDatabase(string PlayerID)
        {
            //DataBaseReference.Child(TurnDataCollectionName).Child("Initlized").SetValueAsync(false);
            //DataBaseReference.Child(TurnDataCollectionName).Child("CurrentPlayer").SetValueAsync("-1");
            LocalTurnSystem.CurrentPlayerID.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("CurrentPlayer"));




            //DataBaseReference.Child("TurnManager").Child("Player1_Ready").SetValueAsync(false);
            //DataBaseReference.Child("TurnManager").Child("Player2_Ready").SetValueAsync(false);

            LocalTurnSystem.PlayerReady.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player1_Ready"));
            LocalTurnSystem.OtherPlayerReady.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player2_Ready"));

            //DataBaseReference.Child("TurnManager").Child("Player1").SetValueAsync("-1");
            //DataBaseReference.Child("TurnManager").Child("Player2").SetValueAsync("-1");

            LocalTurnSystem.PlayerID.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player1"));
            LocalTurnSystem.PlayerID.Value = PlayerID;

            LocalTurnSystem.OtherPlayerID.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player2"));
            //DataBaseReference.Child("TurnManager").Child("TurnChangeTrigger").SetValueAsync(-1);
            //DataBaseReference.Child("TurnManager").Child("GameStartTrigger").SetValueAsync(-1);
            LocalTurnSystem.TurnStartedNotificationTrigger.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("RoundCounter"));
           // LocalTurnSystem.GameStartedNotificationTrigger.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("GameStartTrigger"));




            // Choosing Random FirstPlayer
            *//*string firstPlayerID = "-1";
            if (Random.value > 0.5f)
            {
                firstPlayerID = LocalTurnSystem.PlayerID.Value;
            } 



            // synchronyze local turn counter and cloud data
            DataBaseReference.Child("TurnManager").Child("FirstPlayer").SetValueAsync(firstPlayerID);*//*
         //   LocalTurnSystem.FirstPlayer.BindToFirebaseValue( DataBaseReference.Child(TurnDataCollectionName).Child("FirstPlayer"));

            //DataBaseReference.Child("TurnManager").Child("TurnCounter").SetValueAsync(LocalTurnSystem.MaxTurns);
            LocalTurnSystem.TurnCounter.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("TurnCounter"));

           // DataBaseReference.Child(TurnDataCollectionName).Child("Initlized").SetValueAsync(true);
        }*/

    async Task<string> GetPlayersID(bool first)
    {
        string result = "-1";
        string value = "FirstPlayer";
        if (!first)
        {
            value = "SecondPlayer";
        }
        await DataBaseReference.Child(TurnDataCollectionName).Child(value).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted)
                {
                    Debug.LogError("Fault in access to Database");
                }
                else if (task.IsCompleted)
                {
                    DataSnapshot snapshot = task.Result;
                    if (snapshot.Exists)
                    {
                        if (snapshot.Value is string rs)
                            result = rs;
                        if (first)
                        {
                            LocalTurnSystem.Instance.isPlayerFirstPlayer = result.ToString().Equals(LocalTurnSystem.Instance.PlayerID.Value.ToString());
                              BindPlayersReady(result.Equals(LocalTurnSystem.Instance.PlayerID.Value));
                        }

                    }
                }
            });
        return result;
    }
    private async void SetPlayersID(string[] playerIDs, string MyPlayerID)
    {
        LocalTurnSystem.Instance.PlayerID.Value = MyPlayerID;
        LocalTurnSystem.Instance.OtherPlayerID = playerIDs.First(p => p != MyPlayerID);
        LocalTurnSystem.FirstPlayerStr = await GetPlayersID(true);
        LocalTurnSystem.SecondPlayerStr = await GetPlayersID(false);
    }

    private void BindPlayersReady(bool playerIsFirst)
    {
        if (playerIsFirst)
        {
            LocalTurnSystem.PlayerReady.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player1_Ready"));
            LocalTurnSystem.OtherPlayerReady.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player2_Ready"));
        }
        else
        {
            LocalTurnSystem.PlayerReady.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player2_Ready"));
            LocalTurnSystem.OtherPlayerReady.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player1_Ready"));
        }
    }

    /*  public async void RegisterAsSecondPlayer(string PlayerID)
      {
          LocalTurnSystem.PlayerID.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player2"));
          LocalTurnSystem.PlayerID.Value = PlayerID;
          LocalTurnSystem.OtherPlayerID.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player1"));


          LocalTurnSystem.TurnCounter.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("TurnCounter"));
          LocalTurnSystem.PlayerReady.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player2_Ready"));
         // LocalTurnSystem.PlayerReady.Value = false;
          LocalTurnSystem.OtherPlayerReady.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("Player1_Ready"));
         *//* LocalTurnSystem.OtherPlayerID.Value =
              await LocalTurnSystem.OtherPlayerID.TaskGetValue(DataBaseReference.Child("TurnManager")
                  .Child("Player1"));*/

    /*var firstplayer = await GetFirstPlayerID();
    if (firstplayer == "-1")
    {
        DataBaseReference.Child("TurnManager").Child("FirstPlayer").SetValueAsync(LocalTurnSystem.PlayerID.Value);
    }*//*
   // LocalTurnSystem.FirstPlayer.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("FirstPlayer"));

    LocalTurnSystem.TurnStartedNotificationTrigger.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("RoundCounter"));

    LocalTurnSystem.GameStartedNotificationTrigger.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("GameStartTrigger"));
    LocalTurnSystem.CurrentPlayerID.BindToFirebaseValue(DataBaseReference.Child(TurnDataCollectionName).Child("CurrentPlayer"));
}
*/

    public bool initialized = false;
    [Button]
    public void Initialize(DatabaseReference dbRef, string[] playerIDs, string MyPlayerID)
    {
        if (!initialized)
        {
            //need to be replaced
            //DataBaseAPI.DatabaseAPI.InitializeDatabase();
            DataBaseReference = dbRef;
            BindPlayerToTurnManagement("p");
            SetPlayersID(playerIDs, MyPlayerID);
            /*var b = (MyPlayerID == playerIDs[0]);
            if (b)
            {
               InitlizeFirebaseTurnDatabase(MyPlayerID);
            }
            else
            {
                RegisterAsSecondPlayer(MyPlayerID);
            }*/
            initialized = true;
        }
    }



    private void Start()
    {

    }

    private void OnDisable()
    {
        //DataBaseReference?.Child(TurnDataCollectionName).RemoveValueAsync();
    }


    public void BindVariables()
    {

    }


}
