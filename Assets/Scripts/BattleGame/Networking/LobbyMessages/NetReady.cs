using System;
using BattleGame.Networking.Data;

namespace BattleGame.Networking.LobbyMessages {
    [Serializable]
    public class NetReady : NetMessage {
        public ulong peerId;
        public int state;
        public NetPlayerData data;
    
        public override void Handle(GameHandler handler) {
            handler.Handle(this);
        }

    }
}