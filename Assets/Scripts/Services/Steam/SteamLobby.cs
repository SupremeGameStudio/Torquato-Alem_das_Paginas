#if !UNITY_ANDROID || UNITY_EDITOR

using System;
using System.Collections.Generic;
using BattleGame.Networking.Data;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Services.Steam {
    public class SteamLobby : ILobby {

        private static float DisconnectionLimit = 15;
    
        private Lobby lobby;

        private List<SteamPeer> peers = new List<SteamPeer>();
        private List<PeerLock> peerLocks = new List<PeerLock>();
    
        private Action<Lobby> onLobbyData;
        private Action<Lobby, Friend> onConnected;
        private Action<Lobby, Friend> onDisconnected;
        private Action<Lobby, Friend, Friend> onKickBan;
        private Action<Lobby, Friend, string> onChatMessage;
        private MessageReader reader;
        private float lastMessageChatTime;

        public IPeer Owner { get => FindPeer(lobby.Owner.Id); }

        public bool IsOwner { get => lobby.IsOwnedBy(SteamClient.SteamId); }
    
        public bool IsLocked { get; private set; }

        public bool IsClosed { get; private set; }

        public int MyPeerId { get; private set; }

        public ulong UnicId { get => lobby.Id; }

        public string MapName {
            get => lobby.GetData("mapName");
            set => lobby.SetData("mapName", value);
        }
    
        public string OwnerName {
            get => lobby.GetData("ownerName");
            set => lobby.SetData("ownerName", value);
        }
    
        public int MaxPlayers {
            get => GetInt("maxPlayers");
            set => lobby.SetData("maxPlayers", value.ToString());
        }
    
        public int Players {
            get => lobby.MemberCount;
        }
    
        public int Mode {
            get => GetInt("mode");
            set => lobby.SetData("mode", value.ToString());
        }

        public int Difficulty {
            get => GetInt("difficulty");
            set => lobby.SetData("difficulty", value.ToString());
        }

        public int Upgrades {
            get => GetInt("upgrades");
            set => lobby.SetData("upgrades", value.ToString());
        }

        private int GetInt(string name) {
            try {
                return int.Parse(lobby.GetData(name));
            } catch {
                return 0;
            }
        }

        public MessageListener Listener { get; } = new MessageListener();

        public int FindPosById(ulong unicId) {
            if (IsLocked) {
                for (int i = 0; i < peerLocks.Count; i++) {
                    if (peerLocks[i].unicId == unicId) {
                        return peerLocks[i].pos;
                    }
                }
            }

            return -1;
        }

        public ulong FindIdByPos(int pos) {
            if (IsLocked) {
                for (int i = 0; i < peerLocks.Count; i++) {
                    if (peerLocks[i].pos == pos) {
                        return peerLocks[i].unicId;
                    }
                }
            }

            return 0;
        }

        public NetPlayerData FindDataByPos(int pos) {
            if (IsLocked) {
                for (int i = 0; i < peerLocks.Count; i++) {
                    if (peerLocks[i].pos == pos) {
                        return peerLocks[i].data;
                    }
                }
            }

            return null;
        }

        public IPeer MyPeer { get; private set; }

        public IReadOnlyList<IPeer> Peers {
            get => peers;
        }

        public IPeer FindPeer(ulong unicId) {
            foreach (var peer in peers) {
                if (peer.UnicId == unicId) {
                    return peer;
                }
            }

            return null;
        }

        public IPeer FindPeerByPos(int pos) {
            foreach (var peer in peers) {
                if (peer.Pos == pos) {
                    return peer;
                }
            }

            return null;
        }

        public SteamLobby(Lobby lobby) {
            this.lobby = lobby;
            if (this.IsOwner) {
                lobby.SetJoinable(true);
                lobby.SetPublic();
                lobby.SetData("state", "public");
                lobby.MaxMembers = 4;
            
                lobby.SetData("ownerName", SteamClient.Name);
                lobby.SetData("maxPlayers", "4");
                lobby.SetData("mapName", "Default");
                lobby.SetData("mode", "0");
                lobby.SetData("difficulty", "0");
                lobby.SetData("upgrades", "0");
            }

            MyPeer = new SteamPeer(SteamClient.SteamId, SteamClient.Name);

            reader = new GameObject().AddComponent<MessageReader>();
            reader.Lobby = this;
        
            onConnected = (lobby, friend) => {
                if (AddPeer(friend)) {
                    AddMessage(new Message(Message.Type.Connection).Write(friend.Id).Reset());
                }
            };

            onDisconnected = (lobby, friend) => {
                if (friend.IsMe) {
                    AddMessage(new Message(Message.Type.ServerDisconnected).Write(MyPeer.UnicId).Reset());
                } else if (RemovePeer(friend.Id)) {
                    AddMessage(new Message(Message.Type.Disconnection).Write(friend.Id).Reset());
                }
            };

            onKickBan = (lobby, friend, owner) => {
                if (RemovePeer(friend.Id)) {
                    AddMessage(new Message(Message.Type.Disconnection).Write(friend.Id).Reset());
                }
            };

            onLobbyData = (lobby) => {
                if (IsOwner && OwnerName != SteamClient.Name) {
                    OwnerName = SteamClient.Name;
                }
                AddMessage(new Message(Message.Type.LobbyData));
            };

            onChatMessage = (lobby, friend, chat) => {
                lastMessageChatTime = Time.unscaledTime;
                MessageReceived(friend.Id);
            };
        
            SteamMatchmaking.OnLobbyMemberJoined += onConnected;
            SteamMatchmaking.OnLobbyMemberDisconnected += onDisconnected;
            SteamMatchmaking.OnLobbyMemberLeave += onDisconnected;
            SteamMatchmaking.OnLobbyMemberKicked += onKickBan;
            SteamMatchmaking.OnLobbyMemberBanned += onKickBan;
            SteamMatchmaking.OnLobbyDataChanged += onLobbyData;
            SteamMatchmaking.OnChatMessage += onChatMessage;
        
            IsClosed = false;

            foreach (var friend in lobby.Members) {
                if (friend.Id != MyPeer.UnicId) {
                    if (AddPeer(friend)) {
                        AddMessage(new Message(Message.Type.Connection).Write(friend.Id));
                    }
                }
            }
        }

        public void Lock(ulong[] unicIds, int[] pos, NetPlayerData[] data) {
            IsLocked = true;
            for (int i = 0; i < unicIds.Length; i++) {
                peerLocks.Add(new PeerLock(unicIds[i], pos[i], data[i]));
            }

            for (int i = 0; i < peers.Count; i++) {
                var peer = peers[i];
                int index = FindPosById(peer.UnicId);
                if (index > -1) {
                    peer.Pos = peer.Pos;
                    peer.Data = FindDataByPos(peer.Pos);
                } else {
                    peer.Close();
                    peers.RemoveAt(i--);
                }
            }

            MyPeer.Pos = FindPosById(MyPeer.UnicId);
        }

        public void TestConnection() {
            if (lastMessageChatTime == 0) {
                lastMessageChatTime = Time.unscaledTime;
            
            } else if (Time.unscaledTime - lastMessageChatTime > DisconnectionLimit) {
                AddMessage(new Message(Message.Type.ServerDisconnected).Write(MyPeer.UnicId).Reset());
            
            } else {
                for (int i = 0; i < peers.Count; i++) {
                    if (Time.unscaledTime - peers[i].LastMessageReceived() > DisconnectionLimit) {
                        ulong peerId = peers[i].UnicId;
                        if (RemovePeer(peerId)) {
                            AddMessage(new Message(Message.Type.Disconnection).Write(peerId).Reset());
                        }
                        break;
                    }
                }
            }

            lobby.SendChatString("Test");
        }

        public void OwnerLock() {
            IsLocked = true;
        
            int max = peers.Count + 1;
        
            MaxPlayers = max;
            MyPeer.Pos = 0;
            for (int i = 0; i < peers.Count; i++) {
                peers[i].Pos = i + 1;
            }
        
            peerLocks.Add(new PeerLock(MyPeer.UnicId, MyPeer.Pos, MyPeer.Data));
            foreach (var peer in peers) {
                peerLocks.Add(new PeerLock(peer.UnicId, peer.Pos, peer.Data));
            }
        
            string state = "private";
            for (int i = 0; i < peerLocks.Count; i++) {
                state += "[" + peerLocks[i].unicId + "]";
            }
            lobby.SetData("state", state);
        }

        public void Unlock() {
            peerLocks.Clear();
            IsLocked = false;
            if (IsOwner) {
                MaxPlayers = 4;
                lobby.SetData("state", "public");
            }
        }

        public void Close() {
            if (!IsClosed) {
                IsClosed = true;
                GameObject.Destroy(reader.gameObject);
            
                SteamMatchmaking.OnLobbyMemberJoined -= onConnected;
                SteamMatchmaking.OnLobbyMemberDisconnected -= onDisconnected;
                SteamMatchmaking.OnLobbyMemberLeave -= onDisconnected;
                SteamMatchmaking.OnLobbyMemberKicked -= onKickBan;
                SteamMatchmaking.OnLobbyMemberBanned -= onKickBan;
                SteamMatchmaking.OnLobbyDataChanged -= onLobbyData;
                SteamMatchmaking.OnChatMessage -= onChatMessage;
                lobby.Leave();
            }
        }

        public void BroadCast(Message message) {
            foreach (var peer in peers) {
                peer.SendMessage(message);
            }
        }

        public void Kick(ulong peerId) {
            if (IsOwner) {
                string state = lobby.GetData("state");
                state = state.Replace("[" + peerId + "]", "");
                lobby.SetData("state", state);
            }
        }

        public void Loop() {
            
        }

        private bool AddPeer(Friend friend) {
            if (IsLocked) {
                int pos = FindPosById(friend.Id);

                if (pos > -1) {
                    for (int i = 0; i < peers.Count; i++) {
                        if (peers[i].UnicId == friend.Id) {
                            peers[i] = new SteamPeer(friend, pos);
                            return true;
                        }
                    }

                    peers.Add(new SteamPeer(friend, pos));
                    return true;
                } else {
                    return false;
                }
            } else {
                for (int i = 0; i < peers.Count; i++) {
                    if (peers[i].UnicId == friend.Id) {
                        peers[i] = new SteamPeer(friend);
                        return true;
                    }
                }

                peers.Add(new SteamPeer(friend));
                return true;
            }
        }
    
        private bool RemovePeer(ulong peerId) {
            if (IsLocked) {
                int pos = FindPosById(peerId);
            
                if (pos > -1) {
                    for (int i = 0; i < peers.Count; i++) {
                        var peer = peers[i];
                        if (peer.UnicId == peerId) {
                            peer.Close();
                            peers.RemoveAt(i);
                            return true;
                        }
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

        private void MessageReceived(ulong unicId) {
            foreach (var peer in peers) {
                if (peer.UnicId == unicId) {
                    peer.MessageReceived();
                }
            }
        }

        private void AddMessage(Message message) {
            Listener.EnqueueMessage(message);
        }

        private class PeerLock {
            public ulong unicId;
            public int pos;
            public NetPlayerData data;

            public PeerLock(ulong unicId, int pos, NetPlayerData data) {
                this.unicId = unicId;
                this.pos = pos;
                this.data = data;
            }
        }

        class MessageReader : MonoBehaviour {
        
            public SteamLobby Lobby;
        
            private void Start() {
                DontDestroyOnLoad(gameObject);
            }

            private void Update() {
                while (SteamNetworking.IsP2PPacketAvailable()) {
                    var packet = SteamNetworking.ReadP2PPacket();
                    if (packet.HasValue) {
                        Lobby.MessageReceived(packet.Value.SteamId);
                        Lobby.AddMessage(new Message(Message.Type.Data, packet.Value.Data));
                    }
                }
            }
        }
    }
}
#endif