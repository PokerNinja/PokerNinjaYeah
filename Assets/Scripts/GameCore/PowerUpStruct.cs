using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpStruct : Singleton<PowerUpStruct>
{

    /*0*/
    public PuStructInfo draw_player = new PuStructInfo
    (PowerUpNamesEnum.f1.ToString(), "DRAW PLAYER", "<b><color=#F03B37>Burn</color></b> one of your cards and replace with a new one.", "Choose one card to <color=#F03B37>burn</color>", 1, Constants.PlayerCardsTag, Constants.PoolCardTag);
    /*1*/
    public PuStructInfo draw_enemy = new PuStructInfo
        (PowerUpNamesEnum.f3.ToString(), "DRAW ENEMY", "<b><color=#F03B37>Burn</color></b> one of the opponent's cards and replace with a new one.", "Choose one card to <color=#F03B37>burn</color>", 1, Constants.EnemyCardsTag, Constants.PoolCardTag);
    /*2*/
    public PuStructInfo draw_board = new PuStructInfo
        (PowerUpNamesEnum.f2.ToString(), "DRAW 2 BOARD", "<b><color=#F03B37>Burn</color></b> two cards on the field and replace with a new ones.", "Choose two cards to <color=#F03B37>burn</color>", 2, Constants.BoardCardsTag, Constants.BoardCardsTag);
    /*3*/
    public PuStructInfo draw_all = new PuStructInfo
        (PowerUpNamesEnum.draw_all.ToString(), "DRAW ALL", "Draw 1 card & replace it with 1 card from your hand / enemy hand / board", "Choose one card to redraw", 1, Constants.AllCardsTag, Constants.PoolCardTag);
    /*4*/
    public PuStructInfo swap_player_board = new PuStructInfo
        (PowerUpNamesEnum.w2.ToString(), "STEAL PLAYER BOARD", "<b><color=#FFC35E>Swap</color></b> one card from your hand with one card from the field.", "Choose two cards to <color=#FFC35E>swap</color>", 2, Constants.PlayerCardsTag, Constants.BoardCardsTag);
    /*5*/
    public PuStructInfo swap_enemy_board = new PuStructInfo
        (PowerUpNamesEnum.w3.ToString(), "STEAL ENEMY BOARD", "<b><color=#FFC35E>Swap</color></b> one card from the field with one card from the opponent's hand.", "Choose two cards to <color=#FFC35E>swap</color>", 2, Constants.EnemyCardsTag, Constants.BoardCardsTag);
    /*6*/
    public PuStructInfo swap_player_enemy = new PuStructInfo
        (PowerUpNamesEnum.w1.ToString(), "STEAL ENEMY PLAYER", "<b><color=#FFC35E>Swap</color></b> one card from your hand with one card from the opponent's hand.", "Choose two cards to <color=#FFC35E>swap</color>", 2, Constants.PlayerCardsTag, Constants.EnemyCardsTag);
    /*7*/
    public PuStructInfo swap_hands = new PuStructInfo
        (PowerUpNamesEnum.wm1.ToString(), "SWAP HANDS", "<b><color=#FFC35E>Swapper\nSwap</color></b> your hand with the opponent's hand.", "", 3, Constants.PlayerCardsTag, Constants.EnemyCardsTag);
    /*8*/
    public PuStructInfo draw_two_player = new PuStructInfo
        (PowerUpNamesEnum.fm1.ToString(), "DRAW 2 CARDS", "<b><color=#F03B37>Sacrifice\nDraw</color></b> two cards, <b><color=#F03B37>Burn</color></b>  and replace with one of yours.", "Choose a card and replace with one of yours", 3, Constants.PlayerCardsTag, Constants.DeckCardsTag);
    /*16*/
    public PuStructInfo flip = new PuStructInfo
        (PowerUpNamesEnum.sflip.ToString(), "FLIP", "Reveal 1 card", "Choose a card to reveal", 1, Constants.EnemyCardsTag, Constants.PoolCardTag);
    /*17*/
    public PuStructInfo armagedon = new PuStructInfo
        (PowerUpNamesEnum.fm2.ToString(), "ARMAGEDDON", "<b><color=#F03B37>Armageddon\nBurn</color></b> random cards"/*most the cards in play and replace them with new ones.*/, "", 3, Constants.AllCardsTag, Constants.PoolCardTag);
    /*18*/
    public PuStructInfo block_enemy_card = new PuStructInfo
        (PowerUpNamesEnum.i1.ToString(), "FREEZE ENEMY CARD", "<b><color=#02C8FF>Freeze</color></b> one of the opponent's cards, making it un-moveable.", "Choose a card to <color=#02C8FF>freeze</color>", 1, Constants.EnemyCardsTag, Constants.PoolCardTag);
    /*19*/
    public PuStructInfo block_board = new PuStructInfo
        (PowerUpNamesEnum.i2.ToString(), "FREEZE 2 BOARD", "<b><color=#02C8FF>Freeze</color></b> two of cards from the field, making them un-moveable.", "Choose two card to <color=#02C8FF>freeze</color>", 2, Constants.BoardCardsTag, Constants.BoardCardsTag);
    /*20*/
    public PuStructInfo block_player_card = new PuStructInfo
        (PowerUpNamesEnum.i3.ToString(), "FREEZE PLAYER CARD", "<b><color=#02C8FF>Freeze</color></b> one of your cards, making it un-moveable.", "Choose a card to <color=#02C8FF>freeze</color>", 1, Constants.PlayerCardsTag, Constants.PoolCardTag);
    /*21*/
    public PuStructInfo block_player_hand = new PuStructInfo
        (PowerUpNamesEnum.im1.ToString(), "FREEZE PLAYER HAND", "<b><color=#02C8FF>Igloo\nFreeze</color></b> your hand, making it un-moveable.", "", 3, Constants.PlayerCardsTag, Constants.PoolCardTag);

    /*22*/
    public PuStructInfo strighter = new PuStructInfo
        (PowerUpNamesEnum.sm2.ToString(), "Strighter", "Achive stright with four cards until the end of the round.", "", 3, Constants.AllCardsTag, Constants.PoolCardTag);

    /*23*/
    public PuStructInfo flusher = new PuStructInfo
        (PowerUpNamesEnum.sm3.ToString(), "Flusher", "Achive flush with four cards until the end of the round.", "", 3, Constants.AllCardsTag, Constants.PoolCardTag);

    /*24*/
    public PuStructInfo smoke_player = new PuStructInfo
        (PowerUpNamesEnum.s1.ToString(), "Smoke", "Smoke on of your cards. This card is now untargetable (Except for wind), and the Opponent can't see this card", "Choose one card to smoke", 1, Constants.PlayerCardsTag, Constants.PoolCardTag);

    /*25*/
    public PuStructInfo smoke_board = new PuStructInfo
        (PowerUpNamesEnum.s2.ToString(), "Smoke", "Smoke the river. Those cards are now untargetable (Except for wind), and the Opponent can't see them", "", 0, Constants.BoardCardsTag, Constants.PoolCardTag);

    /*26*/
    public PuStructInfo ghost_board = new PuStructInfo
        (PowerUpNamesEnum.s3.ToString(), "GHOST", "Add a ghost card to the board.", "", 0, Constants.BoardCardsTag, Constants.PoolCardTag);

    /*27*/
    public PuStructInfo ghost_player = new PuStructInfo
        (PowerUpNamesEnum.sm1.ToString(), "GHOST", "Add a ghost card to your hand.", "", 3, Constants.PlayerCardsTag, Constants.PoolCardTag);
    /*28*/
    public PuStructInfo player_value_up_2 = new PuStructInfo
        (PowerUpNamesEnum.t1.ToString(), "Value", "Add 2 of the value of the selected card.", "Choose one card to change its value", 1, Constants.PlayerCardsTag, Constants.PoolCardTag);
    /*29*/
    public PuStructInfo player_value_down_2 = new PuStructInfo
        (PowerUpNamesEnum.t2.ToString(), "Value", "Subtract 2 of the value of the selected card.", "Choose one card to change its value", 1, Constants.PlayerCardsTag, Constants.PoolCardTag);
    /*30*/
    public PuStructInfo enemy_value_up_2 = new PuStructInfo
        (PowerUpNamesEnum.t3.ToString(), "Value", "Add 2 of the value of the selected card.", "Choose one card to change its value", 1, Constants.EnemyCardsTag, Constants.PoolCardTag);
    /*31*/
    public PuStructInfo enemy_value_down_2 = new PuStructInfo
        (PowerUpNamesEnum.t4.ToString(), "Value", "Subtract 2 of the value of the selected card.", "Choose one card to change its value", 1, Constants.EnemyCardsTag, Constants.PoolCardTag);
    /*32*/
    public PuStructInfo smoke_turn_river = new PuStructInfo
        (PowerUpNamesEnum.sm4.ToString(), "Smoke", "Smoke the turn and the river. Those cards are now untargetable (Except for wind), and the Opponent can't see them", "", 3, Constants.BoardCardsTag, Constants.PoolCardTag);
    /*33*/
    public PuStructInfo iceagedon = new PuStructInfo
        (PowerUpNamesEnum.im2.ToString(), "Iceagedon", "<b><color=#02C8FF>Icegeddon\nFreeze</color></b> random cards", "", 3, Constants.AllCardsTag, Constants.PoolCardTag);
    /*34*/
    public PuStructInfo tornado = new PuStructInfo
        (PowerUpNamesEnum.wm2.ToString(), "Tornado", "<b><color=#FFC35E>Tornado\nSwap</color></b> random cards", "", 3, Constants.AllCardsTag, Constants.PoolCardTag);
    /*35*/
    public PuStructInfo esFire = new PuStructInfo
        (PowerUpNamesEnum.fp.ToString(), "fp", "<size=110%><b><color=#F03B37>Burn</color> two cards of your choise.</b>\n<size=50%>\n<b><size=90%>*use 3 <color=#F03B37>Fire</color> Ninja Card to unlock", "Choose two cards to <color=#F03B37>burn</color>", 2, Constants.AllCardsTag, Constants.PoolCardTag);
    /*36*/
    public PuStructInfo esIce = new PuStructInfo
        (PowerUpNamesEnum.ip.ToString(), "ip", "<size=110%><b><color=#02C8FF>Freeze</color> two cards of your choise.</b>\n<size=50%>\n<b><size=90%>*use 3 <color=#02C8FF>Ice</color> Ninja Card to unlock", "Choose two cards to <color=#02C8FF>freeze</color>", 2, Constants.AllCardsTag, Constants.PoolCardTag);
    /*37*/
    public PuStructInfo esWind = new PuStructInfo
        (PowerUpNamesEnum.wp.ToString(), "wp", "<size=110%><b><color=#FFC35E>SWAP</color> two cards of your choise.</b>\n<size=50%>\n<b><size=90%>*use 3 <color=#FFC35E>Wind</color> Ninja Card to unlock", "Choose two cards to <color=#FFC35E>swap</color>", 2, Constants.AllCardsTag, Constants.PoolCardTag);
    /*38*/
    public PuStructInfo board_value_up_2 = new PuStructInfo
        (PowerUpNamesEnum.t5.ToString(), "Value", "Add 2 of the value of the selected card.", "Choose one card to change its value", 1, Constants.BoardCardsTag, Constants.PoolCardTag);
    /*39*/
    public PuStructInfo board_value_down_2 = new PuStructInfo
        (PowerUpNamesEnum.t6.ToString(), "Value", "Subtract 2 of the value of the selected card.", "Choose one card to change its value", 1, Constants.BoardCardsTag, Constants.PoolCardTag);
    /*40*/
    public PuStructInfo techDr = new PuStructInfo
        (PowerUpNamesEnum.tm1.ToString(), "Value", "Change the value of random cards.", "", 2, Constants.PlayerCardsTag, Constants.PoolCardTag);
    /*41*/
    public PuStructInfo esTech = new PuStructInfo
        (PowerUpNamesEnum.wp.ToString(), "tp", "<size=110%><b><color=#FFC35E>SWAP</color> two cards of your choise.</b>\n<size=50%>\n<b><size=90%>*use 3 <color=#FFC35E>Tech/color> Ninja Card to unlock", "Choose two cards to <color=#FFC35E>swap</color>", 2, Constants.AllCardsTag, Constants.PoolCardTag);
    /*42*/
    public PuStructInfo techgeddon = new PuStructInfo
        (PowerUpNamesEnum.tm2.ToString(), "Value", "Change the value of random cards.", "", 3, Constants.AllCardsTag, Constants.PoolCardTag);



    /*9*/
    public PuStructInfo shuffle_board = new PuStructInfo
        (PowerUpNamesEnum.shuffle_board.ToString(), "TSUNAMI", "Shuffle new board cards", "", 0, Constants.BoardCardsTag, Constants.PoolCardTag);
    /*10*/
    public PuStructInfo shuffle_hands = new PuStructInfo
        (PowerUpNamesEnum.shuffle_hands.ToString(), "SHUFFLE HANDS", "Shuffle new hands", "", 0, Constants.PlayerCardsTag, Constants.EnemyCardsTag);
    /*11*/
    public PuStructInfo enemy_pu_freeze = new PuStructInfo
        (PowerUpNamesEnum.enemy_pu_freeze.ToString(), "FREEZE", "Freeze enemy pu from use this round", "", 0, Constants.BoardCardsTag, Constants.PoolCardTag);
    /*12*/
    public PuStructInfo see_enemy_cards = new PuStructInfo
        (PowerUpNamesEnum.see_enemy_cards.ToString(), "DOUBLE PEEK", "Look at enemy hand", "", 0, Constants.EnemyCardsTag, Constants.PoolCardTag);
    /*13*/
    public PuStructInfo heal_1000 = new PuStructInfo
        (PowerUpNamesEnum.heal_1000.ToString(), "HEAL", "Restore 1 life", "", 0, Constants.PoolCardTag, Constants.PoolCardTag);
    /*14*/
    public PuStructInfo card_less_straight = new PuStructInfo
        (PowerUpNamesEnum.card_less_straight.ToString(), "STRAIGHTOR", "You need only 4 cards for straight", "", 0, Constants.PlayerCardsTag, Constants.BoardCardsTag);
    /*15*/
    public PuStructInfo card_less_flush = new PuStructInfo
        (PowerUpNamesEnum.card_less_flush.ToString(), "FLUSHOR", "You need only 4 cards of same suit for flush", "", 0, Constants.PlayerCardsTag, Constants.BoardCardsTag);




    // return Array.FindIndex(arr, x => x.PuName.Equals(powerUpName));

    public PuStructInfo GetPowerUpStruct(string powerUpName)
    {

        switch (powerUpName)
        {
            case nameof(PowerUpNamesEnum.f1):
                return draw_player;
            case nameof(PowerUpNamesEnum.f3):
                return draw_enemy;
            case nameof(PowerUpNamesEnum.f2):
                return draw_board;
            case nameof(PowerUpNamesEnum.fm1):
                return draw_two_player;
            case nameof(PowerUpNamesEnum.fm2):
                return armagedon;
            case nameof(PowerUpNamesEnum.w2):
                return swap_player_board;
            case nameof(PowerUpNamesEnum.w3):
                return swap_enemy_board;
            case nameof(PowerUpNamesEnum.w1):
                return swap_player_enemy;
            case nameof(PowerUpNamesEnum.wm1):
                return swap_hands;
            case nameof(PowerUpNamesEnum.i1):
                return block_enemy_card;
            case nameof(PowerUpNamesEnum.i2):
                return block_board;
            case nameof(PowerUpNamesEnum.i3):
                return block_player_card;
            case nameof(PowerUpNamesEnum.im1):
                return block_player_hand;
            case nameof(PowerUpNamesEnum.sflip):
                return flip;
            case nameof(PowerUpNamesEnum.sm2):
                return strighter;
            case nameof(PowerUpNamesEnum.sm3):
                return flusher;
            case nameof(PowerUpNamesEnum.s1):
                return smoke_player;
            case nameof(PowerUpNamesEnum.s2):
                return smoke_board;
            case nameof(PowerUpNamesEnum.s3):
                return ghost_board;
            case nameof(PowerUpNamesEnum.sm1):
                return ghost_player;
            case nameof(PowerUpNamesEnum.t1):
                return player_value_up_2;
            case nameof(PowerUpNamesEnum.t2):
                return player_value_down_2;
            case nameof(PowerUpNamesEnum.t3):
                return enemy_value_up_2;
            case nameof(PowerUpNamesEnum.t4):
                return enemy_value_down_2;
            case nameof(PowerUpNamesEnum.t5):
                return board_value_up_2;
            case nameof(PowerUpNamesEnum.t6):
                return board_value_down_2;
            case nameof(PowerUpNamesEnum.tm1):
                return techDr;
            case nameof(PowerUpNamesEnum.tm2):
                return techgeddon;
            case nameof(PowerUpNamesEnum.sm4):
                return smoke_turn_river;
            case nameof(PowerUpNamesEnum.im2):
                return iceagedon;
            case nameof(PowerUpNamesEnum.wm2):
                return tornado;
            case nameof(PowerUpNamesEnum.fp):
                return esFire;
            case nameof(PowerUpNamesEnum.ip):
                return esIce;
            case nameof(PowerUpNamesEnum.wp):
                return esWind;
            case nameof(PowerUpNamesEnum.tp):
                return esTech;

                /*draw_all = 3,
                shuffle_board = 9,
                shuffle_hands = 10,
                enemy_pu_freeze = 11,
                see_enemy_cards = 12,
                heal_1000 = 13,
                card_less_straight = 14,
                card_less_flush = 15,*/
        }
        return draw_player;
    }

    public string GetPowerUpDisplayName(string powerUpName)
    {
        return GetPowerUpStruct(powerUpName).displayName;
    }

    public int GetPowerUpCardsToSelect(string powerUpName)
    {
        return GetPowerUpStruct(powerUpName).cardToSelect;
    }

    public string GetPuInfoByName(string powerUpName)
    {
        return GetPowerUpStruct(powerUpName).info;
    }
    public string GetPuInstructionsByName(string powerUpName)
    {
        return GetPowerUpStruct(powerUpName).instructions;
    }

    public enum PowerUpNamesEnum
    {
        f1 = 0, //draw_player
        f3 = 1, //draw_enemy
        f2 = 2, //draw_board
        w2 = 4, //swap_player_board
        w3 = 5, //swap_enemy_board
        w1 = 6, //swap_player_enemy
        wm1 = 7, //swap_hands
        fm1 = 8, //draw_two_player
        sflip = 16,
        fm2 = 17, //armagedon
        i1 = 18, //block_enemy_card
        i2 = 19, //block_board_card
        i3 = 20, //block_player_card
        im1 = 21, //block_player_2_cards
        draw_all = 3,
        shuffle_board = 9,
        shuffle_hands = 10,
        enemy_pu_freeze = 11,
        see_enemy_cards = 12,
        heal_1000 = 13,
        card_less_straight = 14,
        card_less_flush = 15,
        sm2 = 22, //strighter
        sm3 = 23, //flusher
        s1 = 24, //smoke_player
        s2 = 25, //smoke_board
        s3 = 26, //ghost_board
        sm1 = 27, //ghost_player
        t1 = 28, // player value up 2
        t2 = 29, //player value down 2
        t3 = 30, //enemy value up 2
        t4 = 31, //enemy value down 2
        sm4 = 32, //smoke turn river
        im2 = 33, //smoke turn river
        wm2 = 34, //smoke turn river
        fp = 35, //smoke turn river
        ip = 36, //smoke turn river
        wp = 37, //smoke turn river
        t5 = 38, //board value up 2
        t6 = 39, //board value down 2
        tm1 = 40, //drTech
        tp = 41, //smoke turn river
        tm2 = 42, //rainTech
    }

    #region // PuInfo

    public string[] GetReleventTagCards(string puName, bool isPlayerActivate)
    {
        return new string[] { ConvertTagIfEnemyActivate(GetPowerUpStruct(puName).releventCards1, isPlayerActivate),
                              ConvertTagIfEnemyActivate(GetPowerUpStruct(puName).releventCards2, isPlayerActivate)};
    }

    public string ConvertTagIfEnemyActivate(string releventCards, bool isPlayerActivate)
    {
        if (!isPlayerActivate)
        {
            if (releventCards.Equals(Constants.PlayerCardsTag))
            {
                return Constants.EnemyCardsTag;
            }
            else if (releventCards.Equals(Constants.EnemyCardsTag))
            {
                return Constants.PlayerCardsTag;
            }
        }
        return releventCards;
    }


    #endregion


    /* private enum PuElement
     {
         Fire = 0,
         Wind = 1,
         Ice = 2,
         Aether = 3,
     }
     public string GetElement(string puName)
     {
         return puName[0].ToString();

     }*/

    /*public string GetElement(string puName)
    {
        switch (puName[0].ToString())
        {
            case "f":
                {
                    return PuElement.Fire;
                }
            case "w":
                {
                    return PuElement.Wind;
                }
            case "i":
                {
                    return PuElement.Ice;
                }
            case "a":
                {
                    return PuElement.Aether;
                }
        }
        return PuElement.Aether;
    }*/

}
