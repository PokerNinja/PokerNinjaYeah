
using Sirenix.OdinInspector;
using StandardPokerHandEvaluator;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class BattleUI : MonoBehaviour
{

    public bool updateTutorial = true;

    //[SerializeField] public Slider musicSlider;
    [SerializeField] public CoinFlipScript coinFlipTurn;
    [SerializeField] public TextMeshProUGUI playerNameText;
    [SerializeField] public TextMeshProUGUI enemyNameText;
    [SerializeField] public TextMeshProUGUI winLoseBot;
    [SerializeField] public GameObject[] playerLifeUi;
    [SerializeField] public GameObject[] enemyLifeUi;
    [SerializeField] public PowerUpUi[] enemyPus;
    [SerializeField] public PowerUpUi[] playerPus;



    [SerializeField] public TextMeshProUGUI textWinLabel;
    [SerializeField] public int winnerPanelInterval;



    [SerializeField] public GameObject winLabel;
    [SerializeField] public GameObject handRankInfo;
    [SerializeField] public TextMeshProUGUI handRankText;

    [SerializeField] private CanvasGroup infoCanvas;

    [SerializeField] public GameObject rankImageParent;
    [SerializeField] public SpriteRenderer darkScreenRenderer;
    [SerializeField] public SpriteRenderer cancelDarkScreenRenderer;
    [SerializeField] public TextMeshProUGUI largeText;
    [SerializeField] public GameObject largeTextGO;

    // [SerializeField] public GameObject turnTextGO;
    [SerializeField] public GameObject targetTurnSymbol;

    [SerializeField] public GameObject fireProjectile1;
    [SerializeField] public GameObject fireProjectile2;



    [SerializeField] public GameObject iceProjectile1;
    [SerializeField] public GameObject iceProjectile2;



    //[SerializeField] public SpriteRenderer windSpriteRenderer;
    [SerializeField] public GameObject windEffect, windEffect2;

    [SerializeField] public GameObject gameOverPanel;



    public GameObject cardDeckHolder;
    public GameObject puDeckHolder;

    public SpriteRenderer playerFrameTurn;
    public SpriteRenderer enemyFrameTurn;
    public SpriteRenderer playerLargeTurnIndicator;

    public SpriteRenderer btnReplaceRenderer;


    public GameObject youWin;
    public GameObject youLose;
    public GameObject hitGO;
    public GameObject tieTitle;


    public SpriteRenderer[] coinsSpriteRend;
    public EnergyUi[] energyLeft;


    public SpriteRenderer bgSpriteRenderer;
    public Animator playerAvatarAnimator;
    public Animator enemyAvatarAnimator;

    public ParticleSystem rightSlash, leftSlash;
    public ParticleSystem winParticle;
    public GameObject rightCard, leftCard;


    public SpriteRenderer rightCardChildSprite, leftCardChildSprite;
    public Transform rightCardTarget, leftCardTarget;

    public Transform enemyHandLocation;
    public Transform playerHandLocation;

    //Buttons

    public Material freezeMaterial;
    public TextMeshProUGUI dialogTitleUi;
    public TextMeshProUGUI dialogContentUi;
    public SpriteRenderer[] emojis;
    public SpriteRenderer emojiSelector;
    public SpriteRenderer emojiToDisplayRendererPlayer;
    public SpriteRenderer emojiToDisplayRendererEnemy;
    public EmojiToDisplay emojiToDisplayPlayer;
    public EmojiToDisplay emojiToDisplayEnemy;

    public ParticleSystem hideSmokeBoard;
    public ParticleSystem hideSmokeHand;
    public ParticleSystem showSmokeBoard;
    public ParticleSystem showSmokeHand;
    public ParticleSystem icenadoPS;
    public ParticleSystem armageddonPS;
    public ParticleSystem shutterIce;

    [SerializeField] private GameObject turnBtn;
    [SerializeField] public SpriteRenderer turnArrowSprite;// MAYBE in Timer
    [SerializeField] public BetBtnUi betBtn;
    [SerializeField] public TextMeshProUGUI currentRankText;
    [SerializeField] private TextMeshProUGUI currentRankNumber;

    [SerializeField] private GameObject pFlusher, pStrighter, eFlusher, eStrighter;

    public static bool isPlayerTurn;
    private string playerName, enemyName;

    private bool sliding;
    private int lastHandRank = 10;
    public GameObject psParent;
    public Transform playerHpUi;
    public Transform enemyHpUi;
    [SerializeField] private TextMeshProUGUI totalHpText;
    [SerializeField] private TextMeshProUGUI currentHpText;
    [SerializeField] private CanvasGroup hpInfoCanvas;
    [SerializeField] private Animator damageAnimationController;
    [SerializeField] private Animator extraDamageAnimationController;
    [SerializeField] private TextMeshProUGUI damageText;
    [SerializeField] private TextMeshProUGUI extraDamageText;



    [SerializeField] private Animation cameraShakeAnimation;


    public Transform pusThemePlayer, pusThemeEnemy;

    public SpriteRenderer ninjaBgP, ninjaBgE;
    public SpriteRenderer puBgP, puBgE;
    public SpriteRenderer puFrameP, puFrameE;
    public SpriteRenderer ninjaFrameP, ninjaFrameE;
    private string[] ninjaThemeSprites = { "wood", "green", "dojo", "space" };
    private Color[] ninjaFrameColors;


    public void Initialize(PlayerInfo player, PlayerInfo enemy, bool HP_GAME, float totalHp)
    {
        InitializePlayer(player);
        InitializeEnemy(enemy);
        if (HP_GAME)
        {
            totalHpText.text = "/ " + totalHp;
        }
        else
        {
            InitializeAnimations();
        }
    }

    private void InitializeAnimations()
    {
        foreach (SpriteRenderer sr in coinsSpriteRend)
        {
            StartShineEffectLoop(sr.material, 1.5f);
        }
    }

    private void StartShineEffectLoop(Material material, float interval)
    {
        StartCoroutine(AnimationManager.Instance.ShinePU(true, interval, Values.Instance.coinShineEvery, material, () => StartShineEffectLoop(material, interval)));
    }

    private void InitializePlayer(PlayerInfo player)
    {
        playerName = player.id;
        playerNameText.text = playerName;
    }
    private void InitializeEnemy(PlayerInfo enemy)
    {
        enemyName = enemy.id;
        enemyNameText.text = enemyName;
    }


    public void FreezeObject(SpriteRenderer spriteTarget, bool isToFreeze, bool isFaceDown, Action onReset, bool enableSound)
    {

        StartCoroutine(AnimationManager.Instance.FreezeEffect(isToFreeze, isFaceDown, spriteTarget, freezeMaterial, onReset));
        if (enableSound)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.PuFreeze, false);
        }
    }

    internal void ActivateHpObjects()
    {
        betBtn.gameObject.SetActive(true);
        playerHpUi.parent.gameObject.SetActive(true);
        enemyHpUi.parent.gameObject.SetActive(true);
    }

    public IEnumerator CardProjectileEffect(bool isPlayerWin, Action HitEffect, Action LoseCoin)
    {

        if (isPlayerWin)
        {
            rightCard.transform.position = Values.Instance.winCardRightStart.position;
            leftCard.transform.position = Values.Instance.winCardLeftStart.position;
        }
        else
        {
            rightCard.transform.position = Values.Instance.winCardRightEnd.position;
            leftCard.transform.position = Values.Instance.winCardLeftEnd.position;
        }
        StartCoroutine(VisualDamage(HitEffect,LoseCoin));
        yield return new WaitForSeconds(0.25f);
        rightCard.SetActive(true);
        leftCard.SetActive(true);
        StartCoroutine(AnimationManager.Instance.SpinRotateValue(rightCardChildSprite, () =>
        {
           /* HitEffect?.Invoke();
            LoseCoin?.Invoke();*/
        }));
        StartCoroutine(AnimationManager.Instance.SmoothMoveCardProjectile(isPlayerWin, true, rightCard.transform, rightCard.transform.localScale, Values.Instance.winningCardProjectileMoveDuration,
            () =>
            {
                /*HitEffect?.Invoke();
                LoseCoin?.Invoke();*/
            }, () =>
       {
           rightCard.SetActive(false);
       }));
        yield return new WaitForSeconds(0.2f);
        StartCoroutine(AnimationManager.Instance.SpinRotateValue(leftCardChildSprite, null));
        StartCoroutine(AnimationManager.Instance.SmoothMoveCardProjectile(isPlayerWin, false, leftCard.transform, leftCard.transform.localScale, Values.Instance.winningCardProjectileMoveDuration, null, () =>
       {
           leftCard.SetActive(false);
       }));

        yield return new WaitForSeconds(1f);
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Slash1, true);
        yield return new WaitForSeconds(0.2f);
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Slash2, true);

    }

    private IEnumerator VisualDamage(Action hitEffect, Action loseCoin)
    {
        yield return new WaitForSeconds(2f);
        hitEffect.Invoke();
        loseCoin.Invoke();
    }

    internal void SlidePuSlots()
    {
        Vector3 targetPos = new Vector3(0, 0, 90f);
        StartCoroutine(AnimationManager.Instance.SmoothMove(pusThemePlayer, targetPos, new Vector3(1, 1, 1), Values.Instance.pusDrawerMoveDuration, null, null, null, null));
        StartCoroutine(AnimationManager.Instance.SmoothMove(pusThemeEnemy, targetPos, new Vector3(1, 1, 1), Values.Instance.pusDrawerMoveDuration, null, null, null, null));
    }

    internal void LoadNinjaBG()
    {
        ninjaFrameColors = new Color[] { Values.Instance.woodFrameColor, Values.Instance.greenFrameColor, Values.Instance.dojoFrameColor, Values.Instance.spaceFrameColor };
        int randomPlayerTheme = UnityEngine.Random.Range(0, ninjaThemeSprites.Length);
        int randomEnemyTheme = UnityEngine.Random.Range(0, ninjaThemeSprites.Length);
        LoadTheme(ninjaBgP, ninjaThemeSprites[randomPlayerTheme], "bg");
        LoadTheme(ninjaBgE, ninjaThemeSprites[randomEnemyTheme], "bg");
        LoadTheme(puBgP, ninjaThemeSprites[randomPlayerTheme], "card");
        LoadTheme(puBgE, ninjaThemeSprites[randomEnemyTheme], "card");
        LoadTheme(puFrameP, ninjaThemeSprites[randomPlayerTheme], "orn");
        LoadTheme(puFrameE, ninjaThemeSprites[randomEnemyTheme], "orn");
        ninjaFrameP.color = ninjaFrameColors[randomPlayerTheme];
        ninjaFrameE.color = ninjaFrameColors[randomEnemyTheme];
    }

    private void LoadTheme(SpriteRenderer target, string randomTheme, string endPath)
    {
        target.sprite = Resources.Load("Sprites/Ninja/NinjaBg/" + randomTheme + endPath, typeof(Sprite)) as Sprite;
    }

    public IEnumerator SlashEffect()
    {
        rightSlash.Play();
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Slash1, true);
        yield return new WaitForSeconds(0.1f);
        leftSlash.Play();
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Slash2, true);
    }
    public void WinParticleEffect()
    {
        Instantiate(winParticle.gameObject);
    }

    public void LoseLifeUi(bool isPlayer, int lifeIndex)
    {
        SpriteRenderer spTarget;
        Transform tTarget;
        if (isPlayer)
        {
            spTarget = playerLifeUi[lifeIndex].GetComponent<SpriteRenderer>();
            tTarget = playerLifeUi[lifeIndex].transform;
        }
        else
        {
            spTarget = enemyLifeUi[lifeIndex].GetComponent<SpriteRenderer>();
            tTarget = enemyLifeUi[lifeIndex].transform;
        }
        StartCoroutine(AnimationManager.Instance.LoseLifeAnimation(spTarget, () => StartCoroutine(AnimationManager.Instance.AlphaAnimation(spTarget, false, Values.Instance.LoseLifeDuration, null))));
        StartCoroutine(AnimationManager.Instance.SpinCoin(tTarget, 0.4f, null));
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CoinLose, true);
    }


    internal IEnumerator CoinFlipStartGame(bool isPlayerStart, Action EndAction)
    {
        //  coinFlipTurn.RevealCoinFlip();
        yield return new WaitForSeconds(1);
        float yPosition = 7f;
        if (isPlayerStart)
        {
            yPosition = -7;
        }
        Vector3 targetPosition = new Vector3(0, yPosition, 0);
        coinFlipTurn.SetDirection(isPlayerStart);
        coinFlipTurn.FlipCoinAnimation();
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CoinFlipStart, true);
        /* StartCoroutine(AnimationManager.Instance.SmoothMove(coinFlipTurn.transform, new Vector3(0,0,0), coinFlipTurn.transform.localScale, Values.Instance.coinFlipEndMoveDuration, null, null, 
             () => coinFlipTurn.SetDirection(isPlayerStart), () => coinFlipTurn.FlipCoinAnimation()));*/
        yield return new WaitForSeconds(3);
        EndAction?.Invoke();
        //Todo dont stop
        StartCoroutine(AnimationManager.Instance.SmoothMove(coinFlipTurn.transform, targetPosition, new Vector3(1, 1, 1), Values.Instance.coinFlipEndMoveDuration, null, () => coinFlipTurn.gameObject.SetActive(false), null, null));
    }
    internal void InitAvatars()
    {

        playerAvatarAnimator.Play("idle", 0, 0f);
        enemyAvatarAnimator.Play("idle", 0, 0.4f);
    }

    public IEnumerator ShowWinner(string winnerMsg)
    {
        yield return new WaitForSeconds(winnerPanelInterval);
        textWinLabel.text = winnerMsg;
        winLabel.SetActive(true);
    }

    public void WinPanelAfterEnemyLeaveGame(string otherPlayer)
    {
        textWinLabel.text = otherPlayer + " left the game. You Win!";
        winLabel.SetActive(true);
    }

    public void EnablePlayerButtons(bool enable)
    {
        EnableEndTurnBtn(enable);
        EnableBtnReplace(enable);
        if (Constants.HP_GAME)
        {
            betBtn.EnableBetBtn(enable);
        }
        /* if (BattleSystem.Instance.replacePuLeft > 0)
         {
             EnableBtnReplace(enable);
         }*/
    }



    public void EnableEndTurnBtn(bool enable)
    {
        if (enable)
        {
            StartCoroutine(AnimationManager.Instance.AlphaAnimation(turnArrowSprite, true, Values.Instance.turnBtnAlphaDuration, () => turnBtn.GetComponent<Button>().interactable = true));
        }
        else
        {
            turnBtn.GetComponent<Button>().interactable = false;
            StartCoroutine(AnimationManager.Instance.AlphaAnimation(turnArrowSprite, false, Values.Instance.turnBtnAlphaDuration, null));
        }
    }

    internal void HitEffect(bool isPlayerHit)
    {
        {
            hitGO.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            SpriteRenderer hitSpriteRen = hitGO.GetComponent<SpriteRenderer>();
            if (isPlayerHit)
            {
                // hitGO.transform.position = youLose.transform.position;
                hitGO.transform.position = enemyHandLocation.position;
            }
            else
            {
                hitGO.transform.position = playerHandLocation.position;
            }
            hitSpriteRen.color = new Color(hitSpriteRen.color.r, hitSpriteRen.color.g, hitSpriteRen.color.b, 1f);
            // StartCoroutine(AnimationManager.HitEffect(hitSpriteRen, () => hitGO.SetActive(false)));

            StartCoroutine(AnimationManager.Instance.ScaleObjectRatio(10f, Values.Instance.hitTextScaleDuration, hitGO.transform, null, () => StartCoroutine(AnimationManager.Instance.AlphaAnimation(hitSpriteRen, false, Values.Instance.hitTextFDuration, null))));


        }
    }
    public void ShowPuInfoDialog(Vector2 startingPosition, bool paddingRight, string puName, string puDisplayName, bool isEnable, bool isBtnOn, Action OnEnd)
    {
        Vector2 targetDialog;
        if (paddingRight)
        {
            targetDialog = new Vector2(startingPosition.x + 1f, startingPosition.y + 2.5f);
        }
        else
        {
            targetDialog = new Vector2(startingPosition.x, startingPosition.y + 2.5f);
        }
        if (isEnable)
        {
            infoCanvas.transform.position = new Vector2(0, -7f);
            infoCanvas.transform.localScale = new Vector2(0.1f, 0.1f);
            if (puName.Equals("replace"))
            {
                InitDialog(puDisplayName, Constants.ReplacePuInfo, isBtnOn);
            }
            else if (puName.Equals("bet"))
            {
                InitDialog(puDisplayName, Constants.BetInfo, isBtnOn);
            }
            else
            {
                InitDialog(puDisplayName, PowerUpStruct.Instance.GetPuInfoByName(puName), isBtnOn);
            }
            // infoCanvas.alpha = 1f;
            StartCoroutine(AnimationManager.Instance.ShowDialogFromPu(infoCanvas.transform, startingPosition, targetDialog, OnEnd));
            StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(infoCanvas, true, Values.Instance.infoDialogFadeOutDuration, null));
            // StartCoroutine(AnimationManager.Instance.AlphaFontAnimation(dialogContentUi, true, Values.Instance.showDialogMoveDuration, null));
        }
        else
        {
            if (infoCanvas.alpha > 0)
            {
                StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(infoCanvas, false, Values.Instance.infoDialogFadeOutDuration, OnEnd));

                /* StartCoroutine(AnimationManager.Instance.AlphaAnimation(dialogSprite, false, Values.Instance.infoDialogFadeOutDuration, OnEnd));
                 StartCoroutine(AnimationManager.Instance.AlphaFontAnimation(dialogContentUi, false, Values.Instance.infoDialogFadeOutDuration, null));*/
            }
        }

    }

    internal IEnumerator DisplayDamageText(bool isPlayerWin, float currentDamageThisRound, float extraDamage)
    {
        damageText.text ="-"+ currentDamageThisRound.ToString();
        extraDamageText.text = "-" + extraDamage.ToString();
        string anim = "hit_player";
        if (!isPlayerWin)
        {
            anim = "hit_enemy";
        }
        damageText.gameObject.SetActive(true);
        extraDamageText.gameObject.SetActive(extraDamage != 0);
        damageAnimationController.Play(anim);
        yield return new WaitForSeconds(0.2f);
        if (extraDamage != 0)
        {
            extraDamageAnimationController.Play(anim);
        }
        yield return new WaitForSeconds(2f);
        damageText.gameObject.SetActive(false);
        extraDamageText.gameObject.SetActive(false);
    }

    public void InitDialog(string dialogTitle, string dialogContent, bool enableBtns)
    {
        //  dialogTitleUi.text = dialogTitle;
        dialogContentUi.text = dialogContent;
    }

    internal void RevealCards(List<CardUi> whatCards)
    {
        foreach (CardUi card in whatCards)
        {
            if (card.GetisFaceDown())
            {
                card.FlipCard(true, null);
            }
        }
    }

    internal void ShowGameOverPanel(bool isPlayerWin)
    {
        gameOverPanel.SetActive(true);
        if (isPlayerWin)
        {
            youWin.SetActive(true);
        }
        else
        {
            youLose.SetActive(true);
        }
    }

    internal void ResetHandRank()
    {
        lastHandRank = 10;
        currentRankNumber.text = "10";
        Values.Instance.currentVisionColor = Values.Instance.visionColorsByRank[9];
    }

    internal void SlideRankingImg()
    {
        if (!sliding)
        {
            sliding = true;
            StartCoroutine(AnimationManager.Instance.SmoothMoveRank(rankImageParent.transform, Values.Instance.rankInfoMoveDuration,
                () => rankImageParent.SetActive(false),
                () => rankImageParent.SetActive(true)
                , () => sliding = false));
        }
    }
    public bool IsSliderOpen()
    {
        return rankImageParent.activeSelf;
    }


    public void EnableDarkScreen(bool isPlayerActivateSelectMode, bool enable, Action ResetSortingOrder)
    {
        darkScreenRenderer.GetComponent<BoxCollider2D>().enabled = enable;
        // BUG FM2 ResetSortingOrder
        AnimationManager.Instance.FadeBurnDarkScreen(darkScreenRenderer.material, enable, Values.Instance.darkScreenAlphaDuration, ResetSortingOrder);
        if (isPlayerActivateSelectMode && !enable)
        {
            FadeCancelSelectModeScreen(false);
        }

    }

    public void FadeCancelSelectModeScreen(bool enable)
    {
        StartCoroutine(AnimationManager.Instance.AlphaAnimation(cancelDarkScreenRenderer, enable, Values.Instance.darkScreenAlphaDuration, null));
    }

    public void EnableVisionClick(bool enable)
    {
        //TODO sorting
        float targetZ = -1f;
        if (enable)
        {
            targetZ = -30f;
        }
        currentRankNumber.transform.parent.localPosition = new Vector3(currentRankNumber.transform.parent.localPosition.x, currentRankNumber.transform.parent.localPosition.y, targetZ);
    }



    public void VisionEffect(List<Card> winningCards, List<CardUi> winningPlayersCards)
    {
        List<CardUi> cardsToGlow = new List<CardUi>();
        string winningCardDesc;
        for (int i = 0; i < 5; i++)
        {
            winningCardDesc = winningCards[i].ToString(CardToStringFormatEnum.ShortCardName);
            for (int j = 0; j < winningPlayersCards.Count; j++)
            {
                if (winningPlayersCards[j].cardDescription.ToString().Equals(winningCardDesc))
                {
                    cardsToGlow.Add(winningPlayersCards[j]);
                    if (cardsToGlow.Count == 5)
                    {
                        break;
                    }
                    j = winningPlayersCards.Count;
                }
            }
        }

        AnimationManager.Instance.VisionEffect(cardsToGlow, 5, true);
    }



    public void SetTurnIndicator(bool isPlayerTurn, bool enable)
    {
        AnimationManager.Instance.alphaLoopEnable = false;
        if (!enable)
        {
            AnimationManager.Instance.SetAlpha(playerFrameTurn, 0f);
            AnimationManager.Instance.SetAlpha(enemyFrameTurn, 0f);
        }
        else
        {
            EnableTurnIndicator(isPlayerTurn, null);
        }
    }

    /* public void WhosTurnAnimation(bool isPlayer, bool yourLastTurn, bool finalMove, Action EndRoutine)
     {
         string targetTurnTextPath;
         Action endAction = null;
         Action enableBGPulse = null;
         turnTextGO.transform.localScale = new Vector2(0.1f, 0.1f);
         turnTextGO.transform.position = new Vector2(0f, 0f);
         turnTextGO.SetActive(true);
         targetTurnTextPath = GetTurnTextPath(isPlayer, yourLastTurn, finalMove);
         SpriteRenderer spriteRenderer = turnTextGO.GetComponent<SpriteRenderer>();
         AnimationManager.Instance.SetAlpha(spriteRenderer, 1f);
         spriteRenderer.sprite = Resources.Load(targetTurnTextPath, typeof(Sprite)) as Sprite;

         if (finalMove || yourLastTurn)
         {
             enableBGPulse = () => EnableBgColor(true);
         }
         *//* else
          {
              alphaAction = () => StartCoroutine(AnimationManager.AlphaAnimation(spriteRenderer, false, 1f, () =>
              {
                  turnTextGO.SetActive(false);
                  AnimationManager.SetAlpha(spriteRenderer, 1f);
              }));

          }*//*
         endAction = () => StartCoroutine(AnimationManager.Instance.SmoothMove(turnTextGO.transform, targetTurnSymbol.transform.position, targetTurnSymbol.transform.localScale, Values.Instance.turnTextMoveDuration, null, null, EndRoutine, () =>
                 turnTextGO.transform.localPosition = new Vector3(turnTextGO.transform.localPosition.x, turnTextGO.transform.localPosition.y, 19.5f)));

         StartCoroutine(AnimationManager.Instance.ScaleObjectRatio(11f, Values.Instance.turnTextScaleDuration, turnTextGO.transform, enableBGPulse, endAction));

     }*/

    public void EnableBgColor(bool enable)
    {
        StartCoroutine(AnimationManager.Instance.PulseColorAnimation(bgSpriteRenderer, enable, Values.Instance.bgPulseColorSwapDuration));
    }

    private string GetTurnTextPath(bool isPlayer, bool yourLastTurn, bool finalMove)
    {
        string path = "Sprites/GameScene/Indicators/turn_";
        if (finalMove)
        {
            path += "final";
        }
        else if (yourLastTurn)
        {
            path += "player_last";
        }
        else if (isPlayer)
        {
            path += "player";
        }
        else if (!isPlayer)
        {
            path += "enemy";
        }
        return path;
    }

    public void ResetTurnPanels()
    {
        //  turnTextGO.SetActive(false);
    }

    public void EnableTurnIndicator(bool playerTurn, Action OnFinish)
    {
        if (playerTurn)
        {
            //AnimationManager.Instance.SetAlpha(enemyFrameTurn, 0f);
            StartCoroutine(AnimationManager.Instance.AlphaAnimation(playerLargeTurnIndicator, true, Values.Instance.turnIndicatorFadeDuration, () =>
             StartCoroutine(AnimationManager.Instance.AlphaAnimation(playerLargeTurnIndicator, false, Values.Instance.turnIndicatorFadeDuration, null))));
            // StartCoroutine(AnimationManager.Instance.AlphaAnimation(playerLargeNotTurnIndicator, false, Values.Instance.turnIndicatorFadeDuration, OnFinish));

            StartCoroutine(AnimationManager.Instance.AlphaLoop(playerFrameTurn, Values.Instance.turnIndicatorFadeDuration, OnFinish));
        }
        else
        {
            // AnimationManager.Instance.SetAlpha(playerFrameTurn, 0f);
            // StartCoroutine(AnimationManager.Instance.AlphaAnimation(playerLargeNotTurnIndicator, true, Values.Instance.turnIndicatorFadeDuration, null));
            StartCoroutine(AnimationManager.Instance.AlphaLoop(enemyFrameTurn, Values.Instance.turnIndicatorFadeDuration, OnFinish));
        }
    }

    internal void InitLargeText(bool enable, string text)
    {
        if (enable)
        {
            largeText.text = text;
        }
        largeTextGO.SetActive(enable);
    }
    internal void UpdateCardRank(int handRank)
    {
        if (handRank == -1)
        {
            handRank = 7000;
        }
        int currentHandRank = ConvertHandRankToTextNumber(handRank);
        if (lastHandRank != currentHandRank)
        {
            UpdateVisionColor(currentHandRank);
            currentRankNumber.text = "" + currentHandRank;
            StartCoroutine(AnimationManager.Instance.PulseSize(true, currentRankNumber.transform, 1.2f, Values.Instance.pulseDuration, false, null));
            if (lastHandRank < currentHandRank)
            {
                SoundManager.Instance.RandomSoundEffect(SoundManager.SoundName.RankDown);
            }
            else
            {
                SoundManager.Instance.RandomSoundEffect(SoundManager.SoundName.RankUp);
            }
        }
        lastHandRank = currentHandRank;
        // currentRankText.text = ConvertHandRankToTextDescription(handRank);
    }

    /* [Button]
     public void RankUp()
     {
         SoundManager.Instance.RandomSoundEffect(SoundManager.SoundName.RankUp);
     }
     [Button]
     public void RankDown()
     {
         SoundManager.Instance.RandomSoundEffect(SoundManager.SoundName.RankDown);
     }*/
    public void UpdateVisionColor(int currentHandRank)
    {
        Values.Instance.currentVisionColor = Values.Instance.visionColorsByRank[currentHandRank - 1];
    }

    public string ConvertHandRankToTextDescription(int handRank)
    {
        switch (handRank)
        {
            case 1:
                return "Royal Flush";
            case int n when (n <= 10 && n >= 2):
                return "Straight Flush";
            case int n when (n <= 166 && n >= 11):
                return "Four Of A Kind";
            case int n when (n <= 322 && n >= 167):
                return "Full House";
            case int n when (n <= 1599 && n >= 323):
                return "Flush";
            case int n when (n <= 1609 && n >= 1600):
                return "Straight";
            case int n when (n <= 2467 && n >= 1610):
                return "Three Of A Kind";
            case int n when (n <= 3325 && n >= 2468):
                return "Two Pairs";
            case int n when (n <= 6185 && n >= 3326):
                return "Pair";
            case int n when (n <= 7462 && n >= 6186):
                return "High Card";
            case -1:
                return "not enough visible cards!";
            default:
                break;
        }
        return "X";
    }



    public int ConvertHandRankToTextNumber(int handRank)
    {
        switch (handRank)
        {
            case 1:
                return 1;
            case int n when (n <= 10 && n >= 2):
                return 2;
            case int n when (n <= 166 && n >= 11):
                return 3;
            case int n when (n <= 322 && n >= 167):
                return 4;
            case int n when (n <= 1599 && n >= 323):
                return 5;
            case int n when (n <= 1609 && n >= 1600):
                return 6;
            case int n when (n <= 2467 && n >= 1610):
                return 7;
            case int n when (n <= 3325 && n >= 2468):
                return 8;
            case int n when (n <= 6185 && n >= 3326):
                return 9;
            case int n when (n <= 7462 && n >= 6186):
                return 10;
            default:
                break;
        }
        return 10;
    }
    internal void InitProjectile(Vector2 startingPos, string powerUpName, bool isPlayerActivate, Vector2 posTarget1, Vector2 posTarget2, Action PuIgnite)
    {
        GameObject projectile1 = null;
        GameObject projectile2 = null;
        string puElement = powerUpName[0].ToString();
        switch (puElement)
        {
            case "f":
                SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.FireProjectile, false);
                projectile1 = fireProjectile1;
                projectile2 = fireProjectile2;/*
                if (inlargeProjectile)
                {
                    projectile1 = largeFireProjectile1;
                    // StartCoroutine(AnimationManager.ScaleObject(true, 2f, projectile1.transform, () => projectile1.transform.localScale = new Vector3(1f, 1f, 1f)));
                }*/
                break;
            case "i":
                SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.IceProjectile, false);
                projectile1 = iceProjectile1;
                projectile2 = iceProjectile2;
                break;
        }
        if (!puElement.Equals("w"))
        {

            if (posTarget1 == new Vector2(0, 0))
            {
                StartCoroutine(ShootProjectile(false, startingPos, projectile1, posTarget2, PuIgnite));
            }
            else if (posTarget2 == new Vector2(0, 0)) /// fm1 draw 2 choose 1 
            {
                StartCoroutine(ShootProjectile(false, startingPos, projectile1, posTarget1, PuIgnite));
            }
            else
            {
                StartCoroutine(ShootProjectile(false, startingPos, projectile1, posTarget1, null));
                StartCoroutine(ShootProjectile(true, startingPos, projectile2, posTarget2, PuIgnite));
            }
        }
        else
        {
            if (powerUpName.Equals(nameof(PowerUpStruct.PowerUpNamesEnum.wm2)))
            {
                StartCoroutine(AnimationManager.Instance.AnimateWind(powerUpName, isPlayerActivate, false, windEffect, null));
                StartCoroutine(AnimationManager.Instance.AnimateWind(powerUpName, isPlayerActivate, true, windEffect2, PuIgnite));
            }
            else
            {
                StartCoroutine(AnimationManager.Instance.AnimateWind(powerUpName, isPlayerActivate, false, windEffect, PuIgnite));
            }
        }
    }



    private IEnumerator ShootProjectile(bool delay, Vector2 startingPos, GameObject projectile, Vector2 posTarget, Action EndAction)
    {
        // yield return new WaitForFixedUpdate();
        yield return null;

        projectile.transform.position = startingPos;
        projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, CalculateAngle(startingPos, posTarget)));
        SpriteRenderer headRenderer = projectile.transform.GetChild(0).GetComponent<SpriteRenderer>();
        headRenderer.color = new Color(headRenderer.color.r, headRenderer.color.g, headRenderer.color.b, 1f);
        projectile.SetActive(true);
        if (delay)
        {
            yield return new WaitForSeconds(Values.Instance.delayBetweenProjectiles);
        }
        StartCoroutine(AnimationManager.Instance.AnimateShootProjectile(false, projectile.transform, new Vector3(posTarget.x, posTarget.y, projectile.transform.position.z),
        () => StartCoroutine(AnimationManager.Instance.AlphaAnimation(headRenderer, false, Values.Instance.puProjectileFadeOutDuration, () => projectile.SetActive(false))), EndAction));
        //   () => projectile.SetActive(false), EndAction));

    }

    public void FadeStrighterOrFlusher(bool isPlayer, bool isFlusher, bool enable, Action Reset)
    {
        GameObject target;
        if (isPlayer)
        {
            if (isFlusher)
            {
                target = pFlusher;
            }
            else
            {
                target = pStrighter;
            }
        }
        else
        {
            if (isFlusher)
            {
                target = eFlusher;
            }
            else
            {
                target = eStrighter;
            }
        }
        SpriteRenderer sp = target.GetComponent<SpriteRenderer>();
        if (enable)
        {
            target.SetActive(true);
            AnimationManager.Instance.AlphaFade(true, sp, Values.Instance.fadeFlusherDuration, Reset);
        }
        else
        {
            AnimationManager.Instance.AlphaFade(false, sp, Values.Instance.fadeFlusherDuration, () =>
           {
               target.SetActive(false);
               Reset?.Invoke();
           });
        }
    }


    public float CalculateAngle(Vector2 sourceTarget, Vector2 positionTarget)
    {
        //Vector2 sourceTarget = GetAvatarPosition(fromPlayer);

        Vector2 targ = positionTarget;

        Vector2 objectPos = sourceTarget;
        targ.x = targ.x - objectPos.x;
        targ.y = targ.y - objectPos.y;

        float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
        //  return Quaternion.Euler(new Vector3(0, 0, angle - 90));
        return angle - 90;
    }

    internal void FlipTurnIndicator(bool playerEnable)
    {
        //  turnIndicator.flipY = playerEnable;
    }


    internal void UpdateRankTextInfo(bool enable, int rank)
    {
        if (enable)
        {
            handRankText.text = ConvertHandRankToTextDescription(rank) + " +" /*ConvertWinenerRankToDamage(!dealToPlayer)*/;
        }
        handRankInfo.SetActive(enable);
    }

    internal void UpdateEnergy(bool add, int howMany)
    {
        if (add)
        {
            for (int i = 0; i < energyLeft.Length; i++)
            {

                if (howMany > 0)
                {
                    if (!energyLeft[i].Available)
                    {
                        energyLeft[i].Available = true;
                        howMany--;
                    }
                }
                else
                {
                    break;
                }
            }
        }
        else
        {
            for (int i = energyLeft.Length - 1; i >= 0; i--)
            {
                if (howMany > 0)
                {
                    if (energyLeft[i].Available)
                    {
                        energyLeft[i].Available = false;
                        howMany--;
                    }
                }
                else
                {
                    break;
                }
            }
        }
    }

    internal void DisableClickBtnReplace()
    {
        StartCoroutine(AnimationManager.Instance.Shake(btnReplaceRenderer.material, Values.Instance.disableClickShakeDuration));
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CantClick, false);
    }
    public void ClickCoinEffect(int index)
    {
        StartCoroutine(AnimationManager.Instance.Shake(coinsSpriteRend[index].material, Values.Instance.disableClickShakeDuration));
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CoinHit, false);
    }

    internal void EnableBtnReplace(bool enable)
    {
        Action btnEnable = () => BattleSystem.Instance.btnReplaceClickable = enable; ;
        float value = 0.65f;
        if (enable)
        {
            value = 0;
        }
        else
        {
            btnEnable.Invoke();
            btnEnable = null;
        }
        // Make IT more stable
        StartCoroutine(AnimationManager.Instance.UpdateValue(enable, "_GradBlend", Values.Instance.puChangeColorDisableDuration, btnReplaceRenderer.material, value, btnEnable));
    }

    public void BgFadeInColor()
    {
        StartCoroutine(AnimationManager.Instance.FadeColorSwapBlend(bgSpriteRenderer, true, Values.Instance.bgMaxValueSwapColor, Values.Instance.bgPulseColorSwapDuration));
    }

    [Button]
    internal void InitNinjaAttackAnimation(bool isPlayer, string puElement)
    {
        if (isPlayer)
        {
            playerAvatarAnimator.Play("attack_" + puElement, 0, 0f);
        }
        else
        {
            enemyAvatarAnimator.Play("attack_" + puElement, 0, 0f);
        }
    }
    internal void ShowEmojiWheel(bool enable)
    {
        for (int i = 0; i < emojis.Length; i++)
        {
            StartCoroutine(AnimationManager.Instance.AlphaAnimation(emojis[i], enable, Values.Instance.emojiMenuFadeDuration, null));
        }
        StartCoroutine(AnimationManager.Instance.AlphaAnimation(emojiSelector, enable, Values.Instance.emojiMenuFadeDuration, null));
    }

    internal IEnumerator DisplayEmoji(bool isPlayer, int id, Action coolDownEmoji)
    {
        EmojiToDisplay emojiGO;
        SpriteRenderer emojiRenderer;
        if (isPlayer)
        {
            emojiRenderer = emojiToDisplayRendererPlayer;
            emojiGO = emojiToDisplayPlayer;
            //  emojiGO.transform.parent.position = emojiStartPosPlayer.position;
        }
        else
        {
            emojiRenderer = emojiToDisplayRendererEnemy;
            emojiGO = emojiToDisplayEnemy;
            //emojiGO.transform.parent.position = emojiStartPosEnemy.position;
        }
        //  Vector3 targetPos = new Vector3(0, 1.3f, 0f);
        // emojiRenderer.sprite = emojis[id].sprite;
        if (!isPlayer)
        {
            id += 4;
        }
        emojiGO.PlayEmoji(id);
        //ADD 4 if enemy
        if (isPlayer)
        {
            yield return new WaitForSeconds(Values.Instance.emojiCoolDown);
        }
        coolDownEmoji?.Invoke();
        //StartCoroutine(AnimationManager.Instance.SmoothMove(emojiGO.transform.parent, emojiTargetPos.position, emojiGO.transform.localScale, Values.Instance.emojiStay, null, null, null, () => coolDownEmoji?.Invoke()));
        /* StartCoroutine(AnimationManager.Instance.AlphaAnimation(emojiRenderer, true, Values.Instance.emojiDisplayFadeDuration, null));
         StartCoroutine(AnimationManager.Instance.SmoothMove(emojiGO.transform, emojiGO.transform.position + targetPos, emojiGO.transform.localScale, Values.Instance.emojiStay, null, null, null,
             () => StartCoroutine(AnimationManager.Instance.AlphaAnimation(emojiRenderer, false, Values.Instance.emojiDisplayFadeDuration, () => emojiGO.PlayEmoji(-1) *//*()=> emojiToDisplayTransform.position = startingPos*//*))));
    */     // yield return new WaitForSeconds(Values.Instance.emojiStay);
           //   StartCoroutine(AnimationManager.Instance.AlphaAnimation(emojiToDisplayRenderer, false, Values.Instance.emojiDisplayFadeDuration, null));
           // yield return new WaitForSeconds(Values.Instance.emojiStay);
           //  yield return new WaitForSeconds(Values.Instance.emojiCoolDown - Values.Instance.emojiStay);

    }

    internal IEnumerator ShakeEmoji(int id, Action FadeEmojis)
    {
        StartCoroutine(AnimationManager.Instance.Shake(emojis[id].material, Values.Instance.disableClickShakeDurationForEmoji));
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CantClick, false);
        yield return new WaitForSeconds(1f);
        FadeEmojis?.Invoke();
    }

    internal IEnumerator InitSmoke(bool isPlayerActivate, bool delay, CardSlot parent, bool enable, Action Reset)
    {
        if (parent.smokeEnable && enable)
        {
            ParticleSystem ps = GameObject.Find(parent.name + "S").GetComponent<ParticleSystem>();
            StartCoroutine(FadeOutParticleSystem(ps));
        }

        if (delay)
        {
            yield return new WaitForSeconds(1f);
        }
        ParticleSystem target;

        if (isPlayerActivate)
        {
            if (parent.name.Contains("B"))
            {
                target = showSmokeBoard;
            }
            else
            {
                target = showSmokeHand;
            }
        }
        else if (parent.name.Contains("B"))
        {
            target = hideSmokeBoard;
        }
        else
        {
            target = hideSmokeHand;
        }
        if (enable)
        {
            Vector3 posCurretion = new Vector3(0, -1f, 0f);
            ParticleSystem ps = Instantiate(target, parent.transform.position, target.transform.rotation);
            ps.name = parent.name + "S";
            ps.transform.SetParent(psParent.transform, false);
            ps.transform.position = parent.transform.position + posCurretion;
            yield return new WaitForSeconds(1f);
            Reset?.Invoke();
        }
        else
        {
            ParticleSystem ps = GameObject.Find(parent.name + "S").GetComponent<ParticleSystem>();
            StartCoroutine(FadeOutParticleSystem(ps));

            // StartCoroutine(AnimationManager.Instance.FadeOutPS(ps));
        }
        parent.smokeEnable = enable;
        parent.smokeActivateByPlayer = isPlayerActivate;
    }



    /* internal void FocusOnObjectWithText(bool enable, bool isRectMask, int objectNumber, bool endByBtn)
     {
         SpriteRenderer spriteTarget;
         spriteTarget = GetSpriteTargetForTutorial(objectNumber);
         GameObject mask = tutorialMaskRing;
         if (isRectMask)
         {
             mask = tutorialMaskRect;
         }
         if (enable)
         {
             lastObjectToFocus = objectNumber;
             BattleSystem.Instance.continueTutorial = false;

             TutorialObjectsVisible(true, objectNumber == Constants.TutorialObjectEnum.puCost.GetHashCode(), isRectMask, endByBtn);

             if (!endByBtn)
             {
                 // spriteTarget.sortingOrder = 10;
             }

             mask.transform.position = spriteTarget.transform.position;

             mask.transform.localScale = new Vector2(0.01f, 0.01f);
             tutorialText.text = Constants.tutoInfo[objectNumber];
             AnimateTutoObjects(true, isRectMask, endByBtn, objectNumber == 4, null);
         }
         else
         {
             if (objectNumber == Constants.TutorialObjectEnum.pu.GetHashCode())
             {
                 tutorialMaskRect.SetActive(false);
                 FocusOnObjectWithText(true, false, Constants.TutorialObjectEnum.puCost.GetHashCode(), true);
             }
             else
             {
                 AnimateTutoObjects(false, isRectMask, endByBtn, false, () =>
                   {
                       TutorialObjectsVisible(false, false, isRectMask, false);
                       if (!endByBtn)
                       {
                           spriteTarget.sortingOrder = 0;
                       }
                       BattleSystem.Instance.continueTutorial = true;

                       switch (objectNumber)
                       {
                           case 1:
                               FocusOnObjectWithText(true, false, Constants.TutorialObjectEnum.energy.GetHashCode(), true);
                               break;
                           case 2:
                               FocusOnObjectWithText(true, true, Constants.TutorialObjectEnum.pu.GetHashCode(), false);
                               break;

                       }
                   });
             }
         }
     }

     private void AnimateTutoObjects(bool toVisible, bool isRectMask, bool endByBtn, bool jusjAddBtn, Action EndAction)
     {
         GameObject mask = tutorialMaskRing;
         float targetMaskScaleLarge = 200f;
         float targetMaskScaleSmall = 0.01f;
         if (isRectMask)
         {
             mask = tutorialMaskRect;
             targetMaskScaleLarge = 30f;
             targetMaskScaleSmall = 0.033333f;
         }
         float duration = Values.Instance.tutoObjsFadeDuration;
         Action scaleMask = () => StartCoroutine(AnimationManager.Instance.ScaleObject(targetMaskScaleLarge, duration, mask.transform, null, null));


         Action FadeAllObjects = () => AnimationManager.Instance.FadeBurnDarkScreen(darkScreenRenderer.material, toVisible, duration,() => scaleMask?.Invoke());
         FadeAllObjects += () => AnimationManager.Instance.AlphaFade(toVisible, tutorialBgText, duration, null);
         FadeAllObjects += () => StartCoroutine(AnimationManager.Instance.AlphaFontAnimation(tutorialText, toVisible, duration, null));
         if (endByBtn || (!toVisible && btnTutorial.color.a == 1))
         {
             FadeAllObjects += () => AnimationManager.Instance.AlphaFade(toVisible, btnTutorial, duration, EndAction);
         }

         if (toVisible)
         {
             if (!jusjAddBtn)
             {
                 FadeAllObjects.Invoke();
             }
             else
             {
                 mask.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
                 StartCoroutine(AnimationManager.Instance.ScaleObject(0.033333f, duration, tutorialMaskRect.transform, null, scaleMask));
             }
         }
         else
         {
             scaleMask = null;
             StartCoroutine(AnimationManager.Instance.ScaleObject(targetMaskScaleSmall, duration, mask.transform, null, FadeAllObjects));
         }

     }

     private void TutorialObjectsVisible(bool enable, bool isPuCost, bool isRectMask, bool endByBtn)
     {
         if (!isPuCost)
         {
             tutorialText.enabled = enable;
             tutorialText.gameObject.SetActive(enable);
             tutorialBgText.transform.gameObject.SetActive(enable);

         }
         if (isRectMask)
         {
             tutorialMaskRect.SetActive(enable);
         }
         else
         {
             tutorialMaskRing.SetActive(enable);
         }
         btnTutorial.transform.gameObject.SetActive(endByBtn);
     }

     private SpriteRenderer GetSpriteTargetForTutorial(int objectNumber)
     {
         switch (objectNumber)
         {
             case 0:
                 return turnBtn.GetComponent<SpriteRenderer>();
             case 1:
                 return coinsFocus;
             case 2:
                 return energyLeft[1].spriteRenderer;
             case 3:
                 return BattleSystem.Instance.puDeckUi.playerPusUi[0].spriteRenderer;
             case 4:
                 return energyCostTuto;
         }
         return null;
     }
 */
    /*   public void ContinueWithTutorial()
       {
           FocusOnObjectWithText(false, false, lastObjectToFocus, false);
       }*/
    private IEnumerator FadeOutParticleSystem(ParticleSystem ps)
    {
        Debug.LogError("Burn " + ps.name);
        var main = ps.main;
        main.startLifetime = 0;
        main.simulationSpeed = 2.5f;
        yield return new WaitForSeconds(3f);
        Destroy(ps.gameObject);
    }

    [Button]
    public void ICE()
    {
        WinParticleEffect();

    }
    internal IEnumerator StartIcenado()
    {
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Iceagedon, true);
        // icenadoPS.Play();
        GameObject ps = Instantiate(icenadoPS.gameObject);
        yield return new WaitForSeconds(3f);
        var main = ps.GetComponent<ParticleSystem>().main;
        main.startLifetime = 0;
        //yield return new WaitForSeconds(5f);
        // icenadoPS.Stop();
        Destroy(ps.gameObject, 6f);
    }

    internal IEnumerator StartArmageddon()
    {
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Armagedon, true);
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Armagedon2, true);
        GameObject ps = Instantiate(armageddonPS.gameObject);
        // armageddonPS.Play();
        yield return new WaitForSeconds(3f);
        var main = ps.GetComponent<ParticleSystem>().main;
        main.startLifetime = 0;
        // yield return new WaitForSeconds(5f);
        //  armageddonPS.Stop();
        Destroy(ps.gameObject, 6f);
    }
    internal void ShutterIce(Vector2 position)
    {
        GameObject ps = Instantiate(shutterIce.gameObject);
        ps.transform.position = position;
        Destroy(ps.gameObject, 3f);
    }

    internal void SlideRankingImgIfOpen()
    {
        if (rankImageParent.activeSelf)
        {
            SlideRankingImg();
        }
    }

    internal void DoubleFreezeEffect(SpriteRenderer spriteRenderer, Action ResetCard, Action DrawCard)
    {
        StartCoroutine(AnimationManager.Instance.DoubleFreezeEffect(spriteRenderer, () => ShutterIce(spriteRenderer.transform.position), ResetCard, DrawCard));
    }

    internal void UpdateDamage(float damage, bool dealToPlayer)
    {
        if (dealToPlayer)
        {/*
        playerHpUi.position -= new Vector3(0, damage,0);
        playerHpUi.localScale -= new Vector3(0, damage,0);*/
            StartCoroutine(AnimationManager.Instance.ScaleHp(playerHpUi, damage, ()=>ShakeCamera()));
            playerHpUi.GetComponent<Animation>().Play();
        }
        else
        {
            StartCoroutine(AnimationManager.Instance.ScaleHp(enemyHpUi, damage, () => ShakeCamera()));
            enemyHpUi.GetComponent<Animation>().Play();
        }
        Debug.Log("ge;" + damage);
    }

    internal void ActivateLifeCoins()
    {
        playerLifeUi[0].SetActive(true);
        playerLifeUi[1].SetActive(true);
        enemyLifeUi[0].SetActive(true);
        enemyLifeUi[1].SetActive(true);
    }

    internal void DoubleDamageVisual(float damage)
    {
        betBtn.DisplayDoubleDamage(damage);
    }

    internal void ShowHpDialog(float currentHp, bool isPlayer)
    {
        float posY = -2.6f;
        if (!isPlayer)
        {
            posY = 2.6f;
        }
        hpInfoCanvas.transform.position = new Vector3(hpInfoCanvas.transform.position.x, posY, hpInfoCanvas.transform.position.z);
        currentHpText.text = " " + currentHp;
        StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(hpInfoCanvas, true, Values.Instance.infoDialogFadeOutDuration, null));
    }
    public void HideHpDialog()
    {
        StartCoroutine(AnimationManager.Instance.AlphaCanvasGruop(hpInfoCanvas, false, Values.Instance.infoDialogFadeOutDuration, null));
    }

    /* internal void WhosTurnAnimation(bool isPlayer, bool yourLastTurn)
     {
         turnArrowSpriteRenderer.flipX = isPlayer;
         if (yourLastTurn)
         {
             turnArrowAnimation.Play();
         }
     }*/

    //public void EnbaleMusic

    /* internal void SlidingWinnerEyes(bool isPlayerWin)
    {
        winningEyes.transform.position = new Vector2(8f, winningEyes.transform.position.y);
        StartCoroutine(AnimationManager.SmoothMoveEyes(winningEyes.transform, 6f, null, () => StartCoroutine(SlashEffect())));
    }*/

    
    public void ShakeCamera()
    {
        cameraShakeAnimation.Play();
    }

}

