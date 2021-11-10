using System;
using System.Collections;
using APIs;
using Managers;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Handlers
{
    public class MatchmakingSceneHandler : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI searchingText;

        private bool gameFound;
        private bool readyingUp;
        private string gameId;
        private bool isReady = false;

        private void Start()
        {
            readyingUp = false;
            gameFound = false;
            searchingText.text = "Looking for opponent...";
            JoinQueue();
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                LeaveQueue();
            }
        }
        private void JoinQueue()
        {
            if (MainManager.Instance == null)
            {
                Debug.LogError("instance null :(");
            }
            MainManager.Instance.matchmakingManager.JoinQueue(MainManager.Instance.currentLocalPlayerId, gameId =>
            {
                this.gameId = gameId;
                gameFound = true;
            },
                Debug.Log);
        }

        private void Update()
        {
            if(gameFound && !readyingUp)
            {
            readyingUp = true;
            GameFound();
            }

        }

        private void GameFound()
        {
            MainManager.Instance.gameManager.GetCurrentGameInfo(gameId, MainManager.Instance.currentLocalPlayerId,
                gameInfo =>
                {
                    Debug.Log("Game found. Ready-up!");
                    gameFound = true;
                    StartCoroutine(AutoReady());
                    MainManager.Instance.gameManager.ListenForAllPlayersReady(gameInfo.playersIds,
                        playerId => Debug.Log(playerId + " is ready!"), () =>
                        {
                            var dbrefOfGameRoom = DataBaseAPI.DatabaseAPI.GetReferenceFromPath($"games/{gameId}");
                            
                            Debug.Log("All players are ready!");
                            // SingleOrMultiplayer.CrossSceneInformation = "MP";
                            //NO INIT MP

                          //  LocalTurnSystem.Instance.Init(dbrefOfGameRoom, gameInfo.playersIds, MainManager.Instance.currentLocalPlayerId);
                            LocalTurnSystem.Instance.Init(dbrefOfGameRoom, gameInfo.playersIds, MainManager.Instance.currentLocalPlayerId);

                            // Initilize Turn Manager here! Destroy when leave
                            SceneManager.LoadScene("GameScene2");
                        }, Debug.Log);
                }, Debug.Log);
            searchingText.text = "Game found!";
        }

        private void InternetCheck()
        {
            Debug.Log("internet check");
            InternetCheck internet = GetComponent<InternetCheck>();
            if (internet.isConnect)
            {
                Ready(); //FROM WHERE? WHY ON CLICK LOADSCENE // NO LISTENER BEFORE READY
            }
        }

        public void LeaveQueue()
        {
            
            if (gameFound) MainManager.Instance.gameManager.StopListeningForAllPlayersReady();
            else
                MainManager.Instance.matchmakingManager.LeaveQueue(MainManager.Instance.currentLocalPlayerId,
                    () => Debug.Log("Left queue successfully"), Debug.Log);
            SceneManager.LoadScene("GameMenuScene");
        }

        IEnumerator AutoReady()
        {
            if (MainManager.Instance.gameManager.currentGameInfo.gameId.Length > 0)
            {
                isReady = true;
                Ready();
            }
            yield return new WaitForSeconds(1f);
            if (!isReady)
            { 
            StartCoroutine(AutoReady());
            }
        }

        public void Ready() =>
            MainManager.Instance.gameManager.SetLocalPlayerReady(true,() => Debug.Log("You are now ready!"), Debug.Log);
    }

}
