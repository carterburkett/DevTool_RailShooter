using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;


namespace RailEditors {

[InitializeOnLoad]
public static class HierarchyColorizer {
    private static Dictionary<int, Color> backgroundColors = new Dictionary<int, Color>();
    private static Dictionary<int, Color> textColors = new Dictionary<int, Color>();

    static HierarchyColorizer() {
        EditorApplication.hierarchyWindowItemOnGUI += HandleHierarchyWindowItemOnGUI;
    }

    private static void HandleHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect) {
        GameObject obj = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
        if (obj != null) {
            string tag = obj.tag;

            Color backgroundColor = Color.clear;
            Color textColor = Color.white;

            if (backgroundColors.ContainsKey(instanceID)) {
                backgroundColor = backgroundColors[instanceID];
            }
            if (textColors.ContainsKey(instanceID)) {
                textColor = textColors[instanceID];
            }

            if (tag == "ControlPoint") {
                DrawHierarchyItem(selectionRect, backgroundColor, textColor, obj.name, true);
            }
            else if (tag == "BezierHandle") {
                DrawHierarchyItem(selectionRect, backgroundColor, textColor, obj.name, false);
            }
        }
    }

    public static void SetBackgroundColor(GameObject obj, Color color) {
        int instanceID = obj.GetInstanceID();
        if (backgroundColors.ContainsKey(instanceID)) {
            backgroundColors[instanceID] = color;
        }
        else {
            backgroundColors.Add(instanceID, color);
        }
    }

    public static void SetTextColor(GameObject obj, Color color) {
        int instanceID = obj.GetInstanceID();
        if (textColors.ContainsKey(instanceID)) {
            textColors[instanceID] = color;
        }
        else {
            textColors.Add(instanceID, color);
        }
    }

    private static void DrawHierarchyItem(Rect selectionRect, Color backgroundColor, Color textColor, string itemName, bool isControlPoint) {
        Rect backgroundRect = new Rect(selectionRect.x +16, selectionRect.y, selectionRect.width, selectionRect.height);

        EditorGUI.DrawRect(backgroundRect, backgroundColor);
        GUIStyle style;
        if(isControlPoint){ style = new GUIStyle(EditorStyles.boldLabel); }
        else{style = new GUIStyle(EditorStyles.label); }
        
        style.normal.textColor = textColor;
        style.alignment = TextAnchor.MiddleLeft;

        Rect labelRect = new Rect(selectionRect.x + 17f, selectionRect.y  - 1f, selectionRect.width - 16f, selectionRect.height);

        EditorGUI.LabelField(labelRect, itemName, style);
    }

    public static float CalculateRelativeLuminance(Color color) {
        //This should be moved into Unity's color code but I don't want to do that so it can sit here.
        float R = color.r <= 0.03928f ? color.r / 12.92f : Mathf.Pow((color.r + 0.055f) / 1.055f, 2.4f);
        float G = color.g <= 0.03928f ? color.g / 12.92f : Mathf.Pow((color.g + 0.055f) / 1.055f, 2.4f);
        float B = color.b <= 0.03928f ? color.b / 12.92f : Mathf.Pow((color.b + 0.055f) / 1.055f, 2.4f);

        return 0.2126f * R + 0.7152f * G + 0.0722f * B;
    }

    private static Color GetContrastingColor(Color bgColor) {
        //@INFO I don't like the way this continuously seems to force to black or white. I dk where I need to put the threshold for it
                //to feel and natural and look fine. 
        float brightness = bgColor.r * 0.299f + bgColor.g * 0.587f + bgColor.b * 0.114f;

        float threshold = 0.5f;
        Color textColor = brightness > threshold ? Color.black : Color.white;

        float contrastFactor = 0.2f;
        textColor = textColor == Color.black ? new Color(contrastFactor, contrastFactor, contrastFactor) : new Color(1 - contrastFactor, 1 - contrastFactor, 1 - contrastFactor);

        return textColor;
    }

    public static (Color color1, Color color2) GetTriadicColors(Color originalColor) {
        Color.RGBToHSV(originalColor, out float h, out float s, out float v);
        float angle1 = h + 120f / 360f;
        float angle2 = h - 120f / 360f;

        angle1 = Mathf.Repeat(angle1, 1f);
        angle2 = Mathf.Repeat(angle2, 1f);

        Color color1 = Color.HSVToRGB(angle1, s, v);
        Color color2 = Color.HSVToRGB(angle2, s, v);

        return (color1, color2);
    }
    public static (Color color1, Color color2, Color textColor) GetReadableTriadicColors(Color originalColor) {
        Color.RGBToHSV(originalColor, out float h, out float s, out float v);
        Color textColor = originalColor;
        if (s <= .55) {
            textColor = v <= .4f ? textColor = Color.white : textColor = Color.black;
        }
        
        float angle1 = h + 120f / 360f;
        float angle2 = h - 120f / 360f;
        angle1 = Mathf.Repeat(angle1, 1f);
        angle2 = Mathf.Repeat(angle2, 1f);
        
        float maxSaturation = 0.5f;
        float maxBrightness = 0.6f;
        s = Mathf.Clamp(s, 0f, maxSaturation);
        v = Mathf.Clamp(v, 0f, maxBrightness);

        Color color1 = Color.HSVToRGB(angle1, s, v);
        Color color2 = Color.HSVToRGB(angle2, s, v);
       


        return (color1, color2, textColor);
    }
    public static (Color textColor, Color bgColor) GetReadableColors(Color originalColor) {
        Color.RGBToHSV(originalColor, out float h, out float s, out float v);

        float maxSaturation = 0.7f;
        float maxBrightness = 0.9f;
        s = Mathf.Clamp(s, 0f, maxSaturation);
        v = Mathf.Clamp(v, 0f, maxBrightness);

        Color bgColor = Color.HSVToRGB(h, s, v);
        Color textColor = GetContrastingColor(bgColor);

        return (textColor, bgColor);
    }

    public static Color GetReadableColor(Color originalColor) {
        float originalLuminance = CalculateRelativeLuminance(originalColor);
        float targetLuminance = 0.5f;
        float deltaLuminance = targetLuminance - originalLuminance;
        Color newColor = ShiftLuminance(originalColor, deltaLuminance);
        return newColor;
    }

    private static Color ShiftLuminance(Color color, float deltaLuminance) {
        float originalLuminance = CalculateRelativeLuminance(color);
        float scale = originalLuminance > 0 ? (originalLuminance + deltaLuminance) / originalLuminance : 1;
        float R = Mathf.Clamp(color.r * scale, 0f, 1f);
        float G = Mathf.Clamp(color.g * scale, 0f, 1f);
        float B = Mathf.Clamp(color.b * scale, 0f, 1f);

        return new Color(R, G, B, color.a);
    }
}
}