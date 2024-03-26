using System;
using UnityEngine;
using UnityEngine.iOS;

namespace BattleGame.Items.Upgrades {
    public class Upgrade : Indexable {
        public enum Type {
            Fire, Bomb, Speed
        }
        public float duration;
        public Type type;
        private float lifetime;

        private void Update() {
            lifetime += Time.deltaTime;
            if (lifetime > duration) {
                Destroy(gameObject);
            }
        }

        public void Collect() {
            Destroy(gameObject);
        }
    }
}