using System;
using System.IO;
using System.Text;
using Data;
using Services.Local;
using Services.NoConnection;
using Services.Steam;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Services {
    public interface ServicesManager {

        private static ServicesManager _current;
        private static GameData _gameData;
        public static ServicesManager Current {
            get {
                if (_current == null) {
#if UNITY_ANDROID && !UNITY_EDITOR
                    var obj = new GameObject("SteamLocalManager");
                    var steam = obj.AddComponent<NoConnectionManager>();
                    if (steam.Init()) {
                        Object.DontDestroyOnLoad(steam);
                        _current = steam;
                    }
#else
                    var obj = new GameObject("SteamLocalManager");
                    var steam = obj.AddComponent<SteamLocalManager>();
                    if (steam.Init()) {
                        Object.DontDestroyOnLoad(steam);
                        _current = steam;
                    } else {
                        Object.Destroy(steam);

                        var noConnection = obj.AddComponent<LocalManager>();
                        Object.DontDestroyOnLoad(noConnection);
                        _current = noConnection;
                    }
#endif
                }

                return _current;
            }
        }

        public static GameData CurrentGameData {
            get {
                if (_gameData == null) {
                    _gameData = Current.LoadGameData();
                }

                return _gameData;
            }
        }
        
        public ILobby Lobby { get; }

        public bool Init();
        public bool IsNoConnection();

        public ulong GetId();
        public string GetName();
        public string GetCurrentLanguage();
        
        public bool IsAchievement(Achievements achievement);
        public void SetAchievement(Achievements achievement);

        public void SaveGameData(GameData gameData);
        public GameData LoadGameData();
        public void ClearGameData();

        public DelayedTask<LeaderboardEntry[]> GetLeaderboard(string stageName, int count, TaskEvent<LeaderboardEntry[]> onLeaderboard);
        public DelayedTask<object> SetLeaderboard(string stageName, int score, TaskEvent<object> onLeaderboard);

        public DelayedTask<LobbyEntry[]> ListLobbies(TaskEvent<LobbyEntry[]> onLobbiesList);

        public DelayedTask<ILobby> CreateLobby(TaskEvent<ILobby> onLobbyCreate);
        public DelayedTask<ILobby> ConnectToLobby(ulong unicId, TaskEvent<ILobby> onLobbyJoin);
        public void CloseLobby();
        
        protected static void UsualSaveGameData(GameData gameData) {
            try {
                using (var sw = new StreamWriter(File.Create(Application.persistentDataPath + "/gamedata.save"))) {
                    byte[] bytesToEncode = Encoding.UTF8.GetBytes(JsonUtility.ToJson(gameData, true));
                    string encripted = Convert.ToBase64String(bytesToEncode);
                    sw.Write(encripted);
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }
        }

        protected static GameData UsualLoadGameData() {
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

        protected static void UsualClearGameData() {
            try {
                string path = Application.persistentDataPath + "/gamedata.save";
                if (File.Exists(path)) {
                    File.Delete(path);
                }
            } catch (Exception e) {
                Debug.LogError(e);
            }
        }
    }

    public class LeaderboardEntry {
        public string name;
        public int score;
        public int pos;

        public LeaderboardEntry(string name, int score, int pos) {
            this.name = name;
            this.score = score;
            this.pos = pos;
        }
    }

    public class LobbyEntry {
        public ulong unicId;
        public object data;
        public string ownerName;
        public string mapName;
        public int maxPlayers;
        public int players;
        public int mode;
        public int difficulty;
        public int upgrades;
        public bool reconnect;
    }
    
    public delegate void TaskEvent<T>(T result, bool sucess) where T : class;

    public class DelayedTask<T> where T : class {

        private TaskEvent<T> onTaskCompleted;
        private bool canceled;

        public DelayedTask(TaskEvent<T> onTaskCompleted) {
            this.onTaskCompleted = onTaskCompleted;
        }

        public void SetFailure() {
            Cancel();
            onTaskCompleted?.Invoke(null, false);
        }

        public void SetSuccess(T result) {
            if (!canceled) {
                onTaskCompleted?.Invoke(result, true);
            }
        }

        public void Cancel() {
            canceled = true;
        }

        public bool IsCanceled() {
            return canceled;
        }
    }
}