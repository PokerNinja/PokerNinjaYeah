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
    private string puElement;
    public PowerUpState(BattleSystem battleSystem, bool isPlayerActivate, int energyCost, string powerUpName, string cardTarget1, string cardTarget2, Vector2 posTarget1, Vector2 posTarget2, int puIndex) : base(battleSystem)
    {
        Debug.LogError("pos: " + posTarget1);
        Debug.LogError("pos: " + posTarget2);
        this.isPlayerActivate = isPlayerActivate;
        this.powerUpName = powerUpName;
        this.cardTarget1 = cardTarget1;
        this.cardTarget2 = cardTarget2;
        this.posTarget1 = posTarget1;
        this.posTarget2 = posTarget2;
        this.puIndex = puIndex;
        this.energyCost = energyCost;
        this.puElement = powerUpName.Substring(0, 1);
    }

    public override IEnumerator Start()
    {
        bool waitForAction = false;
        if (cardTarget1.Equals("reset"))
        {
            ActivateCardSelection(PowerUpStruct.Instance.GetReleventTagCards(powerUpName, true));
        }
        else
        {
            if (powerUpName.Equals(PowerUpNamesEnum.fm1.ToString())) // DRAW_2_CARDS
            {
                waitForAction = true;
            }
            else
            {
                // UpdateZpositionCardsList(powerUpName, true);
            }
            if (!isPlayerActivate)
            {
                // battleSystem.Interface.InitNinjaAttackAnimation(false, puElement);

                if (waitForAction) // DRAW_2_CARDS
                {
                    EnableZpoitionForCardsList(battleSystem.cardsDeckUi.enemyCardsUi, true);
                    battleSystem.Draw2Cards(true, () => battleSystem.InitProjectile(isPlayerActivate, puIndex, powerUpName, posTarget1, posTarget2, () => IgnitePowerUp(powerUpName, cardTarget1, cardTarget2)));
                    yield return new WaitForSecondsRealtime(0.5f);
                }
                else
                {
                    yield return new WaitForSecondsRealtime(1f);
                    if (puElement.Equals("s"))
                    {
                        IgnitePowerUp(powerUpName, cardTarget1, cardTarget2);
                    }
                    else
                    {
                        battleSystem.InitProjectile(isPlayerActivate, puIndex, powerUpName, posTarget1, posTarget2, () => IgnitePowerUp(powerUpName, cardTarget1, cardTarget2));
                    }

                    /*else
                    {
                        IgnitePowerUp(powerUpName, cardTarget1, cardTarget2);
                    }*/
                }
            }
            else // PLAYER ACTIVATE
            {
                int cardsToSelect = PowerUpStruct.Instance.GetPowerUpCardsToSelect(powerUpName);
                if (cardsToSelect == 0 || cardTarget2.Length > 0)
                {// Shove IT somewhereElse
                 //  battleSystem.Interface.InitNinjaAttackAnimation(true, puElement);

                    battleSystem.skillUsed = false;

                    if (cardTarget1.Contains("Deck"))
                    {
                        posTarget1 = new Vector2(0, 0);
                    }
                    if (cardTarget2.Contains("Deck"))
                    {
                        posTarget2 = new Vector2(0, 0);
                    }
                    if (puElement.Equals("s"))
                    {
                        IgnitePowerUp(powerUpName, cardTarget1, cardTarget2);
                    }
                    else
                    {
                        cardTarget2 = GetListOfRandomCardsForMonster();
                        cardTarget1 = GetRandomAmountForMonster();
                        battleSystem.InitProjectile(isPlayerActivate, puIndex, powerUpName, posTarget1, posTarget2, () => IgnitePowerUp(powerUpName, cardTarget1, cardTarget2));
                    }

                    /*else
                    {
                        battleSystem.skillUsed = true;
                        IgnitePowerUp(powerUpName, cardTarget1, cardTarget2);
                    }*/
                    battleSystem.UpdatePuInDb(cardTarget1, cardTarget2, puIndex);
                }
                else
                {
                    // battleSystem.selectCardsMode = true;
                    if (waitForAction)
                    { //DRAW 2 CARDS
                      //EnableZpoitionForCardsList(battleSystem.cardsDeckUi.playerCardsUi, true);
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
    }

    private string GetRandomAmountForMonster()
    {
        if (powerUpName.Equals("fm2"))
        {
            int randomAmount = battleSystem.GenerateRandom(5, 7);
            return randomAmount.ToString();
        }
        else if (powerUpName.Equals("im2"))
        {
            int randomAmount = battleSystem.GenerateRandom(5, 7);
            return randomAmount.ToString();
        }
        return cardTarget1;
    }

    private string GetListOfRandomCardsForMonster()
    {
        Debug.LogError("HELLO");
        if (powerUpName.Equals("im2") || powerUpName.Equals("fm2"))
        {
            return string.Join(",", battleSystem.GetRandomAvailableCardsNames(false));
        }
        else if (powerUpName.Equals("wm2"))
        {
            return string.Join(",", battleSystem.GetRandomAvailableCardsNames(true));
        }
        return cardTarget2;
    }

    private void ActivateSelectMode(int cardsToSelect, string powerUpName)
    {
        UpdateZpositionCardsList(powerUpName, true);
        Debug.Log("ActivateSelectMode =" + powerUpName);
        battleSystem.SetCardsSelectionAndDisplayInfo(cardsToSelect, powerUpName);
        ActivateCardSelection(PowerUpStruct.Instance.GetReleventTagCards(powerUpName, true));
        if (!PowerUpStruct.Instance.GetReleventTagCards(powerUpName, true)[0].Equals(Constants.AllCardsTag))
        {
            ActivateSelectionPointer(powerUpName);
        }
        if (puElement.Equals(battleSystem.Interface.pEs.element))
        {
            battleSystem.Interface.pEs.EnableSelecetPositionZ(true);
        }
        battleSystem.Interface.EnableDarkScreen(true, true, null);
    }

    private void ActivateSelectionPointer(string powerUpName)
    {
        string[] releventCards = PowerUpStruct.Instance.GetReleventTagCards(powerUpName, true);
        battleSystem.Interface.ApplyPointers(releventCards, powerUpName[0].ToString());
    }

    private void IgnitePowerUp(string powerUpName, string cardTarget1, string cardTarget2)
    {
        if (isPlayerActivate)
        {
            battleSystem.Interface.ResetCardSelection();
        }
        battleSystem.Interface.InitNinjaAttackAnimation(isPlayerActivate, puElement);
        if (puIndex != -1)
        {
            if (isPlayerActivate && battleSystem.Interface.pEs.IsNcEqualsPesElement(puElement))
            {
                
                battleSystem.DissolvePuToNc(puIndex,()=> battleSystem.UpdateEsAfterNcUse(powerUpName));
            }
            else
            {
                battleSystem.DissolvePuAfterUse(isPlayerActivate, puIndex);
            }
        }
        if (puIndex == -1)
        {
            battleSystem.ResetEs(isPlayerActivate);
        }
        if (isPlayerActivate)
        {
            battleSystem.ReduceEnergy(energyCost);
            battleSystem.selectMode = false;
            //  battleSystem.playerPuInProcess = true;
        }
        switch (powerUpName)
        {
            case nameof(PowerUpNamesEnum.f1): //draw_player
            case nameof(PowerUpNamesEnum.f3): //enemy_player
                {
                    battleSystem.DestroyAndDrawCard(cardTarget2, 0.100f, true, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.f2): // draw_board
            case nameof(PowerUpNamesEnum.fp): // draw_board
                {
                    bool isFirstTargetFreeze = battleSystem.cardsDeckUi.GetCardUiByName(cardTarget1).freeze;
                    battleSystem.DestroyAndDrawCard(cardTarget1, 0f, true, false);
                    battleSystem.DestroyAndDrawCard(cardTarget2, Values.Instance.delayBetweenProjectiles, isFirstTargetFreeze, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.w2)://swap_player_board = w2
            case nameof(PowerUpNamesEnum.w3): //swap_enemy_board = w3
            case nameof(PowerUpNamesEnum.w1): //swap_player_enemy = w1
            case nameof(PowerUpNamesEnum.wp): //swap_player_enemy = w1
                {

                   // battleSystem.UpdateZPos(true, PowerUpStruct.Instance.GetReleventTagCards(powerUpName, isPlayerActivate)[0]);
                   // battleSystem.UpdateZPos(true, PowerUpStruct.Instance.GetReleventTagCards(powerUpName, isPlayerActivate)[1]);
                    battleSystem.SwapTwoCards(cardTarget1, cardTarget2, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.wm1): //swap_hands = wm1
                {
                   // battleSystem.UpdateZPos(true, PowerUpStruct.Instance.GetReleventTagCards(powerUpName, isPlayerActivate)[0]);
                   // battleSystem.UpdateZPos(true, PowerUpStruct.Instance.GetReleventTagCards(powerUpName, isPlayerActivate)[1]);
                    battleSystem.SwapPlayersHands();
                    break;
                }
            case nameof(PowerUpNamesEnum.fm1): //draw_two_player = fm1
                {
                    battleSystem.SwapAndDestroy(cardTarget1, cardTarget2);
                    break;
                }
            case nameof(PowerUpNamesEnum.enemy_pu_freeze): //enemy_pu_freeze
                {
                    battleSystem.FreezePlayingCard(cardTarget2, 0, true, true, true);
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
                    BurnRandomCards(cardTarget2, cardTarget1);
                    break;
                }
            case nameof(PowerUpNamesEnum.i1): //block_enemy_card = i1
            case nameof(PowerUpNamesEnum.i3): //block_player_card = i3
                {
                    battleSystem.FreezePlayingCard(cardTarget2, 0, true, true, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.i2): //block_board_card = i2
            case nameof(PowerUpNamesEnum.ip): //block_board_card = i2
                {
                    bool isFirstTargetFreeze = battleSystem.cardsDeckUi.GetCardUiByName(cardTarget1).freeze;
                    battleSystem.FreezePlayingCard(cardTarget1, 0, true, true, false);
                    battleSystem.FreezePlayingCard(cardTarget2, 200, true, !isFirstTargetFreeze, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.im1): //block_player_2_cards = im1
                {
                    bool isFirstTargetFreeze = battleSystem.cardsDeckUi.GetCardUiByName(ConvertFixedCardPlace(Constants.PlayerCard1)).freeze;
                    battleSystem.FreezePlayingCard(ConvertFixedCardPlace(Constants.PlayerCard1), 0, true, true, false);
                    battleSystem.FreezePlayingCard(ConvertFixedCardPlace(Constants.PlayerCard2), 200, true, !isFirstTargetFreeze, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.sm2): //sm2 = 22, //strighter
                {
                    battleSystem.StrighterPU(isPlayerActivate);
                    break;
                }
            case nameof(PowerUpNamesEnum.sm3): //sm3= 23, //flusher

                {
                    battleSystem.FlusherPU(isPlayerActivate);
                    break;
                }
            case nameof(PowerUpNamesEnum.s1): // 24, //smoke_player

                {
                    battleSystem.SmokeCardPu(true, cardTarget2, isPlayerActivate, true, false);
                    break;
                }
            case nameof(PowerUpNamesEnum.s2): // 25, //smoke_board

                {
                    battleSystem.SmokeCardPu(true, Constants.BRiver5, isPlayerActivate, true, false);
                    break;
                }
            case nameof(PowerUpNamesEnum.s3): //ghost_board

                {
                    battleSystem.GhostPu(isPlayerActivate, false);
                    break;
                }
            case nameof(PowerUpNamesEnum.sm1): //ghost_player

                {
                    battleSystem.GhostPu(isPlayerActivate, true);
                    break;
                }
            case nameof(PowerUpNamesEnum.s4): //player value up 2
            case nameof(PowerUpNamesEnum.s6): //board value up 2
                {
                    battleSystem.ChangeValuePu(cardTarget2, 2);
                    break;
                }
            case nameof(PowerUpNamesEnum.s5): //player value down 2
            case nameof(PowerUpNamesEnum.s7): //board value down 2
                {
                    battleSystem.ChangeValuePu(cardTarget2, -2);
                    break;
                }
            case nameof(PowerUpNamesEnum.sm4): //smoke turn river
                {
                    battleSystem.SmokeTurnRiver(isPlayerActivate);
                    break;
                }
            case nameof(PowerUpNamesEnum.im2): //smoke turn river
                {
                    FreezeRandomCards(cardTarget2, cardTarget1);
                    break;
                }
            case nameof(PowerUpNamesEnum.wm2): //smoke turn river
                {
                    SwapRandomCards(cardTarget2);
                    break;
                }
        }

    }

    private async void SwapRandomCards(string listOfCards)
    {
        //  List<string> cardsNames = battleSystem.GetRandomAvailableCardsNames();
        List<string> cardsNames = new List<string>(listOfCards.Split(','));
        int i = 0;
        int count = cardsNames.Count;
        Debug.LogError("COUNT " + cardsNames.Count);
        await Task.Delay(150);
        battleSystem.SwapTwoCards(ConvertFixedCardPlace(cardsNames[i++]), ConvertFixedCardPlace(cardsNames[i++]), IsResetTornado(count - i));

        if (count > 3)
        {
            await Task.Delay(320);
            battleSystem.SwapTwoCards(ConvertFixedCardPlace(cardsNames[i++]), ConvertFixedCardPlace(cardsNames[i++]), IsResetTornado(count - i));
        }
        if (count > 5)
        {
            await Task.Delay(210);
            battleSystem.SwapTwoCards(ConvertFixedCardPlace(cardsNames[i++]), ConvertFixedCardPlace(cardsNames[i++]), IsResetTornado(count - i));
        }
        if (count > 7)
        {
            await Task.Delay(150);
            battleSystem.SwapTwoCards(ConvertFixedCardPlace(cardsNames[i++]), ConvertFixedCardPlace(cardsNames[i++]), IsResetTornado(count - i));
        }
    }

    private bool IsResetTornado(int count)
    {
        if (count <= 2)
        {
            return true;
        }
        return false;
    }

    private async void BurnRandomCards(string listOfCards, string limit)
    {
        // battleSystem.UpdateZPos(true, "All");
        await Task.Delay(300);
        List<string> cardsNames = new List<string>(listOfCards.Split(','));
        int randomAmount = int.Parse(limit);
        bool isLast = false;
        bool isFirst = true;
        for (int i = 0; i < randomAmount; i++)
        {
            if (i == randomAmount - 1)
            {
                isLast = true;
            }
            await Task.Delay(battleSystem.GenerateRandom(400, 700));
            battleSystem.DestroyAndDrawCard(ConvertFixedCardPlace(cardsNames[i]), 0.1f, isFirst, isLast);
            isFirst = false;
        }
    }

    private void FreezeRandomCards(string listOfCards, string limit)
    {
        List<string> cardsNames = new List<string>(listOfCards.Split(','));
        int randomAmount = int.Parse(limit);
        bool isFirstFrozen = false;
        bool frozenCheck = true;
        bool isLast = false;
        for (int i = 0; i < randomAmount; i++)
        {
            if (frozenCheck && battleSystem.cardsDeckUi.GetCardUiByName(cardsNames[i]).freeze)
            {
                isFirstFrozen = true;
                frozenCheck = false;
            }
            if (i == randomAmount - 1)
            {
                isLast = true;
            }
            battleSystem.FreezePlayingCard(ConvertFixedCardPlace(cardsNames[i]), battleSystem.GenerateRandom(400, 800), true, isFirstFrozen, isLast);
            isFirstFrozen = false;
        }
    }

    /*private IEnumerator PlayASync()
    {
        int addition = 300;
        int index = 1;
        yield return new WaitForSeconds(0.3f);
        battleSystem.DestroyAndDrawCard2(ConvertFixedCardPlace(Constants.PlayerCard2), index++ * addition, false, true, false);
        yield return new WaitForSeconds(0.3f);
        battleSystem.DestroyAndDrawCard2(ConvertFixedCardPlace(Constants.EnemyCard2), index++ * addition, false, false, false);
        yield return new WaitForSeconds(0.3f);
        battleSystem.DestroyAndDrawCard2(ConvertFixedCardPlace(Constants.PlayerCard1), index++ * addition, false, false, false);
        yield return new WaitForSeconds(0.3f);
        battleSystem.DestroyAndDrawCard2(ConvertFixedCardPlace(Constants.EnemyCard1), index++ * addition, false, false, false);
        yield return new WaitForSeconds(0.3f);
        battleSystem.DestroyAndDrawCard2(Constants.BoardCards[0], index++ * addition, false, false, false);
        yield return new WaitForSeconds(0.3f);
        battleSystem.DestroyAndDrawCard2(Constants.BoardCards[1], index++ * addition, false, false, false);
    }*/

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
        //  string puElement = powerUpName.Substring(0, 1);
        if (powerUpName.Equals("fm1"))
        {
            puElement = "x";
            Debug.LogWarning("POPO");
        }
        foreach (CardUi card in cardsList)
        {
            /* if (card.freeze && powerUpName.Equals("s1") || card.underSmoke && puElement.Equals("w") || card.underSmoke && puElement.Equals("f") || !card.underSmoke)
             {
             }*/
            card.SetSelection(enable, puElement, powerUpName);
            if (enable)
            {
                if (card.freeze)
                {
                    battleSystem.cardsDeckUi.EnableNcActionSlot(card.cardPlace, puElement);
                    Debug.LogWarning("EnableNCavtion");

                }
            }
            else
            {
                Debug.LogWarning("DsiableNCavtion");
                battleSystem.cardsDeckUi.DisableNcAction(card.cardPlace);

            }

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
        else if ((cardsToActivate[0] == cardsToActivate[1]) || powerUpName.Equals("fp") || powerUpName.Equals("ip"))
        {

            battleSystem.sameCardsSelection = true;
        }
        battleSystem.Interface.FadeCancelSelectModeScreen(true);
    }
}
