using System.Collections.Generic;
using System.Linq;
using BattleGame.Networking.Data;
using UnityEngine;

namespace Services.Local {
    public class LocalLobbyClient : ILobby {
        
        public IPeer Owner { get; private set; }
        
        public bool IsOwner { get => false; }
        
        public bool IsLocked { get; private set; }
        
        public ulong UnicId { get; private set; }
        
        public string OwnerName { get; set; }
        
        public int MaxPlayers { get; set; }
        
        public int Players { get; private set; }
        
        public string MapName { get; set; }
        
        public int Mode { get; set; }
        
        public int Difficulty { get; set; }
        
        public int Upgrades { get; set; }

        public MessageListener Listener { get; } = new MessageListener();

        public IPeer MyPeer { get; private set; }

        public IReadOnlyList<IPeer> Peers {
            get => peers;
        }

        public IPeer FindPeer(ulong unicId) {
            return peers.FirstOrDefault(peer => peer.UnicId == unicId);
        }

        public IPeer FindPeerByPos(int pos) {
            return peers.FirstOrDefault(peer => peer.Pos == pos);
        }

        private LocalTcpClient client;
        private float udpTimer;
        private Message lobbyData = new Message(Message.Type.LobbyData);
        private List<LocalPeer> peers = new List<LocalPeer>();
        
        public LocalLobbyClient(LocalTcpClient client) {
            this.client = client;
            
            LocalPeer owner = new LocalPeer(ServicesManager.Current.GetId(), ServicesManager.Current.GetName());
            MyPeer = owner;
            
            client.OnServerDisconnected = () => {
                Listener.EnqueueMessage(new Message(Message.Type.ServerDisconnected).Write(MyPeer.UnicId).Reset());
            };
            client.OnDataAction = (message) => {
                if (message.type == Message.Type.LobbyData) {
                    ConsumeLobbyData(message.Reset());
                } else {
                    Listener.EnqueueMessage(message.Reset());
                }
            };
        }

        private void ConsumeLobbyData(Message message) {
            IsLocked = message.ReadBool();
            UnicId = message.ReadLong();
            OwnerName = message.ReadString();
            MaxPlayers = message.ReadInt();
            Players = message.ReadInt();
            MapName = message.ReadString();
            Mode = message.ReadInt();
            Difficulty = message.ReadInt();
            Upgrades = message.ReadInt();

            List<LocalPeer> newPeers = new List<LocalPeer>();
            int count = lobbyData.ReadInt();
            for (int i = 0; i < count; i++) {
                bool owner = lobbyData.ReadBool();
                ulong id = lobbyData.ReadLong();
                string name = lobbyData.ReadString();
                int pos = lobbyData.ReadInt();
                int state = lobbyData.ReadInt();
                bool connected = lobbyData.ReadBool();

                var prevPeer = FindPeer(id);
                var newPeer = new LocalPeer(id, name, pos, state, connected, prevPeer?.Data);
                newPeers.Add(newPeer);
                if (owner) {
                    Owner = newPeer;
                }

                if (id == MyPeer.UnicId) {
                    MyPeer = newPeer;
                }
            }

            peers = newPeers;
            Listener.EnqueueMessage(new Message(Message.Type.LobbyData).Reset());
        }

        public void Lock(ulong[] unicIds, int[] pos, NetPlayerData[] data) {
            throw new System.NotImplementedException();
        }

        public void TestConnection() {
            throw new System.NotImplementedException();
        }

        public void OwnerLock() {
            throw new System.NotImplementedException();
        }

        public void Unlock() {
            throw new System.NotImplementedException();
        }

        public void Close() {
            client.Close();
            foreach (var peer in peers) {
                peer.Close();
            }
        }

        public void BroadCast(Message message) {
            throw new System.NotImplementedException();
        }

        public void Kick(ulong peerId) {
            throw new System.NotImplementedException();
        }

        public void Loop() {
            client.Loop();
        }
    }
}