using System;

namespace Serializables
{
    [Serializable]
    internal class EmojiInfo
    {
        public string playerId;
        public int emojiId;
        public long timeStamp;

        public EmojiInfo(string playerId, int emojiId, long timeStamp)
        {
            this.playerId = playerId;
            this.emojiId = emojiId;
            this.timeStamp = timeStamp;
        }
    }
}