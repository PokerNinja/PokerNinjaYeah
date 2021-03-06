using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : Singleton<Constants>
{

    public readonly string volumeSoundKey = "music_key";
    public readonly string botEnergyKey = "bot_energy_key";
    public readonly string botRaiseKey = "bot_raise_key";
    public readonly string botEsElementKey = "bot_es_e_key";
    public readonly string botEsCounterKey = "bot_es_c_key";
    public readonly string PLAYER_WIN_BOT = "PLAYER_WIN_BOT";
    public readonly string PLAYER_LOSE_BOT = "PLAYER_LOSE_BOT";
    public const string PlayerCardsTag = "CardP";
    public const string EnemyCardsTag = "CardE";
    public const string BoardCardsTag = "CardB";
    public const string DeckCardsTag = "CardD";
    public const string AllCardsTag = "All";
    public const string PoolCardTag = "Pool";
    public enum CardsOwener { Player, Enemy, Board, Deck, Pool, }
    public enum NcAction { Shatter, Defrost , Unglitched, Nothing}

    public const string PlayerCard1 = "PlayerCard1";
    public const string PlayerCard2 = "PlayerCard2";
    public const string EnemyCard1 = "EnemyCard1";
    public const string EnemyCard2 = "EnemyCard2";
    public const string BFlop1 = "BFlop1";
    public const string BFlop2 = "BFlop2";
    public const string BFlop3 = "BFlop3";
    public const string BTurn4 = "BTurn4";
    public const string BRiver5 = "BRiver5";
    public const string Deck1 = "Deck1";
    public const string Deck2 = "Deck2";
    public const string PlayerGhost = "PlayerGhost";
    public const string EnemyGhost = "EnemyGhost";
    public const string BoardGhost = "BoardGhost";
    public static readonly string[] BoardCards = { BFlop1, BFlop2, BFlop3, BTurn4, BRiver5 };
    public static readonly string[] deckCardsNames = { Deck1, Deck2 };
    public static readonly string[] ghostCardsNames = { PlayerGhost, EnemyGhost, BoardGhost };
    public static string ReplacePuInfo = "<b>Draw a Ninja Card</b>.\nWhen no slot available - choose one to replace.";
    public static string BetInfo = "Offer your opponent to double the damage for this round";
    public static string EndInfo = "Click to end your turn";
    public static string DealerInfo = "The dealer has the last move, and gets 1 energy in the last round";
    public static string DrawInstructions = "Choose a Ninja Card to replace";

    public const string TechWheel2 = "TechWheel2";
    public const string TechWheel3 = "TechWheel3";

    public static bool TUTORIAL_MODE = false;
    public static bool HP_GAME = false;
    public static bool BOT_MODE = false;
    public static bool IL2CPP_MOD = false;
    public static bool TemproryUnclickable = false;
    public static int cardsToSelectCounter = 0;
    public static string MatchId;

    public static int playerES;
    public static int enemyES;

    public string ConvertCardPlaceForEnemy(string cardPlace)
    {
        switch (cardPlace)
        {
            case PlayerCard1:
                return EnemyCard1;
            case PlayerCard2:
                return EnemyCard2;
            case EnemyCard1:
                return PlayerCard1;
            case EnemyCard2:
                return PlayerCard2;
        }
        return cardPlace;
    }
    public enum TipsEnum
    {
        FirstNc,     //f
        LongPressNc, //f
        Energy,      //f
        Draw,        //f
        EndTurn,     //f
        HpDrop,    
        FirstDragon,
        RankInNumber,//f
        RankMenu,    //f
        LastTurn,
        RaiseBtn,    //f
        Vision,      //f
        Emojis,      //f
        ElementSkill,    
    }
    public enum TutorialObjectEnum
    {
        startGame = 0,
        coins = 1,
        energy = 2,
        pu = 3,
        puCost = 4,
        pu2 = 5,
        endTurn = 8,
        vision = 9,
        rankMenu = 10,
        flipSkill = 11,
        cardToFlipFreeze = 12,
        cardToFlip = 13,
        eyeSymbol = 14,
        drawPu = 15,
        lastEnergyLefy = 16,
        lastTurnPu = 17,
        emojiSelector = 18,
        lastTurnEnergy = 19,
        pu3 = 20,
        pu3select = 21,
        cardToSelectWind = 22,
        rankUp = 23,
        end = 24,

        board = 30,

    }
    public enum ElementalSkill
    {
        fire = 0,
        ice = 1,
        wind = 2,
    }
    /*  public const string BoardCardSlutFlop1 = "BFlop1";
      public const string BoardCardSlutFlop2 = "CardB";
      public const string BoardCardSlutFlop3 = "CardD";
      public const string BoardCardSlutTurn4 = "CardD";
      public const string BoardCardSlutRiver5 = "CardD";
      public const string DeckCardSlut1 = "Deck1";
      public const string DeckCardSlut2 = "Deck2";
      private const string playerCardA = "PlayerCard1";
      private const string playerCardB = "PlayerCard2";
      private const string enemyCardA = "EnemyCard1";
      private const string enemyCardB = "EnemyCard2";*/

}
