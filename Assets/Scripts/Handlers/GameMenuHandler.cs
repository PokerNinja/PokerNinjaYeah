
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using static DataBaseAPI;
using System.Text.RegularExpressions;
using System;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Firebase.Database;
using System.Collections;
using UnityEngine.EventSystems;

public class GameMenuHandler : MonoBehaviour
{
    public TMP_InputField playerName;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI appVersion;
    public TextMeshProUGUI dialogUpdateText;
    public GameObject wrongTypeText;
    public GameObject tutorialImage;
    public GameObject updateCanvas;
    public GameObject rankImageParent;
    public GameObject canvasExitDialog;
    public string linkToDrive;

    [GUIColor(0.3f, 0.8f, 0.8f)]
    public bool TEST_MODE;
    [GUIColor(0.1f, 0.3f, 0.8f)]
    public bool HP_GAME;

    public Transform[] elementChoiceTransforms;
    public SpriteRenderer elementChoicerRenderer;
    private string elementChoiseString;
    private int currentElementIndex;
    private bool clickBlocker;

    private bool tutorialVisible = false;
    public AudioSource auidioSource;

    public void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    void Start()
    {
        Constants.HP_GAME = HP_GAME;
        Constants.BOT_MODE = false;

        appVersion.text = "version: " + Application.version;
        DatabaseAPI.InitializeDatabase();

        playerName.text = LoadPlayerNickName();
        currentElementIndex = -1;
        OnElementPick(LoadPlayerElement());
        if (LoadIsAutoPlay().Equals("True"))
        {
            PlayerPrefs.SetString("PlayAgain", "false");

            //ChangeMyName(Random.Range(1, 45).ToString());
            StartMultiplayer();
        }

        ListenForUpdate(targetVersion =>
        {
            if (Application.version.ToString().Equals(targetVersion))
            {
                // Debug.LogError("You are updated");
            }
            else
            {
                ShowUpdateDialog();
            }
        }, Debug.Log);

        ListenForTextUpdate(updateTxt => linkToDrive = updateTxt
        , Debug.Log); ;
    }

    private void ShowUpdateDialog()
    {
        updateCanvas.SetActive(true);
    }
    public void SendUserToDriveApk()
    {
        // dialogUpdateText.text = linkToDrive;
        //dialogUpdateText.text ="< link ="+ linkToDrive+"/> </ link >";
        string newUrl = linkToDrive.Trim('"');
        Application.OpenURL(newUrl);


    }
    public void SendUserToStore()
    {
        Application.OpenURL("https://play.google.com/store/apps/details?id=poker.ninja.oabk");
    }
    private void OnGUI()
    {
        if (Event.current.type == EventType.KeyDown)
        {

            if (Input.GetKeyDown("escape"))
            {
                if (!isRankOpen && !sliding)
                {
                    EnableExitDialog(true);
                }
                else if (isRankOpen)
                {
                    SlideImgRank();
                }
            }
        }
    }
    public void EnableExitDialog(bool enable)
    {

        canvasExitDialog.SetActive(enable);
    }
    public void BtnExit()
    {
        canvasExitDialog.SetActive(true);
    }

    private KeyValuePair<DatabaseReference, EventHandler<ValueChangedEventArgs>> versionListener;

    public void ListenForUpdate(Action<string> callback,
        Action<AggregateException> fallback)
    {
        // Should update when return to main?
        versionListener =
            DatabaseAPI.ListenForValueChanged("version", args =>
            {
                if (!args.Snapshot.Exists) return;

                string version = args.Snapshot.GetRawJsonValue() as
                            string;
                callback(version);
                // Constants.updated = currentVersion.Equals(version);
                Debug.LogWarning("target " + version);
            },
            fallback);
    }
    public void ListenForTextUpdate(Action<string> callback,
        Action<AggregateException> fallback)
    {
        // Should update when return to main?
        versionListener =
            DatabaseAPI.ListenForValueChanged("update_msg", args =>
            {
                if (!args.Snapshot.Exists) return;

                string text = args.Snapshot.GetRawJsonValue() as
                            string;
                callback(text);
                // Constants.updated = currentVersion.Equals(version);
                Debug.LogWarning("target " + text);
            },
            fallback);
    }

    [Button]
    public static void yalla()
    {
        DatabaseAPI.CheckIfVersionUpdated(Application.version.ToString(),
          () => Debug.Log("Version Updated"), () => Debug.LogError("Update is required"));
    }

    public void StartMultiplayer()
    {
        Constants.MatchId = "placeholder";
        if (TEST_MODE)
        {
            SceneManager.LoadScene("GameScene2");
        }
        else if (CheckIfStringOnlyLettersAndDigits(playerName.text.ToString()))
        {
            SavePlayerNickName(playerName.text.ToString());
            //  DatabaseAPI.InitializeDatabase();
            MainManager.Instance.currentLocalPlayerId = elementChoiseString + playerName.text + UnityEngine.Random.Range(1, 999).ToString();
            SceneManager.LoadScene("MatchMakingScene");
        }
        else
        {
            wrongTypeText.SetActive(true);
        }
    }


    public void StartTutorial()
    {
        SceneManager.LoadScene("TutorialScene");
    }
    [Button]
    private void GetGameLogByNicnName(string nickname)
    {
        DatabaseAPI.GetGameLogByNicnName(nickname);
    }
    private void SavePlayerNickName(string playerNickName)
    {
        PlayerPrefs.SetString("player_name", playerNickName);
    }
    private string LoadPlayerNickName()
    {
        return PlayerPrefs.GetString("player_name", "Ninja");
    }
    private void SavePlayerElement(int element)
    {
        PlayerPrefs.SetInt("player_element", element);
    }
    private void SavePlayerElementString(string element)
    {
        PlayerPrefs.SetString("player_element_string", element);
    }
    private int LoadPlayerElement()
    {
        return PlayerPrefs.GetInt("player_element", 0);
    }

    public void StartPractice()
    {
        // SingleOrMultiplayer.CrossSceneInformation = "SP";
        SceneManager.LoadScene("GameScene");
    }
    public void ChangeMyName(string extra)
    {
        playerName.text += extra;
    }
    private string LoadIsAutoPlay()
    {
        return PlayerPrefs.GetString("PlayAgain", "false");
    }

    public void ToggleTutorialImage()
    {
        if (!tutorialVisible)
        {
            infoText.text = "X";
            tutorialImage.SetActive(true);
        }
        else
        {
            infoText.text = "i";
            tutorialImage.SetActive(false);
        }
        tutorialVisible = !tutorialVisible;
    }
    private bool CheckIfStringOnlyLettersAndDigits(string input)
    {
        if (string.IsNullOrEmpty(input))
        {
            return false;
        }
        return MatchRegex(input);
    }
    public bool MatchRegex(string t)
    {
        return Regex.Match(t, "^[A-Za-z][A-Za-z0-9]{1,13}$").Success;
    }

    [Button]
    public void Cal()
    {
        int counter = 0;
        int dounter = 0;
        for (int a = 0; a < 8; a++)
        {
            for (int b = a + 1; b < 8; b++)
            {

                Debug.LogWarning("a:" + a + " b:" + b);
                if (a == b)
                {
                    dounter++;
                }
                else
                {
                    counter++;
                }

            }
        }
        Debug.LogError("count " + counter);
        Debug.LogError("dount " + dounter);
    }
    [Button]
    public void Cal1()
    {
        int counter = 0;
        int dounter = 0;
        for (int a = 0; a < 8; a++)
        {
            for (int b = a + 1; b < 8; b++)
            {
                for (int c = b + 1; c < 8; c++)
                {
                    Debug.LogWarning("a:" + a + " b:" + b + " c:" + c);
                    if (a == b || a == c || b == c)
                    {
                        dounter++;
                    }
                    else
                    {
                        counter++;
                    }
                }
            }
        }
        Debug.LogError("count " + counter);
        Debug.LogError("dount " + dounter);
    }
    [Button]
    public void Cal2()
    {
        int counter = 0;
        int dounter = 0;
        for (int a = 0; a < 9; a++)
        {
            for (int b = a + 1; b < 9; b++)
            {
                for (int c = b + 1; c < 9; c++)
                {
                    for (int d = c + 1; d < 9; d++)
                    {
                        Debug.LogWarning("a:" + a + " b:" + b + " c:" + c + " d:" + d);
                        if (a == b || a == c || a == d || b == c || b == d || c == d)
                        {
                            dounter++;
                        }
                        else
                        {
                            counter++;
                        }
                    }
                }
            }
        }
        Debug.LogError("count " + counter);
        Debug.LogError("dount " + dounter);
    }

    bool sliding = false;
    public void SlideImgRank()
    {
        if (!sliding)
        {
            sliding = true;
            StartCoroutine(SmoothMoveRank(rankImageParent.transform, 1f, () => rankImageParent.SetActive(false)/*rankingImg.GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/GameScene/Buttons/ranking_empty", typeof(Sprite)) as Sprite*/,
                () => rankImageParent.SetActive(true)/*rankingImg.GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/GameScene/Buttons/ranking_full", typeof(Sprite)) as Sprite*/, () => sliding = false));
        }
    }
    bool isRankOpen = false;
    public IEnumerator SmoothMoveRank(Transform selector, float movementDuration, Action endActionEmptyScroll, Action endActionFullScroll, Action finishSliding)
    {
        float startTime = Time.time;
        float t;
        float targetPositionX;
        bool toFull;
        float targetFull = 2.82f;
        float targetEmpty = 14.72f;

        // SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.OpenDrawer, false);
        if (selector.localPosition.x <= targetFull)
        {
            toFull = false;
            targetPositionX = targetEmpty;
        }
        else
        {
            toFull = true;
            targetPositionX = targetFull;
        }
        if (toFull)
        {
            endActionFullScroll?.Invoke();
        }
        isRankOpen = toFull;
        while (selector.localPosition.x != targetPositionX)
        {
            t = (Time.time - startTime) / movementDuration;
            selector.position = new Vector3(Mathf.SmoothStep(selector.localPosition.x, targetPositionX, t / 2), selector.localPosition.y, 24f);

            if (selector.localPosition.x == targetPositionX)
            {
                finishSliding?.Invoke();
                if (!toFull)
                {
                    endActionEmptyScroll?.Invoke();
                }
            }
            yield return new WaitForFixedUpdate();
        }
        yield break;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    [Button]
    public void EmailUs()
    {
        //email Id to send the mail to
        string email = "pokerninjateam@gmail.com";
        //subject of the mail
        string subject = MyEscapeURL("FEEDBACK/SUGGESTION");
        //body of the mail which consists of Device Model and its Operating System
        string body = MyEscapeURL("Please Enter your message here\n\n\n\n" +
         "________" +
         "\n\nPlease Do Not Modify This\n\n" +
         "Model: " + SystemInfo.deviceModel + "\n\n" +
            "OS: " + SystemInfo.operatingSystem + "\n\n" +
         "________");
        //Open the Default Mail App
        Application.OpenURL("mailto:" + email + "?subject=" + subject + "&body=" + body);
    }

    string MyEscapeURL(string url)
    {
        return WWW.EscapeURL(url).Replace("+", "%20");
    }

    public void OnElementPick(int index)
    {
        if (currentElementIndex != index && !clickBlocker)
        {
            currentElementIndex = index;
            switch (index)
            {
                case 0:
                    elementChoiseString = "f";
                    break;
                case 1:
                    elementChoiseString = "i";
                    break;
                case 2:
                    elementChoiseString = "w";
                    break;
            }
            SavePlayerElement(index);
            SavePlayerElementString(elementChoiseString);
            UpdatePickedElement(index);
        }
    }

    private void UpdatePickedElement(int index)
    {
        clickBlocker = true;
        /*StartCoroutine(AlphaAnimation(elementChoicerRenderer, false, 0.25f, () =>
        {*/
        elementChoicerRenderer.color = new Color(1, 1, 1, 0);
        elementChoicerRenderer.transform.position = new Vector3(elementChoiceTransforms[index].position.x, elementChoiceTransforms[index].position.y, elementChoicerRenderer.transform.position.z);
        StartCoroutine(AlphaAnimation(elementChoicerRenderer, true, 0.17f, () => clickBlocker = false));
        auidioSource.Play();
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

}

