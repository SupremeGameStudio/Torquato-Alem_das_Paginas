using Scripting.Torquato.Control;
using UnityEngine;

namespace Scripting.Torquato.Items {
    public class ItemEnemy : Item {
        
        public override void OnPlayerCollision(Player player, ControllerColliderHit collision) {
            if (collision.normal.y > 0.5f) {
                player.Spring(1.0f);
                Destroy(gameObject);
            } else {
                if (!player.Damage(gameObject)) {
                    Destroy(gameObject);
                }
            }
        }
    }
}
