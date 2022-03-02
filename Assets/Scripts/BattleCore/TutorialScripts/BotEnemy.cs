using StandardPokerHandEvaluator;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class BotEnemy : State
{
    private int turnCounter;
    private List<int> playOptions;
    private int energyLeft;
    private int currentRank;
    private int card1;
    private int card2;
    private bool card1DuplicateBoard;
    private bool card2DuplicateBoard;
    private int boardCardsAmount;
    private List<CardUi> boardCardsUi;
    private string pu1 = "";
    private string pu2 = "";
    private string botEsElement = "";
    private int esCounter = 0;

    private int pu1Odds = 0;
    private int pu2Odds = 0;
    public BotEnemy(BattleSystem battleSystem, int turnCounter, bool betOffer, bool playerAcceptRaise) : base(battleSystem)
    {
        Debug.Log("BOTSTATER");
        this.battleSystem = battleSystem;
        this.turnCounter = turnCounter;
        if (betOffer)
        {
            DecideAcceptBet();
        }
        else
        {
            if (turnCounter > 4)
            {
                battleSystem.SavePrefsInt(Constants.Instance.botRaiseKey, 0);
            }
            if (battleSystem.firstRound)
            {
                battleSystem.firstRound = false;
                energyLeft = 0;
                esCounter = 0;
                battleSystem.SavePrefsInt(Constants.Instance.botEsCounterKey, 0);
            }
            else
            {
                energyLeft = GetLastEnergy();
            }
            if (!playerAcceptRaise)
            {
                ChargeEnergy();
            }
            else
            {
                battleSystem.SavePrefsInt(Constants.Instance.botRaiseKey, 1);
            }
            Debug.Log("raiseBefore: " + battleSystem.LoadPrefsInt(Constants.Instance.botRaiseKey));
            InitBotTurn();
            Debug.Log("BOT ENERGY:" + energyLeft);
        }
    }

    private async void DecideAcceptBet()
    {
        InitFieldSituation();
        EnemyBotGotDuplicateCard();
        if (battleSystem.puDeckUi.GetPu(false, 0) != null)
        {
            pu1 = battleSystem.puDeckUi.GetPu(false, 0).puName;
            Debug.Log("pu1 " + pu1);
        }
        if (battleSystem.puDeckUi.GetPu(false, 1) != null)
        {
            pu2 = battleSystem.puDeckUi.GetPu(false, 1).puName;
            Debug.Log("pu2 " + pu2);
        }
        pu1Odds = CalculatePu(pu1);
        pu2Odds = CalculatePu(pu2);
        int rankOdds = (10 - currentRank) * 2;
        int turnOdds = (7 - turnCounter);
        int totalAcceptOdds = pu1Odds + pu2Odds + rankOdds + turnOdds;
        Debug.Log("AcceptOdds: " + totalAcceptOdds);
        List<int> acceptOdds = GenerateListProbability(EnemyActions.AcceptRaise, totalAcceptOdds);
        List<int> declineOdds = GenerateListProbability(EnemyActions.RefuseRaise, 4);
        List<int> options = acceptOdds.Concat(declineOdds).ToList();
        int act = options[battleSystem.GenerateRandom(0, options.Count)];
        await Task.Delay(battleSystem.GenerateRandom(2200, 5500));

        if (act == EnemyActions.AcceptRaise.GetHashCode())
        {
            battleSystem.EnemyAcceptRaise();
        }
        else
        {
            battleSystem.EnemyDeclineRaise();
        }
    }

    private async void InitBotTurn()
    {
        await Task.Delay(1000);
        battleSystem.finishPuDissolve = true;
        InitFieldSituation();
        CheckMyOptions();
    }

    private void InitFieldSituation()
    {
        pu1 = "";
        pu2 = "";
        card1 = GetEnemyCardRank(0);
        card2 = GetEnemyCardRank(1);
        currentRank = GetHandRank();
        boardCardsUi = battleSystem.cardsDeckUi.boardCardsUi;
        botEsElement = battleSystem.LoadPrefsString(Constants.Instance.botEsElementKey);
        esCounter = battleSystem.LoadPrefsInt(Constants.Instance.botEsCounterKey);
        EnemyBotGotDuplicateCard();
    }

    private int GetWhatPlayerCardValueRevealed()
    {
        if (battleSystem.cardsDeckUi.playerCardsUi[0].cardMark.activeSelf)
        {
            return GetPlayerCardRank(0);
        }
        if (battleSystem.cardsDeckUi.playerCardsUi[1].cardMark.activeSelf)
        {
            return GetPlayerCardRank(1);
        }
        return 0;
    }

    private int GetEnemyCardRank(int index)
    {
        return GetRankFromCardDesc(battleSystem.cardsDeckUi.enemyCardsUi[index].cardDescription);
    }
    private int GetBoardCardRank(int index)
    {
        return GetRankFromCardDesc(battleSystem.cardsDeckUi.boardCardsUi[index].cardDescription);
    }
    private int GetPlayerCardRank(int index)
    {
        return GetRankFromCardDesc(battleSystem.cardsDeckUi.playerCardsUi[index].cardDescription);
    }

    private int GetHandRank()
    {
        return battleSystem.Interface.ConvertHandRankToTextNumber(battleSystem.cardsDeckUi.CalculateHand(false, false, false, false).Rank);
    }

    private int GetLastEnergy()
    {
        return battleSystem.LoadPrefsInt(Constants.Instance.botEnergyKey);
    }

    private void ChargeEnergy()
    {
        if (turnCounter > 1)
        {
            energyLeft += 2;
        }
        else
        {
            energyLeft += 1;
        }
        if (energyLeft > 3)
        {
            energyLeft = 3;
        }
        battleSystem.SavePrefsInt(Constants.Instance.botEnergyKey, energyLeft);
    }

    private void CheckMyOptions()
    {
        List<int> drawness = new List<int>();
        List<int> skillness = new List<int>();
        List<int> puness = new List<int>();
        List<int> raiseness = new List<int>();
        List<int> endness = new List<int>();
        int drawOdds = 0;
        int skillOdds = 0;
        int raiseOdds = 0;
        int endTurnOdds = 0;
        switch (turnCounter)
        {
            case 6:
            case 5:
                raiseOdds = 1;
                drawOdds = 2;
                if (energyLeft == 1)
                {
                    endTurnOdds = 3;
                }
                break;
            case 4:
            case 3:
                raiseOdds = 2;
                if (currentRank > 7)
                {
                    raiseOdds = 1;
                }
                drawOdds = 3;
                skillOdds = 2;
                if (energyLeft == 1)
                {
                    endTurnOdds = 3;
                    drawOdds = 1;
                }
                break;
            case 2:
            case 1:
                raiseOdds = 2;
                skillOdds = 5;
                if (currentRank > 7)
                {
                    raiseOdds = 1;
                }
                if (energyLeft > 1)
                {
                    drawOdds = 2;
                }
                if (energyLeft == 1 && currentRank < 7)
                {
                    endTurnOdds = 2;
                }
                break;
        }

        Debug.Log("turn " + turnCounter);
        if (energyLeft == 0)
        {
            StartAutoEndWithDelay(1200, true);
        }
        else
        {
            puness = CalculatePuUseProbability();
            skillness = CalculateSkillProbability(skillOdds);
            drawness = CalculateDrawProbability(drawOdds);
            raiseness = CalculateRaiseProbability(raiseOdds);
            endness = CalcualteEndTurnProbability(endTurnOdds);
            List<int> options = puness.Concat(skillness).Concat(drawness).Concat(raiseness).Concat(endness).ToList();
            BotRandomAct(options);

            Debug.Log("pu: " + puness.Count + " skill:" + skillness.Count + " draw:" + drawness.Count
                + " raise:" + raiseness.Count + " end:" + endness.Count);
            Debug.Log("total:" + options.Count);
            Debug.Log("rank:" + currentRank + "  energy:" + energyLeft);
            Debug.Log("hand:" + card1 + "," + card2);
        }
    }

    private async void BotRandomAct(List<int> options)
    {
        await Task.Delay(3000);
        int costOfAction = 0;
        bool waitForPlayerResponse = false;
        int delay = 6000;
        if (options.Count > 0)
        {
            int act = options[battleSystem.GenerateRandom(0, options.Count)];
            switch (act)
            {
                case (int)EnemyActions.EndTurn:
                    {
                        Debug.Log("Bot EndTurn");
                        delay = 1000;
                        break;
                    }
                case (int)EnemyActions.ElementSkill:
                    {
                        Debug.Log("Bot ESkill");
                        ActivateBotEs();
                        delay = 2500;
                        break;
                    }
                case (int)EnemyActions.DrawPu:
                    {
                        Debug.Log("Bot Draw");
                        BotDraweCard();
                        costOfAction = 1;
                        delay = battleSystem.GenerateRandom(2200, 2300);
                        break;
                    }
                case (int)EnemyActions.OfferARaise:
                    {
                        Debug.Log("Bot Raise");
                        battleSystem.EnemyBetPopUp(battleSystem.raiseOptions[battleSystem.GenerateRandom(0, TotalRaiseOptions())]);
                        delay = 0;
                        waitForPlayerResponse = true;
                        break;
                    }
                case (int)EnemyActions.Pu1Use:
                    {
                        Debug.Log("Bot PU1");
                        if (!IsMonster(pu1))
                        {
                            delay = 4200;
                        }
                        BotPuUse(pu1, 0);
                        costOfAction = GetPuCost(pu1);
                        break;
                    }
                case (int)EnemyActions.Pu2Use:
                    {
                        Debug.Log("Bot PU2");
                        if (!IsMonster(pu1))
                        {
                            delay = 4200;
                        }
                        BotPuUse(pu2, 1);
                        costOfAction = GetPuCost(pu2);
                        break;
                    }
                case (int)EnemyActions.PuRandomUse:
                    {
                        Debug.Log("Bot RANDOM");
                        string[] pus = { pu1, pu2 };
                        int index = battleSystem.GenerateRandom(0, 2);
                        if (!IsMonster(pus[index]))
                        {
                            delay = 4200;
                        }
                        BotPuUse(pus[index], index);
                        costOfAction = GetPuCost(pus[index]);
                        break;
                    }
            }
            if (!waitForPlayerResponse)
            {
                StartAutoEndWithDelay(delay, ReduceEnergy(costOfAction));
            }
        }
        else
        {
            StartAutoEndWithDelay(battleSystem.GenerateRandom(1100, 1700), true);
        }
    }

    private void ActivateBotEs()
    {
        string cardTarget1 = "";
        string cardTarget2 = "";
        switch (botEsElement)
        {
            case "f":
                if (currentRank >= 7)
                {
                    if (!card1DuplicateBoard || !card2DuplicateBoard)
                    {
                        if (!BotGotFrozenCards(0) && !card1DuplicateBoard && card1 < card2)
                            cardTarget1 = Constants.EnemyCard1;
                        if (!BotGotFrozenCards(1) && !card2DuplicateBoard && card2 < card1)
                            cardTarget1 = Constants.EnemyCard2;
                    }
                    else
                    {
                        cardTarget1 = GetCardOf(Constants.BoardCardsTag, "", 0, false, false, false);
                    }
                    cardTarget2 = GetCardOf(Constants.BoardCardsTag, cardTarget1, 0, false, false, false);
                }
                else if (currentRank > 4) //Stright or flush
                {
                    if (!PlayerGotFrozenCards(0))
                        cardTarget1 = Constants.PlayerCard1;
                    if (!PlayerGotFrozenCards(1))
                        cardTarget1 = Constants.PlayerCard2;
                    cardTarget2 = GetCardOf(Constants.BoardCardsTag, cardTarget1, 0, false, false, false);
                }
                break;

            case "i":
                if (turnCounter < 3)
                {
                    if (PlayerGotFrozenCards(2))
                    {
                        if (PlayerGotFrozenCards(0))
                            cardTarget1 = Constants.PlayerCard1;
                        if (PlayerGotFrozenCards(1))
                            cardTarget1 = Constants.PlayerCard2;
                    }
                    else if ((BotGotFrozenCards(0) && !card1DuplicateBoard))
                        cardTarget1 = Constants.EnemyCard1;
                    else if ((BotGotFrozenCards(1) && !card2DuplicateBoard))
                        cardTarget1 = Constants.EnemyCard1;
                    else
                        cardTarget1 = GetFrozenBoardCard("", false);

                    cardTarget2 = GetFrozenBoardCard(cardTarget1, false);
                }
                else
                {
                    cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", 0, false, true, true);
                    cardTarget2 = GetCardOf(Constants.BoardCardsTag, "", 0, false, true, true);
                }
                break;

            case "w":
                if (currentRank == 7 && card1 != card2)
                {
                    cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", 0, true, false, false);
                    cardTarget2 = GetCardOf(Constants.BoardCardsTag, "", 0, false, true, false);
                }
                else if ((!BotGotFrozenCards(0) || !BotGotFrozenCards(1)) && (!PlayerGotFrozenCards(0) || !PlayerGotFrozenCards(1)))
                {
                    cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", 0, true, false, false);
                    cardTarget2 = GetCardOf(Constants.PlayerCardsTag, "", 0, GetRandomBool(), GetRandomBool(), false);
                }
                else
                {
                    cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", 0, true, false, false);
                    cardTarget2 = GetCardOf(Constants.BoardCardsTag, "", 0, false, true, false);
                }

                break;
        }
        battleSystem.FakeEnemyPuUse(-1, cardTarget1, cardTarget2, false);
        battleSystem.SavePrefsInt(Constants.Instance.botEsCounterKey, 0);
    }

    private bool GetRandomBool()
    {
        System.Random rng = new System.Random();
        return rng.Next(0, 2) > 0;
    }

    private int TotalRaiseOptions()
    {
        if (!battleSystem.EnoughHpToRaise(1))
        {
            return 1;
        }
        if (!battleSystem.EnoughHpToRaise(2))
        {
            return 2;
        }
        return 3;
    }

    private string GetAvailablePlayerCard()
    {
        List<CardUi> playerCards = battleSystem.cardsDeckUi.playerCardsUi;
        if (playerCards[0].freeze)
        {
            return Constants.PlayerCard2;
        }
        else if (playerCards[1].freeze)
        {
            return Constants.PlayerCard1;
        }
        return battleSystem.cardsDeckUi.playerCardsUi[battleSystem.GenerateRandom(0, 2)].cardPlace;
    }

    private bool IsMonster(string pu)
    {
        return pu.Contains("m");
    }

    private void BotPuUse(string puName, int index)
    {
        string cardTarget1 = "";
        string cardTarget2 = "";

        switch (puName)
        {
            case "f1":
                cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", 0, true, false, true);
                break;
            case "f2":
                cardTarget1 = GetCardOf(Constants.BoardCardsTag, "", 0, true, false, true);
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, cardTarget1, 0, true, false, true);
                break;
            case "f3":
                cardTarget1 = GetFrozenOrRandomCardOf(false, Constants.PlayerCardsTag);
                break;
            case "i1":
                cardTarget1 = GetFrozenOrRandomCardOf(true, Constants.PlayerCardsTag);
                break;
            case "i2":
                cardTarget1 = GetCardOf(Constants.BoardCardsTag, "", 0, false, true, true);
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, cardTarget1, 0, false, true, true);
                break;
            case "i3":
                if (currentRank > 7)
                {
                    cardTarget1 = GetFrozenOrRandomCardOf(true, Constants.EnemyCardsTag);
                }
                else
                {
                    cardTarget1 = GetFrozenOrRandomCardOf(false, Constants.EnemyCardsTag);
                }
                break;
            case "w1":
                cardTarget1 = GetCardOf(Constants.PlayerCardsTag, "", 0, true, false, false);
                cardTarget2 = GetCardOf(Constants.EnemyCardsTag, "", 0, false, false, false);
                break;
            case "w2":
                cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", 0, true, false, false);
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, "", GetRankFromCardPlace(cardTarget1), false, true, false);
                break;
            case "w3":
                cardTarget1 = GetCardOf(Constants.PlayerCardsTag, "", 0, true, false, false);
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, "", GetRankFromCardPlace(cardTarget1), true, false, false);
                break;
            case "fm1":
                cardTarget1 = Constants.Deck1;
                cardTarget2 = GetCardOf(Constants.EnemyCardsTag, "", 0, true, false, false);
                break;
            case "wm2":
                cardTarget1 = GetListOfRandomCardsForMonster(true);
                break;
            case "im2":
                cardTarget2 = battleSystem.GenerateRandom(4, 6).ToString();
                cardTarget1 = GetListOfRandomCardsForMonster(false);
                break;
            case "fm2":
                cardTarget2 = battleSystem.GenerateRandom(5, 7).ToString();
                cardTarget1 = GetListOfRandomCardsForMonster(false);
                break;

        }
        Debug.LogWarning("pu " + index + " c " + cardTarget1 + " , " + cardTarget2);
        UpdateNcUse(puName);
        battleSystem.FakeEnemyPuUse(index, cardTarget1, cardTarget2, false);
    }

    private void UpdateNcUse(string powerupName)
    {
        if (powerupName[0].ToString().Equals(botEsElement))
        {
            esCounter++;
            if (powerupName.Contains("m"))
            {
                esCounter++;
            }
            if (esCounter > 3)
            {
                esCounter = 3;
            }
            battleSystem.SavePrefsInt(Constants.Instance.botEsCounterKey, esCounter);
        }
    }

    private string GetListOfRandomCardsForMonster(bool onlyUnfreeze)
    {
        return string.Join(",", battleSystem.GetRandomAvailableCardsNames(onlyUnfreeze));
    }
    /* private string GetPlayerRevealedCardAndIfFreeze(bool canTargetFreeze)
     {
         if (!canTargetFreeze && battleSystem.cardsDeckUi.GetCardUiByName(cardPlace).freeze)
         {
             if (cardPlace.Equals(Constants.PlayerCard1))
             {
                 return Constants.PlayerCard2;
             }
             else
             {
                 return Constants.PlayerCard1;
             }
         }
         return cardPlace;
     }*/
    /*  private string GetPlayerRevealedCard(bool revealed)
      {
          if (battleSystem.cardsDeckUi.playerCardsUi[0].cardMark.activeSelf)
          {
              if (revealed)
              {
                  return Constants.PlayerCard1;
              }
              else
              {
                  return Constants.PlayerCard2;
              }
          }
          if (battleSystem.cardsDeckUi.playerCardsUi[1].cardMark.activeSelf)
          {
              if (revealed)
              {
                  return Constants.PlayerCard2;
              }
              else
              {
                  return Constants.PlayerCard1;
              }
          }
          return battleSystem.cardsDeckUi.playerCardsUi[battleSystem.GenerateRandom(0, 2)].cardPlace;
      }*/

    private string GetDuplicateBoardCardWithPlayer(int playerRevealdCard, bool canTargetFreeze)
    {
        List<CardUi> boardsCards = new List<CardUi>(battleSystem.cardsDeckUi.GetListByTag(Constants.BoardCardsTag).ToArray());

        for (int i = 0; i < boardsCards.Count; i++)
        {
            if (canTargetFreeze || !boardsCards[i].freeze)
            {
                if (GetRankFromCardDesc(boardsCards[i].cardDescription) == playerRevealdCard)
                {
                    return boardsCards[i].cardPlace;
                }
            }
        }
        return "";
    }

    private void BotDraweCard()
    {
        if (battleSystem.puDeckUi.enemyPusUi[0] == null || battleSystem.puDeckUi.enemyPusUi[1] == null)
        {
            battleSystem.DealPu(false, null);
        }
        else
        {       //Make it smarter
            int index = 0;
            if (pu1Odds > pu2Odds && energyLeft > 1)
            {
                index = 1;
            }
            battleSystem.ReplacePu(false, index);
        }

    }

    private async void StartAutoEndWithDelay(int delay, bool autoEnd)
    {
        Debug.LogError("AutoNotice " + delay);
        await Task.Delay(delay);
        if (autoEnd)
        {
            battleSystem.FakeEnemyEndTurn();
        }
        else
        {
            while (!battleSystem.finishPuDissolve)
            {
                await Task.Delay(800);
                Debug.Log("cycle");
            }
            InitBotTurn();
            Debug.Log("InitAgain");
        }

    }

    private string GetFrozenBoardCard(string differentCard, bool lowest)
    {
        List<CardUi> cards = new List<CardUi>(battleSystem.cardsDeckUi.GetListByTag(Constants.BoardCardsTag).ToArray());
        if (lowest)
        {
            cards = cards.OrderBy(h => GetRankFromCardDesc(h.cardDescription)).ToList<CardUi>();
        }
        else
        {
            cards = cards.OrderByDescending(h => GetRankFromCardDesc(h.cardDescription)).ToList<CardUi>();
        }
        for (int i = 0; i < cards.Count; i++)
        {
            if (!cards[i].cardPlace.Equals(differentCard))
            {
                if ((GetBoardCardRank(i) != card1) && (GetBoardCardRank(i) != card2))
                {
                    if (cards[i].freeze)
                    {
                        return cards[i].cardPlace;
                    }
                }
            }
        }
        return cards[0].cardPlace;
    }

    private string GetCardOf(string cardsTag, string differentCard, int differentRank, bool lowest, bool isDuplicate, bool canTargetFreeze)
    {
        List<CardUi> cards = new List<CardUi>(battleSystem.cardsDeckUi.GetListByTag(cardsTag).ToArray());
        if (lowest)
        {
            cards = cards.OrderBy(h => GetRankFromCardDesc(h.cardDescription)).ToList<CardUi>();
        }
        else
        {
            cards = cards.OrderByDescending(h => GetRankFromCardDesc(h.cardDescription)).ToList<CardUi>();
        }
        List<CardUi> boardCards = battleSystem.cardsDeckUi.GetListByTag(Constants.BoardCardsTag);
        if (cardsTag.Equals(Constants.BoardCardsTag))
        {
            boardCards = battleSystem.cardsDeckUi.GetListByTag(Constants.EnemyCardsTag);
        }
        for (int i = 0; i < cards.Count; i++)
        {
            for (int j = 0; j < boardCards.Count; j++)
            {
                if (canTargetFreeze || !cards[i].freeze)
                {
                    int currentCardRank = GetRankFromCardDesc(cards[i].cardDescription);

                    if (!cards[i].cardPlace.Equals(differentCard) && currentCardRank != differentRank)
                    {
                        int boardCardRank = GetRankFromCardDesc(boardCards[j].cardDescription);
                        if (isDuplicate)
                        {
                            if (currentCardRank == boardCardRank)
                            {
                                Debug.Log("Chosen Duplicate " + cards[i].cardPlace);
                                return cards[i].cardPlace;
                            }
                        }
                        else
                        {
                            if (currentCardRank != boardCardRank)
                            {
                                Debug.Log("Chosen NOT duplicate " + cards[i].cardPlace);
                                return cards[i].cardPlace;
                            }
                        }
                    }
                }
            }
        }
        return GetFirstCard(cards, differentCard, canTargetFreeze);
        /*  if (cards[0].cardPlace.Equals(differentCard))
          {
              Debug.Log("toYse " + cards[0].cardPlace);
              Debug.Log("differentThan " + differentCard);
              return GetFirstCard(cards, 1, canTargetFreeze);
              //  return cards[1].cardPlace;
          }
          else
          {
              Debug.Log("ChoseFirst " + cards[0].cardPlace);
              return GetFirstCard(cards, 0, canTargetFreeze);
          }*/
    }

    private string GetFirstCard(List<CardUi> cards, string differetnThan, bool canTargetFreeze)
    {

        for (int i = 0; i < cards.Count; i++)
        {
            if (!cards[i].cardPlace.Equals(differetnThan))
            {
                if (canTargetFreeze || !cards[i].freeze)
                {
                    return cards[i].cardPlace;
                }
            }
        }
        return cards[0].cardPlace;
    }

    private int GetRankFromCardDesc(string cardDescription)
    {
        return Card.StringValueToInt(cardDescription[0].ToString());
    }
    private int GetRankFromCardPlace(string cardPlace)
    {
        return GetRankFromCardDesc(battleSystem.cardsDeckUi.GetCardUiByName(cardPlace).cardDescription);
    }

    private string GetFrozenOrRandomCardOf(bool frozen, string cardsTag)
    {
        List<CardUi> targetList = battleSystem.cardsDeckUi.GetListByTag(cardsTag);
        if (frozen)
        {
            if (targetList[0].freeze)
            {
                return Constants.PlayerCard1;
            }
            else if (targetList[1].freeze)
            {
                return Constants.PlayerCard2;
            }
        }
        return targetList[battleSystem.GenerateRandom(0, 2)].cardPlace;
    }

    private bool ReduceEnergy(int energyToReduce)
    {
        energyLeft -= energyToReduce;
        Debug.Log("energyLeft" + energyLeft);
        battleSystem.SavePrefsInt(Constants.Instance.botEnergyKey, energyLeft);
        return energyLeft == 0 || energyToReduce == 0;
    }

    private List<int> CalcualteEndTurnProbability(int endTurnOdds)
    {

        return GenerateListProbability(EnemyActions.EndTurn, endTurnOdds);

    }

    private List<int> CalculatePuUseProbability()
    {
        if (battleSystem.puDeckUi.GetPu(false, 0) != null)
        {
            pu1 = battleSystem.puDeckUi.GetPu(false, 0).puName;
            Debug.Log("pu1 " + pu1);
        }
        if (battleSystem.puDeckUi.GetPu(false, 1) != null)
        {
            pu2 = battleSystem.puDeckUi.GetPu(false, 1).puName;
            Debug.Log("pu2 " + pu2);
        }
        pu1Odds = CalculatePu(pu1);
        pu2Odds = CalculatePu(pu2);
        if (pu1Odds > pu2Odds)
        {
            return GenerateListProbability(EnemyActions.Pu1Use, pu1Odds);
        }
        else if (pu1Odds < pu2Odds)
        {
            return GenerateListProbability(EnemyActions.Pu2Use, pu2Odds);
        }
        else if (pu1Odds == pu2Odds && pu1Odds > 0)
        {
            return GenerateListProbability(EnemyActions.PuRandomUse, pu2Odds);
        }
        return new List<int>();
    }

    private bool AreBotCardsDuplicateOnBoard(bool bothCards)
    {
        if (card1 == card2)
        {
            return true;
        }
        if (bothCards)
        {
            if ((card1DuplicateBoard && card2DuplicateBoard))
            {
                return true;
            }
        }
        else if (card1DuplicateBoard || card2DuplicateBoard)
        {
            return true;
        }
        return false;
    }
    private int CalculatePu(string pu)
    {
        int odds = 0;
        if (!pu.Equals(""))
        {
            int puCost = GetPuCost(pu);

            if (puCost > energyLeft)
            {
                return 0;
            }
            else
            {
                switch (pu)
                {
                    case "f1":
                        if (currentRank >= 7 && EnemyGotCardLowerThan(7) && !AreBotCardsDuplicateOnBoard(false))
                        {
                            odds = 5;
                        }
                        break;
                    case "f2":
                        if (currentRank >= 7)
                        {
                            if (boardCardsUi.Count > 3)
                            {
                                if (!AreBotCardsDuplicateOnBoard(false))
                                {
                                    odds = 4;
                                }
                                else if (AreBotCardsDuplicateOnBoard(true))
                                {
                                    odds = 2;
                                }
                            }
                        }
                        break;
                    case "f3":
                        odds = 4;
                        break;

                    case "i1":
                        if (PlayerGotFrozenCards(2))
                        {
                            odds = 8;
                        }
                        else
                        {
                            odds = 3;
                        }

                        break;
                    case "i2":

                        if (AreBotCardsDuplicateOnBoard(true))
                        {
                            odds = 10;
                        }
                        else if (AreBotCardsDuplicateOnBoard(false))
                        {
                            odds = 6;
                        }

                        break;
                    case "i3":
                        if (AreBotCardsDuplicateOnBoard(false))
                        {
                            odds = 8;
                        }
                        else
                        {
                            odds = 2;
                        }
                        break;
                    case "w1":
                        if (UnfrozenCardsAvailable(Constants.PlayerCardsTag, 1) && UnfrozenCardsAvailable(Constants.EnemyCardsTag, 1))
                        {
                            if (!AreBotCardsDuplicateOnBoard(false))
                            {
                                if (currentRank > 7)
                                {
                                    odds = 8;
                                }
                                else
                                {
                                    odds = 3;
                                }
                            }
                        }
                        break;
                    case "w2":
                        if (UnfrozenCardsAvailable(Constants.BoardCardsTag, 1) && UnfrozenCardsAvailable(Constants.EnemyCardsTag, 1))
                        {
                            if (!AreBotCardsDuplicateOnBoard(false))
                            {
                                odds = 8;
                            }
                            else
                            {
                                if ((card1DuplicateBoard && !card2DuplicateBoard) || (!card1DuplicateBoard && card2DuplicateBoard))
                                    odds = 3;
                            }
                        }
                        break;
                    case "w3":
                        if (UnfrozenCardsAvailable(Constants.PlayerCardsTag, 1) && UnfrozenCardsAvailable(Constants.BoardCardsTag, 1))
                        {
                            odds = 6;
                        }
                        break;
                    case "fm1":
                        if (UnfrozenCardsAvailable(Constants.EnemyCardsTag, 1))
                        {
                            if (currentRank >= 7 && !AreBotCardsDuplicateOnBoard(true))
                            {
                                odds = 7;
                            }
                        }
                        break;
                    case "fm2":
                    case "wm1":
                    case "wm2":
                        if (currentRank >= 8)
                        {
                            if (turnCounter == 4 || turnCounter == 3)
                            {
                                odds = 4;
                            }
                            else if (turnCounter == 2 || turnCounter == 1)
                            {
                                odds = 8;
                            }
                        }
                        if (pu.Contains("w"))
                        {
                            if (!UnfrozenCardsAvailable(Constants.PlayerCardsTag, 2) || !UnfrozenCardsAvailable(Constants.EnemyCardsTag, 2))
                            {
                                odds = 0;
                            }
                            if (pu.Equals("wm2"))
                            {
                                if (!UnfrozenCardsAvailable(Constants.BoardCardsTag, battleSystem.cardsDeckUi.boardCardsUi.Count))
                                {
                                    odds = 0;
                                }
                            }
                        }


                        break;
                    case "im1":
                    case "im2":
                        if (currentRank <= 7)
                        {
                            if (turnCounter > 2)
                            {
                                odds = 7;
                            }
                        }
                        break;
                }
            }
        }
        if (pu.Contains("i") && turnCounter <= 2)
        {
            odds = 0;
        }
        return odds;
    }

    private bool PlayerGotFrozenCards(int whatCard)
    {
        if (whatCard == 2)
        {
            return battleSystem.cardsDeckUi.playerCardsUi[0].freeze || battleSystem.cardsDeckUi.playerCardsUi[1].freeze;
        }
        else
        {
            return battleSystem.cardsDeckUi.playerCardsUi[whatCard].freeze;
        }
    }
    private bool BotGotFrozenCards(int whatCard)
    {
        if (whatCard == 2)
        {
            return battleSystem.cardsDeckUi.enemyCardsUi[0].freeze || battleSystem.cardsDeckUi.enemyCardsUi[1].freeze;
        }
        else
        {
            return battleSystem.cardsDeckUi.enemyCardsUi[whatCard].freeze;
        }
    }

    private bool BoardHasFrozenCards()
    {
        foreach (CardUi card in battleSystem.cardsDeckUi.boardCardsUi)
        {
            if (card.freeze)
                return true;
        }
        return false;
    }

    private bool UnfrozenCardsAvailable(string cardsTag, int unfrozenCardsTarget)
    {
        int frozenCardsCounter = 0;
        int totalCards = 0;
        foreach (CardUi card in battleSystem.cardsDeckUi.GetListByTag(cardsTag))
        {
            totalCards++;
            if (card.freeze)
            {
                frozenCardsCounter++;
            }
        }
        if (totalCards - frozenCardsCounter >= unfrozenCardsTarget)
        {
            return true;
        }
        return false;
    }

    private int GetPuCost(string pu)
    {
        if (pu.Contains("m"))
        {
            return 2;
        }
        else
        {
            return 1;
        }
    }

    private bool EnemyBotGotDuplicateCard()
    {
        foreach (CardUi card in boardCardsUi)
        {
            int boardCardRank = GetRankFromCardDesc(card.cardDescription);
            if (card1 == boardCardRank)
            {
                card1DuplicateBoard = true;
            }
            if (card2 == boardCardRank)
            {
                card2DuplicateBoard = true;
            }
        }
        return card1DuplicateBoard || card2DuplicateBoard;
    }
    /*  private bool PlayerGotDuplicateCard()
      {
          if (playerRevealdCard > 0)
          {
              foreach (CardUi card in boardCardsUi)
              {
                  if (playerRevealdCard == GetRankFromCardDesc(card.cardDescription))
                  {
                      return true;
                  }
              }
          }
          return false;
      }*/
    private bool EnemyGotCardLowerThan(int cardValue)
    {

        return (card1 <= cardValue || card2 <= cardValue);
    }

    private List<int> CalculateDrawProbability(int amount)
    {
        return GenerateListProbability(EnemyActions.DrawPu, amount);
    }
    private List<int> CalculateRaiseProbability(int amount)
    {
        if (battleSystem.LoadPrefsInt(Constants.Instance.botRaiseKey) == 1 || !battleSystem.HaveEnoughToRaise())
        {
            amount = 0;
        }
        return GenerateListProbability(EnemyActions.OfferARaise, amount);
    }

    private List<int> CalculateSkillProbability(int amount)
    {
        if (esCounter != 3 || (botEsElement.Equals("w") && !UnfrozenCardsAvailable(Constants.AllCardsTag, 2)))
        {
            amount = 0;
        }
        else if (esCounter == 3)
        {
            switch (botEsElement)
            {
                case "f":
                    if (turnCounter < 5)
                    {
                        if (currentRank > 7)
                            amount += 6;
                        else
                            amount += 4;
                    }
                    break;
                case "i":
                    if (turnCounter < 5)
                    {
                        amount += 2;
                        if (turnCounter < 3)
                        {
                            if (!card1DuplicateBoard && BotGotFrozenCards(0) || !card2DuplicateBoard && BotGotFrozenCards(1))
                                amount += 6;
                            if (PlayerGotFrozenCards(2))
                                amount += 6;
                        }
                    }
                    break;
                case "w":
                    if (turnCounter < 3)
                    {
                        if (currentRank > 7)
                            amount += 6;
                        else if (currentRank == 7 && (!card1DuplicateBoard || !card2DuplicateBoard))
                            amount += 8;
                    }
                    break;
            }
        }
        return GenerateListProbability(EnemyActions.ElementSkill, amount);
    }




    private List<int> GenerateListProbability(EnemyActions skillUse, int amount)
    {
        List<int> listProbabilities = new List<int>();
        for (int i = 0; i < amount; i++)
        {
            listProbabilities.Add(skillUse.GetHashCode());
        }
        return listProbabilities;
    }

    private enum EnemyActions
    {
        EndTurn = 0,
        ElementSkill = 1,
        DrawPu = 2,
        Pu1Use = 3,
        Pu2Use = 4,
        PuRandomUse = 6,
        SendEmoji = 5,
        AcceptRaise = 7,
        RefuseRaise = 8,
        OfferARaise = 8,
    }

}