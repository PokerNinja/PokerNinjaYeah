using System;

namespace Serializables
{
    [Serializable]
    public class GameInfo
    {
        public string gameId;
        public int prize;
        public string[] playersIds;
        public string localPlayerId;
        public string EnemyId;
        public string[] cardDeck;
        public string[] puDeck;
        public PowerUpInfo powerup;
        public string creatoOn;
        public string turn;
        public string[] log;
    }

}
