using System;
using System.Numerics;
using BattleGame.Controllers;
using BattleGame.Items.Bombs;
using BattleGame.Items.Upgrades;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BattleGame.Items.Blocks {
    public class Block : Item, IExplosable {
        private BattleGameController gController;
        
        public float upgradeChance = 1f;
        public Upgrade.Type[] types;

        private void Start() {
            if (gController == null) {
                gController = FindObjectOfType<BattleGameController>();
            }
        }

        public void Explode(Explosion explosion) {
            Debug.Log("Here");
            if (Random.value <= upgradeChance && types.Length > 0) {
                Debug.Log("a");
                var type = types[Random.Range(0, types.Length)].ToString();
                var exp = gController.Instance<Upgrade>("Battle/Upgrades/PrefabUpgrade" + type, transform.position);
            }
            
            Destroy(gameObject);
        }
    }
}