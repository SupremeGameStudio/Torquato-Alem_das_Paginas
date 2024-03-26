using System;
using System.Collections.Generic;
using System.Linq;
using BattleGame.Networking.Data;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Services.Local {
    public class LocalLobbyServer : ILobby {
        
        public IPeer Owner { get => MyPeer; }
        
        public bool IsOwner { get => true; }

        private bool isLocked;
        public bool IsLocked {
            get { return isLocked; }
            set { isLocked = value; Invalidate(); }
        }

        private ulong unicId;
        public ulong UnicId {
            get { return unicId; }
            set { unicId = value; Invalidate(); }
        }

        private string ownerName;
        public string OwnerName {
            get { return ownerName; }
            set { ownerName = value; Invalidate(); }
        }

        private int maxPlayers;
        public int MaxPlayers {
            get { return maxPlayers; }
            set { maxPlayers = value; Invalidate(); }
        }

        private int players;
        public int Players {
            get { return players; }
            private set { players = value; Invalidate(); }
        }

        private string mapName;
        public string MapName {
            get { return mapName; }
            set { mapName = value; Invalidate(); }
        }

        private int mode;
        public int Mode {
            get { return mode; }
            set { mode = value; Invalidate(); }
        }

        private int difficulty;
        public int Difficulty {
            get { return difficulty; }
            set { difficulty = value; Invalidate(); }
        }

        private int upgrades;
        public int Upgrades {
            get { return upgrades; }
            set { upgrades = value; Invalidate(); }
        }

        public MessageListener Listener { get; } = new MessageListener();

        public IPeer MyPeer { get; }

        public IReadOnlyList<IPeer> Peers {
            get => peers;
        }

        public IPeer FindPeer(ulong unicId) {
            return peers.FirstOrDefault(peer => peer.UnicId == unicId);
        }

        public IPeer FindPeerByPos(int pos) {
            return peers.FirstOrDefault(peer => peer.Pos == pos);
        }

        private LocalTcpServer server;
        private bool invalid;
        private float udpTimer;
        private Message lobbyData = new Message(Message.Type.LobbyData);
        private List<LocalPeer> peers = new List<LocalPeer>();
        
        public LocalLobbyServer(LocalTcpServer server) {
            this.server = server;
            if (IsOwner) {
                IsLocked = false;

                UnicId = (ulong) DateTimeOffset.Now.ToUnixTimeMilliseconds();
                MaxPlayers = 4;
                OwnerName = ServicesManager.Current.GetName();
                MapName = "Default";
                Mode = 0;
                Difficulty = 0;
                Upgrades = 0;
            }

            LocalPeer owner = new LocalPeer(ServicesManager.Current.GetId(), ServicesManager.Current.GetName());
            MyPeer = owner;
            peers.Add(owner);
            
            server.OnConnectedAction = (peer, id, name) => {
                if (AddPeer(peer, id, name)) {
                    Message message = new Message(Message.Type.Connection).Write(id);
                    Listener.EnqueueMessage(message.Reset());
                    BroadCast(message.Reset());
                }
            };
            server.OnDisconnectedAction = (id) => {
                if (RemovePeer(id)) {
                    Message message = new Message(Message.Type.Disconnection).Write(id);
                    Listener.EnqueueMessage(message.Reset());
                    BroadCast(message.Reset());
                }
            };
            server.OnDataAction = (id, message) => {
                Listener.EnqueueMessage(message.Reset());
                
                // ReSend - Message Broadcast
                foreach (var peer in peers) {
                    if (peer.UnicId != id) {
                        peer.SendMessage(message.Reset());
                    }
                }
            };
        }

        private void Invalidate() {
            invalid = true;
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
            server.Close();
            foreach (var peer in peers) {
                peer.Close();
            }
        }

        public void BroadCast(Message message) {
            ConsumeInvalidate();
            foreach (var peer in peers) {
                peer.SendMessage(message.Reset());
            }
        }

        public void Kick(ulong peerId) {
            throw new System.NotImplementedException();
        }

        public void Loop() {
            server.Loop();
            udpTimer -= Time.deltaTime;
            if (udpTimer <= 0) {
                udpTimer = 2.5f;
                
                lobbyData.Reuse(Message.Type.Udp);
                lobbyData.Write(IsLocked);
                lobbyData.Write(UnicId);
                lobbyData.Write(OwnerName);
                lobbyData.Write(MaxPlayers);
                lobbyData.Write(Players);
                lobbyData.Write(MapName);
                lobbyData.Write(Mode);
                lobbyData.Write(Difficulty);
                lobbyData.Write(Upgrades);
                
                server.BroadcastUDP(lobbyData.data, lobbyData.Length);
            }

            ConsumeInvalidate();
        }

        private void ConsumeInvalidate() {
            if (invalid) {
                invalid = false;
                lobbyData.Reuse(Message.Type.LobbyData);
                lobbyData.Write(IsLocked);
                lobbyData.Write(UnicId);
                lobbyData.Write(OwnerName);
                lobbyData.Write(MaxPlayers);
                lobbyData.Write(Players);
                lobbyData.Write(MapName);
                lobbyData.Write(Mode);
                lobbyData.Write(Difficulty);
                lobbyData.Write(Upgrades);
                
                lobbyData.Write(peers.Count);
                foreach (var peer in peers) {
                    lobbyData.Write(peer == Owner);
                    lobbyData.Write(peer.UnicId);
                    lobbyData.Write(peer.Name);
                    lobbyData.Write(peer.Pos);
                    lobbyData.Write(peer.State);
                    lobbyData.Write(peer.IsConnected);
                }
                BroadCast(lobbyData.Reset());
                Listener.EnqueueMessage(new Message(Message.Type.LobbyData).Reset());
            }
        }

        private bool AddPeer(LocalTcpPeer tcpPeer, ulong id, string name) {
            Invalidate();
            
            foreach (var peer in peers) {
                if (peer.UnicId == id) {
                    peer.UpdatePeer(tcpPeer);
                    return true;
                }
            }
            
            if (!IsLocked && peers.Count < MaxPlayers) {
                peers.Add(new LocalPeer(tcpPeer, id, name));
                return true;
            }

            tcpPeer.Close();
            return false;
        }
    
        private bool RemovePeer(ulong peerId) {
            Invalidate();
            
            if (IsLocked) {
                foreach (var peer in peers) {
                    if (peer.UnicId == peerId) {
                        peer.Close();
                        return true;
                    }
                }
            
            } else {
                for (int i = 0; i < peers.Count; i++) {
                    var peer = peers[i];
                    if (peer.UnicId == peerId) {
                        peer.Close();
                        peers.RemoveAt(i);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}