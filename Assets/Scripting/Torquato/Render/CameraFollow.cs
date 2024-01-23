using UnityEngine;

namespace Scripting.Torquato.Render {
    public class CameraFollow : MonoBehaviour {

        public Transform target;
        public float distance;
        public float followSpeed;
        public float height;
        public Vector3 angle;

        private Vector3 lastTargetPos = new Vector3(0, 0, -999);
        private Vector3 lastLookPos = new Vector3(0, 0, -999);
    
        void Start() {
        
        }
    
        void Update() {
            var targetPos = target.position + (angle * distance);
            if (targetPos.z < lastTargetPos.z) {
                targetPos.z = lastTargetPos.z;
            }
            lastTargetPos = targetPos;
            
            if (Vector3.Distance(transform.position, targetPos) > distance * 5) {
                transform.position = targetPos;
            } else {
                transform.position = Vector3.Lerp(transform.position, targetPos, followSpeed * Time.deltaTime);
            }

            var lookpos = target.position;
            if (lookpos.z < lastLookPos.z) {
                lookpos.z = lastLookPos.z;
            }
            lastLookPos = lookpos;
            lookpos = Vector3.Lerp(lookpos, target.position, 0.5f);
            transform.LookAt(lookpos + new Vector3(0, height, 0));
        }
    }
}
