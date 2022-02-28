using Com.InfallibleCode.TurnBasedGame.Combat;
using StandardPokerHandEvaluator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

public class EndRound : State
{
    private bool isTimedOut;
    private bool endByCards;
    private bool isPlayerPreWin;
    bool startNewRound = false;

    public EndRound(BattleSystem battleSystem, bool endByCards, bool isPlayerPreWin) : base(battleSystem)
    {
        Debug.Log("eniding ROund ()()()()()");
        this.endByCards = endByCards;
        this.isPlayerPreWin = isPlayerPreWin;
    }

    public override IEnumerator Start()
    {
        battleSystem.PlayMusic(false);
        battleSystem.ResetTimers();
        battleSystem.ActivatePlayerButtonsOut(false, false);
        battleSystem.Interface.SetTurnIndicator(false, false);
        battleSystem.Interface.EnableVisionClick(false);
        battleSystem.DeactivateSmoke();
        // yield return new WaitForSeconds(2.5f);
        //bool playerStartNextRound = battleSystem.FirstToPlay(false); // MAYBE here 

        string winningText = "ERROR";
        //  yield return new WaitForSeconds(1f);


        if (endByCards)
        {
            battleSystem.RevealEnemyCards();
            //  yield return new WaitForSeconds(0.7f);
            battleSystem.RevealBoardCards();
            yield return new WaitForSeconds(1.5f);
            winningText = WinnerCalculator();
        }
        else
        {
            winningText = EndByBetCalculator();
        }
        battleSystem.DisplayWinner(winningText);
    }

    private string EndByBetCalculator()
    {
        string text;
        string enemyId = battleSystem.Interface.RichText(battleSystem.enemy.id.Substring(1), Values.Instance.yellowText, true);
        if (isPlayerPreWin)
        {
            text = enemyId + " Refuse your bet. You Win!";
        }
        else
        {
            text = "You refuse " + enemyId + " bet. " + enemyId + " win!";
        }

        startNewRound = !battleSystem.DealHpDamage(!isPlayerPreWin, true);
        battleSystem.EndRoundVisual(isPlayerPreWin);
        //TODO What abot alex
        if (battleSystem.ShouldFlipTurnAfterBetDecline())
        {
            battleSystem.FlipTurnAfterDecline = true;
            if (battleSystem.IsPlayerTurn() && !battleSystem.BOT_MODE)
            {
                LocalTurnSystem.Instance.SyncStarterAfterEnd();
            }
        }

        StartNewRoundAction(true);
        return text;
    }

    public string WinnerCalculator()
    {
        bool isFlusher = Values.Instance.flusherOn;
        bool isStrighter = Values.Instance.strighterOn;
        //MAKE IT BETTER
        Hand bestOpponentHand = battleSystem.cardsDeckUi.CalculateHand(true, false, battleSystem.isEnemyFlusher, battleSystem.isEnemyStrighter);
        if (battleSystem.enemyHandIsFlusher)
        {
            bestOpponentHand = battleSystem.cardsDeckUi.ReplaceCardToFlusher(false, bestOpponentHand);
        }
        else if (battleSystem.enemyHandIsStrighter)
        {
            bestOpponentHand = battleSystem.cardsDeckUi.ReplaceCardToStrighter(false, bestOpponentHand);
        }

        Hand bestPlayerHand = battleSystem.cardsDeckUi.CalculateHand(true, true, battleSystem.isPlayerFlusher, battleSystem.isPlayerStrighter);
        if (battleSystem.playerHandIsFlusher)
        {
            bestPlayerHand = battleSystem.cardsDeckUi.ReplaceCardToFlusher(true, bestPlayerHand);
        }
        else if (battleSystem.playerHandIsStrighter)
        {
            Debug.LogError("Effect Strighter");
            bestPlayerHand = battleSystem.cardsDeckUi.ReplaceCardToStrighter(true, bestPlayerHand);
        }
        battleSystem.Interface.UpdateCardRank(bestPlayerHand.Rank);
        string winnerMsg = "";
        // Opponent win
        // Make it better
        if (bestPlayerHand.Rank.CompareTo(bestOpponentHand.Rank) == 1)
        {
            string enemyId = battleSystem.Interface.RichText(battleSystem.enemy.id.Substring(1), Values.Instance.yellowText, true);
            // SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Lose, true);
            if (battleSystem.DealDamage(true))
            {
                //battleSystem.gameOver = false;
                startNewRound = false;
                winnerMsg = enemyId + " Wins The Game With ";
                battleSystem.SetState(new GameOver(battleSystem, false));
            }
            else
            {
                startNewRound = true;
                winnerMsg = enemyId + " Wins With ";
            }
            if (battleSystem.cardsDeckUi.enemyShadowCard.Equals("x"))
            {
                AnimateWinWithHand(bestOpponentHand, false);
            }
            else
            {
                battleSystem.cardsDeckUi.CreateShadowCard(battleSystem.cardsDeckUi.enemyShadowCard, () => AnimateWinWithHand(bestOpponentHand, false));
            }

            winnerMsg += battleSystem.Interface.ConvertHandRankToTextDescription(bestOpponentHand.Rank); ;
            //  winnerMsg += bestOpponentHand.ToString(Hand.HandToStringFormatEnum.HandDescription);
        }
        // Player win
        else if (bestPlayerHand.Rank.CompareTo(bestOpponentHand.Rank) == -1)
        {
            string playerId = battleSystem.Interface.RichText(battleSystem.player.id.Substring(1), Values.Instance.yellowText, true);
            // SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Win, true);
            if (battleSystem.DealDamage(false))
            {
                startNewRound = false;
                winnerMsg = playerId + " Wins The Game With ";
                battleSystem.SetState(new GameOver(battleSystem, true));
            }
            else
            {
                startNewRound = true;

                winnerMsg = playerId + " Wins With ";
            }
            if (battleSystem.cardsDeckUi.playerShadowCard.Equals("x"))
            {
                AnimateWinWithHand(bestPlayerHand, true);
            }
            else
            {
                battleSystem.cardsDeckUi.CreateShadowCard(battleSystem.cardsDeckUi.playerShadowCard, () => AnimateWinWithHand(bestPlayerHand, true));
            }
            winnerMsg += battleSystem.Interface.ConvertHandRankToTextDescription(bestPlayerHand.Rank);
            // winnerMsg += bestPlayerHand.ToString(Hand.HandToStringFormatEnum.HandDescription);
        }
        else
        {
            battleSystem.Interface.tieTitle.SetActive(true);
            startNewRound = true;
            winnerMsg = battleSystem.Interface.RichText("TIE ", Values.Instance.yellowText, true) + bestOpponentHand.ToString(Hand.HandToStringFormatEnum.HandDescription);
            StartNewRoundAction(true);
        }

        /* if (startNewRound && enemyStartsNextRound)
         {

             battleSystem.DeckGeneratorDB();

         }
         else if (startNewRound)
         {
            // battleSystem.SetIsPlayerReady(false);

             //battleSystem.StartNewRound();
            // battleSystem.Interface.playerNameText.text = "WAIT" + battleSystem.playerLifeLeft + ", " +battleSystem.enemyLifeLeft;
         }*/
        return winnerMsg;
    }


    private void AnimateWinWithHand(Hand winningHand, bool isPlayerWin)
    {
        Debug.Log("animata");
        //MAKE IT BETTER
        List<Card> winningCards = winningHand.getCards();
        List<CardUi> winningPlayersCards = new List<CardUi>();
        List<CardUi> winningBoardCards = new List<CardUi>();
        List<CardUi> losingBoardCards = new List<CardUi>();
        List<CardUi> boardCardsUi = new List<CardUi>();
        List<CardUi> playerCardsUi = new List<CardUi>();
        List<CardUi> EnemyCardsUi = new List<CardUi>();
        boardCardsUi.AddRange(battleSystem.cardsDeckUi.GetListByTag("CardB"));
        losingBoardCards.AddRange(boardCardsUi);
        playerCardsUi.AddRange(battleSystem.cardsDeckUi.GetListByTag("CardP"));
        EnemyCardsUi.AddRange(battleSystem.cardsDeckUi.GetListByTag("CardE"));
        CardUi card1, card2, cardGhost;
        if (isPlayerWin)
        {
            card1 = playerCardsUi[0];
            card2 = playerCardsUi[1];
        }
        else
        {
            card1 = EnemyCardsUi[0];
            card2 = EnemyCardsUi[1];
        }
        string cardGhostStr = "x";
        string cardGhostOwener = "x";
        if (battleSystem.cardsDeckUi.shadowCardUi != null)
        {
            winningPlayersCards.Add(battleSystem.cardsDeckUi.shadowCardUi);
        }
        if (battleSystem.cardsDeckUi.ghostCardUi != null)
        {
            cardGhost = battleSystem.cardsDeckUi.ghostCardUi;
            cardGhostStr = cardGhost.cardDescription.ToString();
            cardGhostOwener = cardGhost.cardPlace.ToString();
            if (cardGhostOwener.Contains("Player") && isPlayerWin)
            {

            }
            else if (cardGhostOwener.Contains("Enemy") && !isPlayerWin)
            {

            }
            else if (cardGhostOwener.Contains("B"))
            {

            }
            else
            {
                cardGhostStr = "x";
            }
        }
        string card1Str = card1.cardDescription.ToString();// Cant catch properly. catch it first place 
        string card2Str = card2.cardDescription.ToString();
        string winningCardDesc;
        for (int i = 0; i < 5; i++)
        {
            winningCardDesc = winningCards[i].ToString(CardToStringFormatEnum.ShortCardName);
            if (card1Str.Equals(winningCardDesc))
            {
                if (card1.freeze)
                {
                    //TODO make it better
                    battleSystem.Interface.FreezeObject(card1.spriteRenderer, false, card1.GetisFaceDown(), null, false);
                }
                winningPlayersCards.Add(card1);
            }
            if (card2Str.Equals(winningCardDesc))
            {
                if (card2.freeze)
                {
                    battleSystem.Interface.FreezeObject(card2.spriteRenderer, false, card2.GetisFaceDown(), null, false);
                    //TODO make it better
                }
                winningPlayersCards.Add(card2);
            }
            if (cardGhostStr.Equals(winningCardDesc))
            {
                winningPlayersCards.Add(battleSystem.cardsDeckUi.ghostCardUi);
            }

            for (int j = 0; j < 5; j++)
            {
                if (boardCardsUi[j].cardDescription.ToString().Equals(winningCardDesc))
                {
                    winningBoardCards.Add(boardCardsUi[j]);
                    j = 5;
                }
            }
        }
        losingBoardCards.RemoveAll(item => winningBoardCards.Contains(item));
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndRoundGong, true);
        /* AnimationManager.Instance.AnimateWinningHandToBoard(winningPlayersCards, losingBoardCards,
             () =>
             {
                 battleSystem.EndRoundVisual(isPlayerWin);
                 StartNewRoundAction(false);
             }); */

        battleSystem.Interface.UpdateVisionColor(battleSystem.Interface.ConvertHandRankToTextNumber(winningHand.Rank));
        winningPlayersCards = winningPlayersCards.Concat(winningBoardCards).ToList();
        winningPlayersCards = RearangeWinningCards(winningPlayersCards, winningHand.Rank);
        AnimationManager.Instance.AnimateWinningHandToBoard2(winningPlayersCards,
           ConvertRankToCardToGlow(battleSystem.Interface.ConvertHandRankToTextNumber(winningHand.Rank)),
           losingBoardCards, battleSystem.cardsDeckUi.boardTransform,
           () =>
           {
               battleSystem.EndRoundVisual(isPlayerWin);
               StartNewRoundAction(true);
           });
    }

    private int ConvertRankToCardToGlow(int rank)
    {
        switch (rank)
        {
            case 10:
                return 1;
            case 9:
                return 2;
            case 8:
            case 3:
                return 4;
            case 7:
                return 3;/*
            case 6:
            case 5:
            case 4:
            case 2:
            case 1:
                return 5;*/
        }
        return 5;
    }

    private bool IsStrightFlushOrFull(int rank)
    {
        return (rank <= 1609 && rank >= 167) || rank <= 10;
    }
    private bool IsFourOfAKind(int rank)
    {
        return (rank <= 166 && rank >= 11);
    }

    private List<CardUi> RearangeWinningCards(List<CardUi> winningHand, int handRank)
    {
        List<CardUi> arangedCards = new List<CardUi>();

        winningHand = winningHand.OrderByDescending(h => Card.StringValueToInt(h.cardDescription[0].ToString())).ToList<CardUi>();
        foreach (CardUi cardUi in winningHand)
        {
            cardUi.EnableSelecetPositionZ(true);
        }
        if (IsStrightFlushOrFull(handRank))
        {
            if (handRank >= 1600 && Card.StringValueToInt(winningHand[0].cardDescription[0].ToString()) == CardEnum.Ace.GetHashCode() && Card.StringValueToInt(winningHand[1].cardDescription[0].ToString()) == CardEnum.Five.GetHashCode())
            {
                winningHand = MoveFirstItemToLast(winningHand);
            }
            return winningHand;
        }
        else if (IsFourOfAKind(handRank))
        {
            if (winningHand[0].cardDescription[0].ToString() != winningHand[1].cardDescription[0].ToString())
            {
                winningHand = MoveFirstItemToLast(winningHand);
            }
            return winningHand;
        }
        else
        {
            List<CardUi> repetitive = new List<CardUi>();
            string lastRepeatValue = "";
            for (int i = 4; i > 0; i--)
            {
                if (lastRepeatValue == winningHand[i].cardDescription[0].ToString())
                {
                    repetitive.Add(winningHand[i]);
                    winningHand.RemoveAt(i);
                }
                else if (winningHand[i].cardDescription[0].ToString() == winningHand[i - 1].cardDescription[0].ToString())
                {
                    lastRepeatValue = winningHand[i].cardDescription[0].ToString();
                    repetitive.Add(winningHand[i]);
                    repetitive.Add(winningHand[i - 1]);
                    winningHand.RemoveAt(i);
                    winningHand.RemoveAt(i - 1);
                    i--;
                }
            }
            arangedCards = arangedCards.Concat(repetitive).ToList();
            arangedCards = arangedCards.OrderByDescending(h => Card.StringValueToInt(h.cardDescription[0].ToString())).ToList<CardUi>();
        }
        arangedCards = arangedCards.Concat(winningHand).ToList();
        return arangedCards;
    }

    private List<CardUi> MoveFirstItemToLast(List<CardUi> winningHand)
    {
        CardUi cardToMove = winningHand[0];
        winningHand.RemoveAt(0);
        winningHand.Add(cardToMove);
        return winningHand;
    }

    private void StartNewRoundAction(bool delay)
    {

        if (startNewRound)
        {
            battleSystem.endRoutineFinished = true;
            battleSystem.StartNewRoundRoutine(delay);
        }
    }
}
