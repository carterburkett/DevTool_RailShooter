using RailAndCart;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;


namespace RailEditors{
public class PluginEditorInterface : EditorWindow {
    private PathInterpolator pathInterpolator;
    
    private PlayerMovement pmove;
    private RailCameraFollow cameraEditor;
    private RailCarMovement railMover;

    private Vector2 scrollPos;

    [MenuItem("Window/RailShooter/RailShooter")]
    public static void ShowWindow() {
        GetWindow<PluginEditorInterface>("Rail Shooter");
    }

    private void OnEnable() {
        InitScriptEditors();
    }

    private void InitScriptEditors() {
        pathInterpolator = FindObjectOfType<PathInterpolator>();
        railMover = FindObjectOfType<RailCarMovement>();
        pmove = FindObjectOfType<PlayerMovement>();
        cameraEditor = FindObjectOfType<RailCameraFollow>();
    }

    private void OnGUI() {
        InitScriptEditors();
        float labelWidth = EditorGUIUtility.currentViewWidth - 212;

        if (!EditorGUIUtility.wideMode) {
            EditorGUIUtility.wideMode = true;
        }

        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        if (pathInterpolator != null){ DrawPathSettingsEditor(); }
        GUILayout.Space(15);
        if (railMover != null && pmove != null){ DrawCartSettingsEditor(); }
        GUILayout.Space(15);
        if (cameraEditor != null){DrawCameraSettingsEditor(); }

        EditorGUILayout.EndScrollView();
    }

    private bool showPathSettings = true;
    bool showRailInspectorSettings = true;
    bool showColorSettings = true;
    public void DrawPathSettingsEditor() {
        SerializedObject pathEditor= new SerializedObject(pathInterpolator);
        if (pathEditor != null) {
            GUIContent pathContent = new GUIContent("Path Settings", "Settings to determine and alter the Rail's path");
            showPathSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showPathSettings, pathContent, EditorStyles.foldoutHeader);
            EditorGUI.indentLevel = 1;

            if (showPathSettings){
                float fadeAlpha = showColorSettings ? 1f : 0f;

                showColorSettings = EditorGUILayout.Foldout(showColorSettings, "Editor Colors", EditorStyles.foldout);
                EditorGUI.indentLevel = 2;

                if (showColorSettings) {
                    EditorGUI.indentLevel = 2;
                    EditorGUILayout.PropertyField(pathEditor.FindProperty("curveColor"), new GUIContent("Rail/Path Color"));
                    EditorGUILayout.PropertyField(pathEditor.FindProperty("gizmoColor"), new GUIContent("Control Point Color"));
                    EditorGUILayout.PropertyField(pathEditor.FindProperty("handleColor"), new GUIContent("Manipulator Color"));
                }

                EditorGUI.indentLevel = 1;
                showRailInspectorSettings = EditorGUILayout.Foldout(showRailInspectorSettings, "Path Manipulator", EditorStyles.foldout);
                EditorGUI.indentLevel = 2;
                if (showRailInspectorSettings) {
                    GUIContent smoothContent = new GUIContent("Smoothing", "Type of Smoothing algorithm to apply to the path");
                    GUIContent resContent = new GUIContent("Intensity", "Lower values are stronger. \nHigh enough Values will eventually ignore waypoints, and B-Line to the final point");

                    EditorGUILayout.PropertyField(pathEditor.FindProperty("resolution"), resContent);
                    EditorGUILayout.PropertyField(pathEditor.FindProperty("smoothing"));
                    if ((PathInterpolator._smoothing)pathEditor.FindProperty("smoothing").enumValueIndex == PathInterpolator._smoothing.none) {
                        EditorGUILayout.HelpBox("Handles will be ignored, and paths with be drawn linerally b/w waypoints.", MessageType.Info);
                    }
                    else if((PathInterpolator._smoothing)pathEditor.FindProperty("smoothing").enumValueIndex == PathInterpolator._smoothing.bezier) {

                    }
                    EditorGUILayout.PropertyField(pathEditor.FindProperty("isCyclical"));
                }


                EditorGUI.indentLevel = 1;
                //================Buttons==========================
                GUIContent plusButtonContent = new GUIContent("Add Points", "Adds One control point and two handles.");
                GUIContent removeButtonContent = new GUIContent("Remove Points", "Removes One control point and the two handles beneath it.");
                EditorGUILayout.BeginHorizontal();
                EditorGUILayout.LabelField("Manage Points: ");
                if (GUILayout.Button(plusButtonContent)) {
                    if (pathEditor != null) {
                        pathInterpolator.AppendPointsToEnd();
                    }
                    else {
                        Debug.LogWarning("PathInterpolator not found in the scene!");
                    }
                }
                if (GUILayout.Button(removeButtonContent)) {
                    if (pathEditor != null) {
                        pathInterpolator.RemovePointsFromEnd();
                    }
                    else {
                        Debug.LogWarning("PathInterpolator not found in the scene!");
                    }
                }
                GUILayout.EndHorizontal();
                EditorGUI.indentLevel = 0;
            }
            EditorGUILayout.EndFoldoutHeaderGroup();
            //pathEditor.serializedObject.ApplyModifiedProperties();

        }
        else { Debug.Break(); }
        pathEditor.ApplyModifiedProperties();

    }

    private bool showRailCarSettings = true;
    private bool showMovementSettings = true;
    public void DrawCartSettingsEditor() {
        SerializedObject playerSerializedObject = new SerializedObject(pmove);
        SerializedObject railMoverSerializedObject = new SerializedObject(railMover);
        SerializedProperty lateralSpeed = playerSerializedObject.FindProperty("xySpeed");
        SerializedProperty lookSpeed = playerSerializedObject.FindProperty("lookSpeed");
        SerializedProperty aimTarget = playerSerializedObject.FindProperty("aimTarget");
        SerializedProperty leanSpeed = playerSerializedObject.FindProperty("leanSpeed");
        SerializedProperty input = playerSerializedObject.FindProperty("inputType");
            SerializedProperty camera = playerSerializedObject.FindProperty("cameraParent");

            GUIContent railCartContent = new GUIContent("RailCar Settings", "These Settings control the values that control your ship, car, etc.");
        showRailCarSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showRailCarSettings, railCartContent, EditorStyles.foldoutHeader);

        EditorGUI.indentLevel = 1;
        GUIContent movementContent = new GUIContent("Movement Settings", "Variables manipulate movement and speed values");
        if (showRailCarSettings) {
            EditorGUILayout.PropertyField(aimTarget); // maybe remove this
            showMovementSettings = EditorGUILayout.Foldout(showMovementSettings, movementContent, EditorStyles.foldout);
            EditorGUI.indentLevel = 2;

            if (showMovementSettings){
                if (pmove != null) {
                    EditorGUILayout.PropertyField(input, new GUIContent("Input Device", "Device Type for Input"));
                    if (railMover != null) {
                        SerializedProperty speedProperty = railMoverSerializedObject.FindProperty("speed");
                        EditorGUILayout.PropertyField(speedProperty, new GUIContent("Forward Speed"));
                        railMoverSerializedObject.ApplyModifiedProperties();
                    }
                    EditorGUILayout.PropertyField(lateralSpeed, new GUIContent("XY Speed", "How fast your object moves in bounds of the camera."));
                    EditorGUILayout.PropertyField(lookSpeed, new GUIContent("Look Speed", "How fast your aimTarget moves"));
                    EditorGUILayout.PropertyField(leanSpeed, new GUIContent("Lean Speed", "How much your model tilts within camera bounds."));
                }
                EditorGUI.indentLevel = 1;
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
            EditorGUILayout.ObjectField(camera, new GUIContent("Camera Parent", "Camera Parent."));

            //EditorGUILayout.Space(5);
            //if (GUILayout.Button("Open In Master Window")) {
            //    CustomEditorWindow.ShowWindow();
            //}
            playerSerializedObject.ApplyModifiedProperties();
        railMoverSerializedObject.ApplyModifiedProperties();
    }


    bool showCameraSettings = true;
    public void DrawCameraSettingsEditor(){
        SerializedObject railCamera = new SerializedObject(cameraEditor);
        SerializedProperty cameraTarget = railCamera.FindProperty("target");
        SerializedProperty cameraOffset = railCamera.FindProperty("offset");
        SerializedProperty limits = railCamera.FindProperty("limits");
        SerializedProperty smoothTime = railCamera.FindProperty("smoothTime");

        GUIContent cameraHeaderContent = new GUIContent("Rail Camera Settings", "Camera Settings.");

        showCameraSettings = EditorGUILayout.BeginFoldoutHeaderGroup(showCameraSettings, cameraHeaderContent, EditorStyles.foldoutHeader);
        EditorGUI.indentLevel = 1;
        if (showCameraSettings) {
            EditorGUILayout.PropertyField(cameraTarget, new GUIContent("Camera Target", "This should normally be the railCart's cursor."));
            EditorGUILayout.PropertyField(cameraOffset, new GUIContent("Camera Offset", "Camera's offset from the target's origin"));
            EditorGUILayout.PropertyField(limits, new GUIContent("Local Limit", "Limits the local X and Y Position of the camera"));
            EditorGUILayout.PropertyField(smoothTime, new GUIContent("Smooth Time", "How Quickly the Camera the camera moves to the ship mode's location"));
            
        }
        railCamera.ApplyModifiedProperties();

        }
    }

[CustomEditor(typeof (PlayerMovement))]
public class PlayerMovementEditor : Editor{
    private bool showRailCarSettings = true;
    private bool showMovementSettings = true;
    
    private PlayerMovement pmove;
    private RailCarMovement railMover;

    private void OnEnable(){
        pmove = FindObjectOfType<PlayerMovement>();
        railMover = FindObjectOfType<RailCarMovement>();
    }    
    private void Update(){
        pmove = FindObjectOfType<PlayerMovement>();
        railMover = FindObjectOfType<RailCarMovement>();
    }

    public override void OnInspectorGUI() {
        SerializedObject playerSerializedObject = new SerializedObject(pmove);
        SerializedObject railMoverSerializedObject = new SerializedObject(railMover);
        SerializedProperty lateralSpeed = playerSerializedObject.FindProperty("xySpeed");
        SerializedProperty lookSpeed = playerSerializedObject.FindProperty("lookSpeed");
        SerializedProperty aimTarget = playerSerializedObject.FindProperty("aimTarget");
        SerializedProperty leanSpeed = playerSerializedObject.FindProperty("leanSpeed");
        SerializedProperty input = playerSerializedObject.FindProperty("inputType");
        SerializedProperty camera = playerSerializedObject.FindProperty("cameraParent");
       EditorGUILayout.ObjectField(camera, new GUIContent("Camera Parent", "Camera Parent."));

            GUIContent railCartContent = new GUIContent("RailCar Settings", "These Settings control the values that control your ship, car, etc.");
        EditorGUILayout.LabelField(railCartContent, EditorStyles.boldLabel);

        EditorGUI.indentLevel = 1;
        GUIContent movementContent = new GUIContent("Movement Settings", "Variables manipulate movement and speed values");
        if (showRailCarSettings) {
            EditorGUILayout.PropertyField(aimTarget); // maybe remove this
            showMovementSettings = EditorGUILayout.Foldout(showMovementSettings, movementContent, EditorStyles.foldout);
            EditorGUI.indentLevel = 2;

            if (showMovementSettings) {
                if (pmove != null) {
                    EditorGUILayout.PropertyField(input, new GUIContent("Input Device", "Device Type for Input"));
                    if (railMover != null) {
                        SerializedProperty speedProperty = railMoverSerializedObject.FindProperty("speed");
                        EditorGUILayout.PropertyField(speedProperty, new GUIContent("Forward Speed"));
                        railMoverSerializedObject.ApplyModifiedProperties();
                    }
                    EditorGUILayout.PropertyField(lateralSpeed, new GUIContent("XY Speed", "How fast your object moves in bounds of the camera."));
                    EditorGUILayout.PropertyField(lookSpeed, new GUIContent("Look Speed", "How fast your aimTarget moves"));
                    EditorGUILayout.PropertyField(leanSpeed, new GUIContent("Lean Speed", "How much your model tilts within camera bounds."));
                }
                EditorGUI.indentLevel = 1;
            }
        }
        EditorGUILayout.EndFoldoutHeaderGroup();
        playerSerializedObject.ApplyModifiedProperties();
    }
}
}