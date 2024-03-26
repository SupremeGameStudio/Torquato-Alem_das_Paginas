using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Services.Local {
    public class LocalTcpServer {
        private TcpListener tcpServer;
        private UdpClient udpServer;

        private List<ClientSource> newClients = new List<ClientSource>();
        private List<ClientSource> clients = new List<ClientSource>();

        public Action<LocalTcpPeer, ulong, string> OnConnectedAction;
        public Action<ulong, Message> OnDataAction;
        public Action<ulong> OnDisconnectedAction;
        
        public LocalTcpServer(TcpListener tcpServer, UdpClient udpServer) {
            this.tcpServer = tcpServer;
            this.udpServer = udpServer;
        }

        public static async Task<LocalTcpServer> Connect() {
            TcpListener tcpServer = null;
            UdpClient udpServer = null;

            bool result = false;

            await Task.Run(() => {
                try {
                    tcpServer = new TcpListener(IPAddress.Any, LocalManager.TcpPort);
                    tcpServer.Start();
                } catch (Exception e) {
                    Debug.LogError(e);
                    try {
                        tcpServer?.Stop();
                    } catch {
                        // ignored
                    }

                    return;
                }

                try {
                    udpServer = new UdpClient();
                    udpServer.EnableBroadcast = true;
                    result = true;
                } catch (Exception e) {
                    Debug.LogError(e);
                    try {
                        udpServer?.Close();
                    } catch {
                        // ignored
                    }
                }
            });

            return result ? new LocalTcpServer(tcpServer, udpServer) : null;
        }

        public void Loop() {
            while (tcpServer != null && tcpServer.Pending()) {
                OnConnected();
            }
            
            lock (newClients) {
                foreach (var newClient in newClients) {
                    clients.Add(newClient);
                    OnConnectedAction(newClient.peer, newClient.id, newClient.name);
                }
                newClients.Clear();
            }
            
            foreach (var client in clients) {
                while (client.peer.NextMessage() > 0) {
                    OnDataAction(client.id, client.peer.ReadMessage());
                }
            }

            for (var i = 0; i < clients.Count; i++) {
                var client = clients[i];
                if (client.peer.IsDisconected()) {
                    clients.Remove(client);
                    OnDisconnectedAction(client.id);
                    i--;
                }
            }
        }

        async void OnConnected() {
            TcpClient tcpClient = null;
            LocalTcpPeer tcpPeer = null;
            try {
                tcpClient = await tcpServer.AcceptTcpClientAsync();
                tcpClient.GetStream().WriteTimeout = 10000;
                tcpClient.GetStream().ReadTimeout = 30000;
                
                tcpPeer = new LocalTcpPeer(tcpClient);
                
                Stopwatch stopwatch = Stopwatch.StartNew();
                TimeSpan maxDuration = TimeSpan.FromSeconds(5);
                while (stopwatch.Elapsed < maxDuration && !tcpPeer.IsDisconected()) {
                    if (tcpPeer.NextMessage() > 0) {
                        Message message = tcpPeer.ReadMessage();
                        ulong clientId = message.ReadLong();
                        string clientName = message.ReadString();
                        
                        lock (newClients) {
                            newClients.Add(new ClientSource(tcpPeer, clientId, clientName));
                        }
                        return;
                    }
                }
            } catch {
                // ignored
            }
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

        public void Close() {

            try {
                tcpServer.Stop();
            } catch {
                // ignored
            }

            try {
                udpServer.Close();
            } catch {
                // ignored
            }

            foreach (var client in clients) {
                client.peer.Close();
            }

            foreach (var client in clients) {
                client.peer.Wait();
            }
        }

        public void BroadcastUDP(byte[] data, int size) {
            try {
                udpServer.Send(data, size, new IPEndPoint(IPAddress.Broadcast, LocalManager.UdpPort));
            } catch {
                try {
                    udpServer = new UdpClient();
                    udpServer.EnableBroadcast = true;
                } catch {
                    // ignored
                }
            }
        }

        public void BroadcastTCP(byte[] data, int size) {
            
        }

        class ClientSource {
            public LocalTcpPeer peer;
            public ulong id;
            public string name;

            public ClientSource(LocalTcpPeer peer, ulong id, string name) {
                this.peer = peer;
                this.id = id;
                this.name = name;
            }
        }
    }
}