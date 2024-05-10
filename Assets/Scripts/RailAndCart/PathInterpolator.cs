using Beziers;
using RailEditors;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

namespace RailAndCart{
public class PathInterpolator : MonoBehaviour{
   
    [HideInInspector] public List<Vector3> controlPoints { get; private set; }
        //Major Waypoints in the waypoint Object
    [HideInInspector] public List<Vector3> pathPoints { get; private set; }
        //All Points in the waypoint Object
    [HideInInspector] public List<Vector3> bezHandlePoints { get; private set; }
        //"Handles" in the way point object. Serve to influence the path
    [HideInInspector] public List<Vector3> interpolatedPoints { get; private set; }
        //the set of points output by the program for the "rail car" to follow

    [HideInInspector] public Color curveColor = Color.blue;
    [HideInInspector] public Color gizmoColor = Color.green;
    [HideInInspector] public Color handleColor = Color.magenta;

    [HideInInspector]
    [Range(0.001f,.99f)]public float resolution = 0.001f;
    
    [HideInInspector] public enum _smoothing {none, bezier, spline}
    [HideInInspector] public _smoothing smoothing = _smoothing.none;
    [HideInInspector]public bool isCyclical;

    private void Awake() {
        if (pathPoints == null) { pathPoints = new List<Vector3>(); }
        if (controlPoints == null) { controlPoints = new List<Vector3>(); }
        if (bezHandlePoints == null) { bezHandlePoints = new List<Vector3>(); }
        if (interpolatedPoints == null) { interpolatedPoints = new List<Vector3>(); }

        foreach (Transform child in transform) {
            pathPoints.Add(child.position);
            if (child.CompareTag("ControlPoint")) {
                controlPoints.Add(child.position);
            }
            else if (child.CompareTag("BezierHandle")) {
                bezHandlePoints.Add(child.position);
            }
        }
        interpolatedPoints = DrawInterpolatedCurve(controlPoints, pathPoints, bezHandlePoints, curveColor, resolution);
        }

    Color colorOut;
    void OnDrawGizmos() {
        //if(isActiveAndEnabled) {
            pathPoints = new List<Vector3>(); 
            controlPoints = new List<Vector3>(); 
            bezHandlePoints = new List<Vector3>(); 
            interpolatedPoints = new List<Vector3>(); 
            UpdateInspectorHierarchy();

            foreach (Transform child in transform) {
                pathPoints.Add(child.position);
                if (child.CompareTag("ControlPoint")) {
                    controlPoints.Add(child.position);
                }
                else if (child.CompareTag("BezierHandle")) {
                    bezHandlePoints.Add(child.position);
                }
            }

            interpolatedPoints = DrawInterpolatedCurve(controlPoints, pathPoints, bezHandlePoints, curveColor, resolution);
            foreach (Vector3 point in controlPoints) {
                Gizmos.color = gizmoColor;
                Gizmos.DrawWireSphere(point, 3.0f);

                Gizmos.color = curveColor;
                Gizmos.DrawWireSphere(point, 1.5f);
            }

            foreach (Vector3 point in bezHandlePoints) {
                Gizmos.color = handleColor;
                Gizmos.DrawWireSphere(point, 2.0f);
            }
            foreach (Vector3 point in pathPoints) {
                Gizmos.color = handleColor;
                Gizmos.DrawWireSphere(point, 2.0f);
            }

            Gizmos.color = curveColor;
            for (int i = 0; i < interpolatedPoints.Count - 1; i++) {
                Gizmos.DrawLine(interpolatedPoints[i], interpolatedPoints[i + 1]);
            }
        //}
    }

    public List<Vector3> DrawInterpolatedCurve(List<Vector3> controlPoints, List<Vector3> pathPoints, 
        List<Vector3> bezHandlePoints, Color color, float res) {
        if (controlPoints.Count < 2) {
            Debug.LogWarning("Cannot draw curve: Insufficient number of path points.");
            return new List<Vector3>();
        }

        if (smoothing != _smoothing.none)
            controlPoints = SmoothCurve(controlPoints, pathPoints, bezHandlePoints);

        List<Vector3> interpolatedPoints = new List<Vector3>();

        for (float t = 0; t < 1f; t += res) {
            Vector3 currentPoint = Vector3.zero;
            Vector3 nextPoint = Vector3.zero;

            if (smoothing == _smoothing.spline){
                currentPoint = CatmullRomInterpolation(controlPoints, t);
                nextPoint = CatmullRomInterpolation(controlPoints, t + res);
            }
            else if(smoothing == _smoothing.bezier){
                currentPoint = InterpolateBezier(controlPoints, t);
                nextPoint = InterpolateBezier(controlPoints, t + res);
            }
            else{
                currentPoint = InterpolatePoints(controlPoints, t);
                nextPoint = InterpolatePoints(controlPoints, t + res);
            }
            
            interpolatedPoints.Add(currentPoint);
        }

        return interpolatedPoints;
    }

    Vector3 InterpolatePoints(List<Vector3> controlPoints, float t) {
        int numSegments = controlPoints.Count - 1;

        int segmentIndex = Mathf.FloorToInt(t * numSegments);
        float segmentT = (t * numSegments) - segmentIndex;

        segmentIndex = Mathf.Clamp(segmentIndex, 0, numSegments - 1);

        Vector3 p0 = controlPoints[segmentIndex];
        Vector3 p1 = controlPoints[segmentIndex + 1];

        Vector3 interpolatedPoint = Vector3.Lerp(p0, p1, segmentT);

        return interpolatedPoint;
    }

    Vector3 CatmullRomInterpolation(List<Vector3> controlPoints, float t) {
        int numPoints = controlPoints.Count;
        int segmentIndex = Mathf.FloorToInt(t * (numPoints - 1));
        float segmentT = (t * (numPoints - 1)) - segmentIndex;

        segmentIndex = Mathf.Clamp(segmentIndex, 0, numPoints - 2);

        // Catmull-Rom interpolation [move along splines]
        Vector3 p0 = controlPoints[Mathf.Max(segmentIndex - 1, 0)];
        Vector3 p1 = controlPoints[segmentIndex];
        Vector3 p2 = controlPoints[Mathf.Min(segmentIndex + 1, numPoints - 1)];
        Vector3 p3 = controlPoints[Mathf.Min(segmentIndex + 2, numPoints - 1)];

        float t2 = segmentT * segmentT;
        float t3 = t2 * segmentT;

        // Catmull-Rom spline formula
        Vector3 interpolatedPoint = 0.5f * (
            (2.0f * p1) +
            (-p0 + p2) * segmentT +
            (2.0f * p0 - 5.0f * p1 + 4.0f * p2 - p3) * t2 +
            (-p0 + 3.0f * p1 - 3.0f * p2 + p3) * t3
        );

        return interpolatedPoint;
    }

    public static Vector3 InterpolateBezier(List<Vector3> controlPoints, float t) {
        int numSegments = controlPoints.Count - 1;
        int segmentIndex = Mathf.FloorToInt(t * numSegments);
        float segmentT = (t * numSegments) - segmentIndex;
        segmentIndex = Mathf.Clamp(segmentIndex, 0, numSegments - 1);

        Vector3 p0 = controlPoints[segmentIndex];
        Vector3 p1 = controlPoints[segmentIndex + 1];

        Vector3 interpolatedPoint = Vector3.Lerp(p0, p1, segmentT);

        return interpolatedPoint;
    }
    float Bernstein(int n, int i, float t) {
        float coefficient = BinomialCoefficient(n, i);

        float basis = coefficient * Mathf.Pow(t, i) * Mathf.Pow(1 - t, n - i);

        return basis;
    }

    float BinomialCoefficient(int n, int k) {
        float result = 1;

        for (int i = 1; i <= k; i++) {
            result *= (float)(n - i + 1) / i;
        }

        return result;
    }
    List<Vector3> SmoothCurve(List<Vector3> controlPoints, List<Vector3> pathPoints, List<Vector3> bezHandlePoints) {
        List<Vector3> smoothedPoints = new List<Vector3>();
        int degree = controlPoints.Count-1;

        if (controlPoints.Count < 3) {
            return controlPoints;
        }

        for (int i = 0; i < degree; i++) {
            Vector3 start = controlPoints[i];
            Vector3 end = controlPoints[i + 1];

            List<Vector3> segmentPoints = new List<Vector3>();
            segmentPoints.Add(start);
            for (int f = pathPoints.IndexOf(start) + 1; f < pathPoints.IndexOf(end); f++) {
                segmentPoints.Add(pathPoints[f]);
            }
            segmentPoints.Add(end);

            for (float t = 0.0f; t <= 1.0f; t += 0.1f) {
                Vector3 pos = Vector3.zero;
                if(smoothing == _smoothing.spline){ 
                     pos = CatmullRomInterpolation(segmentPoints, t);
                }
                else if(smoothing == _smoothing.bezier) {
                     pos = InterpolateBezier(segmentPoints, t);
                        //Should probably swap this BezByDegree bc It's technically more correct. .
                        //Also allows you set "start/end" points bc of tags
                }
                smoothedPoints.Add(pos);
            }
        }
        return smoothedPoints;
    }

    public void AppendPointsToEnd() {
        TagGenerator.AddTag("ControlPoint");
        TagGenerator.AddTag("BezierHandle");
        if (transform.childCount > 0) {
            CreateStandardPoints();
        }
        else {
            GameObject newControlPoint = new GameObject("Vector3.zero");
            newControlPoint.transform.position = transform.position;
            newControlPoint.transform.SetParent(transform);
            newControlPoint.tag = "ControlPoint";
            if (pathPoints == null) { pathPoints = new List<Vector3>(); }
            if (controlPoints == null) { controlPoints = new List<Vector3>(); }
            if (bezHandlePoints == null) { bezHandlePoints = new List<Vector3>(); }
            if (interpolatedPoints == null) { interpolatedPoints = new List<Vector3>(); }
            controlPoints.Add(newControlPoint.transform.position);
            CreateStandardPoints();
        }

        void CreateStandardPoints() {
            Vector3 lastPointPosition = transform.GetChild(transform.childCount - 1).position;
            GameObject newHandlePoint1 = new GameObject("BezierHandle");
            GameObject newHandlePoint2 = new GameObject("BezierHandle");
            GameObject newControlPoint = new GameObject("ControlPoint");

            newHandlePoint1.transform.position = lastPointPosition + Vector3.forward;
            newHandlePoint2.transform.position = lastPointPosition + Vector3.forward * 2;
            newControlPoint.transform.position = lastPointPosition + Vector3.forward * 3;

            newHandlePoint1.transform.SetParent(transform);
            newHandlePoint2.transform.SetParent(transform);
            newControlPoint.transform.SetParent(transform);

            newHandlePoint1.tag = "BezierHandle";
            newHandlePoint2.tag = "BezierHandle";
            newControlPoint.tag = "ControlPoint";

            pathPoints.Add(newControlPoint.transform.position);
            controlPoints.Add(newControlPoint.transform.position);
            bezHandlePoints.Add(newHandlePoint1.transform.position);
            bezHandlePoints.Add(newHandlePoint2.transform.position);

            interpolatedPoints = DrawInterpolatedCurve(controlPoints, pathPoints, bezHandlePoints, curveColor, resolution);
        }
    }

    public void RemovePointsFromEnd() {
        if (transform.childCount >= 3) {
            DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
            DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);
            DestroyImmediate(transform.GetChild(transform.childCount - 1).gameObject);

            pathPoints.RemoveAt(pathPoints.Count - 1);
            controlPoints.RemoveAt(controlPoints.Count - 1);
            bezHandlePoints.RemoveAt(bezHandlePoints.Count - 1);
            bezHandlePoints.RemoveAt(bezHandlePoints.Count - 1);

            interpolatedPoints = DrawInterpolatedCurve(controlPoints, pathPoints, bezHandlePoints, curveColor, resolution);
        }
        else {
            Debug.LogWarning("There are not enough points to remove.");
        }
    }
    
    private void UpdateInspectorHierarchy() {
        (Color color1, Color color2, Color textColor) = HierarchyColorizer.GetReadableTriadicColors(gizmoColor);
        Color colorOut = color1;
        int controlCount = 1;
        int bezCOunt = 1;
        int totalCount = 1;

        foreach (Transform child in transform) {
            //pathPoints.Add(child.position);
            if (totalCount % 3 == 0) {
                colorOut = colorOut == color1 ? color2 : color1;
            }
            if (child.CompareTag("ControlPoint")) {
                //controlPoints.Add(child.position);
                child.gameObject.name = "===WAYPOINT[" + (controlCount - 1).ToString() + "]===";

                HierarchyColorizer.SetTextColor(child.gameObject, textColor);
                HierarchyColorizer.SetBackgroundColor(child.gameObject, colorOut);
                controlCount++;
            }
            else if (child.CompareTag("BezierHandle")) {
                //bezHandlePoints.Add(child.position);
                child.gameObject.name = "   " + (controlCount).ToString() + "----Handle----> " +
                                          (controlCount - 1).ToString();

                HierarchyColorizer.SetTextColor(child.gameObject, textColor);
                HierarchyColorizer.SetBackgroundColor(child.gameObject, colorOut);
                bezCOunt++;
            }
            totalCount++;
        }
    }
}
}
