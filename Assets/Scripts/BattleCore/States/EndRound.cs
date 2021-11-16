using Com.InfallibleCode.TurnBasedGame.Combat;
using StandardPokerHandEvaluator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class EndRound : State
{
    private bool isTimedOut;
    private bool enemyStartsNextRound;
    bool startNewRound = false;

    public EndRound(BattleSystem battleSystem, bool enemyStartsNextRound, bool isTimedOut) : base(battleSystem)
    {
        this.isTimedOut = isTimedOut;
        this.enemyStartsNextRound = enemyStartsNextRound;
    }

    public override IEnumerator Start()
    {
        battleSystem.PlayMusic(false);
        battleSystem.ResetTimers();
        battleSystem.Interface.EnablePlayerButtons(false);
        battleSystem.Interface.SetTurnIndicator(false, false);
        battleSystem.Interface.EnableVisionClick(false);

        // yield return new WaitForSeconds(2.5f);
        //bool playerStartNextRound = battleSystem.FirstToPlay(false); // MAYBE here 

        string winningText = "ERROR";
      //  yield return new WaitForSeconds(1f);
        battleSystem.RevealEnemyCards();
      //  yield return new WaitForSeconds(0.7f);
        battleSystem.RevealBoardCards();
        yield return new WaitForSeconds(1.5f);

        if (!isTimedOut)
        {
            winningText = WinnerCalculator();
        }
        battleSystem.DisplayWinner(winningText);
    }

    public string WinnerCalculator()
    {
        //MAKE IT BETTER
        Hand bestOpponentHand = battleSystem.cardsDeckUi.CalculateHand(true);
        Hand bestPlayerHand = battleSystem.cardsDeckUi.CalculateHand(false);
        string winnerMsg = "";
        // Opponent win
        // Make it better
        if (bestPlayerHand.Rank.CompareTo(bestOpponentHand.Rank) == 1)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Lose);
            if (battleSystem.DealDamage(true))
            {
                //battleSystem.gameOver = false;
                startNewRound = false;
                winnerMsg = battleSystem.enemy.id + " Wins The Game With ";
                battleSystem.SetState(new GameOver(battleSystem, false));
            }
            else
            {
                startNewRound = true;
                winnerMsg = battleSystem.enemy.id + " Wins With ";
            }
            AnimateWinWithHand(bestOpponentHand.getCards(), false);
           
            winnerMsg += battleSystem.Interface.ConvertHandRankToTextDescription(bestOpponentHand.Rank); ;
          //  winnerMsg += bestOpponentHand.ToString(Hand.HandToStringFormatEnum.HandDescription);
        }
        // Player win
        else if (bestPlayerHand.Rank.CompareTo(bestOpponentHand.Rank) == -1)
        {
            SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.Win);
            if (battleSystem.DealDamage(false))
            {
                startNewRound = false;
                winnerMsg = battleSystem.player.id + " Wins The Game With ";
                battleSystem.SetState(new GameOver(battleSystem, true));
            }
            else
            {
                startNewRound = true;

                winnerMsg = battleSystem.player.id + " Wins With ";
            }

            AnimateWinWithHand(bestPlayerHand.getCards(), true);
            winnerMsg +=  battleSystem.Interface.ConvertHandRankToTextDescription(bestPlayerHand.Rank);
            // winnerMsg += bestPlayerHand.ToString(Hand.HandToStringFormatEnum.HandDescription);
        }
        else
        {
            battleSystem.Interface.tieTitle.SetActive(true);
            startNewRound = true;
            winnerMsg = " Tie " + bestOpponentHand.ToString(Hand.HandToStringFormatEnum.HandDescription);
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

    private void AnimateWinWithHand(List<Card> winningCards, bool isPlayerWin)
    {
        //MAKE IT BETTER
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
        CardUi card1, card2;
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

        string card1Str = card1.GetComponent<CardUi>().cardDescription.ToString();// Cant catch properly. catch it first place 
        string card2Str = card2.GetComponent<CardUi>().cardDescription.ToString();
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
            for (int j = 0; j < 5; j++)
            {
                if (boardCardsUi[j].GetComponent<CardUi>().cardDescription.ToString().Equals(winningCardDesc))
                {
                    winningBoardCards.Add(boardCardsUi[j]);
                    j = 5;
                }
            }
        }
        losingBoardCards.RemoveAll(item => winningBoardCards.Contains(item));
        SoundManager.Instance.PlaySingleSound(SoundManager.SoundName.EndRoundGong);
        AnimationManager.Instance.AnimateWinningHandToBoard(winningPlayersCards, losingBoardCards,
            () =>
            {
                battleSystem.EndRoundVisual(isPlayerWin);
                StartNewRoundAction(false);
            });
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
