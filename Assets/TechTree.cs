using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TechTree : MonoBehaviour
{

    public Image[] optionsSprite;
    public bool isDragon;
    public CanvasGroup treeCanvas;

    internal void SetSelection(int option)
    {
        optionsSprite[option].color = new Color(0.5f, 0.5f, 0.5f);
    }

    internal void EnableWheel(Vector2 position)
    {
        gameObject.SetActive(true);

    }

    public IEnumerator EnableEsWheel()
    {
        ResetOptionColor();
        gameObject.SetActive(true);
        yield return new WaitForSeconds(1f);
        StartCoroutine(AnimationManager.Instance.FadeCanvasGroup(treeCanvas, true, 0.6f));
    }

    private void ResetOptionColor()
    {
        optionsSprite[0].color = new Color(1f, 1f, 1f);
        optionsSprite[1].color = new Color(1f, 1f, 1f);
        optionsSprite[2].color = new Color(1f, 1f, 1f);
    }

    internal void DisableWheel()
    {
        StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(treeCanvas, false, 0.6f, () => gameObject.SetActive(false)));
    }

    public void OnSelected(int option)
    {
        SetSelection(option);
        BattleSystem.Instance.OnWheelSelected(isDragon, option);
    }

   
}
