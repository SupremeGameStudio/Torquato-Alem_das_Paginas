#if !UNITY_ANDROID || UNITY_EDITOR
using BattleGame.Networking.Data;
using Steamworks;
using UnityEngine;

namespace Services.Steam {
    public class SteamPeer : IPeer {
        
        public ulong UnicId { get; }
        
        public int Pos { get; set; }
        
        public string Name { get; }
        
        public bool IsConnected { get; private set; }
        
        public int State { get; set; }
        
        public NetPlayerData Data { get; set; }

        protected Friend friend;
        protected float lastMessageTime;

        public SteamPeer(SteamId id, string name) {
            this.UnicId = id;
            this.Name = name;
            this.IsConnected = false;
        }

        public SteamPeer(Friend friend, int pos = 0) {
            this.UnicId = friend.Id;
            this.Name = friend.Name;
            this.IsConnected = true;
            this.friend = friend;
            this.Pos = pos;
            SteamNetworking.AcceptP2PSessionWithUser(friend.Id);
        }

        public void Close() {
            if (IsConnected) {
                IsConnected = false;
                SteamNetworking.CloseP2PSessionWithUser(friend.Id);
            }
        }
        
        public void SendMessage(Message message) {
            if (IsConnected) {
                var sent = SteamNetworking.SendP2PPacket(friend.Id, message.data, message.Length);
                if (!sent) {
                    var sent2 = SteamNetworking.SendP2PPacket(friend.Id, message.data, message.Length);
                    if (!sent2) {
                        Debug.Log("Sent Failed to : " + Name);
                        Close();
                    }
                }
            }
        }

        public void MessageReceived() {
            lastMessageTime = Time.unscaledTime;
        }

        public float LastMessageReceived() {
            if (lastMessageTime == 0) {
                lastMessageTime = Time.unscaledTime;
            }

            return lastMessageTime;
        }
    }
}
#endif