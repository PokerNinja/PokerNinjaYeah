using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject texasDialog;
    public GameObject texasTutorial;
    public GameObject instructions;

    public void StartPokerTutorial()
    {
        texasDialog.SetActive(false);
        texasTutorial.SetActive(true);
    }

    public void StartPnTutorial()
    {
        texasDialog.SetActive(false);
        BattleSystem.Instance.StartBotGame();
    }
}
