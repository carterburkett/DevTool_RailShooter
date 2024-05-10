using RailEditors;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace RailAndCart {
public class RailAndCartGenerator : Editor {
    public static RailAndCartGenerator instance;
    public GameObject modelForCart;

    private void Awake() { instance = this; }

    [MenuItem("GameObject/RailShooter/Generate RailCart", true)]
    private static bool ValidateGenerateRailCart() {
        return GameObject.FindObjectOfType<PlayerMovement>() == null;
    }

    //[MenuItem("GameObject/RailShooter/Generate RailCart")]
    public static void GenerateRailCartInternal() {

        TagGenerator.AddTag("RailCart");
        TagGenerator.AddTag("RailCamera");

        GameObject railCart = new GameObject("RailCart");
        railCart.tag = "RailCart";
        railCart.AddComponent<RailCarMovement>();

        GameObject shipParent = new GameObject("ShipParent");
        shipParent.transform.parent = railCart.transform;
        shipParent.AddComponent<PlayerMovement>();

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.parent = shipParent.transform;
        cube.name = "PlaceHolderMesh";


        GameObject aimParent = new GameObject("AimParent");
        aimParent.transform.parent = railCart.transform;
        GameObject aimTarget = new GameObject("AimTarget");
        aimTarget.transform.parent = aimParent.transform;

        GameObject cameraParent = new GameObject("CameraParent");
        cameraParent.transform.parent = railCart.transform;
        GameObject railCamera = new GameObject("RailCamera");
        railCamera.transform.parent = cameraParent.transform;
        railCamera.tag = "RailCamera";
        railCamera.AddComponent<Camera>();
        railCamera.AddComponent<RailCameraFollow>();

        //string prefabPath = "Assets/Prefabs/RailCart.prefab";
        //PrefabUtility.SaveAsPrefabAsset(railCart, prefabPath);
        //Debug.Log("Prefab generated at: " + prefabPath);
    }

    public static bool ValidateGeneratePathInterpolator() {
        return GameObject.FindObjectOfType<PathInterpolator>() == null;
    }
    
    [MenuItem("GameObject/RailShooter", true)]
    public static bool ValidateGenerateRailSystem() {
        bool path = ValidateGeneratePathInterpolator();
        bool rail = ValidateGenerateRailCart();
    
        if (path && rail) { return true; }
        else{ return false; }
    }

    [MenuItem("GameObject/RailShooter")]
    public static void GenerateRailSystem() {
        if (GameObject.FindObjectOfType<PlayerMovement>() != null) {
            Debug.LogError("Cannot generate RailCart: PlayerMovement already exists in the scene.");
            return;
        }
        if (GameObject.FindObjectOfType<PlayerMovement>() != null) {
            Debug.LogError("Cannot generate RailCart: PlayerMovement already exists in the scene.");
            return;
        }

        GenerateRailCartInternal();
        GeneratePathInterpolator();
    }

    public static void GeneratePathInterpolator() {


        GameObject waypoint = new GameObject("WaypointInterpolator");
        waypoint.AddComponent<PathInterpolator>();
        waypoint.GetComponent<PathInterpolator>().AppendPointsToEnd();
    }
}
}