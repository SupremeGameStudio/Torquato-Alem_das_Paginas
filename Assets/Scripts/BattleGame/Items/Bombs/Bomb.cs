using System;
using BattleGame.Controllers;
using BattleGame.Player;
using Debugging;
using UnityEngine;

namespace BattleGame.Items.Bombs {
    public class Bomb : Item, IExplosable {
        private BattleGameController gController;
        private PlayerController player;
        private bool startCollision;
        
        public float size;
        public float timer;
        public Vector3 groundNormal;
        public Collider boxCollider;
        private Rigidbody rbody;

        public bool IsMoving {
            get => rbody.velocity.sqrMagnitude > 0.01f;
        }
        
        public void Setup(BattleGameController gController, PlayerController player) {
            rbody = GetComponent<Rigidbody>();
            
            this.gController = gController;
            this.player = player;
            foreach (var coll in GetComponentsInChildren<Collider>()) {
                Physics.IgnoreCollision(player.charController, coll, true);
            }
        }

        private void Update() {
            if (!startCollision) {
                var p1 = transform.position;
                var p2 = player.transform.position;
                if (Mathf.Abs(p1.x - p2.x) > 1f || Mathf.Abs(p1.z - p2.z) > 1f) {
                    startCollision = true;
                    foreach (var coll in GetComponentsInChildren<Collider>()) {
                        Physics.IgnoreCollision(player.charController, coll, false);
                    }
                }
            }

            timer -= Time.deltaTime;
            if (timer <= 0) {
                var exp = gController.Instance<Explosion>("Battle/Explosions/PrefabExplosion", transform.position + new Vector3(0, 0.5f, 0));
                exp.duration = 1;
                exp.size = size;
                
                Destroy(gameObject);
            }

            if (Input.GetKeyDown(KeyCode.H)) {
                Kick(new Vector3(0, 0, 1));
            }
        }

        private void FixedUpdate() {
            
        }

        public void Explode(Explosion explosion) {
            timer = 0;
        }

        public void Kick(Vector3 dir) {
            rbody.AddForce(dir * 8f, ForceMode.Impulse);
        }

        public void StopMove() {
            rbody.velocity = Vector3.zero;
        }

        public bool IsCollisionStarted(PlayerController player) {
            return player != this.player || startCollision;
        }
    }
}
