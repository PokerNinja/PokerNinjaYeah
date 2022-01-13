using StandardPokerHandEvaluator;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class BotEnemy : State
{
    private int turnCounter;
    public List<int> playOptions;
    public int energyLeft;
    public int currentRank;
    public int card1;
    public int card2;
    public bool card1DuplicateBoard;
    public bool card2DuplicateBoard;
    public int boardCardsAmount;
    private List<CardUi> boardCardsUi;
    public int playerRevealdCard;

    public BotEnemy(BattleSystem battleSystem, int turnCounter) : base(battleSystem)
    {
        this.battleSystem = battleSystem;
        this.turnCounter = turnCounter;
        card1 = GetEnemyCard(0);
        card2 = GetEnemyCard(1);
        energyLeft = GetLastEnergy();
        currentRank = GetHandRank();
        playerRevealdCard = GetWhatPlayerCardRevealed();
        boardCardsUi = battleSystem.cardsDeckUi.boardCardsUi;
        ChreageEnergy();
        CheckMyOptions();
    }

    private int GetWhatPlayerCardRevealed()
    {
        if (battleSystem.cardsDeckUi.playerCardsUi[0].cardMark)
        {
            return GetPlayerCard(0);
        }
        if (battleSystem.cardsDeckUi.playerCardsUi[1].cardMark)
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
        int drawOdds = 0;
        int skillOdds = 0;
        switch (turnCounter)
        {
            case 1:
            case 2:
                battleSystem.enemyBotSkillUsed = false;
                drawOdds = 2;
                skillOdds = 3;
                break;
            case 3:
            case 4:
                drawOdds = 3;
                skillOdds = 2;
                break;
            case 5:
            case 6:
                if (energyLeft > 1)
                {
                    drawOdds = 2;
                }
                break;
        }

        puness = CalculatePuUseProbability();
        skillness = CalculateSkillProbability(skillOdds);
        drawness = CalculateDrawProbability(drawOdds);

    }

    private List<int> CalculatePuUseProbability()
    {
        EnemyGotDuplicateCard();
        string pu1 = battleSystem.puDeckUi.GetPu(false, 0).puName;
        string pu2 = battleSystem.puDeckUi.GetPu(false, 1).puName;
        int pu1Odds = CalculatePu(pu1);
    }

    private int CalculatePu(string pu)
    {
        int odds = 0;
        int puCost = 1;
        if (pu.Contains("m"))
        {
            puCost = 2;
        }
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
                            odds = 5;
                    }
                    break;
                case "w3":
                    if (PlayerGotDuplicateCard())
                    {
                        odds = 10;
                    }
                    else if(playerRevealdCard > 7)
                    {
                        odds = 6;
                    }
                    else //other player card Not revealed
                    {
                        odds = 3;
                    }
                    break;
                case "fm1":
                    break;
                case "fm2":
                    break;
                case "im1":
                    break;
                case "im2":
                    break;
                case "wm1":
                    break;
                case "wm2":
                    break;
            }
        }
        return odds;
    }


    private bool EnemyGotDuplicateCard()
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
        if (!battleSystem.enemyBotSkillUsed)
        {
            return GenerateListProbability(EnemyActions.SkillUse, amount);
        }
        return null;
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
        PuUse1 = 3,
        PuUse2 = 4,
        SendEmoji = 5,
    }

}