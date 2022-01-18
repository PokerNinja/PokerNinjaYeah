﻿using StandardPokerHandEvaluator;
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
    private int playerRevealdCard;
    private string pu1 = "";
    private string pu2 = "";

    private int pu1Odds = 0;
    private int pu2Odds = 0;
    public BotEnemy(BattleSystem battleSystem, int turnCounter) : base(battleSystem)
    {
        Debug.Log("BOTSTATER");
        this.battleSystem = battleSystem;
        this.turnCounter = turnCounter;
        if (turnCounter > 4)
        {
            energyLeft = 0;
        }
        else
        {
            energyLeft = GetLastEnergy();
        }
        ChreageEnergy();
        InitBotTurn();
    }

    private async void InitBotTurn()
    {
        await Task.Delay(1000);
        battleSystem.finishPuDissolve = true;
        pu1 = "";
        pu2 = "";
        card1 = GetEnemyCard(0);
        card2 = GetEnemyCard(1);
        currentRank = GetHandRank();
        playerRevealdCard = GetWhatPlayerCardRevealed();
        boardCardsUi = battleSystem.cardsDeckUi.boardCardsUi;
        CheckMyOptions();
    }

    private int GetWhatPlayerCardRevealed()
    {
        if (battleSystem.cardsDeckUi.playerCardsUi[0].cardMark.activeSelf)
        {
            return GetPlayerCard(0);
        }
        if (battleSystem.cardsDeckUi.playerCardsUi[1].cardMark.activeSelf)
        {
            return GetPlayerCard(1);
        }
        return 0;
    }

    private int GetEnemyCard(int index)
    {
        return Card.StringValueToInt(battleSystem.cardsDeckUi.enemyCardsUi[index].cardDescription.Substring(0, 1));
    }
    private int GetPlayerCard(int index)
    {
        return Card.StringValueToInt(battleSystem.cardsDeckUi.playerCardsUi[index].cardDescription.Substring(0, 1));
    }

    private int GetHandRank()
    {
        return battleSystem.Interface.ConvertHandRankToTextNumber(battleSystem.cardsDeckUi.CalculateHand(false, false, false, false).Rank);
    }

    private int GetLastEnergy()
    {
        return battleSystem.LoadPrefsInt(Constants.Instance.botEnergyKey.ToString());
    }

    private void ChreageEnergy()
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
        battleSystem.SavePrefsInt(Constants.Instance.botEnergyKey.ToString(), energyLeft);
    }

    private void CheckMyOptions()
    {
        List<int> drawness = new List<int>();
        List<int> skillness = new List<int>();
        List<int> puness = new List<int>();
        List<int> endness = new List<int>();
        int drawOdds = 0;
        int skillOdds = 0;
        int endTurnOdds = 0;
        switch (turnCounter)
        {
            case 6:
            case 5:
                battleSystem.enemyBotSkillUsed = false;
                drawOdds = 2;
                skillOdds = 4;
                if (energyLeft == 1)
                {
                    endTurnOdds = 2;
                }
                break;
            case 4:
            case 3:
                drawOdds = 3;
                skillOdds = 3;
                if (energyLeft == 1)
                {
                    endTurnOdds = 3;
                    drawOdds = 1;
                }
                break;
            case 2:
            case 1:
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
            endness = CalcualteEndTurnProbability(endTurnOdds);
            List<int> options = puness.Concat(skillness).Concat(drawness).Concat(endness).ToList();
            BotRandomAct(options);

            Debug.Log("total:" + options.Count);
            Debug.Log("pu: " + puness.Count + " skill:" + skillness.Count + " draw:" + drawness.Count + " end:" + endness.Count);
            Debug.Log("rank:" + currentRank);
            Debug.Log("hand:" + card1 + "," + card2);
            Debug.Log("energy:" + energyLeft);
        }
    }

    private async void BotRandomAct(List<int> options)
    {
        await Task.Delay(3000);
        int costOfAction = 0;
        int delay = 6000;
        if (options.Count > 0)
        {
            int act = options[battleSystem.GenerateRandom(0, options.Count)];
            switch (act)
            {
                case (int)EnemyActions.EndTurn:
                    {
                        //battleSystem.FakeEnemyEndTurn();
                        delay = 1000;
                        break;
                    }
                case (int)EnemyActions.SkillUse:
                    {
                        battleSystem.enemyBotSkillUsed = true;
                        battleSystem.FakeEnemyPuUse(-1,
                        battleSystem.cardsDeckUi.GetListByTag(Constants.PlayerCardsTag)[battleSystem.GenerateRandom(0, 2)].cardPlace, "", false);
                        costOfAction = 2;
                        delay -= 3000;
                        break;
                    }
                case (int)EnemyActions.DrawPu:
                    {
                        BotDraweCard();
                        costOfAction = 1;
                        delay -= 1500;
                        break;
                    }
                case (int)EnemyActions.Pu1Use:
                    {
                        BotPuUse(pu1, 0);
                        costOfAction = GetPuCost(pu1);
                        break;
                    }
                case (int)EnemyActions.Pu2Use:
                    {
                        BotPuUse(pu2, 1);
                        costOfAction = GetPuCost(pu2);
                        break;
                    }
                case (int)EnemyActions.PuRandomUse:
                    {
                        string[] pus = { pu1, pu2 };
                        int index = battleSystem.GenerateRandom(0, 2);
                        BotPuUse(pus[index], index);
                        costOfAction = GetPuCost(pus[index]);
                        break;
                    }
            }
            StartAutoEndWithDelay(delay, ReduceEnergy(costOfAction));
        }
        else
        {
            StartAutoEndWithDelay(2000, true);
        }



    }

    private void BotPuUse(string puName, int index)
    {
        string cardTarget1 = "";
        string cardTarget2 = "";
        string playerDuplicateOnBoard = GetDuplicateBoardCardWithPlayer(playerRevealdCard, true);
        switch (puName)
        {
            case "f1":
                cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", true, false, true);
                break;
            case "f2":
                if (playerRevealdCard > 0)
                {
                    cardTarget1 = GetDuplicateBoardCardWithPlayer(playerRevealdCard, true);
                }
                if (cardTarget1.Equals(""))
                {
                    cardTarget1 = GetCardOf(Constants.BoardCardsTag, "", true, false, true);
                }
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, cardTarget1, true, false, true);
                break;
            case "f3":
                if (!playerDuplicateOnBoard.Equals(""))
                {
                    cardTarget1 = GetPlayerRevealedCardAndIfFreeze(true, true);
                }
                else if (playerRevealdCard >= 13) // 13 == 7
                {
                    cardTarget1 = GetPlayerRevealedCardAndIfFreeze(true, true);
                }
                else
                {
                    cardTarget1 = GetPlayerRevealedCardAndIfFreeze(false, true);
                }
                break;
            case "i1":
                if (playerDuplicateOnBoard.Equals("") && playerRevealdCard <= 13)
                {
                    cardTarget1 = GetPlayerRevealedCardAndIfFreeze(true, false);
                }
                else
                {
                    cardTarget1 = GetPlayerRevealedCardAndIfFreeze(false, false);
                }
                break;
            case "i2":
                cardTarget1 = GetCardOf(Constants.BoardCardsTag, "", false, true, false);
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, cardTarget1, false, true, false);
                break;
            case "i3":
                cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", false, true, false);
                break;
            case "w1":
                if (!playerDuplicateOnBoard.Equals("") || playerRevealdCard > 13)
                {
                    cardTarget1 = GetPlayerRevealedCardAndIfFreeze(true, false);
                }
                else
                {
                    cardTarget1 = GetPlayerRevealedCardAndIfFreeze(false, false);
                }
                cardTarget2 = GetCardOf(Constants.EnemyCardsTag, "", true, false, false);
                break;
            case "w2":
                cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", true, false, false);
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, "", false, true, false);
                break;
            case "w3":
                if (!playerDuplicateOnBoard.Equals(""))
                {
                    cardTarget1 = GetPlayerRevealedCardAndIfFreeze(true, false);
                }
                else if (playerRevealdCard >= 13) // 13 == 7
                {
                    cardTarget1 = GetPlayerRevealedCardAndIfFreeze(true, false);
                }
                else
                {
                    cardTarget1 = GetPlayerRevealedCardAndIfFreeze(false, false);
                }
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, "", true, false, false);
                break;
            case "fm1":
                cardTarget1 = GetCardOf(Constants.DeckCardsTag, "", false, true, false);
                cardTarget2 = GetCardOf(Constants.EnemyCardsTag, "", true, false, false);
                break;
        }
        Debug.LogWarning("pu " + index + " c " + cardTarget1 + " , " + cardTarget2);
        battleSystem.FakeEnemyPuUse(index, cardTarget1, cardTarget2, false);
    }

    private string GetPlayerRevealedCardAndIfFreeze(bool reveald, bool canTargetFreeze)
    {
        string cardPlace = GetPlayerRevealedCard(reveald);
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
    }
    private string GetPlayerRevealedCard(bool revealed)
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
    }

    private string GetDuplicateBoardCardWithPlayer(int playerRevealdCard, bool canTargetFreeze)
    {
        List<CardUi> boardsCards = new List<CardUi>(battleSystem.cardsDeckUi.GetListByTag(Constants.BoardCardsTag).ToArray());

        for (int i = 0; i < boardsCards.Count; i++)
        {
            if (canTargetFreeze || !boardsCards[i].freeze)
            {
                if (Card.StringValueToInt(boardsCards[i].cardDescription.Substring(0, 1)) == playerRevealdCard)
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

    private string GetCardOf(string cardsTag, string differentThan, bool lowest, bool isDuplicate, bool canTargetFreeze)
    {

        List<CardUi> cards = new List<CardUi>(battleSystem.cardsDeckUi.GetListByTag(cardsTag).ToArray());
        if (lowest)
        {
            cards = cards.OrderBy(h => Card.StringValueToInt(h.cardDescription[0].ToString())).ToList<CardUi>();
        }
        else
        {
            cards = cards.OrderByDescending(h => Card.StringValueToInt(h.cardDescription[0].ToString())).ToList<CardUi>();
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

                    if (!cards[i].cardPlace.Equals(differentThan))
                    {
                        int boardCardRank = Card.StringValueToInt(boardCards[j].cardDescription.Substring(0, 1));
                        int currentCardRank = Card.StringValueToInt(cards[i].cardDescription.Substring(0, 1));
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
        if (cards[0].cardPlace.Equals(differentThan))
        {
            Debug.Log("toYse " + cards[0].cardPlace);
            Debug.Log("differentThan " + differentThan);
            return cards[1].cardPlace;
        }
        else
        {
            Debug.Log("ChoseFirst " + cards[0].cardPlace);
            return cards[0].cardPlace;
        }
    }
    private string GetRandomCardOf(string cardsTag)
    {
        return battleSystem.cardsDeckUi.GetListByTag(cardsTag)[battleSystem.GenerateRandom(0, 2)].cardPlace;
    }

    private bool ReduceEnergy(int energyToReduce)
    {
        energyLeft -= energyToReduce;
        Debug.Log("energyLeft" + energyLeft);
        battleSystem.SavePrefsInt(Constants.Instance.botEnergyKey.ToString(), energyLeft);
        return energyLeft == 0 || energyToReduce == 0;
    }

    private List<int> CalcualteEndTurnProbability(int endTurnOdds)
    {

        return GenerateListProbability(EnemyActions.EndTurn, endTurnOdds);

    }

    private List<int> CalculatePuUseProbability()
    {
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
                        if (PlayerGotDuplicateCard())
                        {
                            odds = 8;
                        }
                        break;
                    case "f3":
                        if (PlayerGotDuplicateCard())
                        {
                            odds = 10;
                        }
                        else
                        {
                            odds = 1;
                        }
                        break;

                    case "i1":
                        if (UnfrozenCardsAvailable(Constants.PlayerCardsTag, 1))
                        {
                            if (!PlayerGotDuplicateCard())
                            {
                                if (playerRevealdCard > 0 && playerRevealdCard <= 7)
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
                    case "i2":
                        if (UnfrozenCardsAvailable(Constants.BoardCardsTag, 2))
                        {

                            if (AreBotCardsDuplicateOnBoard(true))
                            {
                                odds = 10;
                            }
                            else if (AreBotCardsDuplicateOnBoard(false))
                            {
                                odds = 6;
                            }
                        }
                        break;
                    case "i3":
                        if (UnfrozenCardsAvailable(Constants.EnemyCardsTag, 1))
                        {

                            if (AreBotCardsDuplicateOnBoard(false))
                            {
                                odds = 8;
                            }
                        }
                        break;
                    case "w1":
                        if (UnfrozenCardsAvailable(Constants.PlayerCardsTag, 1) && UnfrozenCardsAvailable(Constants.EnemyCardsTag, 1))
                        {
                            if (!AreBotCardsDuplicateOnBoard(false))
                            {
                                if (PlayerGotDuplicateCard())
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

                            if (PlayerGotDuplicateCard() && !AreBotCardsDuplicateOnBoard(false))
                            {
                                odds = 8;
                            }
                            else if (!PlayerGotDuplicateCard())
                            {
                                if ((card1DuplicateBoard && !card2DuplicateBoard) || (!card1DuplicateBoard && card2DuplicateBoard))
                                    odds = 3;
                            }
                        }
                        break;
                    case "w3":
                        if (UnfrozenCardsAvailable(Constants.PlayerCardsTag, 1) && UnfrozenCardsAvailable(Constants.BoardCardsTag, 1))
                        {
                            if (PlayerGotDuplicateCard())
                            {
                                odds = 10;
                            }
                            else if (playerRevealdCard > 7)
                            {
                                odds = 6;
                            }
                            else //other player card Not revealed
                            {
                                odds = 3;
                            }
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
            int boardCardRank = Card.StringValueToInt(card.cardDescription.Substring(0, 1));
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
    private bool PlayerGotDuplicateCard()
    {
        if (playerRevealdCard > 0)
        {
            foreach (CardUi card in boardCardsUi)
            {
                int boardCardRank = Card.StringValueToInt(card.cardDescription.Substring(0, 1));
                if (playerRevealdCard == boardCardRank)
                {
                    return true;
                }
            }
        }
        return false;
    }
    private bool EnemyGotCardLowerThan(int cardValue)
    {

        return (card1 <= cardValue || card2 <= cardValue);
    }

    private List<int> CalculateDrawProbability(int amount)
    {
        return GenerateListProbability(EnemyActions.DrawPu, amount);
    }

    private List<int> CalculateSkillProbability(int amount)
    {
        if (battleSystem.enemyBotSkillUsed || energyLeft == 1)
        {
            amount = 0;
        }
        return GenerateListProbability(EnemyActions.SkillUse, amount);
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
        SkillUse = 1,
        DrawPu = 2,
        Pu1Use = 3,
        Pu2Use = 4,
        PuRandomUse = 6,
        SendEmoji = 5,
    }

}