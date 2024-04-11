namespace BattleGame {
    public class BattleGameConfig {

        public int mapId;
        public readonly BattlePlayerData[] players;

        public BattleGameConfig(int size) {
            this.players = new BattlePlayerData[size];
            for (int i = 0; i < size; i++) {
                players[i] = new BattlePlayerData(i);
            }
        }
    }
}