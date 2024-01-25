using System.Collections;
using System.Collections.Generic;
using Scripting.Torquato.Control;
using Scripting.Torquato.Render;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour {

    public Transform startPoint;
    public GameObject prefabPlayer;
    public CameraFollow cameraFollow;

    private Player player;
    
    void Start() {
        player = Instantiate(prefabPlayer, startPoint.position, quaternion.identity).GetComponent<Player>();
        player.Setup(this);
        cameraFollow.target = player.transform;
    }
    
    void Update() {
        
    }

    public void OnPlayerFallOnHole() {
        Destroy(player.gameObject);
        StartCoroutine(EndLevel());
    }

    private IEnumerator EndLevel() {
        yield return new WaitForSeconds(4);

        SceneManager.LoadScene(0);
    }
}
