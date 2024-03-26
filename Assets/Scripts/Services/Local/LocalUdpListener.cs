using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Debug = UnityEngine.Debug;

namespace Services.Local {
    public class LocalUdpListener {
        
        public static async Task<LobbyEntry[]> SearchLobbies() {
            UdpClient udpClient = new UdpClient();
            try {
                udpClient.Client.ReceiveTimeout = 2500;
                udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, LocalManager.UdpPort));
            } catch (Exception e) {
                Debug.LogError(e);

                try {
                    udpClient.Close();
                } catch {
                    // ignored
                }

                return null;
            }
            
            List<LobbyEntry> lobbyEntries = new List<LobbyEntry>();
            
            await Task.Run(() => {
                Stopwatch stopwatch = Stopwatch.StartNew();
                TimeSpan maxDuration = TimeSpan.FromSeconds(5);
                while (stopwatch.Elapsed < maxDuration) {
                    IPEndPoint from = new IPEndPoint(0, 0);
                    try {
                        byte[] data = udpClient.Receive(ref from);
                        var message = new Message(Message.Type.Udp, data);
                        
                        LobbyEntry entry = new LobbyEntry();
                        bool IsLocked = message.ReadBool();
                        entry.unicId = message.ReadLong();
                        entry.ownerName = message.ReadString();
                        entry.maxPlayers = message.ReadInt();
                        entry.players = message.ReadInt();
                        entry.mapName = message.ReadString();
                        entry.mode = message.ReadInt();
                        entry.difficulty = message.ReadInt();
                        entry.upgrades = message.ReadInt();
                        entry.data = from;
                        
                        lobbyEntries.Add(entry);
                    } catch (Exception e) {
                        Debug.LogError(e);
                    }
                }
            });

            return lobbyEntries.ToArray();
        }
    }
}