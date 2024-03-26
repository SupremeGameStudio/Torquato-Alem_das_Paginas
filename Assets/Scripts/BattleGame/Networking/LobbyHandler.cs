using BattleGame.Networking.Data;
using BattleGame.Networking.LobbyMessages;
using Services;
using UnityEngine;

namespace BattleGame.Networking {
    public class LobbyHandler : MonoBehaviour {
        private ILobby lobby;
        private float autoUpdateTimer;

        public void Connect() {
            lobby = ServicesManager.Current.Lobby;
            lobby.Unlock();
        }
        
        public void Loop() {
            Message message;
            while (lobby != null && (message = lobby.Listener.DequeueMessage()) != null) {
                if (message.type == Message.Type.Connection) {
                    HandleConnection();

                } else if (message.type == Message.Type.Disconnection) {
                    HandleDisconnection();

                } else if (message.type == Message.Type.ServerDisconnected) {
                    lobby = null;
                    HandleServerDisconnection();

                } else if (message.type == Message.Type.LobbyData) {
                    HandleLobbyData();

                } else if (message.type == Message.Type.Data) {
                    NetMessage netMessage = NetMessage.Read(message);
                    switch (netMessage) {
                        case NetReady ready:
                            Handle(ready);
                            break;
                        case NetKick kick:
                            Handle(kick);
                            break;
                        case NetGameStart gameStart:
                            Handle(gameStart);
                            break;
                        case NetGameReconnection gameReconnection:
                            Handle(gameReconnection);
                            break;
                        default:
                            Handle(netMessage);
                            break;
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
        /// Set the current peer state. Broadcast a NetReady
        /// <param name="state">NetReady State</param>
        public void SetReady(int state) {
            lobby.MyPeer.State = state;
            lobby.MyPeer.Data = new NetPlayerData(ServicesManager.CurrentGameData);
            var netReady = new NetReady {
                peerId = lobby.MyPeer.UnicId,
                state = lobby.MyPeer.State,
                data = lobby.MyPeer.Data
            };
            lobby.BroadCast(NetMessage.Write(netReady));
        }

        /// [Anyone]
        /// 
        /// Close the current lobby and connection
        public void Leave() {
            ServicesManager.Current.CloseLobby();
        }

        /// [Oner Only]
        /// 
        /// Lock the current lobby. Broadcast NetGameStart. Use right before starting the game
        public void Lock() {
            lobby.OwnerLock();
        
            int max = lobby.MaxPlayers;
        
            var netGameStart = new NetGameStart {
                peerUnicId = new ulong[max],
                peerPos = new int[max],
                peerData = new NetPlayerData[max]
            };

            for (int i = 0; i < max; i++) {
                netGameStart.peerPos[i] = i;
                netGameStart.peerUnicId[i] = lobby.FindPeerByPos(i).UnicId;
                netGameStart.peerData[i] = lobby.FindPeerByPos(i).Data;
            }

            lobby.BroadCast(NetMessage.Write(netGameStart));
        }

        /// [Anyone]
        ///
        /// Event happens when some peer is connected
        public virtual void HandleConnection() {
            
        }
    
        /// [Anyone]
        ///
        /// Event happens when some peer is disconnected
        public virtual void HandleDisconnection() {
            
        }
    
        /// [Anyone]
        ///
        /// Event happens when the current user lose internet connection
        public virtual void HandleServerDisconnection() {
            
        }

        /// [Anyone]
        ///
        /// Event happens when the lobby data or peer data changes
        public virtual void HandleLobbyData() {
            
        }

        /// [Peer Only]
        ///
        /// Event happens when the Owner Kick the current player
        public virtual void HandleKick() {
            
        }

        /// [Peer Only]
        ///
        /// Event happens when the Owner start the game
        public virtual void HandleGameStart() {
            
        }

        /// [Peer Only]
        ///
        /// Event happens when the current user connect to a started Game
        public virtual void HandleReconnection(NetGameData gameData) {
            
        }

        private void Handle(NetMessage message) {
            // Debug.Log("Unexpected message : " + message.GetType());
        }

        // Everybody Receives
        private void Handle(NetReady ready) {
            var peer = lobby.FindPeer(ready.peerId);
            if (peer != null) {
                peer.State = ready.state;
                peer.Data = ready.data;
            }
            HandleLobbyData();
        
        }
    
        // Only Peers Receives
        private void Handle(NetKick kick) {
            if (kick.peerId == lobby.MyPeer.UnicId) {
                HandleKick();
            }
        }
    
        // Only Peers Receives
        private void Handle(NetGameStart gameStart) {
            lobby.Lock(gameStart.peerUnicId, gameStart.peerPos, gameStart.peerData);
            HandleGameStart();
        }

        // Only Reconected Peers Receives
        private void Handle(NetGameReconnection gameReconnection) {
            lobby.Lock(gameReconnection.peerUnicId, gameReconnection.peerPos, gameReconnection.peerData);
            HandleReconnection(gameReconnection.recreate);
        }
    }
}