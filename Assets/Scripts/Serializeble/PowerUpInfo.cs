using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Serializables
{
    [Serializable]
    public class PowerUpInfo
    {
        public string playerId;
        public string powerupName;
        public string cardPlace1;
        public string cardPlace2;
        public long timeStamp;
        public int slot;

        //In Progress PU
        public PowerUpInfo(string playerId, string powerupName, string cardPlace1, string cardPlace2, int slot,  long timeStamp)
        {
            this.playerId = playerId;
            this.powerupName = powerupName;
            this.cardPlace1 = ReplaceNullWithEmpty(cardPlace1);
            this.cardPlace2 = ReplaceNullWithEmpty(cardPlace2);
            this.slot = slot;
            this.timeStamp = timeStamp;
        }

        private string ReplaceNullWithEmpty(string cardPlace)
        {
            if(cardPlace == string.Empty)
            {
                return "empty";
            }

            return cardPlace;

        }

        //Initial PU
        public PowerUpInfo(string playerId, string powerupName, int amount)
        {
            this.playerId = playerId;
            this.powerupName = powerupName;
            this.cardPlace1 = "";
            this.cardPlace2 = "";
            this.timeStamp = 0;

        }




    }



    public enum CardSelectedEnum
    {
        [Description("IM_Desc")]

        NoCard = -1,
        PlayerA = 0,
        PlayerB = 1,
        EnemyA = 2,
        EnemyB = 3,
        [Description("IM_Desc")]

        BoardA = 4,
        BoardB = 5,
        BoardC = 6,
        BoardD = 7,
        BoardE = 8,
        DeckA = 9,
        DeckB = 10,
        DeckC = 11,
    }

    public enum CardsToSelectEnum
    {
        NoCards = -1,
        PlayerCards = 0,
        EnemyCards = 1,
        BoardCards = 2,
        Deck1Card = 3,
        Deck2Cards = 4,
        Deck3Cards = 5,
    }
    
}