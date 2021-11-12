using Com.InfallibleCode.TurnBasedGame.Combat;
using Serializables;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using static PowerUpStruct;

public class PowerUpState : State
{
    private bool isPlayerActivate;
    private string powerUpName;
    private string cardTarget1;
    private string cardTarget2;
    private Vector2 posTarget1;
    private Vector2 posTarget2;
    private int puIndex;
    private int energyCost;
    public PowerUpState(BattleSystem battleSystem, bool isPlayerActivate, int energyCost,string powerUpName, string cardTarget1, string cardTarget2, Vector2 posTarget1, Vector2 posTarget2, int puIndex) : base(battleSystem)
    {
        this.isPlayerActivate = isPlayerActivate;
        this.powerUpName = powerUpName;
        this.cardTarget1 = cardTarget1;
        this.cardTarget2 = cardTarget2;
        this.posTarget1 = posTarget1;
        this.posTarget2 = posTarget2;
        this.puIndex = puIndex;
        this.energyCost = energyCost;
    }

    public override IEnumerator Start()
    {
        bool waitForAction = false;
        if (powerUpName.Equals(PowerUpNamesEnum.fm1.ToString())) // DRAW_2_CARDS
        {
            waitForAction = true;
        }
        else
        {
            UpdateZpositionCardsList(powerUpName, true);
        }
        if (!isPlayerActivate)
        {
            if (waitForAction) // DRAW_2_CARDS
            {
                EnableZpoitionForCardsList(battleSystem.cardsDeckUi.enemyCardsUi, true);
                battleSystem.Draw2Cards(true, () => battleSystem.InitProjectile(isPlayerActivate, puIndex, powerUpName, posTarget1, posTarget2, () => IgnitePowerUp(powerUpName, cardTarget1, cardTarget2)));
                yield return new WaitForSecondsRealtime(0.5f);
            }
            else
            {
                if (puIndex != -1)
                {
                    yield return new WaitForSecondsRealtime(1f);

                    battleSystem.InitProjectile(isPlayerActivate, puIndex, powerUpName, posTarget1, posTarget2, () => IgnitePowerUp(powerUpName, cardTarget1, cardTarget2));
                }
                else
                {
                    IgnitePowerUp(powerUpName, cardTarget1, cardTarget2);
                }
            }
        }
        else // PLAYER ACTIVATE
        {
            int cardsToSelect = PowerUpStruct.Instance.GetPowerUpCardsToSelect(powerUpName);
            if (cardsToSelect == 0 || cardTarget2.Length > 0)
            {// Shove IT somewhereElse
                if (puIndex != -1)
                {
                    battleSystem.skillUsed = false;

                    if (cardTarget1.Contains("Deck"))
                    {
                        posTarget1 = new Vector2(0, 0);
                    }
                    if (cardTarget2.Contains("Deck"))
                    {
                        posTarget2 = new Vector2(0, 0);
                    }
                    battleSystem.InitProjectile(isPlayerActivate, puIndex, powerUpName, posTarget1, posTarget2, () => IgnitePowerUp(powerUpName, cardTarget1, cardTarget2));
                }
                else
                {
                    battleSystem.skillUsed = true;
                    IgnitePowerUp(powerUpName, cardTarget1, cardTarget2);
                }
                battleSystem.UpdatePuInDb(cardTarget1, cardTarget2, puIndex);
            }
            else
            {
               // battleSystem.selectCardsMode = true;
                if (waitForAction)
                { //DRAW 2 CARDS
                    EnableZpoitionForCardsList(battleSystem.cardsDeckUi.playerCardsUi, true);
                    battleSystem.Draw2Cards(false, () => ActivateSelectMode(cardsToSelect, powerUpName));
                }
                else
                {
                    ActivateSelectMode(cardsToSelect, powerUpName);
                }
            }
        }
        yield break;

    }

    private void ActivateSelectMode(int cardsToSelect, string powerUpName)
    {
        battleSystem.SetCardsSelectionAndDisplayInfo(cardsToSelect, powerUpName);
        ActivateCardSelection(PowerUpStruct.Instance.GetReleventTagCards(powerUpName, true));
    }

    private void IgnitePowerUp(string powerUpName, string cardTarget1, string cardTarget2)
    {
        if (puIndex != -1)
        {
            battleSystem.DissolvePuAfterUse(isPlayerActivate, puIndex);
            battleSystem.ReduceSkillUse();
        }
        if (isPlayerActivate)
        {
            battleSystem.ReduceEnergy(energyCost);
            battleSystem.selectMode = false;
        }
        switch (powerUpName)
        {
            case nameof(PowerUpNamesEnum.f1): //draw_player
            case nameof(PowerUpNamesEnum.f3): //enemy_player
                {
                    battleSystem.DestroyAndDrawCard(cardTarget2, 0.100f, true, true, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.f2): // draw_board
                {
                    battleSystem.DestroyAndDrawCard(cardTarget2, 0.100f, false, true, false);
                    battleSystem.DestroyAndDrawCard(cardTarget1, 0.400f, true, false, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.w2)://swap_player_board = w2
            case nameof(PowerUpNamesEnum.w3): //swap_enemy_board = w3
            case nameof(PowerUpNamesEnum.w1): //swap_player_enemy = w1
                {

                    battleSystem.UpdateZPos(true, PowerUpStruct.Instance.GetReleventTagCards(powerUpName,isPlayerActivate)[0]);
                    battleSystem.UpdateZPos(true, PowerUpStruct.Instance.GetReleventTagCards(powerUpName, isPlayerActivate)[1]);
                    battleSystem.SwapTwoCards(cardTarget1, cardTarget2);
                    break;
                }
            case nameof(PowerUpNamesEnum.wm1): //swap_hands = wm1
                {
                    battleSystem.UpdateZPos(true, PowerUpStruct.Instance.GetReleventTagCards(powerUpName, isPlayerActivate)[0]);
                    battleSystem.UpdateZPos(true, PowerUpStruct.Instance.GetReleventTagCards(powerUpName, isPlayerActivate)[1]);
                    battleSystem.SwapPlayersHands();
                    break;
                }
            case nameof(PowerUpNamesEnum.fm1): //draw_two_player = fm1
                {
                    battleSystem.SwapAndDestroy(cardTarget1, cardTarget2);
                    break;
                }
            case nameof(PowerUpNamesEnum.shuffle_board): //shuffle_board
                {
                    battleSystem.UpdateZPos(true, PowerUpStruct.Instance.GetReleventTagCards(powerUpName, isPlayerActivate)[0]);
                    battleSystem.DestroyAndDrawCard(Constants.BoardCards[0] , 0.1f, false, true, false);
                    battleSystem.DestroyAndDrawCard(Constants.BoardCards[1] , 0.35f, false, false, false);
                    battleSystem.DestroyAndDrawCard(Constants.BoardCards[2] , 0.6f, false, false, false);
                    battleSystem.DestroyAndDrawCard(Constants.BoardCards[3] , 0.85f, false, false, false);
                    battleSystem.DestroyAndDrawCard(Constants.BoardCards[4], 1.1f, true, false, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.enemy_pu_freeze): //enemy_pu_freeze
                {
                    battleSystem.FreezePlayingCard(cardTarget2, true, true);
                    // battleSystem.FreezePu(isPlayerTurn);
                    break;
                }

            case nameof(PowerUpNamesEnum.sflip): //flip
                {
                    battleSystem.FlipCard(cardTarget2, isPlayerActivate);
                    break;
                }
            case nameof(PowerUpNamesEnum.fm2):  //armagedon = fm2
                {
                    //MAKE IT LOOK BETTER WHOS FIRST MAYNBE FUCKED HERE
                    battleSystem.UpdateZPos(true, "All");
                    int boardCount = battleSystem.cardsDeckUi.boardCardsUi.Count;
                    battleSystem.DestroyAndDrawCard(ConvertFixedCardPlace(Constants.PlayerCard2), 0.1f, false, true, false);
                    battleSystem.DestroyAndDrawCard(ConvertFixedCardPlace(Constants.EnemyCard2), 0.4f, false, false, false);
                    battleSystem.DestroyAndDrawCard(ConvertFixedCardPlace(Constants.PlayerCard1), 0.7f, false, false, false);
                    battleSystem.DestroyAndDrawCard(ConvertFixedCardPlace(Constants.EnemyCard1),1f, false, false, false);
                    battleSystem.DestroyAndDrawCard(Constants.BoardCards[0], 1.3f, false, false, false);
                    battleSystem.DestroyAndDrawCard(Constants.BoardCards[1], 1.6f, false, false, false);
                    if (boardCount == 3)
                    {
                        battleSystem.DestroyAndDrawCard(Constants.BoardCards[2], 1.9f, true, false, true);
                    }
                    else if (boardCount == 4)
                    {
                        battleSystem.DestroyAndDrawCard(Constants.BoardCards[2], 1.9f, false, false, false);
                        battleSystem.DestroyAndDrawCard(Constants.BoardCards[3], 2.1f, true, false, true);
                    }
                    else if (boardCount == 5)
                    {
                        battleSystem.DestroyAndDrawCard(Constants.BoardCards[2], 1.9f, false, false, false);
                        battleSystem.DestroyAndDrawCard(Constants.BoardCards[3], 2.1f, false, false, false);
                        battleSystem.DestroyAndDrawCard(Constants.BoardCards[4], 2.3f, true, false, true);
                    }

                    break;
                }
            case nameof(PowerUpNamesEnum.i1): //block_enemy_card = i1
            case nameof(PowerUpNamesEnum.i3): //block_player_card = i3
                {
                    battleSystem.FreezePlayingCard(cardTarget2, true, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.i2): //block_board_card = i2
                {
                    battleSystem.FreezePlayingCard(cardTarget2, true, false);
                    battleSystem.FreezePlayingCard(cardTarget1, true, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.im1): //block_player_2_cards = im1
                {

                    battleSystem.FreezePlayingCard(ConvertFixedCardPlace(Constants.PlayerCard1), true, false);
                    battleSystem.FreezePlayingCard(ConvertFixedCardPlace(Constants.PlayerCard2), true, true);
                    break;
                }
        }

    }

    private string ConvertFixedCardPlace(string targetToConvert)
    {
        if (!isPlayerActivate)
        {
            return Constants.Instance.ConvertCardPlaceForEnemy(targetToConvert);
        }
        else
        {
            return targetToConvert;
        }
    }

    private void EnableZpoitionForCardsList(List<CardUi> cardsList, bool enable)
    {
        foreach (CardUi card in cardsList)
        {
            card.EnableSelecetPositionZ(enable);
        }
    }
    private void EnableSelectForCardsList(List<CardUi> cardsList, bool enable)
    {
        foreach (CardUi card in cardsList)
        {
            card.SetSelection(enable, powerUpName.Substring(0,1));
        }
    }

    private void UpdateZpositionCardsList(string puName, bool enable)
    {
        string[] cardsTag = PowerUpStruct.Instance.GetReleventTagCards(puName, isPlayerActivate);
        List<CardUi> releventCards = new List<CardUi>();
        releventCards.AddRange(battleSystem.cardsDeckUi.GetListByTag(cardsTag[0]));
        if (cardsTag[1] != Constants.PoolCardTag)
        {
            releventCards.AddRange(battleSystem.cardsDeckUi.GetListByTag(cardsTag[1]));

        }
        EnableZpoitionForCardsList(releventCards, enable);

    }


    private void ActivateCardSelection(string[] cardsToActivate)
    {
        if (cardsToActivate[0] == Constants.AllCardsTag)
        {
            battleSystem.resetAllCardsSelectionWhenCardClicked = true;
        }
        EnableSelectForCardsList(battleSystem.cardsDeckUi.GetListByTag(cardsToActivate[0]), true);
        if (cardsToActivate[1] != Constants.PoolCardTag && cardsToActivate[0] != cardsToActivate[1])
        {
            EnableSelectForCardsList(battleSystem.cardsDeckUi.GetListByTag(cardsToActivate[1]), true);
        }
        else if (cardsToActivate[0] == cardsToActivate[1])
        {
            battleSystem.sameCardsSelection = true;
        }
    }
}
