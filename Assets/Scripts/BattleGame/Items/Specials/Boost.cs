using BattleGame.Controllers;
using BattleGame.Items.Bombs;
using BattleGame.Player;
using UnityEngine;

namespace BattleGame.Items.Specials {
    public class Boost : Indexable  {
        private BattleGameController gController;
        private PlayerController player;
        private Vector3 prevExplosionPos;
        
        public void Setup(BattleGameController gController, PlayerController player) {
            this.gController = gController;
            this.player = player;
            prevExplosionPos = this.player.transform.position;
        }

        private void Update() {
            if (player == null || player.attackState != PlayerController.AttackState.SPECIAL) {
                Destroy(gameObject);
                
            } else {
                transform.position = player.transform.position;
                while (Vector3.Distance(prevExplosionPos, transform.position) > 0.75f) {
                    var exp = gController.Instance<Explosion>("Battle/Explosions/PrefabExplosion",
                        prevExplosionPos + new Vector3(0, 0.5f, 0));
                    exp.Setup(1f, 0.5f);
                    exp.SetImmunity(player);
                    prevExplosionPos = Vector3.MoveTowards(prevExplosionPos, transform.position, 0.75f);
                }
            }
        }
    }
}