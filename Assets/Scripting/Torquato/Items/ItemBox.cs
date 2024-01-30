using Scripting.Torquato.Control;
using UnityEngine;

namespace Scripting.Torquato.Items {
    public class ItemBox : Item {
        public Rigidbody body;
        public float pushForce = 0.5f;
        
        public override void OnPlayerCollision(Player player, ControllerColliderHit collision) {
            if (collision.normal.y < 0.5f) {
                body.velocity = collision.moveDirection * pushForce;
            }
        }
    }
}