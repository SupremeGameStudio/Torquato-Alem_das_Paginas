using System.Collections.Generic;
using System.Linq;
using BattleGame.Player;
using Data;
using Services;
using UnityEngine;

namespace BattleGame.Controllers {
    public class BattleGameController : MonoBehaviour {

        public int currentIndex;
        public Dictionary<int, Indexable> indexables = new Dictionary<int, Indexable>();
    
        void Start() {
            Debug.Log("Load Complete :" + ServicesManager.Current.GetName());
        
            var player = Instance<PlayerController>("Battle/Players/PrefabPlayer", transform.position);
            player.Setup(this);
            player.gameObject.name += " " + player.index;
        }

        void Update() {
        
        }

        private void LateUpdate() {
            indexables
                .Where(pair => pair.Value == null).ToList()
                .ForEach(pair => indexables.Remove(pair.Key));
        }

        public T Instance<T>(string resource, Vector3 pos) where T : Indexable {
            var obj = Instantiate(ResourceManager.load(resource), pos, Quaternion.identity).GetComponent<T>();
            obj.index = ++currentIndex;

            indexables[obj.index] = obj;
            return obj;
        }
    }
}
