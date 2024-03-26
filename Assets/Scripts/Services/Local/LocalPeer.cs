using BattleGame.Networking.Data;

namespace Services.Local {
    public class LocalPeer : IPeer {
        
        public ulong UnicId { get; }
        
        public int Pos { get; set; }
        
        public string Name { get; }
        
        public bool IsConnected {
            get => forceConnected ?? (peer != null && !peer.IsDisconected());
        }
        
        public int State { get; set; }
        
        public NetPlayerData Data { get; set; }

        private bool? forceConnected;
        private LocalTcpPeer peer;

        public LocalPeer(ulong myId, string myName) {
            UnicId = myId;
            Name = myName;
            forceConnected = true;
        }
        
        public LocalPeer(LocalTcpPeer peer, ulong id, string name, int pos = 0) {
            this.peer = peer;
            UnicId = id;
            Name = name;
            Pos = pos;
        }
        
        public LocalPeer(ulong id, string name, int pos, int state, bool connected, NetPlayerData data) {
            UnicId = id;
            Name = name;
            Pos = pos;
            State = state;
            forceConnected = connected;
            Data = data;
        }

        public void UpdatePeer(LocalTcpPeer peer) {
            this.peer = peer;
        }

        public void Close() {
            if (peer != null) {
                peer.Close();
                peer = null;
            }
        }
        
        public void SendMessage(Message message) {
            if (peer != null && !peer.IsDisconected()) {
                peer.Send(message);
            }
        }
        
        public Message ReadMessage() {
            if (peer != null && peer.NextMessage() > 0) {
                return peer.ReadMessage();
            }

            return null;
        }
    }
}