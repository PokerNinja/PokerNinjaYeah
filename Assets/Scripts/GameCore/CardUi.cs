using Sirenix.OdinInspector;
using StandardPokerHandEvaluator;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardUi : MonoBehaviour, IPointerClickHandler
{
    public string cardPlace;
    [SerializeField] bool isFaceDown = true;
    [SerializeField] public bool clickbleForPU = false;
    [SerializeField] public string cardDescription;
    public bool availableForReuse;
    // public Material burnMaterial;
    // public Material dissolveMaterial;
    public SpriteRenderer spriteRenderer;
    //public SpriteRenderer cardSelectionRenderer;
    public GameObject cardMark;
    public bool freeze;
    public bool isGhost;
    public Constants.CardsOwener whosCards = Constants.CardsOwener.Pool;
    private bool flipInProgress = false;
    public bool underSmoke;


    public void Init(string cardsTag, string cardDescription, bool isFaceDown, bool aboveDarkScreen, string cardPlace)
    {
        this.isFaceDown = isFaceDown;
        this.cardPlace = cardPlace;
        this.cardDescription = cardDescription;
        freeze = false;
        isGhost = false;
        underSmoke = false;
        InitCardsTag(cardsTag);
        LoadSprite(false);
        EnableSelecetPositionZ(aboveDarkScreen);
    }


    [Button]
    public void ChangeBool(bool enable)
    {
        /*   if (enable)
           {
               spriteRenderer.sharedMaterial.SetFloat("_OnlyInnerOutline", 0.0f);
           }
           else
           {
               spriteRenderer.sharedMaterial.SetFloat("_OnlyInnerOutline", 1.0f);
           }*/
        spriteRenderer.material.SetFloat("_OnlyInnerOutline", 0.0f);

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
        if (!Constants.TemproryUnclickable && clickbleForPU && !flipInProgress && Constants.cardsToSelectCounter > 0)
        {
            Constants.TemproryUnclickable = true;
            --Constants.cardsToSelectCounter;
            clickbleForPU = false;
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CardSelectClick, false);
            SetSelection(false, "", "");
            Vector2 posTarget = transform.position;
            if (gameObject.name.Contains("Deck"))
            {
                posTarget = new Vector2(0, 0);
            }
            //SHOULD BE WITH INTERFACE LISTENR
            StartCoroutine(AnimationManager.Instance.PulseSize(true, gameObject.transform, 1.2f, 0.135f, true, () =>
            {
                Constants.TemproryUnclickable = false;
                if (Constants.TUTORIAL_MODE)
                {
                StartCoroutine(BattleSystemTuto.Instance.OnCardsSelectedForPU(cardPlace, transform.position));
                }
                else
                {
                StartCoroutine(BattleSystem.Instance.OnCardsSelectedForPU(cardPlace, transform.position));
                }

            }));
        }
        else if(Constants.cardsToSelectCounter > 0 && freeze)
        {
            BattleSystem.Instance.Interface.FreezeSign(transform.position);
        }
        else
        {
            StartCoroutine(AnimationManager.Instance.Shake(spriteRenderer.material, Values.Instance.disableClickShakeDuration));
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CantClick, false);
        }
    }

    internal void LoadNewFlusherSprite()
    {
        LoadSprite(true);
    }

    public bool GetisFaceDown()
    {
        return isFaceDown;
    }

    public void SetSelection(bool selectionEnable, string puElement, string puName)
    {
        bool okToSelect = true;
        if (freeze)
        {
            if (puName.Equals("s1") || puElement.Equals("f")|| puElement.Equals("i") || !selectionEnable)
            {
                okToSelect = true;
            }
            else
            {
                okToSelect = false;
            }
        }
        if (underSmoke)
        {
            if (puElement.Equals("w") || puElement.Equals("f") || !selectionEnable)
            {
                okToSelect = true;
            }
            else
            {
                okToSelect = false;
            }
        }


        if (okToSelect)
        {
            //   if (BattleSystem.Instance.cardsToSelectCounter > 0)
            /*if (Values.Instance.TUTORIAL_MODE)
            {
                if (cardPlace.Equals(Constants.PlayerCard1) || cardPlace.Equals(Constants.EnemyCard2) || cardPlace.Equals(Constants.BFlop2))
                {
                    clickbleForPU = selectionEnable;
                }
            }
            else
            {
                clickbleForPU = selectionEnable;
            }*/
            clickbleForPU = selectionEnable;

            //cardSelection.SetActive(selectionEnable);
            if (selectionEnable)
            {
               /* cardSelectionRenderer.color = Values.Instance.cardSelectionColor;
                StartCoroutine(CardSelectionPulse(cardSelectionRenderer));*/
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


    public void LoadSprite(bool revealCard)
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
        StartCoroutine(AnimationManager.Instance.FlipCard(gameObject.transform, Values.Instance.cardFlipDuration, () => flipInProgress = false, () =>
        {
            LoadSprite(reveal);
        }, onFinish));
        StartCoroutine(AnimationManager.Instance.PulseSize(true, gameObject.transform.parent, 1.15f, Values.Instance.cardFlipDuration, false, null));
    }

    public void CardReveal(bool reveal)
    {
        if (reveal)
        {
            FlipCard(reveal, null);
        }
    }




    public IEnumerator FadeBurnOut(Material burnMaterial, bool changeOffset, Action onFinishDissolve)
    {

        spriteRenderer.material = burnMaterial;

        float dissolveDuration = Values.Instance.cardBurnDuration;
        float dissolveAmount = -0.01f;
        if (changeOffset)
        {
            float offsetX = UnityEngine.Random.Range(-1.18f, -0.73f);
            spriteRenderer.material.SetTextureOffset("_FadeTex", new Vector2(offsetX, -0.14f));
        }

        //TODO this is problam
        SoundManager.Instance.RandomSoundEffect(SoundManager.SoundName.BurnCard);

        //  material.SetTextureScale("_FadeTex", new Vector2(offset, 0.07f));

        while (dissolveAmount <= 1)
        {
            yield return new WaitForFixedUpdate();
            dissolveAmount += Time.deltaTime / dissolveDuration;
            spriteRenderer.material.SetFloat("_FadeAmount", dissolveAmount);
            if (dissolveAmount >= 1)
            {
               // Activate(false);
                onFinishDissolve?.Invoke();
                break;
            }
        }
    }


    public IEnumerator FadeGhost(bool fadeIn, Action onFinishDissolve)
    {

        float dissolveDuration = Values.Instance.cardBurnDuration * 2;
        //  float dissolveAmount = 0.12f;
        float dissolveAmount = -0.01f;
        float shadowAlpha = 0.31f;
        if (fadeIn)
        {
            shadowAlpha = 0f;
            dissolveAmount = 1f;
        }
        //  material.SetTextureScale("_FadeTex", new Vector2(offset, 0.07f));
        SoundManager.Instance.RandomSoundEffect(SoundManager.SoundName.BurnCard);
        while (dissolveAmount < 1 || dissolveAmount > -0.01f)
        {
            if (fadeIn)
            {
                dissolveAmount -= Time.deltaTime / dissolveDuration;
                if (shadowAlpha < 0.32f)
                {
                    shadowAlpha += Time.deltaTime / dissolveDuration;
                }
            }
            else
            {
                dissolveAmount += Time.deltaTime / dissolveDuration;
                if (shadowAlpha > 0f)
                {
                    shadowAlpha -= Time.deltaTime / dissolveDuration;
                }
            }
            spriteRenderer.material.SetFloat("_FadeAmount", dissolveAmount);
            spriteRenderer.material.SetFloat("_ShadowAlpha", shadowAlpha);
            yield return new WaitForFixedUpdate();
            if (dissolveAmount >= 1 || dissolveAmount <= -0.01f)
            {
                if (!fadeIn)
                {
                    Activate(false);
                }
                else
                {
                    spriteRenderer.material.SetFloat("_FadeAmount", -0.01f);
                }
                onFinishDissolve?.Invoke();
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



    internal IEnumerator Dissolve(bool isFrozen, Material dissolveMaterial, float delayInSec, Action onFinishDissolve)
    {
        spriteRenderer.material = dissolveMaterial;
        //TODO MAYBE NEED DISSOLVEMAT
        float dissolveAmount = -0.01f;
        yield return new WaitForSeconds(delayInSec);
        if (isFrozen)
        {
            StartCoroutine(AnimationManager.Instance.UpdateValue(false, "_FadeBurnWidth", dissolveAmount, spriteRenderer.material, 0,
                () => spriteRenderer.material.SetFloat("_FadeBurnWidth", 0.088f)));
        }
        while (dissolveAmount <= 1)
        {
            dissolveAmount += Time.deltaTime;
            spriteRenderer.material.SetFloat("_FadeAmount", dissolveAmount);
            yield return new WaitForFixedUpdate();
            if (dissolveAmount >= 1)
            {
               // Activate(false);
                onFinishDissolve();
                break;
            }
        }
    }
}
#endregion