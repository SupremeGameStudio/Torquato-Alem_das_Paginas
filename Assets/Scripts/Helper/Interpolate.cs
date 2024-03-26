using UnityEngine;

namespace Helper {
    public class Interpolate {

        public static float circleOut(float a) {
            a--;
            return Mathf.Sqrt(1 - a * a);
        }

        public static float circleIn(float a) {
            return 1 - Mathf.Sqrt(1 - a * a);
        }

        public static float circle(float a) {
            if (a <= 0.5f) {
                a *= 2;
                return (1 - Mathf.Sqrt(1 - a * a)) / 2;
            }

            a--;
            a *= 2;
            return (Mathf.Sqrt(1 - a * a) + 1) / 2;
        }
    
        public static float powerIn(float a, float power) {
            return Mathf.Pow(a, power);
        }
    
        public static float powerOut(float a, float power) {
            return -Mathf.Abs(Mathf.Pow(a - 1, power)) + 1;
        }

        public static float circleOutSmooth(float a) {
            return (Mathf.Sqrt(1 - (a - 1) * (a - 1)) + a) * 0.5f;
        }

    }
}
