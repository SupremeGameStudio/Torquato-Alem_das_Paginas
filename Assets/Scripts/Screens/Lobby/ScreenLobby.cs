using System.Collections;
using BattleGame;
using UnityEngine;
using UnityEngine.UI;

namespace Screens.Lobby {
    public class ScreenLobby : ScreenBase {

        public Button btnPrevMap;
        public Button btnNextMap;
        public Button btnPrevHero;
        public Button btnNextHero;
        public Button btnLockHero;

        public Image imgMap;
        public CardPlayer[] cards;
        public Sprite[] sprites;
        public Sprite[] maps;
        
        public BattleGameConfig gameConfig;
        public int mapEnabled;
        public int heroEnabled;

        public override IEnumerator Open() {
            yield return base.Open();

            gameConfig = new BattleGameConfig(4);
            gameConfig.players[0].playerType = PlayerType.OWNER;
            for (int i = 1; i < gameConfig.players.Length; i++) {
                gameConfig.players[i].playerType = PlayerType.BOT;
            }
        }

        public void OnReadyClick() {
            ScreenManager.TransferData = gameConfig;
            StartCoroutine(ScreenManager.ToGame());
        }

        public void OnPrevMapClick() {
            gameConfig.mapId--;
            if (gameConfig.mapId < 0) {
                gameConfig.mapId = mapEnabled - 1;
            }

            UpdateMap();
        }

        public void OnNextMapClick() {
            gameConfig.mapId++;
            if (gameConfig.mapId >= mapEnabled) {
                gameConfig.mapId = 0;
            }

            UpdateMap();
        }

        public void OnCardButtonClick(int index) {
            var player = gameConfig.players[index];
            if (player.playerType == PlayerType.OFF) {
                player.playerType = PlayerType.LOCAL;
                
            } else if (player.playerType == PlayerType.LOCAL) {
                player.playerType = PlayerType.BOT;
                
            } else if (player.playerType == PlayerType.BOT) {
                player.playerType = PlayerType.OFF;
                
            }
            
            UpdateCards();
        }

        public void OnCardPrevHeroClick(int index) {
            var player = gameConfig.players[index];
            player.heroId--;
            if (player.heroId < 0) {
                player.heroId = heroEnabled - 1;
            }
            
            UpdateCards();
        }

        public void OnCardNextHeroClick(int index) {
            var player = gameConfig.players[index];
            player.heroId++;
            if (player.heroId >= heroEnabled) {
                player.heroId = 0;
            }

            UpdateCards();
        }

        private void UpdateCards() {
            for (int i = 0; i < 4; i++) {
                if (gameConfig.players[i].playerType == PlayerType.OWNER) {
                    cards[i].Setup(this, i, sprites[0], "LEADER", false);
                    
                } else if (gameConfig.players[i].playerType == PlayerType.BOT) {
                    cards[i].Setup(this, i, sprites[1], "BOT", false);
                    
                } else if (gameConfig.players[i].playerType == PlayerType.LOCAL) {
                    cards[i].Setup(this, i, sprites[2], "HUMAN", false);
                    
                } else if (gameConfig.players[i].playerType == PlayerType.OFF) {
                    cards[i].Setup(this, i, sprites[3], "OFF", false);
                    
                }
            }
        }

        private void UpdateMap() {
            imgMap.sprite = maps[gameConfig.mapId];
        }
    }
}
