using UnityEngine;

namespace Scripting.Torquato.Control {
    public class Player : MonoBehaviour {

        public float zFoward;
        public float zBarrierDistance = 3;
        public Transform zBarrier;
        
        public CharacterController charController;
        public Animator anim;
        public Transform model;
        public float speed;
        public float jumpSpeed;
        public float gravity;

        private Vector3 prevMoveDir = Vector3.forward;
        private float verticalSpeed;
        
        
        void Start() {
        
        }
        
        void Update() {
            Gravity();
            
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            Vector3 movementDirection = new Vector3(horizontalInput, 0f, verticalInput);
            movementDirection.Normalize();

            if (movementDirection.sqrMagnitude > 0.001F) {
                prevMoveDir = movementDirection;
            }
            model.transform.rotation = Quaternion.Lerp(model.transform.rotation, Quaternion.LookRotation(prevMoveDir), 10 * Time.deltaTime);

            if (charController.isGrounded && Input.GetKeyDown(KeyCode.Space)) {
                verticalSpeed = jumpSpeed;
            }

            if (!charController.isGrounded) {
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Jump")) {
                    anim.CrossFade("Jump", 0.5f);
                }
            } else if (movementDirection.sqrMagnitude > 0.001f) {
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Walk")) {
                    anim.Play("Walk");
                }
            } else {
                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Idle")) {
                    anim.Play("Idle");
                }
            }

            charController.Move(movementDirection * (speed * Time.deltaTime) + new Vector3(0, verticalSpeed * Time.deltaTime, 0));
            var pos = transform.position;
            zFoward = Mathf.Max(zFoward, pos.z);
            zBarrier.transform.position = new Vector3(0, 0, zFoward - zBarrierDistance);
        }

        private void Gravity() {
            if (charController.isGrounded) {
                verticalSpeed = -1f;
            }
            else {
                verticalSpeed -= gravity * Time.deltaTime;
            }
        }
    }
}
