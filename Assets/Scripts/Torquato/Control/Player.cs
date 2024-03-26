using System;
using System.Collections.Generic;
using Debugging;
using Scripting.Torquato.Items;
using UnityEngine;

namespace Scripting.Torquato.Control {
    public class Player : MonoBehaviour {

        private const float epsilon = 0.00001f;

        public enum State {
            IDLE, MOVE, DASH, DAMAGE, ATTACK
        }
        
        [Header("Components")]
        public CharacterController charController;
        public Animator anim;
        public Transform model;
        
        [Header("Player Configuration")]
        public float speed;
        public float jumpSpeed;
        public float doubleJumpForce = 1f;
        public float gravity;
        public float gravityWater;

        [Header("Path")] public PathLineDistance currentPath;
        public List<PathLineDistance> closeLines = new List<PathLineDistance>();

        private GameController gameController;
        
        // States
        [Header("States")]
        public State state;
        public float stateTimer;
        public float stateSpeed;
        public float immuneTimer;
        
        // Jump
        public int jumpCount;
        public bool grounded;
        public int waterCollision;
        public bool IsSwim {
            get => waterCollision != 0;
        }
        public bool IsSubmerging {
            get => waterCollision != 0 && verticalSpeed > 0;
        }

        // Movement
        private Vector3 moveDir = Vector3.zero;
        private Vector3 faceMoveDir = Vector3.forward;
        private bool IsPlatform => currentPath.line.type is PathType.PLATFORM or PathType.PLATFORM_REVERSE;
        private float verticalSpeed;
        
        private Vector3 ctrlMoveDir = Vector3.zero;
        private Vector3 ctrlDashDir = Vector3.zero;
        private bool ctrlMove;
        private bool ctrlJump;
        private bool ctrlAttack;

        public void Setup(GameController gameController) {
            this.gameController = gameController;
        }

        public void Spring(float springForce) {
            verticalSpeed = Mathf.Max(verticalSpeed, jumpSpeed * springForce);
        }
        
        void Start() {
            
        }
        
        void Update() {
            // Physics
            Gravity();
            PathRotation();
            
            // Control States
            InputControl();
            StateControl();
            Move();
            
            // Rendering
            Animation();
        }

        private void StateControl() {
            immuneTimer -= Time.deltaTime;
            
            if (state == State.IDLE) {
                StateIdle();
                
            } else if (state == State.MOVE) {
                StateMove();
                
            } else if (state == State.ATTACK) {
                StateAttack();
                
            } else if (state == State.DASH) {
                StateDash();
                
            } else if (state == State.DAMAGE) {
                StateDamage();
                
            }
        }

        private bool StateActionControl() {
            if (ctrlJump) {
                if (IsSwim) {
                    if (!IsWaterAbove(Vector3.up)) { // Correto!
                        Spring(0.2f);
                    } else {
                        Spring(1.0f);
                    }

                } else if (grounded) {
                    Spring(1.0f);
                    
                } else if (jumpCount > 0) {
                    jumpCount -= 1;
                    Spring(doubleJumpForce);
                    
                }
            }
            
            if (ctrlAttack && !IsSwim) {
                state = State.ATTACK;
                stateTimer = 1f;
                return false;

            } 
            if (ctrlDashDir.sqrMagnitude > epsilon && grounded && !IsSwim && !IsWallInFront(ctrlDashDir)) {
                state = State.DASH;
                stateTimer = 0.2f;
                moveDir = ctrlDashDir;
                faceMoveDir = ctrlDashDir;
                return false;        
            }

            return true;
        }

        private void StateIdle() {
            if (!StateActionControl()) {
                return;
            }

            if (ctrlMoveDir.sqrMagnitude > epsilon) {
                state = State.MOVE;
                moveDir = ctrlMoveDir;
                faceMoveDir = ctrlMoveDir;
                    
            } else {
                moveDir = new Vector3();
                stateSpeed = 1f;
            }
        }

        private void StateMove() {
            if (!StateActionControl()) {
                return;
            }
                
            if (ctrlMoveDir.sqrMagnitude <= epsilon) {
                state = State.IDLE;
                moveDir = new Vector3();
                    
            } else {
                moveDir = ctrlMoveDir;
                faceMoveDir = ctrlMoveDir;
                stateSpeed = IsSwim ? 0.75f : 1f;
            }
        }

        private void StateAttack() {
            if (ctrlMoveDir.sqrMagnitude > epsilon) {
                moveDir = ctrlMoveDir;
                    
            } else {
                moveDir = new Vector3();
            }
            
            stateSpeed = 1f;
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0) {
                state = State.IDLE;
            }
        }

        private void StateDash() {
            stateSpeed = 6f;
                
            stateTimer -= Time.deltaTime;
            if (stateTimer <= 0.15f) {
                stateSpeed = stateTimer / 0.15f * 6f;
            }
            if (stateTimer <= 0 || IsWallInFront(moveDir)) {
                state = State.IDLE;
                moveDir = new Vector3();
            }
        }
        
        private void StateDamage() {
            stateTimer -= Time.deltaTime;
            stateSpeed = stateTimer / 0.25f * 2f;
            if (stateTimer <= 0) {
                state = State.IDLE;
            }
        }

        public bool IsAttacking {
            get => state == State.ATTACK;
        }

        public bool Damage(GameObject source, bool ignoreAttack = false) {
            if (!ignoreAttack && state == State.ATTACK) {
                return false;
            }

            if (immuneTimer <= 0) {
                Vector3 dir = (transform.position - source.transform.position).normalized;
                state = State.DAMAGE;
                stateTimer = 0.25f;
                moveDir = dir;
                immuneTimer = 3;
            }

            return true;
        }

        private void PathRotation() {
            gameController.FindCurrentPath(transform.position, closeLines);
            foreach (var line in closeLines) {
                var l = line.line;
                if (line == closeLines[0]) {
                    DebugDraw.DrawLine(l.pointA, l.pointB, Color.red);
                } else {
                    DebugDraw.DrawLine(l.pointA, l.pointB, Color.white);
                }
                DebugDraw.DrawSphere(Vector3.LerpUnclamped(l.pointA, l.pointB, line.time), 0.1f, Color.blue);
            }

            currentPath = closeLines[0];
            Quaternion rot = closeLines[0].line.rotation;
            transform.rotation = Quaternion.Slerp(transform.rotation, rot, 15 * Time.deltaTime);
        }

        private void Move() {
            float spd = speed * stateSpeed;
            Vector3 foward = moveDir;
            Vector3 center = new Vector3();
            if (foward.sqrMagnitude > epsilon && IsPlatform) {
                if (currentPath.distance > 0.1f) {
                    int side = SideOfLine(currentPath.line.pointA, currentPath.line.pointB, transform.position);
                    if (side == 1) {
                        center = (currentPath.line.pointB - currentPath.line.pointA);
                        center = new Vector3(center.z, 0, -center.x).normalized * Mathf.Min(currentPath.distance, spd * 2f * Time.deltaTime);
                    } else if (side == -1) {
                        center = (currentPath.line.pointB - currentPath.line.pointA);
                        center = new Vector3(-center.z, 0, center.x).normalized * Mathf.Min(currentPath.distance, spd * 2f * Time.deltaTime);
                    }
                }
            }
            foward = transform.TransformDirection(foward);
            charController.Move((foward * spd) * Time.deltaTime + new Vector3(0, verticalSpeed * Time.deltaTime, 0) + center);
        }
        
        private int SideOfLine(Vector3 A, Vector3 B, Vector3 P) {
            double crossProduct = (P.z - A.z) * (B.x - A.x) - (P.x - A.x) * (B.z - A.z);

            if (Math.Abs(crossProduct) < 0.01) {
                return 0;
            } else if (crossProduct > 0) {
                return 1;
            } else {
                return -1;
            }
        }

        private void Gravity() {
            if (charController.isGrounded) {
                grounded = true;
                jumpCount = 1;
                if (verticalSpeed <= -1) verticalSpeed = -1f;
            } else {
                grounded = false;
                if (IsSwim) {
                    jumpCount = 1;
                    verticalSpeed -= gravityWater * Time.deltaTime;
                    verticalSpeed = Mathf.Max(-gravityWater / 5f, verticalSpeed);
                } else {
                    verticalSpeed -= gravity * Time.deltaTime;
                    verticalSpeed = Mathf.Max(-gravity, verticalSpeed);
                }
            }
        }

        private void InputControl() {
            // Movemente Keyboard
            float horizontalInput = (Input.GetKey(KeyCode.D) ? 1 : 0) - (Input.GetKey(KeyCode.A) ? 1 : 0);
            float verticalInput = (Input.GetKey(KeyCode.W) ? 1 : 0) - (Input.GetKey(KeyCode.S) ? 1 : 0);
            
            // Platform Movemente
            ctrlMoveDir = new Vector3(horizontalInput, 0f, verticalInput);
            ctrlMove = ctrlMoveDir.sqrMagnitude > epsilon;
            
            if (IsPlatform) {
                ctrlMoveDir.z = 0;
            }
            ctrlMoveDir.Normalize();

            ctrlJump = Input.GetKeyDown(KeyCode.Space);
            ctrlAttack = Input.GetKeyDown(KeyCode.Q);
            if (Input.GetKeyDown(KeyCode.E)) {
                if (ctrlMoveDir.sqrMagnitude > epsilon) {
                    ctrlDashDir = ctrlMoveDir;
                } else {
                    ctrlDashDir = faceMoveDir;
                }
            } else {
                ctrlDashDir = new Vector3();
            }
        }
        
        private bool IsWallInFront(Vector3 dir) {
            RaycastHit hit;

            // Cast a ray forward from the player's position
            Vector3 rayOrigin = transform.position + charController.center;
            int layer = 1 << LayerMask.NameToLayer("Default");
            if (Physics.Raycast(rayOrigin, transform.TransformDirection(dir), out hit, charController.radius + 0.2f, layer, QueryTriggerInteraction.Ignore)) {
                return true; // Wall detected
            }

            return false; // No wall detected
        }
        
        private bool IsWaterAbove(Vector3 dir) {
            RaycastHit hit;

            // Cast a ray forward from the player's position
            Vector3 rayOrigin = transform.position + charController.center;
            int layer = 1 << LayerMask.NameToLayer("Water");
            if (Physics.Raycast(rayOrigin, dir, out hit, charController.height + 0.5f, layer, QueryTriggerInteraction.Collide)) {
                return true; // Water detected
            }

            return false; // No wall detected
        }
        
        private void Animation() {
            var dir = transform.TransformDirection(faceMoveDir);
            model.transform.rotation = 
                Quaternion.Lerp(model.transform.rotation, Quaternion.LookRotation(dir), 35 * Time.deltaTime);

            if (state == State.IDLE) {
                if (IsSwim && (grounded || !IsSubmerging)) PlayAnim("Float");
                else if (IsSwim) PlayAnim("Swim");
                else if (grounded) PlayAnim("Idle");
                else if (jumpCount <= 0) PlayAnim("DoubleJump");
                else PlayAnim("Jump", 0.5f);
                    
            } else if (state == State.MOVE) {
                if (IsSwim) PlayAnim("Swim");
                else if (grounded && IsWallInFront(moveDir)) PlayAnim("Push");
                else if (grounded) PlayAnim("Walk");
                else if (jumpCount <= 0) PlayAnim("DoubleJump");
                else PlayAnim("Jump", 0.5f);
                    
            } else if (state == State.DASH) {
                PlayAnim("Dash");
            } else if (state == State.DAMAGE) {
                PlayAnim("Damage");
            } else if (state == State.ATTACK) {
                PlayAnim("Attack");
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
        
        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Water") {
                waterCollision++;
                Debug.Log("emter");
            }
            
            if (other.gameObject.tag == "Hole") {
                gameController.OnPlayerFallOnHole();
            } else {
                Item item = other.gameObject.GetComponent<Item>();
                if (item != null) {
                    item.OnPlayerCollision(this, null);
                } else if (other.transform.parent != null) {
                    item = other.transform.parent.gameObject.GetComponent<Item>();
                    if (item != null) {
                        item.OnPlayerCollision(this, null);
                    } 
                }
            }
        }
        
        private void OnTriggerExit(Collider other) {
            if (other.gameObject.tag == "Water") {
                waterCollision--;
                Debug.Log("leave");
            }
        }
        
        private void OnControllerColliderHit(ControllerColliderHit hit) {
            Item item = hit.gameObject.GetComponent<Item>();
            if (item != null) {
                item.OnPlayerCollision(this, hit);
            }
        }
    }
}
