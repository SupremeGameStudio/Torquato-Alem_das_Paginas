using Scripting.Torquato.Control;
using UnityEngine;

namespace Scripting.Torquato.Items {
    public class ItemBox : Item {
        public Rigidbody body;
        public float pushForce = 0.5f;
        
        public override void OnPlayerCollision(Player player, ControllerColliderHit collision) {
            if (collision.normal.y < 0.5f) {
                var t0 = transform.TransformDirection(Vector3.back);
                var t1 = transform.TransformDirection(Vector3.forward);
                var t2 = transform.TransformDirection(Vector3.left);
                var t3 = transform.TransformDirection(Vector3.right);
                var d0 = Vector3.SqrMagnitude(collision.moveDirection - t0);
                var d1 = Vector3.SqrMagnitude(collision.moveDirection - t1);
                var d2 = Vector3.SqrMagnitude(collision.moveDirection - t2);
                var d3 = Vector3.SqrMagnitude(collision.moveDirection - t3);
                if (d0 <= d1 && d0 <= d2 && d0 <= d3) body.velocity = t0 * pushForce;
                else if (d1 <= d0 && d1 <= d2 && d1 <= d3) body.velocity = t1 * pushForce;
                else if (d2 <= d1 && d2 <= d0 && d2 <= d3) body.velocity = t2 * pushForce;
                else if (d3 <= d1 && d3 <= d2 && d3 <= d0) body.velocity = t3 * pushForce;
            }
        }
    }
}