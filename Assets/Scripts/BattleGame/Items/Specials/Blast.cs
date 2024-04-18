using BattleGame.Controllers;
using BattleGame.Items.Bombs;
using BattleGame.Player;
using UnityEngine;

namespace BattleGame.Items.Specials {
    public class Blast : Indexable {
        public float size;
        public float timer;
        private BattleGameController gController;
        private PlayerController player;
        
        public void Setup(BattleGameController gController, PlayerController player, float size, float timer) {
            this.gController = gController;
            this.player = player;
            this.size = size;
            this.timer = timer;
        }

        private void Update() {
            timer -= Time.deltaTime;
            if (timer <= 0) {
                for (int i = 0; i < size; i++) {
                    var exp = gController.Instance<Explosion>("Battle/Explosions/PrefabExplosion",
                        transform.position + (transform.forward * (i + 1)) + new Vector3(0, 0.5f, 0));
                    exp.Setup(0.75f, 1);
                    exp.SetImmunity(player);
                }
                Destroy(gameObject);
            }
        }
    }
}