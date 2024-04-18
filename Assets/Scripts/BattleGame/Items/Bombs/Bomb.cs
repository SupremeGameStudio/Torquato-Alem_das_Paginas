using System;
using BattleGame.Controllers;
using BattleGame.Player;
using Debugging;
using UnityEngine;

namespace BattleGame.Items.Bombs {
    public class Bomb : Indexable, IExplosable {
        private BattleGameController gController;
        private PlayerController player;
        private bool startCollision;
        
        public float size;
        public float timer;
        public Vector3 groundNormal;
        public Collider mainCollider;
        private Rigidbody rbody;

        public bool IsMoving {
            get {
                return Mathf.Abs(rbody.velocity.x) > 0.01f || Mathf.Abs(rbody.velocity.z) > 0.01f;
            }
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
                exp.Setup(1, size);
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

        public void Hold() {
            rbody.constraints = RigidbodyConstraints.FreezeAll;
            mainCollider.enabled = false;
        }
        
        public void Throw(Vector3 dir) {
            rbody.constraints = RigidbodyConstraints.FreezeRotation;
            mainCollider.enabled = true;
            rbody.AddForce((dir + new Vector3(0, 1.0f, 0)) * 5f, ForceMode.Impulse);
        }

        public void StopMove() {
            rbody.velocity = Vector3.zero;
        }

        public bool IsCollisionStarted(PlayerController player) {
            return player != this.player || startCollision;
        }
    }
}
