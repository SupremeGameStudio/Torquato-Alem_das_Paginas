using System;

namespace BattleGame.Networking.LobbyMessages {
    [Serializable]
    public class NetKick : NetMessage {
        public ulong peerId;
    
        public override void Handle(GameHandler handler) {
            
        }
    }
}