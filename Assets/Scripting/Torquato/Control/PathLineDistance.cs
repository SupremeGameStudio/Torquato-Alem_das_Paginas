namespace Scripting.Torquato.Control {
    public class PathLineDistance {
        public PathLine line;
        public float distance;
        public float time;

        public PathLineDistance(PathLine line, float distance, float time) {
            this.line = line;
            this.distance = distance;
            this.time = time;
        }
    }
}