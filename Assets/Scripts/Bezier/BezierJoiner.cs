using Beziers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Beziers{
    public class BezierJoiner : MonoBehaviour {
        public Color curveColor = Color.white;
        public float resolution = 0.1f;

        void OnDrawGizmos() {
            List<List<Transform>> curves = new List<List<Transform>>();
            List<Transform> points = new List<Transform>();

            foreach (Transform child in transform) {
                BezierByDegree bezierComponent = child.GetComponent<BezierByDegree>();
                if (bezierComponent != null) {
                    List<Transform> curveControlPoints = new List<Transform>();
                    foreach (Transform point in child) {
                        curveControlPoints.Add(point);
                    }
                    curves.Add(curveControlPoints);
                }
                else {
                    points.Add(child);
                }
            }

            DrawConnectedCurvesAndPoints(curves, points, curveColor, resolution);
        }

        void DrawConnectedCurvesAndPoints(List<List<Transform>> curves, List<Transform> points, Color color, float res) {
            foreach (List<Transform> curve in curves) {
                if (curve.Count < 2) {
                    Debug.LogWarning("Cannot draw curve: Insufficient number of control points.");
                    continue;
                }

                for (float t = 0; t < 1f; t += res) {
                    Vector3 currentPoint = BezGenerationAlgorithm.Evaluate(t, curve);
                    Vector3 nextPoint = BezGenerationAlgorithm.Evaluate(t + res, curve);
                    Gizmos.color = color;
                    Gizmos.DrawLine(currentPoint, nextPoint);
                }
            }

            Gizmos.color = color;
            foreach (Transform point in points) {
                Gizmos.DrawSphere(point.position, 0.1f);
            }
        }
    }
}
