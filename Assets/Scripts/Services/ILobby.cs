using System.Collections.Generic;
using BattleGame.Networking.Data;

namespace Services {
    public interface ILobby {
        public IPeer Owner { get; }
        public bool IsOwner { get; }
        public bool IsLocked { get; }
        
        public ulong UnicId { get; }
        
        public string OwnerName { get; set; }
        public int MaxPlayers { get; set; }
        public int Players { get; }
        
        public string MapName { get; set; }
        public int Mode { get; set; }
        public int Difficulty { get; set; }
        public int Upgrades { get; set; }

        public MessageListener Listener { get; }
        
        public IReadOnlyList<IPeer> Peers { get; }

        public IPeer FindPeer(ulong unicId);
        
        public IPeer FindPeerByPos(int pos);
        
        public IPeer MyPeer { get; }

        public void Lock(ulong[] unicIds, int[] pos, NetPlayerData[] data);

        public void TestConnection();

        public void OwnerLock();

        public void Unlock();
        
        public void Close();

        public void BroadCast(Message message);
        
        public void Kick(ulong peerId);

        public void Loop();
    }
}