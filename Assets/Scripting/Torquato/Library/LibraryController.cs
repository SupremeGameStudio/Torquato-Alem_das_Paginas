using System;
using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Scripting.Torquato.Library {
    public class LibraryController : MonoBehaviour {
        
        public Book[] books;
        public GameObject sphere;
        
        private Book lastSelected;
        private Ray ray;
        private RaycastHit hit;

        private bool selected;

        private void Start() {
            sphere.SetActive(false);
        }

        void Update() {
            if (selected) return;
            
            ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit)) {
                if (Input.GetMouseButtonDown(0)) {
                    var book = hit.collider.gameObject.GetComponent<Book>();
                    if (book != null && book != lastSelected) {
                        if (lastSelected != null) {
                            lastSelected.Close();
                        }
                        lastSelected = book;
                        lastSelected.Open();
                        sphere.SetActive(true);
                    }

                    if (hit.collider.gameObject == sphere) {
                        OnGameSelected();
                    }
                }
            }
        }

        public void OnGameSelected() {
            selected = true;
            StartCoroutine(PlayAnimation());
        }

        IEnumerator PlayAnimation() {
            yield return null;

            SceneManager.LoadScene(1);
        }
    }
}