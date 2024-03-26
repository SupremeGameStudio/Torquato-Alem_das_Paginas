using System;
using BattleGame.Networking.Data;

namespace BattleGame.Networking.LobbyMessages {
    [Serializable]
    public class NetGameStart : NetMessage {
        public ulong[] peerUnicId;
        public int[] peerPos;
        public NetPlayerData[] peerData;
    
        public override void Handle(GameHandler handler) {
            
        }
    }
}