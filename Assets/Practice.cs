using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Practice : MonoBehaviour
{
    public Text btnTxt;
    public void FD()
    {
        btnTxt.text = "D     D";
        Debug.LogError("FD");

    }
    public void FU()
    {
        btnTxt.text = "U     U";
        Debug.LogError("FU");
        SceneManager.LoadScene("GameScene");

    }
}
