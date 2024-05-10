using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beziers {
    public static class QuadraticBezFormula {
        static Vector3 a;
        static Vector3 b;
        static Vector3 c;

        public static void GetCurve(out Vector3 result, Vector3 p0, Vector3 p1, Vector3 p2, float time) {
            float tt = time * time;

            float u = 1f - time;
            float uu = u * u;

            result = uu * p0;
            result += 2f * u * time * p1;
            result += tt * p2;
        }

        public static void GetCurveDerivative(out Vector3 result, Vector3 p0, Vector3 p1, Vector3 p2, float time) {
            float u = 1f - time;

            result = -2f * u * p0;
            result += (2f * u - 2f * time) * p1;
            result += 2f * time * p2;
        }

        public static void DrawLines(Vector3 p0, Vector3 p1, Vector3 p2, float time) {
            Debug.DrawLine(p0, p1, Color.green);
            Debug.DrawLine(p1, p2, Color.green);

            a = Vector3.Lerp(p0, p1, time);
            b = Vector3.Lerp(p1, p2, time);

            Debug.DrawLine(a, b, Color.yellow);

            c = Vector3.Lerp(a, b, time);

            Debug.DrawLine(c, c, Color.grey);   
        }
    }
}