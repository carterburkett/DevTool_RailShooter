using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

namespace Beziers {
    public class BezierByDegree : BezierManager {
        public bool DrawCurve = false;
        public Color color = Color.white;

        public static BezierByDegree _Instance { get; private set; }

        int numPoints;
        int nCurves;
        float subTotal = 0f;
        Vector3 derivative;
        Vector3 endOfCurrCurve;

        private void Awake() {
            if (_Instance == null)
                _Instance = this;
            else
                Destroy(gameObject);

            type = _objectType.bezier;
            waypoints = GetComponentsInChildren<Transform>().ToList();
            waypoints = waypoints[0] == transform ? waypoints.Skip(1).ToList() : waypoints.ToList();
        }

        private void Update() {
            GetBezier(out Vector3 myPosition, waypoints, mTime);

            objectOnRail.transform.position = myPosition;
        }

        public void GetBezier(out Vector3 pos, List<Vector3> waypoints, float time) {
            numPoints = waypoints.Count;
            nCurves = numPoints / (3 - 1);

            if (subTotal == 0f) {
                subTotal = 1f / nCurves;
            }

            float localPercentage = GetLocalPercentage(time);
            int localStartPoint = GetLocalStart(time);

            int index0 = localStartPoint;
            int index1 = localStartPoint + 1;
            int index2 = localStartPoint + 2;

            if (index0 < numPoints - 2) {
                Vector3 p0 = waypoints[index0];
                Vector3 p1 = waypoints[index1];
                Vector3 p2 = waypoints[index2];
                endOfCurrCurve = p2;

                BezGenerationAlgorithm.GetCurve(out pos, waypoints, localPercentage);
                BezGenerationAlgorithm.GetCurveDerivative(out derivative, waypoints, localPercentage);

                Quaternion rot = Quaternion.LookRotation(derivative.normalized, Vector3.up);
                objectOnRail.transform.rotation = rot;
            }
            else {
                Vector3 start = waypoints[index0];
                Vector3 end = waypoints[numPoints - 1];

                pos = Vector3.Lerp(start, end, localPercentage);
                derivative = end - start;

                Quaternion rot = Quaternion.LookRotation(derivative.normalized, Vector3.up);
                objectOnRail.transform.rotation = rot;
            }
        }

        public override void GetBezier(out Vector3 pos, List<Transform> waypoints, float time) {
            List<Vector3> pointsOut = new List<Vector3>();
            foreach (Transform t in waypoints) {
                pointsOut.Add(t.position);
            }

            GetBezier(out pos, pointsOut, time);
        }

        private void OnDrawGizmos() {
            if(DrawCurve) {
            waypoints = GetComponentsInChildren<Transform>().ToList();
            waypoints = waypoints[0] == transform ? waypoints.Skip(1).ToList() : waypoints.ToList();
            BezGenerationAlgorithm.DrawCurve(waypoints, color, isCyclic);
            }
        }
        float GetLocalPercentage(float time) {
            float remainder = time % subTotal; 
            float percentage = remainder / subTotal;
            return percentage;
        }

        int GetLocalStart(float time) {
            int n = (int)(time / subTotal);

            int index = n * (3 - 1);

            return index;
        }
    }
}