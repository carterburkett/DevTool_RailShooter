using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Unity.Android.Types;
using Unity.VisualScripting;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace RailAndCart{
public class PlayerMovement : MonoBehaviour
{
    [HideInInspector] public enum _InputType {SoloKB, MKB, Controller};

    public float xySpeed = 15;
    public float lookSpeed = 300;
    public float moveSpeed = 5;

    public Transform aimTarget;
    [Range(0,1)]public float leanSpeed = .1f;
    private float speedMod = 0;

    [Tooltip("Controller is not currently supported. Sorry!")]
    public _InputType inputType = _InputType.MKB;
    

    public GameObject cameraParent;
    [HideInInspector] public Transform targetWaypoint;

    private int waypointIndex = 1;

    void LocalMove(float x, float y, float speed) {
        transform.localPosition += new Vector3(x, y, 0) * speed * Time.deltaTime;
        ClampPosition();
    }

    void ClampPosition() {
        Vector3 pos = cameraParent.GetComponentInChildren<Camera>().WorldToViewportPoint(transform.position);
        pos.x = Mathf.Clamp01(pos.x);
        pos.y = Mathf.Clamp01(pos.y);
        transform.position = cameraParent.GetComponentInChildren<Camera>().ViewportToWorldPoint(pos);
    }

    void RotationLook(float h, float v, float speed) {
        aimTarget.parent.localPosition = Vector3.zero;
        aimTarget.localPosition = new Vector3(h, v, 1);

        Vector3 direction = aimTarget.position - transform.position;

        if (direction != Vector3.zero) {
            Quaternion targetRotation = Quaternion.LookRotation(transform.parent.forward);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, speed * Time.deltaTime);
        }
    }

    void HorizontalLean(Transform target, float axis, float leanLimit, float lerpTime) {
        Vector3 targetEulerAngels = target.localEulerAngles;
        target.localEulerAngles = new Vector3(targetEulerAngels.x, targetEulerAngels.y, 
            Mathf.LerpAngle(targetEulerAngels.z, -axis * leanLimit, lerpTime));
    }

    private void Start() {
        Cursor.lockState = CursorLockMode.Locked;
    }

    void Update() {
        Cursor.lockState = CursorLockMode.Confined;
        float x = (inputType == _InputType.MKB) ? Input.GetAxis("Mouse X") : Input.GetAxis("Horizontal");
        float y = (inputType == _InputType.MKB) ? Input.GetAxis("Mouse Y") : Input.GetAxis("Vertical");

        LocalMove(x, y, xySpeed);
        RotationLook(x, y, lookSpeed);
        HorizontalLean(transform.GetChild(0), x, 80, leanSpeed);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.blue;
        if(aimTarget != null ) {
        Gizmos.DrawWireSphere(aimTarget.position, .5f);
        Gizmos.DrawSphere(aimTarget.position, .15f);
        
        }
    }
}
}
