using MyBox;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Constants : Singleton<Constants>
{
    public const string PlayerCardsTag = "CardP";
    public const string EnemyCardsTag = "CardE";
    public const string BoardCardsTag = "CardB";
    public const string DeckCardsTag = "CardD";
    public const string AllCardsTag = "All";
    public const string PoolCardTag = "Pool";
    public enum CardsOwener { Player, Enemy, Board, Deck, Pool,}

public const string PlayerCard1 = "PlayerCard1";
    public const string PlayerCard2 = "PlayerCard2";
    public const string EnemyCard1 = "EnemyCard1";
    public const string EnemyCard2 = "EnemyCard2";
    public static readonly string[] BoardCards = { "BFlop1", "BFlop2", "BFlop3", "BTurn4", "BRiver5" };
    public static readonly string[] deckCardsNames = { "Deck1", "Deck2" };
    public static readonly string[] ghostCardsNames = { "PlayerGhost", "EnemyGhost" , "BGhost" };
    public static string ReplacePuInfo = "Choose one PowerUp and replace it with a new one.";

    public  string ConvertCardPlaceForEnemy(string cardPlace)
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
