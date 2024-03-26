using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using Data;
using UnityEngine;

namespace Services.NoConnection {
    public class NoConnectionManager : MonoBehaviour, ServicesManager {
        public ILobby Lobby { get; protected set; }

        public bool Init() {
            return true;
        }

        public bool IsNoConnection() {
            return true;
        }

        public ulong GetId() {
            return 0;
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
            StartCoroutine(RunLater(task));
            return task;
        }

        public DelayedTask<object> SetLeaderboard(string stageName, int score, TaskEvent<object> onLeaderboard) {
            var task = new DelayedTask<object>(onLeaderboard);
            StartCoroutine(RunLater(task));
            return task;
        }

        public DelayedTask<LobbyEntry[]> ListLobbies(TaskEvent<LobbyEntry[]> onLobbiesList) {
            var task = new DelayedTask<LobbyEntry[]>(onLobbiesList);
            StartCoroutine(RunLater(task));
            return task;
        }

        public DelayedTask<ILobby> CreateLobby(TaskEvent<ILobby> onLobbyCreate) {
            var task = new DelayedTask<ILobby>(onLobbyCreate);
            StartCoroutine(RunLater(task));
            return task;
        }

        public DelayedTask<ILobby> ConnectToLobby(ulong unicId, TaskEvent<ILobby> onLobbyJoin) {
            var task = new DelayedTask<ILobby>(onLobbyJoin);
            StartCoroutine(RunLater(task));
            return task;
        }

        public void CloseLobby() {
            
        }

        private IEnumerator RunLater<T>(DelayedTask<T> task) where T : class {
            yield return new WaitForSeconds(0.5f);
            task.SetFailure();
        }
    }
}