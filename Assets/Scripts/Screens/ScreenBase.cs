using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Screens {
    public class ScreenBase : MonoBehaviour {
        
        public virtual IEnumerator Open() {
            yield return null;
        }
        
        public virtual IEnumerator Close() {
            yield return null;
            Destroy(gameObject);
        }
        
    }
}
