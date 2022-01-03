using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;

public class TutorialUi : MonoBehaviour
{
    public GameObject tutorialMaskRing;
    public GameObject tutorialMaskRect;
    public GameObject tutorialMaskCards;
    public SpriteRenderer darkScreenRenderer;
    public SpriteRenderer btnTutorial;
    public TextMeshProUGUI tutorialText;
    public int lastObjectToFocus;
    public SpriteRenderer coinsFocus;
    public SpriteRenderer energyCostTuto;

    public async void ContinueWithTutorial()
    {
        bool enable = false;
        bool endByBtn = false;
        int maskShape = 0;
        int objectToFocus = 0;
        switch (lastObjectToFocus)
        {
            case 0://after Start
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.coins.GetHashCode();
                maskShape = 1;
                endByBtn = true;
                break;
            case 1:// after coins
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.energy.GetHashCode();
                maskShape = 1;
                endByBtn = true;
                break;
            case 2:// after energy
                BattleSystem.Instance.DealPu(true, () =>
                {
                    BattleSystem.Instance.FocusOnObjectWithText(true, 2, Constants.TutorialObjectEnum.pu.GetHashCode(), false);
                });
                enable = false;
                objectToFocus = Constants.TutorialObjectEnum.startGame.GetHashCode();
                maskShape = 1;
                endByBtn = false;
                break;
            case 3:// after puInfo
                ScaleDownMask(2);
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.puCost.GetHashCode();
                maskShape = 1;
                endByBtn = true;
                break;
            case 4:// after puCost
                ScaleDownMask(1);
                await Task.Delay(1500);
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.pu2.GetHashCode();
                maskShape = 2;
                endByBtn = false;
                MoveCardsMask(true,null);
                break;
            case 7:// after puCost
                MoveCardsMask(false,null);
               // await Task.Delay(1500);
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.endTurn.GetHashCode();
                maskShape = 1;
                endByBtn = false;
                break;
        }

        BattleSystem.Instance.FocusOnObjectWithText(enable, maskShape, objectToFocus, endByBtn);
    }

    public void MoveCardsMask(bool up , Action OnEndMove)
    {
        Vector3 targetPos = new Vector3(0, -11f, 10);
        Action DeactivateMask = ()=>tutorialMaskCards.SetActive(false);
        if (up)
        {
            tutorialMaskCards.SetActive(true);
            targetPos = new Vector3(0, -3.71f, 10);
            DeactivateMask = null;
        }
            StartCoroutine(AnimationManager.Instance.SmoothMove(tutorialMaskCards.transform,
                targetPos,tutorialMaskCards.transform.localScale, Values.Instance.tutoObjsFadeDuration,null,null, DeactivateMask, OnEndMove));
    }

    public void ScaleDownMask(int maskShape)
    {
        GameObject mask = tutorialMaskRing;
        float targetMaskScale= 0.001f;
        if(maskShape == 2)
        {
            targetMaskScale= 0.0033333f;
            mask = tutorialMaskRect;
        }
        StartCoroutine(AnimationManager.Instance.ScaleObject(targetMaskScale, Values.Instance.tutoObjsFadeDuration, mask.transform, null, null));
    }

    private void Start()
    {
        darkScreenRenderer.color = new Color(0, 0, 0, 0.8f);
        darkScreenRenderer.sortingOrder = 33;
    }
}
