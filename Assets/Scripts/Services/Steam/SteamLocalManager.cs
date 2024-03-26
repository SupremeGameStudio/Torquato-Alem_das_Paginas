#if !UNITY_ANDROID || UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Data;
using Steamworks;
using Steamworks.Data;
using UnityEngine;

namespace Services.Steam {
    public class SteamLocalManager : MonoBehaviour, ServicesManager {
        
        public ILobby Lobby { get; protected set; }

        private string _name;
        
        void Start() {
            StartCoroutine(RunLater());
        }

        void Update() {
            SteamClient.RunCallbacks();
        }

        void OnDestroy() {
            Close();
        }

        public bool Init() {
            try {
                SteamClient.Init(480);
            }
            catch (Exception e) {
                Debug.LogError("[Steamworks.NET] Failed to connect " + e, this);
                return false;
            }
            Debug.Log("Steam Started");
            SteamUserStats.RequestCurrentStats();
            return true;
        }

        public bool IsNoConnection() {
            return false;
        }

        private void Close() {
            SteamClient.Shutdown();
        }

        IEnumerator RunLater() {
            yield return new WaitForSeconds(2);
            SetAchievement(Achievements.BETA);
        }

        public ulong GetId() {
            return SteamClient.SteamId;
        }

        public string GetName() {
            return _name ??= SteamClient.Name;
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
            foreach(var ach in SteamUserStats.Achievements) {
                if (ach.Name == achievement.ToString()) {
                    return ach.State;
                }
            }
            return false;
        }

        public void SetAchievement(Achievements achievement) {
            foreach(var ach in SteamUserStats.Achievements) {
                if (ach.Name == achievement.ToString()) {
                    ach.Trigger();
                    return;
                }
            }
        }

        public void SaveGameData(GameData gameData) {
            try {
                gameData.name = GetName();
                
                using (var sw = new StreamWriter(File.Create(Application.persistentDataPath + "/gamedata.save"))) {
                    byte[] bytesToEncode = Encoding.UTF8.GetBytes(JsonUtility.ToJson(gameData, true));
                    string encripted = Convert.ToBase64String(bytesToEncode);
                    sw.Write(encripted);
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }
        }

        public GameData LoadGameData() {
            GameData gameData;
            try {
                string path = Application.persistentDataPath + "/gamedata.save";
                if (File.Exists(path)) {
                    using (var sr = new StreamReader(path)) {
                        string ecripted = sr.ReadToEnd();
                        try {
                            byte[] bytesToEncode = Convert.FromBase64String(ecripted);
                            ecripted = Encoding.UTF8.GetString(bytesToEncode);
                        } catch {
                        }
                        gameData = JsonUtility.FromJson<GameData>(ecripted);
                    }
                } else {
                    gameData = new GameData(true);
                }
            } catch (Exception e) {
                Debug.LogError(e);
                gameData = new GameData(true);
            }
            gameData.Upgrade();
            return gameData;
        }

        public void ClearGameData() {
            try {
                string path = Application.persistentDataPath + "/gamedata.save";
                if (File.Exists(path)) {
                    File.Delete(path);
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }
        }

        public DelayedTask<LeaderboardEntry[]> GetLeaderboard(string stageName, int count, TaskEvent<LeaderboardEntry[]> onLeaderboard) {
            var task = new DelayedTask<LeaderboardEntry[]>(onLeaderboard);
            CreateLeaderboard(stageName, count, task);
            
            return task;
        }

        private async void CreateLeaderboard(string stageName, int count, DelayedTask<LeaderboardEntry[]> task) {
            var leaderboardAsync = await SteamUserStats.FindOrCreateLeaderboardAsync("Leaderboard_" + stageName,
                LeaderboardSort.Descending, LeaderboardDisplay.Numeric);
            if (leaderboardAsync.HasValue) {
                var leaderboard = leaderboardAsync.Value;
                var list = new List<LeaderboardEntry>();

                bool hasUser = false;
                var global = await leaderboard.GetScoresAsync(count - 1);
                if (global != null) {
                    foreach (var entry in global) {
                        if (entry.User.Name == GetName()) {
                            hasUser = true;
                        }

                        list.Add(new LeaderboardEntry(entry.User.Name, entry.Score,
                            entry.Score < 1 ? 100 : entry.GlobalRank));
                    }
                }

                if (!hasUser) {
                    var current = await leaderboard.GetScoresAroundUserAsync(0, 0);
                    var user = current == null || current.Length == 0 ? 
                        new LeaderboardEntry(GetName(), 0, 100) : 
                        new LeaderboardEntry(current[0].User.Name, current[0].Score, current[0].Score < 1 ? 100 : current[0].GlobalRank);

                    if (list.Count == count) {
                        list.RemoveAt(list.Count - 1);
                    }
                    list.Add(user);
                }
                
                for (int i = list.Count; i < count; i++) {
                    list.Add(new LeaderboardEntry("", 0, 100));
                }
                task.SetSuccess(list.ToArray());

            } else {
                task.SetFailure();
            }
        }

        public DelayedTask<object> SetLeaderboard(string stageName, int score, TaskEvent<object> onLeaderboard) {
            var task = new DelayedTask<object>(onLeaderboard);
            SetLeaderboard(stageName, score, task);
            
            return task;
        }

        private async void SetLeaderboard(string stageName, int score, DelayedTask<object> task) {
            var leaderboardAsync = await SteamUserStats.FindOrCreateLeaderboardAsync("Leaderboard_" + stageName,
                LeaderboardSort.Descending, LeaderboardDisplay.Numeric);
            if (leaderboardAsync.HasValue) {
                var leaderboard = leaderboardAsync.Value;

                var result = await leaderboard.SubmitScoreAsync(score);
                if (result.HasValue) {
                    task.SetSuccess(true);
                } else {
                    task.SetFailure();
                }
                

            } else {
                task.SetFailure();
            }
        }

        public DelayedTask<LobbyEntry[]> ListLobbies(TaskEvent<LobbyEntry[]> onLobbiesList) {
            var task = new DelayedTask<LobbyEntry[]>(onLobbiesList);
            ListLobbiesAsync(task);
            return task;
        }

        private async void ListLobbiesAsync(DelayedTask<LobbyEntry[]> task) {
            var list = await SteamMatchmaking.LobbyList.RequestAsync();
            if (list != null) {
                List<LobbyEntry> entries = new List<LobbyEntry>();
                foreach (var lobby in list) {
                    LobbyEntry entry = new LobbyEntry();
                    string state = lobby.GetData("state");
                    if (state.StartsWith("private")) {
                        if (state.IndexOf("[" + SteamClient.SteamId + "]", StringComparison.Ordinal) != -1) {
                            entry.reconnect = true;
                        } else {
                            continue;
                        }
                    }
                    
                    entry.unicId = lobby.Id;
                    entry.data = lobby;
                    entry.ownerName = lobby.GetData("ownerName");
                    entry.mapName = lobby.GetData("mapName");
                    entry.players = lobby.MemberCount;
                    int.TryParse(lobby.GetData("maxPlayers"), out entry.maxPlayers);
                    int.TryParse(lobby.GetData("mode"), out entry.mode);
                    int.TryParse(lobby.GetData("difficulty"), out entry.difficulty);
                    int.TryParse(lobby.GetData("upgrades"), out entry.upgrades);
                    entries.Add(entry);
                }
                entries.Sort((a, b) => 
                    (a.reconnect ? 0 : a.unicId).CompareTo((b.reconnect ? 0 : b.unicId))
                );
                if (entries.Count == 0) {
                    task.SetFailure();
                } else {
                    task.SetSuccess(entries.ToArray());
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
            var lobby = await SteamMatchmaking.CreateLobbyAsync();
            if (lobby.HasValue) {
                var created = new SteamLobby(lobby.Value);
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
            if (Lobby != null) {
                StartCoroutine(FailLater(task));
                return task;
            }
            
            ConnectToLobbyAsync(unicId, task);
            return task;
        }

        private async void ConnectToLobbyAsync(ulong lobbyId, DelayedTask<ILobby> task) {
            var lobby = await SteamMatchmaking.JoinLobbyAsync(lobbyId);
            if (lobby.HasValue) {
                var created = new SteamLobby(lobby.Value);
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
    }
}
#endif