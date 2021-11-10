
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using static DataBaseAPI;
using System.Text.RegularExpressions;
using System;

public class GameMenuHandler : MonoBehaviour
{
    public TMP_InputField playerName;
    public TextMeshProUGUI infoText;
    public TextMeshProUGUI appVersion;
    public GameObject wrongTypeText;
    public GameObject tutorialImage;
    private bool tutorialVisible = false;

    public string RegexPattern;
    public string textToCheck;

    public void Awake()
    {
        Screen.sleepTimeout = SleepTimeout.NeverSleep;
    }
    void Start()
    {
        appVersion.text = "version: " + Application.version;
        DatabaseAPI.InitializeDatabase();

        playerName.text = LoadPlayerNickName();

        if (LoadIsAutoPlay().Equals("True"))
        {
            PlayerPrefs.SetString("PlayAgain", "false");

            //ChangeMyName(Random.Range(1, 45).ToString());
            StartMultiplayer();
        }
    }

    public void StartMultiplayer()
    {
        if (CheckIfStringOnlyLettersAndDigits(playerName.text.ToString()))
        {
            SavePlayerNickName(playerName.text.ToString());
          //  DatabaseAPI.InitializeDatabase();
            MainManager.Instance.currentLocalPlayerId = playerName.text + UnityEngine.Random.Range(1, 999).ToString();
            SceneManager.LoadScene("MatchMakingScene");
        }
        else
        {
            wrongTypeText.SetActive(true);
        }
    }

    private void SavePlayerNickName(string playerNickName)
    {
        PlayerPrefs.SetString("player_name", playerNickName);
    }
    private string LoadPlayerNickName()
    {
         return PlayerPrefs.GetString("player_name", "Ninja");
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
}

