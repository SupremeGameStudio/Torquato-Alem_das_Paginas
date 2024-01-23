using System.Collections;
using System.Collections.Generic;
using Scripting.Torquato.Control;
using Scripting.Torquato.Render;
using Unity.Mathematics;
using UnityEngine;

public class GameController : MonoBehaviour {

    public Transform startPoint;
    public GameObject prefabPlayer;
    public CameraFollow cameraFollow;

    private Player player;
    
    void Start() {
        player = Instantiate(prefabPlayer, startPoint.position, quaternion.identity).GetComponent<Player>();
        cameraFollow.target = player.transform;
    }
    
    void Update() {
        
    }
}
