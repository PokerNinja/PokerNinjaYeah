using System;
using System.Collections;
using System.Linq;
using System.Threading.Tasks;
using APIs;
using Managers;
using Sirenix.OdinInspector;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Handlers
{
    public class MatchmakingSceneHandler : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI searchingText;
        [SerializeField] TextMeshProUGUI randomTip;
        [SerializeField] SpriteRenderer tip12SR;
        [SerializeField] SpriteRenderer tip13SP;

        private bool gameFound;
        private bool readyingUp;
        private string gameId;
        private bool isReady = false;

        //MOVE IT
        private readonly string tip1 = "The ninja's last turn gets you only 1 energy instead of 2. STRATEGIZE!";
        private readonly string tip2 = "There's only 1 flip per round! USE IT WISELY!";
        private readonly string tip3 = "You can always HOLD your finger on a card or button to read what it does.";
        private readonly string tip4 = "Use fire cards on frozen cards to unfreeze them!";
        private readonly string tip5 = "You can always see the hand ranks from the scroll-menu on the right.";
        private readonly string tip6 = "You can't hold more than 2 ninja cards at a time - the left one will be destroyed!";
        private readonly string tip7 = "Remember you can express yourself by holding your finger on the ninja and choose the expression you want.";
        private readonly string tip8 = "If you have 2 ninja-cards and you use DRAW - you need to choose which one you want to destroy.";
        private readonly string tip9 = "Hold your finger on your cards- it will show you your current highest hand combination.";
        private readonly string tip10 = "Tie results in no points.";
        private readonly string tip11 = "MONSTER Ninja-cards can flip the game around! They cost 2 energy.";
        private readonly string tip12 = "Ninja-Cards have an easy indications symbols. ▼ -Effects your hand. ▲ - Effects opponent’s hand.  ▬  - Effects the board.";
        private readonly string tip13 = "Ninja-cards are divided to 3 Elements- Fire, ice, Wind. Fire burns. Ice freezes. Wind swaps.";
        private string[] tips;

       // public GameObject dotPrefab;
        public GameObject dot1;
        public GameObject dot2;
        public GameObject dot3;

        private void Start()
        {

            readyingUp = false;
            gameFound = false;
            searchingText.text = "Looking for opponent";
            JoinQueue(Constants.MatchId);
            tips = new string[] { tip1, tip2, tip3, tip4, tip5, tip6, tip7, tip8, tip9, tip10, tip11, tip12, tip13 };
            StartCoroutine(DisplayRandomTip());
            StartCoroutine(StartBotGame());
            DotsEffect();
        }

        private IEnumerator StartBotGame()
        {
            yield return new WaitForSeconds(20f);
            if (!gameFound)
            {
                LeaveQueue();
                Constants.BOT_MODE = true;
                SceneManager.LoadScene("GameScene2");
            }
        }

        private IEnumerator DisplayRandomTip()
        {
            int i = 0;
            float duration = 0.7f;
            var rnd = new System.Random();
            string[] shuffleTips = tips.OrderBy(a => Guid.NewGuid()).ToArray();
            while (!gameFound)
            {

                randomTip.text = shuffleTips[i];

                if (shuffleTips[i].Equals(tip12) || shuffleTips[i].Equals(tip13))
                {
                    SpriteRenderer tip = tip12SR;
                    if (shuffleTips[i].Equals(tip13))
                    {
                        tip = tip13SP;
                    }
                    StartCoroutine(AlphaAnimation(tip, true, duration, null));
                    yield return new WaitForSeconds(5.5f);
                    StartCoroutine(AlphaAnimation(tip, false, duration, null));
                }
                else
                {
                    StartCoroutine(AlphaFontAnim(randomTip, true, duration, null));
                    yield return new WaitForSeconds(5.5f);
                    StartCoroutine(AlphaFontAnim(randomTip, false, duration, null));
                }
                i++;
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
        private void JoinQueue(string matchId)
        {
            if (MainManager.Instance == null)
            {
                Debug.LogError("instance null :(");
            }
            MainManager.Instance.matchmakingManager.JoinQueue( MainManager.Instance.currentLocalPlayerId, matchId, gameId =>
            {
                this.gameId = gameId;
                gameFound = true;
            },
                Debug.Log);
        }

        private void Update()
        {
            if (gameFound && !readyingUp)
            {
                readyingUp = true;
                GameFound();
            }

        }


        private bool cancelGame = false;
        private void GameFound()
        {
            MainManager.Instance.gameManager.GetCurrentGameInfo(gameId, MainManager.Instance.currentLocalPlayerId,
                gameInfo =>
                {
                    Debug.Log("Game found. Ready-up!");
                    StartCoroutine(CheckIfGameForReal());
                    gameFound = true;
                    StartCoroutine(AutoReady());
                    MainManager.Instance.gameManager.ListenForAllPlayersReady(gameInfo.playersIds,
                        playerId =>
                        {
                            Debug.Log(playerId + " is ready!");
                            cancelGame = playerId.ToString() == MainManager.Instance.gameManager.currentGameInfo.localPlayerId.ToString();
                        }
                        , () =>
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
            DisableDots();
        }

        private void DisableDots()
        {
            dot1.SetActive(false);
            dot2.SetActive(false);
            dot3.SetActive(false);
        }

        private IEnumerator CheckIfGameForReal()
        {
            yield return new WaitForSeconds(10f);
            if (cancelGame)
            {
                StartCoroutine(LeaveSearchRoutine());
            }
        }

        private IEnumerator LeaveSearchRoutine()
        {
            MainManager.Instance.gameManager.StopListeningForAllPlayersReady();
            DeleteGame(() => Debug.Log("YEAH DELETEEE"), Debug.LogError);
            LeaveQueue();
            yield return new WaitForSeconds(1.5f);
            SceneManager.LoadScene("GameMenuScene");
        }

        public void DeleteGame(Action callback, Action<AggregateException> fallback)
        {
            var dbrefOfGameRoom = DataBaseAPI.DatabaseAPI.GetReferenceFromPath($"games/{gameId}");
            dbrefOfGameRoom.RemoveValueAsync().ContinueWith(task =>
            {
                if (task.IsCanceled)
                {
                    Debug.LogError("PostJSON was canceled.");
                    fallback(task.Exception);
                    return;
                }

                if (task.IsFaulted)
                {
                    Debug.LogError("PostJSON encountered an error: " + task.Exception);
                    fallback(task.Exception);
                    return;
                }

                callback();
            }
                );

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

        public IEnumerator AlphaAnimation(SpriteRenderer spriteRenderer, bool fadeIn, float duration, Action OnFinish)
        {
            float r = spriteRenderer.color.r;
            float g = spriteRenderer.color.g;
            float b = spriteRenderer.color.b;
            float startingAlpha = spriteRenderer.color.a;
            float dissolveAmount = 1;
            float alphaTarget = 0;
            if (fadeIn)
            {
                dissolveAmount = 0;
                alphaTarget = 1;
            }
            if (startingAlpha == alphaTarget)
            {
                Debug.LogError("SAME VaLUE");
                OnFinish?.Invoke();
            }
            else
            {

                spriteRenderer.color = new Color(r, g, b, dissolveAmount);
                //FIXIT
                while (dissolveAmount != alphaTarget)
                {
                    //yield return new WaitForFixedUpdate();
                    yield return null;
                    if (fadeIn)
                    {
                        dissolveAmount += Time.deltaTime / duration;
                    }
                    else
                    {
                        dissolveAmount -= Time.deltaTime / duration;
                    }

                    spriteRenderer.color = new Color(r, g, b, Mathf.Lerp(0f, 1f, dissolveAmount));
                    if (dissolveAmount >= 1 || dissolveAmount <= 0)
                    {
                        // Debug.LogError("NEEDTOFIX? " + dissolveAmount);
                        // Debug.LogError("NEEDTOFIX " + spriteRenderer.gameObject.name);

                        spriteRenderer.color = new Color(r, g, b, Mathf.Lerp(0f, 1f, alphaTarget));
                        OnFinish?.Invoke();
                        break;
                    }
                }
            }
        }

        private IEnumerator AutoReady()
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

        [Button]
        private  async void DotsEffect()
        {
            dot1.SetActive(true);
            await Task.Delay(320);
            dot2.SetActive(true);
            await Task.Delay(320);
            dot3.SetActive(true);
            /* await Task.Delay(390);
            Instantiate(dotPrefab,new Vector3(1.1f, 1.23f, 1f), Quaternion.identity);*/
        }
        private void OnGUI()
        {
            if (Event.current.type == EventType.KeyDown)
            {
                StartCoroutine(LeaveSearchRoutine());
            }
        }

        public void Ready() =>
            MainManager.Instance.gameManager.SetLocalPlayerReady(true, () => Debug.Log("You are now ready!"), Debug.Log);
    }
}
