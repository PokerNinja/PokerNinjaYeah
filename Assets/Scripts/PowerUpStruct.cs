using MyBox;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerUpStruct : Singleton<PowerUpStruct>
{

    /*0*/
        public PuStructInfo draw_player = new PuStructInfo
        (PowerUpNamesEnum.f1.ToString(), "DRAW PLAYER", "Burn one of your cards and replace with a new one.", "Choose one card to redraw", 1,Constants.PlayerCardsTag, Constants.PoolCardTag);
    /*1*/
    public PuStructInfo draw_enemy = new PuStructInfo
        (PowerUpNamesEnum.f3.ToString(), "DRAW ENEMY", "Burn one of the opponent's cards and replace with a new one.", "Choose one card to redraw", 1, Constants.EnemyCardsTag, Constants.PoolCardTag);
    /*2*/
    public PuStructInfo draw_board = new PuStructInfo
        (PowerUpNamesEnum.f2.ToString(), "DRAW 2 BOARD", "Burn two cards on the field and replace with a new ones.", "Choose two cards to redraw", 2, Constants.BoardCardsTag, Constants.BoardCardsTag);
    /*3*/
    public PuStructInfo draw_all = new PuStructInfo
        (PowerUpNamesEnum.draw_all.ToString(), "DRAW ALL", "Draw 1 card & replace it with 1 card from your hand / enemy hand / board", "Choose one card to redraw", 1, Constants.AllCardsTag, Constants.PoolCardTag);
    /*4*/
    public PuStructInfo swap_player_board = new PuStructInfo
        (PowerUpNamesEnum.w2.ToString(), "STEAL PLAYER BOARD", "Swap one card from your hand with one card from the field.", "Choose two cards to swap between them", 2, Constants.PlayerCardsTag, Constants.BoardCardsTag);
    /*5*/
    public PuStructInfo swap_enemy_board = new PuStructInfo
        (PowerUpNamesEnum.w3.ToString(), "STEAL ENEMY BOARD", "Swap one card from the field with one card from the opponent's hand.", "Choose two cards to swap between them", 2, Constants.EnemyCardsTag, Constants.BoardCardsTag);
    /*6*/
    public PuStructInfo swap_player_enemy = new PuStructInfo
        (PowerUpNamesEnum.w1.ToString(), "STEAL ENEMY PLAYER", "Swap one card from your hand with one card from the opponent's hand.", "Choose two cards to swap between them", 2, Constants.PlayerCardsTag, Constants.EnemyCardsTag);
    /*7*/
    public PuStructInfo swap_hands = new PuStructInfo
        (PowerUpNamesEnum.wm1.ToString(), "SWAP HANDS", "Swap your hand with the opponent's hand.", "", 0, Constants.PlayerCardsTag, Constants.EnemyCardsTag);
    /*8*/
    public PuStructInfo draw_two_player = new PuStructInfo
        (PowerUpNamesEnum.fm1.ToString(), "DRAW 2 CARDS", "Draw two cards from the deck, Burn and replace one with one of yours.", "Choose one card and replace with one of yours", 2, Constants.PlayerCardsTag, Constants.DeckCardsTag);
    /*16*/
    public PuStructInfo flip = new PuStructInfo
        (PowerUpNamesEnum.sflip.ToString(), "FLIP", "Flip 1 card", "Choose one card to flip", 1, Constants.PlayerCardsTag, Constants.EnemyCardsTag);
    /*17*/
    public PuStructInfo armagedon = new PuStructInfo
        (PowerUpNamesEnum.fm2.ToString(), "ARMAGEDON", "Burn all the cards in play and replace them with new ones.", "", 0, Constants.AllCardsTag, Constants.PoolCardTag);
    /*18*/
    public PuStructInfo block_enemy_card = new PuStructInfo
        (PowerUpNamesEnum.i1.ToString(), "FREEZE ENEMY CARD", "Freeze one of the opponent's cards, making it un-targetable.", "Choose one card to block", 1, Constants.EnemyCardsTag, Constants.PoolCardTag);
    /*19*/
    public PuStructInfo block_board = new PuStructInfo
        (PowerUpNamesEnum.i2.ToString(), "FREEZE 2 BOARD", "Freeze two of cards from the field, making them un-targetable.", "Choose one card to block", 2, Constants.BoardCardsTag, Constants.BoardCardsTag);
    /*20*/
    public PuStructInfo block_player_card = new PuStructInfo
        (PowerUpNamesEnum.i3.ToString(), "FREEZE PLAYER CARD", "Freeze one of your cards, making it un-targetable.", "Choose one card to block", 1, Constants.PlayerCardsTag, Constants.PoolCardTag);
    /*21*/
    public PuStructInfo block_player_hand = new PuStructInfo
        (PowerUpNamesEnum.im1.ToString(), "FREEZE PLAYER HAND", "Freeze your hand, making it un-targetable.", "", 0, Constants.PlayerCardsTag, Constants.PoolCardTag);

     /*22*/
    public PuStructInfo strighter = new PuStructInfo
        (PowerUpNamesEnum.sm2.ToString(), "Strighter", "Achive stright with four cards until the end of the round.", "", 0, Constants.AllCardsTag, Constants.PoolCardTag);
    
     /*23*/
    public PuStructInfo flusher = new PuStructInfo
        (PowerUpNamesEnum.sm3.ToString(), "Flusher", "Achive flush with four cards until the end of the round.", "", 0, Constants.AllCardsTag, Constants.PoolCardTag);
    
     /*24*/
    public PuStructInfo smoke_player = new PuStructInfo
        (PowerUpNamesEnum.s1.ToString(), "Smoke", "Smoke on of your cards. This card is now untargetable (Except for wind), and the Opponent can't see this card", "", 1, Constants.PlayerCardsTag, Constants.PoolCardTag);
    
     /*25*/
    public PuStructInfo smoke_board = new PuStructInfo
        (PowerUpNamesEnum.s2.ToString(), "Smoke", "Smoke the turn and the river. Those cards are now untargetable (Except for wind), and the Opponent can't see them", "", 0, Constants.BoardCardsTag, Constants.PoolCardTag);
    
     /*26*/
    public PuStructInfo ghost_board = new PuStructInfo
        (PowerUpNamesEnum.s3.ToString(), "GHOST", "Add a ghost card to the board.", "", 0, Constants.BoardCardsTag, Constants.PoolCardTag);
    
     /*27*/
    public PuStructInfo ghost_player = new PuStructInfo
        (PowerUpNamesEnum.sm1.ToString(), "GHOST", "Add a ghost card to your hand.", "", 0, Constants.PlayerCardsTag, Constants.PoolCardTag);

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
        sm3= 23, //flusher
        s1 = 24, //smoke_player
        s2 = 25, //smoke_board
        s3 = 26, //ghost_board
        sm1 = 27, //ghost_player
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
