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
    private int playerRevealdCard;
    private string pu1 = "";
    private string pu2 = "";
    public BotEnemy(BattleSystem battleSystem, int turnCounter) : base(battleSystem)
    {
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

    private void InitBotTurn()
    {
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
        if (turnCounter >= 1 && turnCounter <= 5)
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
                skillOdds = 3;
                if (energyLeft == 1)
                {
                    endTurnOdds = 2;
                }
                break;
            case 4:
            case 3:
                drawOdds = 3;
                skillOdds = 2;
                if (energyLeft == 1)
                {
                    endTurnOdds = 3;
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
            StartAutoEndWithDelay(3000, true);
        }
        else
        {

            puness = CalculatePuUseProbability();
            skillness = CalculateSkillProbability(skillOdds);
            drawness = CalculateDrawProbability(drawOdds);
            endness = CalcualteEndTurnProbability(endTurnOdds);
            List<int> options = puness.Concat(skillness).Concat(drawness).Concat(endness).ToList();
            BotRandomAct(options);

            Debug.Log("i " + options.Count);
            Debug.Log("puness " + puness.Count);
            Debug.Log("skillness " + skillness.Count);
            Debug.Log("drawness " + drawness.Count);
            Debug.Log("endness " + endness.Count);
        }
    }

    private async void BotRandomAct(List<int> options)
    {
        await Task.Delay(3000);
        int costOfAction = 0;
        int delay = 5000;
        if (options.Count > 0)
        {
            int act = options[battleSystem.GenerateRandom(0, options.Count - 1)];
            switch (act)
            {
                case (int)EnemyActions.EndTurn:
                    {
                        battleSystem.FakeEnemyEndTurn();
                        break;
                    }
                case (int)EnemyActions.SkillUse:
                    {
                        battleSystem.enemyBotSkillUsed = true;
                        battleSystem.FakeEnemyPuUse(-1,
                        battleSystem.cardsDeckUi.GetListByTag(Constants.PlayerCardsTag)[battleSystem.GenerateRandom(0, 1)].cardPlace, "", false);
                        costOfAction = 2;
                        delay -= 1500;
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
                        int index = battleSystem.GenerateRandom(0, 1);
                        BotPuUse(pus[index], index);
                        costOfAction = GetPuCost(pus[index]);
                        break;
                    }
            }
            StartAutoEndWithDelay(delay, ReduceEnergy(costOfAction));
        }
        else
        {
            StartAutoEndWithDelay(delay, true);
        }



    }

    private void BotPuUse(string puName, int index)
    {
        string cardTarget1 = "";
        string cardTarget2 = "";
        string playerDuplicateOnBoard = GetDuplicateBoardCardWithPlayer(playerRevealdCard);
        switch (puName)
        {
            case "f1":
                cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", true, false);
                break;
            case "f2":
                if (playerRevealdCard > 0)
                {
                    cardTarget1 = GetDuplicateBoardCardWithPlayer(playerRevealdCard);
                }
                if (cardTarget1.Equals(""))
                {
                    cardTarget1 = GetCardOf(Constants.BoardCardsTag, "", true, false);
                }
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, cardTarget1, true, false);
                break;
            case "f3":
                if (!playerDuplicateOnBoard.Equals(""))
                {
                    cardTarget1 = GetPlayerRevealedCard(true);
                }
                else if (playerRevealdCard >= 13) // 13 == 7
                {
                    cardTarget1 = GetPlayerRevealedCard(true);
                }
                else
                {
                    cardTarget1 = GetPlayerRevealedCard(false);
                }
                break;
            case "i1":
                if (playerDuplicateOnBoard.Equals("") && playerRevealdCard <= 13)
                {
                    cardTarget1 = GetPlayerRevealedCard(true);
                }
                else
                {
                    cardTarget1 = GetPlayerRevealedCard(false);
                }
                break;
            case "i2":
                cardTarget1 = GetCardOf(Constants.BoardCardsTag, "", false, false);
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, cardTarget1, false, false);
                break;
            case "i3":
                cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", false, true);
                break;
            case "w1":
                if (!playerDuplicateOnBoard.Equals("") || playerRevealdCard > 13)
                {
                    cardTarget1 = GetPlayerRevealedCard(true);
                }
                else
                {
                    cardTarget1 = GetPlayerRevealedCard(false);
                }
                cardTarget2 = GetCardOf(Constants.EnemyCardsTag, "", true, false);
                break;
            case "w2":
                cardTarget1 = GetCardOf(Constants.EnemyCardsTag, "", true, false);
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, "", false, true);
                break;
            case "w3":
                if (!playerDuplicateOnBoard.Equals(""))
                {
                    cardTarget1 = GetPlayerRevealedCard(true);
                }
                else if (playerRevealdCard >= 13) // 13 == 7
                {
                    cardTarget1 = GetPlayerRevealedCard(true);
                }
                else
                {
                    cardTarget1 = GetPlayerRevealedCard(false);
                }
                cardTarget2 = GetCardOf(Constants.BoardCardsTag, "", true, false);
                break;
            case "fm1":
                cardTarget1 = GetCardOf(Constants.DeckCardsTag, "", false, true);
                cardTarget2 = GetCardOf(Constants.EnemyCardsTag, "", true, false);
                break;
        }
        Debug.LogWarning("pu " + index + " c " + cardTarget1 + " , " + cardTarget2);
        battleSystem.FakeEnemyPuUse(index, cardTarget1, cardTarget2, false);
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
        return battleSystem.cardsDeckUi.playerCardsUi[battleSystem.GenerateRandom(0, 1)].cardPlace;
    }

    private string GetDuplicateBoardCardWithPlayer(int playerRevealdCard)
    {
        List<CardUi> boardsCards = new List<CardUi>(battleSystem.cardsDeckUi.GetListByTag(Constants.BoardCardsTag).ToArray());

        for (int i = 0; i < boardsCards.Count; i++)
        {
            if (Card.StringValueToInt(boardsCards[i].cardDescription.Substring(0, 1)) == playerRevealdCard)
            {
                return boardsCards[i].cardPlace;
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
            battleSystem.ReplacePu(false, 0);
        }

    }

    private async void StartAutoEndWithDelay(int delay, bool autoEnd)
    {
        await Task.Delay(delay);
        if (autoEnd)
        {
            battleSystem.FakeEnemyEndTurn();
        }
        else
        {
            InitBotTurn();
        }

    }

    private string GetCardOf(string cardsTag, string differentThan, bool lowest, bool isDuplicate)
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
                if (!cards[i].Equals(differentThan))
                {
                    Debug.Log("current " + cards[i].cardPlace);
                    Debug.Log("differentThan " + differentThan);
                    int boardCardRank = Card.StringValueToInt(boardCards[j].cardDescription.Substring(0, 1));
                    int currentCardRank = Card.StringValueToInt(cards[i].cardDescription.Substring(0, 1));
                    if (isDuplicate)
                    {
                        if (currentCardRank == boardCardRank)
                        {
                            return cards[i].cardPlace;
                        }
                    }
                    else
                    {
                        if (currentCardRank != boardCardRank)
                        {
                            return cards[i].cardPlace;
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
            return cards[0].cardPlace;
        }
    }
    private string GetRandomCardOf(string cardsTag)
    {
        return battleSystem.cardsDeckUi.GetListByTag(cardsTag)[battleSystem.GenerateRandom(0, 1)].cardPlace;
    }

    private bool ReduceEnergy(int energyToReduce)
    {
        energyLeft -= energyToReduce;
        Debug.Log("energyToReduce" + energyLeft);
        battleSystem.SavePrefsInt(Constants.Instance.botEnergyKey.ToString(), energyLeft);
        return energyLeft == 0;
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
        int pu1Odds = CalculatePu(pu1);
        int pu2Odds = CalculatePu(pu2);
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
                        if (currentRank >= 7 && EnemyGotCardLowerThan(7) && (!card1DuplicateBoard || !card2DuplicateBoard))
                        {
                            odds = 5;
                        }
                        break;
                    case "f2":
                        if (currentRank >= 7)
                        {
                            if (boardCardsUi.Count > 3)
                            {
                                if (!card1DuplicateBoard || !card2DuplicateBoard)
                                {
                                    odds = 4;
                                }
                                else if (card1DuplicateBoard && card1DuplicateBoard)
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
                        if (!PlayerGotDuplicateCard())
                        {
                            if (playerRevealdCard < 7)
                            {
                                odds = 8;
                            }
                            else
                            {
                                odds = 3;
                            }
                        }
                        break;
                    case "i2":
                        if (card1DuplicateBoard && card2DuplicateBoard)
                        {
                            odds = 10;
                        }
                        else if (card1DuplicateBoard || card2DuplicateBoard)
                        {
                            odds = 6;
                        }
                        break;
                    case "i3":
                        if (card1DuplicateBoard || card2DuplicateBoard)
                        {
                            odds = 8;
                        }
                        break;
                    case "w1":
                        if (PlayerGotDuplicateCard() && (!card1DuplicateBoard || !card2DuplicateBoard))
                        {
                            odds = 8;
                        }
                        else if (!card1DuplicateBoard || !card2DuplicateBoard)
                        {
                            odds = 3;
                        }
                        break;
                    case "w2":
                        if (PlayerGotDuplicateCard() && (!card1DuplicateBoard || !card2DuplicateBoard))
                        {
                            odds = 8;
                        }
                        else if (!PlayerGotDuplicateCard())
                        {
                            if ((card1DuplicateBoard && !card2DuplicateBoard) || (!card1DuplicateBoard && card2DuplicateBoard))
                                odds = 3;
                        }
                        break;
                    case "w3":
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
                        break;
                    case "fm1":
                        if (currentRank >= 7 && (!card1DuplicateBoard || !card2DuplicateBoard))
                        {
                            odds = 7;
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