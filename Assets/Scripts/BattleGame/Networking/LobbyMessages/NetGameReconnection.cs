using System;
using BattleGame.Networking.Data;

namespace BattleGame.Networking.LobbyMessages {
    [Serializable]
    public class NetGameReconnection : NetMessage {
        public ulong[] peerUnicId;
        public int[] peerPos;
        public NetPlayerData[] peerData;
        public NetGameData recreate;
    
        public override void Handle(GameHandler handler) {
            
        }
    }
}