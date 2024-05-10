using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ProBuilder.Shapes;
using UnityEngine.UIElements;

namespace Beziers {
    public class MakeCubicBez : BezierManager {
        public override void GetBezier(out Vector3 pos, List<Transform> waypoints, float time) {
            if (waypoints.Count > 4) {
                for (int i = 4; i < waypoints.Count; i++) {
                    waypoints.RemoveAt(i);
                }
            }

            CubicBezFormula.GetCurve(out pos,
                waypoints[0].position,
                waypoints[1].position,
                waypoints[2].position,
                waypoints[3].position,
            time);

            CubicBezFormula.DrawLines(
                waypoints[0].position,
                waypoints[1].position,
                waypoints[2].position,
                waypoints[3].position,
            time);
        }

        private void Update() {
            GetBezier(out myPosition, waypoints, mTime);
            objectOnRail.transform.position = myPosition;
        }
    }
}
