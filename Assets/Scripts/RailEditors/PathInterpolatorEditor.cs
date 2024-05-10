using RailAndCart;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

namespace RailEditors {

    [CustomEditor(typeof(PathInterpolator))]
public class PathInterpolatorEditor : Editor {
    [HideInInspector] public PathInterpolator pathInterpolator;
    [HideInInspector] public SerializedObject serializedPathInterpolator;

    private void OnEnable() {
        pathInterpolator = FindFirstObjectByType<PathInterpolator>();
        if(pathInterpolator != null){ serializedPathInterpolator = new SerializedObject(pathInterpolator); }
    }

    public override void OnInspectorGUI() {
        base.OnInspectorGUI();

        if (pathInterpolator == null) {
            EditorGUILayout.LabelField("Path Interpolator not found in the scene.");
            return;
        }

        serializedPathInterpolator.Update();

        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("curveColor"));
        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("gizmoColor"));
        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("handleColor"));
        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("resolution"));
        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("smoothing"));
        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("isCyclical"));

        EditorGUILayout.Space(5);
        if(GUILayout.Button("Add Control Point")){
            pathInterpolator.AppendPointsToEnd();
        }
        if (GUILayout.Button("Open In Window")) {
            CustomEditorWindow.ShowWindow();
        }

        serializedPathInterpolator.ApplyModifiedProperties();
    }
}

public class CustomEditorWindow : EditorWindow {
    private PathInterpolator pathInterpolator;
    [HideInInspector]public SerializedObject serializedPathInterpolator;

    private void OnEnable() {
        pathInterpolator = FindFirstObjectByType<PathInterpolator>();
        serializedPathInterpolator = new SerializedObject(pathInterpolator);
    }

    //[MenuItem("Window/RailShooter/Waypoints")]
    public static void ShowWindow() {
        GetWindow<CustomEditorWindow>("Custom Editor");
    }

    void OnGUI() {
        GUILayout.Label("Editor Window Content", EditorStyles.boldLabel);

        if (pathInterpolator == null) {
            EditorGUILayout.LabelField("Path Interpolator not found in the scene.");
            return;
        }

        serializedPathInterpolator.Update();

        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("curveColor"));
        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("gizmoColor"));
        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("handleColor"));
        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("resolution"));
        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("smoothing"));
        EditorGUILayout.PropertyField(serializedPathInterpolator.FindProperty("isCyclical"));

        EditorGUILayout.Space(5);
        if (GUILayout.Button("Open In Master Window")) {
            ShowWindow();
        }

        serializedPathInterpolator.ApplyModifiedProperties();

        if (GUILayout.Button("Add Points")) {
            if (pathInterpolator != null) {
                pathInterpolator.AppendPointsToEnd();
            }
            else {
                Debug.LogWarning("PathInterpolator not found in the scene!");
            }
        }
    }
}
}
