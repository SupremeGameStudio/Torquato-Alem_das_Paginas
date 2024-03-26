using System;
using Debugging;
using Helper;
using UnityEngine;
using UnityEngine.PlayerLoop;

namespace BattleGame.Items.Bombs {
    public class Explosion : Indexable {
        
        public float size;
        public float duration;
        private float maxDuration;

        private void Start() {
            maxDuration = duration;
        }

        void FixedUpdate() {
            duration -= Time.deltaTime;
            if (duration > 0) {
                float currentSize = Mathf.Lerp(1, size, 1 - Interpolate.circleIn(duration / maxDuration)) * 2f;
                transform.localScale = Vector3.one * currentSize;
                DebugDraw.DrawSphere(transform.position, currentSize * 0.5f, Color.red);
            } else {
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision other) {
            var explosable = other.gameObject.GetComponent<IExplosable>();
            if (explosable != null) {
                explosable.Explode(this);
            }
        }

        private void OnTriggerEnter(Collider other) {
            var explosable = other.GetComponent<IExplosable>();
            if (explosable != null) {
                explosable.Explode(this);
            }
        }
    }
}