using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

namespace RailAndCart {

    public class RailCameraFollow : MonoBehaviour
{
    public Transform target;
        public Vector3 offset = new Vector3(0, 0, -5);
    public Vector2 limits = new Vector2(5, 3);
    
    [Range(0, 1)] public float smoothTime = 0;

    private Vector3 velocity = Vector3.zero;

    void Update(){
        Vector3 parentPos = transform.parent.localPosition;
        transform.parent.localPosition = new Vector3(parentPos.x, parentPos.y, offset.z);

        if (!Application.isPlaying){ 
            transform.localPosition = new Vector3(target.transform.localPosition.x + offset.x, target.transform.localPosition.y + offset.y, transform.localPosition.z);
            transform.LookAt(target.transform);
        }
        FollowTarget(target);
    }

    private void LateUpdate() {
        Vector3 localPos = transform.localPosition;
        transform.localPosition = new Vector3(Mathf.Clamp(localPos.x, -limits.x, limits.x), Mathf.Clamp(localPos.y, -limits.y, limits.y), localPos.z);
    }

    public void FollowTarget(Transform t) {
        Vector3 localPos = transform.localPosition;
        Vector3 targetLocalPos = t.transform.localPosition;
        
        transform.localPosition = Vector3.SmoothDamp(localPos, new Vector3(targetLocalPos.x + offset.x, targetLocalPos.y + offset.y, localPos.z), ref velocity, smoothTime); //should z recv offset?
    }

    private void OnDrawGizmos() {
        Vector3 parentPos = transform.parent.position;
        Vector3 startPos = new Vector3(parentPos.x + offset.x, parentPos.y + offset.y, parentPos.z);

        Gizmos.color = Color.blue;
        Gizmos.DrawLine(new Vector3(transform.position.z, -limits.y, -limits.x) + startPos, new Vector3(transform.position.z, -limits.y, limits.x) + startPos);
        Gizmos.DrawLine(new Vector3(transform.position.z, limits.y, -limits.x) + startPos, new Vector3(transform.position.z, limits.y, limits.x) + startPos);
        Gizmos.DrawLine(new Vector3(transform.position.z, -limits.y, -limits.x) + startPos, new Vector3(transform.position.z, limits.y, -limits.x) + startPos);
        Gizmos.DrawLine(new Vector3(transform.position.z, -limits.y, limits.x) + startPos, new Vector3(transform.position.z, limits.y, limits.x) + startPos);
    }
}
}