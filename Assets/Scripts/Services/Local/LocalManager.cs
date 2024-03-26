using System;
using System.Collections;
using System.Globalization;
using System.Net;
using System.Threading;
using Data;
using UnityEngine;

namespace Services.Local {
    public class LocalManager : MonoBehaviour, ServicesManager {
        public const int TcpPort = 8962;
        public const int UdpPort = 8963;

        private LobbyEntry[] lastEntries;
        
        public ILobby Lobby { get; protected set; }

        private ulong temporaryUnicId;

        public bool Init() {
            temporaryUnicId = (ulong)(DateTimeOffset.Now.ToUnixTimeMilliseconds() + Environment.MachineName).GetHashCode();
            return true;
        }

        public bool IsNoConnection() {
            return false;
        }

        public ulong GetId() {
            return temporaryUnicId;
        }

        public string GetName() {
            return ServicesManager.CurrentGameData.name;
        }

        public string GetCurrentLanguage() {
            CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
            if (currentCulture.Name.StartsWith("es")) {
                return "spanish";
            }
            if (currentCulture.Name.StartsWith("pt")) {
                return "portuguese";
            }
            if (currentCulture.Name.StartsWith("zh")) {
                return "chinese";
            }
            return "english";
        }

        public bool IsAchievement(Achievements achievement) {
            return false;
        }

        public void SetAchievement(Achievements achievement) {
            
        }

        public void SaveGameData(GameData gameData) {
            ServicesManager.UsualSaveGameData(gameData);
        }

        public GameData LoadGameData() {
            return ServicesManager.UsualLoadGameData();
        }

        public void ClearGameData() {
            ServicesManager.UsualClearGameData();
        }

        public DelayedTask<LeaderboardEntry[]> GetLeaderboard(string stageName, int count, TaskEvent<LeaderboardEntry[]> onLeaderboard) {
            var task = new DelayedTask<LeaderboardEntry[]>(onLeaderboard);
            StartCoroutine(FailLater(task));
            return task;
        }

        public DelayedTask<object> SetLeaderboard(string stageName, int score, TaskEvent<object> onLeaderboard) {
            var task = new DelayedTask<object>(onLeaderboard);
            StartCoroutine(FailLater(task));
            return task;
        }
        
        public DelayedTask<LobbyEntry[]> ListLobbies(TaskEvent<LobbyEntry[]> onLobbiesList) {
            var task = new DelayedTask<LobbyEntry[]>(onLobbiesList);
            ListLobbiesAsync(task);
            return task;
        }

        private async void ListLobbiesAsync(DelayedTask<LobbyEntry[]> task) {
            var list = await LocalUdpListener.SearchLobbies();
            lastEntries = list;
            
            if (list != null) {
                if (list.Length == 0) {
                    task.SetFailure();
                } else {
                    task.SetSuccess(list);
                }
            } else {
                task.SetFailure();
            }
        }

        public DelayedTask<ILobby> CreateLobby(TaskEvent<ILobby> onLobbyCreate) {
            var task = new DelayedTask<ILobby>(onLobbyCreate);
            if (Lobby != null) {
                StartCoroutine(FailLater(task));
                return task;
            }
            
            CreteLobbyAsync(task);
            return task;
        }

        private async void CreteLobbyAsync(DelayedTask<ILobby> task) {
            var server = await LocalTcpServer.Connect();
            if (server != null) {
                var created = new LocalLobbyServer(server);
                if (task.IsCanceled()) {
                    created.Close();
                    task.SetFailure();
                } else {
                    Lobby = created;
                    task.SetSuccess(Lobby);
                }
            } else {
                task.SetFailure();
            }
        }

        public DelayedTask<ILobby> ConnectToLobby(ulong unicId, TaskEvent<ILobby> onLobbyJoin) {
            var task = new DelayedTask<ILobby>(onLobbyJoin);
            
            IPAddress ipAddress = null;
            if (lastEntries != null) {
                foreach (var entry in lastEntries) {
                    if (entry.unicId == unicId) {
                        ipAddress = (IPAddress)entry.data;
                        break;
                    }
                }
            }
            
            if (Lobby != null || ipAddress == null) {
                StartCoroutine(FailLater(task));
                return task;
            }

            ConnectToLobbyAsync(ipAddress, task);
            return task;
        }

        private async void ConnectToLobbyAsync(IPAddress ipAddress, DelayedTask<ILobby> task) {
            var client = await LocalTcpClient.Connect(ipAddress);
            if (client != null) {
                var created = new LocalLobbyClient(client);
                if (task.IsCanceled()) {
                    created.Close();
                    task.SetFailure();
                } else {
                    Lobby = created;
                    task.SetSuccess(Lobby);
                }
            } else {
                task.SetFailure();
            }
        }
        
        public void CloseLobby() {
            if (Lobby != null) {
                Lobby.Close();
            }

            Lobby = null;
        }

        private IEnumerator FailLater<T>(DelayedTask<T> task) where T : class {
            yield return new WaitForSeconds(0.5f);
            task.SetFailure();
        }

        private void Update() {
            if (Lobby != null) {
                Lobby.Loop();
            }
        }
    }
}