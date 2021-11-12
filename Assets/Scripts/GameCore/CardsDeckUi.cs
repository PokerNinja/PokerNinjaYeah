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
    Deck deck;
    public bool isPlayerFirst { set; get; }
    public List<Card> boardCards { get; set; }

    private static CardsDeckUi deckUi;
    private GameObject[] boardParents;

    private Vector3 cardHandVector, cardVectorBoard;

    ObjectPooler objectPooler;

    


    public GameObject playerCardAParent;
    public GameObject playerCardBParent;
    public GameObject enemyCardAParent;
    public GameObject enemyCardBParent;
    public GameObject deckCardAParent;
    public GameObject deckCardBParent;

    public GameObject flopAParent;
    public GameObject flopBParent;
    public GameObject flopCParent;
    public GameObject turnParent;
    public GameObject riverParent;

    public List<CardUi> playerCardsUi;
    public List<CardUi> enemyCardsUi;
    public List<CardUi> boardCardsUi;
    public List<CardUi> extraDeckCardsUi;
    public Material burnMaterial;
    public Material dissolveMaterial;

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
        boardParents = new GameObject[] { flopAParent, flopBParent, flopCParent, turnParent, riverParent };
        cardHandVector = new Vector3(1f, 1f, 1f);
        cardVectorBoard = new Vector3(0.75f, 0.75f, 0.75f);
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

        GameObject[] parentForHands = CreateParentForHands();
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
            CardCreatorUi(playerCard, dealToPlayer, false, parentForHands[i], cardName[i], null, false,-1);
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

            CardCreatorUi(newCard, false, false, boardParents[indexCard], Constants.BoardCards[indexCard], null, false,-1);
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
    private CardUi GetCardUiByName(string cardTarget2)
    {
        List<CardUi> allCardsUi = playerCardsUi.Concat(enemyCardsUi).Concat(boardCardsUi).ToList();
        for (int i = 0; i <= allCardsUi.Count; i++)
        {
            if (allCardsUi[i].cardPlace.Equals(cardTarget2))
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
        if (cardPlace.Equals(Constants.PlayerCard2) || cardPlace.Equals(Constants.EnemyCard2)  || cardPlace.Equals(Constants.BoardCards[1]))
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
        return playerHandWithBoard;
    }

    private void CardCreatorUi(Card newCard, bool isFaceDown, bool aboveDarkScreen, GameObject cardParent, string cardPlace, Action disableDarkScreen, bool isCloseDrawer, int indexToInsert)
    {
        Vector3 cardScale;
        // TODO add list value to catch the object and store it
        string cardTag = "Card" + cardPlace.Substring(0, 1);
        CardUi cardObject = objectPooler.SpwanCardFromPool(cardTag);
        cardScale = cardHandVector;
        cardObject.transform.SetParent(cardParent.transform);
        if (cardParent.transform.parent.name == "Board")
        {
            cardScale = cardVectorBoard;
        }

        cardObject.name = cardPlace;
        cardObject.transform.position = new Vector3(cardTransform.position.x, cardTransform.position.y, 1); ;
        cardObject.transform.localScale = cardTransform.localScale;
        cardObject.Init(cardTag,newCard, isFaceDown, aboveDarkScreen, cardPlace);
        Vector3 targetPosition = new Vector3(cardParent.transform.position.x, cardParent.transform.position.y, cardObject.transform.position.z);
        Action closeDrawer = null;
        if (isCloseDrawer)
        {
            closeDrawer = () => AnimateDrawer(false, null);
        }
        StartCoroutine(AnimationManager.Instance.SmoothMove(cardObject.transform, targetPosition, cardScale,
        Values.Instance.cardDrawMoveDuration, null, () => cardObject.CardReveal(!isFaceDown), disableDarkScreen,()=> {
            closeDrawer?.Invoke();
        AddCardToList(cardTag, cardObject,indexToInsert);
            }));

    }

    private void AddCardToList(string cardTag, CardUi cardObject, int indexToInsert)
    {
        if(indexToInsert == -1 || GetListByTag(cardTag).Count < indexToInsert)
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

    public Hand CalculateHand(bool isPlayer)
    {

        TexasHand playerCards;
        if (isPlayer)
        {
            playerCards = GetEnemyHand();
        }
        else
        {
            playerCards = GetPlayerHand();
        }
        PokerHandRankingTable rankingTable = new PokerHandRankingTable();

        List<Card> totalCards = new List<Card>();
        totalCards.AddRange(playerCards.getCards());
        totalCards.AddRange(boardCards);
        if (boardCards.Count == 3)
        {
            return new Hand(totalCards[0], totalCards[1], totalCards[2], totalCards[3], totalCards[4], rankingTable); ;
        }
        List<Hand> handsOptions = new List<Hand>();
        List<Card> currentHandList = new List<Card>();
        Hand currentHand;
        if (boardCards.Count == 4)
        {
            for (int i = 0; i < totalCards.Count; i++)
            {
                currentHandList = new List<Card>(totalCards.ToList<Card>());
                currentHandList.RemoveAt(i);
                currentHand = new Hand(currentHandList[0], currentHandList[1], currentHandList[2], currentHandList[3], currentHandList[4], rankingTable);
                handsOptions.Add(currentHand);
            }
            handsOptions = handsOptions.OrderBy(h => h.Rank).ToList<Hand>();
            return handsOptions[0];
        }
        for (int i = 0; i < totalCards.Count; i++)
        {
            for (int j = i + 1; j < totalCards.Count; j++)
            {
                currentHandList = new List<Card>(totalCards.ToList<Card>());
                currentHandList.RemoveAt(j);
                currentHandList.RemoveAt(i);
                currentHand = new Hand(currentHandList[0], currentHandList[1], currentHandList[2], currentHandList[3], currentHandList[4], rankingTable);
                handsOptions.Add(currentHand);
            }
        }
        handsOptions = handsOptions.OrderBy(h => h.Rank).ToList<Hand>();
        return handsOptions[0];

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
        Debug.LogError("disable" + cardPlace);
        foreach (CardUi card in GetListByTag(CardPlaceToTag(cardPlace)))
        {
            card.SetSelection(false,"");
        }
    }

    
    private GameObject GetParentByPlace(string cardPlace)
    {
        // Make It batter
        return GameObject.Find(cardPlace + "Slut");
    }

    private GameObject[] CreateParentForHands()
    {
        GameObject[] parents;
        if (isPlayerFirst)
        {
            parents = new GameObject[] { playerCardAParent, enemyCardAParent, playerCardBParent, enemyCardBParent };
        }
        else
        {
            parents = new GameObject[] { enemyCardAParent, playerCardAParent, enemyCardBParent, playerCardBParent };
        }
        return parents;
    }


    private string[] CreateCardNamesForHands()
    {
        string[] names;

        if (isPlayerFirst)
        {
            names = new string[] { Constants.PlayerCard1, Constants.EnemyCard1, Constants.PlayerCard2, Constants.EnemyCard2};
        }
        else
        {
            names = new string[] { Constants.EnemyCard1, Constants.PlayerCard1, Constants.EnemyCard2, Constants.PlayerCard2};
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
            extraDeckCardsUi[0].SetSelection(true,"");
            extraDeckCardsUi[1].SetSelection(true,"");
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
            StartCoroutine(cardToDestroy.FadeBurnOut(() =>
        RestAfterDestroy(cardToDestroy, OnEnd)));
        }
        else
        {
            StartCoroutine(cardToDestroy.Dissolve(0f,() =>
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
            CardCreatorUi(newCard, isFlip, true, GetParentByPlace(cardPlace), cardPlace, disableDarkScreen, isLastCard , indexToInsert);
        }
    }
    internal void SwapTwoCards(string cardPlace1, string cardPlace2, Action DisableDarkScreen)
    {
      
        //COPy WASTE
        CardUi cardSwap1 = GameObject.Find(cardPlace1).GetComponent<CardUi>();
        CardUi cardSwap2 = GameObject.Find(cardPlace2).GetComponent<CardUi>();
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
        StartCoroutine(AnimationManager.Instance.SmoothMove(cardFromDeckUI.transform, tempTransform2.position, tempTransform2.localScale,Values.Instance.cardSwapMoveDuration,
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

    #endregion



   

    private void AnimateDrawer(bool open, Action action)
    {
        float targetX;
        if (open)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.OpenDrawer);
            targetX = -1.15f;
            StartCoroutine(AnimationManager.Instance.SmoothMoveDrawer(transform.parent,
            new Vector3(targetX, transform.parent.position.y, transform.parent.position.z), Values.Instance.drawerMoveDuration, null, action));
        }
        else
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.CloseDrawer);
            targetX = 1.1f;
            action?.Invoke();
            StartCoroutine(AnimationManager.Instance.SmoothMoveDrawer(transform.parent,
            new Vector3(targetX, transform.parent.position.y, transform.parent.position.z), Values.Instance.drawerMoveDuration, null, null));
        }
    }
        public void DeleteAllCards(Action DealHands)
        {
            ResetUiLists();
            foreach (CardUi cardToDestroy in FindAllCardsObjects())
            {
            if (cardToDestroy.cardMark.activeSelf)
            {
                cardToDestroy.cardMark.SetActive(false);
            }
                StartCoroutine(cardToDestroy.Dissolve(0, ()=>ResetCardUI(cardToDestroy)));
            }
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Dissolve);
        DealHands();
        }

        private void handToPrint(List<Card> cardsList, String msg)
        {
            String totalCards = " ";
            foreach (Card c in cardsList)
            {
                totalCards += c.ToString(CardToStringFormatEnum.ShortCardName) + " ,";
            }
        }

    }
    /* internal void Blind2RandomCards(bool isPlayerActivate, string firstToBlind, string secondToBlind, Action endAction)
    {

        BlindCard(isPlayerActivate, firstToBlind, null);
        BlindCard(isPlayerActivate, secondToBlind, endAction);
    }

    internal void BlindCard(bool isPlayerActivate, string cardTarget2, Action onFinish)
    {
        CardUi cardToBlind = GameObject.Find(cardTarget2).GetComponent<CardUi>();
        cardToBlind.FlipCard(false, onFinish);
    }*/

