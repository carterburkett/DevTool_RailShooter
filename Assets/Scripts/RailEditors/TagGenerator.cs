using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RailEditors{
public class TagGenerator : MonoBehaviour
{
    public static bool TagExists(string tag) {
        foreach (string existingTag in UnityEditorInternal.InternalEditorUtility.tags) {
            if (existingTag == tag)
                return true;
        }
        return false;
    }

    public static void AddTag(string tag) {
        if (!TagExists(tag)) {
            UnityEditorInternal.InternalEditorUtility.AddTag(tag);
            Debug.Log("Tag added: " + tag);
        }
    }
}
}
