using Screens;
using UnityEngine;

namespace Data {
    public class MainControl : MonoBehaviour {
        void Start() {
            var tr = FindObjectOfType<Canvas>().transform;
            foreach (Transform t in tr) {
                Destroy(t.gameObject);
            }
            ScreenManager.ToScreen(ScreenManager.ScreenType.LOBBY);
        }
    }
}
