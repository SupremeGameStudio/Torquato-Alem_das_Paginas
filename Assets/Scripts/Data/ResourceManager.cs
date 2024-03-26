using UnityEngine;

namespace Data {
    public class ResourceManager {

        public static GameObject load(string path) {
            
            return Resources.Load<GameObject>(path);
        }
    }
}