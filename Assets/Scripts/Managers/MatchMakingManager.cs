using APIs;
using Firebase.Database;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static DataBaseAPI;

public class MatchMakingManager : MonoBehaviour
{

        private KeyValuePair<DatabaseReference, EventHandler<ValueChangedEventArgs>> queueListener;

    public void JoinQueue(string playerId, Action<string> onGameFound, Action<AggregateException> fallback)
    {
        Debug.Log("MMM JOINF");
        string gameMode = "placeholder";
       /* string gameMode = "c_game";
        if (Constants.HP_GAME)
        {
            gameMode = "h_game";
        }*/
        DatabaseAPI.PostObject($"matchmaking/{playerId}", gameMode,
            () => queueListener = DatabaseAPI.ListenForValueChanged($"matchmaking/{playerId}",
                args =>
                {
                    Debug.Log("MMM JOINQ");
                    var gameId =
                        StringSerializationAPI.Deserialize(typeof(string), args.Snapshot.GetRawJsonValue()) as
                            string;
                    if (gameId == gameMode) return;
                    LeaveQueue(playerId, () => onGameFound(
                        gameId), fallback);
                }, fallback), fallback);
    }
        public void LeaveQueue(string playerId, Action callback, Action<AggregateException> fallback)
        {
        Debug.Log("MMM LeaveQ");

        DatabaseAPI.StopListeningForValueChanged(queueListener);
            DatabaseAPI.PostJSON($"matchmaking/{playerId}", "null", callback, fallback);
        }
    }

