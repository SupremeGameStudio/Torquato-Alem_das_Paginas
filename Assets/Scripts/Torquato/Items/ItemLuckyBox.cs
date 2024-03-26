using System;
using Scripting.Torquato.Control;
using UnityEngine;

namespace Scripting.Torquato.Items {
    public class ItemLuckyBox : Item {
        public GameObject luckyItem;

        private void Start() {
            if (luckyItem != null) {
                luckyItem.SetActive(false);
            }
        }

        public override void OnPlayerCollision(Player player, ControllerColliderHit collision) {
            if (collision.normal.y > 0.5f) {
                player.Spring(1.0f);
                if (luckyItem != null) {
                    luckyItem.SetActive(true);
                    luckyItem.transform.SetParent(null, true);
                }

                Destroy(gameObject);
            } else if (player.IsAttacking) {
                if (luckyItem != null) {
                    luckyItem.SetActive(true);
                    luckyItem.transform.SetParent(null, true);
                }

                Destroy(gameObject);
            }
        }
    }
}