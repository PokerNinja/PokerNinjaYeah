using Sirenix.OdinInspector;
using StandardPokerHandEvaluator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CardsDeckUi : MonoBehaviour, IPointerDownHandler
{

    [SerializeField] public Transform cardTransform;
    private List<Card> EnemyHand;
    private List<Card> playerHand;
    private Deck deck;
    public bool isPlayerFirst { set; get; }
    public List<Card> boardCards { get; set; }
    public Card ghostCard;
    private static CardsDeckUi deckUi;
    private CardSlot[] boardParents;

    private Vector3 cardHandVector, cardVectorBoard, cardVectorGhostBoard;

    ObjectPooler objectPooler;




    public CardSlot playerCardAParent;
    public CardSlot playerCardBParent;
    public CardSlot enemyCardAParent;
    public CardSlot enemyCardBParent;
    public CardSlot deckCardAParent;
    public CardSlot deckCardBParent;

    public CardSlot flopAParent;
    public CardSlot flopBParent;
    public CardSlot flopCParent;
    public CardSlot turnParent;
    public CardSlot riverParent;
    public CardSlot[] allCardSlots;
    public CardSlot[] ghostParents;

    public List<CardUi> playerCardsUi;
    public List<CardUi> enemyCardsUi;
    public List<CardUi> boardCardsUi;
    public List<CardUi> extraDeckCardsUi;
    public CardUi ghostCardUi;
    public Material burnMaterial;
    public Material dissolveMaterial;
    public Material ghostMaterial;
    public Material glitchMaterial;
    private PokerHandRankingTable poker;

    #region Settings

    public static CardsDeckUi Instance()
    {
        if (!deckUi)
        {
            deckUi = FindObjectOfType(typeof(CardsDeckUi)) as CardsDeckUi;
        }
        return deckUi;
    }


    private void Start()
    {
        objectPooler = ObjectPooler.Instance;
        AddPhysics2DRaycaster();
        boardParents = new CardSlot[] { flopAParent, flopBParent, flopCParent, turnParent, riverParent };
        cardHandVector = new Vector3(1f, 1f, 1f);
        cardVectorBoard = new Vector3(0.75f, 0.75f, 0.75f);
        cardVectorGhostBoard = new Vector3(0.6f, 0.6f, 0.6f);
        poker = new PokerHandRankingTable();
        allCardSlots = new CardSlot[] { playerCardAParent, playerCardBParent, enemyCardAParent, enemyCardBParent, flopAParent, flopBParent, flopCParent, turnParent, riverParent };
    }

    private Card cardToSave;
    private SuitEnum targetSiut;
    internal Hand ReplaceCardToFlusher(Hand hand)
    {
        List<Card> cards = hand.getCards();
        handToPrint(cards);
        cards = cards.OrderBy(h => h.CardSuit).ToList<Card>();
        handToPrint(cards);

        for (int i = 0; i < 5; i++)
        {
            Debug.LogWarning("card: " + cards[i].ToString(CardToStringFormatEnum.ShortCardName));
            if (cards[i].CardSuit != highestSuitForFlusher)
            {
                /*cards[i] = GetNewSuitCard(highestSuitForFlusher, cards, cards[i]);
                ReplaceUiCardForFlusher(cardToSave.ToString(CardToStringFormatEnum.ShortCardName), cards[i].ToString(CardToStringFormatEnum.ShortCardName));*/
                Card newCard = GetNewSuitCard(highestSuitForFlusher, cards, cards[i]);
                ReplaceUiCard(cards[i].ToString(CardToStringFormatEnum.ShortCardName), newCard.ToString(CardToStringFormatEnum.ShortCardName));
                cards[i] = newCard;
            }
        }
        return ConvertCardListToHand(cards);
    }

    private void ReplaceUiCard(string oldCard, string newCard)
    {
        Debug.LogError("o " + oldCard);
        Debug.LogError("n " + newCard);
        CardUi targetCardUi = GetCardUiByDescription(oldCard);
        targetCardUi.cardDescription = newCard;
        targetCardUi.LoadNewFlusherSprite();
    }
    internal Hand ReplaceCardToStrighter(Hand hand)
    {
        List<Card> cards = hand.getCards();
        handToPrint(cards);
        bool firstCardToRemove = false;
        int counter = 0;
        //cards = cards.OrderBy(h => h.CardValue).ToList<Card>();
        //handToPrint(cards);
        if (IsBabyStrightFourCards(cards))
        {
            if (cards[0].CardValue == CardEnum.Three)
            {
                cards[3] = UpdateCardForStrighter(cards[3], cards[0]);
            }
            else // First is Two
            {
                for (int i = 0; i < 3; i++)
                {
                    if (cards[i].GetCardValueInSimpleInt() + 1 != cards[i + 1].GetCardValueInSimpleInt())
                    {
                        if (i == 2)
                        {
                            cards[3] = UpdateCardForStrighter(cards[3], new Card(CardEnum.Six, cards[4].CardSuit));
                        }
                        else
                        {
                            cards[3] = UpdateCardForStrighter(cards[3], cards[i + 1]);
                            i = 3;
                        }
                    }
                }
            }
        }
        else
        {
            for (int i = 0; i < 5; i++)
            {
                if (!firstCardToRemove && counter == 3)
                {
                    cards[4] = UpdateCardForStrighter(cards[4], cards[0]);
                    i = 5;
                }
                else if (firstCardToRemove && counter == 3)
                {
                    cards[0] = UpdateCardForStrighter(cards[0], cards[1]);
                    i = 5;
                }
                else if (cards[i].GetCardValueInSimpleInt() + 1 == cards[i + 1].GetCardValueInSimpleInt())
                {
                    counter++;
                }
                else if (cards[i].GetCardValueInSimpleInt() + 2 == cards[i + 1].GetCardValueInSimpleInt())
                {
                    /*cardToReplace = cards[4].ToString(CardToStringFormatEnum.ShortCardName);
                    cards[4] = CreateLowerValueCard(cards[i + 1]);
                    ReplaceUiCardForFlusher(cardToReplace, cards[4].ToString(CardToStringFormatEnum.ShortCardName));*/
                    cards[4] = UpdateCardForStrighter(cards[4], cards[i + 1]);
                    i = 5;
                }
                else // First card Off
                {
                    firstCardToRemove = true;
                }
            }
        }
        return ConvertCardListToHand(cards);
    }

    private Card UpdateCardForStrighter(Card cardToReplace, Card cardTarget)
    {
        Card newCard = CreateLowerValueCard(cardTarget);
        ReplaceUiCard(cardToReplace.ToString(CardToStringFormatEnum.ShortCardName), newCard.ToString(CardToStringFormatEnum.ShortCardName));
        return newCard;
    }

    private Card CreateLowerValueCard(Card card)
    {
        SuitEnum newSuit = SuitEnum.Hearts;
        if (card.CardSuit == SuitEnum.Hearts)
        {
            newSuit = SuitEnum.Spades;
        }
        return new Card(card.GetLowerValuer(), newSuit);
    }



    private void ReplaceUiCardForStrighter(string oldCard, string newCard)
    {
        Debug.LogError("o " + oldCard);
        Debug.LogError("n " + newCard);
        CardUi targetCardUi = GetCardUiByDescription(oldCard);
        targetCardUi.cardDescription = newCard;
        targetCardUi.LoadNewFlusherSprite();
    }

    private Card GetNewSuitCard(SuitEnum targetSiut, List<Card> cards, Card cardToRemove)
    {
        List<Card> newCards = new List<Card>();
        List<Card> newCardsOptions = new List<Card>();
        newCardsOptions = InitCardsOptionsFlusher(targetSiut);
        foreach (Card card in cards)
        {
            if (card.CardSuit == targetSiut)
            {
                newCards.Add(card);
            }
        }
        newCards = newCards.OrderBy(h => h.CardValue).ToList<Card>();
        foreach (Card card in newCardsOptions)
        {
            if (!CheckIfChanceForSfOrDuplicate(card, cards))
            {
                return card;
            }
        }

        return new Card(CardEnum.Nine, targetSiut);
    }

    private bool CheckIfChanceForSfOrDuplicate(Card card, List<Card> cards)
    {
        if (cards.Contains(card))
        {
            return true;
        }
        else
        {
            cards.Add(card);
            Hand hand = new Hand(cards[0], cards[1], cards[2], cards[3], cards[4], poker);
            if (hand.Rank <= 10)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }

    private List<Card> InitCardsOptionsFlusher(SuitEnum targetSiut)
    {
        List<Card> newCardsOptions = new List<Card>();
        newCardsOptions.Add(new Card(CardEnum.Two, targetSiut));
        newCardsOptions.Add(new Card(CardEnum.Three, targetSiut));
        newCardsOptions.Add(new Card(CardEnum.Four, targetSiut));
        newCardsOptions.Add(new Card(CardEnum.Five, targetSiut));
        newCardsOptions.Add(new Card(CardEnum.Six, targetSiut));
        newCardsOptions.Add(new Card(CardEnum.Seven, targetSiut));
        newCardsOptions.Add(new Card(CardEnum.Eight, targetSiut));
        return newCardsOptions;
    }

    internal void InitDeckFromServer(string[] deckFromDB)
    {
        deck = new Deck();
        Card card;
        foreach (string cardStr in deckFromDB)
        {
            card = Card.StringToCard(cardStr);
            deck.Push(card);
        }
    }

    internal void ResetExtraDeckCards()
    {
        DestroyCardObject(extraDeckCardsUi[1].cardPlace, null);
        DestroyCardObject(extraDeckCardsUi[0].cardPlace, () => extraDeckCardsUi = new List<CardUi>());

    }

    public IEnumerator CreateHands(Action WaitForCloseDrawerAnimation, Action FinishCallback)
    {
        EnemyHand = new List<Card>();
        playerHand = new List<Card>();
        yield return new WaitForSeconds(1f);
        AnimateDrawer(true, () => StartCoroutine(DealCardsToHands(WaitForCloseDrawerAnimation, FinishCallback)));

    }


    public void InitBoardAndDeal(Action WaitForDrawerToClose, Action FinishCallback)
    {
        boardCards = new List<Card>();
        DealCardsForBoard(false, WaitForDrawerToClose, FinishCallback);
    }

    public void DealCardsForBoard(bool openDrawer, Action StartCloseDrawer, Action FinishCallback)
    {
        if (openDrawer)
        {
            AnimateDrawer(true, () => StartCoroutine(DealCardsForBoardRoutine(FinishCallback, () => AnimateDrawer(false, StartCloseDrawer))));
        }
        else
        {
            StartCoroutine(DealCardsForBoardRoutine(FinishCallback, () => AnimateDrawer(false, StartCloseDrawer)));
        }
    }


    private IEnumerator DealCardsToHands(Action WaitForCloseDrawerAnimation, Action FinishCallback)
    {
        float delayBetweenDealHandsCards = Values.Instance.delayBetweenDealPlayersCards;
        float delayBetweenHandsAndFlopDeal = Values.Instance.delayBetweenHandsAndFlopDeal;

        CardSlot[] parentForHands = CreateParentForHands();
        string[] cardName = CreateCardNamesForHands();
        bool dealToPlayer = isPlayerFirst;
        for (int i = 0; i < 4; i++)
        {
            Card playerCard = deck.Pop();
            if (dealToPlayer)
            {
                playerHand.Add(playerCard);
            }
            else
            {
                EnemyHand.Add(playerCard);
            }
            dealToPlayer = !dealToPlayer;
            CardCreatorUi(playerCard, dealToPlayer, false, parentForHands[i], cardName[i], null, false, -1);
            yield return new WaitForSecondsRealtime(delayBetweenDealHandsCards);
            if (i == 3)
            {
                yield return new WaitForSecondsRealtime(delayBetweenHandsAndFlopDeal);
                InitBoardAndDeal(WaitForCloseDrawerAnimation, FinishCallback);

            }
        }
        yield break;

    }

    private IEnumerator DealCardsForBoardRoutine(Action FinishCallback, Action closeDrawer)
    {
        float delayBetweenDealBoardCards = Values.Instance.delayBetweenDealBoardCards;
        Card newCard;
        int indexCard = boardCards.Count;
        bool keepDealingForBoard = true;
        while (keepDealingForBoard)
        {
            newCard = deck.Pop();
            boardCards.Add(newCard);

            CardCreatorUi(newCard, false, false, boardParents[indexCard], Constants.BoardCards[indexCard], null, false, -1);
            yield return new WaitForSecondsRealtime(delayBetweenDealBoardCards);
            indexCard++;
            if (indexCard == 3 || indexCard == 4 || indexCard == 5)
            {
                yield return new WaitForSecondsRealtime(1.5f);
                keepDealingForBoard = false;
                FinishCallback();
                closeDrawer?.Invoke();
            }

        }
        yield break;
    }
    public CardUi GetCardUiByName(string cardTarget2)
    {
        List<CardUi> allCardsUi = playerCardsUi.Concat(enemyCardsUi).Concat(boardCardsUi).ToList();
        for (int i = 0; i < allCardsUi.Count; i++)
        {
            if (allCardsUi[i].cardPlace.Equals(cardTarget2))
            {
                return allCardsUi[i];
            }
        }
        return null;
    }
    private CardUi GetCardUiByDescription(string cardDesc)
    {
        List<CardUi> allCardsUi = playerCardsUi.Concat(enemyCardsUi).Concat(boardCardsUi).ToList();
        for (int i = 0; i < allCardsUi.Count; i++)
        {
            if (allCardsUi[i].cardDescription.Equals(cardDesc))
            {
                return allCardsUi[i];
            }
        }
        return null;
    }



    private void RemoveFromList(List<CardUi> listToRemoveFrom, CardUi cardToDestroy)
    {
        listToRemoveFrom.Remove(cardToDestroy);

    }

    private void RestAfterDestroy(CardUi cardToDestroy, Action OnEnd)
    {
        ResetCardUI(cardToDestroy);
        OnEnd?.Invoke();
    }

    private void ResetCardUI(CardUi cardToReset)
    {
        cardToReset.name = "CardUII";
        if (cardToReset.freeze)
        {
            cardToReset.freeze = false;
            cardToReset.spriteRenderer.material.SetColor("_FadeBurnColor", Color.yellow);
        }
        cardToReset.transform.position = cardTransform.position;
        cardToReset.transform.localScale = cardTransform.localScale;
        cardToReset.transform.SetParent(objectPooler.transform);
        // cardToReset.tag = "Card";
        cardToReset.InitCardsTag(Constants.PoolCardTag);
        if (cardToReset.underSmoke)
        {
            EnableCardSmoke(false, true, cardToReset);
        }
        objectPooler.ReturnCard(cardToReset);
    }

    private void UpdateCardsList(string cardPlace, Card newCard, bool isAddToList)
    {

        switch (cardPlace)
        {
            case Constants.PlayerCard1:
                playerHand = UpdateCardsByIndex(playerHand, 0, newCard, true);
                break;
            case Constants.PlayerCard2:
                playerHand = UpdateCardsByIndex(playerHand, 1, newCard, true);
                break;
            case Constants.EnemyCard1:
                EnemyHand = UpdateCardsByIndex(EnemyHand, 0, newCard, true);
                break;
            case Constants.EnemyCard2:
                EnemyHand = UpdateCardsByIndex(EnemyHand, 1, newCard, true);
                break;
            default:
                boardCards = UpdateCardsByIndex(boardCards, ConvertCardPlaceToIndex(cardPlace), newCard, true);
                break;
        }

    }


    private int ConvertCardPlaceToIndex(string cardPlace)
    {
        if (cardPlace.Equals(Constants.PlayerCard1) || cardPlace.Equals(Constants.EnemyCard1) || cardPlace.Equals(Constants.BoardCards[0]))
        {
            return 0;
        }
        if (cardPlace.Equals(Constants.PlayerCard2) || cardPlace.Equals(Constants.EnemyCard2) || cardPlace.Equals(Constants.BoardCards[1]))
        {
            return 1;
        }
        if (cardPlace.Equals(Constants.BoardCards[2]))
        {
            return 2;
        }
        if (cardPlace.Equals(Constants.BoardCards[3]))
        {
            return 3;
        }
        if (cardPlace.Equals(Constants.BoardCards[4]))
        {
            return 4;
        }
        return -1;
    }

    private List<Card> UpdateCardsByIndex(List<Card> handToUpdate, int index, Card newCard, bool toDelete)
    {
        if (handToUpdate != null)
        {
            handToUpdate[index] = newCard;
            return handToUpdate;
        }
        return null;
    }

    public List<CardUi> GetListByTag(string tag)
    {
        switch (tag)
        {
            case Constants.PlayerCardsTag:
                {
                    return playerCardsUi;

                }
            case Constants.EnemyCardsTag:
                {
                    return enemyCardsUi;
                }
            case Constants.BoardCardsTag:
                {
                    return boardCardsUi;
                }
            case Constants.DeckCardsTag:
                {
                    return extraDeckCardsUi;
                }
            case Constants.AllCardsTag:
                {
                    List<CardUi> combinedList = new List<CardUi>();
                    combinedList.AddRange(playerCardsUi);
                    combinedList.AddRange(enemyCardsUi);
                    combinedList.AddRange(boardCardsUi);
                    return combinedList;
                }
        }
        return null;
    }

    private int GetIndexOfCard(CardUi cardSwap1)
    {
        if (cardSwap1.name.Contains("1"))
        {
            return 0;
        }
        else if (cardSwap1.name.Contains("2"))
        {
            return 1;
        }
        else if (cardSwap1.name.Contains("3"))
        {
            return 2;
        }
        else if (cardSwap1.name.Contains("4"))
        {
            return 3;
        }
        else if (cardSwap1.name.Contains("5"))
        {
            return 4;
        }
        return -1;
    }

    private void ResetUiLists()
    {
        playerCardsUi = new List<CardUi>();
        enemyCardsUi = new List<CardUi>();
        boardCardsUi = new List<CardUi>();
        extraDeckCardsUi = new List<CardUi>();
    }

    private IEnumerable<CardUi> FindAllCardsObjects()
    {
        return FindObjectsOfType<CardUi>();
    }


    public string CardPlaceToTag(string cardPlace)
    {
        if (cardPlace == "All")
        {
            return Constants.AllCardsTag;
        }
        if (cardPlace.Contains("Player"))
        {
            return Constants.PlayerCardsTag;
        }
        else if (cardPlace.Contains("Enemy"))
        {
            return Constants.EnemyCardsTag;

        }
        else if (cardPlace.Contains("Deck"))
        {
            return Constants.DeckCardsTag;

        }
        else
        {
            foreach (string cardName in Constants.BoardCards)
            {
                if (cardPlace.Contains(cardName))
                {
                    return Constants.BoardCardsTag;

                }
            }
        }
        return cardPlace;
    }

    internal List<CardUi> GetBoardAndPlayerHandList()
    {
        List<CardUi> playerHandWithBoard = new List<CardUi>();
        playerHandWithBoard.AddRange(boardCardsUi);
        playerHandWithBoard.AddRange(playerCardsUi);
        if (ghostCardUi != null && !ghostCardUi.cardPlace.Contains("Enemy"))
        {
            playerHandWithBoard.Add(ghostCardUi);
        }
        return playerHandWithBoard;
    }

    private void CardCreatorUi(Card newCard, bool isFaceDown, bool aboveDarkScreen, CardSlot cardParent, string cardPlace, Action disableDarkScreen, bool isCloseDrawer, int indexToInsert)
    {
        Action DarkCardUnderSmoke = null;
        Vector3 cardScale;
        // TODO add list value to catch the object and store it
        string cardTag = "Card" + cardPlace.Substring(0, 1);
        CardUi cardObject = objectPooler.SpwanCardFromPool(cardTag);
        cardScale = cardHandVector;
        cardObject.transform.SetParent(cardParent.transform);
        if (cardParent.name.Contains("B") || indexToInsert == -10)
        {
            if (cardParent.name.Contains("B") && indexToInsert == -10)
            {
                cardScale = cardVectorGhostBoard;
                Debug.LogError("ghostboat");
            }
            else
            {
                cardScale = cardVectorBoard;
            }
        }
        Debug.LogError("cardParent.transform.parent.name" + cardParent.transform.parent.name);

        cardObject.name = cardPlace;
        cardObject.transform.position = new Vector3(cardTransform.position.x, cardTransform.position.y, 1); ;
        cardObject.transform.localScale = cardTransform.localScale;
        cardObject.Init(cardTag, newCard.ToString(CardToStringFormatEnum.ShortCardName), isFaceDown, aboveDarkScreen, cardPlace);
        if (cardParent.smokeEnable)
        {
            DarkCardUnderSmoke = () => EnableCardSmoke(true, cardParent.smokeActivateByPlayer, cardObject);
        }
        Vector3 targetPosition = new Vector3(cardParent.transform.position.x, cardParent.transform.position.y, cardObject.transform.position.z);
        Action closeDrawer = null;
        if (isCloseDrawer)
        {
            closeDrawer = () => AnimateDrawer(false, null);
        }
        if (indexToInsert == -10)
        {
            cardObject.transform.position = targetPosition;
            cardObject.transform.localScale = cardScale;
            cardObject.isGhost = true;
            cardObject.spriteRenderer.material = ghostMaterial;
            cardObject.LoadSprite(true);
            AddCardToList(cardTag, cardObject, indexToInsert);

            StartCoroutine(cardObject.FadeGhost(true, disableDarkScreen));
        }
        else
        {
            cardObject.spriteRenderer.material = dissolveMaterial;

            StartCoroutine(AnimationManager.Instance.SmoothMove(cardObject.transform, targetPosition, cardScale,
        Values.Instance.cardDrawMoveDuration, null, () =>
        {
            DarkCardUnderSmoke?.Invoke();
            cardObject.CardReveal(!isFaceDown);
        }
           , disableDarkScreen, () =>
        {
            closeDrawer?.Invoke();
            AddCardToList(cardTag, cardObject, indexToInsert);
        }));
        }

    }

    private void AddCardToList(string cardTag, CardUi cardObject, int indexToInsert)
    {
        if (indexToInsert == -10)
        {
            ghostCardUi = cardObject;
        }
        else if (indexToInsert == -1 || GetListByTag(cardTag).Count < indexToInsert)
        {
            GetListByTag(cardTag).Add(cardObject);
        }
        else
        {
            GetListByTag(cardTag).Insert(indexToInsert, cardObject);
        }
    }



    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.pointerCurrentRaycast.gameObject.name == "Deck")
        {
            // gameController.EndTurn();
        }
    }



    private void AddPhysics2DRaycaster()
    {
        Physics2DRaycaster physicsRaycaster = FindObjectOfType<Physics2DRaycaster>();
        if (physicsRaycaster == null)
        {
            Camera.main.gameObject.AddComponent<Physics2DRaycaster>();
        }
    }

    public Hand CalculateHand(bool isPlayer, bool isFlusher, bool isStrighter)
    {
        TexasHand playerCards;
        if (isPlayer)
        {
            playerCards = GetPlayerHand();
            BattleSystem.Instance.playerHandIsStrighter = false;
            BattleSystem.Instance.playerHandIsFlusher = false;
        }
        else
        {
            playerCards = GetEnemyHand();
            BattleSystem.Instance.enemyHandIsFlusher = false;
            BattleSystem.Instance.enemyHandIsFlusher = false;
        }

        List<Card> totalCards = new List<Card>();
        totalCards.AddRange(playerCards.getCards());
        totalCards.AddRange(boardCards);
        if (ghostCardUi != null)
        {
            if (isPlayer && ghostCardUi.whosCards == Constants.CardsOwener.Player)
            {
                totalCards.Add(ghostCard);
            }
            else if (!isPlayer && ghostCardUi.whosCards == Constants.CardsOwener.Enemy)
            {
                totalCards.Add(ghostCard);
            }
            else if (ghostCardUi.whosCards == Constants.CardsOwener.Board)
            {
                totalCards.Add(ghostCard);
            }
        }
        if (totalCards.Count == 5)
        {
            Hand hand = new Hand(totalCards[0], totalCards[1], totalCards[2], totalCards[3], totalCards[4], poker);
            if (isFlusher && hand.Rank > 1600)
            {
                if (isPlayer)
                {
                    BattleSystem.Instance.playerHandIsFlusher = IsHandWithFourSameSuit(hand);
                }
                else
                {
                    BattleSystem.Instance.enemyHandIsFlusher = IsHandWithFourSameSuit(hand);
                }
            }
            else if (isStrighter && hand.Rank > 1609)
            {
                if (isPlayer)
                {
                    BattleSystem.Instance.playerHandIsStrighter = IsHandWithFourStright(hand.getCards());
                }
                else
                {
                    BattleSystem.Instance.enemyHandIsStrighter = IsHandWithFourStright(hand.getCards());
                }
            }
            return hand;
        }
        List<Hand> handsOptions = GetBestHand(totalCards);

        if (isFlusher && handsOptions[0].Rank > 1600)
        {
            Hand newHand = CheckIfFlusher(isPlayer, handsOptions);
            handsOptions.Insert(0, newHand);
        }
        else if (isStrighter && handsOptions[0].Rank > 1609)
        {
            Hand newHand = CheckIfStrighter(isPlayer, handsOptions);
            handsOptions.Insert(0, newHand);
        }
        //MAYBE SMARTER^
        return handsOptions[0];

    }

    private List<Hand> GetBestHand(List<Card> totalCards)
    {
        List<Hand> handsOptions = new List<Hand>();
        List<Card> currentHandList = new List<Card>();
        Hand currentHand;
        switch (totalCards.Count)
        {
            case 6:
                for (int i = 0; i < totalCards.Count; i++)
                {
                    currentHandList = new List<Card>(totalCards.ToList<Card>());
                    currentHandList.RemoveAt(i);
                    currentHand = new Hand(currentHandList[0], currentHandList[1], currentHandList[2], currentHandList[3], currentHandList[4], poker);
                    handsOptions.Add(currentHand);
                }
                break;
            case 7:
                for (int i = 0; i < totalCards.Count; i++)
                {
                    for (int j = i + 1; j < totalCards.Count; j++)
                    {
                        currentHandList = new List<Card>(totalCards.ToList<Card>());
                        currentHandList.RemoveAt(j);
                        currentHandList.RemoveAt(i);
                        currentHand = ConvertCardListToHand(currentHandList);
                        handsOptions.Add(currentHand);
                    }
                }
                break;
            case 8:
                for (int g = 0; g < totalCards.Count; g++)
                {
                    for (int i = g + 1; i < totalCards.Count; i++)
                    {
                        for (int j = i + 1; j < totalCards.Count; j++)
                        {
                            currentHandList = new List<Card>(totalCards.ToList<Card>());
                            currentHandList.RemoveAt(j);
                            currentHandList.RemoveAt(i);
                            currentHandList.RemoveAt(g);
                            currentHand = ConvertCardListToHand(currentHandList);
                            handsOptions.Add(currentHand);
                        }
                    }
                }
                break;
        }
        return handsOptions.OrderBy(h => h.Rank).ToList<Hand>();
    }

    private bool IsHandWithFourStright(List<Card> cards)
    {
        int strightCounter = 0;

        //SuitEnum sameSuit = SuitEnum.Clubs;
        //  Card tempCardToSave = null;
        //   handToPrint(originCard);
        cards = cards.OrderBy(c => c.CardValue).ToList<Card>();
        //   handToPrint(cards);
        bool haveGapOfTwo = false;
        if (IsBabyStrightFourCards(cards))
        {
            /* Debug.LogError("d Baby");
             handToPrint(cards);*/
            strightCounter = 3;
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                int x = cards[i].GetCardValueInSimpleInt();
                int y = cards[i + 1].GetCardValueInSimpleInt();
                if (cards[i].GetCardValueInSimpleInt() + 1 == cards[i + 1].GetCardValueInSimpleInt())
                {
                    strightCounter++;
                }
                else if (cards[i].GetCardValueInSimpleInt() + 2 == cards[i + 1].GetCardValueInSimpleInt())
                {
                    if (!haveGapOfTwo)
                    {
                        strightCounter++;
                    }
                    else if (i != 4)
                    {
                        i = 5;
                    }
                    haveGapOfTwo = true;
                }
                else if (i == 1 || i == 2)
                {
                    i = 5;
                }

            }
        }
        if (strightCounter == 3 || haveGapOfTwo && strightCounter == 4)
        {
            return true;
        }
        return false;
    }

    private bool IsBabyStrightFourCards(List<Card> cards)
    {
        int strightCounter = 0;
        bool haveGapOfTwo = false;
        if (cards[1].CardValue != CardEnum.Six && cards[2].CardValue != CardEnum.Six && cards[3].CardValue != CardEnum.Six)
        {

            if (cards[4].CardValue == CardEnum.Ace)
            {
                if (cards[0].CardValue == CardEnum.Two || cards[0].CardValue == CardEnum.Three)
                {
                    for (int i = 0; i < 2; i++)
                    {
                        if (cards[i].GetCardValueInSimpleInt() + 1 == cards[i + 1].GetCardValueInSimpleInt())
                        {
                            strightCounter++;
                        }
                        else if (!haveGapOfTwo)
                        {
                            if (cards[i].GetCardValueInSimpleInt() + 2 == cards[i + 1].GetCardValueInSimpleInt())
                            {
                                haveGapOfTwo = true;
                                strightCounter++;
                            }
                        }
                    }

                    if (strightCounter == 2)
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private Hand ConvertCardListToHand(List<Card> cards)
    {
        return new Hand(cards[0], cards[1], cards[2], cards[3], cards[4], poker);
    }

    /* private Hand ReplaceCardForFlusher(Hand hand)
     {
         SuitEnum targetSuit = GetSuitFromHand(hand);
        Card newCardAvailable = SearchForLowestSuitInField()
     }*/

    [Button]
    public void Check2()
    {

    }
    private Hand CheckIfStrighter(bool isPlayer, List<Hand> handsOptions)
    {
        List<Hand> handsWithStrighter = new List<Hand>();
        foreach (Hand hand in handsOptions)
        {
            List<Card> cards = hand.getCards().OrderBy(c => c.CardValue).ToList<Card>();
            //handToPrint(hand.getCards());
            if (IsHandWithFourStright(cards))
            {
                handsWithStrighter.Add(ConvertCardListToHand(cards));
            }
        }

        if (handsWithStrighter.Count > 0)
        {
            if (isPlayer)
            {
                BattleSystem.Instance.playerHandIsStrighter = true;
            }
            else
            {
                BattleSystem.Instance.enemyHandIsStrighter = true;
            }
            return GetHighestStrighterFromHands(handsWithStrighter);
        }
        return handsOptions[0];
    }

    private Hand GetHighestStrighterFromHands(List<Hand> handsWithStrighter)
    {
        CardEnum cardValue = CardEnum.Two;
        Card tempCard;
        int indexOfTopHand = 0;
        for (int i = 0; i < handsWithStrighter.Count; i++)
        {
            // cards = handsWithStrighter[i].getCards().OrderBy(c => c.CardValue).ToList<Card>();
            if (IsBabyStrightFourCards(handsWithStrighter[i].getCards()))
            {
                tempCard = handsWithStrighter[i].getCards()[2];
            }
            else
            {
                tempCard = GetTopValueForStrigher(handsWithStrighter[i]);
                //  Debug.LogError("top " + i + " " + tempCard.CardValue.ToString() + tempCard.CardSuit.ToString());
            }
            if (tempCard.CardValue > cardValue)
            {
                //    Debug.LogError("highest" + i);
                cardValue = tempCard.CardValue;
                // highestSuitForFlusher = tempCard.CardSuit;
                indexOfTopHand = i;
            }
        }
        return handsWithStrighter[indexOfTopHand];
    }

    private Card GetTopValueForStrigher(Hand hand)
    {
        List<Card> cards = hand.getCards();
        int lastCard = cards[4].GetCardValueInSimpleInt();
        int preLastCard = cards[3].GetCardValueInSimpleInt();

        if (lastCard - 1 == preLastCard || lastCard - 2 == preLastCard)
        {
            return cards[3];
        }
        else
        {
            return cards[3];
        }
    }

    private Hand CheckIfFlusher(bool isPlayer, List<Hand> handsOptions)
    {
        List<Hand> handsWithFlusher = new List<Hand>();
        foreach (Hand hand in handsOptions)
        {
            if (IsHandWithFourSameSuit(hand))
            {
                handsWithFlusher.Add(hand);
            }
        }

        if (handsWithFlusher.Count > 0)
        {
            if (isPlayer)
            {
                BattleSystem.Instance.playerHandIsFlusher = true;
            }
            else
            {
                BattleSystem.Instance.enemyHandIsFlusher = true;
            }
            return GetHighestFlusherFromHands(handsWithFlusher);
        }

        return handsOptions[0];
    }



    public SuitEnum highestSuitForFlusher;
    private Hand GetHighestFlusherFromHands(List<Hand> handsWithFlusher)
    {
        CardEnum cardValue = CardEnum.Two;
        Card tempCard;
        SuitEnum cardSuit;
        int indexOfTopHand = 0;
        for (int i = 0; i < handsWithFlusher.Count; i++)
        {
            tempCard = GetTopValueForFlusher(handsWithFlusher[i]);
            //  Debug.LogError("top " + i + " " + tempCard.CardValue.ToString() + tempCard.CardSuit.ToString());
            if (tempCard.CardValue > cardValue)
            {
                //    Debug.LogError("highest" + i);
                // handToPrint(handsWithFlusher[i].getCards());
                cardValue = tempCard.CardValue;
                highestSuitForFlusher = tempCard.CardSuit;
                indexOfTopHand = i;
            }
        }
        return handsWithFlusher[indexOfTopHand];
    }

    private Card GetTopValueForFlusher(Hand hand)
    {
        int sameSuitCounter = 0;
        int indexOfRedundentCard = 0;
        SuitEnum sameSuit = SuitEnum.Clubs;
        List<Card> cards = hand.getCards().OrderBy(c => c.CardSuit).ToList<Card>();
        for (int i = 0; i < 4; i++)
        {
            if (cards[i].CardSuit == cards[i + 1].CardSuit)
            {
                if (sameSuitCounter == 0)
                {
                    sameSuit = cards[i].CardSuit;
                    //targetSiut = cards[i].CardSuit;
                    sameSuitCounter++;
                }
                else if (sameSuit == cards[i].CardSuit)
                {
                    sameSuitCounter++;
                }
            }
            else
            {
                if (i == 0)
                {
                    indexOfRedundentCard = 0;
                }
                else if (i == 3)
                {
                    indexOfRedundentCard = 4;
                }
            }
        }
        cards.RemoveAt(indexOfRedundentCard);
        return cards.OrderBy(c => c.CardValue).ToList<Card>()[3];

    }


    private bool IsHandWithFourSameSuit(Hand hand)
    {
        int sameSuitCounter = 0;
        SuitEnum sameSuit = SuitEnum.Clubs;
        List<Card> cards = hand.getCards().OrderBy(c => c.CardSuit).ToList<Card>();
        for (int i = 0; i < 4; i++)
        {
            if (cards[i].CardSuit == cards[i + 1].CardSuit)
            {
                if (sameSuitCounter == 0)
                {
                    sameSuit = cards[i].CardSuit;
                    targetSiut = cards[i].CardSuit;
                    sameSuitCounter++;
                }
                else if (sameSuit == cards[i].CardSuit)
                {
                    sameSuitCounter++;
                }
            }
        }
        if (sameSuitCounter == 3)
        {
            return true;
        }
        return false;
    }
    [Button]
    public void CheckFlush()
    {
        Card a = new Card(CardEnum.Ace, SuitEnum.Diamonds);
        Card b = new Card(CardEnum.King, SuitEnum.Hearts);
        Card c = new Card(CardEnum.Queen, SuitEnum.Diamonds);
        Card d = new Card(CardEnum.Jack, SuitEnum.Clubs);
        Card e = new Card(CardEnum.Six, SuitEnum.Diamonds);
        Card x = new Card(CardEnum.Seven, SuitEnum.Diamonds);
        Hand hand1 = new Hand(a, b, c, d, e, poker);
        Hand hand2 = new Hand(c, x, d, b, e, poker);
        Hand hand3 = new Hand(e, c, b, d, a, poker);
        List<Card> cards1 = hand1.getCards().OrderBy(c => c.CardValue).ToList<Card>();
        List<Card> cards2 = hand2.getCards().OrderBy(c => c.CardValue).ToList<Card>();
        List<Card> cards3 = hand3.getCards().OrderBy(c => c.CardValue).ToList<Card>();
        handToPrint(cards1);
        handToPrint(cards2);
        handToPrint(cards3);
    }

    internal Vector2 GetCardPosition(string cardPlace)
    {

        if (cardPlace.Contains("Deck") || cardPlace.Equals("empty"))
        {
            return new Vector2(0, 0);
        }
        else
        {
            return GetCardUiByName(cardPlace).transform.position;

        }
    }

    internal void DisableCardsSelection(string cardPlace)
    {
        foreach (CardUi card in GetListByTag(CardPlaceToTag(cardPlace)))
        {
            card.SetSelection(false, "");
        }
    }


    public CardSlot GetParentByPlace(string cardPlace)
    {
        switch (cardPlace)
        {
            case Constants.PlayerCard1:
                return playerCardAParent;
            case Constants.PlayerCard2:
                return playerCardBParent;
            case Constants.EnemyCard1:
                return enemyCardAParent;
            case Constants.EnemyCard2:
                return enemyCardBParent;
            case Constants.BFlop1:
                return boardParents[0];
            case Constants.BFlop2:
                return boardParents[1];
            case Constants.BFlop3:
                return boardParents[2];
            case Constants.BTurn4:
                return boardParents[3];
            case Constants.BRiver5:
                return boardParents[4];
            case Constants.Deck1:
                return deckCardAParent;
            case Constants.Deck2:
                return deckCardBParent;
            default:
                break;
        }
        return null;
    }

    private CardSlot[] CreateParentForHands()
    {
        CardSlot[] parents;
        if (isPlayerFirst)
        {
            parents = new CardSlot[] { playerCardAParent, enemyCardAParent, playerCardBParent, enemyCardBParent };
        }
        else
        {
            parents = new CardSlot[] { enemyCardAParent, playerCardAParent, enemyCardBParent, playerCardBParent };
        }
        return parents;
    }


    private string[] CreateCardNamesForHands()
    {
        string[] names;

        if (isPlayerFirst)
        {
            names = new string[] { Constants.PlayerCard1, Constants.EnemyCard1, Constants.PlayerCard2, Constants.EnemyCard2 };
        }
        else
        {
            names = new string[] { Constants.EnemyCard1, Constants.PlayerCard1, Constants.EnemyCard2, Constants.PlayerCard2 };
        }
        return names;

    }

    public TexasHand GetEnemyHand()
    {
        return new TexasHand(EnemyHand[0], EnemyHand[1]);
    }

    public TexasHand GetPlayerHand()
    {
        return new TexasHand(playerHand[0], playerHand[1]);
    }
    #endregion

    #region Pu Functions


    public void GhostCardActivate(Constants.CardsOwener cardsOwener, Action UpdateRank, Action Reset)
    {
        if (ghostCardUi != null)
        {
            DestroyCardObject(ghostCardUi.cardPlace, () => AddGhostCard(cardsOwener, UpdateRank, Reset));
        }
        else
        {
            AddGhostCard(cardsOwener, UpdateRank, Reset);
        }
    }

    internal void EnableCardSmoke(bool enable, bool isPlayerActivate, CardUi targetCard)
    {
        if (targetCard != null)
        {

            targetCard.underSmoke = enable;
            if (!isPlayerActivate)
            {
                targetCard.spriteRenderer.color = Color.black;
            }
            if (!enable)
            {
                targetCard.spriteRenderer.color = Color.white;
            }
        }
    }
    internal void AddGhostCard(Constants.CardsOwener cardsOwener, Action UpdateRank, Action Reset)
    {

        Card newCard = deck.Pop();
        ghostCard = newCard;
        int index = 0;
        switch (cardsOwener)
        {
            case Constants.CardsOwener.Player:
                //  playerHand.Add(newCard);
                index = 0;
                break;
            case Constants.CardsOwener.Enemy:
                // EnemyHand.Add(newCard);
                index = 1;
                break;
            case Constants.CardsOwener.Board:
                //   boardCards.Add(newCard);
                index = 2;
                break;
        }
        CardCreatorUi(newCard, false, true, ghostParents[index], Constants.ghostCardsNames[index], UpdateRank += Reset, true, -10);
    }

    internal IEnumerator Draw2Cards(bool isEnemy, Action endAction)
    {
        float delayBetweenDealBoardCards = Values.Instance.delayBetweenDealBoardCards;
        CardCreatorUi(deck.Pop(), isEnemy, true, deckCardAParent, Constants.deckCardsNames[0], null, false, -1);
        yield return new WaitForSecondsRealtime(delayBetweenDealBoardCards);
        CardCreatorUi(deck.Pop(), isEnemy, true, deckCardBParent, Constants.deckCardsNames[1], null, true, -1);
        yield return new WaitForSecondsRealtime(1f);
        if (isEnemy)
        {
            yield return new WaitForSecondsRealtime(0.9f);
        }
        else
        {
            extraDeckCardsUi[0].SetSelection(true, "");
            extraDeckCardsUi[1].SetSelection(true, "");
        }
        endAction?.Invoke();
    }

    internal void Draw2CardsWithDrawer(bool isEnemy, Action endAction)
    {
        AnimateDrawer(true, () => StartCoroutine(Draw2Cards(isEnemy, endAction)));
    }

    internal void FlipCardPu(string cardTarget2, bool isPlayerActivate, Action endAction)
    {

        CardUi cardToSee = GetCardUiByName(cardTarget2);
        if (cardToSee.whosCards.Equals(Constants.CardsOwener.Board))
        {
            if (isPlayerActivate)
            {
                if (cardToSee.GetisFaceDown())
                {
                    cardToSee.FlipCard(true, () => cardToSee.ApplyEyeEffect(endAction, true, false));
                }
                else
                {
                    cardToSee.ApplyEyeEffect(endAction, false, true);
                }
            }
            else
            {
                cardToSee.FlipCard(cardToSee.GetisFaceDown(), () => cardToSee.ApplyEyeEffect(endAction, true, false));
            }
        }
        if (cardToSee.whosCards.Equals(Constants.CardsOwener.Player))
        {
            cardToSee.ApplyEyeEffect(endAction, cardToSee.cardMark.activeSelf, false);
        }
        if (cardToSee.whosCards.Equals(Constants.CardsOwener.Enemy))
        {
            cardToSee.FlipCard(cardToSee.GetisFaceDown(), () => cardToSee.ApplyEyeEffect(endAction, cardToSee.GetisFaceDown(), false));

        }
    }


    internal void DestroyCardObject(string cardPlace, Action OnEnd)
    {
        DestroyWithDelay(cardPlace, OnEnd);
    }

    private void DestroyWithDelay(string cardPlace, Action OnEnd)
    {
        CardUi cardToDestroy = GameObject.Find(cardPlace).GetComponent<CardUi>();
        RemoveFromList(GetListByTag(cardToDestroy.tag), cardToDestroy);
        //boardCardsUi.Remove(cardToDestroy); //TODO why this
        if (cardToDestroy == null)
        {
            Debug.LogError("Destroy: fck");
        }
        if (!cardPlace.Contains("Deck"))
        {
            if (cardToDestroy.cardMark.activeSelf)
            {
                cardToDestroy.cardMark.SetActive(false);
            }
            Material targetMaterial = burnMaterial;
            bool changeOffset = true;
            if (cardPlace.Contains("Ghost"))
            {
                targetMaterial = ghostMaterial;
                changeOffset = false;
            }
            StartCoroutine(cardToDestroy.FadeBurnOut(targetMaterial, changeOffset, () =>
          RestAfterDestroy(cardToDestroy, OnEnd)));
        }
        else
        {
            StartCoroutine(cardToDestroy.Dissolve(dissolveMaterial, 0f, () =>
                     RestAfterDestroy(cardToDestroy, OnEnd)));
        }
    }




    internal void DrawAndReplaceCard(string cardPlace, bool isFlip, Action disableDarkScreen, bool isFirstCard, bool isLastCard)
    {

        Card newCard = deck.Pop();
        UpdateCardsList(cardPlace, newCard, true);
        int indexToInsert = ConvertCardPlaceToIndex(cardPlace);
        if (isFirstCard)
        {
            AnimateDrawer(true, () => CardCreatorUi(newCard, isFlip, true, GetParentByPlace(cardPlace), cardPlace, disableDarkScreen, isLastCard, indexToInsert));
        }
        else
        {
            CardCreatorUi(newCard, isFlip, true, GetParentByPlace(cardPlace), cardPlace, disableDarkScreen, isLastCard, indexToInsert);
        }
    }

    internal void UpdateCardValue(string cardTarget, int value, Action disableDarkScreen)
    {
        CardUi pickedCardUi = GetCardUiByName(cardTarget);
        Card pickedCard = Card.StringToCard(pickedCardUi.cardDescription);
        Card newCard = new Card(pickedCard.ConvertIntToValue(pickedCard.GetCardValueInSimpleInt() + value), pickedCard.CardSuit);
        CardUi targetCardUi = GetCardUiByDescription(newCard.ToString(CardToStringFormatEnum.ShortCardName));
        if (targetCardUi != null)
        {
            targetCardUi.cardDescription = pickedCard.ToString(CardToStringFormatEnum.ShortCardName);
            StartCoroutine(GlitchEffect(targetCardUi, targetCardUi.GetisFaceDown(), null));
            UpdateCardsList(targetCardUi.cardPlace, pickedCard, true);
        }
        else
        {
            ReplaceCardsInDeck(newCard, pickedCard);
        }

        pickedCardUi.cardDescription = newCard.ToString(CardToStringFormatEnum.ShortCardName);

        StartCoroutine(GlitchEffect(pickedCardUi, pickedCardUi.GetisFaceDown(), disableDarkScreen));


        UpdateCardsList(pickedCardUi.cardPlace, newCard, true);
    }

    private void ReplaceCardsInDeck(Card targetCard, Card newCard)
    {
        Card[] cards = deck.ToArray();
        for (int i = 0; i < cards.Length; i++)
        {
            if (cards[i].ToString(CardToStringFormatEnum.ShortCardName).Equals(targetCard.ToString(CardToStringFormatEnum.ShortCardName)))
            {
                cards[i] = newCard;
                i = 52;
            }
        }
        deck = new Deck();
        for (int i = cards.Length - 1; i >= 0; i--)
        {
            deck.Push(cards[i]);
        }
    }

    private IEnumerator GlitchEffect(CardUi pickedCardUi, bool isFaceDown, Action Reset)
    {
        pickedCardUi.spriteRenderer.material = glitchMaterial;
        yield return new WaitForSeconds(Values.Instance.durationGlitchBeforeChange);
        if (!isFaceDown)
        {
            pickedCardUi.LoadSprite(true);
        }
        yield return new WaitForSeconds(Values.Instance.durationGlitchAfterChange);
        pickedCardUi.spriteRenderer.material = dissolveMaterial;
        Reset?.Invoke();
    }

    internal void SwapTwoCards(string cardPlace1, string cardPlace2, Action DisableDarkScreen)
    {

        //COPy WASTE
        CardUi cardSwap1 = GameObject.Find(cardPlace1).GetComponent<CardUi>();
        CardUi cardSwap2 = GameObject.Find(cardPlace2).GetComponent<CardUi>();
        if (cardSwap1.underSmoke)
        {
            EnableCardSmoke(false, false, cardSwap1);
        }
        if (cardSwap2.underSmoke)
        {
            EnableCardSmoke(false, false, cardSwap1);
        }
        SwapCardUiList(cardSwap1, cardSwap2);
        bool card1WasFaceDown = cardSwap1.GetisFaceDown();
        bool card2WasFaceDown = cardSwap2.GetisFaceDown(); // NOT IMPLENMANETD
        Transform tempTransform1 = cardSwap1.transform;
        Transform tempTransform2 = cardSwap2.transform;
        StartCoroutine(AnimationManager.Instance.SmoothMove(cardSwap1.transform, tempTransform2.position, tempTransform2.localScale, Values.Instance.cardSwapMoveDuration, () => SwitchCardsInfo(cardSwap1, cardSwap2), () => FlipAfterSwap(cardSwap1, !cardSwap1.cardMark.activeSelf, CardPlaceToTag(cardPlace1), CardPlaceToTag(cardPlace2)), null, null));
        StartCoroutine(AnimationManager.Instance.SmoothMove(cardSwap2.transform, tempTransform1.position, tempTransform1.localScale, Values.Instance.cardSwapMoveDuration, null, () => FlipAfterSwap(cardSwap2, !cardSwap2.cardMark.activeSelf, CardPlaceToTag(cardPlace2), CardPlaceToTag(cardPlace1)), DisableDarkScreen, null));

        Card tempCard1 = Card.StringToCard(cardSwap1.cardDescription);
        Card tempCard2 = Card.StringToCard(cardSwap2.cardDescription);
        UpdateCardsList(cardPlace1, tempCard2, true);
        UpdateCardsList(cardPlace2, tempCard1, true);

    }

    private void SwapCardUiList(CardUi cardSwap1, CardUi cardSwap2)
    {
        int index1 = GetIndexOfCard(cardSwap1);
        int index2 = GetIndexOfCard(cardSwap2);
        GetListByTag(cardSwap1.tag)[index1] = cardSwap2;
        GetListByTag(cardSwap2.tag)[index2] = cardSwap1;
    }


    internal void SwapAndDestroy(string cardFromDeck, string playerCard, Action DisableDarkScreen)
    {
        Debug.LogError("card" + cardFromDeck);
        Debug.LogError("card" + playerCard);
        CardUi cardFromDeckUI = GameObject.Find(cardFromDeck).GetComponent<CardUi>();
        CardUi playerCardUI = GameObject.Find(playerCard).GetComponent<CardUi>();
        /*int sortingOrder1 = cardFromDeckUI.GetComponent<Renderer>().sortingOrder;
        int sortingOrder2 = playerCardUI.GetComponent<Renderer>().sortingOrder;*/

        Transform tempTransform1 = cardFromDeckUI.transform;
        Transform tempTransform2 = playerCardUI.transform;
        SwapCardUiList(cardFromDeckUI, playerCardUI);
        StartCoroutine(AnimationManager.Instance.SmoothMove(cardFromDeckUI.transform, tempTransform2.position, tempTransform2.localScale, Values.Instance.cardSwapMoveDuration,
            () => ResetExtraDeckCards(), null, DisableDarkScreen, null));
        Card tempCard1 = Card.StringToCard(cardFromDeckUI.cardDescription);
        Card tempCard2 = Card.StringToCard(playerCardUI.cardDescription);
        UpdateCardsList(playerCard, tempCard1, true);
        cardFromDeckUI.transform.SetParent(playerCardUI.transform.parent);
        cardFromDeckUI.name = playerCardUI.name;
        //cardFromDeckUI.tag = playerCardUI.tag;
        cardFromDeckUI.cardPlace = playerCardUI.cardPlace;
        cardFromDeckUI.InitCardsTag(playerCardUI.tag);
        playerCardUI.InitCardsTag(Constants.DeckCardsTag);
        //  cardFromDeckUI.EnableSelecetPositionZ(false);
    }

    private void FlipAfterSwap(CardUi cardToFlip, bool enable, string tagFrom, string tagTo)
    {
        if (enable)
        {

            if (tagFrom == Constants.EnemyCardsTag)
            {
                cardToFlip.FlipCard(true, null);
            }
            else if (tagTo == Constants.EnemyCardsTag)
            {
                cardToFlip.FlipCard(false, null);

            }
        }
    }

    internal void SwapPlayersHands(Action DisableDarkScreen)
    {
        SwapTwoCards(Constants.PlayerCard1, Constants.EnemyCard1, null);
        SwapTwoCards(Constants.EnemyCard2, Constants.PlayerCard2, DisableDarkScreen);
    }


    private void SwitchCardsInfo(CardUi cardSwap1, CardUi cardSwap2)
    {
        Transform tempParent1 = cardSwap1.transform.parent;
        string tempName1 = cardSwap1.name;
        string tempTag1 = cardSwap1.tag;
        string tempPlace1 = cardSwap1.cardPlace;
        cardSwap1.transform.SetParent(cardSwap2.transform.parent);
        cardSwap1.name = cardSwap2.name;
        //cardSwap1.tag = cardSwap2.tag;
        cardSwap1.cardPlace = cardSwap2.cardPlace;
        cardSwap2.transform.SetParent(tempParent1);
        cardSwap2.name = tempName1;
        //  cardSwap2.tag = tempTag1;
        cardSwap2.cardPlace = tempPlace1;
        cardSwap1.InitCardsTag(cardSwap2.tag);
        cardSwap2.InitCardsTag(tempTag1);
        cardSwap1.EnableSelecetPositionZ(false);
        cardSwap2.EnableSelecetPositionZ(false);

    }

    private void GhostCardEffect(bool enable, CardUi cardObject)
    {
        if (enable)
        {
            cardObject.spriteRenderer.material = ghostMaterial;
        }
        else
        {
            cardObject.spriteRenderer.material = dissolveMaterial;
        }
    }

    #endregion





    private void AnimateDrawer(bool open, Action action)
    {
        float targetX;
        if (open)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.OpenDrawer, false);
            targetX = -1.15f;
            StartCoroutine(AnimationManager.Instance.SmoothMoveDrawer(transform.parent,
            new Vector3(targetX, transform.parent.position.y, transform.parent.position.z), Values.Instance.drawerMoveDuration, null, action));
        }
        else
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CloseDrawer, false);
            targetX = 1.1f;
            action?.Invoke();
            StartCoroutine(AnimationManager.Instance.SmoothMoveDrawer(transform.parent,
            new Vector3(targetX, transform.parent.position.y, transform.parent.position.z), Values.Instance.drawerMoveDuration, null, null));
        }
    }
    public void DeleteAllCards(Action DealHands)
    {
        ghostCardUi = null;
        ghostCard = null;
        ResetUiLists();
        foreach (CardUi cardToDestroy in FindAllCardsObjects())
        {
            if (cardToDestroy.cardMark.activeSelf)
            {
                cardToDestroy.cardMark.SetActive(false);
            }
            StartCoroutine(cardToDestroy.Dissolve(dissolveMaterial, 0, () => ResetCardUI(cardToDestroy)));
        }
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Dissolve, true);
        DealHands();
    }

    public void handToPrint(List<Card> cardsList)
    {
        String totalCards = " ";
        foreach (Card c in cardsList)
        {
            totalCards += c.ToString(CardToStringFormatEnum.ShortCardName) + " ,";
        }
        Debug.LogWarning(totalCards);
    }

}


