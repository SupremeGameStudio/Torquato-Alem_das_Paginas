using System.Collections;
using Data;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

namespace Screens {
    public class ScreenManager : MonoBehaviour {

        public enum ScreenType {
            START, LOBBY
        }
        
        private static ScreenBase currentScreen;
        private static int currentScene;
        public static bool IsTransition { get; private set; }
        public static object TransferData { get; set; }
        
        public static IEnumerator ToGame() {
            IsTransition = true;
            return ToGameIterate();
        }
        
        private static IEnumerator ToGameIterate() {
            EventSystem es = EventSystem.current;
            es.enabled = false;
            
            yield return SceneManager.LoadSceneAsync(1);
            currentScreen = null;
            currentScene = 1;
            
            es.enabled = true;
            IsTransition = false;
        }
        
        public static void ToScreen(ScreenType screen) {
            IsTransition = true;
            var temp = new GameObject().AddComponent<ScreenLoader>();
            temp.StartCoroutine(ToScreenIterate(temp, screen));
        }

        private static IEnumerator ToScreenIterate(ScreenLoader loader, ScreenType screen) {
            if (currentScene == 1) {
                yield return SceneManager.LoadSceneAsync(0);
                currentScene = 0;
            }

            Canvas canvas = FindObjectOfType<Canvas>();
            string name = (screen.ToString()).Substring(0, 1) + (screen.ToString()).Substring(1).ToLower();
            GameObject obj = ResourceManager.load("Screens/PrefabScreen" + name);
            GameObject instance = Instantiate(obj, canvas.transform, true);
            instance.transform.localPosition = Vector3.zero;
            instance.transform.localScale = Vector3.one;

            yield return TransitionScreen(instance.GetComponent<ScreenBase>());

            if (loader != null) {
                Destroy(loader.gameObject);
            }
            IsTransition = false;
        }

        private static IEnumerator TransitionScreen(ScreenBase nextScreen) {
            EventSystem es = EventSystem.current;
            es.enabled = false;
            
            if (currentScreen != null) {
                yield return currentScreen.Close();
            }
            
            currentScreen = nextScreen;

            if (nextScreen != null) {
                yield return nextScreen.Open();
            }
            
            es.enabled = true;
        }
    }
}
