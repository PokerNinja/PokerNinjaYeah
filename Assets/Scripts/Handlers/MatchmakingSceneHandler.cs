using System;
using System.Collections;
using System.Linq;
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
        [SerializeField] TextMeshProUGUI randomTip;

        private bool gameFound;
        private bool readyingUp;
        private string gameId;
        private bool isReady = false;

        private string tip1 = "The ninja's last turn gets you only 1 energy instead of 2. STRATEGIZE!";
        private string tip2 = "There's only 1 flip per round! USE IT WISELY!";
        private string tip3 = "You can always HOLD your finger on a card or button to read what it does.";
        private string tip4 = "Use fire cards on frozen cards to unfreeze them!";
        private string tip5 = "You can always see the hand ranks from the scroll-menu on the right.";
        private string tip6 = "You can't hold more than 2 ninja cards at a time - the left one will be destroyed!";
        private string tip7 = "Remember you can express yourself by holding your finger on the ninja and choose the expression you want.";
        private string tip8 = "If you have 2 ninja-cards and you use DRAW - you need to choose which one you want to destroy.";
        private string tip9 = "Hold your finger on your cards- it will show you your current highest hand combination.";
        private string tip10 = "Tie results in no points.";
        private string tip11 = "MONSTER Ninja-cards can flip the game around! They cost 2 energy. ";
        private string[] tips;

        private void Start()
        {
            readyingUp = false;
            gameFound = false;
            searchingText.text = "Looking for opponent...";
            JoinQueue();
            tips =  new string[]{ tip1,tip2,tip3,tip4,tip5,tip6,tip7,tip8,tip9,tip10,tip11};
            StartCoroutine(DisplayRandomTip());
            StartCoroutine(StartBotGame());
        }

        private IEnumerator StartBotGame()
        {
            yield return new WaitForSeconds(10f);
            LeaveQueue();
            Constants.BOT_MODE = true;
            SceneManager.LoadScene("GameScene2");
        }

        private IEnumerator DisplayRandomTip()
        {
            int i = 0;
            float duration = 0.7f;
            var rnd = new System.Random();
            string[] shuffleTips = tips.OrderBy(a => Guid.NewGuid()).ToArray();
            while (!gameFound)
            {
                randomTip.text = shuffleTips[i++];
                StartCoroutine(AlphaFontAnim(randomTip, true, duration, null));
                yield return new WaitForSeconds(7f);
                StartCoroutine(AlphaFontAnim(randomTip, false, duration, null));
                yield return new WaitForSeconds(duration);
            }
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
        public IEnumerator AlphaFontAnim(TextMeshProUGUI txtMesh, bool fadeIn, float duration, Action OnFinish)
        {
            float r = txtMesh.color.r;
            float g = txtMesh.color.g;
            float b = txtMesh.color.b;
            float dissolveAmount = 1;
            float alphaTarget = 0;
            if (fadeIn)
            {
                dissolveAmount = 0;
                alphaTarget = 1;
            }
            if (txtMesh.color.a != alphaTarget)
            {
                while (dissolveAmount != alphaTarget)
                {

                    yield return new WaitForFixedUpdate();
                    if (fadeIn)
                    {
                        dissolveAmount += Time.deltaTime / duration;
                    }
                    else
                    {
                        dissolveAmount -= Time.deltaTime / duration;
                    }

                    txtMesh.color = new Color(r, g, b, Mathf.Lerp(0f, 1f, dissolveAmount));
                    if ((dissolveAmount >= alphaTarget && fadeIn) || (dissolveAmount <= alphaTarget && !fadeIn))
                    {
                        txtMesh.color = new Color(r, g, b, alphaTarget);
                        OnFinish?.Invoke();
                        break;
                    }
                }
            }
            else
            {
                OnFinish?.Invoke();
            }
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
