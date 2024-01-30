using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

namespace Scripting.Torquato.Control {
    public class PathLine {
        public readonly PathType type;
        public readonly Vector3 pointA;
        public readonly Vector3 pointB;
        public readonly Vector3 dir;
        public readonly Quaternion rotation;
        public readonly Quaternion cleanRotation;
        public readonly float length;
        
        private List<PathLine> prev = new List<PathLine>();
        public IReadOnlyList<PathLine> PrevLines {
            get => prev; 
        }
        
        private List<PathLine> next = new List<PathLine>();
        public IReadOnlyList<PathLine> NextLines {
            get => next; 
        }

        public PathLine(PathType type, Vector3 pointA, Vector3 pointB) {
            this.type = type;
            this.pointA = pointA;
            this.pointB = pointB;
            this.length = Vector3.Distance(this.pointA, this.pointB);
            var cleanDir = (this.pointB - this.pointA).normalized;
            dir = cleanDir;
            if (type == PathType.FOWARD) {
                dir = new Vector3(dir.x, 0, dir.z).normalized;
                
            } else if (type == PathType.REVERSE) {
                dir = -new Vector3(dir.x, 0, dir.z).normalized;
                
            } else if (type == PathType.PLATFORM || type == PathType.PLATFORM_TRANSITION) {
                dir = new Vector3(dir.z, 0, -dir.x).normalized;
                
            } else if (type == PathType.PLATFORM_REVERSE || type == PathType.PLATFORM_TRANSITION_REVERSE) {
                dir = new Vector3(-dir.z, 0, dir.x).normalized;
                
            }
            this.rotation = Quaternion.LookRotation(dir);
            this.cleanRotation = Quaternion.LookRotation(cleanDir);
        }

        public PathLine(PathPoint pointA, PathPoint pointB) : this(pointA.type, pointA.transform.position, pointB.transform.position) {
            
        }

        public void AddNext(PathLine line) {
            next.Add(line);
            line.prev.Add(this);
        }
    }
}