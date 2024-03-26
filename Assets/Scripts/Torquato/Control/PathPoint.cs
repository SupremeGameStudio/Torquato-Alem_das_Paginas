using UnityEngine;

namespace Scripting.Torquato.Control {
    public class PathPoint : MonoBehaviour {
        public PathType type;
        public bool autoRunner;
        public PathPoint[] nextPoints;
    
        public void OnDrawGizmos() {
            Gizmos.color = 
                type == PathType.FOWARD ? Color.white : 
                type == PathType.REVERSE ? Color.red : 
                type == PathType.PLATFORM ? Color.green :
                type == PathType.PLATFORM_TRANSITION ? new Color(0, 0.5f, 0f) : 
                type == PathType.PLATFORM_REVERSE ? Color.cyan :Color.blue;
            
            Gizmos.DrawSphere(transform.position, 0.1f);
            if (nextPoints != null) {
                foreach (var nextPoint in nextPoints) {
                    if (nextPoint != null) {
                        Gizmos.DrawLine(transform.position, nextPoint.transform.position);
                    }
                }
            }
        }
    }
}
