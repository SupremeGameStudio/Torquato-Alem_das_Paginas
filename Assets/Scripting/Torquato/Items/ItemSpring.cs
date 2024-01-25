using Scripting.Torquato.Control;
using UnityEngine;

namespace Scripting.Torquato.Items {
    public class ItemSpring : Item {
        public float extraForce = 1.5f;
        
        public override void OnPlayerCollision(Player player, ControllerColliderHit collision) {
            Debug.Log(collision.normal);
            if (collision.normal.y > 0.5f) {
                player.Spring(extraForce);
            }
        }
    }
}