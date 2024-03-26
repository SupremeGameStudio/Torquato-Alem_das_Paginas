using Scripting.Torquato.Control;
using UnityEngine;

namespace Scripting.Torquato.Items {
    public class ItemCollect : Item {
        public override void OnPlayerCollision(Player player, ControllerColliderHit collision) {
            Destroy(gameObject);
        }
    }
}
