using Beziers;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailAndCart {

    public class RailCarMovement : MonoBehaviour
{
    public PathInterpolator pathInterpolator;
    public float speed = 5f;
    private List<Vector3> waypoints;
    private int currentWaypointIndex = 0; 
    private bool pathDone = false;

    void Start() {
        waypoints = pathInterpolator.interpolatedPoints;

        if (waypoints.Count == 0) {
            Debug.LogWarning("No waypoints found. Make sure the PathInterpolator has generated interpolated points.");
            enabled = false;
        }

        transform.position = waypoints[0];
    }

    void Update() {
        if (waypoints == null || waypoints.Count == 0) return;

        transform.position = Vector3.MoveTowards(transform.position, waypoints[currentWaypointIndex], speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, waypoints[currentWaypointIndex]) < 0.1f) {
            if (currentWaypointIndex + 1 >= waypoints.Count) {
                if(pathInterpolator.isCyclical) { currentWaypointIndex = 0; }
                else{ 
                    pathDone = true;
                }
            }
            if (!pathDone) { currentWaypointIndex++; }
        }
       
        if (currentWaypointIndex < waypoints.Count - 1) {
            Vector3 direction = waypoints[currentWaypointIndex + 1] - waypoints[currentWaypointIndex];
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 180f * Time.deltaTime);
        }
    }
}
}