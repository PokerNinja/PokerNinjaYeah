
using StandardPokerHandEvaluator;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class BattleUI : MonoBehaviour
{

    [SerializeField] public TextMeshProUGUI playerNameText;
    [SerializeField] public TextMeshProUGUI enemyNameText;
    [SerializeField] public GameObject[] playerLifeUi;
    [SerializeField] public GameObject[] enemyLifeUi;
    [SerializeField] public PowerUpUi[] enemyPus;
    [SerializeField] public PowerUpUi[] playerPus;



    [SerializeField] public TextMeshProUGUI textWinLabel;
    [SerializeField] public int winnerPanelInterval;



    [SerializeField] public GameObject winLabel;
    [SerializeField] public GameObject handRankInfo;
    [SerializeField] public TextMeshProUGUI handRankText;

    [SerializeField] public Transform infoDialog;
    [SerializeField] public SpriteRenderer dialogSprite;
    [SerializeField] public Transform targetDialogTransform;

    [SerializeField] public GameObject rankingImg;
    [SerializeField] public SpriteRenderer darkScreenRenderer;
    [SerializeField] public SpriteRenderer cancelDarkScreenRenderer;
    [SerializeField] public TextMeshProUGUI largeText;
    [SerializeField] public GameObject largeTextGO;

    [SerializeField] public GameObject turnTextGO;
    [SerializeField] public GameObject targetTurnSymbol;

    [SerializeField] public GameObject fireProjectile1;
    [SerializeField] public GameObject fireProjectile2;
    [SerializeField] public GameObject largeFireProjectile1;
    [SerializeField] public GameObject iceProjectile1;
    [SerializeField] public GameObject iceProjectile2;
    [SerializeField] public SpriteRenderer windSpriteRenderer;

    [SerializeField] public GameObject gameOverPanel;


    [SerializeField] public GameObject playerAvatar;
    [SerializeField] public GameObject enemyAvatar;




    public GameObject cardDeckHolder;
    public GameObject puDeckHolder;

    public SpriteRenderer playerTurnIndicator;
    public SpriteRenderer enemyTurnIndicator;
    public SpriteRenderer enemyNotTurnIndicator;
    public SpriteRenderer playerLargeTurnIndicator;
    public SpriteRenderer playerLargeNotTurnIndicator;

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

    public ParticleSystem rightSlash, leftSlash, winParticle;
    public GameObject rightCard, leftCard;


    public SpriteRenderer rightCardChildSprite, leftCardChildSprite;
    public Transform rightCardTarget, leftCardTarget;

    public Transform enemyHandLocation;
    public Transform playerHandLocation;

    //Buttons

    public Material freezeMaterial;
    public TextMeshProUGUI dialogTitleUi;
    public TextMeshProUGUI dialogContentUi;
    public CanvasGroup emojisWheel;
    public SpriteRenderer emojiSelector;


    [SerializeField] private GameObject turnBtn;
    [SerializeField] private SpriteRenderer turnBtnSpriteREnderer;
    [SerializeField] private Button replaceBtn;
    //  [SerializeField] private SpriteRenderer turnIndicator;
    [SerializeField] public TextMeshProUGUI currentRankText;
    [SerializeField] public TextMeshProUGUI currentRankNumber;

    public static bool isPlayerTurn;
    private string playerName, enemyName;

    private bool sliding;
    private int lastHandRank = 10;

    public void Initialize(PlayerInfo player, PlayerInfo enemy)
    {
        InitializePlayer(player);
        InitializeEnemy(enemy);
        InitializeAnimations();
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
        enemyNameText.text = enemy.id;
        enemyName = enemy.id;
        enemyNameText.text = enemyName;
    }


    public void FreezeObject(SpriteRenderer spriteTarget, bool isToFreeze, bool isFaceDown, Action onReset, bool enableSound)
    {

        StartCoroutine(AnimationManager.Instance.FreezeEffect(isToFreeze, isFaceDown, spriteTarget, freezeMaterial, onReset));
        if (enableSound)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.PuFreeze);
        }
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
        HitEffect?.Invoke();
        LoseCoin?.Invoke();
        yield return new WaitForSeconds(0.25f);
        rightCard.SetActive(true);
        leftCard.SetActive(true);
        StartCoroutine(AnimationManager.Instance.SpinRotateValue(rightCardChildSprite, null));
        StartCoroutine(AnimationManager.Instance.SmoothMoveCardProjectile(isPlayerWin, true, rightCard.transform, rightCard.transform.localScale, Values.Instance.winningCardProjectileMoveDuration, null, () =>
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
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Slash1);
        yield return new WaitForSeconds(0.2f);
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Slash2);

    }
    public IEnumerator SlashEffect()
    {
        rightSlash.Play();
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Slash1);
        yield return new WaitForSeconds(0.1f);
        leftSlash.Play();
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Slash2);
    }
    public void WinParticleEffect()
    {
        winParticle.Play();
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

    public void EnablePlayerButtons(bool enable)
    {
        StartCoroutine(AnimationManager.Instance.AlphaAnimation(turnBtnSpriteREnderer, enable, Values.Instance.turnBtnAlphaDuration, () => turnBtn.GetComponent<Button>().interactable = enable));
        EnableBtnReplace(enable);
        /* if (BattleSystem.Instance.replacePuLeft > 0)
         {
             EnableBtnReplace(enable);
         }*/
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

            StartCoroutine(AnimationManager.Instance.ScaleObject(false, 10f, Values.Instance.hitTextScaleDuration, hitGO.transform, null, () => StartCoroutine(AnimationManager.Instance.AlphaAnimation(hitSpriteRen, false, Values.Instance.hitTextFDuration, null))));


        }
    }
    public void ShowPuInfoDialog(Vector2 startingPosition, string puName, string puDisplayName, bool isEnable, bool isBtnOn, Action OnEnd)
    {
        Vector2 targetDialog = new Vector2(startingPosition.x,startingPosition.y + 2.5f);
        if (isEnable)
        {
            infoDialog.position = new Vector2(0, -7f);
            infoDialog.localScale = new Vector2(0.1f, 0.1f);
            if (puName.Equals("replace"))
            {
                InitDialog(puDisplayName, Constants.ReplacePuInfo, isBtnOn);
            }
            else
            {
                InitDialog(puDisplayName, PowerUpStruct.Instance.GetPuInfoByName(puName), isBtnOn);
            }
            StartCoroutine(AnimationManager.Instance.ShowDialogFromPu(infoDialog, dialogSprite, startingPosition, targetDialog, OnEnd));
            //StartCoroutine(AnimationManager.Instance.ShowDialogFromPu(infoDialog, dialogSprite, startingPosition, targetDialogTransform, OnEnd));
            StartCoroutine(AnimationManager.Instance.AlphaFontAnimation(dialogContentUi, true, Values.Instance.showDialogMoveDuration, null));
        }
        else
        {
            if (dialogSprite.color.a > 0)
            {
                StartCoroutine(AnimationManager.Instance.AlphaAnimation(dialogSprite, false, Values.Instance.infoDialogFadeOutDuration, OnEnd));
                StartCoroutine(AnimationManager.Instance.AlphaFontAnimation(dialogContentUi, false, Values.Instance.infoDialogFadeOutDuration, null));
            }
        }

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

    internal void SlideRankingImg()
    {
        if (!sliding)
        {
            sliding = true;
            StartCoroutine(AnimationManager.Instance.SmoothMoveRank(rankingImg.transform, Values.Instance.rankInfoMoveDuration, () => rankingImg.GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/GameScene/Buttons/ranking_empty", typeof(Sprite)) as Sprite,
                () => rankingImg.GetComponent<SpriteRenderer>().sprite = Resources.Load("Sprites/GameScene/Buttons/ranking_full", typeof(Sprite)) as Sprite, () => sliding = false));
        }

    }


    public void EnableDarkScreen(bool enable, Action ResetSortingOrder)
    {
        darkScreenRenderer.GetComponent<BoxCollider2D>().enabled = enable;
        AnimationManager.Instance.FadeBurnDarkScreen(darkScreenRenderer.material, enable, ResetSortingOrder);
        StartCoroutine(AnimationManager.Instance.AlphaAnimation(cancelDarkScreenRenderer, enable, Values.Instance.darkScreenAlphaDuration, null));
    }

    public void EnableVisionClick(bool enable)
    {
        float targetZ = -1f;
        if (enable)
        {
            targetZ = -30f;
        }
        currentRankNumber.transform.parent.localPosition = new Vector3(currentRankNumber.transform.parent.localPosition.x, currentRankNumber.transform.parent.localPosition.y, targetZ);
    }



    public void VisionEffect(List<Card> winningCards, List<CardUi> boardCardsUi, List<CardUi> playerCardsUi)
    {


        List<CardUi> winningPlayersCards = new List<CardUi>();
        List<CardUi> cardsToGlow = new List<CardUi>();
        winningPlayersCards.AddRange(boardCardsUi);
        winningPlayersCards.AddRange(playerCardsUi);
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
        AnimationManager.Instance.VisionEffect(cardsToGlow, true);
    }


    public void SetTurnIndicator(bool isPlayerTurn, bool enable)
    {
        if (enable)
        {

            EnableTurnIndicator(-1, !isPlayerTurn, null);
            EnableTurnIndicator(2, !isPlayerTurn, null);
            EnableTurnIndicator(1, isPlayerTurn, null);
            EnableTurnIndicator(3, isPlayerTurn, null);
            if (isPlayerTurn)
            {
                EnableTurnIndicator(0, isPlayerTurn, () => EnableTurnIndicator(0, !isPlayerTurn, null));
            }
        }
        else
        {
            EnableTurnIndicator(-1, false, null);
            EnableTurnIndicator(0, false, null);
            EnableTurnIndicator(1, false, null);
            EnableTurnIndicator(2, false, null);
            EnableTurnIndicator(3, false, null);
        }

    }

    public void WhosTurnAnimation(bool isPlayer, bool yourLastTurn, bool finalMove)
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
        /* else
         {
             alphaAction = () => StartCoroutine(AnimationManager.AlphaAnimation(spriteRenderer, false, 1f, () =>
             {
                 turnTextGO.SetActive(false);
                 AnimationManager.SetAlpha(spriteRenderer, 1f);
             }));

         }*/
        endAction = () => StartCoroutine(AnimationManager.Instance.SmoothMove(turnTextGO.transform, targetTurnSymbol.transform.position, targetTurnSymbol.transform.localScale, Values.Instance.turnTextMoveDuration, null, null, null, () =>
                turnTextGO.transform.localPosition = new Vector3(turnTextGO.transform.localPosition.x, turnTextGO.transform.localPosition.y, 19.5f)));

        StartCoroutine(AnimationManager.Instance.ScaleObject(false, 11f, Values.Instance.turnTextScaleDuration, turnTextGO.transform, enableBGPulse, endAction));
    }

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
        turnTextGO.SetActive(false);
    }

    public void EnableTurnIndicator(int whatSpriteTarget, bool fadeIn, Action OnFinish)
    {
        SpriteRenderer spriteTarget = null;
        switch (whatSpriteTarget)
        {
            case -1:
                spriteTarget = playerLargeNotTurnIndicator;
                break;
            case 0:
                spriteTarget = playerLargeTurnIndicator;
                break;
            case 1:
                spriteTarget = playerTurnIndicator;
                break;
            case 2:
                spriteTarget = enemyTurnIndicator;
                break;
            case 3:
                spriteTarget = enemyNotTurnIndicator;
                break;
            default:
                break;
        }
        StartCoroutine(AnimationManager.Instance.AlphaAnimation(spriteTarget, fadeIn, Values.Instance.turnIndicatorFadeDuration, OnFinish));
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
        int currentHandRank = ConvertHandRankToTextNumber(handRank);
        if (lastHandRank != currentHandRank)
        {
            UpdateVisionColor(currentHandRank);
            currentRankNumber.text = "" + currentHandRank;
            StartCoroutine(AnimationManager.Instance.PulseSize(true, currentRankNumber.transform, 1.2f, Values.Instance.pulseDuration, false, null));
            if (lastHandRank > currentHandRank)
            {
                //SoundManager.Instance.PlaySingleSound(HAPPY);
            }
            else
            {
                //SoundManager.Instance.PlaySingleSound(SAD);
            }
        }
        lastHandRank = currentHandRank;
        // currentRankText.text = ConvertHandRankToTextDescription(handRank);
    }

    private void UpdateVisionColor(int currentHandRank)
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
            default:
                break;
        }
        return "X";
    }
    private int ConvertHandRankToTextNumber(int handRank)
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
    internal void InitProjectile(Vector2 startingPos, bool inlargeProjectile, string powerUpElement, Vector2 posTarget1, Vector2 posTarget2, Action PuIgnite)
    {
        GameObject projectile1 = null;
        GameObject projectile2 = null;
        switch (powerUpElement)
        {
            case "f":
                SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.FireProjectile);
                projectile1 = fireProjectile1;
                projectile2 = fireProjectile2;
                if (inlargeProjectile)
                {
                    projectile1 = largeFireProjectile1;
                    // StartCoroutine(AnimationManager.ScaleObject(true, 2f, projectile1.transform, () => projectile1.transform.localScale = new Vector3(1f, 1f, 1f)));
                }
                break;
            case "i":
                SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.IceProjectile);
                projectile1 = iceProjectile1;
                projectile2 = iceProjectile2;
                break;
        }
        if (!powerUpElement.Equals("w"))
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
                StartCoroutine(ShootProjectile(true, startingPos, projectile1, posTarget1, null));
                StartCoroutine(ShootProjectile(false, startingPos, projectile2, posTarget2, PuIgnite));
            }
        }
        else
        {
            StartCoroutine(AnimationManager.Instance.AnimateWind(windSpriteRenderer, PuIgnite,
                () => StartCoroutine(AnimationManager.Instance.AlphaAnimation(windSpriteRenderer, false, Values.Instance.windFadeOutDuration, null))));
        }
    }



    private IEnumerator ShootProjectile(bool delay, Vector2 startingPos, GameObject projectile, Vector2 posTarget, Action EndAction)
    {
        yield return new WaitForFixedUpdate();

        projectile.transform.position = startingPos;
        projectile.transform.rotation = Quaternion.Euler(new Vector3(0, 0, CalculateAngle(startingPos, posTarget)));
        SpriteRenderer headRenderer = projectile.transform.GetChild(0).GetComponent<SpriteRenderer>();
        headRenderer.color = new Color(headRenderer.color.r, headRenderer.color.g, headRenderer.color.b, 1f);
        projectile.SetActive(true);
        if (delay)
        {
            yield return new WaitForSeconds(0.2f);
        }
        StartCoroutine(AnimationManager.Instance.AnimateShootProjectile(false, projectile.transform, new Vector3(posTarget.x, posTarget.y, projectile.transform.position.z),
        () => StartCoroutine(AnimationManager.Instance.AlphaAnimation(headRenderer, false, Values.Instance.puProjectileFadeOutDuration, () => projectile.SetActive(false))), EndAction));
        //   () => projectile.SetActive(false), EndAction));

    }

    private Vector2 GetAvatarPosition(bool isPlayer)
    {
        if (isPlayer)
        {
            return playerAvatar.transform.position;
        }
        else
        {
            return enemyAvatar.transform.position;
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
            handRankText.text = ConvertHandRankToTextDescription(rank);
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
        StartCoroutine(AnimationManager.Instance.Shake(btnReplaceRenderer.material));
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CantClick);
    }
    public void ClickCoinEffect(int index)
    {
        StartCoroutine(AnimationManager.Instance.Shake(coinsSpriteRend[index].material));
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CoinHit);
    }

    internal void EnableBtnReplace(bool enable)
    {
        float value = 0.65f;
        if (enable)
        {
            value = 0;
        }
        StartCoroutine(AnimationManager.Instance.UpdateValue(enable, "_GradBlend", Values.Instance.puChangeColorDisableDuration, btnReplaceRenderer.material, value, () => BattleSystem.Instance.btnReplaceClickable = enable));
    }

    public void BgFadeInColor()
    {
        StartCoroutine(AnimationManager.Instance.FadeColorSwapBlend(bgSpriteRenderer,true,Values.Instance.bgMaxValueSwapColor,Values.Instance.bgPulseColorSwapDuration));
    }

    internal void ShowEmojiWheel(bool enable)
    {
        StartCoroutine(AnimationManager.Instance.FadeCanvasGroup(emojisWheel, enable,Values.Instance.emojiWheelFadeDuration));
        StartCoroutine(AnimationManager.Instance.AlphaAnimation(emojiSelector, enable,Values.Instance.emojiWheelFadeDuration,null));
    }


    /* internal void SlidingWinnerEyes(bool isPlayerWin)
    {
        winningEyes.transform.position = new Vector2(8f, winningEyes.transform.position.y);
        StartCoroutine(AnimationManager.SmoothMoveEyes(winningEyes.transform, 6f, null, () => StartCoroutine(SlashEffect())));
    }*/

}

