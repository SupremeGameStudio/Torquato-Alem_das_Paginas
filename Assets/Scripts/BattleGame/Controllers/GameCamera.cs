using System;
using BattleGame.Player;
using UnityEngine;

namespace BattleGame.Controllers {
    public class GameCamera : MonoBehaviour {

        public float distance = 5;
        public float speed;
        public Vector3 angle = new Vector3(1, 1, 0);
        private PlayerController player;
        
        private void Update() {
            if (player == null) {
                foreach (var pl in FindObjectsOfType<PlayerController>()) {
                    player = pl;
                    break;
                }
            } else {
                var pos = transform.position;
                Vector3 idealPos = player.transform.position + angle * distance;
                pos = Vector3.Lerp(pos, idealPos, speed * Time.deltaTime);

                transform.position = pos;
                transform.LookAt(player.transform);
            }
        }
    }
}