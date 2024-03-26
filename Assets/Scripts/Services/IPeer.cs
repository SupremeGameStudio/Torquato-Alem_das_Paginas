using BattleGame.Networking.Data;

namespace Services {
    public interface IPeer {
        
        public ulong UnicId { get; }
        
        public int Pos { get; set; }
        
        public string Name { get; }
        
        public bool IsConnected { get; }
        
        public int State { get; set; }
        
        public NetPlayerData Data { get; set; }

        public void SendMessage(Message message);
    }
}