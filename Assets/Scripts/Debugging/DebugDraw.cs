using System.Collections.Generic;
using UnityEngine;

namespace Debugging {
    public class DebugDraw : MonoBehaviour {

        private static Material _lineMaterial;
        public static Material LineMaterial {
            get {
                if (!_lineMaterial) {
                    Shader shader = Shader.Find("Hidden/Internal-Colored");
                    _lineMaterial = new Material(shader);
                    _lineMaterial.hideFlags = HideFlags.HideAndDontSave;
                    _lineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    _lineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    _lineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
                    _lineMaterial.SetInt("_ZWrite", 0);
                    _lineMaterial.SetInt("_ZTest", 0);
                }

                return _lineMaterial;
            }
        }

        private static Material _flatMaterial;
        public static Material FlatMaterial {
            get {
                if (!_flatMaterial) {
                    Shader shader = Shader.Find("Standard");
                    _flatMaterial = new Material(shader);
                    _flatMaterial.hideFlags = HideFlags.HideAndDontSave;
                    _flatMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                    _flatMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                    _flatMaterial.SetInt("_ZWrite", 1);
                    _flatMaterial.DisableKeyword("_ALPHATEST_ON");
                    _flatMaterial.EnableKeyword("_ALPHABLEND_ON");
                    _flatMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                    _flatMaterial.renderQueue = 3000;
                }

                return _lineMaterial;
            }
        }
        
        const float halfSize = 0.5f;
        Vector3[] vertices = {
            new Vector3(-halfSize, -halfSize, halfSize),
            new Vector3(halfSize, -halfSize, halfSize),
            new Vector3(halfSize, halfSize, halfSize),
            new Vector3(-halfSize, halfSize, halfSize),

            new Vector3(halfSize, -halfSize, -halfSize),
            new Vector3(-halfSize, -halfSize, -halfSize),
            new Vector3(-halfSize, halfSize, -halfSize),
            new Vector3(halfSize, halfSize, -halfSize),

            new Vector3(-halfSize, -halfSize, -halfSize),
            new Vector3(-halfSize, -halfSize, halfSize),
            new Vector3(-halfSize, halfSize, halfSize),
            new Vector3(-halfSize, halfSize, -halfSize),

            new Vector3(halfSize, -halfSize, halfSize),
            new Vector3(halfSize, -halfSize, -halfSize),
            new Vector3(halfSize, halfSize, -halfSize),
            new Vector3(halfSize, halfSize, halfSize),

            new Vector3(-halfSize, halfSize, halfSize),
            new Vector3(halfSize, halfSize, halfSize),
            new Vector3(halfSize, halfSize, -halfSize),
            new Vector3(-halfSize, halfSize, -halfSize),

            new Vector3(-halfSize, -halfSize, -halfSize),
            new Vector3(halfSize, -halfSize, -halfSize),
            new Vector3(halfSize, -halfSize, halfSize),
            new Vector3(-halfSize, -halfSize, halfSize)
        };

        
        public class Line3 {
            public Color color;
            public float x1, y1, z1, x2, y2, z2;
            public bool draw;
        }

        public class Sphere {
            public Color color;
            public float x, y, z, radius;
            public bool draw;
        }

        public class Cube {
            public Color color;
            public float x, y, z, size;
            public bool draw;
        }

        private static List<Line3> lines3 = new List<Line3>();
        private static List<Sphere> spheres = new List<Sphere>();
        private static List<Cube> cubes = new List<Cube>();
        
        public static void DrawLine(float x1, float y1, float z1, float x2, float y2, float z2, Color color) {
            lines3.Add(new Line3 { x1 = x1, y1 = y1, z1 = z1, x2 = x2, y2 = y2, z2 = z2, color = color });
        }

        public static void DrawLine(Vector3 from, Vector3 to, Color color) {
            lines3.Add(new Line3 { x1 = from.x, y1 = from.y, z1 = from.z, x2 = to.x, y2 = to.y, z2 = to.z, color = color });
        }

        public static void DrawCube(Vector3 pos, float size, Color color) {
            cubes.Add(new Cube { x = pos.x, y = pos.y, z = pos.z, size = size, color = color });
        }

        public static void DrawCube(float x, float y, float z, float size, Color color) {
            cubes.Add(new Cube { x = x, y = y, z = z, size = size, color = color });
        }

        public static void DrawSphere(float x, float y, float z, float radius, Color color) {
            spheres.Add(new Sphere { x = x, y = y, z = z, radius = radius, color = color });
        }

        public static void DrawSphere(Vector3 pos, float radius, Color color) {
            spheres.Add(new Sphere { x = pos.x, y = pos.y, z = pos.z, radius = radius, color = color });
        }

        public void Update() {
            lines3.RemoveAll(line => line.draw);
            cubes.RemoveAll(cube => cube.draw);
            spheres.RemoveAll(sphere => sphere.draw);
        }

        public void OnRenderObject() {
            LineMaterial.SetPass(0);
            FlatMaterial.SetPass(0);

            GL.PushMatrix();
            GL.MultMatrix(transform.localToWorldMatrix);

            GL.Begin(GL.LINES);
            foreach (var line in lines3) {
                GL.Color(line.color);
                GL.Vertex3(line.x1, line.y1, line.z1);
                GL.Vertex3(line.x2, line.y2, line.z2);
                line.draw = true;
            }
            foreach (var sphere in spheres) {
                GL.Color(sphere.color);
                // Calculate vertices for the circle
                Vector3 pos = new Vector3(sphere.x, sphere.y, sphere.z);
                for (int i = 0; i < 10; i++) {
                    float theta = i * Mathf.PI / 5;
                    float thetab = (i + 1) * Mathf.PI / 5;
                    Vector3 vertex = new Vector3 (
                            Mathf.Cos(theta) * sphere.radius, 
                            Mathf.Sin(theta) * sphere.radius, 0);
                    Vector3 vertexb = new Vector3 (
                        Mathf.Cos(thetab) * sphere.radius, 
                        Mathf.Sin(thetab) * sphere.radius, 0);
                    
                    GL.Vertex3(pos.x + vertex.x, pos.y + vertex.y, pos.z + vertex.z);
                    GL.Vertex3(pos.x + vertexb.x, pos.y + vertexb.y, pos.z + vertexb.z);
                    GL.Vertex3(pos.x + vertex.x, pos.y + vertex.z, pos.z + vertex.y);
                    GL.Vertex3(pos.x + vertexb.x, pos.y + vertexb.z, pos.z + vertexb.y);
                    GL.Vertex3(pos.x + vertex.z, pos.y + vertex.y, pos.z + vertex.x);
                    GL.Vertex3(pos.x + vertexb.z, pos.y + vertexb.y, pos.z + vertexb.x);
                }
                sphere.draw = true;
            }
            GL.End();
            
            GL.Begin(GL.QUADS);
            foreach (var cube in cubes) {
                GL.Color(cube.color);
                foreach (var vertice in vertices) {
                    Vector3 vertex = vertice * cube.size + new Vector3(cube.x, cube.y, cube.z);
                    GL.Vertex3(vertex.x, vertex.y, vertex.z);
                }
                cube.draw = true;
            }
            GL.End();
            
            GL.PopMatrix();
        }
    }
}