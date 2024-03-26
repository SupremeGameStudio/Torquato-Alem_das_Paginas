using UnityEngine;

namespace Scripting.Torquato.Library {
    public class Book : MonoBehaviour {
        public int bookId;
        public Animator anim;
        private bool open = false;
        
        public void Start() {
            open = false;
            anim.Play("Closed");
        }
        
        public void Open() {
            if (!open) {
                open = true;
                anim.Play("Open");
            }
        }

        public void Close() {
            if (open) {
                open = false;
                anim.Play("Close");
            }
        }
    }
}
