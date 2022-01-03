using Sirenix.OdinInspector;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialUi : MonoBehaviour
{
    public GameObject tutorialMaskRing;
    public GameObject tutorialMaskRect;
    public GameObject tutorialMaskCards;
    public SpriteRenderer darkScreenRenderer;
    public SpriteRenderer btnTutorialSprite;
    public Button btnTutorial;
    public TextMeshProUGUI tutorialText;
    public int lastObjectToFocus;
    public SpriteRenderer coinsFocus;
    public SpriteRenderer energyCostTuto;
    public SpriteRenderer visionBtn;
    public SpriteRenderer handRankMenu;
    public SpriteRenderer eyeRenderer;
    public Transform maskPositionForEmojis;

    public async void ContinueWithTutorial()
    {
        bool enable = false;
        bool endByBtn = false;
        int maskShape = 0;
        int objectToFocus = 0;
        btnTutorial.interactable = false; // maybe fade again
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
                objectToFocus = Constants.TutorialObjectEnum.board.GetHashCode();
                maskShape = 0;
                endByBtn = true;
                MoveCardsMaskPlayer(true,null);
                break;
            case 30:// after puCost
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.pu2.GetHashCode();
                maskShape = 2;
                endByBtn = false;
                break;
            case 7:// after puCost
                MoveCardsMaskPlayer(false,null);
               // await Task.Delay(1500);
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.endTurn.GetHashCode();
                maskShape = 0;
                endByBtn = false;
                break;
            case 9:// after puCost
                MoveCardsMaskPlayer(false,null);
               // await Task.Delay(1500);
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.rankMenu.GetHashCode();
                maskShape = 1;
                endByBtn = false;
                break;
            case 14:// after flip symbol
                // await Task.Delay(1500);
                  tutorialText.transform.position = new Vector3(-0.843f,2.45f,10f);
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.drawPu.GetHashCode();
                maskShape = 1;
                endByBtn = false;
                break;
            case 16:// after draw
                ScaleDownMask(1);
                await Task.Delay(1500); 
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.lastTurnPu.GetHashCode();
                maskShape = 2;
                endByBtn = true;
                break;
            case 17:
                enable = false;
                objectToFocus = Constants.TutorialObjectEnum.lastTurnPu.GetHashCode();
                maskShape = 2;
                endByBtn = false;
                break;
            case 19:
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.pu3.GetHashCode();
                maskShape = 2;
                endByBtn = false;
                break;
            case 20:
                enable = true;
                objectToFocus = Constants.TutorialObjectEnum.pu3select.GetHashCode();
                maskShape = 2;
                endByBtn = false;
                break;

        }

        BattleSystem.Instance.FocusOnObjectWithText(enable, maskShape, objectToFocus, endByBtn);
    }

    public void MoveCardsMaskPlayer(bool up , Action OnEndMove)
    {
        Vector3 targetPos = new Vector3(0, -11f, 10);
        tutorialMaskCards.transform.localScale = new Vector2(1.37f, 0.98f);
        Action DeactivateMask = ()=>tutorialMaskCards.SetActive(false);
        if (up)
        {
            tutorialMaskCards.transform.position = new Vector3(0, -11.3f, 10);
            tutorialMaskCards.SetActive(true);
            targetPos = new Vector3(0, -3.71f, 10);
            DeactivateMask = null;
        }
            StartCoroutine(AnimationManager.Instance.SmoothMove(tutorialMaskCards.transform,
                targetPos,tutorialMaskCards.transform.localScale, Values.Instance.tutoObjsFadeDuration,null,null, DeactivateMask, OnEndMove));
    }
    public void MoveCardsMaskEnemy(bool up , Action OnEndMove)
    {
        Vector3 targetPos = new Vector3(0, 11f, 10);
        tutorialMaskCards.transform.localScale = new Vector2(0.77f, 0.81f);
        Action DeactivateMask = ()=>tutorialMaskCards.SetActive(false);
        if (up)
        {
            tutorialMaskCards.transform.position = new Vector3(0, 11.17f, 10);
            tutorialMaskCards.SetActive(true);
            targetPos = new Vector3(0, 6.34f, 10);
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
        StartCoroutine(AnimationManager.Instance.ScaleObjectRatio(targetMaskScale, Values.Instance.tutoObjsFadeDuration, mask.transform, null, null));
    }

    public void ScaleMaskForEmojis()
    {
        tutorialMaskRing.transform.position = maskPositionForEmojis.position;
        StartCoroutine(AnimationManager.Instance.ScaleObjectRatio(2500f, Values.Instance.tutoObjsFadeDuration, tutorialMaskRing.transform, null, null));
    }
    [Button]
    public void ScaleMaskForEndFirstTurn()
    {
        tutorialMaskRect.transform.position = new Vector3(-7.29f, -0.38f, 10f);
        StartCoroutine(AnimationManager.Instance.ScaleObjectWithTarget(new Vector3(0.4491432f, 0.2902843f, 1f), Values.Instance.tutoObjsFadeDuration, tutorialMaskRect.transform, null, null));
    }

    private void Start()
    {
        darkScreenRenderer.color = new Color(0, 0, 0, 0.8f);
        darkScreenRenderer.sortingOrder = 33;
    }
}
