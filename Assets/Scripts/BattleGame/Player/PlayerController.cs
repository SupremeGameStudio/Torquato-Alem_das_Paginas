using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using BattleGame.Controllers;
using BattleGame.Items;
using BattleGame.Items.Bombs;
using BattleGame.Items.Upgrades;
using UnityEngine;
using UnityEngine.Serialization;

namespace BattleGame.Player {
    public class PlayerController : Indexable, IExplosable {
        private const float epsilon = 0.00001f;
    
        public enum MoveState {
            IDLE, MOVE, DASH, RECOIL, STUN, DEAD
        }
    
        public enum AttackState {
            POSE, ATTACK, KICK, PICK, THROW, SPECIAL
        }

        private BattleGameController gController;
        
        [Header("Components")]
        public CharacterController charController;
        public Animator anim;
        public Transform model;
        
        [Header("Player Base Configuration")]
        public float speed;
        public float jumpForce;
        public float doubleJumpForce = 1f;
        public float gravity;
        public bool grounded;
        
        [Header("Input Control")]
        private Vector3 ctrlMoveDir = Vector3.zero;
        private bool ctrlJump;
        private bool ctrlPrimary;
        private bool ctrlSecondary;
        private bool ctrlSpecial;

        [Header("Movemente Control")]
        private float verSpeed;
        private Vector2 horSpeed;
        private Vector3 moveDir = Vector3.zero;
        private Vector3 faceMoveDir = Vector3.forward;

        [Header("States")]
        public MoveState moveState;
        private MoveState prevMoveState;
        public float stateTimer;
        public float stateDuration;
        public float speedAmplify = 1;
        private Vector3 stateDir = Vector3.zero;
    
        [Header("Attack")]
        public AttackState attackState;
        public Transform bombPosition;
        private AttackState prevAttackState;
        public float attackTimer;
        public float prevAttackTimer;
        private bool IsHolding => bombToHold != null;
        private Bomb bombToKick;
        private Bomb bombToHold;
        private Vector3 kickDir;

        [Header("Upgrades")]
        public int upgradeFire;
        public int upgradeBomb;
        public int upgradeSpeed;
        private List<Bomb> bombs = new List<Bomb>();
        
        private bool IsSimpleState => moveState is MoveState.IDLE or MoveState.MOVE && attackState == AttackState.POSE;

        public int MaxBombs {
            get {
                return upgradeBomb + 1;
            }
        }

        public int BombCount {
            get {
                int count = 0;
                for (int i = 0; i < bombs.Count; i++) {
                    if (bombs[i] == null) {
                        bombs.RemoveAt(i--);
                    } else {
                        count++;
                    }
                }

                return count;
            }
        }
        
        public float BombSize {
            get {
                return upgradeFire * 0.25f + 2f;
            }
        }
        
        public float Speed {
            get {
                return (upgradeSpeed / 20f + 1.0f) * speed;
            }
        }

        public void Setup(BattleGameController gController) {
            this.gController = gController;
        }
        
        void Start() {
            
        }
        
        void Update() {
            // Physics
            Gravity();
            
            // Control States
            InputControl();
            StateControl();
            Move();
            
            // Rendering
            Animation();
        }

        private void InputControl() {
            // Movemente Keyboard
            float horizontalInput = (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
            float verticalInput = (Input.GetKey(KeyCode.UpArrow) ? 1 : 0) - (Input.GetKey(KeyCode.DownArrow) ? 1 : 0);
        
            // Platform Movemente
            ctrlMoveDir = new Vector3(horizontalInput, 0f, verticalInput);
            ctrlMoveDir.Normalize();
            ctrlMoveDir = transform.TransformDirection(ctrlMoveDir);

            ctrlJump = Input.GetKey(KeyCode.S);
            ctrlPrimary = Input.GetKey(KeyCode.A);
            ctrlSecondary = Input.GetKey(KeyCode.X);
            ctrlSpecial = Input.GetKey(KeyCode.Z);
        }

        private void StateControl() {
            JumpStateControl();

            MoveStateControl();

            AttackStateControl();
        }

        private void JumpStateControl() {
            if (IsSimpleState) {
                if (ctrlJump && grounded) {
                    Spring(1.0f);
                }
            }
        }

        private void MoveStateControl() {
            if (prevMoveState != moveState) {
                prevMoveState = moveState;
                stateTimer = 0;
            } else {
                stateTimer += Time.deltaTime;
            }
            
            if (moveState == MoveState.IDLE) {
                StateIdle();
                
            } else if (moveState == MoveState.MOVE) {
                StateMove();
                
            } else if (moveState == MoveState.DASH) {
                StateDash();
                
            } else if (moveState == MoveState.RECOIL) {
                StateRecoil();
                
            } else if (moveState == MoveState.STUN) {
                StateStun();
                
            } else if (moveState == MoveState.DEAD) {
                StateDead();
                
            }
        }

        private void AttackStateControl() {
            if (prevAttackState != attackState) {
                prevAttackState = attackState;
                prevAttackTimer = 0;
                attackTimer = 0;
                speedAmplify = 1;
            } else {
                prevAttackTimer = attackTimer;
                attackTimer += Time.deltaTime;
            }
            
            if (attackState == AttackState.POSE) {
                StatePose();
                
            } else if (attackState == AttackState.ATTACK) {
                StateAttack();
                
            } else if (attackState == AttackState.KICK) {
                StateKick();
                
            } else if (attackState == AttackState.PICK) {
                StatePick();
                
            } else if (attackState == AttackState.THROW) {
                StateThrow();
                
            } else if (attackState == AttackState.SPECIAL) {
                StateSpecial();
                
            }
        }
        
        private void StateIdle() {
            if (ctrlMoveDir.sqrMagnitude <= epsilon || speedAmplify <= 0) {
                moveDir = Vector3.zero;
                
            } else {
                moveState = MoveState.MOVE;
            }
        }

        private void StateMove() {
            if (ctrlMoveDir.sqrMagnitude > epsilon && speedAmplify > 0) {
                moveDir = ctrlMoveDir;
                faceMoveDir = ctrlMoveDir;
                
            } else {
                moveState = MoveState.IDLE;
            }
        }

        private void StateDash() {
            if (stateTimer < stateDuration) {
                moveDir = stateDir;
                
            } else {
                moveState = (ctrlMoveDir.sqrMagnitude > epsilon && speedAmplify > 0) ? MoveState.MOVE : MoveState.IDLE;
            }
        }

        private void StateRecoil() {
            if (stateTimer < stateDuration) {
                moveDir = stateDir;
                
            } else {
                moveState = (ctrlMoveDir.sqrMagnitude > epsilon && speedAmplify > 0) ? MoveState.MOVE : MoveState.IDLE;
            }
        }

        private void StateStun() {
            if (stateTimer < stateDuration) {
                moveDir = Vector3.zero;
                
            } else {
                moveState = (ctrlMoveDir.sqrMagnitude > epsilon && speedAmplify > 0) ? MoveState.MOVE : MoveState.IDLE;
            }
        }

        private void StateDead() {
            moveDir = Vector3.zero;
        }

        private void StatePose() {
            if (!IsSimpleState) return;

            if (IsHolding) {
                if (ctrlPrimary || ctrlSpecial) {
                    Vector3 dir = faceMoveDir.normalized;
                    dir = RoundYAngle(dir);
                    
                    attackState = AttackState.THROW;
                    kickDir = dir;
                    
                } else if (ctrlSecondary) {
                    
                }
            } else if (ctrlPrimary) {
                if (CheckAttackEnabled()) {
                    attackState = AttackState.ATTACK;
                }
            } else if (ctrlSecondary) {
                Bomb bomb = CheckBombFoward(faceMoveDir);
                if (bomb) {
                    Vector3 dir = faceMoveDir.normalized;
                    dir = RoundYAngle(dir);

                    attackState = AttackState.KICK;
                    bombToKick = bomb;
                    kickDir = dir;
                }
            } else if (ctrlSpecial) {
                Bomb bomb = CheckBombFoward(faceMoveDir);
                if (bomb) {
                    attackState = AttackState.PICK;
                    bombToHold = bomb;
                    bomb.Hold();
                }
            }
        }

        private void StateAttack() {
            if (ctrlPrimary && attackTimer <= 1.0f) {
                if (attackTimer == 0) {
                    Vector3 pos = transform.position;
                    var bomb = gController.Instance<Bomb>("Battle/Bombs/PrefabBomb", pos);
                    bomb.Setup(gController, this);
                    bomb.size = BombSize;
                    bomb.timer = 5;
                    bombs.Add(bomb);
                }
                
            } else {
                attackState = AttackState.POSE;
            }
        }

        private void StateKick() {
            if (prevAttackTimer <= 0.5 && attackTimer > 0.5f) {
                if (bombToKick != null && !bombToKick.IsMoving) {
                    bombToKick.Kick(kickDir);
                }
            }
            if (attackTimer <= 0.66f) {
                speedAmplify = 0;
                
            } else {
                attackState = AttackState.POSE;
            }
        }

        private void StatePick() {
            if (attackTimer <= 0.5f) {
                speedAmplify = 0;
                if (bombToHold != null) {
                    bombToHold.transform.position = Vector3.Lerp(bombToHold.transform.position, bombPosition.position, 0.5f);
                }
                
            } else {
                if (bombToHold != null) {
                    bombToHold.transform.SetParent(bombPosition, false);
                    bombToHold.transform.localPosition = Vector3.zero;
                }
                attackState = AttackState.POSE;
            }
        }

        private void StateThrow() {
            if (prevAttackTimer <= 0 && attackTimer > 0) {
                if (bombToHold != null) {
                    bombToHold.transform.SetParent(null, true);
                    bombToHold.Throw(kickDir);
                }

                bombToHold = null;
            }

            if (attackTimer <= 0.5f) {
                speedAmplify = 0;
                
            } else {
                attackState = AttackState.POSE;
            }
        }

        private void StateSpecial() {
            
        }

        
        private void Gravity() {
            grounded = charController.isGrounded;
            
            if (!grounded) {
                verSpeed -= gravity * Time.deltaTime;
                verSpeed = Mathf.Max(-gravity, verSpeed);
            }
        }

        private void Move() {
            float spd = Speed * speedAmplify;
            charController.Move((moveDir * spd + new Vector3(0, verSpeed, 0)) * Time.deltaTime);
        }

        private void Animation() {
            var dir = faceMoveDir;
            model.transform.rotation = 
                Quaternion.Lerp(model.transform.rotation, Quaternion.LookRotation(dir), 35 * Time.deltaTime);
            
            if (moveState == MoveState.DEAD) PlayAnim("Death");
            else if (attackState == AttackState.KICK) PlayAnim("Kick");
            else if (attackState == AttackState.PICK) PlayAnim("Pick");
            else if (attackState == AttackState.THROW) PlayAnim("Throw");
            else if (moveState == MoveState.IDLE) PlayAnim(grounded ? "Idle" : "Jump");
            else if (moveState == MoveState.MOVE) PlayAnim(grounded ? "Walk" : "Jump");
            else if (moveState == MoveState.DASH) PlayAnim("Dash");
            else if (moveState == MoveState.STUN) PlayAnim("Stun");
            else if (moveState == MoveState.RECOIL) PlayAnim("Damage");

            if (attackState != AttackState.PICK && attackState != AttackState.THROW) {
                anim.SetLayerWeight(1, IsHolding ? 1 : 0);
            } else {
                anim.SetLayerWeight(1, 0);
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

        public void Damage() {
            attackState = AttackState.POSE;
            moveState = MoveState.DEAD;
        }

        public void Stun() {
            attackState = AttackState.POSE;
            moveState = MoveState.STUN;
            stateDuration = 5.0f;
        }
        
        public void PushTo(float duration, Vector3 dir, float speed) {
            attackState = AttackState.POSE;
            moveState = MoveState.RECOIL;
            stateDir = dir;
            stateDuration = duration;
            speedAmplify = speed / this.Speed;
        }

        public void Spring(float springForce) {
            verSpeed = Mathf.Max(verSpeed, jumpForce * springForce);
        }

        public bool CheckPickEnabled() {
            return false;
        }

        public bool CheckAttackEnabled() {
            return grounded && BombCount < MaxBombs;
        }

        private Bomb CheckBombFoward(Vector3 dir) {
            int layerMask = 1 << LayerMask.NameToLayer("Bomb");
            if (Physics.Raycast(
                    transform.TransformPoint(new Vector3(0.0f, 0.5f, 0)), dir, out var hit, 0.85f, layerMask)) {
                var b1 = hit.transform.gameObject.GetComponent<Bomb>();
                if (b1 == null && hit.transform.parent != null) {
                    b1 = hit.transform.parent.gameObject.GetComponent<Bomb>();
                }
                if (b1 != null && !b1.IsMoving && b1.IsCollisionStarted(this)) {
                    return b1;
                }
            }

            return null;
        }

        public void Explode(Explosion explosion) {
            Damage();
        }

        public void OnControllerColliderHit(ControllerColliderHit hit) {
            var indexable = hit.gameObject.GetComponent<Indexable>();
            if (indexable == null && hit.transform.parent != null) {
                indexable = hit.transform.parent.GetComponent<Indexable>();
            }
            if (indexable != null) {
                if (indexable is Bomb bomb) {
                    Vector3 diff = bomb.transform.position - transform.position;
                    if (hit.normal.y > 0.5f && diff.y < -0.9f) {
                        Spring(1.0f);
                        
                    }/* else if (bomb.IsMoving) {
                        Stun();
                        bomb.StopMove();
                        
                    }*/
                }
            }
        }
        
        private void OnTriggerEnter(Collider other) {
            var indexable = other.gameObject.GetComponent<Indexable>();
            if (indexable == null && other.transform.parent != null) {
                indexable = other.transform.parent.GetComponent<Indexable>();
            }

            if (indexable != null) {
                if (indexable is Upgrade upgrade) {
                    if (upgrade.type == Upgrade.Type.Bomb) {
                        upgradeBomb++;
                    } else if (upgrade.type == Upgrade.Type.Fire) {
                        upgradeFire++;
                    } else if (upgrade.type == Upgrade.Type.Speed) {
                        upgradeSpeed++;
                    }
                    upgrade.Collect();
                    
                }
            }
        }

        private Vector3 RoundYAngle(Vector3 dir) {
            double angle = Math.Atan2(dir.z, dir.x) * (180 / Math.PI);
            if (angle < 0) angle += 360;
            int roundedAngleDegrees = (int)Math.Round(angle / 45.0) * 45;
            double radians = roundedAngleDegrees * (Math.PI / 180);
            dir.x = (float)Math.Cos(radians);
            dir.z = (float)Math.Sin(radians);
            dir.y = 0;
            
            return dir;
        }
    }
}
