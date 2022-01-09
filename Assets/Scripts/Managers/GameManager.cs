using System;
using System.Collections.Generic;
using System.Linq;
using APIs;
using Firebase.Database;
using Serializables;
using UnityEngine;
using static DataBaseAPI;

namespace Managers
{
    public class GameManager : MonoBehaviour
    {
        public GameInfo currentGameInfo;
        private string localPlayerSTPorFTP;
        private Dictionary<string, bool> readyPlayers;
        private KeyValuePair<DatabaseReference, EventHandler<ChildChangedEventArgs>> readyListener;
        private KeyValuePair<DatabaseReference, EventHandler<ChildChangedEventArgs>> readyListenerInGame;
        private KeyValuePair<DatabaseReference, EventHandler<ValueChangedEventArgs>> localPlayerTurnListener;
        private KeyValuePair<DatabaseReference, EventHandler<ValueChangedEventArgs>> currentGameInfoListener;
        private KeyValuePair<DatabaseReference, EventHandler<ValueChangedEventArgs>> newDeckListener;
        private KeyValuePair<DatabaseReference, EventHandler<ValueChangedEventArgs>> currentPowerupListener;
        private KeyValuePair<DatabaseReference, EventHandler<ValueChangedEventArgs>> currentEmojiListener;

        public void GetCurrentGameInfo(string gameId, string localPlayerId, Action<GameInfo> callback,
            Action<AggregateException> fallback)
        {
            currentGameInfoListener =
                DatabaseAPI.ListenForValueChanged($"games/{gameId}/gameInfo", args =>
                {
                    if (!args.Snapshot.Exists) return;

                    var gameInfo =
                        StringSerializationAPI.Deserialize(typeof(GameInfo), args.Snapshot.GetRawJsonValue()) as
                            GameInfo;
                    currentGameInfo = gameInfo;
                    currentGameInfo.localPlayerId = localPlayerId;
                    currentGameInfo.EnemyId = GetEnemyId();
                    DatabaseAPI.StopListeningForValueChanged(currentGameInfoListener);
                    callback(currentGameInfo);
                },
                fallback);
        }

        private string GetEnemyId()
        {
            if (currentGameInfo.playersIds[0].Equals(currentGameInfo.localPlayerId))
            {
                return currentGameInfo.playersIds[1];
            }
            else
            {
                return currentGameInfo.playersIds[0];
            }
        }

        public void SetLocalPlayerReady(bool ready, Action callback, Action<AggregateException> fallback)
        {
            if (!currentGameInfo.gameId.Equals(""))
            {
                DatabaseAPI.PostObject($"games/{currentGameInfo.gameId}/playersReady/{currentGameInfo.localPlayerId}", ready,
                    callback,
                    fallback);
            }
        }


        public void ListenForAllPlayersReady(IEnumerable<string> playersId, Action<string> onNewPlayerReady,
            Action onAllPlayersReady,
            Action<AggregateException> fallback)
        {
            readyPlayers = playersId.ToDictionary(playerId => playerId, playerId => false);
            readyListener = DatabaseAPI.ListenForChildAdded($"games/{currentGameInfo.gameId}/playersReady/", args =>
            {
                readyPlayers[args.Snapshot.Key] = true;
                onNewPlayerReady(args.Snapshot.Key);
                if (!readyPlayers.All(readyPlayer => readyPlayer.Value)) return;
                StopListeningForAllPlayersReady();
                Debug.LogError("PlayersReady?");
                onAllPlayersReady();
            }, fallback);
        }

        public void ListenForAllPlayersReadyInPlay(IEnumerable<string> playersId, Action<string> onNewPlayerReady,
            Action onAllPlayersReady,
            Action<AggregateException> fallback)
        {
            readyPlayers = playersId.ToDictionary(playerId => playerId, playerId => false);
            readyListenerInGame = DatabaseAPI.ListenForChildChanged($"games/{currentGameInfo.gameId}/playersReady/", args =>
            {
                readyPlayers[args.Snapshot.Key] = true;
                onNewPlayerReady(args.Snapshot.Key);

                if (readyPlayers.All(readyPlayer => readyPlayer.Value))
                {
                    Debug.LogWarning("PlayersReady!!!");

                    onAllPlayersReady();
                }
                else
                {
                    Debug.LogWarning("PlayersNotReady");
                }

                Debug.LogWarning("PlayersReady?");
            }, fallback);
        }


        /// <summary>
        /// Sends an action log to the firebase instance
        /// </summary>
        /// <param name="ActionDescription"> the action's description </param>
        /// <param name="Parameters"> optional value </param>
        /// <param name="callback"> required callback action </param>
        /// <param name="fallback"> required fallback action </param>
        internal void AddGameActionLog(ActionEnum action, string parameters, Action callback, Action<AggregateException> fallback)
        {
            DatabaseAPI.PushJsonAsChild($"games/{currentGameInfo.gameId}/log/", currentGameInfo.localPlayerId,
                currentGameInfo.localPlayerId + "= " + ConvertEnumActionToString(action) + ": " + parameters,
                         callback,
                         fallback);
        }

        public enum ActionEnum
        {
            EndTurn,
            PuUse,
            ReplacePu,
            SetReady,
            GenerateDeck,
        }
        private string ConvertEnumActionToString(ActionEnum action)
        {
            switch (action)
            {
                case ActionEnum.EndTurn:
                    return "EndTurn";
                case ActionEnum.PuUse:
                    return "PuUse";
                case ActionEnum.ReplacePu:
                    return "PuReplace";
                case ActionEnum.SetReady:
                    return "SetReady";
                case ActionEnum.GenerateDeck:
                    return "GenerateDeck";
            }
            return "ERROR";
        }


        internal void ListenForPowerupUse(Action<PowerUpInfo> callbackPuUse,
            Action<PowerUpInfo> callbackPuReplace,
            Action<AggregateException> fallback)
        {
            currentPowerupListener =
                           DatabaseAPI.ListenForValueChanged($"games/" + currentGameInfo.gameId + "/gameInfo/powerup", args =>
                            {
                                if (!args.Snapshot.Exists) return;

                                var powerup =
                                    StringSerializationAPI.Deserialize(typeof(PowerUpInfo), args.Snapshot.GetRawJsonValue()) as
                                        PowerUpInfo;
                                currentGameInfo.powerup = powerup;
                                if (powerup.timeStamp > 0)
                                {
                                    if (!powerup.powerupName.Equals("replace"))
                                    {
                                        callbackPuUse(currentGameInfo.powerup);
                                    }
                                    else
                                    {
                                        callbackPuReplace(currentGameInfo.powerup);
                                    }
                                }
                            },
                           fallback);
        }
        internal void ListenForEmoji(string currentPlayerId,Action<int> callbackEmoji,
            Action<AggregateException> fallback)
        {
            currentEmojiListener =
                           DatabaseAPI.ListenForValueChanged($"games/" + currentGameInfo.gameId + "/emoji", args =>
                            {
                                if (!args.Snapshot.Exists) return;

                                var emoji =
                                    StringSerializationAPI.Deserialize(typeof(EmojiInfo), args.Snapshot.GetRawJsonValue()) as
                                        EmojiInfo;
                               // currentGameInfo.powerup = powerup;
                                if (!emoji.playerId.Equals(currentPlayerId))
                                {
                                    callbackEmoji(emoji.emojiId);
                                }
                            },
                           fallback);
        }
        public void ListenForUpdate(Action callback,
       Action<AggregateException> fallback)
        {
            currentEmojiListener =
                DatabaseAPI.ListenForValueChanged("version", args =>
                {
                    if (!args.Snapshot.Exists) return;

                    string version = args.Snapshot.GetRawJsonValue() as
                            string;
                    callback();
                    Debug.LogWarning("ty " + version);
                },
                fallback);
        }
        public void StopListeningForAllPlayersReady() => DatabaseAPI.StopListeningForChildAdded(readyListener);
        public void StopListeningForAllPlayersReadyInGame() => DatabaseAPI.StopListeningForChildChanged(readyListenerInGame);


        public void ListenForLocalPlayerTurn(Action onLocalPlayerTurn, Action onEndRound, Action<AggregateException> fallback)
        {
            localPlayerTurnListener =
                DatabaseAPI.ListenForValueChanged($"games/{currentGameInfo.gameId}/turn", args =>
                {
                    var turn =
                        StringSerializationAPI.Deserialize(typeof(string), args.Snapshot.GetRawJsonValue()) as string;
                    if (turn.Equals(currentGameInfo.localPlayerId)) onLocalPlayerTurn();
                    if (turn.Equals("End" + currentGameInfo.localPlayerId)) onEndRound();
                    if (turn == "End") Debug.LogWarning("SetEndGetEnd");
                }, fallback);
        }


        public void SetTurnToOtherPlayer(string dataToSend, string currentPlayerId, Action callback, Action<AggregateException> fallback)
        {

            var otherPlayerId = currentGameInfo.playersIds.First(p => p != currentPlayerId);
            Debug.LogError("Turn " + otherPlayerId);
            if (dataToSend.Equals("End"))
            {
                otherPlayerId = "End" + otherPlayerId;
            }
            DatabaseAPI.PostObject(
                $"games/{currentGameInfo.gameId}/turn", otherPlayerId, callback, fallback);
        }
        public void ListenForNewDeck(Action callback,
            Action<AggregateException> fallback)
        {
            newDeckListener =
                DatabaseAPI.ListenForValueChanged($"games/{currentGameInfo.gameId}/gameInfo/cardDeck", args =>
                {
                    if (!args.Snapshot.Exists) return;

                    var deckDB =
                        StringSerializationAPI.Deserialize(typeof(string[]), args.Snapshot.GetRawJsonValue()) as
                            string[];
                    currentGameInfo.cardDeck = deckDB;
                    callback();
                },
                fallback);
        }

        internal void SetNewPowerupUseDB(PowerUpInfo newPowerUp, Action callback, Action<AggregateException> fallback)
        {

            DatabaseAPI.PostObject($"games/{currentGameInfo.gameId}/gameInfo/powerup", newPowerUp,
                   callback
                   ,fallback);
        }


        internal void UpdateWinnerDB(string EnemyId, Action callback, Action<AggregateException> fallback)
        {
            DatabaseAPI.PostObject($"games/{currentGameInfo.gameId}/winner/", currentGameInfo.localPlayerId,
                         callback,
                         fallback);
        }

        internal void UpdateEmojiDB(EmojiInfo emojiInfo, Action callback, Action<AggregateException> fallback)
        {
            DatabaseAPI.PostObject($"games/{currentGameInfo.gameId}/emoji/", emojiInfo,
                                    callback,
                                    fallback);
        }
    }
}
