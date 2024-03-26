using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace Services.Local {
    public class LocalTcpClient {
        
        private TcpClient tcpClient;
        private LocalTcpPeer peer;
        
        public Action<Message> OnDataAction;
        public Action OnServerDisconnected;
        private bool disconnected;

        LocalTcpClient(TcpClient tcpClient, LocalTcpPeer peer) {
            this.tcpClient = tcpClient;
            this.peer = peer;
        }

        public void Loop() {
            while (peer.NextMessage() > 0) {
                OnDataAction(peer.ReadMessage());
            }

            if (peer.IsDisconected() && !disconnected) {
                disconnected = true;
                Close();
                
                OnServerDisconnected();
            }
        }

        public void Close() {
            peer.Close();
        }
        
        public static async Task<LocalTcpClient> Connect(IPAddress joinIP) {
            bool result = false;
            TcpClient tcpClient = null;
            LocalTcpPeer tcpPeer = null;
        
            await Task.Run(() => {
                try {
                    tcpClient = new TcpClient();
                    tcpClient.Connect(joinIP, LocalManager.TcpPort);
                    tcpClient.GetStream().WriteTimeout = 10000;
                    tcpClient.GetStream().ReadTimeout = 30000;

                    tcpPeer = new LocalTcpPeer(tcpClient);
                    Message message = new Message(Message.Type.Tcp);
                    message.Write(ServicesManager.Current.GetId());
                    message.Write(ServicesManager.Current.GetName());
                    tcpPeer.WriteMessage(message);
                    
                    result = true;
                } catch (Exception e) {
                    Debug.LogError(e);
                    try {
                        tcpPeer?.Close();
                    } catch {
                        // ignored
                    }
                    try {
                        tcpClient?.Close();
                    } catch {
                        // ignored
                    }
                }
            });

            return result ? new LocalTcpClient(tcpClient, tcpPeer) : null;
        }
    }
}