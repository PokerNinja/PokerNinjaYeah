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

       




    }


    
}