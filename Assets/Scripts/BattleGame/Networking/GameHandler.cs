using BattleGame.Networking.Data;
using BattleGame.Networking.LobbyMessages;
using Services;
using UnityEngine;

namespace BattleGame.Networking {
    public class GameHandler : MonoBehaviour {
        private ILobby lobby;
        private float autoUpdateTimer;

        public void Loop() {
            Message message;
            while (lobby != null && (message = lobby.Listener.DequeueMessage()) != null) {
                if (message.type == Message.Type.Connection) {
                    HandleConnection(message.ReadLong());
                
                } else if (message.type == Message.Type.Disconnection) {
                    HandleDisconnection(message.ReadLong());
                
                } else if (message.type == Message.Type.ServerDisconnected) {
                    lobby = null;
                    HandleServerDisconnection();
                
                } else if (message.type == Message.Type.LobbyData) {
                
                } else if (message.type == Message.Type.Data) {
                    var netMessage = NetMessage.Read(message);
                    if (netMessage == null) {
                        Handle<NetMessage>(null);
                    } else {
                        netMessage.Handle(this);
                    }
                }
            }
            
            autoUpdateTimer -= Time.deltaTime;
            if (autoUpdateTimer <= 0) {
                autoUpdateTimer = 5f;
                if (lobby != null) {
                    lobby.TestConnection();
                }
            }

            if (ServicesManager.Current.Lobby == null && lobby != null) {
                lobby = null;
                HandleServerDisconnection();
            }
        }
        
        /// [Anyone]
        ///
        /// Event happens when some peer is connected
        public virtual void HandleConnection(ulong peerId) {
            
        }
    
        /// [Anyone]
        ///
        /// Event happens when some peer is disconnected
        public virtual void HandleDisconnection(ulong peerId) {
            
        }
    
        /// [Anyone]
        ///
        /// Event happens when the current user lose internet connection
        public virtual void HandleServerDisconnection() {
            
        }

        /// [Anyone]
        /// 
        /// Fallback handler
        public void Handle<T>(T message) where T : NetMessage {
            if (!enabled) return;
            
            Debug.Log("Received a unexpected message : " + (message == null ? "Null" : message.GetType().ToString()));
        }
    
        /// [Anyone]
        /// 
        /// Set the current peer state. Broadcast a NetReady
        /// <param name="state">NetReady State</param>
        public void OnNetReady(int state) {
            if (!enabled) return;
            
            lobby.MyPeer.State = state;
            lobby.MyPeer.Data = new NetPlayerData(ServicesManager.CurrentGameData);
            var netReady = new NetReady {
                peerId = lobby.MyPeer.UnicId,
                state = lobby.MyPeer.State,
                data = lobby.MyPeer.Data
            };
            lobby.BroadCast(NetMessage.Write(netReady));
        }

        public void Handle(NetReady netReady) {
            if (!enabled) return;
            
            var peer = lobby.FindPeer(netReady.peerId);
            if (peer != null) {
                peer.State = netReady.state;
                peer.Data = netReady.data;
            }
        }

        public void OnKick(int pos) {
            if (!enabled) return;
        
            var netKick = new NetKick {
                peerId = lobby.FindPeerByPos(pos).UnicId
            };
            lobby.Kick(netKick.peerId);
            SendToAll(netKick);
        }
    
        public void Handle(NetKick netKick) {
            if (!enabled) return;

            if (netKick.peerId == lobby.MyPeer.UnicId) {
                // TODO - LEAVE
            }
        }

        /// [Anyone]
        /// 
        /// Close the current lobby and connection
        public void Leave() {
            ServicesManager.Current.CloseLobby();
        }
        
        /// [Anyone]
        /// 
        /// Broadcast a message
        public void SendToAll(NetMessage netMessage) {
            if (!enabled) return;
            
            lobby?.BroadCast(NetMessage.Write(netMessage));
        }
    }
}