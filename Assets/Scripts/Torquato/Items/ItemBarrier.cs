using System;
using Scripting.Torquato.Control;
using UnityEngine;

namespace Scripting.Torquato.Items {
    public class ItemBarrier : Item {
        public GameObject trigger;
        public GameObject barrier;

        private void Start() {
            barrier.SetActive(false);
            trigger.SetActive(true);
        }

        public override void OnPlayerCollision(Player player, ControllerColliderHit collision) {
            if (!barrier.activeSelf) {
                barrier.SetActive(true);
                trigger.SetActive(false);
            }
        }
    }
}