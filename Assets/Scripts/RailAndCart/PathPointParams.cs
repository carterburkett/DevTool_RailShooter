using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailAndCart {

    public class PathPointParams : MonoBehaviour
{
    [HideInInspector] public enum _pointType {controlPoint, bezierHandle}
    public _pointType pointType = _pointType.controlPoint;

    private void Awake() {
        if(pointType == _pointType.controlPoint) { tag = "ControlPoint"; }
        else if(pointType == _pointType.bezierHandle) { tag = "BezierHandle"; }
    }

    private void OnDrawGizmos() {
        if (pointType == _pointType.controlPoint) { tag = "ControlPoint"; }
        else if (pointType == _pointType.bezierHandle) { tag = "BezierHandle"; }
    }
}
}
