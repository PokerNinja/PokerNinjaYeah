﻿using StandardPokerHandEvaluator;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardUi : MonoBehaviour, IPointerClickHandler
{
    public Card card { get; set; }
    public string cardPlace;
    [SerializeField] bool isFaceDown = true;
    [SerializeField] public bool clickbleForPU = false;
    [SerializeField] public string cardDescription;
    public bool availableForReuse;
    public Material burnMaterial;
    public Material dissolveMaterial;
    public SpriteRenderer spriteRenderer;
    public SpriteRenderer cardSelectionRenderer;
    public GameObject cardMark;
    public GameObject cardSelection;
    public bool freeze;
    public Constants.CardsOwener whosCards = Constants.CardsOwener.Pool;
    private bool flipInProgress = false;



    public void Init(string cardsTag, Card newCard, bool isFaceDown, bool aboveDarkScreen, string cardPlace)
    {
        this.card = newCard;
        this.isFaceDown = isFaceDown;
        this.cardPlace = cardPlace;
        this.cardDescription = card.ToString(CardToStringFormatEnum.ShortCardName);
        InitCardsTag(cardsTag);
        LoadSprite(false);
        EnableSelecetPositionZ(aboveDarkScreen);
    }

    #region Settings

    public void OnObjectSpawn()
    {
        spriteRenderer.material.SetFloat("_FadeAmount", -0.1f);
        spriteRenderer.material.SetFloat("_OutlineAlpha", 0f);
        EnableSelecetPositionZ(false);
        cardMark.SetActive(false);
        SoundManager.Instance.RandomSoundEffect(0);
    }

    public void Activate(bool enable)
    {
        availableForReuse = !enable;
        gameObject.SetActive(enable);
    }

    public void InitCardsTag(string cardsTag)
    {
        tag = cardsTag;
        whosCards = WhosCard(cardsTag);
    }

    private Constants.CardsOwener WhosCard(string tag)
    {
        switch (tag)
        {
            case Constants.PlayerCardsTag:
                return Constants.CardsOwener.Player;
            case Constants.EnemyCardsTag:
                return Constants.CardsOwener.Enemy;
            case Constants.BoardCardsTag:
                return Constants.CardsOwener.Board;
            case Constants.DeckCardsTag:
                return Constants.CardsOwener.Deck;
            case Constants.PoolCardTag:
                return Constants.CardsOwener.Pool;
        }
        return Constants.CardsOwener.Pool;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        StartCoroutine(OnClickHandler());
    }

    private IEnumerator OnClickHandler()
    {
            yield return new WaitForSeconds(0.1f);
        if (!BattleSystem.Instance.TemproryUnclickable && clickbleForPU && !flipInProgress/* && BattleSystem.Instance.cardsToSelectCounter > 0*/)
        {
            BattleSystem.Instance.TemproryUnclickable = true;
            --BattleSystem.Instance.cardsToSelectCounter;
            clickbleForPU = false;
            SetSelection(false, "");
            Vector2 posTarget = transform.position;
            if (gameObject.name.Contains("Deck"))
            {
                posTarget = new Vector2(0, 0);
            }
            //SHOULD BE WITH INTERFACE LISTENR
            StartCoroutine(AnimationManager.Instance.PulseSize(true, gameObject.transform, 1.2f, 0.135f, true, () =>
            {
                BattleSystem.Instance.TemproryUnclickable = false;
                StartCoroutine(BattleSystem.Instance.OnCardsSelectedForPU(cardPlace, transform.position));

            }));
        }

        else
        {
        }
    }

    public bool GetisFaceDown()
    {
        return isFaceDown;
    }

    public void SetSelection(bool selectionEnable, string puElement)
    {
        if (!freeze || freeze && puElement.Equals("f") || freeze && !selectionEnable)
        {
            if (BattleSystem.Instance.cardsToSelectCounter > 0)
                clickbleForPU = selectionEnable;
            cardSelection.SetActive(selectionEnable);
            if (selectionEnable)
            {
                StartCoroutine(CardSelectionPulse(cardSelectionRenderer));
            }
        }
    }

    public void EnableSelecetPositionZ(bool aboveDarkScreen)
    {
        float interval = 0f;
        if (aboveDarkScreen)
        {
            interval = -26f;
        }
        transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, interval);
    }

    #endregion

    #region Visual


    private void LoadSprite(bool revealCard)
    {
        string sprite = cardDescription;

        if (!revealCard)
        {
            sprite = "back";
        }
        spriteRenderer.sprite = Resources.Load("Sprites/Cards/" + sprite, typeof(Sprite)) as Sprite;
    }




    public void FlipCard(bool reveal, Action onFinish)
    {
        isFaceDown = !reveal;
        flipInProgress = true;
        StartCoroutine(AnimationManager.Instance.FlipCard(gameObject.transform, 0.45f, () => flipInProgress = false, () => LoadSprite(reveal), onFinish));
        StartCoroutine(AnimationManager.Instance.PulseSize(true, gameObject.transform.parent, 1.15f, 0.45f, false, null));
    }

    public void CardReveal(bool reveal)
    {
        if (reveal)
        {
            FlipCard(reveal, null);
        }
    }




    public IEnumerator FadeBurnOut(Action onFinishDissolve)
    {

        spriteRenderer.material = burnMaterial;

        float dissolveAmount = -0.01f;
        float offsetX = UnityEngine.Random.Range(-1.18f, -0.73f);
        spriteRenderer.material.SetTextureOffset("_FadeTex", new Vector2(offsetX, -0.14f));
        //  material.SetTextureScale("_FadeTex", new Vector2(offset, 0.07f));
        SoundManager.Instance.RandomSoundEffect(SoundManager.SoundName.BurnCard);
        while (dissolveAmount < 1)
        {
            dissolveAmount += Time.deltaTime;
            spriteRenderer.material.SetFloat("_FadeAmount", dissolveAmount);
            yield return new WaitForFixedUpdate();
            if (dissolveAmount >= 1)
            {
                Activate(false);
                onFinishDissolve();
                break;
            }
        }
    }





    internal IEnumerator CardSelectionPulse(SpriteRenderer spriteRenderer)
    {
        float r = spriteRenderer.color.r;
        float g = spriteRenderer.color.g;
        float b = spriteRenderer.color.b;
        float dissolveAmount = 0.2f;
        bool floatUp = true;
        float alpha;
        while (clickbleForPU)
        {
            if (floatUp)
            {
                dissolveAmount += Time.deltaTime;
            }
            else
            {
                dissolveAmount -= Time.deltaTime;
            }
            if (dissolveAmount >= 1f)
            {
                yield return new WaitForSeconds(0.2f);
                floatUp = false;
            }
            else if (dissolveAmount <= 0f)
            {
                floatUp = true;
            }
            alpha = Mathf.Lerp(0f, 1f, dissolveAmount);
            spriteRenderer.color = new Color(r, g, b, alpha);
            yield return new WaitForFixedUpdate();
            if (!clickbleForPU)
            {
                spriteRenderer.color = new Color(r, g, b, 0);
                break;
            }
        }
        if (!clickbleForPU)
        {
            spriteRenderer.color = new Color(r, g, b, 0);
        }
    }

    internal void ApplyEyeEffect(Action EndAction, bool setImageTransparent, bool replaceImage)
    {

        cardMark.transform.localPosition = new Vector3(0.55f, 1.18f, -1f);
        if (setImageTransparent)
        {
            cardMark.transform.localScale = new Vector3(0.76f, 0.76f, 1f);
        }
        else
        {
            cardMark.transform.localScale = new Vector3(4f, 4f, 1f);
        }
        cardMark.SetActive(true);
        if (replaceImage)
        {

            StartCoroutine(AnimationManager.Instance.ScaleAndFadeEye(cardMark.transform, setImageTransparent, () => ReplaceMarkToFlipSign(), EndAction));
        }
        else
        {
            StartCoroutine(AnimationManager.Instance.ScaleAndFadeEye(cardMark.transform, setImageTransparent, null, EndAction));
        }
    }

    private void ReplaceMarkToFlipSign()
    {
        SpriteRenderer cardMarkSprite = cardMark.GetComponent<SpriteRenderer>();
        cardMarkSprite.sprite = Resources.Load("Sprites/Cards/flipflipsign", typeof(Sprite)) as Sprite;
        cardMark.transform.localScale = new Vector3(1, 1, 1);
        cardMark.transform.localPosition = new Vector3(0, 0, -1);
    }



    internal IEnumerator Dissolve(float delayInSec, Action onFinishDissolve)
    {
        spriteRenderer.material = dissolveMaterial;
        float dissolveAmount = -0.01f;
        yield return new WaitForSeconds(delayInSec);

        while (dissolveAmount < 1)
        {
            dissolveAmount += Time.deltaTime;
            spriteRenderer.material.SetFloat("_FadeAmount", dissolveAmount);
            yield return new WaitForFixedUpdate();
            if (dissolveAmount >= 1)
            {
                Activate(false);
                onFinishDissolve();
                break;
            }
        }
    }
}
#endregion