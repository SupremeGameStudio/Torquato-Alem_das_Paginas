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
    public PathPoint firstPoint;
    
    private Player player;
    private List<PathLine> lines;
    private List<PathPoint> tempList;
    private List<PathLine> tempSearchList = new List<PathLine>();

    public List<PathLine> Lines {
        get {
            if (lines == null) {
                CalculatePath();
            }

            return lines;
        }
    }
    
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

    private void CalculatePath() {
        tempList = new List<PathPoint>();
        lines = new List<PathLine>();
        AddLine(firstPoint);
        tempList.Clear();
    }

    private void AddLine(PathPoint lineStart) {
        if (lineStart.nextPoints != null && !tempList.Contains(lineStart)) {
            tempList.Add(lineStart);
            
            foreach (var point in lineStart.nextPoints) {
                lines.Add(new PathLine(lineStart, point));
                AddLine(point);
            }
        }

        for (int i = 0; i < lines.Count; i++) {
            var line = lines[i];
            for (int j = 0; j < lines.Count; j++) {
                var line2 = lines[j];
                if (i == j) continue;
                if (line.pointB == line2.pointA) {
                    line.AddNext(line2);
                }
            }
        }
    }

    public void FindCurrentPath(Vector3 pos, List<PathLineDistance> closePaths) {
        closePaths.Clear();
        
        foreach (var line in Lines) {
            CalculateLineDistance(pos, line.pointA, line.pointB, out var dis, out var time);
            
            if (closePaths.Count == 0) {
                closePaths.Add(new PathLineDistance(line, dis, time));
                
            } else {
                bool added = false;
                for (int i = 0; i < closePaths.Count && i < 3; i++) {
                    if (dis <= closePaths[i].distance) {
                        added = true;
                        closePaths.Insert(i, new PathLineDistance(line, dis, time));
                        break;
                    }
                }

                if (!added) {
                    closePaths.Add(new PathLineDistance(line, dis, time));
                }
                while (closePaths.Count > 3) {
                    closePaths.RemoveAt(closePaths.Count - 1);
                }
            }
        }
        foreach (var closePath in closePaths) {
            closePath.distance = Mathf.Sqrt(closePath.distance);
        }
    }

    public static void CalculateLineDistance(Vector3 point, Vector3 pointA, Vector3 pointB, out float distance, out float time) {
        Vector3 v1 = point - pointA;
        Vector3 v2 = pointB - pointA;

        float dot = Vector3.Dot(v1, v2);
        float length2 = v2.sqrMagnitude;

        if (Mathf.Abs(length2) > 0.00001f) {
            time = dot / length2;
        } else {
            time = 0;
        }

        Vector3 closestPoint = pointA + Mathf.Clamp01(time) * v2;
        distance = Vector2.Distance(new Vector2(point.x, point.z), new Vector2(closestPoint.x, closestPoint.z));
    }
}
