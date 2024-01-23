using UnityEngine;

namespace Scripting.Torquato.Control {
    public class Player : MonoBehaviour {

        [Header("ZBarrier")]
        public float zBarrierDistance = 3;
        public GameObject prefabZBarrier;
        private Transform zBarrier;
        private float zFoward;
        
        [Header("Components")]
        public CharacterController charController;
        public Animator anim;
        public Transform model;
        
        [Header("Player Configuration")]
        public float speed;
        public float jumpSpeed;
        public float gravity;

        // States
        private bool moving;
        private bool grounded;
        private float attackTimer;

        // Movement
        private Vector3 moveDir = Vector3.zero;
        private Vector3 prevMoveDir = Vector3.forward;
        private float verticalSpeed;
        
        void Start() {
            zBarrier = Instantiate(prefabZBarrier, transform.position - new Vector3(0, 0, zBarrierDistance),
                Quaternion.identity).transform;
        }
        
        void Update() {
            Gravity();
            
            float horizontalInput = Input.GetAxis("Horizontal");
            float verticalInput = Input.GetAxis("Vertical");
            moveDir = new Vector3(horizontalInput, 0f, verticalInput);
            moveDir.Normalize();
            
            moving = moveDir.sqrMagnitude > 0.001F;
            grounded = charController.isGrounded;
            
            if (moving) {
                prevMoveDir = moveDir;
            }

            if (grounded && Input.GetKeyDown(KeyCode.Space)) {
                verticalSpeed = jumpSpeed;
            }

            if (attackTimer <= 0 && Input.GetKeyDown(KeyCode.Q)) {
                attackTimer = 0.5f;
            }

            if (attackTimer > 0) {
                attackTimer -= Time.deltaTime;
            }

            Move();
            Animation();
            ZBarrierFollow();
        }

        private void Move() {
            charController.Move(moveDir * (speed * Time.deltaTime) + new Vector3(0, verticalSpeed * Time.deltaTime, 0));
        }

        private void Gravity() {
            if (charController.isGrounded) {
                verticalSpeed = -1f;
            } else {
                verticalSpeed -= gravity * Time.deltaTime;
            }
        }

        private void ZBarrierFollow() {
            var pos = transform.position;
            zFoward = Mathf.Max(zFoward, pos.z);
            zBarrier.transform.position = new Vector3(0, 0, zFoward - zBarrierDistance);
        }

        private void Animation() {
            model.transform.rotation = Quaternion.Lerp(model.transform.rotation, Quaternion.LookRotation(prevMoveDir), 10 * Time.deltaTime);
            if (attackTimer > 0) {
                PlayAnim("Attack");
            } else if (!grounded) {
                PlayAnim("Jump", 0.5f);
            } else if (moving) {
                PlayAnim("Walk");
            } else {
                PlayAnim("Idle");
            }

        }

        private void PlayAnim(string animName, float cross = 0) {
            if (!anim.GetCurrentAnimatorStateInfo(0).IsName(animName)) {
                if (cross <= 0) {
                    anim.Play(animName);
                } else {
                    anim.CrossFade(animName, cross);
                }
            }
        }
    }
}
