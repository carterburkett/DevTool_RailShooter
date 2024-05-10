using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.iOS;
using UnityEngine.UIElements;

namespace Beziers {
    public static class BezGenerationAlgorithm {
        private static float BinomialCoefficient(int n, int k) {
            float result = 1;
            for (int i = 1; i <= k; i++) {
                result *= (float)(n - i + 1) / i;
            }
            return result;
        }

        public static void GetCurve(out Vector3 result, List<Vector3> points, float time) {
            int degree = points.Count - 1;
            result = Vector3.zero;

            List<float> t = new List<float>();
            List<float> mt = new List<float>();

            t.Add(1);
            mt.Add(1);

            for (int i = 1; i <= degree; i++) {
                t.Add(t[i - 1] * time);
                mt.Add(mt[i - 1] * (1 - time));
            }

            for (int i = 0; i <= degree; i++) {
                float binomialCoeff = BinomialCoefficient(degree, i);
                Vector3 outStep = binomialCoeff * mt[degree - i] * t[i] * points[i];
                result += outStep;
            }
        }


        public static void GetCurveDerivative(out Vector3 derivative, List<Vector3> points, float time) {
            int degree = points.Count - 1;
            derivative = Vector3.zero;

            List<float> t = new List<float>();
            List<float> mt = new List<float>();

            t.Add(1);
            mt.Add(1);

            for (int i = 1; i <= degree; i++) {
                t.Add(t[i - 1] * time);
                mt.Add(mt[i - 1] * (1 - time));
            }
            
            for (int i = 0; i < degree; i++) {
                float binomialCoeff = BinomialCoefficient(degree - 1, i);
                Vector3 outStep = binomialCoeff * (degree - i) * mt[degree - 1 - i] * points[i + 1];
                derivative += outStep;
            }
        }

        public static void DrawCurve(List<Transform> points, Color color, bool recursive, float resolution = 0.1f) {
            if (points.Count < 2) {
                Debug.LogWarning("Cannot draw curve: Insufficient number of control points.");
                return;
            }

            Vector3 previousPoint = Evaluate(0f, points);
            for (float t = resolution; t <= 1f; t += resolution) {
                Vector3 nextPoint = Evaluate(t, points);
                Debug.DrawLine(previousPoint, nextPoint, color);
                previousPoint = nextPoint;
            }

            // Ensure the last control point is included
            Vector3 lastPoint = Evaluate(1f, points);
            Debug.DrawLine(previousPoint, lastPoint, color);
            
            if(recursive) {
               Debug.DrawLine(lastPoint, points[0].position, color);
            }
        }

        public static Vector3 Evaluate(float t, List<Transform> points) {
            int degree = points.Count - 1;
            List<Vector3> currentPoints = new List<Vector3>(points.Select(p => p.position));

            while (currentPoints.Count > 1) {
                List<Vector3> newPoints = new List<Vector3>();
                for (int i = 0; i < currentPoints.Count - 1; i++) {
                    Vector3 newPoint = Vector3.Lerp(currentPoints[i], currentPoints[i + 1], t);
                    newPoints.Add(newPoint);
                }
                currentPoints = newPoints;
            }

            return currentPoints[0];
        }
    }
}